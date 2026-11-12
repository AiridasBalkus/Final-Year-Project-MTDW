using UnityEngine;

// Greybox shield flash: white on a block, red on a guard break.
public class ShieldFeedback : MonoBehaviour
{
    public Guard guard;
    public Renderer[] renderers; // materials need emission enabled
    [ColorUsage(false, true)] public Color blockColor = new Color(2f, 2f, 2f);
    [ColorUsage(false, true)] public Color breakColor = new Color(4f, 0.3f, 0.1f);
    public float blockFlashTime = 0.15f;
    public float breakFlashTime = 0.5f;

    static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

    Material[] materials;
    Color color;
    float flashStart, flashTime;

    void Awake()
    {
        materials = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            materials[i] = renderers[i].material;
    }

    void OnEnable() => guard.Blocked += OnBlocked;
    void OnDisable() => guard.Blocked -= OnBlocked;

    void OnBlocked(bool broke)
    {
        color = broke ? breakColor : blockColor;
        flashTime = broke ? breakFlashTime : blockFlashTime;
        flashStart = Time.time;
    }

    void Update()
    {
        if (flashTime <= 0f) return;
        float t = (Time.time - flashStart) / flashTime;
        if (t >= 1f) flashTime = 0f;
        SetGlow(color * (1f - Mathf.Clamp01(t)));
    }

    void SetGlow(Color c)
    {
        foreach (Material m in materials)
            m.SetColor(EmissionId, c);
    }
}
