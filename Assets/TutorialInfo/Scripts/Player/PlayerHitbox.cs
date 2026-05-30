using System.Collections;
using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    // --------------------
    // Napojeni z Inspectoru
    // --------------------
    public GameManager gameManager;
    public FPSController fpsController;
    public bool ignoreDamageWhileDodging = true;

    [Header("Burn Effect")]
    public GameObject burnEffectObject;
    public int burnDamagePerTick = 5;
    public float burnDuration = 4f;
    public float burnTickInterval = 1f;

    private Coroutine burnRoutine;

    // --------------------
    // Unity zivotni cyklus
    // --------------------
    void Awake()
    {
        CacheReferences();
        ValidateHitboxSettings();
    }

    void OnEnable()
    {
        CacheReferences();
        ValidateHitboxSettings();
    }

    void OnDisable()
    {
        if (burnRoutine != null)
        {
            StopCoroutine(burnRoutine);
            burnRoutine = null;
        }

        SetBurnEffectActive(false);
    }

    // --------------------
    // Damage do hrace
    // --------------------
    public bool TakeDamage(int damageAmount)
    {
        if (ShouldIgnoreDamage())
        {
            return false;
        }

        return ApplyDamage(damageAmount);
    }

    public void StartBurn()
    {
        StartBurn(burnDuration, burnDamagePerTick, burnTickInterval);
    }

    public void StartBurn(float duration, int damagePerTick, float tickInterval)
    {
        if (duration <= 0f)
        {
            return;
        }

        if (damagePerTick <= 0)
        {
            return;
        }

        if (tickInterval <= 0f)
        {
            tickInterval = 0.1f;
        }

        if (burnRoutine != null)
        {
            StopCoroutine(burnRoutine);
        }

        burnRoutine = StartCoroutine(BurnRoutine(duration, damagePerTick, tickInterval));
    }

    IEnumerator BurnRoutine(float duration, int damagePerTick, float tickInterval)
    {
        SetBurnEffectActive(true);

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            ApplyDamage(damagePerTick);

            float waitTime = Mathf.Min(tickInterval, duration - elapsedTime);
            yield return new WaitForSeconds(waitTime);

            elapsedTime += waitTime;
        }

        SetBurnEffectActive(false);
        burnRoutine = null;
    }

    bool ApplyDamage(int damageAmount)
    {
        if (gameManager == null)
        {
            return false;
        }

        if (damageAmount <= 0)
        {
            return false;
        }

        if (gameManager.health <= 0)
        {
            return false;
        }

        gameManager.TakeDamage(damageAmount);
        return true;
    }

    void SetBurnEffectActive(bool isActive)
    {
        if (burnEffectObject != null)
        {
            burnEffectObject.SetActive(isActive);
        }
    }

    bool ShouldIgnoreDamage()
    {
        if (!ignoreDamageWhileDodging)
        {
            return false;
        }

        if (fpsController == null)
        {
            return false;
        }

        return fpsController.IsDodging();
    }

    // --------------------
    // Automaticke dohledani referenci
    // --------------------
    void CacheReferences()
    {
        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }

        if (fpsController == null)
        {
            fpsController = GetComponentInParent<FPSController>();
        }
    }

    // --------------------
    // Kontrola nastaveni hitboxu
    // --------------------
    void ValidateHitboxSettings()
    {
        Collider hitboxCollider = GetComponent<Collider>();

        if (hitboxCollider == null)
        {
            Debug.LogWarning(gameObject.name + " nema Collider pro PlayerHitbox.");
            return;
        }

        if (!hitboxCollider.isTrigger)
        {
            Debug.LogWarning(gameObject.name + " ma PlayerHitbox collider, ale neni nastaveny jako Trigger.");
        }
    }
}
