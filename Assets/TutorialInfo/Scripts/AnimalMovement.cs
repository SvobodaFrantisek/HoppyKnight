using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AnimalMovement : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float patrolPointReachDistance = 0.5f;
    public float waitAtPatrolPoint = 1f;

    [Header("Fear")]
    public float fearRange = 5f;
    public float safeRange = 9f;
    public float fleeDistance = 6f;

    [Header("Animation")]
    public Animator animator;

    private NavMeshAgent agent;
    private int currentPatrolIndex;
    private float waitTimer;
    private bool fleeing;

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
        GoToCurrentPatrolPoint();
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
            GoToCurrentPatrolPoint();
        }
    }

    void HandlePatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            agent.isStopped = true;
            return;
        }

        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            agent.isStopped = true;
            return;
        }

        agent.isStopped = false;

        if (!agent.pathPending && agent.remainingDistance <= patrolPointReachDistance)
        {
            MoveToNextPatrolPoint();
        }
    }

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
            agent.isStopped = false;
            agent.SetDestination(navMeshHit.position);
        }
    }

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

        animator.SetFloat("Speed", agent.velocity.magnitude);
    }
}
