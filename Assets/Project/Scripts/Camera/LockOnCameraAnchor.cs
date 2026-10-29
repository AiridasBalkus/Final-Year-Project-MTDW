using UnityEngine;

// The lock-on camera follows this instead of the player. It turns towards the target
// by itself so the camera can be snappy while the character turns slower.
[DefaultExecutionOrder(-100)] // update before cinemachine
public class LockOnCameraAnchor : MonoBehaviour
{
    public PlayerLocomotion locomotion;
    public Transform followPoint;  // player's CameraTarget
    public Camera viewCamera;
    public float rotationSharpness = 10f; // higher = snappier

    void LateUpdate()
    {
        transform.position = followPoint.position;

        Transform target = locomotion.LockOnTarget;
        if (target == null)
        {
            // copy the camera while unlocked so locking on swings from the current view
            Vector3 camForward = viewCamera.transform.forward;
            camForward.y = 0f;
            if (camForward.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(camForward);
            return;
        }

        Vector3 toTarget = target.position - followPoint.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude < 0.0001f) return;

        float t = 1f - Mathf.Exp(-rotationSharpness * Time.deltaTime); // framerate independent smoothing
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(toTarget), t);
    }
}
