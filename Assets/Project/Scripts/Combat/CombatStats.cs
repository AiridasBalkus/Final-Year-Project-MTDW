using System;
using UnityEngine;

public class CombatStats : MonoBehaviour
{
    public int maxHealth = 100;
    public int attackPower = 20;
    public int defence = 5;

    public int Health { get; private set; }
    public bool IsDead => Health <= 0;
    public bool Invincible { get; set; } // dodge i-frames, set by the combat FSM

    public event Action<int> Damaged;
    public event Action Died;

    void Awake()
    {
        Health = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;

        Health = Mathf.Max(Health - amount, 0);
        Damaged?.Invoke(amount);
        if (IsDead) Died?.Invoke();
    }

    public void RestoreFullHealth()
    {
        Health = maxHealth;
    }
}
