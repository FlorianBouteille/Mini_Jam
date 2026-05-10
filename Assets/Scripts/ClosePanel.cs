using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ClosePanel : MonoBehaviour
{
    [SerializeField] private GameObject panelToClose;

    private void Start()
    {
        // Récupère le composant Button et s'abonne à l'événement de clic
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnCloseButtonClicked);
            
            // Force le panel Tuto au-dessus
            button.transform.root.GetComponent<RectTransform>().SetAsLastSibling();
            
            // Force le focus sur le bouton
            EventSystem.current.SetSelectedGameObject(button.gameObject);
        }
    }

    private void OnCloseButtonClicked()
    {
        if (panelToClose != null)
        {
            panelToClose.SetActive(false);
        }
    }
}
