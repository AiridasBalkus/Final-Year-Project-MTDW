using UnityEngine;

// One attack's tuning. HitResolver reads reach from here, the FSM reads stamina cost.
[CreateAssetMenu(menuName = "MTDW/Attack Data")]
public class AttackData : ScriptableObject
{
    public float range = 2.5f;    // metres
    public float halfAngle = 60f; // degrees either side of forward
    public float attackMultiplier = 1f;
    public float staminaCost = 20f;
    public float poiseDamage = 10f; // stored only, no stagger yet
}
