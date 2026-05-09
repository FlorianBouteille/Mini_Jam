using UnityEngine;

public class MessageTriggerZone : MonoBehaviour
{
    public string senderName = "Unknown";
    public string messageText = "Hello!";
    private bool triggered = false;

    void OnTriggerEnter(Collider collision)
    {
        if (triggered) return;
        if (!collision.CompareTag("Player")) return;

        Debug.Log("coucou toi ! ");
        triggered = true;
        if (MessagingSystem.Instance != null)
            MessagingSystem.Instance.ReceiveMessage(senderName, messageText);

        Debug.Log($"Message triggered: {senderName}");
    }
}