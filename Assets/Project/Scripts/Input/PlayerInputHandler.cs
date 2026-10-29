using System;
using UnityEngine;
using UnityEngine.InputSystem;

// Reads the Player action map. Other scripts get input from here instead of the Input System.
public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 Move { get; private set; }
    public Vector2 Look { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool LookIsMouse { get; private set; } // mouse delta and stick values are on different scales

    public event Action LockOnPressed;

    PlayerControls controls;

    void OnEnable()
    {
        // created here instead of Awake because a script reload in play mode skips Awake
        controls ??= new PlayerControls();
        controls.Player.LockOn.performed += OnLockOn;
        controls.Player.Enable();
    }

    void OnDisable()
    {
        controls.Player.LockOn.performed -= OnLockOn;
        controls.Player.Disable();
    }

    void OnDestroy()
    {
        controls?.Dispose();
    }

    void Update()
    {
        var player = controls.Player;
        Move = Vector2.ClampMagnitude(player.Move.ReadValue<Vector2>(), 1f);
        Look = player.Look.ReadValue<Vector2>();
        SprintHeld = player.Sprint.IsPressed();

        if (player.Look.activeControl != null)
            LookIsMouse = player.Look.activeControl.device is Mouse;
    }

    void OnLockOn(InputAction.CallbackContext ctx)
    {
        LockOnPressed?.Invoke();
    }
}
