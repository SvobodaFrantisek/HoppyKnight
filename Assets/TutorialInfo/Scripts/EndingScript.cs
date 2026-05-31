using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingScript : MonoBehaviour
{
    // --------------------
    // Nastaveni z Inspectoru
    // --------------------
    public DragonBossController boss3;
    public GameObject cageObject;
    public GameObject endingPanel;
    public Camera cameraToFreeze;
    public string endingSceneName;
    public float endingDelay = 0f;
    public float sceneChangeDelay = 10f;
    public bool disablePlayerOnEnding = true;
    public bool freezeCameraOnEnding = true;
    public bool disableCinemachineOnEnding = true;
    public bool unlockCursorOnEnding = true;

    // --------------------
    // Stav behem hry
    // --------------------
    private bool captiveReleased;
    private bool playerIsNearCaptive;
    private bool endingStarted;
    private GameObject currentPlayer;
    private Vector3 frozenCameraPosition;
    private Quaternion frozenCameraRotation;

    // --------------------
    // Unity zivotni cyklus
    // --------------------
    void Start()
    {
        if (endingPanel != null)
        {
            endingPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (!captiveReleased && IsBossDead())
        {
            ReleaseCaptive();
        }

        if (captiveReleased && playerIsNearCaptive)
        {
            StartEnding(currentPlayer);
        }
    }

    void LateUpdate()
    {
        if (!endingStarted || !freezeCameraOnEnding || cameraToFreeze == null)
        {
            return;
        }

        cameraToFreeze.transform.position = frozenCameraPosition;
        cameraToFreeze.transform.rotation = frozenCameraRotation;
    }

    // --------------------
    // Detekce hrace
    // --------------------
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerIsNearCaptive = true;
        currentPlayer = other.gameObject;

        if (captiveReleased)
        {
            StartEnding(currentPlayer);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerIsNearCaptive = false;

        if (currentPlayer == other.gameObject)
        {
            currentPlayer = null;
        }
    }

    // --------------------
    // Boss a osvobozeni
    // --------------------
    bool IsBossDead()
    {
        return boss3 != null && boss3.health <= 0;
    }

    void ReleaseCaptive()
    {
        captiveReleased = true;

        if (cageObject != null)
        {
            cageObject.SetActive(false);
        }
    }

    // --------------------
    // Ending
    // --------------------
    void StartEnding(GameObject player)
    {
        if (endingStarted)
        {
            return;
        }

        endingStarted = true;

        if (freezeCameraOnEnding)
        {
            FreezeCamera();
        }

        if (disablePlayerOnEnding)
        {
            DisablePlayerGameplay(player);
        }

        if (unlockCursorOnEnding)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        StartCoroutine(EndingRoutine());
    }

    void FreezeCamera()
    {
        if (cameraToFreeze == null)
        {
            cameraToFreeze = Camera.main;
        }

        if (cameraToFreeze == null)
        {
            return;
        }

        if (disableCinemachineOnEnding)
        {
            DisableCinemachineBehaviours();
        }

        frozenCameraPosition = cameraToFreeze.transform.position;
        frozenCameraRotation = cameraToFreeze.transform.rotation;
    }

    void DisableCinemachineBehaviours()
    {
        Behaviour[] behaviours = FindObjectsByType<Behaviour>(FindObjectsSortMode.None);

        for (int i = 0; i < behaviours.Length; i++)
        {
            Behaviour behaviour = behaviours[i];

            if (behaviour != null && IsCinemachineBehaviour(behaviour))
            {
                behaviour.enabled = false;
            }
        }
    }

    bool IsCinemachineBehaviour(Behaviour behaviour)
    {
        string typeName = behaviour.GetType().FullName;
        return !string.IsNullOrEmpty(typeName) && typeName.Contains("Cinemachine");
    }

    IEnumerator EndingRoutine()
    {
        if (endingDelay > 0f)
        {
            yield return new WaitForSeconds(endingDelay);
        }

        if (endingPanel != null)
        {
            endingPanel.SetActive(true);
        }

        if (sceneChangeDelay > 0f)
        {
            yield return new WaitForSeconds(sceneChangeDelay);
        }

        if (!string.IsNullOrEmpty(endingSceneName))
        {
            SceneManager.LoadScene(endingSceneName);
        }
    }

    void DisablePlayerGameplay(GameObject player)
    {
        if (player == null)
        {
            return;
        }

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
}
