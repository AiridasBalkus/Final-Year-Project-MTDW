using System;
using UnityEngine;

// Shield block, switched on by the combat FSM while Blocking. HitResolver checks Covers().
public class Guard : MonoBehaviour
{
    public float blockHalfAngle = 60f;   // degrees either side of forward
    public float blockReduction = 0.8f;  // 0.8 = a blocked hit deals 20% damage
    public float regenMultiplier = 0.3f; // stamina regen while blocking
    public float guardBreakTime = 1.5f;  // seconds of GuardBroken stagger

    public bool Active { get; set; }

    public event Action<bool> Blocked; // true if the hit broke the guard

    public bool Covers(Vector3 attackerPosition)
    {
        if (!Active) return false;
        Vector3 toAttacker = attackerPosition - transform.position;
        toAttacker.y = 0f;
        return Vector3.Angle(transform.forward, toAttacker) <= blockHalfAngle;
    }

    public void ReportBlock(bool broke) => Blocked?.Invoke(broke);
}
