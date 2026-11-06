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
    float[] attackReadyAt; // per attack in config.attacks, for cooldowns
    float leashIgnoredUntil;

    void Awake()
    {
        Motor = GetComponent<EnemyMotor>();
        Animator = GetComponent<Animator>();
        Resolver = GetComponent<HitResolver>();
        Stamina = GetComponent<Stamina>();
        Cue = GetComponent<EnemyWindUpCue>();
        stats = GetComponent<CombatStats>();
        SpawnPoint = transform.position;
        SpawnRotation = transform.rotation;
        attackReadyAt = new float[config.attacks.Count];

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
    }

    void OnDisable()
    {
        stats.Damaged -= OnDamaged;
        stats.PoiseDamaged -= OnPoiseDamaged;
        stats.Died -= OnDied;
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
    }

    // changing state exits Attack, which cancels the swing and its glow
    void OnPoiseDamaged(bool broke)
    {
        if (!broke || machine.Current == Dead) return;
        Debug.Log(InAttack ? $"[Enemy] {name} staggered, attack cancelled" : $"[Enemy] {name} staggered");
        ChangeState(Staggered);
    }

    void OnDied()
    {
        ChangeState(Dead);
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
