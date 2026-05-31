using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AnimalMovement : MonoBehaviour
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
    public float patrolPointReachDistance = 0.5f;
    public float waitAtPatrolPoint = 1f;
    public float randomPatrolRadius = 8f;
    public int randomPatrolSearchAttempts = 10;

    // --------------------
    // Utek od hrace
    // --------------------
    [Header("Fear")]
    public float fearRange = 5f;
    public float safeRange = 9f;
    public float fleeDistance = 6f;

    [Header("Movement Speed")]
    public float walkSpeed = 1.5f;
    public float runSpeed = 4f;

    [Header("Animation")]
    public Animator animator;
    public string speedParameterName = "Speed";
    public string idleParameterName = "IsIdle";
    public string walkingParameterName = "IsWalking";
    public string runningParameterName = "IsRunning";

    // --------------------
    // Stav behem hry
    // --------------------
    private NavMeshAgent agent;
    private float waitTimer;
    private bool fleeing;
    private Vector3 randomPatrolCenter;

    // --------------------
    // Unity zivotni cyklus
    // --------------------
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    void Start()
    {
        FindPlayerIfNeeded();
        randomPatrolCenter = transform.position;
        GoToRandomPatrolPoint();
    }

    void Update()
    {
        UpdateState();

        if (fleeing)
        {
            HandleFlee();
        }
        else
        {
            HandlePatrol();
        }

        UpdateAnimation();
    }

    // --------------------
    // Prepinani patrol / utek
    // --------------------
    void UpdateState()
    {
        if (player == null)
        {
            fleeing = false;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (!fleeing && distanceToPlayer <= fearRange)
        {
            fleeing = true;
            return;
        }

        if (fleeing && distanceToPlayer >= safeRange)
        {
            fleeing = false;
            GoToRandomPatrolPoint();
        }
    }

    // --------------------
    // Patrolovani
    // --------------------
    void HandlePatrol()
    {
        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            agent.isStopped = true;
            return;
        }

        agent.speed = walkSpeed;
        agent.isStopped = false;

        if (!agent.hasPath || (!agent.pathPending && agent.remainingDistance <= patrolPointReachDistance))
        {
            waitTimer = waitAtPatrolPoint;
            GoToRandomPatrolPoint();
        }
    }

    // --------------------
    // Utek od hrace
    // --------------------
    void HandleFlee()
    {
        if (player == null)
        {
            return;
        }

        Vector3 directionAwayFromPlayer = transform.position - player.position;
        directionAwayFromPlayer.y = 0f;

        if (directionAwayFromPlayer == Vector3.zero)
        {
            return;
        }

        Vector3 targetPosition = transform.position + directionAwayFromPlayer.normalized * fleeDistance;

        NavMeshHit navMeshHit;

        if (NavMesh.SamplePosition(targetPosition, out navMeshHit, fleeDistance, NavMesh.AllAreas))
        {
            agent.speed = runSpeed;
            agent.isStopped = false;
            agent.SetDestination(navMeshHit.position);
        }
    }

    void GoToRandomPatrolPoint()
    {
        int attempt = 0;

        while (attempt < randomPatrolSearchAttempts)
        {
            Vector2 randomCircle = Random.insideUnitCircle * randomPatrolRadius;
            Vector3 randomPosition = randomPatrolCenter + new Vector3(randomCircle.x, 0f, randomCircle.y);

            NavMeshHit navMeshHit;

            if (NavMesh.SamplePosition(randomPosition, out navMeshHit, randomPatrolRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(navMeshHit.position);
                return;
            }

            attempt++;
        }

        agent.isStopped = true;
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

        float currentSpeed = agent.velocity.magnitude;
        bool isMoving = currentSpeed > 0.1f;
        bool isRunning = fleeing && isMoving;
        bool isWalking = !fleeing && isMoving;
        bool isIdle = !isMoving;

        SetAnimatorFloat(speedParameterName, currentSpeed);
        SetAnimatorBool(idleParameterName, isIdle);
        SetAnimatorBool(walkingParameterName, isWalking);
        SetAnimatorBool(runningParameterName, isRunning);
    }

    void SetAnimatorFloat(string parameterName, float value)
    {
        if (string.IsNullOrEmpty(parameterName))
        {
            return;
        }

        animator.SetFloat(parameterName, value);
    }

    void SetAnimatorBool(string parameterName, bool value)
    {
        if (string.IsNullOrEmpty(parameterName))
        {
            return;
        }

        animator.SetBool(parameterName, value);
    }
}
