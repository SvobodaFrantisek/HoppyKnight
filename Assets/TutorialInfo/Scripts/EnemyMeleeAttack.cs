using UnityEngine;
using UnityEngine.UI;

public class EnemyMeleeAttack : MonoBehaviour
{
    // --------------------
    // Cil a dosah
    // --------------------
    [Header("Target")]
    public Transform player;
    public float attackRange = 2f;

    // --------------------
    // Nastaveni utoku
    // --------------------
    [Header("Attack")]
    public int damage = 15;
    public float attackCooldown = 2f;
    public string attackTriggerName = "Attack";

    [Header("UI")]
    public Slider attackProgressSlider;

    [Header("Animation")]
    public Animator animator;

    // --------------------
    // Stav behem hry
    // --------------------
    private float cooldownTimer;
    private bool attackInProgress;
    private bool hitWindowActive;
    private bool hasHitThisAttack;

    // --------------------
    // Unity zivotni cyklus
    // --------------------
    void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    void Start()
    {
        FindPlayerIfNeeded();
        cooldownTimer = attackCooldown;
        RefreshAttackSlider();
    }

    void Update()
    {
        FindPlayerIfNeeded();
        UpdateCooldown();
        TryStartAttack();
        RefreshAttackSlider();
    }

    // --------------------
    // Animation Events
    // --------------------
    public void StartHitWindow()
    {
        if (!attackInProgress)
        {
            return;
        }

        hitWindowActive = true;
        hasHitThisAttack = false;
    }

    public void EndHitWindow()
    {
        hitWindowActive = false;
    }

    public void EndAttack()
    {
        attackInProgress = false;
        hitWindowActive = false;
        hasHitThisAttack = false;
    }

    // --------------------
    // API pro hitbox utoku
    // --------------------
    public bool CanHitNow()
    {
        if (!attackInProgress)
        {
            return false;
        }

        if (!hitWindowActive)
        {
            return false;
        }

        if (hasHitThisAttack)
        {
            return false;
        }

        return true;
    }

    public void RegisterHit(PlayerHitbox playerHitbox)
    {
        if (!CanHitNow())
        {
            return;
        }

        if (playerHitbox == null)
        {
            return;
        }

        playerHitbox.TakeDamage(damage);
        hasHitThisAttack = true;
    }

    // --------------------
    // Cooldown
    // --------------------
    void UpdateCooldown()
    {
        if (cooldownTimer <= 0f)
        {
            return;
        }

        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer < 0f)
        {
            cooldownTimer = 0f;
        }
    }

    // --------------------
    // Spusteni utoku
    // --------------------
    void TryStartAttack()
    {
        if (attackInProgress)
        {
            return;
        }

        if (cooldownTimer > 0f)
        {
            return;
        }

        if (!IsPlayerInAttackRange())
        {
            return;
        }

        StartAttack();
    }

    void StartAttack()
    {
        cooldownTimer = attackCooldown;
        attackInProgress = true;
        hitWindowActive = false;
        hasHitThisAttack = false;

        if (animator != null)
        {
            animator.ResetTrigger(attackTriggerName);
            animator.SetTrigger(attackTriggerName);
        }
    }

    bool IsPlayerInAttackRange()
    {
        if (player == null)
        {
            return false;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        return distanceToPlayer <= attackRange;
    }

    // --------------------
    // UI cooldownu
    // --------------------
    void RefreshAttackSlider()
    {
        if (attackProgressSlider == null)
        {
            return;
        }

        attackProgressSlider.maxValue = 1f;
        attackProgressSlider.value = GetAttackProgress();
    }

    float GetAttackProgress()
    {
        if (attackCooldown <= 0f)
        {
            return 1f;
        }

        float progress = 1f - cooldownTimer / attackCooldown;

        if (progress < 0f)
        {
            progress = 0f;
        }

        if (progress > 1f)
        {
            progress = 1f;
        }

        return progress;
    }

    // --------------------
    // Hledani hrace
    // --------------------
    void FindPlayerIfNeeded()
    {
        if (player != null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }
}
