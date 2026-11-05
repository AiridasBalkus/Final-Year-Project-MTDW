using UnityEngine;

public enum HitResult { Hit, HitPoiseBroken, Dodged, OutOfReach, AlreadyResolved }

// Decides if a swing lands. No hitboxes - this gets called from the AttackHit
// animation event (or the dummy's timer). Connecting is deterministic:
// in reach + target in i-frames = Dodged, in reach otherwise = Hit.
[RequireComponent(typeof(CombatStats))]
public class HitResolver : MonoBehaviour
{
    public WeaponData weapon;
    public AttackData attack; // one attack per character for now
    public float strengthScaling = 0.03f; // k in the damage formula
    public float damageVariance = 0.1f;   // 0.1 = random 0.9–1.1
    public float critMultiplier = 1.5f;

    CombatStats stats;
    bool hitResolved;
    int hitCount; // for checking the crit rate
    int critCount;

    void Awake()
    {
        stats = GetComponent<CombatStats>();
    }

    public void BeginSwing()
    {
        hitResolved = false;
    }

    public HitResult Resolve(CombatStats target)
    {
        // one hit per swing
        if (hitResolved) return HitResult.AlreadyResolved;
        hitResolved = true;

        Vector3 toTarget = target.transform.position - transform.position;
        toTarget.y = 0f;
        float dist = toTarget.magnitude;
        float angle = Vector3.Angle(transform.forward, toTarget);

        if (dist > attack.range || angle > attack.halfAngle)
        {
            Debug.Log($"[Hit] {name}, Target: {target.name}: out of reach ({dist:F1}m, {angle:F0} deg)");
            return HitResult.OutOfReach;
        }

        if (target.Invincible)
        {
            Debug.Log($"[Hit] {name}, Target: {target.name}: DODGED (i-frames)");
            return HitResult.Dodged;
        }

        // damage = WeaponBase × AttackMultiplier × (1 + Strength × k) × 100 / (100 + Defence) × Variance × Crit
        // only variance and crit are random, whether the hit lands was decided above
        float strengthTerm = 1f + stats.strength * strengthScaling;
        float defenceTerm = 100f / (100f + target.defence);
        float variance = Random.Range(1f - damageVariance, 1f + damageVariance);
        bool crit = Random.value < stats.CritChance;
        float critTerm = crit ? critMultiplier : 1f;

        float raw = weapon.weaponBase * attack.attackMultiplier * strengthTerm * defenceTerm * variance * critTerm;
        int damage = Mathf.Max(Mathf.RoundToInt(raw), 1);

        hitCount++;
        if (crit) critCount++;
        target.TakeDamage(damage);

        // poise after damage, no randomness
        float poiseBefore = target.CurrentPoise;
        bool broke = target.TakePoiseDamage(attack.poiseDamage);
        string poise = broke
            ? $"poise {poiseBefore:F0} - {attack.poiseDamage:F0} → BROKEN (reset to {target.MaxPoise:F0})"
            : $"poise {poiseBefore:F0} - {attack.poiseDamage:F0} → {target.CurrentPoise:F0}/{target.MaxPoise:F0}";

        Debug.Log($"[Hit] {name}, Target: {target.name}: HIT {damage}{(crit ? " CRIT" : "")}, {target.name} HP {target.Health}/{target.MaxHealth}, {poise}\n" +
            $"[Dmg] base {weapon.weaponBase} × mult {attack.attackMultiplier:F2}" +
            $" × str (1 + {stats.strength} × {strengthScaling}) = {strengthTerm:F4}" +
            $" × def 100/(100 + {target.defence}) = {defenceTerm:F4}" +
            $" × var {variance:F4} × crit {critTerm:F2} = {raw:F3} → {damage}" +
            $" | crits {critCount}/{hitCount} ({(float)critCount / hitCount:P0}, chance {stats.CritChance:P0})");
        return broke ? HitResult.HitPoiseBroken : HitResult.Hit;
    }
}
