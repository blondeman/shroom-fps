using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StatusUI : MonoBehaviour
{
    public Image profile;
    public Sprite[] sprites;
    public Sprite[] blinkSprites;

    public int damageLevel = 0;

    public float blinkInterval = 4f;
    float blinkTimer = 0;

    public float bobHeight = 3;
    public float bobSpeed = 3;
    Vector2 profileOrigin;

    void Start()
    {
        blinkTimer = blinkInterval;
        profileOrigin = profile.transform.position;
    }

    void Update()
    {
        blinkTimer -= Time.deltaTime;
        if (blinkTimer < 0)
        {
            blinkTimer = blinkInterval;
            StartCoroutine(Blink());
        }

        HeadBob();
    }

    void HeadBob()
    {
        profile.transform.position = new Vector2(profileOrigin.x, profileOrigin.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight);
    }

    private IEnumerator Blink()
    {
        profile.sprite = blinkSprites[damageLevel];
        yield return new WaitForSeconds(0.3f);
        profile.sprite = sprites[damageLevel];
    }
}
