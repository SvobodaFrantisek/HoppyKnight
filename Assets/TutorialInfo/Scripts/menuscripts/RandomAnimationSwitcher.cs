using UnityEngine;

public class RandomAnimationSwitcher : MonoBehaviour
{
    // --------------------
    // Animator
    // --------------------
    [Header("Animator")]
    public Animator animator;
    public string animationIndexParameter = "AnimationIndex";

    // --------------------
    // Random nastaveni
    // --------------------
    [Header("Random Animation")]
    public int animationCount = 4;
    public float minChangeTime = 3f;
    public float maxChangeTime = 6f;
    public bool changeOnStart = true;
    public bool avoidSameAnimation = true;

    // --------------------
    // Nahodny upper body trigger
    // --------------------
    [Header("Upper Body Trigger")]
    public bool useRandomUpperBodyTrigger = false;
    public string upperBodyTriggerParameter = "Wave";
    public float minUpperBodyTriggerTime = 8f;
    public float maxUpperBodyTriggerTime = 15f;

    // --------------------
    // Stav behem hry
    // --------------------
    private float timer;
    private float nextChangeTime;
    private int currentAnimationIndex = -1;
    private float upperBodyTimer;
    private float nextUpperBodyTriggerTime;

    // --------------------
    // Unity zivotni cyklus
    // --------------------
    void Awake()
    {
        CacheAnimator();
    }

    void Start()
    {
        PickNextChangeTime();
        PickNextUpperBodyTriggerTime();

        if (changeOnStart)
        {
            ChangeAnimation();
        }
    }

    void Update()
    {
        if (animator == null)
        {
            return;
        }

        if (animationCount <= 0)
        {
            return;
        }

        timer += Time.deltaTime;

        if (timer >= nextChangeTime)
        {
            timer = 0f;
            ChangeAnimation();
            PickNextChangeTime();
        }

        UpdateUpperBodyTrigger();
    }

    // --------------------
    // Vyber nahodne animace
    // --------------------
    void ChangeAnimation()
    {
        if (animator == null)
        {
            return;
        }

        if (animationCount <= 0)
        {
            return;
        }

        int randomIndex = Random.Range(0, animationCount);

        if (avoidSameAnimation && animationCount > 1)
        {
            while (randomIndex == currentAnimationIndex)
            {
                randomIndex = Random.Range(0, animationCount);
            }
        }

        currentAnimationIndex = randomIndex;
        animator.SetInteger(animationIndexParameter, currentAnimationIndex);
    }

    void PickNextChangeTime()
    {
        if (maxChangeTime < minChangeTime)
        {
            maxChangeTime = minChangeTime;
        }

        nextChangeTime = Random.Range(minChangeTime, maxChangeTime);

        if (nextChangeTime <= 0f)
        {
            nextChangeTime = 0.1f;
        }
    }

    // --------------------
    // Nahodne mavnuti
    // --------------------
    void UpdateUpperBodyTrigger()
    {
        if (!useRandomUpperBodyTrigger)
        {
            return;
        }

        if (string.IsNullOrEmpty(upperBodyTriggerParameter))
        {
            return;
        }

        upperBodyTimer += Time.deltaTime;

        if (upperBodyTimer >= nextUpperBodyTriggerTime)
        {
            upperBodyTimer = 0f;
            animator.SetTrigger(upperBodyTriggerParameter);
            PickNextUpperBodyTriggerTime();
        }
    }

    void PickNextUpperBodyTriggerTime()
    {
        if (maxUpperBodyTriggerTime < minUpperBodyTriggerTime)
        {
            maxUpperBodyTriggerTime = minUpperBodyTriggerTime;
        }

        nextUpperBodyTriggerTime = Random.Range(minUpperBodyTriggerTime, maxUpperBodyTriggerTime);

        if (nextUpperBodyTriggerTime <= 0f)
        {
            nextUpperBodyTriggerTime = 0.1f;
        }
    }

    // --------------------
    // Reference
    // --------------------
    void CacheAnimator()
    {
        if (animator != null)
        {
            return;
        }

        animator = GetComponent<Animator>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }
}
