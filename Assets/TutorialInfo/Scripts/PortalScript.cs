using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalScript : MonoBehaviour
{
    // --------------------
    // Napojeni z Inspectoru
    // --------------------
    public GameManager gameManager;
    public string nextSceneName;
    public float portalDelay = 1.5f;
    public Animator portalAnimator;

    private bool isFinishing;

    // --------------------
    // Unity zivotni cyklus
    // --------------------
    void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }
    }

    // --------------------
    // Vstup do portalu
    // --------------------
    void OnTriggerEnter(Collider other)
    {
        if (isFinishing)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (!CanFinishLevel())
        {
            return;
        }

        StartCoroutine(FinishLevel(other.gameObject));
    }

    // --------------------
    // Dokonceni levelu
    // --------------------
    IEnumerator FinishLevel(GameObject player)
    {
        isFinishing = true;

        DisablePlayerGameplay(player);
        PlayWinAnimation(player);

        yield return new WaitForSeconds(portalDelay);

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("Next scene name is not set in the PortalScript.");
        }

        SceneManager.LoadScene(nextSceneName);
    }

    // --------------------
    // Podminky portalu
    // --------------------
    bool CanFinishLevel()
    {
        if (gameManager == null)
        {
            return true;
        }

        return gameManager.gems >= gameManager.remainGems;
    }

    // --------------------
    // Vypnuti hrace a animace vyhry
    // --------------------
    void DisablePlayerGameplay(GameObject player)
    {
        FPSController controller = player.GetComponent<FPSController>();

        if (controller != null)
        {
            controller.enabled = false;
        }

        CombatScript combat = player.GetComponent<CombatScript>();

        if (combat != null)
        {
            combat.enabled = false;
        }
    }

    void PlayWinAnimation(GameObject player)
    {
        Animator targetAnimator = portalAnimator;

        if (targetAnimator == null)
        {
            targetAnimator = player.GetComponentInChildren<Animator>();
        }

        if (targetAnimator == null)
        {
            return;
        }

        targetAnimator.ResetTrigger("Jump");
        targetAnimator.ResetTrigger("doublejump");
        targetAnimator.ResetTrigger("Attack");
        targetAnimator.ResetTrigger("Dodge");
        targetAnimator.ResetTrigger("Win");
        targetAnimator.SetFloat("Speed", 0f);
        targetAnimator.SetBool("IsGrounded", true);
        targetAnimator.SetBool("IsClimbing", false);
        targetAnimator.SetTrigger("Win");
    }
}
