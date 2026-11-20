using System.Collections.Generic;
using UnityEngine;

// Owns the enemy's state machine. States read config, target and components through this.
[RequireComponent(typeof(EnemyMotor), typeof(CombatStats), typeof(HitResolver))]
[RequireComponent(typeof(Stamina), typeof(EnemyWindUpCue))]
public class EnemyBrain : MonoBehaviour
{
    public EnemyConfig config;
    public CombatStats target;
    public Transform[] patrolPoints; // optional, idles at spawn if empty

    public EnemyMotor Motor { get; private set; }
    public Animator Animator { get; private set; }
    public HitResolver Resolver { get; private set; }
    public Stamina Stamina { get; private set; }
    public EnemyWindUpCue Cue { get; private set; }
    public Vector3 SpawnPoint { get; private set; }
    public Quaternion SpawnRotation { get; private set; }
    public float WalkSpeed { get; private set; } // m/s, rolled per instance
    public float RunSpeed { get; private set; }
    public string StatePath => machine.Path;

    public State Idle { get; private set; }
    public State Alert { get; private set; }
    public State Chase { get; private set; }
    public State Return { get; private set; }
    public EnemyCombatState Combat { get; private set; }
    public State Staggered { get; private set; }
    public State Dead { get; private set; }

    readonly StateMachine machine = new StateMachine();
    CombatStats stats;
    PlayerNoise targetNoise;
    float[] attackReadyAt; // per attack in config.attacks, for cooldowns
    float leashIgnoredUntil;
    readonly Queue<float> recentHits = new Queue<float>(); // times of hits taken outside an attack, for retaliation

    void Awake()
    {
        Motor = GetComponent<EnemyMotor>();
        Animator = GetComponent<Animator>();
        Resolver = GetComponent<HitResolver>();
        Stamina = GetComponent<Stamina>();
        Cue = GetComponent<EnemyWindUpCue>();
        stats = GetComponent<CombatStats>();
        targetNoise = target.GetComponent<PlayerNoise>();
        SpawnPoint = transform.position;
        SpawnRotation = transform.rotation;
        attackReadyAt = new float[config.attacks.Count];

        // so two enemies of the same type don't move identically
        WalkSpeed = Random.Range(config.walkSpeedMin, config.walkSpeedMax);
        RunSpeed = Random.Range(config.runSpeedMin, config.runSpeedMax);
        Motor.Acceleration = config.acceleration;
        Debug.Log($"[Enemy] {name} speeds walk {WalkSpeed:F2} run {RunSpeed:F2}");

        Idle = new EnemyIdleState(this);
        Alert = new EnemyAlertState(this);
        Chase = new EnemyChaseState(this);
        Return = new EnemyReturnState(this);
        Combat = new EnemyCombatState(this);
        Staggered = new EnemyStaggeredState(this);
        Dead = new EnemyDeadState(this);
    }

    void OnEnable()
    {
        stats.Damaged += OnDamaged;
        stats.PoiseDamaged += OnPoiseDamaged;
        stats.Died += OnDied;
        target.Died += OnTargetDied;
    }

    void OnDisable()
    {
        stats.Damaged -= OnDamaged;
        stats.PoiseDamaged -= OnPoiseDamaged;
        stats.Died -= OnDied;
        target.Died -= OnTargetDied;
    }

    void Start()
    {
        machine.Change(Idle);
    }

    void Update()
    {
        machine.Tick();
    }

    public void ChangeState(State next)
    {
        machine.Change(next);
    }

    // like a fresh spawn: alive at the spawn point, full health/poise/stamina, idle. Used by rest and respawn
    public void ResetToSpawn()
    {
        machine.ExitCurrent();
        Motor.Warp(SpawnPoint, SpawnRotation);
        stats.RestoreAll();
        Stamina.Refill();
        Cue.Off();
        for (int i = 0; i < attackReadyAt.Length; i++) attackReadyAt[i] = 0f;
        recentHits.Clear();
        leashIgnoredUntil = 0f;
        GetComponent<LockOnTarget>().enabled = true;
        GetComponent<Collider>().enabled = true;
        Animator.Rebind();
        Animator.Update(0f);
        machine.Change(Idle);
    }

    void OnDamaged(int amount)
    {
        // getting hit from behind still wakes it up
        if (machine.Current == Idle)
        {
            ChangeState(Alert);
        }
        else if (machine.Current == Return)
        {
            // the grace stops it leashing straight back when hit right at the leash edge
            leashIgnoredUntil = Time.time + config.provokedLeashGrace;
            Debug.Log($"[Enemy] {name} provoked while returning");
            ChangeState(Chase);
        }

        CountHitForRetaliation();
    }

