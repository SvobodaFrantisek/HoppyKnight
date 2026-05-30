using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    // --------------------
    // Nastaveni projectilu
    // --------------------
    public int damage = 15;
    public float speed = 8f;
    public float lifeTime = 5f;
    public float hitDistance = 0.2f;

    [Header("Burn")]
    public bool startBurnOnHit;

    // --------------------
    // Stav behem hry
    // --------------------
    private Vector3 targetPosition;
    private GameObject targetMarker;
    private bool hasTarget;
    private bool hasHit;

    // --------------------
    // Unity zivotni cyklus
    // --------------------
    void Awake()
    {
        ValidateHitboxSettings();
    }

    void OnEnable()
    {
        ValidateHitboxSettings();
    }

    void Update()
    {
        if (!hasTarget)
        {
            return;
        }

        lifeTime -= Time.deltaTime;

        if (lifeTime <= 0f)
        {
            DestroyProjectile();
            return;
        }

        MoveToTarget();
    }

    // --------------------
    // Zasah hrace
    // --------------------
    void OnTriggerEnter(Collider other)
    {
        if (hasHit)
        {
            return;
        }

        PlayerHitbox playerHitbox = GetPlayerHitbox(other);

        if (playerHitbox == null)
        {
            return;
        }

        hasHit = true;

        bool damageApplied = playerHitbox.TakeDamage(damage);

        if (startBurnOnHit && damageApplied)
        {
            playerHitbox.StartBurn();
        }

        DestroyProjectile();
    }

    // --------------------
    // Nastaveni po spawnuti
    // --------------------
    public void Setup(Vector3 newTargetPosition, int newDamage, float newSpeed)
    {
        Setup(newTargetPosition, newDamage, newSpeed, startBurnOnHit);
    }

    public void Setup(Vector3 newTargetPosition, int newDamage, float newSpeed, bool newStartBurnOnHit)
    {
        targetPosition = newTargetPosition;
        damage = newDamage;
        speed = newSpeed;
        startBurnOnHit = newStartBurnOnHit;
        hasTarget = true;
        hasHit = false;
    }

    public void SetTargetMarker(GameObject newTargetMarker)
    {
        targetMarker = newTargetMarker;
    }

    // --------------------
    // Let na zamcenou pozici
    // --------------------
    void MoveToTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        if (distanceToTarget <= hitDistance)
        {
            DestroyProjectile();
        }
    }

    void DestroyProjectile()
    {
        if (targetMarker != null)
        {
            Destroy(targetMarker);
            targetMarker = null;
        }

        Destroy(gameObject);
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
    // Kontrola nastaveni hitboxu
    // --------------------
    void ValidateHitboxSettings()
    {
        Collider hitboxCollider = GetComponent<Collider>();

        if (hitboxCollider == null)
        {
            Debug.LogWarning(gameObject.name + " nema Collider pro EnemyProjectile.");
            return;
        }

        if (!hitboxCollider.isTrigger)
        {
            Debug.LogWarning(gameObject.name + " ma EnemyProjectile collider, ale neni nastaveny jako Trigger.");
        }
    }
}
