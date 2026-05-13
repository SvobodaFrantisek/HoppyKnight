using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class DragonBossController : MonoBehaviour
{
    private enum BossState
    {
        Sleeping,
        WakingUp,
        Chasing,
        Attacking,
        Dead
    }

    // --------------------
    // Reference
    // --------------------
    [Header("References")]
    public Transform player;
    public Animator animator;
    public NavMeshAgent navMeshAgent;
    public Slider healthSlider;
    public GameObject objectToDisableOnDeath;

    // --------------------
    // Zdravi
    // --------------------
    [Header("Health")]
    public int maxHealth = 500;
    public int health = 500;
    public float disableAfterDeathDelay = 3f;

    // --------------------
    // Detekce a pohyb
    // --------------------
    [Header("Detection")]
    public float wakeRange = 12f;
    public float attackRange = 4f;

    [Header("Movement")]
    public float moveSpeed = 3.5f;
    public float stoppingDistance = 3f;
    public float rotationSpeed = 6f;

    // --------------------
    // Utok
    // --------------------
    [Header("Attack")]
    public int basicAttackDamage = 25;
    public int heavyAttackDamage = 35;
    public int projectileAttackDamage = 20;
    public float attackCooldown = 2.5f;
    public bool useHeavyAttack = false;
    public bool useProjectileAttack = false;
    public float projectileAttackRange = 12f;

    [Header("Projectile Attack")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 8f;
    public float projectileTargetHeight = 1f;

    // --------------------
    // Parametry v Animatoru
    // --------------------
    [Header("Animator Parameters")]
    public string speedParameterName = "Speed";
    public string wakeUpTriggerName = "WakeUp";
    public string attackTriggerName = "Attack";
    public string heavyAttackTriggerName = "HeavyAttack";
    public string projectileAttackTriggerName = "ProjectileAttack";
    public string getHitTriggerName = "GetHit";
    public string dieTriggerName = "Die";

    // --------------------
    // Stav behem hry
    // --------------------
    private BossState currentState = BossState.Sleeping;
    private float cooldownTimer;
    private int meleeAttackCount;
    private int currentAttackDamage;
    private Vector3 lockedProjectileTargetPosition;
    private bool hitWindowActive;
    private bool hasHitThisAttack;

    // --------------------
    // Unity zivotni cyklus
    // --------------------
    void Awake()
    {
        CacheReferences();
    }

    void Start()
    {
        health = maxHealth;
        cooldownTimer = attackCooldown;

        SetupNavMeshAgent();
        RefreshHealthSlider();
        SetAnimatorSpeed(0f);
    }

    void Update()
    {
        FindPlayerIfNeeded();
        UpdateCooldown();

        if (currentState == BossState.Dead)
        {
            return;
        }

        if (currentState == BossState.Sleeping)
        {
            HandleSleeping();
            return;
        }

        if (currentState == BossState.WakingUp)
        {
            return;
        }

        if (currentState == BossState.Attacking)
        {
            return;
        }

        HandleChaseAndAttack();
    }

    // --------------------
    // Spici stav a probuzeni
    // --------------------
    void HandleSleeping()
    {
        StopMovement();
        SetAnimatorSpeed(0f);

        if (player == null)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= wakeRange)
        {
            WakeUp();
        }
    }

    void WakeUp()
    {
        currentState = BossState.WakingUp;
        StopMovement();
        SetAnimatorSpeed(0f);
        FacePlayer();
        SetAnimatorTrigger(wakeUpTriggerName);
    }

    public void FinishWakeUp()
    {
        if (currentState != BossState.WakingUp)
        {
            return;
        }

        currentState = BossState.Chasing;
        SetAnimatorSpeed(0f);
    }

    // --------------------
    // Chase a vyber utoku
    // --------------------
    void HandleChaseAndAttack()
    {
        if (player == null)
        {
            StopMovement();
            SetAnimatorSpeed(0f);
            return;
        }

        FacePlayer();

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            StopMovement();
            SetAnimatorSpeed(0f);

            if (cooldownTimer <= 0f)
            {
                StartCloseAttack();
                return;
            }

            return;
        }

        if (CanStartProjectileAttack(distanceToPlayer))
        {
            StopMovement();
            SetAnimatorSpeed(0f);

            if (cooldownTimer <= 0f)
            {
                StartProjectileAttack();
                return;
            }

            return;
        }

        MoveToPlayer();
    }

    void MoveToPlayer()
    {
        if (navMeshAgent == null)
        {
            SetAnimatorSpeed(moveSpeed);
            return;
        }

        navMeshAgent.isStopped = false;
        navMeshAgent.stoppingDistance = stoppingDistance;
        navMeshAgent.SetDestination(player.position);
        SetAnimatorSpeed(moveSpeed);
    }

    void StopMovement()
    {
        if (navMeshAgent == null)
        {
            return;
        }

        if (navMeshAgent.enabled)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();
        }
    }

    void FacePlayer()
    {
        if (player == null)
        {
            return;
        }

        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0f;

        if (directionToPlayer == Vector3.zero)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    // --------------------
    // Utok a Animation Eventy
    // --------------------
    void StartCloseAttack()
    {
        if (!useHeavyAttack)
        {
            StartBasicAttack();
            return;
        }

        meleeAttackCount++;

        if (meleeAttackCount >= 3)
        {
            meleeAttackCount = 0;
            StartHeavyAttack();
            return;
        }

        StartBasicAttack();
    }

    void StartBasicAttack()
    {
        StartMeleeAttack(basicAttackDamage, attackTriggerName);
    }

    void StartHeavyAttack()
    {
        StartMeleeAttack(heavyAttackDamage, heavyAttackTriggerName);
    }

    void StartMeleeAttack(int damage, string triggerName)
    {
        currentState = BossState.Attacking;
        cooldownTimer = attackCooldown;
        currentAttackDamage = damage;
        hitWindowActive = false;
        hasHitThisAttack = false;

        StopMovement();
        SetAnimatorSpeed(0f);
        FacePlayer();
        SetAnimatorTrigger(triggerName);
    }

    void StartProjectileAttack()
    {
        currentState = BossState.Attacking;
        cooldownTimer = attackCooldown;
        currentAttackDamage = projectileAttackDamage;
        hitWindowActive = false;
        hasHitThisAttack = false;

        if (player != null)
        {
            lockedProjectileTargetPosition = player.position + Vector3.up * projectileTargetHeight;
        }
        else
        {
            lockedProjectileTargetPosition = transform.position + transform.forward * projectileAttackRange;
        }

        StopMovement();
        SetAnimatorSpeed(0f);
        FacePlayer();
        SetAnimatorTrigger(projectileAttackTriggerName);
    }

    public void StartHitWindow()
    {
        if (currentState != BossState.Attacking)
        {
            return;
        }

        hitWindowActive = true;
        currentAttackDamage = basicAttackDamage;
        hasHitThisAttack = false;
    }

    public void EndHitWindow()
    {
        hitWindowActive = false;
    }

    public void StartHeavyHitWindow()
    {
        if (currentState != BossState.Attacking)
        {
            return;
        }

        currentAttackDamage = heavyAttackDamage;
        hitWindowActive = true;
        hasHitThisAttack = false;
    }

    public void EndHeavyHitWindow()
    {
        hitWindowActive = false;
    }

    public void EndAttack()
    {
        if (currentState == BossState.Dead)
        {
            return;
        }

        hitWindowActive = false;
        hasHitThisAttack = false;
        currentState = BossState.Chasing;
        SetAnimatorSpeed(0f);
    }

    public void ShootProjectile()
    {
        if (currentState != BossState.Attacking)
        {
            return;
        }

        if (projectilePrefab == null)
        {
            Debug.LogWarning(gameObject.name + " nema nastaveny projectilePrefab.");
            return;
        }

        EnemyProjectile projectilePrefabScript = projectilePrefab.GetComponent<EnemyProjectile>();

        if (projectilePrefabScript == null)
        {
            Debug.LogWarning(projectilePrefab.name + " nema EnemyProjectile script.");
            return;
        }

        Vector3 spawnPosition = transform.position + Vector3.up * 2f;

        if (firePoint != null)
        {
            spawnPosition = firePoint.position;
        }

        Quaternion spawnRotation = Quaternion.identity;
        Vector3 directionToTarget = lockedProjectileTargetPosition - spawnPosition;

        if (directionToTarget != Vector3.zero)
        {
            spawnRotation = Quaternion.LookRotation(directionToTarget);
        }

        GameObject projectileObject = Instantiate(projectilePrefab, spawnPosition, spawnRotation);
        EnemyProjectile projectile = projectileObject.GetComponent<EnemyProjectile>();

        if (projectile != null)
        {
            projectile.Setup(lockedProjectileTargetPosition, projectileAttackDamage, projectileSpeed);
        }
    }

    public void TryHitPlayer(PlayerHitbox playerHitbox)
    {
        if (!CanHitNow())
        {
            return;
        }

        if (playerHitbox == null)
        {
            return;
        }

        playerHitbox.TakeDamage(currentAttackDamage);
        hasHitThisAttack = true;
    }

    bool CanHitNow()
    {
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

    bool CanStartProjectileAttack(float distanceToPlayer)
    {
        if (!useProjectileAttack)
        {
            return false;
        }

        if (distanceToPlayer <= attackRange)
        {
            return false;
        }

        if (distanceToPlayer > projectileAttackRange)
        {
            return false;
        }

        return true;
    }

    // --------------------
    // Prijimani damage
    // --------------------
    public void TakeDamage(int damageAmount)
    {
        if (currentState == BossState.Dead)
        {
            return;
        }

        if (damageAmount <= 0)
        {
            return;
        }

        if (currentState == BossState.Sleeping)
        {
            WakeUp();
        }

        health -= damageAmount;

        if (health < 0)
        {
            health = 0;
        }

        RefreshHealthSlider();

        if (health <= 0)
        {
            Die();
            return;
        }

        if (currentState != BossState.Attacking && currentState != BossState.WakingUp)
        {
            SetAnimatorTrigger(getHitTriggerName);
        }
    }

    void Die()
    {
        currentState = BossState.Dead;
        hitWindowActive = false;
        hasHitThisAttack = false;

        StopMovement();
        SetAnimatorSpeed(0f);
        DisableBossHitboxes();
        SetAnimatorTrigger(dieTriggerName);

        StartCoroutine(DisableAfterDeath());
    }

    IEnumerator DisableAfterDeath()
    {
        yield return new WaitForSeconds(disableAfterDeathDelay);

        if (objectToDisableOnDeath != null)
        {
            objectToDisableOnDeath.SetActive(false);
        }
    }

    void DisableBossHitboxes()
    {
        DragonBossHitbox[] bossHitboxes = GetComponentsInChildren<DragonBossHitbox>();

        int i = 0;

        while (i < bossHitboxes.Length)
        {
            bossHitboxes[i].enabled = false;
            i++;
        }
    }

    // --------------------
    // Pomocne metody
    // --------------------
    void CacheReferences()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (navMeshAgent == null)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }
    }

    void SetupNavMeshAgent()
    {
        if (navMeshAgent == null)
        {
            return;
        }

        navMeshAgent.speed = moveSpeed;
        navMeshAgent.stoppingDistance = stoppingDistance;
        navMeshAgent.updateRotation = false;
        navMeshAgent.isStopped = true;
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

    void RefreshHealthSlider()
    {
        if (healthSlider == null)
        {
            return;
        }

        healthSlider.maxValue = maxHealth;
        healthSlider.value = health;
    }

    void SetAnimatorSpeed(float speedValue)
    {
        if (animator == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(speedParameterName))
        {
            return;
        }

        animator.SetFloat(speedParameterName, speedValue);
    }

    void SetAnimatorTrigger(string triggerName)
    {
        if (animator == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(triggerName))
        {
            return;
        }

        animator.ResetTrigger(triggerName);
        animator.SetTrigger(triggerName);
    }
}
