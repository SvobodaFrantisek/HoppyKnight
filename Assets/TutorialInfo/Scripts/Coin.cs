using UnityEngine;

public class Coin : MonoBehaviour
{
    // --------------------
    // Nastaveni pohybu
    // --------------------
    [Header("Idle")]
    public float rotationSpeed = 100f;
    public float hoverSpeed = 2f;
    public float hoverHeight = 0.25f;

    [Header("Collect")]
    public float flySpeed = 15f;

    private GameManager gameManager;
    private Vector3 startPosition;
    private bool isFlyingToPlayer;
    private Transform playerTransform;

    // --------------------
    // Unity zivotni cyklus
    // --------------------
    void Start()
    {
        CacheGameManager();
        startPosition = transform.position;
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

    // --------------------
    // Idle animace mince
    // --------------------
    void UpdateIdleMotion()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        float newY = startPosition.y + Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }

    // --------------------
    // Let mince k hraci
    // --------------------
    void UpdateFlyToPlayer()
    {
        if (playerTransform == null)
        {
            return;
        }

        transform.Rotate(Vector3.up, rotationSpeed * 5f * Time.deltaTime);

        Vector3 targetPosition = playerTransform.position + Vector3.up * 1.1f;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, flySpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
        {
            CollectCoin();
        }
    }

    // --------------------
    // Sebrani mince
    // --------------------
    void StartFlyToPlayer(Transform target)
    {
        isFlyingToPlayer = true;
        playerTransform = target;

        Collider coinCollider = GetComponent<Collider>();

        if (coinCollider != null)
        {
            coinCollider.enabled = false;
        }
    }

    void CollectCoin()
    {
        if (gameManager != null)
        {
            gameManager.AddCoin(1);
        }

        Destroy(gameObject);
    }

    // --------------------
    // Reference
    // --------------------
    void CacheGameManager()
    {
        gameManager = FindAnyObjectByType<GameManager>();

        if (gameManager == null)
        {
            Debug.LogError("GameManager nenalezen");
        }
    }
}
