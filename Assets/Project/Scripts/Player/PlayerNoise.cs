using UnityEngine;

// How far enemies can hear the player, from how fast they move. Walking is quiet enough to sneak.
public class PlayerNoise : MonoBehaviour
{
    public PlayerLocomotion locomotion;
    // metres
    public float walkRadius = 2f;
    public float runRadius = 7f;
    public float sprintRadius = 12f;

    public float Radius
    {
        get
        {
            float speed = locomotion.Speed;
            PlayerMovementConfig config = locomotion.config;
            if (speed > config.runSpeed + 0.1f) return sprintRadius;
            if (speed > config.walkSpeed + 0.1f) return runRadius;
            return speed > 0.1f ? walkRadius : 0f;
        }
    }
}
