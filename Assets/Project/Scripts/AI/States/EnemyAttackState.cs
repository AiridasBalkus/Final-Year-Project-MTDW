using UnityEngine;

// Wind-up (glow, still turning to track) until AttackHit, then committed until AttackEnd.
public class EnemyAttackState : State
{
    public int attackIndex; // set by Reposition before entering

    static readonly int AttackHash = Animator.StringToHash("Attack");
    static readonly int AttackIdHash = Animator.StringToHash("AttackId");

    readonly EnemyBrain brain;
    readonly EnemyCombatState combat;
    bool windingUp;

    public EnemyAttackState(EnemyBrain brain, EnemyCombatState combat) : base("Attack")
    {
        this.brain = brain;
        this.combat = combat;
    }

    public override void Enter()
    {
        EnemyAttack attack = brain.config.attacks[attackIndex];
        brain.StartCooldown(attackIndex);
        brain.Resolver.BeginSwing(attack.attack);
        brain.Stamina.Spend(attack.attack.staminaCost);
        brain.Stamina.RegenPaused = true;

        brain.Animator.SetInteger(AttackIdHash, attack.animatorId);
        brain.Animator.SetTrigger(AttackHash);
        brain.Cue.On();
        windingUp = true;
        Debug.Log($"[Enemy] {brain.name} attack {attack.attack.name}");
    }

    public override void Tick()
    {
        brain.Motor.Stop();
        // tracking stops at the hit, so a well-timed dodge isn't punished by a late turn
        if (windingUp) brain.Motor.TurnTowards(brain.ToTarget, brain.config.turnSpeed);
    }

    public void OnHit()
    {
        if (!windingUp) return;
        windingUp = false;
        brain.Cue.Off();
        brain.Resolver.Resolve(brain.target);
    }

    public void OnEnd()
    {
        combat.Sub.Change(combat.Recover);
    }

    public override void Exit()
    {
        windingUp = false;
        brain.Cue.Off();
        brain.Animator.ResetTrigger(AttackHash);
        brain.Stamina.RegenPaused = false;
    }
}
