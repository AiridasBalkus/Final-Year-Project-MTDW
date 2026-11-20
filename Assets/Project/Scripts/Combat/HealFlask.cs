using UnityEngine;

// Heal charges, used by the combat FSM. Refilled by resting or respawning (GameLoop).
public class HealFlask : MonoBehaviour
{
    public int maxCharges = 3;
    public int healAmount = 40;     // HP
    public float staminaCost = 15f;

    public int Charges { get; private set; }

    void Awake()
    {
        Charges = maxCharges;
    }

    public void UseCharge()
    {
        Charges--;
    }

    public void Refill()
    {
        Charges = maxCharges;
    }
}
