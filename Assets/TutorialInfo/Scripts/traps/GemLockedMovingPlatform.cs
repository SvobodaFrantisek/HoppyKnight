using UnityEngine;

public class GemLockedMovingPlatform : MonoBehaviour
{
    // --------------------
    // Nastaveni z Inspectoru
    // --------------------
    public MovingPlatform movingPlatform;
    public GameManager gameManager;
    public bool disablePlatformOnStart = true;

    private bool platformStarted;

    // --------------------
    // Unity zivotni cyklus
    // --------------------
    void Awake()
    {
        CacheReferences();

        if (disablePlatformOnStart && movingPlatform != null)
        {
            movingPlatform.enabled = false;
            movingPlatform.frameDelta = Vector3.zero;
        }
    }

    void Update()
    {
        if (platformStarted)
        {
            return;
        }

        CacheReferences();

        if (movingPlatform == null || gameManager == null)
        {
            return;
        }

        if (gameManager.gems < gameManager.remainGems)
        {
            return;
        }

        StartPlatform();
    }

    // --------------------
    // Spusteni platformy
    // --------------------
    void StartPlatform()
    {
        platformStarted = true;
        movingPlatform.enabled = true;
        enabled = false;
    }

    // --------------------
    // Reference
    // --------------------
    void CacheReferences()
    {
        if (movingPlatform == null)
        {
            movingPlatform = GetComponent<MovingPlatform>();
        }

        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }
    }
}
