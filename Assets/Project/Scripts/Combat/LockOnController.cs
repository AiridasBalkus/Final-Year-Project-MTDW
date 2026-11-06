using Unity.Cinemachine;
using UnityEngine;

// Souls style lock-on. Picks a target, flicks between targets, and tells the
// locomotion and lock-on camera what we're locked onto.
public class LockOnController : MonoBehaviour
{
    [Header("References")]
    public PlayerInputHandler input;
    public PlayerLocomotion locomotion;
    public Camera viewCamera;
    public CinemachineCamera lockOnCamera;
    public CinemachineTargetGroup targetGroup; // should already have the player's camera target in it
    public Transform sightOrigin;              // line of sight rays start here

    [Header("Finding targets")]
    public float lockRange = 20f;
    public float unlockRange = 25f;  // a bit bigger than lockRange so it doesn't flicker at the edge
    public float viewAngle = 60f;    // half angle from the camera forward
    public LayerMask lineOfSightMask = ~0;

    [Header("Switching targets")]
    public float stickFlickThreshold = 0.85f;
    public float stickResetThreshold = 0.3f;  // stick has to come back under this before the next flick
    public float mouseFlickDistance = 400f;   // pixels of quick sideways movement needed
    public float mouseFlickWindow = 0.1f;     // how fast built up mouse movement fades (seconds)
    public float mouseRearmFraction = 0.2f;   // swipe has to die down to this before the next switch
    public float switchCooldown = 0.5f;

    [Header("Camera")]
    public int lockedPriority = 20;
    public int unlockedPriority = 0;
    public float targetGroupWeight = 1f;
    public float targetGroupRadius = 0.5f;

    public LockOnTarget Current { get; private set; }

    float nextSwitchTime;
    float mouseSwipe;
    bool stickReady = true;
    bool mouseReady = true;

    void OnEnable()
    {
        input.LockOnPressed += OnLockOnPressed;
    }

    void OnDisable()
    {
        input.LockOnPressed -= OnLockOnPressed;
        Unlock();
    }

    void Update()
    {
        if (Current == null) return;

        if (!Current.isActiveAndEnabled || DistanceTo(Current) > unlockRange)
        {
            Unlock();
            return;
        }

        HandleSwitching();
    }

    void OnLockOnPressed()
    {
        if (Current != null)
        {
            Unlock();
            return;
        }

        LockOnTarget best = FindClosestToScreenCentre();
        if (best != null) Lock(best);
    }

    LockOnTarget FindClosestToScreenCentre()
    {
        LockOnTarget best = null;
        float bestDist = float.MaxValue;
        Transform cam = viewCamera.transform;

        foreach (LockOnTarget target in LockOnTarget.all)
        {
            if (!CanTarget(target)) continue;

            Vector3 point = target.LockPoint.position;
            if (Vector3.Angle(cam.forward, point - cam.position) > viewAngle) continue;

            Vector3 screen = viewCamera.WorldToViewportPoint(point);
            float dist = new Vector2(screen.x - 0.5f, screen.y - 0.5f).sqrMagnitude;
            if (dist < bestDist)
            {
                bestDist = dist;
                best = target;
            }
        }
        return best;
    }

    void HandleSwitching()
    {
        float x = input.Look.x;
        bool mouse = input.LookIsMouse;

        // mouse movement adds up but fades fast, so only a quick swipe reaches the distance
        mouseSwipe = mouse ? mouseSwipe * Mathf.Exp(-Time.deltaTime / mouseFlickWindow) + x : 0f;
        if (!mouse && Mathf.Abs(x) < stickResetThreshold) stickReady = true;
        if (mouse && Mathf.Abs(mouseSwipe) < mouseFlickDistance * mouseRearmFraction) mouseReady = true;

        if (Time.time < nextSwitchTime) return;

        float dir;
        if (mouse)
        {
            if (!mouseReady || Mathf.Abs(mouseSwipe) < mouseFlickDistance) return;
            dir = Mathf.Sign(mouseSwipe);
        }
        else
        {
            if (!stickReady || Mathf.Abs(x) < stickFlickThreshold) return;
            dir = Mathf.Sign(x);
        }

        nextSwitchTime = Time.time + switchCooldown;
        stickReady = false;
        mouseReady = false;

        LockOnTarget next = FindNextTarget(dir);
        if (next != null) Lock(next);
    }

    // closest target on screen to the left (-1) or right (+1) of the current one
    LockOnTarget FindNextTarget(float dir)
    {
        float currentX = viewCamera.WorldToViewportPoint(Current.LockPoint.position).x;
        LockOnTarget best = null;
        float bestGap = float.MaxValue;

        foreach (LockOnTarget target in LockOnTarget.all)
        {
            if (target == Current || !CanTarget(target)) continue;

            Vector3 screen = viewCamera.WorldToViewportPoint(target.LockPoint.position);
            if (screen.z <= 0f) continue; // behind the camera

            float gap = (screen.x - currentX) * dir;
            if (gap > 0f && gap < bestGap)
            {
                bestGap = gap;
                best = target;
            }
        }
        return best;
    }

    bool CanTarget(LockOnTarget target)
    {
        return target.isActiveAndEnabled && DistanceTo(target) <= lockRange && HasLineOfSight(target);
    }

    bool HasLineOfSight(LockOnTarget target)
    {
        if (!Physics.Linecast(sightOrigin.position, target.LockPoint.position, out RaycastHit hit, lineOfSightMask, QueryTriggerInteraction.Ignore))
            return true;

        // hitting the target's own collider is fine
        return hit.transform.IsChildOf(target.transform);
    }

    float DistanceTo(LockOnTarget target)
    {
        return Vector3.Distance(transform.position, target.LockPoint.position);
    }

    void Lock(LockOnTarget target)
    {
        if (Current != null) targetGroup.RemoveMember(Current.LockPoint);

        Current = target;
        targetGroup.AddMember(target.LockPoint, targetGroupWeight, targetGroupRadius);
        locomotion.SetLockOnTarget(target.LockPoint);
        lockOnCamera.Priority = lockedPriority;
    }

    void Unlock()
    {
        if (Current == null) return;

        targetGroup.RemoveMember(Current.LockPoint);
        Current = null;
        locomotion.SetLockOnTarget(null);
        lockOnCamera.Priority = unlockedPriority;
    }
}
