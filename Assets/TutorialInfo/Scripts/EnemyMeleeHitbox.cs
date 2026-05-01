using UnityEngine;

public class EnemyMeleeHitbox : MonoBehaviour
{
    public EnemyMeleeAttack meleeAttack;

    private PlayerHitbox currentPlayerHitbox;

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
