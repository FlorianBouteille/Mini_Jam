using UnityEngine;

public class butterfly : MonoBehaviour
{
    [Header("Light Detection (Charging)")]
    [Tooltip("Distance at which the enemy detects the light")]
    public float lightDetectionRange = 20f;
    [Tooltip("Small detection range when NO music is playing (always dangerous)")]
    public float chargeDetectionRangeNoMusic = 8f;

    [Header("Music Detection (Sleep)")]
    [Tooltip("Radius of sound detection (when Spotify is playing)")]
    public float musicDetectionRadius = 15f;

    [Header("Charging")]
    [Tooltip("Speed at which the butterfly charges toward the player")]
    public float chargeSpeed = 12f;

    [Header("Idle Flying")]
    [Tooltip("Speed while flying in circles")]
    public float idleFlightSpeed = 4f;
    [Tooltip("Radius of the circular flight pattern")]
    public float circleFlightRadius = 10f;
    [Tooltip("How much randomness in the circle (0-1, where 1 is chaotic)")]
    public float circleRandomness = 0.3f;

    [Header("Freeze & Sleep")]
    [Tooltip("Time spent decelerating before falling asleep")]
    public float freezeDuration = 1f;
    [Tooltip("Time spent falling asleep (motionless) before actual sleep")]
    public float fallAsleepDuration = 3.5f;
    [Tooltip("Duration the butterfly stays asleep")]
    public float sleepDuration = 5f;
    [Tooltip("How many times per second to update the wake timer (smooth countdown)")]
    public float wakeTimerTickRate = 2f;

    // Private state variables
    private enum ButterflyState { Idle, Charging, FreezePhase, FallingAsleep, Sleeping, WakingUp }
    private ButterflyState currentState = ButterflyState.Idle;

    private Transform playerTransform;
    private Light playerLight;
    private PlayerAppPowers playerAppPowers;
    private Rigidbody rb;
    private Vector3 startPosition;

    // Idle flying state
    private Vector3 circleCenter;
    private float circleAngle;
    private float circleRandomOffset;

    // Freeze & Sleep state
    private float freezeTimer;
    private float fallAsleepTimer;
    private float sleepTimer;
    private float wakeUpTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
        circleCenter = startPosition;

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
            Debug.LogWarning("ButterflyAI: Player not found. Make sure the player GameObject has tag 'Player'.");

