using UnityEngine;

public class ActivateNext : MonoBehaviour
{
    [SerializeField] private GameObject nextZone;
    private float deactivationDelay = 0.5f;

    [SerializeField] private GameObject optionalObject;
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Coucou");
        // Vérifier que c'est le joueur qui entre
        if (other.CompareTag("Player"))
        {
            // Activer la zone suivante
            if (nextZone != null)
            {
                nextZone.SetActive(true);
            }
            if (optionalObject != null)
            {
                optionalObject.SetActive(true);
            }

            // Désactiver cette zone après un délai pour laisser le temps à l'autre script de s'exécuter
            Invoke(nameof(DeactivateZone), deactivationDelay);
        }
    }

    private void DeactivateZone()
    {
        gameObject.SetActive(false);
    }
}
