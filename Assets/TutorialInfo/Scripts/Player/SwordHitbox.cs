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
    private DragonBossController currentDragonBoss;

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
        currentDragonBoss = null;
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

        if (enemyHealth != null)
        {
            isInsideEnemyHitbox = true;
            currentEnemyHealth = enemyHealth;
            currentDragonBoss = null;
            return;
        }

        DragonBossController dragonBoss = GetDragonBoss(other);

        if (dragonBoss != null)
        {
            isInsideEnemyHitbox = true;
            currentEnemyHealth = null;
            currentDragonBoss = dragonBoss;
        }
    }

    void OnTriggerExit(Collider other)
    {
        EnemyHealth enemyHealth = GetEnemyHealth(other);

        if (enemyHealth != null)
        {
            if (enemyHealth == currentEnemyHealth)
            {
                ClearCurrentTarget();
            }

            return;
        }

        DragonBossController dragonBoss = GetDragonBoss(other);

        if (dragonBoss != null)
        {
            if (dragonBoss == currentDragonBoss)
            {
                ClearCurrentTarget();
            }
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

        if (currentEnemyHealth != null)
        {
            currentEnemyHealth.TakeDamage(combatScript.attackDamage);
            combatScript.RegisterHit();
            return;
        }

        if (currentDragonBoss != null)
        {
            currentDragonBoss.TakeDamage(combatScript.attackDamage);
            combatScript.RegisterHit();
        }
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

    DragonBossController GetDragonBoss(Collider other)
    {
        if (!IsEnemyHitbox(other))
        {
            return null;
        }

        return other.GetComponentInParent<DragonBossController>();
    }

    bool IsEnemyHitbox(Collider other)
    {
        if (other == null)
        {
            return false;
        }

        return other.gameObject.layer == LayerMask.NameToLayer("EnemyHitbox");
    }

    void ClearCurrentTarget()
    {
        isInsideEnemyHitbox = false;
        currentEnemyHealth = null;
        currentDragonBoss = null;
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
