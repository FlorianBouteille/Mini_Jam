using UnityEngine;

public class PlayerAppPowers : MonoBehaviour
{
    [Tooltip("Assign the Light used as the camera's light. If empty, the script will try to find one in children.")]
    public Light cameraLight;
    [Tooltip("If true the light will be on when no app is active. If false it stays off until the camera app is selected.")]
    public bool defaultOnWhenNoApp = false;

    void OnEnable()
    {
        UIManager.OnActiveAppChanged += HandleActiveAppChanged;
    }

    void OnDisable()
    {
        UIManager.OnActiveAppChanged -= HandleActiveAppChanged;
    }

    void Start()
    {
        if (cameraLight == null)
            cameraLight = GetComponentInChildren<Light>();

        if (cameraLight == null)
            Debug.LogWarning("PlayerAppPowers: No Light assigned or found in children.");
        else
            Debug.Log($"PlayerAppPowers: found Light '{cameraLight.name}', defaultOnWhenNoApp={defaultOnWhenNoApp}");

        // initialize state based on current active app
        if (UIManager.Instance != null)
            UpdateLightState(UIManager.Instance.activeAppIndex);
    }

    void HandleActiveAppChanged(int index)
    {
        UpdateLightState(index);
    }

    void UpdateLightState(int activeIndex)
    {
        bool on = false;
        if (activeIndex < 0)
        {
            on = defaultOnWhenNoApp;
        }
        else if (activeIndex >= 0 && UIManager.Instance != null && activeIndex < UIManager.Instance.apps.Count)
        {
            var app = UIManager.Instance.apps[activeIndex];
            if (app != null && !string.IsNullOrEmpty(app.name))
            {
                string n = app.name.ToLowerInvariant();
                if (n.Contains("camera") || n.Contains("appareil") || n.Contains("photo"))
                    on = true;
            }
        }

        if (cameraLight != null)
        {
            cameraLight.enabled = on;
            Debug.Log($"PlayerAppPowers: Set light '{cameraLight.name}' enabled={on} (activeIndex={activeIndex})");
        }
    }
}
