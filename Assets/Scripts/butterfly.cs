using UnityEngine;

public class butterfly : MonoBehaviour
{
    [Header("Light Detection (Charging)")]
    [Tooltip("Distance at which the enemy detects the light")]
    public float lightDetectionRange = 20f;
    [Tooltip("Small detection range when NO music is playing (always dangerous)")]
    public float chargeDetectionRangeNoMusic = 5f;

    [Header("Music Detection (Sleep)")]
    [Tooltip("Radius of sound detection (when Spotify is playing)")]
    public float musicDetectionRadius = 15f;

    [Header("Charging")]
    [Tooltip("Speed at which the butterfly charges toward the player")]
    public float chargeSpeed = 12f;

    [Header("Explosion")]
    [Tooltip("Force d'impulsion appliquée au joueur lors de l'explosion")]
    public float explosionForce = 20f;
    [Tooltip("Rayon de l'explosion")]
    public float explosionRadius = 5f;
    [Tooltip("Prefab de l'effet d'explosion")]
    public GameObject explosionEffectPrefab;

    [Header("Health")]
    [Tooltip("Santé du papillon")]
    public float maxHealth = 3f;
    private float currentHealth;

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
    private enum ButterflyState { Idle, Charging, FreezePhase, FallingAsleep, Sleeping, WakingUp, Exploding }
    private ButterflyState currentState = ButterflyState.Idle;

    private Transform playerTransform;
    private Light playerLight;
    private PlayerAppPowers playerAppPowers;
    private Rigidbody rb;
    private Vector3 startPosition;
    private Rigidbody playerRb;

    // Animation
    private Animator animator;
    private const int ANIM_STATE_FLY = 0;
    private const int ANIM_STATE_FALL_ASLEEP = 1;
    private const int ANIM_STATE_SLEEP = 2;

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
        currentHealth = maxHealth;

        // Get animator from child
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
            Debug.LogWarning("ButterflyAI: Animator not found in children!");

        // Try to find player by tag
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerRb = playerObj.GetComponent<Rigidbody>();
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

        // Set initial animation
        if (animator != null)
            animator.SetInteger("State", ANIM_STATE_FLY);
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

            case ButterflyState.Exploding:
                // Ne rien faire pendant l'explosion
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
                if (animator != null) animator.SetInteger("State", ANIM_STATE_FLY);
                break;

            case ButterflyState.Charging:
                BestiaryManager.Instance?.UnlockImage("Butterfly");
                if (animator != null) animator.SetInteger("State", ANIM_STATE_FLY);
                break;

            case ButterflyState.FreezePhase:
                freezeTimer = freezeDuration;
                if (animator != null) animator.SetInteger("State", ANIM_STATE_FLY);
                break;

            case ButterflyState.FallingAsleep:
                fallAsleepTimer = fallAsleepDuration;
                rb.linearVelocity = Vector3.zero;
                if (animator != null) animator.SetInteger("State", ANIM_STATE_FALL_ASLEEP);
                break;

            case ButterflyState.Sleeping:
                BestiaryManager.Instance?.UnlockDescription("Butterfly");
                sleepTimer = sleepDuration;
                rb.linearVelocity = Vector3.zero;
                if (animator != null) animator.SetInteger("State", ANIM_STATE_SLEEP);
                break;

            case ButterflyState.WakingUp:
                wakeUpTimer = 0.5f; // Brief 0.5 sec transition
                if (animator != null) animator.SetInteger("State", ANIM_STATE_SLEEP);
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

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Créer un effet d'explosion visuel
        CreateExplosionEffect();

        // Appliquer une impulsion au joueur
        if (playerRb != null && playerTransform != null)
        {
            Vector3 explosionDirection = (playerTransform.position - transform.position).normalized;
            playerRb.AddForce(explosionDirection * explosionForce, ForceMode.Impulse);
        }

