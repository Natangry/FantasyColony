using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach this to the Main Menu "Log" button to open/close the Dev Log overlay.
/// Keeps scene/prefab changes minimal and avoids coupling to MainMenuScreen implementation.
/// </summary>
[RequireComponent(typeof(Button))]
public class OpenDevLogButton : MonoBehaviour
{
    [Tooltip("Button that will toggle the Dev Log overlay. If left empty, this component will use the Button on the same GameObject.")]
    public Button button;

    [Tooltip("If true, the overlay will open (Show) instead of toggling when clicked.")]
    public bool openOnly = false;

    private void Reset()
    {
        if (button == null) button = GetComponent<Button>();
    }

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnClick);
    }

    private void OnClick()
    {
        if (openOnly) DevLogOverlay.Show(); else DevLogOverlay.ToggleVisible();
    }
}
