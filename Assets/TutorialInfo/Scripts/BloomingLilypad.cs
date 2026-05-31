using UnityEngine;

public class BloomingLilypad : MonoBehaviour
{
    // --------------------
    // Napojeni z Inspectoru
    // --------------------
    public BloomingLilypadManager manager;
    public Transform activationCenter;

    // --------------------
    // Nastaveni zapocitani
    // --------------------
    public float activationRadius = 0.8f;
    public bool requirePlayerAboveLilypad = true;
    public float minPlayerHeightAboveLilypad = -0.25f;
    public float maxPlayerHeightAboveLilypad = 2f;

    // --------------------
    // Stav lekninu
    // --------------------
    private bool counted;

    // --------------------
    // Unity zivotni cyklus
    // --------------------
    void Awake()
    {
        CacheManager();
    }

    void OnEnable()
    {
        CacheManager();
    }

    void OnTriggerEnter(Collider other)
    {
        if (counted)
        {
            return;
        }

        Transform playerTransform;

        if (!TryGetPlayerTransform(other, out playerTransform))
        {
            return;
        }

        if (!IsPlayerCloseEnough(playerTransform))
        {
            return;
        }

        CountLilypad();
    }

    // --------------------
    // Zapocitani lekninu
    // --------------------
    public void CountLilypad()
    {
        if (counted)
        {
            return;
        }

        counted = true;
        CacheManager();

        if (manager != null)
        {
            manager.RegisterLilypad(this);
            return;
        }

        Debug.LogWarning(gameObject.name + " nema prirazeny BloomingLilypadManager.");
    }

    public void SetManager(BloomingLilypadManager newManager)
    {
        manager = newManager;
    }

    // --------------------
    // Hledani hrace
    // --------------------
    bool TryGetPlayerTransform(Collider other, out Transform playerTransform)
    {
        playerTransform = null;

        if (other == null)
        {
            return false;
        }

        if (other.CompareTag("Player"))
        {
            playerTransform = other.transform;
            return true;
        }

        Transform root = other.transform.root;

        if (root != null && root.CompareTag("Player"))
        {
            playerTransform = root;
            return true;
        }

        FPSController controller = other.GetComponentInParent<FPSController>();

        if (controller != null)
        {
            playerTransform = controller.transform;
            return true;
        }

        return false;
    }

    bool IsPlayerCloseEnough(Transform playerTransform)
    {
        if (playerTransform == null)
        {
            return false;
        }

        Transform center = GetActivationCenter();
        Vector3 centerPosition = center.position;
        Vector3 playerPosition = playerTransform.position;

        Vector2 centerFlatPosition = new Vector2(centerPosition.x, centerPosition.z);
        Vector2 playerFlatPosition = new Vector2(playerPosition.x, playerPosition.z);
        float horizontalDistance = Vector2.Distance(centerFlatPosition, playerFlatPosition);

        if (horizontalDistance > activationRadius)
        {
            return false;
        }

        if (!requirePlayerAboveLilypad)
        {
            return true;
        }

        float heightDifference = playerPosition.y - centerPosition.y;

        if (heightDifference < minPlayerHeightAboveLilypad)
        {
            return false;
        }

        if (heightDifference > maxPlayerHeightAboveLilypad)
        {
            return false;
        }

        return true;
    }

    Transform GetActivationCenter()
    {
        if (activationCenter != null)
        {
            return activationCenter;
        }

        return transform;
    }

    // --------------------
    // Reference
    // --------------------
    void CacheManager()
    {
        if (manager != null)
        {
            return;
        }

        manager = FindAnyObjectByType<BloomingLilypadManager>();
    }
}
