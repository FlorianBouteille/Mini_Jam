using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [Tooltip("Maximum health in half-stars (10 = 5 full stars)")]
    public int maxHealth = 10;
    private int currentHealth;

    [Header("Invulnerability")]
    [Tooltip("Duration of invulnerability after taking damage")]
    public float invulnerabilityDuration = 1f;
    private float invulnerabilityTimer;

    [Header("Respawn")]
    [Tooltip("Position where the player respawns")]
    public Vector3 respawnPosition;

    // Events
    public static Action<int, int> OnHealthChanged; // (currentHealth, maxHealth)
    public static Action OnPlayerDeath;
    public static Action OnPlayerRespawn;

    public static PlayerHealth Instance { get; private set; }

    private Rigidbody rb;
    private PlayerControls playerControls;
    private UIManager uiManager;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerControls = GetComponent<PlayerControls>();
        uiManager = UIManager.Instance;

        // Initialize health to max
        currentHealth = maxHealth;
        respawnPosition = transform.position;

        if (uiManager == null)
            Debug.LogWarning("PlayerHealth: UIManager not found!");

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void Update()
    {
        // Decrement invulnerability timer
        if (invulnerabilityTimer > 0f)
            invulnerabilityTimer -= Time.deltaTime;
    }

    /// <summary>
    /// Deal 0.5 stars (half-star) damage to the player
    /// </summary>
    public void TakeDamage()
    {
        // Check if invulnerable
        if (invulnerabilityTimer > 0f)
            return;

        // Take damage
        currentHealth -= 1; // 1 unit = 0.5 stars
        invulnerabilityTimer = invulnerabilityDuration;

        Debug.Log($"PlayerHealth: Took damage! Current health: {currentHealth}/{maxHealth} ({GetHealthInStars()})");

        // Notify UI
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Check if dead
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Debug.Log("PlayerHealth: Player died!");
        OnPlayerDeath?.Invoke();

        // Respawn
        Respawn();
    }

    public void Respawn()
    {
        // Reset position
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            transform.position = respawnPosition;
        }
        else
        {
            transform.position = respawnPosition;
        }

        // Reset health
        currentHealth = maxHealth;
        invulnerabilityTimer = 0f;

        // Reset battery
        if (uiManager != null)
        {
            uiManager.battery = 100f;
        }

        // Notify UI
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnPlayerRespawn?.Invoke();

        Debug.Log("PlayerHealth: Player respawned!");
    }

    /// <summary>
    /// Get health in star format (e.g., 3.5 for 7 half-stars)
    /// </summary>
    public float GetHealthInStars()
    {
        return currentHealth / 2f;
    }

    public bool IsInvulnerable()
    {
        return invulnerabilityTimer > 0f;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    void OnDrawGizmosSelected()
    {
        // Visualize respawn position
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(respawnPosition, 0.5f);
    }

    void OnTriggerStay(Collider collision)
    {
        // Check if colliding with an enemy
        if (collision.CompareTag("Enemy"))
        {
            Debug.Log("Taking dammage");
            TakeDamage();
        }
    }
}
