using System.Collections.Generic;
using UnityEngine;

// Put this on anything the player should be able to lock onto.
public class LockOnTarget : MonoBehaviour
{
    // every active target, so the lock-on doesn't have to search the scene
     public static List<LockOnTarget> all = new List<LockOnTarget>();

    public Transform lockPoint; // optional, e.g. the chest. uses this transform if empty

    public Transform LockPoint => lockPoint != null ? lockPoint : transform;

    void OnEnable()
    {
        all.Add(this);
    }

    void OnDisable()
    {
        all.Remove(this);
    }
}
