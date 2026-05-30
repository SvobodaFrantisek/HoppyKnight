using UnityEngine;

public class Acid : MonoBehaviour
{
    // --------------------
    // Damage pri stani v kyseline
    // --------------------
    [Header("Acid Damage")]
    public int acidDamagePerTick = 10;
    public float damageInterval = 0.5f;
    public bool damageImmediatelyOnEnter = true;

    private PlayerHitbox currentPlayerHitbox;
    private float nextDamageTime;

    // --------------------
    // Unity zivotni cyklus
    // --------------------
    void Awake()
    {
        ValidateAcidSettings();
    }

    void OnValidate()
    {
        if (acidDamagePerTick < 0)
        {
            acidDamagePerTick = 0;
        }

        if (damageInterval < 0.05f)
        {
            damageInterval = 0.05f;
        }
    }

    // --------------------
    // Trigger kyseliny
    // --------------------
    void OnTriggerEnter(Collider other)
    {
        PlayerHitbox playerHitbox = GetPlayerHitbox(other);

        if (playerHitbox == null)
        {
            return;
        }

        currentPlayerHitbox = playerHitbox;

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
        if (acidDamagePerTick <= 0)
        {
            return;
        }

        playerHitbox.TakeDamage(acidDamagePerTick);
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
    void ValidateAcidSettings()
    {
        Collider[] acidColliders = GetComponents<Collider>();

        if (acidColliders.Length == 0)
        {
            Debug.LogWarning(gameObject.name + " nema Collider pro Acid script.");
            return;
        }

        int i = 0;

        while (i < acidColliders.Length)
        {
            if (acidColliders[i].isTrigger)
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

        if (acidColliders.Length == 1)
        {
            Debug.LogWarning(gameObject.name + " ma Acid collider, ale neni nastaveny jako Trigger.");
            return;
        }

        Debug.LogWarning(gameObject.name + " ma Acid collidery, ale zadny z nich neni nastaveny jako Trigger.");
    }
}
