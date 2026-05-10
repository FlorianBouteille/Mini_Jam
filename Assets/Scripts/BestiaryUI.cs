using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BestiaryUI : MonoBehaviour
{
    public GameObject mobEntryPrefab;
    public Transform content;

    void OnEnable()
    {
        RefreshBestiary();
    }

    void RefreshBestiary()
    {
        // Clear existing entries
        foreach (Transform child in content)
            Destroy(child.gameObject);

        // Rebuild from BestiaryManager
        foreach (MonsterData monster in BestiaryManager.Instance.monsters)
        {
            if (!monster.imageUnlocked) continue;

            GameObject entry = Instantiate(mobEntryPrefab, content);
            Image img = entry.transform.Find("MonsterImage").GetComponent<Image>();
            TextMeshProUGUI txt = entry.GetComponentInChildren<TextMeshProUGUI>();

            if (img != null && monster.illustration != null)
                img.sprite = monster.illustration;

            if (txt != null)
                txt.text = monster.descriptionUnlocked ? monster.description : "???";
        }
    }
}