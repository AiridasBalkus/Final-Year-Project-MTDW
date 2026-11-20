using UnityEngine;

// Slides up into the ceiling when opened, and stays open (also after a death).
public class BossDoor : Interactable
{
    public float openHeight = 4f; // metres
    public float openTime = 1.5f; // seconds

    public override string Prompt => "Open";
    public override bool CanInteract => !opened;

    bool opened;
    float openedAt;
    Vector3 closedPosition;

    void Awake()
    {
        closedPosition = transform.position;
    }

    public override void Interact()
    {
        opened = true;
        openedAt = Time.time;
        Debug.Log("[Level] boss door opened");
    }

    void Update()
    {
        if (!opened) return;
        float t = Mathf.Clamp01((Time.time - openedAt) / openTime);
        transform.position = closedPosition + Vector3.up * openHeight * Mathf.SmoothStep(0f, 1f, t);
    }
}
