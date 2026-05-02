using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    // --------------------
    // Napojeni na combat
    // --------------------
    public CombatScript combatScript;

    // --------------------
    // Stav behem hry
    // --------------------
    private bool isInsideEnemyHitbox;
    private EnemyHealth currentEnemyHealth;

    // --------------------
    // Unity zivotni cyklus
    // --------------------
    void Awake()
    {
        CacheCombatScript();
        ValidateHitboxSettings();
    }

    void OnEnable()
    {
        CacheCombatScript();
        ValidateHitboxSettings();
        isInsideEnemyHitbox = false;
        currentEnemyHealth = null;
    }

    // --------------------
    // Trigger mece
    // --------------------
    void Update()
    {
        TryRegisterCurrentHit();
    }

    void OnTriggerEnter(Collider other)
    {
        EnemyHealth enemyHealth = GetEnemyHealth(other);

        if (enemyHealth == null)
        {
            return;
        }

        isInsideEnemyHitbox = true;
        currentEnemyHealth = enemyHealth;
    }

    void OnTriggerExit(Collider other)
    {
        EnemyHealth enemyHealth = GetEnemyHealth(other);

        if (enemyHealth == null)
        {
            return;
        }

        if (enemyHealth == currentEnemyHealth)
        {
            isInsideEnemyHitbox = false;
            currentEnemyHealth = null;
        }
    }

    // --------------------
    // Zapocitani hitu
    // --------------------
    void TryRegisterCurrentHit()
    {
        if (combatScript == null)
        {
            return;
        }

        if (!isInsideEnemyHitbox)
        {
            return;
        }

        if (!combatScript.CanHitNow())
        {
            return;
        }

        if (currentEnemyHealth == null)
        {
            return;
        }

        currentEnemyHealth.TakeDamage(combatScript.attackDamage);
        combatScript.RegisterHit();
    }

    // --------------------
    // Hledani EnemyHitboxu
    // --------------------
    EnemyHealth GetEnemyHealth(Collider other)
    {
        if (!IsEnemyHitbox(other))
        {
            return null;
        }

        return other.GetComponentInParent<EnemyHealth>();
    }

    bool IsEnemyHitbox(Collider other)
    {
        if (other == null)
        {
            return false;
        }

        return other.gameObject.layer == LayerMask.NameToLayer("EnemyHitbox");
    }

    // --------------------
    // Reference a validace
    // --------------------
    void CacheCombatScript()
    {
        if (combatScript == null)
        {
            combatScript = GetComponentInParent<CombatScript>();
        }
    }

    void ValidateHitboxSettings()
    {
        Collider hitboxCollider = GetComponent<Collider>();

        if (hitboxCollider == null)
        {
            Debug.LogWarning(gameObject.name + " nema Collider pro SwordHitbox.");
            return;
        }

        if (!hitboxCollider.isTrigger)
        {
            Debug.LogWarning(gameObject.name + " ma SwordHitbox collider, ale neni nastaveny jako Trigger.");
        }
    }
}
