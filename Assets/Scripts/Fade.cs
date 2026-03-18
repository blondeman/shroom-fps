using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Fade : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI text;
    public float timer;

    public float fadeDuration = 5f;
    private float fadeTimer = 0f;
    private bool fading = false;

void Update()
{
    timer -= Time.deltaTime;
    if (timer <= 0)
    {
        fading = true;
    }

    if (fading)
    {
        fadeTimer += Time.deltaTime;
        float t = Mathf.Clamp01(fadeTimer / fadeDuration);
        Color c = Color.Lerp(new Color(1,1,1,1), new Color(1,1,1,0), t);
        icon.color = c;
        text.color = c;

        if (t==1)
        {
            Destroy(gameObject);
        }
    }
}
}
