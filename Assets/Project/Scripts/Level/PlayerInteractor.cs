using UnityEngine;

// Picks the closest usable Interactable in range and uses it on Interact.
// Only from Locomotion, so not mid-attack, mid-roll, while blocking or healing.
public class PlayerInteractor : MonoBehaviour
{
    public PlayerInputHandler input;
    public PlayerCombatFSM fsm;

    public Interactable Current { get; private set; }

    Interactable[] interactables;

    void Awake()
    {
        interactables = FindObjectsByType<Interactable>();
    }

    void OnEnable()
    {
        input.InteractPressed += OnInteract;
    }

    void OnDisable()
    {
        input.InteractPressed -= OnInteract;
    }

    void Update()
    {
        Current = null;
        if (fsm.State != CombatState.Locomotion) return;

        float best = float.MaxValue;
        foreach (Interactable i in interactables)
        {
            if (!i.CanInteract) continue;
            Vector3 d = i.transform.position - transform.position;
            d.y = 0f;
            float dist = d.magnitude;
            if (dist <= i.range && dist < best)
            {
                best = dist;
                Current = i;
            }
        }
    }

    void OnInteract()
    {
        if (Current != null) Current.Interact();
    }
}
