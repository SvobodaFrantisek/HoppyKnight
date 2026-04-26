using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;
    public float waitTime = 0.5f;
    public bool startFromA = true;
    public Vector3 frameDelta;

    private Vector3 pointAPosition;
    private Vector3 pointBPosition;
    private Vector3 currentTargetPosition;
    private Vector3 previousPosition;
    private bool movingToPointB;
    private float waitTimer;

    void Start()
    {
        if (!HasRequiredPoints())
        {
            Debug.LogError("MovingPlatform: chybi pointA nebo pointB.");
            enabled = false;
            return;
        }

        CachePointPositions();
        SetStartState();

        previousPosition = transform.position;
        frameDelta = Vector3.zero;
    }

    void Update()
    {
        previousPosition = transform.position;

        if (waitTimer > 0f)
        {
            UpdateWaitingState();
            return;
        }

        MoveTowardsCurrentTarget();

        if (transform.position == currentTargetPosition)
        {
            SwitchTargetAndStartWaiting();
        }

        frameDelta = transform.position - previousPosition;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        FPSController controller = other.GetComponent<FPSController>();

        if (controller != null)
        {
            controller.SetCurrentPlatform(this);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        FPSController controller = other.GetComponent<FPSController>();

        if (controller != null)
        {
            controller.ClearCurrentPlatform(this);
        }
    }

    bool HasRequiredPoints()
    {
        if (pointA == null)
        {
            return false;
        }

        if (pointB == null)
        {
            return false;
        }

        return true;
    }

    void CachePointPositions()
    {
        pointAPosition = pointA.position;
        pointBPosition = pointB.position;
    }

    void SetStartState()
    {
        if (startFromA)
        {
            transform.position = pointAPosition;
            movingToPointB = true;
            currentTargetPosition = pointBPosition;
            return;
        }

        transform.position = pointBPosition;
        movingToPointB = false;
        currentTargetPosition = pointAPosition;
    }

    void UpdateWaitingState()
    {
        waitTimer -= Time.deltaTime;
        frameDelta = Vector3.zero;
    }

    void MoveTowardsCurrentTarget()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            currentTargetPosition,
            speed * Time.deltaTime
        );
    }

    void SwitchTargetAndStartWaiting()
    {
        if (movingToPointB)
        {
            transform.position = pointBPosition;
            currentTargetPosition = pointAPosition;
            movingToPointB = false;
        }
        else
        {
            transform.position = pointAPosition;
            currentTargetPosition = pointBPosition;
            movingToPointB = true;
        }

        waitTimer = waitTime;
    }
}
