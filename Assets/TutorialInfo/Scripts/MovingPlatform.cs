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
        if (pointA == null || pointB == null)
        {
            Debug.LogError("MovingPlatform: chybi pointA nebo pointB.");
            enabled = false;
            return;
        }

        pointAPosition = pointA.position;
        pointBPosition = pointB.position;

        if (startFromA)
        {
            transform.position = pointAPosition;
            movingToPointB = true;
            currentTargetPosition = pointBPosition;
        }
        else
        {
            transform.position = pointBPosition;
            movingToPointB = false;
            currentTargetPosition = pointAPosition;
        }

        previousPosition = transform.position;
        frameDelta = Vector3.zero;
    }

    void Update()
    {
        previousPosition = transform.position;

        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            frameDelta = Vector3.zero;
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            currentTargetPosition,
            speed * Time.deltaTime
        );

        if (transform.position == currentTargetPosition)
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

        frameDelta = transform.position - previousPosition;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            FPSController controller = other.GetComponent<FPSController>();

            if (controller != null)
            {
                controller.SetCurrentPlatform(this);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            FPSController controller = other.GetComponent<FPSController>();

            if (controller != null)
            {
                controller.ClearCurrentPlatform(this);
            }
        }
    }
}
