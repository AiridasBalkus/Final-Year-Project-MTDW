using UnityEngine;

// Hides and locks the cursor while playing. Esc frees it in the editor.
public class CursorLock : MonoBehaviour
{
    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
