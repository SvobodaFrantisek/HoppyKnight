using UnityEngine;
using UnityEngine.UI;

public class EnemyThrowerAttack : MonoBehaviour
{
    [Header("Target")]
    public Transform player;
    public float attackRange = 12f;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 8f;
    public GameObject targetMarkerPrefab;

    [Header("Attack")]
    public int damage = 15;
    public float attackCooldown = 3f;
    public float aimDelay = 0.8f;

    [Header("UI")]
    public Slider attackProgressSlider;

    [Header("Animation")]
    public Animator animator;
    public string throwStartTriggerName = "ThrowStart";
    public string aimingParameterName = "IsAiming";
    public string throwReleaseTriggerName = "ThrowRelease";

    private float cooldownTimer;
    private float aimTimer;
    private bool preparingShot;
    private Vector3 lockedTargetPosition;
    private GameObject currentMarker;

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
        UpdateAiming();
        TryStartAiming();
        RefreshAttackSlider();
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

    void UpdateAiming()
    {
        if (!preparingShot)
        {
            return;
        }

        aimTimer -= Time.deltaTime;

        if (aimTimer <= 0f)
        {
            PlayThrowReleaseAnimation();
            ShootAtLockedPosition();
            FinishAiming();
        }
    }

    void TryStartAiming()
    {
        if (preparingShot)
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

        StartAiming();
    }

    void StartAiming()
    {
        preparingShot = true;
        aimTimer = aimDelay;
        lockedTargetPosition = player.position;

        ShowTargetMarker();
        PlayThrowStartAnimation();
        SetAimingAnimation(true);
    }

    void PlayThrowStartAnimation()
    {
        if (animator != null)
        {
            animator.ResetTrigger(throwStartTriggerName);
            animator.SetTrigger(throwStartTriggerName);
        }
    }

    void FinishAiming()
    {
        preparingShot = false;
        cooldownTimer = attackCooldown;
        SetAimingAnimation(false);
    }

    void PlayThrowReleaseAnimation()
    {
        if (animator != null)
        {
            animator.ResetTrigger(throwReleaseTriggerName);
            animator.SetTrigger(throwReleaseTriggerName);
        }
    }

    void SetAimingAnimation(bool aiming)
    {
        if (animator != null)
        {
            animator.SetBool(aimingParameterName, aiming);
        }
    }

    void ShootAtLockedPosition()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning(gameObject.name + " nema nastaveny projectile prefab.");
            return;
        }

        EnemyProjectile projectilePrefabScript = projectilePrefab.GetComponent<EnemyProjectile>();

        if (projectilePrefabScript == null)
        {
            Debug.LogWarning(projectilePrefab.name + " nema EnemyProjectile script.");
            return;
        }

        Vector3 spawnPosition = transform.position + Vector3.up;

        if (firePoint != null)
        {
            spawnPosition = firePoint.position;
        }

        Quaternion spawnRotation = Quaternion.identity;
        Vector3 directionToTarget = lockedTargetPosition - spawnPosition;

        if (directionToTarget != Vector3.zero)
        {
            spawnRotation = Quaternion.LookRotation(directionToTarget);
        }

        GameObject projectileObject = Instantiate(projectilePrefab, spawnPosition, spawnRotation);
        EnemyProjectile projectile = projectileObject.GetComponent<EnemyProjectile>();
        projectile.Setup(lockedTargetPosition, damage, projectileSpeed);
        projectile.SetTargetMarker(currentMarker);
        currentMarker = null;
    }

    void ShowTargetMarker()
    {
        if (targetMarkerPrefab == null)
        {
            return;
        }

        if (currentMarker != null)
        {
            Destroy(currentMarker);
            currentMarker = null;
        }

        currentMarker = Instantiate(targetMarkerPrefab, player.position, Quaternion.identity);
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
