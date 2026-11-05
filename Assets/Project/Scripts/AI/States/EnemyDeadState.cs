using UnityEngine;

// Final state. Disabling LockOnTarget makes the player's lock-on drop it.
public class EnemyDeadState : State
{
    static readonly int DieHash = Animator.StringToHash("Die");

    readonly EnemyBrain brain;

    public EnemyDeadState(EnemyBrain brain) : base("Dead")
    {
        this.brain = brain;
    }

    public override void Enter()
    {
        brain.Motor.Stop();
        brain.Animator.SetTrigger(DieHash);
        brain.GetComponent<LockOnTarget>().enabled = false;
        brain.GetComponent<Collider>().enabled = false;
        Debug.Log($"[Enemy] {brain.name} dead");
    }
}
