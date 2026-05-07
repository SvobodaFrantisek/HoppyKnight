using UnityEngine;

public class EnemyMeleeHitbox : MonoBehaviour
{
    // --------------------
    // Napojeni na melee utok
    // --------------------
    public EnemyMeleeAttack meleeAttack;

    // --------------------
    // Stav behem hry
    // --------------------
    private PlayerHitbox currentPlayerHitbox;

    // --------------------
    // Unity zivotni cyklus
    // --------------------
    void Awake()
    {
        CacheMeleeAttack();
        ValidateHitboxSettings();
    }

    void OnEnable()
    {
        CacheMeleeAttack();
        ValidateHitboxSettings();
        currentPlayerHitbox = null;
    }

    // --------------------
    // Trigger zbrane enemaka
    // --------------------
    void Update()
    {
        TryHitCurrentPlayer();
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerHitbox playerHitbox = GetPlayerHitbox(other);

        if (playerHitbox == null)
        {
            return;
        }

        currentPlayerHitbox = playerHitbox;
    }

    void OnTriggerExit(Collider other)
    {
        PlayerHitbox playerHitbox = GetPlayerHitbox(other);

        if (playerHitbox == null)
        {
            return;
        }

        if (playerHitbox == currentPlayerHitbox)
        {
            currentPlayerHitbox = null;
        }
    }

    // --------------------
    // Zapocitani hitu
    // --------------------
    void TryHitCurrentPlayer()
    {
        if (meleeAttack == null)
        {
            return;
        }

        if (currentPlayerHitbox == null)
        {
            return;
        }

        if (!meleeAttack.CanHitNow())
        {
            return;
        }

        meleeAttack.RegisterHit(currentPlayerHitbox);
    }

    // --------------------
    // Hledani PlayerHitboxu
    // --------------------
    PlayerHitbox GetPlayerHitbox(Collider other)
    {
        if (!IsPlayerHitbox(other))
        {
            return null;
        }

        return other.GetComponentInParent<PlayerHitbox>();
    }

    bool IsPlayerHitbox(Collider other)
    {
        if (other == null)
        {
            return false;
        }

        return other.gameObject.layer == LayerMask.NameToLayer("PlayerHitbox");
    }

    // --------------------
    // Reference a validace
    // --------------------
    void CacheMeleeAttack()
    {
        if (meleeAttack == null)
        {
            meleeAttack = GetComponentInParent<EnemyMeleeAttack>();
        }
    }

    void ValidateHitboxSettings()
    {
        Collider hitboxCollider = GetComponent<Collider>();

        if (hitboxCollider == null)
        {
            Debug.LogWarning(gameObject.name + " nema Collider pro EnemyMeleeHitbox.");
            return;
        }

        if (!hitboxCollider.isTrigger)
        {
            Debug.LogWarning(gameObject.name + " ma EnemyMeleeHitbox collider, ale neni nastaveny jako Trigger.");
        }
    }
}
