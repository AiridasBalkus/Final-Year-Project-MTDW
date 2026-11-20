using UnityEngine;

[CreateAssetMenu(menuName = "MTDW/Player Movement Config")]
public class PlayerMovementConfig : ScriptableObject
{
    // m/s
    public float walkSpeed = 1.3f;
    public float runSpeed = 3.5f;
    public float sprintSpeed = 3.5f;
    public float lockedRunSpeed = 3f; // cap while locked on, no sprint strafe clips
    // m/s²
    public float acceleration = 12f;
    public float deceleration = 16f;

    public float walkInputThreshold = 0.5f; // stick below this = walk, keyboard always runs
    public float turnSpeed = 720f;          // deg/sec
    public float lockedTurnSpeed = 240f;    // deg/sec, slower so facing the target doesn't snap
    public float attackTurnSpeed = 180f;    // deg/sec, tracking during a swing's wind-up (until AttackHit), 0 = none

    public float lungeStopDistance = 1.2f; // metres, the attack lunge stops this far from the lock-on target
    public float dodgeDistanceScale = 1f; // 1 = the roll clip's own distance (~3.1 m)
}
