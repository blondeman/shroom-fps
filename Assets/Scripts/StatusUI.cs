using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StatusUI : MonoBehaviour
{
    public GameObject[] dead;
    public Image profile;
    public Sprite[] sprites;
    public Sprite[] blinkSprites;

    int damageLevel = 0;
    public float thresholdLevel1 = 0.5f;
    public float thresholdLevel2 = 0.3f;
    public Image healthFill;

    public float blinkInterval = 4f;
    float blinkTimer = 0;

    public float bobHeight = 3;
    public float bobSpeed = 3;
    Vector2 profileOrigin;

    void Start()
    {
        foreach(GameObject go in dead)
        {
            go.SetActive(false);
        }
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

    public void SetHealth(float percentage)
    {
        int lastLevel = damageLevel;
        healthFill.fillAmount = percentage;
        if (percentage <= 0) {
            damageLevel = 3;
            foreach(GameObject go in dead)
            {
                go.SetActive(true);
            }
        }else if (percentage <= thresholdLevel2) {
            damageLevel = 2;
        }else if (percentage <= thresholdLevel1) {
            damageLevel = 1;
        }else {
            damageLevel = 0;
        }
        
        if(damageLevel!=lastLevel) {
            profile.sprite = sprites[damageLevel];
        }
    }
}
