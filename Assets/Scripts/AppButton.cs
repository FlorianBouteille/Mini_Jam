using UnityEngine;

public class AppButton : MonoBehaviour
{
    public int appIndex = 0;

    // Hook this to the Button OnClick() in the Inspector or call this from other code.
    public void OnClick_OpenApp()
    {
        if (UIManager.Instance != null) UIManager.Instance.OpenApp(appIndex);
    }

    public void OnClick_CloseApp()
    {
        if (UIManager.Instance != null) UIManager.Instance.CloseApp(appIndex);
    }
}
