using UnityEngine;
using UnityEngine.UI;

// "E  Rest" while the player stands at something they can use.
public class InteractPromptDisplay : MonoBehaviour
{
    public PlayerInteractor interactor;
    public Text label;

    void Update()
    {
        Interactable current = interactor.Current;
        label.enabled = current != null;
        if (current != null) label.text = "E  " + current.Prompt;
    }
}
