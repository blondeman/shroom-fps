using UnityEngine;
using System.Collections;

public class Health: MonoBehaviour
{
    public float maxHealth = 100;
    float currentHealth;
    
    public float iTime = 0.2f;
    float iTimer; 

    public new Renderer renderer;
    public Color damageTint;
    Color originalColor;

    void Start()
    {
        if (renderer) originalColor = renderer.material.color;
        iTimer = 0;
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (iTimer <= 0)
        {
            currentHealth -= damage;
            iTimer = iTime;
            if (renderer) StartCoroutine(DamageTint());
            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }

    private IEnumerator DamageTint()
    {
        renderer.material.color = damageTint;
        yield return new WaitForSeconds(0.1f);
        renderer.material.color = originalColor;
    }

    void Die()
    {
        Destroy(gameObject);
    }

    void Update()
    {
        if (iTimer > 0)
        {
            iTimer -= Time.deltaTime;
        }
    }
}
