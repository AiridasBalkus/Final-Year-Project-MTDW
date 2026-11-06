using System;
using UnityEngine;

public class CombatStats : MonoBehaviour
{
    [Header("Stats")]
    [Range(10, 20)] public int vitality = 10;
    [Range(10, 20)] public int endurance = 10;
    [Range(10, 20)] public int strength = 10;
    [Range(10, 20)] public int precision = 10;
    [Range(10, 20)] public int poise = 10;
    public int defence = 5;

    // derived value = base + perPoint * stat
    [Header("Scaling")]
    public float healthBase = 50f;
    public float healthPerVitality = 5f;
    public float staminaBase = 50f;
    public float staminaPerEndurance = 5f;
    public float critBase = 0f;
    public float critPerPrecision = 0.01f; // 0.01 = 1% per point
    public float poiseBase = 0f;
    public float poisePerPoint = 0.8f;
    public float poiseResetDelay = 3f; // seconds without poise damage before it snaps back to max

    public int MaxHealth => Mathf.RoundToInt(healthBase + healthPerVitality * vitality);
    public float MaxStamina => staminaBase + staminaPerEndurance * endurance;
    public float CritChance => Mathf.Clamp01(critBase + critPerPrecision * precision);
    public float MaxPoise => poiseBase + poisePerPoint * poise;

    public int Health { get; private set; }
    public float CurrentPoise { get; private set; }
    public bool IsDead => Health <= 0;
    public bool Invincible { get; set; } // dodge i-frames, set by the combat FSM

    public event Action<int> Damaged;
    public event Action Died;
    public event Action<bool> PoiseDamaged; // true if the hit broke poise

    float lastPoiseDamageTime;

    void Awake()
    {
        Health = MaxHealth;
        CurrentPoise = MaxPoise;
    }

    void Update()
    {
        if (CurrentPoise < MaxPoise && Time.time - lastPoiseDamageTime >= poiseResetDelay)
            CurrentPoise = MaxPoise;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;

        Health = Mathf.Max(Health - amount, 0);
        Damaged?.Invoke(amount);
        if (IsDead) Died?.Invoke();
    }

    // returns true if poise broke, it then resets to max
    public bool TakePoiseDamage(float amount)
    {
        if (IsDead) return false;

        lastPoiseDamageTime = Time.time;
        CurrentPoise -= amount;
        bool broke = CurrentPoise <= 0f;
        if (broke) CurrentPoise = MaxPoise;

        PoiseDamaged?.Invoke(broke);
        return broke;
    }

    public void RestoreFullHealth()
    {
        Health = MaxHealth;
    }
}
