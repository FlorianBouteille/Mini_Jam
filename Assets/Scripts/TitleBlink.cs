using UnityEngine;
using UnityEngine.UI;

public class TitleBlink : MonoBehaviour
{
    public float speed = 1f;
    public float minAlpha = 0.5f;
    public float maxAlpha = 1f;

    private Image image;

    void Start()
    {
        image = GetComponent<Image>();
    }

    void Update()
    {
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * speed) + 1) / 2);
        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }
}