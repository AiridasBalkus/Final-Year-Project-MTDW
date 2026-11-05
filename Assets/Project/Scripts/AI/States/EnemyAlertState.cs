using UnityEngine;

// Short "noticed you" pause facing the player before the chase.
public class EnemyAlertState : State
{
    readonly EnemyBrain brain;
    float chaseAt;

    public EnemyAlertState(EnemyBrain brain) : base("Alert")
    {
        this.brain = brain;
    }

    public override void Enter()
    {
        chaseAt = Time.time + brain.config.alertTime;
    }

    public override void Tick()
    {
        brain.Motor.Stop();
        brain.Motor.TurnTowards(brain.ToTarget, brain.config.turnSpeed);

        if (Time.time >= chaseAt)
            brain.ChangeState(brain.Chase);
    }
}