    // runs before the hit's poise damage, so a hit that also breaks poise staggers it out of the retaliation
    void CountHitForRetaliation()
    {
        if (config.retaliateHits <= 0 || InAttack || machine.Current == Staggered || machine.Current == Dead) return;

        recentHits.Enqueue(Time.time);
        while (Time.time - recentHits.Peek() > config.retaliateWindow) recentHits.Dequeue();
        if (recentHits.Count < config.retaliateHits) return;

        recentHits.Clear();
        Debug.Log($"[Enemy] {name} retaliates ({config.retaliateHits} hits in {config.retaliateWindow:F1} s)");
        if (machine.Current != Combat) ChangeState(Combat);
        Combat.ForceAttack(config.retaliateAttack);
    }

    // changing state exits Attack, which cancels the swing and its glow
    void OnPoiseDamaged(bool broke)
    {
        if (!broke || machine.Current == Dead) return;

        if (InAttack && CurrentAttack.hyperArmour)
        {
            Debug.Log($"[Enemy] {name} poise broken, hyper armour keeps {CurrentAttack.name} going");
            return;
        }

        Debug.Log(InAttack ? $"[Enemy] {name} staggered, attack cancelled" : $"[Enemy] {name} staggered");
        recentHits.Clear();
        ChangeState(Staggered);
    }

    AttackData CurrentAttack => config.attacks[Combat.Attack.attackIndex].attack;

    void OnDied()
    {
        ChangeState(Dead);
    }

    // stop fighting a corpse, also cancels a swing in progress
    void OnTargetDied()
    {
        if (machine.Current == Dead || machine.Current == Idle || machine.Current == Return) return;
        Debug.Log($"[Enemy] {name} target died, returning");
        ChangeState(Return);
    }

    // animation events, only the active attack acts on them
    public void AttackHit()
    {
        if (InAttack) Combat.Attack.OnHit();
    }

    public void AttackEnd()
    {
        if (InAttack) Combat.Attack.OnEnd();
    }

    // player-only cancel point on the shared slash clip, enemies finish at AttackEnd
    public void AttackRecover() { }

    bool InAttack => machine.Current == Combat && Combat.Sub.Current == Combat.Attack;

    // weighted random pick from attacks that are off cooldown and in reach; -1 if none
    public int PickAttack()
    {
        if (!Stamina.CanAct) return -1;

        float total = 0f;
        for (int i = 0; i < config.attacks.Count; i++)
            if (CanUse(i)) total += config.attacks[i].weight;
        if (total <= 0f) return -1;

        float roll = Random.value * total;
        for (int i = 0; i < config.attacks.Count; i++)
        {
            if (!CanUse(i)) continue;
            roll -= config.attacks[i].weight;
            if (roll <= 0f) return i;
        }
        return -1;
    }

    bool CanUse(int i)
    {
        return Time.time >= attackReadyAt[i] && DistanceToTarget <= config.attacks[i].attack.range - config.reachBuffer;
    }

    public void StartCooldown(int i)
    {
        attackReadyAt[i] = Time.time + config.attacks[i].cooldown;
    }

    public Vector3 ToTarget
    {
        get
        {
            Vector3 d = target.transform.position - transform.position;
            d.y = 0f;
            return d;
        }
    }

    public float DistanceToTarget => ToTarget.magnitude;

    public float DistanceFromSpawn
    {
        get
        {
            Vector3 d = transform.position - SpawnPoint;
            d.y = 0f;
            return d.magnitude;
        }
    }

    public float CombatRange => config.MaxAttackRange + config.combatRangeMargin;

    // sight, or hearing inside the player's noise radius
    public bool NoticesTarget()
    {
        if (target.IsDead) return false;
        if (CanSeeTarget()) return true;
        if (DistanceToTarget > targetNoise.Radius) return false;
        Debug.Log($"[Enemy] {name} heard the player ({DistanceToTarget:F1} m)");
        return true;
    }

    public bool CanSeeTarget()
    {
        if (DistanceToTarget > config.detectionRange) return false;
        if (Vector3.Angle(transform.forward, ToTarget) > config.viewAngle) return false;

        Vector3 eye = transform.position + Vector3.up * config.eyeHeight;
        Vector3 aim = target.transform.position + Vector3.up * config.targetHeight;
        if (!Physics.Linecast(eye, aim, out RaycastHit hit, config.lineOfSightMask, QueryTriggerInteraction.Ignore))
            return true;
        return hit.transform.IsChildOf(target.transform);
    }

    public bool ShouldGiveUp =>
        DistanceToTarget > config.loseTrackRange ||
        (Time.time >= leashIgnoredUntil && DistanceFromSpawn > config.leashDistance);
}
