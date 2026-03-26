using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Health: MonoBehaviour
{
    public float maxHealth = 100;
    float currentHealth;
    
    public float iTime = 0.2f;
    float iTimer; 

    public new Renderer renderer;
    public GameObject deathParticle;
    public Color damageTint;
    Color originalColor;

    public StatusUI status;
    public MonoBehaviour[] controllers;

    public AudioClip[] hitSounds;

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
            AudioSource.PlayClipAtPoint(hitSounds[Random.Range(0, hitSounds.Length-1)], transform.position);
            currentHealth -= damage;
            iTimer = iTime;
            if (renderer) StartCoroutine(DamageTint());
            if(status) status.SetHealth(currentHealth / maxHealth);
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
        if (!status) {
            GetComponent<CharacterController>().enabled = false;
            Instantiate(deathParticle, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
            foreach(MonoBehaviour mb in controllers)
            {
                mb.enabled = false;
            }
        }
    }

    public void Respawn()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void Update()
    {
        if (iTimer > 0)
        {
            iTimer -= Time.deltaTime;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit) {
        if (hit.transform.tag == "Hazard" && iTimer <= 0) {
            TakeDamage(10);
            iTimer = 0.5f;
        }
    }

}
