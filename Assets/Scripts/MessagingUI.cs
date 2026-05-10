using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MessagingUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Container for the list of messages (vertical layout)")]
    public Transform messageListContainer;
    [Tooltip("Prefab for a message list item button")]
    public GameObject messageListItemPrefab;
    [Tooltip("TextMeshPro for displaying the selected message content")]
    public TextMeshProUGUI selectedMessageText;
    [Tooltip("TextMeshPro for displaying the sender of selected message")]
    public TextMeshProUGUI selectedMessageSender;

    private List<GameObject> listItems = new List<GameObject>();
    private Message selectedMessage;

    void Awake()
    {
        // Subscribe to message events EARLY (even if script is inactive)
        if (MessagingSystem.Instance != null)
            MessagingSystem.OnMessageReceived += HandleMessageReceived;
    }

    void OnEnable()
    {
        // Refresh the list when the UI is shown
        RefreshMessageList();

        // Mark as read
        if (MessagingSystem.Instance != null)
            MessagingSystem.Instance.MarkAsRead();
    }

    void OnDestroy()
    {
        MessagingSystem.OnMessageReceived -= HandleMessageReceived;
    }

    void HandleMessageReceived(Message msg)
    {
        // Refresh list when a new message arrives
        RefreshMessageList();
    }

    void RefreshMessageList()
    {
        if (MessagingSystem.Instance == null) return;

        // Clear old list items
        foreach (GameObject item in listItems)
            Destroy(item);
        listItems.Clear();

        // Get messages (newest first)
        List<Message> messages = MessagingSystem.Instance.GetMessages();

        // Create list items
        foreach (Message msg in messages)
        {
            GameObject itemGO = Instantiate(messageListItemPrefab, messageListContainer);
            Button button = itemGO.GetComponent<Button>();
            TextMeshProUGUI buttonText = itemGO.GetComponentInChildren<TextMeshProUGUI>();

            if (buttonText != null)
                buttonText.text = $"<b>{msg.sender}</b>: {msg.text.Substring(0, Mathf.Min(30, msg.text.Length))}...";

            if (button != null)
            {
                Message msgRef = msg;
                button.onClick.AddListener(() => SelectMessage(msgRef));
            }

            listItems.Add(itemGO);
        }

        // Select first message if available
        if (messages.Count > 0)
            SelectMessage(messages[0]);
        else
            ClearSelection();
    }

    void SelectMessage(Message message)
    {
        selectedMessage = message;

        if (selectedMessageText != null)
            selectedMessageText.text = message.text;

        if (selectedMessageSender != null)
            selectedMessageSender.text = $"From: <b>{message.sender}</b>";
    }

    void ClearSelection()
    {
        selectedMessage = null;

        if (selectedMessageText != null)
            selectedMessageText.text = "No messages";

        if (selectedMessageSender != null)
            selectedMessageSender.text = "";
    }
}
