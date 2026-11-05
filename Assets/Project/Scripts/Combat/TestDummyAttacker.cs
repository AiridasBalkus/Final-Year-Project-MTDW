using UnityEngine;

// Test dummy. Swings at the player every few seconds so dodging can be tested.
// Goes red while winding up, flashes white when it gets hit, turns yellow while
// staggered (poise broken), which cancels the wind-up like an interrupted enemy attack.
[RequireComponent(typeof(CombatStats), typeof(HitResolver))]
public class TestDummyAttacker : MonoBehaviour
{
    public CombatStats target;
    public float attackInterval = 3f;
    public float windUpTime = 0.6f;
    public float staggerTime = 1.5f;
    public Color windUpColor = Color.red;
    public Color staggerColor = Color.yellow;
    public Color hitFlashColor = Color.white;
    public float hitFlashTime = 0.1f;

    CombatStats stats;
    HitResolver resolver;
    Material mat;
    Color baseColor;
    float nextAttack;
    float hitAt;
    float flashEnd;
    float staggerEnd;
    bool windingUp;
    bool staggered;

    void Awake()
    {
        stats = GetComponent<CombatStats>();
        resolver = GetComponent<HitResolver>();
        mat = GetComponent<Renderer>().material;
        baseColor = mat.color;
        nextAttack = Time.time + attackInterval;
    }

    void OnEnable()
    {
        stats.Damaged += OnDamaged;
        stats.Died += OnDied;
        stats.PoiseDamaged += OnPoiseDamaged;
    }

    void OnDisable()
    {
        stats.Damaged -= OnDamaged;
        stats.Died -= OnDied;
        stats.PoiseDamaged -= OnPoiseDamaged;
    }

    void Update()
    {
        if (staggered && Time.time >= staggerEnd)
        {
            // attack timer restarts once the stagger is over
            staggered = false;
            nextAttack = Time.time + attackInterval;
            Debug.Log("[Dummy] stagger over");
        }

        if (!staggered && !target.IsDead)
            UpdateAttack();

        if (Time.time < flashEnd)
            mat.color = hitFlashColor;
        else if (staggered)
            mat.color = staggerColor;
        else
            mat.color = windingUp ? windUpColor : baseColor;
    }

    void UpdateAttack()
    {
        if (!windingUp && Time.time >= nextAttack)
        {
            windingUp = true;
            hitAt = Time.time + windUpTime;
            Debug.Log("[Dummy] winding up");
        }
        else if (windingUp && Time.time >= hitAt)
        {
            windingUp = false;
            nextAttack = Time.time + attackInterval;
            resolver.BeginSwing();
            resolver.Resolve(target);
        }
    }

    void OnDamaged(int amount)
    {
        flashEnd = Time.time + hitFlashTime;
    }

    void OnPoiseDamaged(bool broke)
    {
        if (!broke) return;

        Debug.Log(windingUp ? "[Dummy] staggered, wind-up cancelled" : "[Dummy] staggered");
        windingUp = false;
        staggered = true;
        staggerEnd = Time.time + staggerTime;
    }

    void OnDied()
    {
        Debug.Log("[Dummy] dead, resetting health");
        stats.RestoreFullHealth();
    }
}
