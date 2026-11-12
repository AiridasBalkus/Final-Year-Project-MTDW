using UnityEngine;

// Code-driven lunge for attack clips without root motion. Follows the attack state's normalised
// time (states tagged "Attack"), so it scales with the swing speed.
public class AttackLunge
{
    static readonly int AttackTag = Animator.StringToHash("Attack");

    AttackData attack;
    float lastProgress;
    bool started;

    public void Begin(AttackData data)
    {
        attack = data;
        lastProgress = 0f;
        started = false;
    }

    public void End()
    {
        attack = null;
    }

    // forward metres to move this frame
    public float Step(Animator animator)
    {
        if (attack == null || attack.lungeDistance <= 0f) return 0f;

        // the attack is the next state while blending in, the current one while blending out
        AnimatorStateInfo next = animator.GetNextAnimatorStateInfo(0);
        AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(0);
        float t;
        if (animator.IsInTransition(0) && next.tagHash == AttackTag) t = next.normalizedTime;
        else if (current.tagHash == AttackTag) t = current.normalizedTime;
        else return 0f;

        // back to back swings: the last swing's tail can still be playing, wait for this one to start
        if (!started)
        {
            if (t > Mathf.Max(attack.lungeStart, 0.1f)) return 0f;
            started = true;
        }

        float progress = attack.lungeCurve.Evaluate(Mathf.InverseLerp(attack.lungeStart, attack.lungeEnd, t));
        float step = (progress - lastProgress) * attack.lungeDistance;
        lastProgress = progress;
        return Mathf.Max(0f, step);
    }
}
