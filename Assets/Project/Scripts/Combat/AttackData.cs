using UnityEngine;

// One attack's tuning. HitResolver reads reach from here, the FSM reads stamina cost.
[CreateAssetMenu(menuName = "MTDW/Attack Data")]
public class AttackData : ScriptableObject
{
    public float range = 2.5f;    // metres
    public float halfAngle = 60f; // degrees either side of forward
    public float attackMultiplier = 1f;
    public float staminaCost = 20f;
    public float blockStaminaCost = 20f; // taken from a defender who blocks it
    public float poiseDamage = 10f;
    public bool hyperArmour; // enemy only: a poise break during this attack doesn't interrupt it, damage still applies

    public float lungeScale = 1f; // scales the clip's forward root motion, 1 = as animated

    // code lunge for clips without root motion, start/end in normalised clip time
    public float lungeDistance = 0f; // metres, 0 = none
    public float lungeStart = 0.2f;
    public float lungeEnd = 0.42f;
    public AnimationCurve lungeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // playback speed rolled per swing, keep within about ±10% so the timing stays learnable
    public float speedMin = 1f;
    public float speedMax = 1f;

    public float RollSpeed() => Random.Range(speedMin, speedMax);
}
