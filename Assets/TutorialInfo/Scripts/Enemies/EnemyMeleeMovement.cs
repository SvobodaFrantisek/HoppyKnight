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
    public float patrolPointReachDistance = 0.5f;
    public float waitAtPatrolPoint = 1f;
    public float randomPatrolRadius = 8f;
    public int randomPatrolSearchAttempts = 10;
    public float navMeshSnapDistance = 3f;

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
    private float waitTimer;
    private bool chasingPlayer;
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
        agent.stoppingDistance = 0f;

        if (!EnsureAgentIsOnNavMesh())
        {
            return;
        }

        GoToRandomPatrolPoint();
    }

    void Update()
    {
        if (!EnsureAgentIsOnNavMesh())
        {
            return;
        }

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
            GoToRandomPatrolPoint();
        }
    }

    // --------------------
    // Patrolovani
    // --------------------
    void HandlePatrol()
    {
        agent.stoppingDistance = 0f;

        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            agent.isStopped = true;
            return;
        }

        agent.isStopped = false;

        if (!agent.hasPath || (!agent.pathPending && agent.remainingDistance <= patrolPointReachDistance))
        {
            waitTimer = waitAtPatrolPoint;
            GoToRandomPatrolPoint();
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

    bool EnsureAgentIsOnNavMesh()
    {
        if (agent == null || !agent.enabled)
        {
            return false;
        }

        if (agent.isOnNavMesh)
        {
            return true;
        }

        NavMeshHit navMeshHit;

        if (NavMesh.SamplePosition(transform.position, out navMeshHit, navMeshSnapDistance, NavMesh.AllAreas))
        {
            agent.Warp(navMeshHit.position);
            randomPatrolCenter = navMeshHit.position;
            return agent.isOnNavMesh;
        }

        return false;
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
        if (animator == null || agent == null || !agent.enabled || !agent.isOnNavMesh)
        {
            return;
        }

        animator.SetFloat(speedParameterName, agent.velocity.magnitude);
    }
}
