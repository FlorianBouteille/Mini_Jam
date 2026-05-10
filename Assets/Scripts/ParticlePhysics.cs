using UnityEngine;

public class ParticlePhysics : MonoBehaviour
{
    public float duration = 1f;
    public Vector3 direction = Vector3.up;
    public float speed = 5f;
    public float gravity = 9.8f;
    public bool isSmokeParticle = false;
    public bool isDebris = false;

    private float elapsed = 0f;
    private Vector3 velocity;
    private Renderer renderer;

    void Start()
    {
        velocity = direction * speed;
        renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float progress = elapsed / duration;

        // Appliquer la gravité
        velocity.y -= gravity * Time.deltaTime;

        // Déplacer la particule
        transform.position += velocity * Time.deltaTime;

        // Appliquer une rotation aux débris
        if (isDebris)
        {
            transform.Rotate(Random.Range(-500f, 500f) * Time.deltaTime, 
                           Random.Range(-500f, 500f) * Time.deltaTime, 
                           Random.Range(-500f, 500f) * Time.deltaTime);
        }

        // Réduire l'opacité
        if (renderer != null)
        {
            foreach (Material mat in renderer.materials)
            {
                Color color = mat.color;
                
                if (isSmokeParticle)
                {
                    // La fumée disparaît graduellement
                    color.a = Mathf.Lerp(1f, 0f, progress);
                }
                else if (isDebris)
                {
                    // Les débris disparaissent rapidement
                    color.a = Mathf.Lerp(1f, 0f, progress * progress);
                }
                else
                {
                    // Le feu disparaît rapidement au début
                    color.a = Mathf.Lerp(1f, 0f, Mathf.Pow(progress, 0.5f));
                }
                
                mat.color = color;
            }
        }

        // Réduire l'échelle (les particules rétrécissent)
        float scale = Mathf.Lerp(1f, 0.1f, progress);
        transform.localScale = Vector3.one * scale;

        // Détruire après la durée
        if (elapsed >= duration)
        {
            Destroy(gameObject);
        }
    }
}
