using UnityEngine;

// Test dummy. Swings at the player every few seconds so dodging can be tested.
// Goes red while winding up, flashes white when it gets hit.
[RequireComponent(typeof(CombatStats), typeof(HitResolver))]
public class TestDummyAttacker : MonoBehaviour
{
    public CombatStats target;
    public float attackInterval = 3f;
    public float windUpTime = 0.6f;
    public Color windUpColor = Color.red;
    public Color hitFlashColor = Color.white;
    public float hitFlashTime = 0.1f;

    CombatStats stats;
    HitResolver resolver;
    Material mat;
    Color baseColor;
    float nextAttack;
    float hitAt;
    float flashEnd;
    bool windingUp;

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
    }

    void OnDisable()
    {
        stats.Damaged -= OnDamaged;
        stats.Died -= OnDied;
    }

    void Update()
    {
        if (!target.IsDead)
            UpdateAttack();

        if (Time.time < flashEnd)
            mat.color = hitFlashColor;
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

    void OnDied()
    {
        Debug.Log("[Dummy] dead, resetting health");
        stats.RestoreFullHealth();
    }
}
