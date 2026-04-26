using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SwordHitbox : MonoBehaviour
{
    public CombatScript combatScript;

    private bool isInsideEnemyHitbox;

    void Awake()
    {
        CacheCombatScript();
        PrepareHitbox();
    }

    void OnEnable()
    {
        CacheCombatScript();
        PrepareHitbox();
        isInsideEnemyHitbox = false;
    }

    void Update()
    {
        TryRegisterCurrentHit();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsEnemyHitbox(other))
        {
            return;
        }

        isInsideEnemyHitbox = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsEnemyHitbox(other))
        {
            return;
        }

        isInsideEnemyHitbox = false;
    }

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

        combatScript.RegisterHit();
    }

    bool IsEnemyHitbox(Collider other)
    {
        if (other == null)
        {
            return false;
        }

        return other.gameObject.layer == LayerMask.NameToLayer("EnemyHitbox");
    }

    void CacheCombatScript()
    {
        if (combatScript == null)
        {
            combatScript = GetComponentInParent<CombatScript>();
        }
    }

    void PrepareHitbox()
    {
        Collider hitboxCollider = GetComponent<Collider>();

        if (hitboxCollider != null)
        {
            hitboxCollider.isTrigger = true;
        }

        Rigidbody hitboxBody = GetComponent<Rigidbody>();

        if (hitboxBody == null)
        {
            hitboxBody = gameObject.AddComponent<Rigidbody>();
        }

        hitboxBody.isKinematic = true;
        hitboxBody.useGravity = false;
    }
}
