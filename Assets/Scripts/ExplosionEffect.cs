using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    [Header("Visual")]
    [Tooltip("Durée de l'effet d'explosion")]
    public float effectDuration = 1.5f;
    [Tooltip("Taille initiale de l'explosion")]
    public float initialScale = 1f;
    [Tooltip("Taille finale de l'explosion")]
    public float finalScale = 3f;

    [Header("Light")]
    [Tooltip("Créer une lumière d'explosion")]
    public bool createLight = true;
    [Tooltip("Intensité de la lumière")]
    public float lightIntensity = 2f;
    [Tooltip("Rayon de la lumière")]
    public float lightRange = 15f;
    [Tooltip("Couleur de la lumière")]
    public Color lightColor = Color.yellow;

    private float timer = 0f;
    private Light explosionLight;

    void Start()
    {
        // Créer une lumière temporaire
        if (createLight)
        {
            GameObject lightObj = new GameObject("ExplosionLight");
            lightObj.transform.parent = transform;
            lightObj.transform.localPosition = Vector3.zero;
            
            explosionLight = lightObj.AddComponent<Light>();
            explosionLight.type = LightType.Point;
            explosionLight.intensity = lightIntensity;
            explosionLight.range = lightRange;
            explosionLight.color = lightColor;
        }

        transform.localScale = Vector3.one * initialScale;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float progress = timer / effectDuration;

        // Augmenter l'échelle
        float scale = Mathf.Lerp(initialScale, finalScale, progress);
        transform.localScale = Vector3.one * scale;

        // Réduire l'opacité (si on a un renderer)
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            foreach (Material mat in renderer.materials)
            {
                Color color = mat.color;
                color.a = Mathf.Lerp(1f, 0f, progress);
                mat.color = color;
            }
        }

        // Réduire la lumière
        if (explosionLight != null)
        {
            explosionLight.intensity = Mathf.Lerp(lightIntensity, 0f, progress);
        }

        // Détruire l'effet après la durée
        if (timer >= effectDuration)
        {
            Destroy(gameObject);
        }
    }
}
