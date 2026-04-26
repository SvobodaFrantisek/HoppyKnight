using UnityEngine;

public class Gem : MonoBehaviour
{
    [Header("Idle")]
    public float rotationSpeed = 100f;

    [Header("Collect")]
    public float flySpeed = 15f;

    public GameManager gameManager;
    public bool isFlyingToPlayer = false;

    private Transform playerPosition;

    void Start()
    {
        CacheGameManager();
    }

    void Update()
    {
        if (isFlyingToPlayer)
        {
            UpdateFlyToPlayer();
            return;
        }

        UpdateIdleMotion();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (isFlyingToPlayer)
        {
            return;
        }

        StartFlyToPlayer(other.transform);
    }

    void UpdateIdleMotion()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    void UpdateFlyToPlayer()
    {
        if (playerPosition == null)
        {
            return;
        }

        transform.Rotate(Vector3.up, rotationSpeed * 5f * Time.deltaTime);

        Vector3 targetPosition = playerPosition.position + Vector3.up * 1.1f;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, flySpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
        {
            CollectGem();
        }
    }

    void StartFlyToPlayer(Transform target)
    {
        playerPosition = target;
        isFlyingToPlayer = true;
    }

    void CollectGem()
    {
        if (gameManager != null)
        {
            gameManager.AddGem(1);
        }

        Destroy(gameObject);
    }

    void CacheGameManager()
    {
        gameManager = FindAnyObjectByType<GameManager>();

        if (gameManager == null)
        {
            Debug.LogError("GameManager nenalezen");
        }
    }
}
