using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Debug only, draws combat info in the scene view (game view too with Gizmos on):
// attack reach, HP and state, lock-on ranges and the current target.
public class CombatDebugGizmos : MonoBehaviour
{
    public bool showReach = true;
    public bool showLabel = true;
    public bool showLockOn = true;
    public float labelHeight = 2.2f;


    void OnDrawGizmos()
    {
        // not every object has all of these
        TryGetComponent(out CombatStats stats);
        TryGetComponent(out HitResolver resolver);
        TryGetComponent(out LockOnController lockOn);

        Vector3 feet = transform.position + Vector3.up * 0.05f; // stops z-fighting with the floor

        if (showReach && resolver != null) DrawReach(feet, resolver);
        if (showLockOn && lockOn != null) DrawLockOn(feet, lockOn);
        if (showLabel && stats != null) DrawLabel(stats);
    }

    void DrawReach(Vector3 feet, HitResolver resolver)
    {
        float arc = Mathf.Min(resolver.maxAngle * 2f, 360f);
        Vector3 forward = transform.forward;
        forward.y = 0f;
        Vector3 from = Quaternion.AngleAxis(-arc / 2f, Vector3.up) * forward.normalized;

        Handles.color = new Color(1f, 0.6f, 0f, 0.15f);
        Handles.DrawSolidArc(feet, Vector3.up, from, arc, resolver.range);
        Handles.color = new Color(1f, 0.6f, 0f);
        Handles.DrawWireArc(feet, Vector3.up, from, arc, resolver.range);
    }

    void DrawLockOn(Vector3 feet, LockOnController lockOn)
    {
        Handles.color = Color.green;
        Handles.DrawWireDisc(feet, Vector3.up, lockOn.lockRange);
        Handles.color = Color.red;
        Handles.DrawWireDisc(feet, Vector3.up, lockOn.unlockRange);

        if (lockOn.Current == null) return;

        Vector3 target = lockOn.Current.LockPoint.position;
        Handles.color = Color.yellow;
        Handles.DrawDottedLine(transform.position + Vector3.up, target, 4f);
        Handles.Label(target + Vector3.up * 0.8f, Vector3.Distance(transform.position, target).ToString("F1") + "m");
    }

    void DrawLabel(CombatStats stats)
    {
        string text = name;
        if (Application.isPlaying)
        {
            text += $"\nHP {stats.Health}/{stats.maxHealth}";
            if (TryGetComponent(out PlayerCombatFSM fsm)) text += "\n" + fsm.State;
            if (stats.Invincible) text += "\nI-FRAMES";
        }

        var style = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
        style.normal.textColor = stats.Invincible ? Color.cyan : Color.white;
        Handles.Label(transform.position + Vector3.up * labelHeight, text, style);
    }

}
