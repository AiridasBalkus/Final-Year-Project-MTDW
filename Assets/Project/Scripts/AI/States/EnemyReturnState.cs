using UnityEngine;

// Walks back to spawn. Notices the player again once well inside the leash
// (near the edge it would just leash again); getting hit is handled by the brain.
public class EnemyReturnState : State
{
    readonly EnemyBrain brain;

    public EnemyReturnState(EnemyBrain brain) : base("Return")
    {
        this.brain = brain;
    }

    public override void Tick()
    {
        EnemyConfig config = brain.config;

        if (brain.DistanceFromSpawn <= config.arriveDistance)
        {
            brain.transform.rotation = brain.SpawnRotation;
            brain.ChangeState(brain.Idle);
            return;
        }

        if (brain.DistanceFromSpawn < config.leashDistance - config.reaggroMargin && brain.CanSeeTarget())
        {
            brain.ChangeState(brain.Alert);
            return;
        }

        brain.Motor.MoveTo(brain.SpawnPoint, config.returnSpeed, config.turnSpeed);
    }

    public override void Exit()
    {
        brain.Motor.Stop();
    }
}
