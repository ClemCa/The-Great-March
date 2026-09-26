using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Put on any Button to open the settings menu. Self-wires so it can be dropped onto cloned
/// buttons without editing their persistent onClick list.
/// </summary>
[RequireComponent(typeof(Button))]
public class SettingsButton : MonoBehaviour
{
    private void Start()
    {
        var button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(Open);
    }

    public void Open()
    {
        if (MenuAudioManager.Instance != null)
            MenuAudioManager.Instance.PlayClick();
        var menu = SettingsMenu.FindAny();
        if (menu != null)
            menu.Show();
        else
            Debug.LogWarning("SettingsButton: no SettingsMenu found in the scene.");
    }
}
