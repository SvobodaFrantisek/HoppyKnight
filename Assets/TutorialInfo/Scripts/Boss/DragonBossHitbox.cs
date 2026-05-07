using UnityEngine;

public class DragonBossHitbox : MonoBehaviour
{
    // --------------------
    // Napojeni na bosse
    // --------------------
    public DragonBossController bossController;

    // --------------------
    // Stav behem hry
    // --------------------
    private PlayerHitbox currentPlayerHitbox;

    // --------------------
    // Unity zivotni cyklus
    // --------------------
    void Awake()
    {
        CacheBossController();
        ValidateHitboxSettings();
    }

    void OnEnable()
    {
        CacheBossController();
        ValidateHitboxSettings();
        currentPlayerHitbox = null;
    }

    void Update()
    {
        TryHitCurrentPlayer();
    }

    // --------------------
    // Trigger hitboxu
    // --------------------
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
        if (bossController == null)
        {
            return;
        }

        if (currentPlayerHitbox == null)
        {
            return;
        }

        bossController.TryHitPlayer(currentPlayerHitbox);
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
    void CacheBossController()
    {
        if (bossController == null)
        {
            bossController = GetComponentInParent<DragonBossController>();
        }
    }

    void ValidateHitboxSettings()
    {
        Collider hitboxCollider = GetComponent<Collider>();

        if (hitboxCollider == null)
        {
            Debug.LogWarning(gameObject.name + " nema Collider pro DragonBossHitbox.");
            return;
        }

        if (!hitboxCollider.isTrigger)
        {
            Debug.LogWarning(gameObject.name + " ma DragonBossHitbox collider, ale neni nastaveny jako Trigger.");
        }
    }
}
