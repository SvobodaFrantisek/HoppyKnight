using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public int damage = 15;
    public float speed = 8f;
    public float lifeTime = 5f;
    public float hitDistance = 0.2f;

    private Vector3 targetPosition;
    private GameObject targetMarker;
    private bool hasTarget;
    private bool hasHit;

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
        playerHitbox.TakeDamage(damage);
        DestroyProjectile();
    }

    public void Setup(Vector3 newTargetPosition, int newDamage, float newSpeed)
    {
        targetPosition = newTargetPosition;
        damage = newDamage;
        speed = newSpeed;
        hasTarget = true;
        hasHit = false;
    }

    public void SetTargetMarker(GameObject newTargetMarker)
    {
        targetMarker = newTargetMarker;
    }

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
