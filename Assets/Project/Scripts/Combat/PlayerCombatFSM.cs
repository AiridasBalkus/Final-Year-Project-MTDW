using System.Collections.Generic;
using UnityEngine;

public enum CombatState { Locomotion, Blocking, Attacking, Dodging, Healing, HitReact, GuardBroken, Dead }
public enum CombatEventType { AttackPressed, DodgePressed, HealPressed, HitReceived, BlockHit, GuardBreak, AnimationFinished }

public struct CombatEvent
{
    public CombatEventType type;
    public float time;
    public CombatState finishedState; // for AnimationFinished, the state the clip belonged to
}

// Player combat state machine. Input, damage and animation events all go into one queue.
// If the current state can't handle an event yet it stays queued (that's the input buffer)
// until it's older than bufferTime. Only this script sets the Attack/Dodge/Hit triggers.
// Blocking is held, so it's read from input every frame instead of queued.
// GuardBroken ends on a timer, Healing only restores health on HealApply.
[RequireComponent(typeof(Animator), typeof(CombatStats), typeof(HitResolver))]
[RequireComponent(typeof(Stamina), typeof(Guard), typeof(HealFlask))]
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
    static readonly int AttackSpeedHash = Animator.StringToHash("AttackSpeed");
    static readonly int IsBlockingHash = Animator.StringToHash("IsBlocking");
    static readonly int BlockHitHash = Animator.StringToHash("BlockHit");
    static readonly int GuardBreakHash = Animator.StringToHash("GuardBreak");
    static readonly int GuardBreakSpeedHash = Animator.StringToHash("GuardBreakSpeed");
    static readonly int IsHealingHash = Animator.StringToHash("IsHealing");
    const float GuardBreakClipLength = 0.77f; // Hit_Shield, stretched over guardBreakTime

    Queue<CombatEvent> events = new Queue<CombatEvent>();
    Animator animator;
    CombatStats stats;
    HitResolver resolver;
    Stamina stamina;
    Guard guard;
    HealFlask flask;
    float guardBrokenUntil;
    bool healApplied; // HealApply only heals once per drink

    void Awake()
    {
        animator = GetComponent<Animator>();
        stats = GetComponent<CombatStats>();
        resolver = GetComponent<HitResolver>();
        stamina = GetComponent<Stamina>();
        guard = GetComponent<Guard>();
        flask = GetComponent<HealFlask>();
    }

    void OnEnable()
    {
        input.AttackPressed += OnAttackPressed;
        input.DodgePressed += OnDodgePressed;
        input.HealPressed += OnHealPressed;
        stats.PoiseDamaged += OnPoiseDamaged;
        stats.Died += OnDied;
        guard.Blocked += OnBlocked;
    }

    void OnDisable()
    {
        input.AttackPressed -= OnAttackPressed;
        input.DodgePressed -= OnDodgePressed;
        input.HealPressed -= OnHealPressed;
        stats.PoiseDamaged -= OnPoiseDamaged;
        stats.Died -= OnDied;
        guard.Blocked -= OnBlocked;
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

        if (State == CombatState.GuardBroken && Time.time >= guardBrokenUntil) ChangeState(CombatState.Locomotion);

        // back to blocking after an attack or dodge if the button is still held
        if (State == CombatState.Locomotion && input.BlockHeld) ChangeState(CombatState.Blocking);
        else if (State == CombatState.Blocking && !input.BlockHeld) ChangeState(CombatState.Locomotion);
    }

    // attack and dodge can start from a block, they drop the guard
    bool CanStartAction => State == CombatState.Locomotion || State == CombatState.Blocking;

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

            case CombatEventType.BlockHit:
                // the guard already took the hit, this is only the reaction
                if (State == CombatState.Blocking) animator.SetTrigger(BlockHitHash);
                return true;

            case CombatEventType.GuardBreak:
                ChangeState(CombatState.GuardBroken);
                return true;

            case CombatEventType.AttackPressed:
                if (!CanStartAction) return false;
                if (OutOfStamina(e)) return true;
                ChangeState(CombatState.Attacking);
                return true;

            case CombatEventType.DodgePressed:
                if (!CanStartAction) return false;
                if (OutOfStamina(e)) return true;
                ChangeState(CombatState.Dodging);
                return true;

            case CombatEventType.HealPressed:
                if (State != CombatState.Locomotion) return false;
                if (flask.Charges <= 0)
                {
                    Log($"{e.type} dropped in {State}, no charges");
                    return true;
                }
                if (OutOfStamina(e)) return true;
                ChangeState(CombatState.Healing);
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
        if (State == CombatState.Healing && !healApplied) Log("heal interrupted before HealApply, charge lost");
        State = next;

        locomotion.MovementEnabled = next == CombatState.Locomotion || next == CombatState.Blocking || next == CombatState.Healing;
        locomotion.WalkOnly = next == CombatState.Blocking || next == CombatState.Healing;
        animator.SetBool(IsBlockingHash, next == CombatState.Blocking);
        animator.SetBool(IsHealingHash, next == CombatState.Healing);
        locomotion.ActionMotionScale = 1f;
        locomotion.EndLunge();
        stamina.RegenPaused = next == CombatState.Attacking || next == CombatState.Dodging || next == CombatState.Healing;
        stamina.RegenMultiplier = next == CombatState.Blocking ? guard.regenMultiplier : 1f;
        guard.Active = next == CombatState.Blocking;
        stats.Invincible = false; // the window only opens through IFrameStart

        if (next == CombatState.Attacking)
        {
            AttackData attack = resolver.attack;
            float speed = attack.RollSpeed();
            animator.SetFloat(AttackSpeedHash, speed);
            locomotion.ActionMotionScale = attack.lungeScale;
            locomotion.BeginLunge(attack);
            stamina.Spend(attack.staminaCost);
            resolver.BeginSwing();
            SetTrigger(AttackHash);
            Log($"attack {attack.name} x{speed:F2}");
        }
        else if (next == CombatState.Dodging)
        {
            // the roll always plays at 1x so the i-frame window never changes
            locomotion.ActionMotionScale = locomotion.config.dodgeDistanceScale;
            stamina.Spend(dodgeStaminaCost);
            locomotion.FaceMoveInput();
            SetTrigger(DodgeHash);
        }
        else if (next == CombatState.HitReact)
        {
            SetTrigger(HitHash);
        }
        else if (next == CombatState.Healing)
        {
            flask.UseCharge();
            stamina.Spend(flask.staminaCost);
            healApplied = false;
            Log($"heal started, {flask.Charges} charges left");
        }
        else if (next == CombatState.GuardBroken)
        {
            guardBrokenUntil = Time.time + guard.guardBreakTime;
            animator.SetFloat(GuardBreakSpeedHash, GuardBreakClipLength / guard.guardBreakTime);
            SetTrigger(GuardBreakHash);
        }
    }

    void SetTrigger(int hash)
    {
        // clear the others so a leftover trigger doesn't fire later
        animator.ResetTrigger(AttackHash);
        animator.ResetTrigger(DodgeHash);
        animator.ResetTrigger(HitHash);
        animator.ResetTrigger(GuardBreakHash);
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
    void OnHealPressed() => Enqueue(CombatEventType.HealPressed);
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

    void OnBlocked(bool broke)
    {
        if (broke)
        {
            // drop it now so a second hit this frame isn't blocked too
            guard.Active = false;
            Log($"guard broken (stamina {stamina.Current:F0})");
            Enqueue(CombatEventType.GuardBreak);
        }
        else
        {
            Enqueue(CombatEventType.BlockHit);
        }
    }

    void OnDied()
    {
        Log($"{State} -> Dead");
        State = CombatState.Dead;
        events.Clear();
        locomotion.MovementEnabled = false;
        locomotion.WalkOnly = false;
        animator.SetBool(IsBlockingHash, false);
        animator.SetBool(IsHealingHash, false);
        guard.Active = false;
        stamina.RegenMultiplier = 1f;
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

    // on the heal clip, partway through
    public void HealApply()
    {
        if (State != CombatState.Healing || healApplied) return;
        healApplied = true;
        int before = stats.Health;
        stats.Heal(flask.healAmount);
        Log($"healed {before} -> {stats.Health}");
    }

    public void HealEnd() => Enqueue(CombatEventType.AnimationFinished, CombatState.Healing);
    public void AttackEnd() => Enqueue(CombatEventType.AnimationFinished, CombatState.Attacking);
    public void DodgeEnd() => Enqueue(CombatEventType.AnimationFinished, CombatState.Dodging);
    public void HitReactEnd() => Enqueue(CombatEventType.AnimationFinished, CombatState.HitReact);

    void Log(string msg)
    {
        if (logEvents) Debug.Log("[FSM] " + msg);
    }
}
