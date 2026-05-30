using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    // --------------------
    // Nastaveni z Inspectoru
    // --------------------
    public int maxHealth = 100;
    public int health = 100;
    public Slider healthSlider;
    public GameObject objectToDisableOnDeath;
    public bool destroyOnDeath = false;
    public float disableAfterDeathDelay = 2f;
    public float destroyAfterDeathDelay = 2f;

    [Header("Animation")]
    public Animator animator;
    public string getHitTriggerName = "GetHit";
    public string dieTriggerName = "Die";

    private bool isDead;
    private bool deathRoutineStarted;

    // --------------------
    // Unity zivotni cyklus
    // --------------------
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

    // --------------------
    // Prijimani damage
    // --------------------
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

        if (health <= 0)
        {
            Die();
            return;
        }

        PlayGetHitAnimation();
    }

    // --------------------
    // UI zdravi
    // --------------------
    void RefreshHealthSlider()
    {
        if (healthSlider == null)
        {
            return;
        }

        healthSlider.maxValue = maxHealth;
        healthSlider.value = health;
    }

    // --------------------
    // Smrt enemaka
    // --------------------
    void Die()
    {
        isDead = true;

        if (deathRoutineStarted)
        {
            return;
        }

        deathRoutineStarted = true;
        StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        StopEnemyBehaviour();
        DisableBarsOnDeath();
        PlayDieAnimation();

        yield return new WaitForSeconds(GetDeathDelay());

        DisableDeadObject();
    }

    float GetDeathDelay()
    {
        if (objectToDisableOnDeath != null)
        {
            return disableAfterDeathDelay;
        }

        if (destroyOnDeath)
        {
            return destroyAfterDeathDelay;
        }

        return disableAfterDeathDelay;
    }

    void StopEnemyBehaviour()
    {
        NavMeshAgent navMeshAgent = GetComponent<NavMeshAgent>();

        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();
            navMeshAgent.enabled = false;
        }

        DisableBehaviour(GetComponent<EnemyMeleeMovement>());
        DisableBehaviour(GetComponentInChildren<EnemyMeleeAttack>());
        DisableBehaviour(GetComponentInChildren<EnemyMeleeHitbox>());
        DisableBehaviour(GetComponent<EnemyThrowerMovement>());
        DisableBehaviour(GetComponent<EnemyThrowerAttack>());
        DisableBehaviour(GetComponent<AnimalMovement>());
    }

    void DisableBehaviour(MonoBehaviour behaviour)
    {
        if (behaviour != null)
        {
            behaviour.enabled = false;
        }
    }

    void DisableDeadObject()
    {
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

    void DisableBarsOnDeath()
    {
        DisableBarFromSlider(healthSlider);

        Transform barsTransform = FindChildByName(transform, "Enemy Bars");

        if (barsTransform != null)
        {
            barsTransform.gameObject.SetActive(false);
        }
    }

    void DisableBarFromSlider(Slider slider)
    {
        if (slider == null)
        {
            return;
        }

        Canvas sliderCanvas = slider.GetComponentInParent<Canvas>(true);

        if (sliderCanvas != null)
        {
            sliderCanvas.gameObject.SetActive(false);
            return;
        }

        slider.gameObject.SetActive(false);
    }

    Transform FindChildByName(Transform parent, string childName)
    {
        if (parent == null)
        {
            return null;
        }

        Transform[] childTransforms = parent.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < childTransforms.Length; i++)
        {
            if (childTransforms[i].name == childName)
            {
                return childTransforms[i];
            }
        }

        return null;
    }

    // --------------------
    // Animator
    // --------------------
    void PlayGetHitAnimation()
    {
        if (animator != null && !string.IsNullOrEmpty(getHitTriggerName))
        {
            animator.ResetTrigger(getHitTriggerName);
            animator.SetTrigger(getHitTriggerName);
        }
    }

    void PlayDieAnimation()
    {
        if (animator != null && !string.IsNullOrEmpty(dieTriggerName))
        {
            animator.ResetTrigger(dieTriggerName);
            animator.SetTrigger(dieTriggerName);
        }
    }
}
