using UnityEngine;
using UnityEngine.AI;

// The agent only plans the path; root motion moves the enemy and rotation is done here.
[RequireComponent(typeof(Animator), typeof(NavMeshAgent))]
public class EnemyMotor : MonoBehaviour
{
    public float moveDampTime = 0.1f;

    static readonly int MoveXHash = Animator.StringToHash("MoveX");
    static readonly int MoveYHash = Animator.StringToHash("MoveY");

    Animator animator;
    NavMeshAgent agent;

    void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        agent.updateRotation = false;
        animator.applyRootMotion = true;
    }

    // speed is an animator blend value (walk 1, run 3)
    public void MoveTo(Vector3 destination, float speed, float turnSpeed)
    {
        agent.SetDestination(destination);

        Vector3 dir = agent.desiredVelocity;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f)
        {
            SetMove(Vector3.zero);
            return;
        }

        TurnTowards(dir, turnSpeed);
        SetMove(transform.InverseTransformDirection(dir.normalized) * speed);
    }

    public void Stop()
    {
        MoveLocal(Vector3.zero);
    }

    // moves relative to facing, e.g. strafing while facing the player; the NavMesh still clamps it
    public void MoveLocal(Vector3 local)
    {
        if (agent.hasPath) agent.ResetPath();
        SetMove(local);
    }

    public void TurnTowards(Vector3 direction, float degreesPerSecond)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) return;
        Quaternion target = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, degreesPerSecond * Time.deltaTime);
    }

    void SetMove(Vector3 local)
    {
        animator.SetFloat(MoveXHash, local.x, moveDampTime, Time.deltaTime);
        animator.SetFloat(MoveYHash, local.z, moveDampTime, Time.deltaTime);
    }

    void OnAnimatorMove()
    {
        // setting nextPosition clamps the step onto the NavMesh, which also handles height
        agent.nextPosition = transform.position + animator.deltaPosition;
        transform.position = agent.nextPosition;
    }
}