        // Initialize state
        currentState = ButterflyState.Idle;
        circleAngle = Random.Range(0f, 360f);
        circleRandomOffset = Random.Range(-circleRandomness, circleRandomness);
    }

    void Update()
    {
        if (playerTransform == null) return;

        // Check distance to player for light detection
        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        bool playerInLightRange = distToPlayer < lightDetectionRange;

        // Check if player is actually in light cone
        bool inLightCone = false;
        if (playerLight != null && playerLight.enabled && playerLight.type == LightType.Spot)
        {
            inLightCone = IsInLightCone(playerLight);
        }

        // Check if music is playing and player is within audio range
        bool musicPlaying = playerAppPowers != null && playerAppPowers.MusicOn;
        bool inMusicRange = distToPlayer < musicDetectionRadius;

        // Check if player is close enough to charge (without music)
        bool closeEnoughToChargeNoMusic = distToPlayer < chargeDetectionRangeNoMusic;

        // State machine
        switch (currentState)
        {
            case ButterflyState.Idle:
                if (inLightCone && playerInLightRange)
                {
                    // Detected light → charge!
                    TransitionTo(ButterflyState.Charging);
                }
                else if (closeEnoughToChargeNoMusic && !musicPlaying)
                {
                    // Close enough and no music → charge!
                    TransitionTo(ButterflyState.Charging);
                }
                else if (musicPlaying && inMusicRange)
                {
                    // Detected music → start freezing
                    TransitionTo(ButterflyState.FreezePhase);
                }
                break;

            case ButterflyState.Charging:
                if (inLightCone && playerInLightRange)
                {
                    // Still in light → keep charging
                }
                else if (closeEnoughToChargeNoMusic && !musicPlaying)
                {
                    // Still close enough and no music → keep charging
                }
                else
                {
                    // Lost sight → back to idle
                    TransitionTo(ButterflyState.Idle);
                }
                break;

            case ButterflyState.FreezePhase:
                freezeTimer -= Time.deltaTime;
                if (freezeTimer <= 0f)
                {
                    // Transition to falling asleep
                    TransitionTo(ButterflyState.FallingAsleep);
                }
                break;

            case ButterflyState.FallingAsleep:
                // If music stops, wake up early and resume normal behavior
                if (!musicPlaying || !inMusicRange)
                {
                    TransitionTo(ButterflyState.Idle);
                }
                else
                {
                    fallAsleepTimer -= Time.deltaTime;
                    if (fallAsleepTimer <= 0f)
                    {
                        // Transition to full sleep
                        TransitionTo(ButterflyState.Sleeping);
                    }
                }
                break;

            case ButterflyState.Sleeping:
                // While sleeping, ignore light and sound
                // Only countdown the sleep timer
                sleepTimer -= Time.deltaTime;
                if (sleepTimer <= 0f)
                {
                    // Time to wake up
                    TransitionTo(ButterflyState.WakingUp);
                }
                break;

            case ButterflyState.WakingUp:
                // Brief transition state before returning to idle
                wakeUpTimer -= Time.deltaTime;
                if (wakeUpTimer <= 0f)
                {
                    TransitionTo(ButterflyState.Idle);
                }
                break;
        }

        // Update circle center (follow player loosely)
        circleCenter = Vector3.Lerp(circleCenter, playerTransform.position, Time.deltaTime * 0.5f);
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        switch (currentState)
        {
            case ButterflyState.Idle:
                IdleFlightBehavior();
                break;

            case ButterflyState.Charging:
                ChargeBehavior();
                break;

            case ButterflyState.FreezePhase:
                FreezeBehavior();
                break;

            case ButterflyState.FallingAsleep:
                SleepBehavior(); // Immobile while falling asleep
                break;

            case ButterflyState.Sleeping:
                SleepBehavior();
                break;

            case ButterflyState.WakingUp:
                SleepBehavior(); // Still immobile while waking up
                break;
        }
    }

    void TransitionTo(ButterflyState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case ButterflyState.Idle:
                circleRandomOffset = Random.Range(-circleRandomness, circleRandomness);
                break;

            case ButterflyState.Charging:
                // No special init needed
                break;

            case ButterflyState.FreezePhase:
                freezeTimer = freezeDuration;
                break;

            case ButterflyState.FallingAsleep:
                fallAsleepTimer = fallAsleepDuration;
                rb.linearVelocity = Vector3.zero;
                break;

            case ButterflyState.Sleeping:
                sleepTimer = sleepDuration;
                rb.linearVelocity = Vector3.zero;
                break;

            case ButterflyState.WakingUp:
                wakeUpTimer = 0.5f; // Brief 0.5 sec transition
                break;
        }
    }

    void IdleFlightBehavior()
    {
        // Fly in circles around the player with randomness
        circleAngle += Time.fixedDeltaTime * 30f; // Speed of circle rotation
        circleRandomOffset = Mathf.Lerp(circleRandomOffset, Random.Range(-circleRandomness, circleRandomness), Time.fixedDeltaTime);

        float angle = circleAngle + (circleRandomOffset * 60f);
        Vector3 targetPos = circleCenter + new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad) * circleFlightRadius,
            2f, // Slight height offset for flying
            Mathf.Sin(angle * Mathf.Deg2Rad) * circleFlightRadius
        );

        Vector3 dirToTarget = (targetPos - transform.position).normalized;
        rb.linearVelocity = dirToTarget * idleFlightSpeed;

        // Rotate toward flight direction
        Quaternion targetRot = Quaternion.LookRotation(dirToTarget);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 3f * Time.fixedDeltaTime);
    }

    void ChargeBehavior()
    {
        if (playerTransform == null) return;

        Vector3 dirToPlayer = (playerTransform.position - transform.position).normalized;
        rb.linearVelocity = dirToPlayer * chargeSpeed;

        // Rotate toward player
        Quaternion targetRot = Quaternion.LookRotation(dirToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 5f * Time.fixedDeltaTime);
    }

    void FreezeBehavior()
    {
        // Decelerate smoothly
        float decelerationFactor = freezeTimer / freezeDuration; // Goes from 1 to 0
        rb.linearVelocity = rb.linearVelocity * decelerationFactor;
    }

    void SleepBehavior()
    {
        // Completely immobile
        rb.linearVelocity = Vector3.zero;
    }

    bool IsInLightCone(Light light)
    {
        if (light == null || light.type != LightType.Spot) return false;

        // Distance check
        float distToLight = Vector3.Distance(transform.position, light.transform.position);
        if (distToLight > lightDetectionRange) return false;

        // Cone angle check
        Vector3 dirToEnemy = (transform.position - light.transform.position).normalized;
        float angleToEnemy = Vector3.Angle(light.transform.forward, dirToEnemy);
        float spotHalfAngle = light.spotAngle / 2f;

        return angleToEnemy < spotHalfAngle;
    }

    void OnDrawGizmosSelected()
    {
        // Visualize light detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lightDetectionRange);

        // Visualize music detection range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, musicDetectionRadius);

        // Visualize ideal circle flight path
        Gizmos.color = Color.green;
        float circleSteps = 16;
        for (int i = 0; i < circleSteps; i++)
        {
            float angle1 = (i / circleSteps) * 360f;
            float angle2 = ((i + 1) / circleSteps) * 360f;

            Vector3 pos1 = transform.position + new Vector3(
                Mathf.Cos(angle1 * Mathf.Deg2Rad) * circleFlightRadius,
                2f,
                Mathf.Sin(angle1 * Mathf.Deg2Rad) * circleFlightRadius
            );

            Vector3 pos2 = transform.position + new Vector3(
                Mathf.Cos(angle2 * Mathf.Deg2Rad) * circleFlightRadius,
                2f,
                Mathf.Sin(angle2 * Mathf.Deg2Rad) * circleFlightRadius
            );

            Gizmos.DrawLine(pos1, pos2);
        }
    }
}
