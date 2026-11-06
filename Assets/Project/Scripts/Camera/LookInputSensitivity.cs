using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

// Separate mouse and gamepad sensitivity on the one Look action.
// Mouse delta is already per frame but a stick is per second, so one gain can't work for both.
[RequireComponent(typeof(CinemachineInputAxisController))]
public class LookInputSensitivity : MonoBehaviour
{
    public float mouseSensitivity = 0.12f;    // degrees per pixel
    public float stickHorizontalSpeed = 200f; // deg/sec at full stick
    public float stickVerticalSpeed = 120f;
    public bool invertY;

    void Awake()
    {
        GetComponent<CinemachineInputAxisController>().ReadControlValueOverride = ReadValue;
    }

    float ReadValue(InputAction action, IInputAxisOwner.AxisDescriptor.Hints hint, Object context,
        CinemachineInputAxisController.Reader.ControlValueReader defaultReader)
    {
        float value = defaultReader(action, hint, context, null);
        bool isY = hint == IInputAxisOwner.AxisDescriptor.Hints.Y;

        // moving up should look up, which means lowering the orbit angle
        if (isY && !invertY) value = -value;

        if (action.activeControl?.device is Mouse)
        {
            // cinemachine multiplies by deltaTime later, undo that for the mouse
            return value * mouseSensitivity / Mathf.Max(Time.deltaTime, 0.0001f);
        }

        return value * (isY ? stickVerticalSpeed : stickHorizontalSpeed);
    }
}
