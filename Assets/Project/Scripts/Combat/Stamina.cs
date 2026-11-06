using UnityEngine;

// Current stamina. Max comes from CombatStats (Endurance).
// An action can start with any stamina above 0, so one action can take it negative.
[RequireComponent(typeof(CombatStats))]
public class Stamina : MonoBehaviour
{
    public float regenRate = 30f; // per second
    public float regenDelay = 1f; // seconds after the last spend, or after the action ends, before regen starts

    public float Current { get; private set; }
    public float Max => stats.MaxStamina;
    public bool CanAct => Current > 0f;
    public bool RegenPaused { get; set; } // set by the combat FSM while attacking/dodging

    CombatStats stats;
    float lastSpendTime;

    void Awake()
    {
        stats = GetComponent<CombatStats>();
        Current = Max;
    }

    void Update()
    {
        // keep pushing the delay back while paused, so it counts from the end of the action
        if (RegenPaused) lastSpendTime = Time.time;
        if (Time.time - lastSpendTime < regenDelay) return;
        Current = Mathf.Min(Current + regenRate * Time.deltaTime, Max);
    }

    public void Spend(float amount)
    {
        Current -= amount;
        lastSpendTime = Time.time;
    }
}
