using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    // --------------------
    // Napojeni z Inspectoru
    // --------------------
    public GameManager gameManager;
    public FPSController fpsController;
    public bool ignoreDamageWhileDodging = true;

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

    // --------------------
    // Damage do hrace
    // --------------------
    public void TakeDamage(int damageAmount)
    {
        if (ShouldIgnoreDamage())
        {
            return;
        }

        if (gameManager != null)
        {
            gameManager.TakeDamage(damageAmount);
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
