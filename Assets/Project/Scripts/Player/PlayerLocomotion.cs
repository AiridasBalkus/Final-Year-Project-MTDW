using UnityEngine;

// Moves the player by code at the config speeds, the animator just follows the real velocity.
// Attacks and the dodge use the clip's root motion along facing, plus the attack lunge.
[RequireComponent(typeof(Animator), typeof(CharacterController))]
public class PlayerLocomotion : MonoBehaviour
{
    public PlayerMovementConfig config;
    public PlayerInputHandler input;
    public Transform cameraTransform;
    public float paramDampTime = 0.1f;
    public float speedMultDampTime = 0.5f; // about one stride, root speed varies within a step

    [Header("Gravity")]
    public float gravity = -20f;
    public float groundedStickVelocity = -2f; // keeps the controller pressed onto slopes

    public Transform LockOnTarget { get; private set; }

    // turned off by the combat FSM during attacks, dodges and staggers
    public bool MovementEnabled { get; set; } = true;
    // set by the combat FSM while blocking or healing
    public bool WalkOnly { get; set; }
    // set by the combat FSM for attacks and the dodge, 1 = the clip's own distance
    public float ActionMotionScale { get; set; } = 1f;
    // deg/sec the character may still turn while MovementEnabled is off, set by the combat FSM
    public float ActionTurnSpeed { get; set; }
    public float Speed => velocity.magnitude; // m/s, from code movement (0 during actions)

    static readonly int SpeedHash = Animator.StringToHash("Speed");
    static readonly int MoveXHash = Animator.StringToHash("MoveX");
    static readonly int MoveYHash = Animator.StringToHash("MoveY");
    static readonly int IsLockedOnHash = Animator.StringToHash("IsLockedOn");
    static readonly int LocoSpeedMultHash = Animator.StringToHash("LocoSpeedMult");

    Animator animator;
    CharacterController controller;
    Vector3 velocity; // horizontal, m/s
    float verticalVelocity;
    float speedMult = 1f;
    readonly AttackLunge lunge = new AttackLunge();

    // pass null to go back to free movement
    public void SetLockOnTarget(Transform target)
    {
        LockOnTarget = target;
        animator.SetBool(IsLockedOnHash, target != null);
    }

    // the controller has to be off to move the transform directly
    public void Teleport(Vector3 position, Quaternion rotation)
    {
        controller.enabled = false;
        transform.SetPositionAndRotation(position, rotation);
        controller.enabled = true;
        velocity = Vector3.zero;
        verticalVelocity = 0f;
    }

    // on death: settle into idle, otherwise the frozen run params keep playing the run and its root motion moves the body
    public void Stop()
    {
        velocity = Vector3.zero;
        ActionMotionScale = 0f;
        ActionTurnSpeed = 0f;
        lunge.End();
        animator.SetFloat(SpeedHash, 0f);
        animator.SetFloat(MoveXHash, 0f);
        animator.SetFloat(MoveYHash, 0f);
    }

    void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        // on so OnAnimatorMove runs and actions can use the clip's motion
        animator.applyRootMotion = true;
    }

    void Update()
    {
        if (!MovementEnabled)
        {
            velocity = Vector3.zero;
            // swing wind-up: track the lock-on target, or the stick when free
            if (ActionTurnSpeed > 0f)
            {
                Vector3 dir = LockOnTarget != null ? LockOnTarget.position - transform.position : CameraRelative(input.Move);
                dir.y = 0f; // the lock point is at chest height, don't pitch the body
                RotateTowards(dir, ActionTurnSpeed);
            }
            return;
        }

        Vector3 moveDir = CameraRelative(input.Move);
        float speed = GetTargetSpeed(input.Move.magnitude);

        if (LockOnTarget != null)
        {
            // strafe clips cover every direction
            Vector3 toTarget = LockOnTarget.position - transform.position;
            toTarget.y = 0f;
            RotateTowards(toTarget, config.lockedTurnSpeed);
            Vector3 target = moveDir.normalized * Mathf.Min(speed, config.lockedRunSpeed);
            float rate = target.sqrMagnitude > velocity.sqrMagnitude ? config.acceleration : config.deceleration;
            velocity = Vector3.MoveTowards(velocity, target, rate * Time.deltaTime);
        }
        else
        {
            // only forward clips here, so move along facing and slow down in sharp turns
            RotateTowards(moveDir, config.turnSpeed);
            float facing = moveDir.sqrMagnitude > 0.0001f ? Vector3.Dot(transform.forward, moveDir.normalized) : 1f;
            float target = speed * Mathf.Clamp01(facing);
            float current = velocity.magnitude;
            float rate = target > current ? config.acceleration : config.deceleration;
            velocity = transform.forward * Mathf.MoveTowards(current, target, rate * Time.deltaTime);
        }

        UpdateAnimator();
    }

    void UpdateAnimator()
    {
        float dt = Time.deltaTime;
        // what the controller actually did last frame, so walking into a wall doesn't play a run
        Vector3 actual = controller.velocity;
        actual.y = 0f;
        Vector3 local = transform.InverseTransformDirection(actual);
        animator.SetFloat(SpeedHash, actual.magnitude, paramDampTime, dt);
        animator.SetFloat(MoveXHash, local.x, paramDampTime, dt);
        animator.SetFloat(MoveYHash, local.z, paramDampTime, dt);

        speedMult = LocoPlayback.Update(animator, speedMult, actual, speedMultDampTime);
        animator.SetFloat(LocoSpeedMultHash, speedMult);
    }

    void OnAnimatorMove()
    {
        Vector3 delta;
        if (MovementEnabled)
        {
            delta = velocity * Time.deltaTime;
        }
        else
        {
            // along facing only, the roll drifts sideways (backwards is kept for the slash's step back)
            float forward = Vector3.Dot(animator.deltaPosition, transform.forward);
            delta = transform.forward * (forward * ActionMotionScale + LungeStep());
        }

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = groundedStickVelocity;
        else
            verticalVelocity += gravity * Time.deltaTime;

        delta.y = verticalVelocity * Time.deltaTime;
        controller.Move(delta);
    }

    // stops short of the lock-on target, lunging into a close enemy pushes you out of the swing's arc
    float LungeStep()
    {
        float step = lunge.Step(animator);
        if (LockOnTarget == null) return step;
        Vector3 toTarget = LockOnTarget.position - transform.position;
        toTarget.y = 0f;
        return Mathf.Min(step, Mathf.Max(0f, toTarget.magnitude - config.lungeStopDistance));
    }

    public void BeginLunge(AttackData attack) => lunge.Begin(attack);
    public void EndLunge() => lunge.End();

    // so the dodge goes where the stick is pushed
    public void FaceMoveInput()
    {
        Vector3 dir = CameraRelative(input.Move);
        if (dir.sqrMagnitude < 0.01f) return;
        transform.rotation = Quaternion.LookRotation(dir);
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
        if (WalkOnly) return config.walkSpeed;
        if (input.SprintHeld) return config.sprintSpeed;
        return inputAmount < config.walkInputThreshold ? config.walkSpeed : config.runSpeed;
    }

    void RotateTowards(Vector3 direction, float degreesPerSecond)
    {
        if (direction.sqrMagnitude < 0.0001f) return;
        Quaternion target = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, degreesPerSecond * Time.deltaTime);
    }
}
