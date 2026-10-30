using UnityEngine;

public enum HitResult { Hit, Dodged, OutOfReach, AlreadyResolved }

// Decides if a swing lands. No hitboxes - this gets called from the AttackHit
// animation event (or the dummy's timer). Connecting is deterministic:
// in reach + target in i-frames = Dodged, in reach otherwise = Hit.
[RequireComponent(typeof(CombatStats))]
public class HitResolver : MonoBehaviour
{
    public float range = 2.5f;
    public float maxAngle = 60f; // either side of forward
    public float damageVariance = 0.1f;

    CombatStats stats;
    bool hitResolved;

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

        if (dist > range || angle > maxAngle)
        {
            Debug.Log($"[Hit] {name}, Target: {target.name}: out of reach ({dist:F1}m, {angle:F0} deg)");
            return HitResult.OutOfReach;
        }

        if (target.Invincible)
        {
            Debug.Log($"[Hit] {name}, Target: {target.name}: DODGED (i-frames)");
            return HitResult.Dodged;
        }

        int baseDamage = Mathf.Max(stats.attackPower - target.defence, 1);
        float roll = Random.Range(1f - damageVariance, 1f + damageVariance);
        int damage = Mathf.Max(Mathf.RoundToInt(baseDamage * roll), 1);
        target.TakeDamage(damage);

        Debug.Log($"[Hit] {name}, Target: {target.name}: HIT {damage}, {target.name} HP {target.Health}/{target.maxHealth}");
        return HitResult.Hit;
    }
}
