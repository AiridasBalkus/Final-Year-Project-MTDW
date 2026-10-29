using UnityEngine;

// Root motion movement. This script only sets animator params and rotation,
// the clips do the actual moving (applied in OnAnimatorMove).
[RequireComponent(typeof(Animator), typeof(CharacterController))]
public class PlayerLocomotion : MonoBehaviour
{
    [Header("References")]
    public PlayerInputHandler input;
    public Transform cameraTransform;

    [Header("Turning")]
    public float turnSpeed = 720f;       // deg/sec
    public float lockedTurnSpeed = 240f; // deg/sec, slower so facing the target doesn't snap

    [Header("Gait")]
    public float walkInputThreshold = 0.5f; // stick below this = walk, keyboard always runs
    // these need to match the thresholds in the animator blend trees
    public float walkSpeedValue = 1f;
    public float runSpeedValue = 3f;
    public float sprintSpeedValue = 3f;
    public float speedDampTime = 0.1f;

    [Header("Gravity")]
    public float gravity = -20f;
    public float groundedStickVelocity = -2f; // keeps the controller pressed onto slopes

    public Transform LockOnTarget { get; private set; }

    static readonly int SpeedHash = Animator.StringToHash("Speed");
    static readonly int MoveXHash = Animator.StringToHash("MoveX");
    static readonly int MoveYHash = Animator.StringToHash("MoveY");
    static readonly int IsLockedOnHash = Animator.StringToHash("IsLockedOn");

    Animator animator;
    CharacterController controller;
    float verticalVelocity;

    // pass null to go back to free movement
    public void SetLockOnTarget(Transform target)
    {
        LockOnTarget = target;
        animator.SetBool(IsLockedOnHash, target != null);
    }

    void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        animator.applyRootMotion = true;
    }

    void Update()
    {
        Vector3 moveDir = CameraRelative(input.Move);
        float speed = GetTargetSpeed(input.Move.magnitude);
        float dt = Time.deltaTime;

        if (LockOnTarget != null)
        {
            Vector3 toTarget = LockOnTarget.position - transform.position;
            toTarget.y = 0f;
            RotateTowards(toTarget, lockedTurnSpeed);

            // no sprint strafe clips, so cap at run
            Vector3 local = transform.InverseTransformDirection(moveDir.normalized) * Mathf.Min(speed, runSpeedValue);
            animator.SetFloat(MoveXHash, local.x, speedDampTime, dt);
            animator.SetFloat(MoveYHash, local.z, speedDampTime, dt);
        }
        else
        {
            RotateTowards(moveDir, turnSpeed);
            // keep the strafe params roughly in sync so locking on mid-run doesn't pop
            animator.SetFloat(MoveXHash, 0f, speedDampTime, dt);
            animator.SetFloat(MoveYHash, Mathf.Min(speed, runSpeedValue), speedDampTime, dt);
        }

        animator.SetFloat(SpeedHash, speed, speedDampTime, dt);
    }

    void OnAnimatorMove()
    {
        // only use XZ from the animation, gravity handles Y
        Vector3 delta = animator.deltaPosition;

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = groundedStickVelocity;
        else
            verticalVelocity += gravity * Time.deltaTime;

        delta.y = verticalVelocity * Time.deltaTime;
        controller.Move(delta);
    }

    Vector3 CameraRelative(Vector2 stick)
    {
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();
        return Vector3.ClampMagnitude(forward * stick.y + right * stick.x, 1f);
    }

    float GetTargetSpeed(float inputAmount)
    {
        if (inputAmount < 0.01f) return 0f;
        if (input.SprintHeld) return sprintSpeedValue;
        return inputAmount < walkInputThreshold ? walkSpeedValue : runSpeedValue;
    }

    void RotateTowards(Vector3 direction, float degreesPerSecond)
    {
        if (direction.sqrMagnitude < 0.0001f) return;
        Quaternion target = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, degreesPerSecond * Time.deltaTime);
    }
}
