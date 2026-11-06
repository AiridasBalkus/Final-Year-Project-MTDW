using UnityEngine;

// Greybox telegraph: body and weapon glow during an attack's wind-up.
public class EnemyWindUpCue : MonoBehaviour
{
    public Renderer[] renderers; // materials need emission enabled
    [ColorUsage(false, true)] public Color glowColor = new Color(4f, 1.5f, 0f);
    public float rampTime = 0.3f;

    static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

    Material[] materials;
    float onAt = -1f;

    void Awake()
    {
        materials = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            materials[i] = renderers[i].material;
    }

    public void On()
    {
        onAt = Time.time;
    }

    public void Off()
    {
        onAt = -1f;
        SetGlow(Color.black);
    }

    void Update()
    {
        if (onAt < 0f) return;
        float t = Mathf.Clamp01((Time.time - onAt) / rampTime);
        SetGlow(glowColor * t);
    }

    void SetGlow(Color color)
    {
        foreach (Material m in materials)
            m.SetColor(EmissionId, color);
    }
}
