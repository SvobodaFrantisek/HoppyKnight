using UnityEngine;

public class Lava : MonoBehaviour
{
    // --------------------
    // Burn efekt
    // --------------------
    [Header("Burn")]
    public bool startBurnOnEnter = true;

    // --------------------
    // Damage pri stani v lave
    // --------------------
    [Header("Lava Damage")]
    public int lavaDamagePerTick = 20;
    public float damageInterval = 0.5f;
    public bool damageImmediatelyOnEnter = true;

    private PlayerHitbox currentPlayerHitbox;
    private float nextDamageTime;

    // --------------------
    // Unity zivotni cyklus
    // --------------------
    void Awake()
    {
        ValidateLavaSettings();
    }

    void OnValidate()
    {
        if (lavaDamagePerTick < 0)
        {
            lavaDamagePerTick = 0;
        }

        if (damageInterval < 0.05f)
        {
            damageInterval = 0.05f;
        }
    }

    // --------------------
    // Trigger lavy
    // --------------------
    void OnTriggerEnter(Collider other)
    {
        PlayerHitbox playerHitbox = GetPlayerHitbox(other);

        if (playerHitbox == null)
        {
            return;
        }

        currentPlayerHitbox = playerHitbox;

        if (startBurnOnEnter)
        {
            playerHitbox.StartBurn();
        }

        if (damageImmediatelyOnEnter)
        {
            DamagePlayer(playerHitbox);
            return;
        }

        nextDamageTime = Time.time + damageInterval;
    }

    void OnTriggerStay(Collider other)
    {
        PlayerHitbox playerHitbox = GetPlayerHitbox(other);

        if (playerHitbox == null)
        {
            return;
        }

        currentPlayerHitbox = playerHitbox;

        if (Time.time < nextDamageTime)
        {
            return;
        }

        DamagePlayer(playerHitbox);
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
    // Damage
    // --------------------
    void DamagePlayer(PlayerHitbox playerHitbox)
    {
        if (lavaDamagePerTick <= 0)
        {
            return;
        }

        playerHitbox.TakeDamage(lavaDamagePerTick);
        nextDamageTime = Time.time + damageInterval;
    }

    // --------------------
    // Hledani hrace
    // --------------------
    PlayerHitbox GetPlayerHitbox(Collider other)
    {
        if (other == null)
        {
            return null;
        }

        return other.GetComponentInParent<PlayerHitbox>();
    }

    // --------------------
    // Kontrola nastaveni
    // --------------------
    void ValidateLavaSettings()
    {
        Collider[] lavaColliders = GetComponents<Collider>();

        if (lavaColliders.Length == 0)
        {
            Debug.LogWarning(gameObject.name + " nema Collider pro Lava script.");
            return;
        }

        int i = 0;

        while (i < lavaColliders.Length)
        {
            if (lavaColliders[i].isTrigger)
            {
                return;
            }

            i++;
        }

        if (transform.childCount > 0)
        {
            Collider[] childColliders = GetComponentsInChildren<Collider>();
            i = 0;

            while (i < childColliders.Length)
            {
                if (childColliders[i].isTrigger)
                {
                    return;
                }

                i++;
            }
        }

        if (lavaColliders.Length == 1)
        {
            Debug.LogWarning(gameObject.name + " ma Lava collider, ale neni nastaveny jako Trigger.");
            return;
        }

        Debug.LogWarning(gameObject.name + " ma Lava collidery, ale zadny z nich neni nastaveny jako Trigger.");
    }
}
