using UnityEngine;

// Something the player can use with Interact when standing close enough (PlayerInteractor).
public abstract class Interactable : MonoBehaviour
{
    public float range = 2f; // metres, flat distance from this transform

    public abstract string Prompt { get; }
    public virtual bool CanInteract => true;
    public abstract void Interact();
}
