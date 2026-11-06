using UnityEngine;

public class EnemyStaggeredState : State
{
    static readonly int StaggerHash = Animator.StringToHash("Stagger");

    readonly EnemyBrain brain;
    float doneAt;

    public EnemyStaggeredState(EnemyBrain brain) : base("Staggered")
    {
        this.brain = brain;
    }

    public override void Enter()
    {
        doneAt = Time.time + brain.config.staggerTime;
        brain.Animator.SetTrigger(StaggerHash);
    }

    public override void Tick()
    {
        brain.Motor.Stop();
        // Combat routes on from here (back to Chase or Return if the player is gone)
        if (Time.time >= doneAt)
            brain.ChangeState(brain.Combat);
    }

    public override void Exit()
    {
        brain.Animator.ResetTrigger(StaggerHash);
    }
}
