using UnityEngine;

public class GeneratorButterfly : MonoBehaviour
{
    [Header("Butterfly Generation")]
    [Tooltip("Prefab du papillon à générer")]
    public GameObject butterflyPrefab;

    [Tooltip("Rayon de variation de spawn autour du générateur")]
    public float spawnRadius = 0.5f;

    [Tooltip("Délai avant de générer le premier papillon")]
    public float initialSpawnDelay = 0.5f;

    [Tooltip("Délai avant de générer un nouveau papillon après la mort")]
    public float respawnDelay = 1f;

    [Header("Spawn Particles")]
    [Tooltip("Afficher les particules de spawn")]
    public bool showSpawnParticles = true;

    [Tooltip("Nombre de particules")]
    public int particleCount = 15;

    [Tooltip("Durée des particules")]
    public float particleDuration = 1f;

    private GameObject currentButterfly;
    private butterfly butterflyScript;

    void Start()
    {
        // Générer le premier papillon après un court délai
        Invoke("SpawnButterfly", initialSpawnDelay);
    }

    void SpawnButterfly()
    {
        // Calculer une position de spawn aléatoire autour du générateur
        Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
        Vector3 finalPosition = transform.position + randomOffset;

        // Instancier le papillon
        if (butterflyPrefab != null)
        {
            currentButterfly = Instantiate(butterflyPrefab, finalPosition, Quaternion.identity);
            butterflyScript = currentButterfly.GetComponent<butterfly>();

            if (butterflyScript == null)
            {
                Debug.LogError("GeneratorButterfly: Le prefab n'a pas le script 'butterfly'!");
                return;
            }

            // Afficher les particules de spawn
            if (showSpawnParticles)
            {
                CreateSpawnParticles(finalPosition);
            }

            // Abonner à la mort du papillon
            StartCoroutine(WaitForButterflyDeath());
        }
        else
        {
            Debug.LogError("GeneratorButterfly: butterflyPrefab n'est pas assigné!");
        }
    }

    System.Collections.IEnumerator WaitForButterflyDeath()
    {
        // Attendre que le papillon soit détruit
        while (currentButterfly != null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        // Le papillon est mort, générer un nouveau après un délai
        yield return new WaitForSeconds(respawnDelay);
        SpawnButterfly();
    }

    void CreateSpawnParticles(Vector3 spawnPos)
    {
        for (int i = 0; i < particleCount; i++)
        {
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            particle.transform.position = spawnPos;
            
            // Taille aléatoire (petite)
            float scale = Random.Range(0.05f, 0.15f);
            particle.transform.localScale = Vector3.one * scale;

            // Supprimer le collider
            Collider col = particle.GetComponent<Collider>();
            if (col != null)
                Destroy(col);

            // Matériau jaune/blanc (particule de lumière)
            Renderer renderer = particle.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.Lerp(Color.yellow, Color.white, Random.value);
            renderer.material = mat;

            // Ajouter un composant pour animer la particule
            ParticlePhysics pp = particle.AddComponent<ParticlePhysics>();
            pp.duration = particleDuration;
            pp.direction = Random.onUnitSphere;
            pp.speed = Random.Range(5f, 12f);
            pp.gravity = -3f; // Remonte légèrement
        }
    }

    void OnDrawGizmosSelected()
    {
        // Visualiser la zone de spawn
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.3f);
    }
}

