using UnityEngine;
using UnityEngine.AI;

// Moves the enemy by code at the speeds its states ask for. The agent only plans the path and keeps
// it on the NavMesh (its own speed settings aren't used). Attacks and death use the clip's root motion.
[RequireComponent(typeof(Animator), typeof(NavMeshAgent))]
public class EnemyMotor : MonoBehaviour
{
    public float paramDampTime = 0.1f;
    public float speedMultDampTime = 0.5f; // about one stride, root speed varies within a step

    public float Acceleration { get; set; } = 10f; // m/s², the brain sets it from its config
    public Vector3 Velocity => actualVelocity;     // horizontal, m/s, what it really moved

    static readonly int MoveXHash = Animator.StringToHash("MoveX");
    static readonly int MoveYHash = Animator.StringToHash("MoveY");
    static readonly int LocoSpeedMultHash = Animator.StringToHash("LocoSpeedMult");

    Animator animator;
    NavMeshAgent agent;
    Vector3 velocity, targetVelocity, actualVelocity;
    float speedMult = 1f;
    bool inAction;
    float actionScale;
    readonly AttackLunge lunge = new AttackLunge();

    void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        agent.updateRotation = false;
        // on so OnAnimatorMove runs and actions can use the clip's motion
        animator.applyRootMotion = true;
    }

    public void MoveTo(Vector3 destination, float speed, float turnSpeed)
    {
        agent.speed = speed;
        agent.SetDestination(destination);

        Vector3 dir = agent.desiredVelocity;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f)
        {
            targetVelocity = Vector3.zero;
            return;
        }

        TurnTowards(dir, turnSpeed);
        targetVelocity = dir.normalized * speed;
    }

    public void Stop()
    {
        MoveLocal(Vector3.zero, 0f);
    }

    // moves relative to facing, e.g. strafing while facing the player; the NavMesh still clamps it
    public void MoveLocal(Vector3 direction, float speed)
    {
        if (agent.hasPath) agent.ResetPath();
        targetVelocity = transform.TransformDirection(direction.normalized) * speed;
    }

    public void TurnTowards(Vector3 direction, float degreesPerSecond)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) return;
        Quaternion target = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, degreesPerSecond * Time.deltaTime);
    }

    // motionScale scales the clip's root motion (1 = as animated), attack adds its lunge
    public void BeginAction(float motionScale, AttackData attack = null)
    {
        inAction = true;
        actionScale = motionScale;
        if (attack != null) lunge.Begin(attack);
        else lunge.End();
        velocity = Vector3.zero;
        targetVelocity = Vector3.zero;
    }

    public void EndAction()
    {
        inAction = false;
        lunge.End();
    }

    void Update()
    {
        if (inAction) return;

        velocity = Vector3.MoveTowards(velocity, targetVelocity, Acceleration * Time.deltaTime);

        float dt = Time.deltaTime;
        Vector3 local = transform.InverseTransformDirection(actualVelocity);
        animator.SetFloat(MoveXHash, local.x, paramDampTime, dt);
        animator.SetFloat(MoveYHash, local.z, paramDampTime, dt);
        speedMult = LocoPlayback.Update(animator, speedMult, actualVelocity, speedMultDampTime);
        animator.SetFloat(LocoSpeedMultHash, speedMult);
    }

    void OnAnimatorMove()
    {
        Vector3 delta;
        if (inAction)
        {
            float forward = Vector3.Dot(animator.deltaPosition, transform.forward); // along facing, no sideways drift
            delta = transform.forward * (forward * actionScale + lunge.Step(animator));
        }
        else
        {
            delta = velocity * Time.deltaTime;
        }

        // setting nextPosition clamps the step onto the NavMesh, which also handles height
        Vector3 before = transform.position;
        agent.nextPosition = before + delta;
        transform.position = agent.nextPosition;

        Vector3 moved = transform.position - before;
        moved.y = 0f;
        actualVelocity = Time.deltaTime > 0f ? moved / Time.deltaTime : Vector3.zero;
        agent.velocity = actualVelocity; // so local avoidance sees how it really moves
    }
}