        // Détruire le papillon
        Destroy(gameObject);
    }

    void CreateExplosionEffect()
    {
        // GameObject parent pour l'explosion
        GameObject explosionObj = new GameObject("Explosion");
        explosionObj.transform.position = transform.position;

        // Ajouter une lumière flash (explosion principale)
        Light flashLight = explosionObj.AddComponent<Light>();
        flashLight.type = LightType.Point;
        flashLight.intensity = 8f;
        flashLight.range = 40f;
        flashLight.color = new Color(1f, 0.7f, 0.2f); // Orange chaud

        // Jouer le son d'explosion
        PlayExplosionSound(explosionObj);

        // Créer les particules de feu
        CreateFireParticles(explosionObj);

        // Créer les particules de fumée
        CreateSmokeParticles(explosionObj);

        // Créer les débris/étincelles
        CreateDebrisParticles(explosionObj);

        // Animation de la lumière
        StartCoroutine(AnimateExplosionLight(flashLight));

        // Détruire après 3 secondes
        Destroy(explosionObj, 3f);
    }

    void PlayExplosionSound(GameObject explosionObj)
    {
        // Créer une source audio
        AudioSource audioSource = explosionObj.AddComponent<AudioSource>();
        audioSource.volume = 1f;
        audioSource.pitch = Random.Range(0.9f, 1.1f); // Légère variation de pitch
        audioSource.spatialBlend = 1f; // 3D audio

        // Essayer de charger un son d'explosion
        AudioClip explosionClip = Resources.Load<AudioClip>("Sounds/explosion");
        
        if (explosionClip != null)
        {
            audioSource.PlayOneShot(explosionClip);
        }
        else
        {
            // Si pas de son, créer un bruit synthétique
            CreateSynthExplosionSound(audioSource);
        }
    }

    void CreateSynthExplosionSound(AudioSource audioSource)
    {
        // Créer un clip audio synthétique (explosion basse)
        int sampleRate = 44100;
        float duration = 0.5f;
        int samples = (int)(sampleRate * duration);
        float[] audioData = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float progress = t / duration;

            // Onde sinusoïdale décroissante pour simuler une explosion
            float frequency = Mathf.Lerp(200f, 50f, progress); // Fréquence qui baisse
            float amplitude = Mathf.Exp(-4f * progress); // Volume qui diminue
            float noise = Random.Range(-0.3f, 0.3f); // Bruit blanc

            audioData[i] = (Mathf.Sin(2f * Mathf.PI * frequency * t) * amplitude + noise * amplitude) * 0.8f;
        }

        // Créer le clip
        AudioClip clip = AudioClip.Create("ExplosionSound", samples, 1, sampleRate, false);
        clip.SetData(audioData, 0);

        audioSource.clip = clip;
        audioSource.Play();
    }

    void CreateFireParticles(GameObject parent)
    {
        int particleCount = 30;
        for (int i = 0; i < particleCount; i++)
        {
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            particle.transform.parent = parent.transform;
            particle.transform.localPosition = Vector3.zero;
            
            // Taille aléatoire
            float scale = Random.Range(0.1f, 0.4f);
            particle.transform.localScale = Vector3.one * scale;

            // Supprimer le collider
            Collider col = particle.GetComponent<Collider>();
            if (col != null)
                Destroy(col);

            // Matériau orange/rouge (explosion)
            Renderer renderer = particle.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.Lerp(new Color(1f, 0.5f, 0f), new Color(1f, 0.2f, 0f), Random.value);
            renderer.material = mat;

            // Ajouter un composant pour animer la particule
            ParticlePhysics pp = particle.AddComponent<ParticlePhysics>();
            pp.duration = Random.Range(0.8f, 1.5f);
            pp.direction = Random.onUnitSphere;
            pp.speed = Random.Range(8f, 15f);
            pp.gravity = 5f;
        }
    }

    void CreateSmokeParticles(GameObject parent)
    {
        int particleCount = 20;
        for (int i = 0; i < particleCount; i++)
        {
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            particle.transform.parent = parent.transform;
            particle.transform.localPosition = Vector3.zero;

            // Taille plus grande pour la fumée
            float scale = Random.Range(0.2f, 0.6f);
            particle.transform.localScale = Vector3.one * scale;

            // Supprimer le collider
            Collider col = particle.GetComponent<Collider>();
            if (col != null)
                Destroy(col);

            // Matériau orange (fumée d'explosion)
            Renderer renderer = particle.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.Lerp(new Color(1f, 0.6f, 0f), new Color(1f, 0.4f, 0.1f), Random.value);
            renderer.material = mat;

            // Ajouter un composant pour animer la particule
            ParticlePhysics pp = particle.AddComponent<ParticlePhysics>();
            pp.duration = Random.Range(1.5f, 2.5f);
            pp.direction = Random.onUnitSphere;
            pp.speed = Random.Range(3f, 8f);
            pp.gravity = -2f; // Remonte avec la chaleur
            pp.isSmokeParticle = true;
        }
    }

    void CreateDebrisParticles(GameObject parent)
    {
        int particleCount = 15;
        for (int i = 0; i < particleCount; i++)
        {
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            particle.transform.parent = parent.transform;
            particle.transform.localPosition = Vector3.zero;

            // Petits débris
            float scale = Random.Range(0.05f, 0.15f);
            particle.transform.localScale = Vector3.one * scale;

            // Supprimer le collider
            Collider col = particle.GetComponent<Collider>();
            if (col != null)
                Destroy(col);

            // Matériau orange/marron (débris brûlés)
            Renderer renderer = particle.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.Lerp(new Color(1f, 0.4f, 0f), new Color(0.8f, 0.3f, 0.1f), Random.value);
            renderer.material = mat;

            // Ajouter un composant pour animer la particule
            ParticlePhysics pp = particle.AddComponent<ParticlePhysics>();
            pp.duration = Random.Range(0.5f, 1.5f);
            pp.direction = Random.onUnitSphere;
            pp.speed = Random.Range(15f, 25f);
            pp.gravity = 15f;
            pp.isDebris = true;
        }
    }

    System.Collections.IEnumerator AnimateExplosionLight(Light light)
    {
        float elapsed = 0f;
        float duration = 0.3f; // Flash rapide

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            light.intensity = Mathf.Lerp(8f, 0f, progress * progress);
            yield return null;
        }

        light.intensity = 0f;
    }

    void Explode()
    {
        // Transition to exploding state
        TransitionTo(ButterflyState.Exploding);

        // Créer l'effet d'explosion visuel
        CreateExplosionEffect();

        // Appliquer une impulsion au joueur
        if (playerRb != null)
        {
            Vector3 explosionDirection = (playerTransform.position - transform.position).normalized;
            playerRb.AddForce(explosionDirection * explosionForce, ForceMode.Impulse);
        }

        // Désactiver et détruire le papillon
        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Vérifier si c'est le joueur
        if (collision.gameObject.CompareTag("Player") && currentState == ButterflyState.Charging)
        {
            collision.gameObject.GetComponent<PlayerHealth>()?.TakeDamage();
            Explode();
        }   
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
