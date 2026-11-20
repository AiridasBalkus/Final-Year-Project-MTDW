// Composite: Reposition -> Attack -> Recover -> Reposition.
public class EnemyCombatState : State
{
    public EnemyRepositionState Reposition { get; }
    public EnemyAttackState Attack { get; }
    public EnemyRecoverState Recover { get; }

    readonly EnemyBrain brain;

    public EnemyCombatState(EnemyBrain brain) : base("Combat")
    {
        this.brain = brain;
        Sub = new StateMachine();
        Reposition = new EnemyRepositionState(brain, this);
        Attack = new EnemyAttackState(brain, this);
        Recover = new EnemyRecoverState(brain, this);
    }

    public override void Enter()
    {
        Sub.Change(Reposition);
    }

    // straight into an attack, skipping the reposition wait, reach check and cooldown (retaliation)
    public void ForceAttack(int index)
    {
        Attack.attackIndex = index;
        Sub.Change(Attack);
    }

    public override void Tick()
    {
        // only leave between attacks, never mid-swing or during the punish window
        if (Sub.Current != Reposition) return;

        if (brain.ShouldGiveUp)
            brain.ChangeState(brain.Return);
        else if (brain.DistanceToTarget > brain.CombatRange + brain.config.combatRangeMargin)
            brain.ChangeState(brain.Chase);
    }
}
