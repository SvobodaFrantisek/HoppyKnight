using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int health = 100;
    public Slider healthSlider;
    public GameObject objectToDisableOnDeath;
    public bool destroyOnDeath = false;

    [Header("Animation")]
    public Animator animator;
    public string getHitTriggerName = "GetHit";
    public string dieTriggerName = "Die";

    private bool isDead;

    void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    void Start()
    {
        health = maxHealth;
        RefreshHealthSlider();
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead)
        {
            return;
        }

        if (damageAmount <= 0)
        {
            return;
        }

        health -= damageAmount;

        if (health < 0)
        {
            health = 0;
        }

        RefreshHealthSlider();
        Debug.Log(gameObject.name + " HP: " + health);

        if (health <= 0)
        {
            Die();
            return;
        }

        PlayGetHitAnimation();
    }

    void RefreshHealthSlider()
    {
        if (healthSlider == null)
        {
            return;
        }

        healthSlider.maxValue = maxHealth;
        healthSlider.value = health;
    }

    void Die()
    {
        isDead = true;
        PlayDieAnimation();
        Debug.Log(gameObject.name + " died.");

        if (objectToDisableOnDeath != null)
        {
            objectToDisableOnDeath.SetActive(false);
            return;
        }

        if (destroyOnDeath)
        {
            Destroy(gameObject);
        }
    }

    void PlayGetHitAnimation()
    {
        if (animator != null)
        {
            animator.ResetTrigger(getHitTriggerName);
            animator.SetTrigger(getHitTriggerName);
        }
    }

    void PlayDieAnimation()
    {
        if (animator != null)
        {
            animator.ResetTrigger(dieTriggerName);
            animator.SetTrigger(dieTriggerName);
        }
    }
}
