using UnityEngine;

// Faces the player and holds spacing (closes in, strafes, or backs off), then attacks once the wait is over.
public class EnemyRepositionState : State
{
    readonly EnemyBrain brain;
    readonly EnemyCombatState combat;
    float attackAt;
    float strafeDir;

    public EnemyRepositionState(EnemyBrain brain, EnemyCombatState combat) : base("Reposition")
    {
        this.brain = brain;
        this.combat = combat;
    }

    public override void Enter()
    {
        EnemyConfig config = brain.config;
        attackAt = Time.time + Random.Range(config.repositionTimeMin, config.repositionTimeMax);
        strafeDir = Random.value < 0.5f ? -1f : 1f;
    }

    public override void Tick()
    {
        EnemyConfig config = brain.config;
        brain.Motor.TurnTowards(brain.ToTarget, config.turnSpeed);

        if (Time.time >= attackAt)
        {
            int attack = brain.PickAttack();
            if (attack >= 0)
            {
                combat.Attack.attackIndex = attack;
                combat.Sub.Change(combat.Attack);
                return;
            }
        }

        float dist = brain.DistanceToTarget;
        Vector3 move;
        if (dist > config.preferredDistance) move = Vector3.forward;
        else if (dist < config.minDistance) move = Vector3.back;
        else move = Vector3.right * strafeDir;
        brain.Motor.MoveLocal(move * config.repositionSpeed);
    }
}
