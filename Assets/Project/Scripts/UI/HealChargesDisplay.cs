using UnityEngine;
using UnityEngine.UI;

// One pip per heal charge, dimmed once used.
public class HealChargesDisplay : MonoBehaviour
{
    public HealFlask flask;
    public Image[] pips;
    public Color fullColor = new Color(0.95f, 0.75f, 0.2f);
    public Color usedColor = new Color(0.25f, 0.25f, 0.25f, 0.6f);

    void Update()
    {
        for (int i = 0; i < pips.Length; i++)
            pips[i].color = i < flask.Charges ? fullColor : usedColor;
    }
}
