using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyAttack
{
    public AttackData attack;
    public float weight = 1f;
    public float cooldown = 2f;
    public int animatorId; // AttackId parameter in the enemy's animator
}

[CreateAssetMenu(menuName = "MTDW/Enemy Config")]
public class EnemyConfig : ScriptableObject
{
    [Header("Perception")]
    public float detectionRange = 10f;
    public float viewAngle = 60f; // either side of forward
    public float alertTime = 0.6f;
    public float loseTrackRange = 18f;
    public float leashDistance = 20f; // from spawn
    public float provokedLeashGrace = 5f; // hit while returning: fights back and ignores the leash this long
    public float reaggroMargin = 5f;      // while returning, can notice the player again once this far inside the leash
    public LayerMask lineOfSightMask = ~0;
    public float eyeHeight = 1.6f;
    public float targetHeight = 1f; // where on the player line of sight aims

    // animator blend values like the player's (walk 1, run 3), the clips set the real speed
    [Header("Movement")]
    public float patrolSpeed = 1f;
    public float chaseSpeed = 3f;
    public float repositionSpeed = 1f;
    public float returnSpeed = 1f;
    public float turnSpeed = 360f; // deg/sec
    public float patrolWaitTime = 2f;
    public float arriveDistance = 0.5f;

    [Header("Combat")]
    public List<EnemyAttack> attacks = new List<EnemyAttack>();
    public float combatRangeMargin = 1f; // enters combat within the longest attack range plus this
    public float preferredDistance = 1.8f;
    public float minDistance = 1.3f;    // backs off when closer
    public float reachBuffer = 0.4f;    // attacks drift with root motion, so only pick them this far inside their reach
    public float repositionTimeMin = 0.5f;
    public float repositionTimeMax = 1.5f;
    public float recoverTime = 0.8f;
    public float staggerTime = 1.5f;

    public float MaxAttackRange
    {
        get
        {
            float range = 0f;
            foreach (EnemyAttack a in attacks)
                range = Mathf.Max(range, a.attack.range);
            return range;
        }
    }
}
