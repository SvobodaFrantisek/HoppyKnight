using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    public GameManager gameManager;
    public FPSController fpsController;
    public bool ignoreDamageWhileDodging = true;

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

    public void TakeDamage(int damageAmount)
    {
        if (ShouldIgnoreDamage())
        {
            Debug.Log("Player dodged damage.");
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
