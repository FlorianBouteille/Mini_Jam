using UnityEngine;
using UnityEngine.SceneManagement;

public class portal : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;
    [SerializeField] private int numberOfOrbs = 3;
    [SerializeField] private float orbRotationSpeed = 100f;
    [SerializeField] private float orbDistance = 1.5f;
    [SerializeField] private float orbSize = 0.2f;
    [SerializeField] private Color orbColor = new Color(1f, 0.84f, 0f); // Couleur or
    [SerializeField] private float pulseScale = 1.2f;
    [SerializeField] private float pulseDuration = 0.5f;
    
    private Vector3 originalScale;
    private bool isPulsing = false;
    private float currentRotation = 0f;
    
    private void Start()
    {
        originalScale = transform.localScale;
    }
    
    private void Update()
    {
        // Rotation continue
        currentRotation += orbRotationSpeed * Time.deltaTime;
    }
    
    private void OnDrawGizmos()
    {
        // Afficher les orbes en preview dans l'éditeur
        DrawOrbs();
    }
    
    private void DrawOrbs()
    {
        float rotation = Application.isPlaying ? currentRotation : 0;
        
        for (int i = 0; i < numberOfOrbs; i++)
        {
            float angle = (360f / numberOfOrbs) * i + rotation;
            float x = Mathf.Cos(angle * Mathf.Deg2Rad) * orbDistance;
            float y = Mathf.Sin(angle * Mathf.Deg2Rad) * orbDistance;
            
            Vector3 orbPos = transform.position + new Vector3(x, y, 0);
            
            if (Application.isPlaying)
            {
                Gizmos.color = orbColor;
                Gizmos.DrawSphere(orbPos, orbSize);
            }
        }
    }
    
    // Détecte quand le joueur entre dans le portal
    private void OnTriggerEnter(Collider other)
    {
        // Vérifie si c'est le joueur
        if (other.CompareTag("Player") && !string.IsNullOrEmpty(sceneToLoad))
        {
            // Effet de pulsation
            if (!isPulsing)
            {
                StartCoroutine(PulseEffect());
            }
            
            // Change de scène après un court délai
            Invoke("LoadScene", pulseDuration);
        }
    }
    
    private System.Collections.IEnumerator PulseEffect()
    {
        isPulsing = true;
        float elapsedTime = 0f;
        
        while (elapsedTime < pulseDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / pulseDuration;
            
            // Scale pulse effect
            transform.localScale = Vector3.Lerp(originalScale, originalScale * pulseScale, progress);
            
            yield return null;
        }
        
        transform.localScale = originalScale;
        isPulsing = false;
    }
    
    private void LoadScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
