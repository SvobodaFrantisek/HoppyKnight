using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMeleeMovement : MonoBehaviour
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
    public float detectionRange = 8f;
    public float losePlayerRange = 12f;

    // --------------------
    // Pohyb v boji
    // --------------------
    [Header("Combat Movement")]
    public float stopDistance = 1.5f;

    [Header("Animation")]
    public Animator animator;
    public string speedParameterName = "Speed";

    // --------------------
    // Stav behem hry
    // --------------------
    private NavMeshAgent agent;
    private int currentPatrolIndex;
    private float waitTimer;
    private bool chasingPlayer;

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
        agent.stoppingDistance = 0f;
        GoToCurrentPatrolPoint();
    }

    void Update()
    {
        UpdateState();

        if (chasingPlayer)
        {
            HandleChase();
        }
        else
        {
            HandlePatrol();
        }

        UpdateAnimation();
    }

    // --------------------
    // Prepinani patrol / chase
    // --------------------
    void UpdateState()
    {
        if (player == null)
        {
            chasingPlayer = false;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (!chasingPlayer && distanceToPlayer <= detectionRange)
        {
            chasingPlayer = true;
            return;
        }

        if (chasingPlayer && distanceToPlayer > losePlayerRange)
        {
            chasingPlayer = false;
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
            return;
        }

        agent.stoppingDistance = 0f;

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

    // --------------------
    // Chase hrace
    // --------------------
    void HandleChase()
    {
        if (player == null)
        {
            return;
        }

        agent.isStopped = false;
        agent.stoppingDistance = stopDistance;
        agent.SetDestination(player.position);
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
    }
}
