using UnityEngine;
using UnityEngine.UI;

public class HealthDisplayStars : MonoBehaviour
{
    [Header("Stars Display")]
    [SerializeField] private Image starDisplayImage;
    [Tooltip("Array of sprite images for each health level (5 stars, 4.5 stars, 4 stars, ..., 0.5 stars)")]
    public Sprite[] starSprites = new Sprite[9];

    void Start()
    {
        if (PlayerHealth.Instance == null)
        {
            Debug.LogError("HealthDisplayStars: PlayerHealth.Instance is NULL!");
            return;
        }

        // Subscribe to health changes
        PlayerHealth.OnHealthChanged += UpdateStarsDisplay;

        // Initial update
        UpdateStarsDisplay(PlayerHealth.Instance.GetCurrentHealth(), PlayerHealth.Instance.GetMaxHealth());
    }

    void OnDisable()
    {
        PlayerHealth.OnHealthChanged -= UpdateStarsDisplay;
    }

    void UpdateStarsDisplay(int currentHealth, int maxHealth)
    {
        // currentHealth is in half-stars
        // 10 = 5 stars, 9 = 4.5 stars, 8 = 4 stars, 7 = 3.5 stars, 6 = 3 stars, 5 = 2.5 stars, 4 = 2 stars, 3 = 1.5 stars, 2 = 1 star, 1 = 0.5 stars
        
        if (starDisplayImage == null)
        {
            Debug.LogError("HealthDisplayStars: starDisplayImage is not assigned!");
            return;
        }

        // Clamp to valid range
        int healthIndex = Mathf.Clamp(currentHealth, 1, 10);
        
        // Map: health 10->index 0, health 9->index 1, ..., health 1->index 8
        int spriteIndex = 10 - healthIndex;

        // Set the sprite and enable the image
        if (spriteIndex >= 0 && spriteIndex < starSprites.Length && starSprites[spriteIndex] != null)
        {
            starDisplayImage.sprite = starSprites[spriteIndex];
            starDisplayImage.enabled = true;
            Debug.Log($"HealthDisplayStars: Showing sprite {spriteIndex} for health {currentHealth} ({currentHealth / 2f} stars)");
        }
        else
        {
            starDisplayImage.enabled = false;
            Debug.LogWarning($"HealthDisplayStars: Sprite missing at index {spriteIndex}!");
        }
    }

    /// <summary>
    /// Retourne le sprite approprié pour une santé donnée
    /// </summary>
    public Sprite GetHealthSprite(int health)
    {
        // Clamp to valid range
        int healthIndex = Mathf.Clamp(health, 1, 10);
        
        // Map: health 10->index 0, health 9->index 1, ..., health 1->index 8
        int spriteIndex = 10 - healthIndex;

        if (spriteIndex >= 0 && spriteIndex < starSprites.Length && starSprites[spriteIndex] != null)
        {
            return starSprites[spriteIndex];
        }
        
        return null;
    }
}
