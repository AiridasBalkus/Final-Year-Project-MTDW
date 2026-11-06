public class EnemyChaseState : State
{
    readonly EnemyBrain brain;

    public EnemyChaseState(EnemyBrain brain) : base("Chase")
    {
        this.brain = brain;
    }

    public override void Tick()
    {
        if (brain.ShouldGiveUp)
        {
            brain.ChangeState(brain.Return);
            return;
        }

        if (brain.DistanceToTarget <= brain.CombatRange)
        {
            brain.ChangeState(brain.Combat);
            return;
        }

        brain.Motor.MoveTo(brain.target.transform.position, brain.config.chaseSpeed, brain.config.turnSpeed);
    }

    public override void Exit()
    {
        brain.Motor.Stop();
    }
}
