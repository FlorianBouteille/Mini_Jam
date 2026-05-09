using UnityEngine;
using System;
using System.Collections.Generic;

public class MessagingSystem : MonoBehaviour
{
    private List<Message> messages = new List<Message>();
    private bool hasUnreadMessages = false;

    public static Action<Message> OnMessageReceived; // Fired when a new message arrives
    public static Action OnMessageRead; // Fired when user opens messages app

    public static MessagingSystem Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Add a new message to the inbox
    /// </summary>
    public void ReceiveMessage(string sender, string text)
    {
        Message newMessage = new Message(sender, text);
        messages.Add(newMessage);
        hasUnreadMessages = true;

        Debug.Log($"MessagingSystem: Message from '{sender}': {text}");
        OnMessageReceived?.Invoke(newMessage);
    }

    /// <summary>
    /// Get all messages in reverse chronological order (newest first)
    /// </summary>
    public List<Message> GetMessages()
    {
        // Return a reversed list so newest is first
        List<Message> reversed = new List<Message>(messages);
        reversed.Reverse();
        return reversed;
    }

    /// <summary>
    /// Clear the unread notification
    /// </summary>
    public void MarkAsRead()
    {
        hasUnreadMessages = false;
        OnMessageRead?.Invoke();
    }

    public bool HasUnreadMessages()
    {
        return hasUnreadMessages;
    }

    public int GetMessageCount()
    {
        return messages.Count;
    }
}
