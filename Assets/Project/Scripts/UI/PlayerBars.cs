using UnityEngine;

// Player health and stamina bars. Each fill is a RectTransform whose right anchor
// is moved to the current fraction, so no sprite is needed.
public class PlayerBars : MonoBehaviour
{
    public CombatStats stats;
    public Stamina stamina;
    public RectTransform healthFill;
    public RectTransform staminaFill;

    void Update()
    {
        SetFill(healthFill, (float)stats.Health / stats.MaxHealth);
        SetFill(staminaFill, stamina.Current / stamina.Max); // negative stamina shows as empty
    }

    void SetFill(RectTransform fill, float fraction)
    {
        Vector2 max = fill.anchorMax;
        max.x = Mathf.Clamp01(fraction);
        fill.anchorMax = max;
    }
}
