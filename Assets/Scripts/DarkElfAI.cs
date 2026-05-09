using UnityEngine;

public class DarkElfAI : MonoBehaviour
{
    [Header("Detection")]
    [Tooltip("Base detection distance (when no music)")]
    public float detectionRangeBase = 5f;
    [Tooltip("Detection distance when player has Spotify playing")]
    public float detectionRangeWithMusic = 35f;

    [Header("Charging")]
    [Tooltip("Speed at which the enemy charges toward the player")]
    public float chargeSpeed = 8f;

    [Header("Patrol")]
    [Tooltip("Speed while patrolling")]
    public float patrolSpeed = 2f;
    [Tooltip("Distance to wander from starting position")]
    public float patrolRange = 15f;
    [Tooltip("Wait time before picking new patrol destination")]
    public float patrolWaitTime = 2f;

    [Header("Light Fear")]
    [Tooltip("Distance at which the enemy detects the light")]
    public float lightRepelRange = 15f;
    [Tooltip("Force of repulsion from the light")]
    public float lightRepelForce = 10f;
    [Tooltip("Time the enemy keeps fleeing after leaving the light")]
    public float fleeAfterLightDuration = 1f;

    private Transform playerTransform;
    private Light playerLight;
    private PlayerAppPowers playerAppPowers;
    private Rigidbody rb;
    private bool isCharging;
    private bool isInLight;
    
    // Animation
    private Animator animator;
    private const int ANIM_STATE_GUARD = 0;
    private const int ANIM_STATE_RUN = 1;
    
    // Patrol state
    private Vector3 startPosition;
    private Vector3 patrolDestination;
    private float patrolTimer;
    
    // Flee state
    private float fleeTimer;

    // attack

    private Collider attackCollider;
    private float detectionRange;  // Current active detection range
    private float attackDist = 2f;
    private float attackColliderStart = 0.3f;
    private float attackColliderEnd = 0.5f;
    private float attackColliderDelay = 0.917f; // Time between attacks
    private float attackColliderTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        attackCollider = GetComponentInChildren<BoxCollider>();
        if (attackCollider != null)
            attackCollider.enabled = false;
        attackColliderTimer = 0f;
        startPosition = transform.position;
        
        // Get animator from child
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
            Debug.LogWarning("DarkElfAI: Animator not found in children!");
        
        // Try to find player by tag
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            // Try to find the Light (could be on player or in children)
            playerLight = playerObj.GetComponentInChildren<Light>();
            if (playerLight == null)
                playerLight = playerObj.GetComponent<Light>();
            
