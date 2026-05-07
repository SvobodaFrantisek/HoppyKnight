using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyThrowerMovement : MonoBehaviour
{
    // --------------------
    // Cil
    // --------------------
    [Header("Target")]
    public Transform player;

    // --------------------
    // Patrolovani
    // --------------------
    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float patrolPointReachDistance = 0.5f;
    public float waitAtPatrolPoint = 1f;

    // --------------------
    // Detekce hrace
    // --------------------
    [Header("Detection")]
    public float detectionRange = 10f;
    public float losePlayerRange = 14f;

    // --------------------
    // Pohyb v boji
    // --------------------
    [Header("Combat Movement")]
    public float preferredDistance = 6f;
    public float distanceTolerance = 1f;
    public float retreatDistance = 3f;
    public float rotationSpeed = 8f;

    [Header("Animation")]
    public Animator animator;
    public string speedParameterName = "Speed";
    public string walkingForwardParameterName = "IsWalkingForward";
    public string walkingBackParameterName = "IsWalkingBack";

    // --------------------
    // Stav behem hry
    // --------------------
    private NavMeshAgent agent;
    private int currentPatrolIndex;
    private float waitTimer;
    private bool fightingPlayer;
    private bool walkingForward;
    private bool walkingBack;

    // --------------------
    // Unity zivotni cyklus
    // --------------------
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    void Start()
    {
        FindPlayerIfNeeded();
        agent.stoppingDistance = 0f;
        GoToCurrentPatrolPoint();
    }

    void Update()
    {
        UpdateState();

        if (fightingPlayer)
        {
            HandleCombatMovement();
        }
        else
        {
            HandlePatrol();
            RotateTowardsMovement();
        }

        UpdateAnimation();
    }

    // --------------------
    // Prepinani mezi patrolem a bojem
    // --------------------
    void UpdateState()
    {
        if (player == null)
        {
            fightingPlayer = false;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (!fightingPlayer && distanceToPlayer <= detectionRange)
        {
            fightingPlayer = true;
            return;
        }

        if (fightingPlayer && distanceToPlayer > losePlayerRange)
        {
            fightingPlayer = false;
            agent.stoppingDistance = 0f;
            GoToCurrentPatrolPoint();
        }
    }

    // --------------------
    // Patrolovani
    // --------------------
    void HandlePatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            agent.isStopped = true;
            SetMovementAnimation(false, false);
            return;
        }

        agent.stoppingDistance = 0f;

        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            agent.isStopped = true;
            SetMovementAnimation(false, false);
            return;
        }

        agent.isStopped = false;
        SetMovementAnimation(true, false);

        if (!agent.pathPending && agent.remainingDistance <= patrolPointReachDistance)
        {
            MoveToNextPatrolPoint();
        }
    }

    // --------------------
    // Pohyb okolo hrace
    // --------------------
    void HandleCombatMovement()
    {
        if (player == null)
        {
            return;
        }

        RotateTowardsPlayer();

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float minDistance = preferredDistance - distanceTolerance;
        float maxDistance = preferredDistance + distanceTolerance;

        if (distanceToPlayer > maxDistance)
        {
            MoveCloserToPlayer();
            return;
        }

        if (distanceToPlayer < minDistance)
        {
            MoveAwayFromPlayer();
            return;
        }

        agent.isStopped = true;
        SetMovementAnimation(false, false);
    }

    void MoveCloserToPlayer()
    {
        agent.isStopped = false;
        agent.stoppingDistance = preferredDistance;
        agent.SetDestination(player.position);
        SetMovementAnimation(true, false);
    }

    void MoveAwayFromPlayer()
    {
        Vector3 directionAwayFromPlayer = transform.position - player.position;
        directionAwayFromPlayer.y = 0f;

        if (directionAwayFromPlayer == Vector3.zero)
        {
            SetMovementAnimation(false, false);
            return;
        }

        Vector3 targetPosition = transform.position + directionAwayFromPlayer.normalized * retreatDistance;

        NavMeshHit navMeshHit;

        if (NavMesh.SamplePosition(targetPosition, out navMeshHit, retreatDistance, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.stoppingDistance = 0f;
            agent.SetDestination(navMeshHit.position);
            SetMovementAnimation(false, true);
            return;
        }

        SetMovementAnimation(false, false);
    }

    // --------------------
    // Patrol pointy
    // --------------------
    void MoveToNextPatrolPoint()
    {
        currentPatrolIndex++;

        if (currentPatrolIndex >= patrolPoints.Length)
        {
            currentPatrolIndex = 0;
        }

        waitTimer = waitAtPatrolPoint;
        GoToCurrentPatrolPoint();
    }

    void GoToCurrentPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            return;
        }

        Transform patrolPoint = patrolPoints[currentPatrolIndex];

        if (patrolPoint == null)
        {
            return;
        }

        agent.SetDestination(patrolPoint.position);
    }

    // --------------------
    // Otaceni modelu
    // --------------------
    void RotateTowardsPlayer()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0f;

        if (directionToPlayer == Vector3.zero)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void RotateTowardsMovement()
    {
        Vector3 movementDirection = agent.velocity;
        movementDirection.y = 0f;

        if (movementDirection == Vector3.zero)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    // --------------------
    // Reference a animace
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

    void UpdateAnimation()
    {
        if (animator == null)
        {
            return;
        }

        animator.SetFloat(speedParameterName, agent.velocity.magnitude);
        animator.SetBool(walkingForwardParameterName, walkingForward);
        animator.SetBool(walkingBackParameterName, walkingBack);
    }

    void SetMovementAnimation(bool forward, bool back)
    {
        walkingForward = forward;
        walkingBack = back;
    }
}
