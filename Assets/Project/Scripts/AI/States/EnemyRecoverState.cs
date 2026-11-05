using UnityEngine;

// Punish window: stands still without turning.
public class EnemyRecoverState : State
{
    readonly EnemyBrain brain;
    readonly EnemyCombatState combat;
    float doneAt;

    public EnemyRecoverState(EnemyBrain brain, EnemyCombatState combat) : base("Recover")
    {
        this.brain = brain;
        this.combat = combat;
    }

    public override void Enter()
    {
        doneAt = Time.time + brain.config.recoverTime;
    }

    public override void Tick()
    {
        brain.Motor.Stop();
        if (Time.time >= doneAt)
            combat.Sub.Change(combat.Reposition);
    }
}
