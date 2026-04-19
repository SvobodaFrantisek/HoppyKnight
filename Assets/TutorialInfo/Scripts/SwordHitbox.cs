using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SwordHitbox : MonoBehaviour
{
    public CombatScript combatScript;
    private bool isInsideEnemyHitbox = false;

    void Awake()
    {
        FindCombatScript();
        PrepareHitbox();
    }

    void OnEnable()
    {
        FindCombatScript();
        PrepareHitbox();
        isInsideEnemyHitbox = false;
    }

    void Update()
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

    private void OnTriggerEnter(Collider other)
    {
        if (other == null)
        {
            return;
        }

        if (other.gameObject.layer != LayerMask.NameToLayer("EnemyHitbox"))
        {
            return;
        }

        isInsideEnemyHitbox = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == null)
        {
            return;
        }

        if (other.gameObject.layer != LayerMask.NameToLayer("EnemyHitbox"))
        {
            return;
        }

        isInsideEnemyHitbox = false;
    }

    private void FindCombatScript()
    {
        if (combatScript == null)
        {
            combatScript = GetComponentInParent<CombatScript>();
        }
    }

    private void PrepareHitbox()
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
