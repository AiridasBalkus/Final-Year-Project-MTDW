using UnityEngine;

// Stands at spawn, or walks between patrol points if the brain has any.
public class EnemyIdleState : State
{
    readonly EnemyBrain brain;
    int pointIndex;
    float waitUntil;

    public EnemyIdleState(EnemyBrain brain) : base(brain.patrolPoints.Length > 0 ? "Patrol" : "Idle")
    {
        this.brain = brain;
    }

    public override void Enter()
    {
        brain.Motor.Stop();
        waitUntil = Time.time + brain.config.patrolWaitTime;
    }

    public override void Tick()
    {
        if (brain.CanSeeTarget())
        {
            brain.ChangeState(brain.Alert);
            return;
        }

        if (brain.patrolPoints.Length == 0 || Time.time < waitUntil)
        {
            brain.Motor.Stop();
            return;
        }

        Vector3 point = brain.patrolPoints[pointIndex].position;
        Vector3 toPoint = point - brain.transform.position;
        toPoint.y = 0f;

        if (toPoint.magnitude <= brain.config.arriveDistance)
        {
            pointIndex = (pointIndex + 1) % brain.patrolPoints.Length;
            waitUntil = Time.time + brain.config.patrolWaitTime;
            brain.Motor.Stop();
        }
        else
        {
            brain.Motor.MoveTo(point, brain.config.patrolSpeed, brain.config.turnSpeed);
        }
    }
}
