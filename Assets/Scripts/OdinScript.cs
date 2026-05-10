using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OdinScript : MonoBehaviour
{
    [SerializeField] private GameObject endScreenPanel;
    [SerializeField] private TextMeshProUGUI endMessageText;
    [SerializeField] private Image starsImage;
    
    [SerializeField] private string textNotMaxHealth = "Tu as réussi, mais pas sans blessures...";
    [SerializeField] private string textMaxHealth = "Parfait! Tu as réussi sans une égratignure!";

    private void OnTriggerEnter(Collider other)
    {
        // Vérifier que c'est le joueur qui entre
        if (other.CompareTag("Player"))
        {
            Debug.Log("Coucou !");
            ShowEndScreen();
        }
    }

    private void ShowEndScreen()
    {
        // Récupérer la santé du joueur
        int currentHealth = PlayerHealth.Instance.GetCurrentHealth();
        int maxHealth = PlayerHealth.Instance.GetMaxHealth();
        float stars = currentHealth / 2f;

        // Afficher le panel final
        if (endScreenPanel != null)
        {
            endScreenPanel.SetActive(true);
        }

        // Afficher le texte approprié selon la santé
        if (endMessageText != null)
        {
            if (stars == maxHealth / 2f) // Si santé au max
            {
                endMessageText.text = textMaxHealth;
            }
            else
            {
                endMessageText.text = textNotMaxHealth;
            }
        }

        // Afficher les étoiles avec le bon sprite
        if (starsImage != null)
        {
            HealthDisplayStars healthDisplay = FindObjectOfType<HealthDisplayStars>();
            if (healthDisplay != null)
            {
                Sprite healthSprite = healthDisplay.GetHealthSprite(currentHealth);
                if (healthSprite != null)
                {
                    starsImage.sprite = healthSprite;
                    starsImage.enabled = true;
                }
            }
        }

        // Optionnel: désactiver les contrôles du joueur
        PlayerControls playerControls = PlayerHealth.Instance.GetComponent<PlayerControls>();
        if (playerControls != null)
        {
            playerControls.enabled = false;
        }
    }
}

