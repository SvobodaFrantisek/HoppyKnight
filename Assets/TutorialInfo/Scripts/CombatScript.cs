using UnityEngine;
using UnityEngine.InputSystem;

public class CombatScript : MonoBehaviour
{
    [Header("Attack")]
    public float attackCooldown = 0.8f;
    public int attackDamage = 20;
    public Animator animator;

    [Header("Visual")]
    public GameObject traileffect;

    private PlayerInput playerInput;
    private InputAction attackAction;
    private float lastAttackTime;
    private bool combatBlocked;
    private bool attackActive;
    private bool hasRegisteredHitThisAttack;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        SetTrailActive(false);
    }

    void OnEnable()
    {
        attackAction = playerInput.actions.FindAction("Attack");

        if (attackAction != null)
        {
            attackAction.Enable();
        }
    }

    void OnDisable()
    {
        if (attackAction != null)
        {
            attackAction.Disable();
        }

        EndAttackAnimation();
    }

    void Update()
    {
        if (!CanReadAttackInput())
        {
            return;
        }

        if (!CanAttack())
        {
            return;
        }

        if (attackActive)
        {
            return;
        }

        Attack();
    }

    public void StartAttackAnimation()
    {
        if (combatBlocked)
        {
            return;
        }

        attackActive = true;
        hasRegisteredHitThisAttack = false;
        SetTrailActive(true);
    }

    public void EndAttackAnimation()
    {
        attackActive = false;
        hasRegisteredHitThisAttack = false;
        SetTrailActive(false);
    }

    public void SetCombatBlocked(bool blocked)
    {
        combatBlocked = blocked;

        if (combatBlocked)
        {
            attackActive = false;
            hasRegisteredHitThisAttack = false;
            SetTrailActive(false);
        }
    }

    public bool CanHitNow()
    {
        if (combatBlocked)
        {
            return false;
        }

        if (!attackActive)
        {
            return false;
        }

        if (hasRegisteredHitThisAttack)
        {
            return false;
        }

        return true;
    }

    public void RegisterHit()
    {
        hasRegisteredHitThisAttack = true;
        Debug.Log("hit");
    }

    bool CanReadAttackInput()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            return false;
        }

        if (combatBlocked)
        {
            return false;
        }

        if (attackAction == null)
        {
            return false;
        }

        return attackAction.triggered;
    }

    bool CanAttack()
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        hasRegisteredHitThisAttack = false;
        SetTrailActive(false);

        if (animator != null)
        {
            animator.ResetTrigger("Attack");
            animator.SetTrigger("Attack");
        }
    }

    void SetTrailActive(bool activeState)
    {
        if (traileffect != null)
        {
            traileffect.SetActive(activeState);
        }
    }
}
