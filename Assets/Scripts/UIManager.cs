using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
[System.Serializable]
public class AppEntry
{
    public string name;
    public Sprite icon;
    public GameObject appPanel;
}
public class UIManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject menuPanel;
    public GameObject firstSelected;
    [Header("Apps")]
    public List<AppEntry> apps = new List<AppEntry>();
    public Image activeAppIcon;
    public GameObject activeAppIconContainer;

    // active app tracking
    // -1 means no active app
    public int activeAppIndex = -1;
    public string activeAppName;
    public static Action<int> OnActiveAppChanged;

    [Header("Behavior")]
    public KeyCode toggleKey = KeyCode.Tab;
    public MonoBehaviour[] disableOnOpen;

    public bool menuOpen { get; private set; }

    void Start()
    {
        if (menuPanel) menuPanel.SetActive(false);
        menuOpen = false;
    }

    void Awake()
    {
        Instance = this;
    }

    public static UIManager Instance { get; private set; }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey)) ToggleMenu();
    }

    public void OpenMenu()
    {
        if (menuPanel) menuPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (disableOnOpen != null)
        {
            foreach (var c in disableOnOpen)
                if (c != null) c.enabled = false;
        }

        menuOpen = true;

        if (firstSelected != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelected);
        }
    }

    // Open an app by index (from apps list). Will open menu if closed.
    public void OpenApp(int index)
    {
        if (index < 0 || index >= apps.Count) return;

        // ensure menu visible
        if (!menuOpen) OpenMenu();

        // deactivate all app panels
        for (int i = 0; i < apps.Count; i++)
        {
            if (apps[i].appPanel != null)
                apps[i].appPanel.SetActive(i == index);
        }

        // show active icon if provided
        if (activeAppIcon != null)
        {
            activeAppIcon.sprite = apps[index].icon;
            if (activeAppIconContainer != null)
                activeAppIconContainer.SetActive(true);
        }

        // set active app
        activeAppIndex = index;
        activeAppName = apps[index].name;
        OnActiveAppChanged?.Invoke(activeAppIndex);
    }

    public void CloseApp(int index)
    {
        if (index < 0 || index >= apps.Count) return;
        if (apps[index].appPanel != null) apps[index].appPanel.SetActive(false);
        if (activeAppIndex == index)
        {
            activeAppIndex = -1;
            activeAppName = null;
            OnActiveAppChanged?.Invoke(-1);
        }
    }

    public void CloseAllApps()
    {
        for (int i = 0; i < apps.Count; i++) if (apps[i].appPanel != null) apps[i].appPanel.SetActive(false);
        activeAppIndex = -1;
        activeAppName = null;
        OnActiveAppChanged?.Invoke(-1);
    }

    // Close app panels without resetting the active app state
    private void CloseAllAppPanels()
    {
        for (int i = 0; i < apps.Count; i++) if (apps[i].appPanel != null) apps[i].appPanel.SetActive(false);
    }

    public void CloseMenu()
    {
        CloseAllAppPanels();

        if (menuPanel) menuPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (disableOnOpen != null)
        {
            foreach (var c in disableOnOpen)
                if (c != null) c.enabled = true;
        }

        menuOpen = false;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void ToggleMenu()
    {
        if (menuOpen) CloseMenu(); else OpenMenu();
    }
}
