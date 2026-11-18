using UnityEngine;

// Locomotion playback speed (LocoSpeedMult) for the player and enemies. Stays ~1 inside the
// blend tree's range and speeds the clips up or down past it, so the feet match the movement.
public static class LocoPlayback
{
    const float MinMult = 0.5f;
    const float MaxMult = 2f;
    const float MinNaturalSpeed = 0.2f; // m/s, below this the blend is mostly idle, leave it at 1

    // actualVelocity is horizontal, m/s
    public static float Update(Animator animator, float current, Vector3 actualVelocity, float dampTime)
    {
        // during a transition the velocity mixes in the other state's clip
        if (animator.IsInTransition(0)) return current;

        // along the movement, the clips sway sideways every step
        float actualSpeed = actualVelocity.magnitude;
        Vector3 clipVelocity = animator.velocity;
        clipVelocity.y = 0f;
        float clipSpeed = actualSpeed > 0.01f ? Vector3.Dot(clipVelocity, actualVelocity / actualSpeed) : clipVelocity.magnitude;
        float natural = clipSpeed / current;
        float wanted = natural > MinNaturalSpeed ? Mathf.Clamp(actualSpeed / natural, MinMult, MaxMult) : 1f;
        return Mathf.Lerp(current, wanted, Time.deltaTime / dampTime);
    }
}
