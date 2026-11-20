using UnityEngine;

// Dark overlay with "YOU DIED", faded in by GameLoop on death and hidden on respawn.
[RequireComponent(typeof(CanvasGroup))]
public class DeathScreen : MonoBehaviour
{
    public float fadeTime = 0.8f; // seconds

    public bool Visible { get; private set; }

    CanvasGroup group;
    float shownAt;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        group.alpha = 0f;
    }

    public void Show()
    {
        Visible = true;
        shownAt = Time.time;
    }

    public void Hide()
    {
        Visible = false;
        group.alpha = 0f;
    }

    void Update()
    {
        if (Visible) group.alpha = Mathf.Clamp01((Time.time - shownAt) / fadeTime);
    }
}
