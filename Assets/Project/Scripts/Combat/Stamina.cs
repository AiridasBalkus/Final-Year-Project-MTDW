using UnityEngine;

// Current stamina. Max comes from CombatStats (Endurance).
// An action can start with any stamina above 0, so one action can take it negative.
[RequireComponent(typeof(CombatStats))]
public class Stamina : MonoBehaviour
{
    public float regenRate = 30f; // per second
    public float regenDelay = 1f; // seconds since the last spend before regen starts

    public float Current { get; private set; }
    public float Max => stats.MaxStamina;
    public bool CanAct => Current > 0f;

    CombatStats stats;
    float lastSpendTime;

    void Awake()
    {
        stats = GetComponent<CombatStats>();
        Current = Max;
    }

    void Update()
    {
        if (Time.time - lastSpendTime < regenDelay) return;
        Current = Mathf.Min(Current + regenRate * Time.deltaTime, Max);
    }

    public void Spend(float amount)
    {
        Current -= amount;
        lastSpendTime = Time.time;
    }
}