            // Try to find PlayerAppPowers for music detection
            playerAppPowers = playerObj.GetComponent<PlayerAppPowers>();
            if (playerAppPowers == null)
                playerAppPowers = playerObj.GetComponentInChildren<PlayerAppPowers>();
        }
        else
            Debug.LogWarning("DarkElfAI: Player not found. Make sure the player GameObject has tag 'Player'.");
        
        // Initialize detection range
        detectionRange = detectionRangeBase;
        
        // Pick first patrol destination
        // PickNewPatrolDestination();
        
        // Set initial animation
        if (animator != null)
            animator.SetInteger("State", ANIM_STATE_GUARD);
    }

    void Update()
    {
        if (playerTransform == null) return;

        // Adapt detection range based on music
        if (playerAppPowers != null && playerAppPowers.MusicOn)
        {
            detectionRange = detectionRangeWithMusic;
        }
        else
        {
            detectionRange = detectionRangeBase;
        }

        // Check distance to player
        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        isCharging = distToPlayer < detectionRange;
        if (isCharging)
            if (animator != null)
                animator.SetInteger("State", ANIM_STATE_RUN);
        // Check if in light cone
        isInLight = false;
        if (playerLight != null && playerLight.enabled && playerLight.type == LightType.Spot)
        {
            isInLight = IsInLightCone(playerLight);
        }

        // Manage flee timer
        if (isInLight)
        {
            // In light: reset flee timer to duration
            fleeTimer = fleeAfterLightDuration;
        }
        else
        {
            // Out of light: countdown flee timer
            fleeTimer -= Time.deltaTime;
        }
        
        // // Update patrol timer (only when not fleeing and not charging)
        // if (!isCharging && fleeTimer <= 0f)
        // {
        //     patrolTimer -= Time.deltaTime;
        //     if (patrolTimer <= 0f)
        //         PickNewPatrolDestination();
        // }

        // Update animation based on state
        if (fleeTimer > 0f || isCharging)
        {
            if (animator != null)
                animator.SetInteger("State", ANIM_STATE_RUN);
        }
        else
        {
            if (animator != null)
                animator.SetInteger("State", ANIM_STATE_GUARD);
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        // Flee has priority over charge
        if (fleeTimer > 0f)
        {
            FleeFromLightBehavior();
        }
        else if (isCharging && playerTransform != null)
        {
            ChargeBehavior();
        }
        else
        {
            GuardBehavior();
        }
    }

    void ChargeBehavior()
    {
        // Direction toward player
        Vector3 dirToPlayer = (playerTransform.position - transform.position).normalized;

        // Apply velocity
        rb.linearVelocity = dirToPlayer * chargeSpeed;

        // Check distance to player
        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distToPlayer <= attackDist)
        {
            if (attackColliderTimer <= 0f)
                attackColliderTimer = 0f;

            attackColliderTimer += Time.fixedDeltaTime;
            if (attackColliderTimer >= attackColliderDelay)
                attackColliderTimer = 0f;
            else if (attackColliderTimer >= attackColliderEnd && attackCollider != null)
                attackCollider.enabled = false;
            else if (attackColliderTimer >= attackColliderStart && attackCollider != null)
                attackCollider.enabled = true;
            rb.linearVelocity = Vector3.zero;
            if (animator != null)
                animator.SetTrigger("attack");
        }
        else
        {
            attackColliderTimer = 0f;
            if (attackCollider != null)
                attackCollider.enabled = false;
        }
        // Rotate toward player
        Quaternion targetRot = Quaternion.LookRotation(dirToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 5f * Time.fixedDeltaTime);
    }

    void GuardBehavior()
    {
        rb.linearVelocity = Vector3.zero;

        // Check distance to player
        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distToPlayer < detectionRangeWithMusic)
        {

        // Direction toward player
        Vector3 dirToPlayer = (playerTransform.position - transform.position).normalized;

        // Rotate toward player
        Quaternion targetRot = Quaternion.LookRotation(dirToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 3f * Time.fixedDeltaTime);
        }
    }

    // void PickNewPatrolDestination()
    // {
    //     // Random point within patrol range
    //     Vector3 randomOffset = Random.insideUnitSphere * patrolRange;
    //     randomOffset.y = 0f; // Keep on same height
    //     patrolDestination = startPosition + randomOffset;
    //     patrolTimer = patrolWaitTime;
    // }

    bool IsInLightCone(Light light)
    {
        if (light == null || light.type != LightType.Spot) return false;

        // Distance check
        float distToLight = Vector3.Distance(transform.position, light.transform.position);
        if (distToLight > lightRepelRange) return false;

        // Cone angle check
        Vector3 dirToEnemy = (transform.position - light.transform.position).normalized;
        float angleToEnemy = Vector3.Angle(light.transform.forward, dirToEnemy);
        float spotHalfAngle = light.spotAngle / 2f;

        return angleToEnemy < spotHalfAngle;
    }

    void FleeFromLightBehavior()
    {
        if (playerLight == null) return;

        // Direction away from light
        Vector3 dirAwayFromLight = (transform.position - playerLight.transform.position).normalized;

        // Apply repulsion velocity
        rb.linearVelocity = dirAwayFromLight * lightRepelForce;

        // Rotate away from light
        Quaternion targetRot = Quaternion.LookRotation(dirAwayFromLight);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 5f * Time.fixedDeltaTime);
    }

    void OnDrawGizmosSelected()
    {
        // Visualize detection range in editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Visualize patrol range in editor
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, patrolRange);
    }
}
