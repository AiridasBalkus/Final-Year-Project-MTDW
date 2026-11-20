using UnityEngine;

// Rest point: refills the player and becomes the respawn point. Enemies only reset on death.
public class Checkpoint : Interactable
{
    public GameLoop gameLoop;
    public Transform respawnPoint; // where and facing which way the player comes back

    public override string Prompt => "Rest";

    public override void Interact()
    {
        gameLoop.Rest(this);
    }
}
