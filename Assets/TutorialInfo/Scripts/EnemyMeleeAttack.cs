using UnityEngine;
using UnityEngine.UI;

public class EnemyMeleeAttack : MonoBehaviour
{
    [Header("Target")]
    public Transform player;
    public float attackRange = 2f;

    [Header("Attack")]
    public int damage = 15;
    public float attackCooldown = 2f;
    public float attackWindow = 0.35f;
    public string attackTriggerName = "Attack";

    [Header("UI")]
    public Slider attackProgressSlider;

    [Header("Animation")]
    public Animator animator;

    private float cooldownTimer;
    private float attackWindowTimer;
    private bool attackActive;
    private bool hasHitThisAttack;

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
        UpdateAttackWindow();
        TryStartAttack();
        RefreshAttackSlider();
    }

    public bool CanHitNow()
    {
        if (!attackActive)
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
        Debug.Log(gameObject.name + " hit player.");
    }

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

    void UpdateAttackWindow()
    {
        if (!attackActive)
        {
            return;
        }

        attackWindowTimer -= Time.deltaTime;

        if (attackWindowTimer <= 0f)
        {
            attackActive = false;
        }
    }

    void TryStartAttack()
    {
        if (attackActive)
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
        attackWindowTimer = attackWindow;
        attackActive = true;
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
