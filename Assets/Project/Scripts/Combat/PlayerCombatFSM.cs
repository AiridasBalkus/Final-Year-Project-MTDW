using System.Collections.Generic;
using UnityEngine;

public enum CombatState { Locomotion, Attacking, Dodging, HitReact, Dead }
public enum CombatEventType { AttackPressed, DodgePressed, HitReceived, AnimationFinished }

public struct CombatEvent
{
    public CombatEventType type;
    public float time;
    public CombatState finishedState; // for AnimationFinished, the state the clip belonged to
}

// Player combat state machine. Input, damage and animation events all go into one queue.
// If the current state can't handle an event yet it stays queued (that's the input buffer)
// until it's older than bufferTime. Only this script sets the Attack/Dodge/Hit triggers.
[RequireComponent(typeof(Animator), typeof(CombatStats), typeof(HitResolver))]
[RequireComponent(typeof(Stamina))]
public class PlayerCombatFSM : MonoBehaviour
{
    public PlayerInputHandler input;
    public PlayerLocomotion locomotion;
    public LockOnController lockOn;
    public float bufferTime = 0.3f;
    public float dodgeStaminaCost = 15f; // attack cost comes from the AttackData
    public bool logEvents = true;

    public CombatState State { get; private set; } = CombatState.Locomotion;

    static readonly int AttackHash = Animator.StringToHash("Attack");
    static readonly int DodgeHash = Animator.StringToHash("Dodge");
    static readonly int HitHash = Animator.StringToHash("Hit");

    Queue<CombatEvent> events = new Queue<CombatEvent>();
    Animator animator;
    CombatStats stats;
    HitResolver resolver;
    Stamina stamina;

    void Awake()
    {
        animator = GetComponent<Animator>();
        stats = GetComponent<CombatStats>();
        resolver = GetComponent<HitResolver>();
        stamina = GetComponent<Stamina>();
    }

    void OnEnable()
    {
        input.AttackPressed += OnAttackPressed;
        input.DodgePressed += OnDodgePressed;
        stats.PoiseDamaged += OnPoiseDamaged;
        stats.Died += OnDied;
    }

    void OnDisable()
    {
        input.AttackPressed -= OnAttackPressed;
        input.DodgePressed -= OnDodgePressed;
        stats.PoiseDamaged -= OnPoiseDamaged;
        stats.Died -= OnDied;
    }

    void Update()
    {
        // Go through the queue once in order, putting back whatever can't be handled yet.
        // Only ever looking at the front would deadlock: an attack press waiting there
        // blocks the AttackEnd behind it, so the attack never finishes.
        int count = events.Count;
        for (int i = 0; i < count; i++)
        {
            CombatEvent e = events.Dequeue();

            if (Time.time - e.time > bufferTime)
            {
                Log($"{e.type} expired in {State}");
                continue;
            }

            if (!TryHandle(e))
                events.Enqueue(e);
        }
    }

    // returns false if the event should wait
    bool TryHandle(CombatEvent e)
    {
        switch (e.type)
        {
            case CombatEventType.AnimationFinished:
                // a clip that's blending out can still fire its end event after we've moved on
                if (e.finishedState == State)
                    ChangeState(CombatState.Locomotion);
                return true;

            case CombatEventType.HitReceived:
                if (State == CombatState.HitReact) return false;
                ChangeState(CombatState.HitReact);
                return true;

            case CombatEventType.AttackPressed:
                if (State != CombatState.Locomotion) return false;
                if (OutOfStamina(e)) return true;
                ChangeState(CombatState.Attacking);
                return true;

            case CombatEventType.DodgePressed:
                if (State != CombatState.Locomotion) return false;
                if (OutOfStamina(e)) return true;
                ChangeState(CombatState.Dodging);
                return true;
        }
        return true;
    }

    // checked when the action would start, so the event is dropped rather than kept waiting
    bool OutOfStamina(CombatEvent e)
    {
        if (stamina.CanAct) return false;
        Log($"{e.type} dropped in {State}, no stamina ({stamina.Current:F0})");
        return true;
    }

    void ChangeState(CombatState next)
    {
        Log($"{State} -> {next}");
        State = next;

        locomotion.MovementEnabled = next == CombatState.Locomotion;
        locomotion.StraightRootMotion = next == CombatState.Dodging;
        stamina.RegenPaused = next == CombatState.Attacking || next == CombatState.Dodging;
        stats.Invincible = false; // the window only opens through IFrameStart

        if (next == CombatState.Attacking)
        {
            stamina.Spend(resolver.attack.staminaCost);
            resolver.BeginSwing();
            SetTrigger(AttackHash);
        }
        else if (next == CombatState.Dodging)
        {
            stamina.Spend(dodgeStaminaCost);
            locomotion.FaceMoveInput();
            SetTrigger(DodgeHash);
        }
        else if (next == CombatState.HitReact)
        {
            SetTrigger(HitHash);
        }
    }

    void SetTrigger(int hash)
    {
        // clear the others so a leftover trigger doesn't fire later
        animator.ResetTrigger(AttackHash);
        animator.ResetTrigger(DodgeHash);
        animator.ResetTrigger(HitHash);
        animator.SetTrigger(hash);
    }

    void Enqueue(CombatEventType type, CombatState finishedState = CombatState.Locomotion)
    {
        if (State == CombatState.Dead) return;

        events.Enqueue(new CombatEvent { type = type, time = Time.time, finishedState = finishedState });
        Log($"queued {type} in {State}");
    }

    void OnAttackPressed() => Enqueue(CombatEventType.AttackPressed);
    void OnDodgePressed() => Enqueue(CombatEventType.DodgePressed);
    // only a hit that breaks poise interrupts, anything else just deals damage
    void OnPoiseDamaged(bool broke)
    {
        if (broke)
        {
            Log("poise broken");
            Enqueue(CombatEventType.HitReceived);
        }
        else
        {
            Log($"hit absorbed by poise, no interrupt ({State})");
        }
    }

    void OnDied()
    {
        Log($"{State} -> Dead");
        State = CombatState.Dead;
        events.Clear();
        locomotion.MovementEnabled = false;
        locomotion.StraightRootMotion = false;
        stats.Invincible = false;
    }

    // animation events

    public void AttackHit()
    {
        if (State != CombatState.Attacking) return;

        if (lockOn.Current == null)
        {
            Debug.Log("[Hit] no lock-on target");
            return;
        }

        if (lockOn.Current.TryGetComponent(out CombatStats target))
            resolver.Resolve(target);
    }

    // i-frame window, placed on Dodge_Roll
    public void IFrameStart()
    {
        if (State != CombatState.Dodging) return;
        stats.Invincible = true;
        Log("i-frames on");
    }

    public void IFrameEnd()
    {
        if (!stats.Invincible) return;
        stats.Invincible = false;
        Log("i-frames off");
    }

    public void AttackEnd() => Enqueue(CombatEventType.AnimationFinished, CombatState.Attacking);
    public void DodgeEnd() => Enqueue(CombatEventType.AnimationFinished, CombatState.Dodging);
    public void HitReactEnd() => Enqueue(CombatEventType.AnimationFinished, CombatState.HitReact);

    void Log(string msg)
    {
        if (logEvents) Debug.Log("[FSM] " + msg);
    }
}
