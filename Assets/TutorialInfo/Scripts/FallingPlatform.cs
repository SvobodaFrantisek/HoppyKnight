using System.Collections;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    // --------------------
    // Nastaveni padu
    // --------------------
    public float fallDelay = 0.5f;
    public float resetDelay = 2f;
    public float minShakeAmount = 0.02f;
    public float maxShakeAmount = 0.12f;
    public float minShakeSpeed = 8f;
    public float maxShakeSpeed = 35f;

    // --------------------
    // Stav behem hry
    // --------------------
    private Vector3 startPosition;
    private Quaternion startRotation;
    private bool isTriggered;
    private MeshRenderer platformRenderer;
    private MeshCollider mainCollider;
    private BoxCollider triggerCollider;

    // --------------------
    // Unity zivotni cyklus
    // --------------------
    void Awake()
    {
        CacheStartTransform();
        CacheComponents();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        Activate();
    }

    // --------------------
    // Spusteni platformy
    // --------------------
    public void Activate()
    {
        if (isTriggered)
        {
            return;
        }

        StartCoroutine(FallRoutine());
    }

    // --------------------
    // Flow padu a resetu
    // --------------------
    IEnumerator FallRoutine()
    {
        isTriggered = true;

        yield return ShakeBeforeFall();

        ResetTransform();
        SetVisualState(false);

        yield return new WaitForSeconds(resetDelay);

        ResetTransform();
        SetVisualState(true);
        isTriggered = false;
    }

    IEnumerator ShakeBeforeFall()
    {
        float timer = 0f;

        while (timer < fallDelay)
        {
            timer += Time.deltaTime;
            UpdateShake(timer);
            yield return null;
        }
    }

    // --------------------
    // Shake pred zmizenim
    // --------------------
    void UpdateShake(float timer)
    {
        float progress = 1f;

        if (fallDelay > 0f)
        {
            progress = timer / fallDelay;

            if (progress > 1f)
            {
                progress = 1f;
            }
        }

        float currentShakeAmount = Mathf.Lerp(minShakeAmount, maxShakeAmount, progress);
        float currentShakeSpeed = Mathf.Lerp(minShakeSpeed, maxShakeSpeed, progress);
        float shakeTime = timer * currentShakeSpeed;

        float offsetX = Mathf.Sin(shakeTime) * currentShakeAmount;
        float offsetZ = Mathf.Cos(shakeTime * 1.15f) * currentShakeAmount;

        transform.position = startPosition + new Vector3(offsetX, 0f, offsetZ);
    }

    void ResetTransform()
    {
        transform.position = startPosition;
        transform.rotation = startRotation;
    }

    // --------------------
    // Vypnuti/zapnuti platformy
    // --------------------
    void SetVisualState(bool enabledState)
    {
        if (platformRenderer != null)
        {
            platformRenderer.enabled = enabledState;
        }

        if (mainCollider != null)
        {
            mainCollider.enabled = enabledState;
        }

        if (triggerCollider != null)
        {
            triggerCollider.enabled = enabledState;
        }
    }

    // --------------------
    // Cache
    // --------------------
    void CacheStartTransform()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    void CacheComponents()
    {
        platformRenderer = GetComponent<MeshRenderer>();
        mainCollider = GetComponent<MeshCollider>();
        triggerCollider = GetComponent<BoxCollider>();
    }
}
