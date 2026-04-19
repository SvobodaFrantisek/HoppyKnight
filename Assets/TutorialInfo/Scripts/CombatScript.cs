using UnityEngine;
using UnityEngine.InputSystem;

public class CombatScript : MonoBehaviour
{
    [Header("Attack")]
    public float attackCooldown = 0.8f;
    public Animator animator;

    [Header("Visual")]
    public GameObject traileffect;

    [Header("Hitbox")]
    public GameObject attackHitbox;

    private PlayerInput _playerInput;
    private InputAction _attackAction;
    private float _lastAttackTime;
    private bool combatBlocked;
    private bool hitboxActive = false;
    private bool isAttackAnimationPlaying = false;
    private bool hasRegisteredHitThisAttack = false;

    void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        TrailOff();
    }

    void OnEnable()
    {
        _attackAction = _playerInput.actions.FindAction("Attack");

        if (_attackAction != null)
        {
            _attackAction.Enable();
        }
    }

    void OnDisable()
    {
        if (_attackAction != null)
        {
            _attackAction.Disable();
        }

        EndAttackAnimation();
    }

    void Update()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            return;
        }

        if (combatBlocked)
        {
            return;
        }

        if (_attackAction != null && _attackAction.triggered && CanAttack() && !isAttackAnimationPlaying)
        {
            Attack();
        }
    }

    bool CanAttack()
    {
        return Time.time >= _lastAttackTime + attackCooldown;
    }

    void Attack()
    {
        _lastAttackTime = Time.time;
        hasRegisteredHitThisAttack = false;
        TrailOff();

        if (animator != null)
        {
            animator.ResetTrigger("Attack");
            animator.SetTrigger("Attack");
        }
    }

    public void StartAttackAnimation()
    {
        if (combatBlocked)
        {
            return;
        }

        isAttackAnimationPlaying = true;
        hitboxActive = true;
        hasRegisteredHitThisAttack = false;

        if (traileffect != null)
        {
            traileffect.SetActive(true);
        }
    }

    public void EndAttackAnimation()
    {
        isAttackAnimationPlaying = false;
        hitboxActive = false;
        hasRegisteredHitThisAttack = false;

        if (traileffect != null)
        {
            traileffect.SetActive(false);
        }
    }

    public void TrailOn()
    {
        if (traileffect != null)
        {
            traileffect.SetActive(true);
        }
    }

    public void TrailOff()
    {
        if (traileffect != null)
        {
            traileffect.SetActive(false);
        }
    }

    public void HitboxOn()
    {
        if (combatBlocked)
        {
            return;
        }

        if (!isAttackAnimationPlaying)
        {
            return;
        }

        hitboxActive = true;
    }

    public void HitboxOff()
    {
        hitboxActive = false;
    }

    public void SetCombatBlocked(bool blocked)
    {
        combatBlocked = blocked;

        if (combatBlocked)
        {
            isAttackAnimationPlaying = false;
            hasRegisteredHitThisAttack = false;
            TrailOff();
        }
    }

    public bool CanHitNow()
    {
        if (combatBlocked)
        {
            return false;
        }

        if (!isAttackAnimationPlaying)
        {
            return false;
        }

        if (!hitboxActive)
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
}
