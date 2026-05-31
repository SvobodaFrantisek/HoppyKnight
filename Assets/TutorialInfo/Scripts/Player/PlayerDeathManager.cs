using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerDeathManager : MonoBehaviour
{
    // --------------------
    // Napojeni z Inspectoru
    // --------------------
    [Header("Player")]
    public GameManager gameManager;
    public FPSController fpsController;
    public CombatScript combatScript;
    public PlayerHitbox playerHitbox;
    public Respawn respawn;
    public Animator playerAnimator;

    [Header("Death UI")]
    public GameObject deathPanel;
    public Button respawnButton;
    public Button mainMenuButton;
    public string mainMenuSceneName = "MainMenu";

    [Header("Death Settings")]
    public string deathTriggerName = "Death";
    public bool freezeCameraOnDeath = true;
    public bool disableCinemachineOnDeath = true;
    public bool disablePauseMenuOnDeath = true;
    public bool resetAnimatorOnRespawn = true;

    // --------------------
    // Stav behem hry
    // --------------------
    private bool isDead;
    private Camera cameraToFreeze;
    private PauseMenuManager pauseMenuManager;
    private Vector3 frozenCameraPosition;
    private Quaternion frozenCameraRotation;
    private readonly List<Behaviour> disabledCinemachineBehaviours = new List<Behaviour>();

    // --------------------
    // Unity zivotni cyklus
    // --------------------
    void Awake()
    {
        CacheReferences();
        EnsureUiCanReceiveClicks();
        SetupButtons();
        SetDeathPanelActive(false);
    }

    void OnEnable()
    {
        CacheReferences();
        SetupButtons();
    }

    void Update()
    {
        if (isDead)
        {
            UnlockCursor();
            return;
        }

        if (gameManager == null)
        {
            CacheReferences();
        }

        if (gameManager != null && gameManager.health <= 0)
        {
            StartDeath();
        }
    }

    void LateUpdate()
    {
        if (!isDead || !freezeCameraOnDeath || cameraToFreeze == null)
        {
            return;
        }

        cameraToFreeze.transform.position = frozenCameraPosition;
        cameraToFreeze.transform.rotation = frozenCameraRotation;
    }

    // --------------------
    // Death flow
    // --------------------
    void StartDeath()
    {
        isDead = true;
        Time.timeScale = 1f;
        AudioListener.pause = false;

        CacheReferences();
        DisablePauseMenu();
        StopPlayerGameplay();
        TriggerDeathAnimation();
        FreezeCamera();
        SetDeathPanelActive(true);
        EnsureUiCanReceiveClicks();
        SetupButtons();
        UnlockCursor();
        SelectButton(respawnButton);
    }

    public void RespawnPlayer()
    {
        CacheReferences();
        SetDeathPanelActive(false);

        HealPlayerToFull();

        if (respawn != null)
        {
            respawn.PlayerRespawn();
        }

        ResetAnimatorAfterRespawn();
        RestorePlayerGameplay();
        RestoreCamera();
        RestorePauseMenu();
        LockCursor();
        ClearSelectedButton();

        isDead = false;
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (string.IsNullOrEmpty(mainMenuSceneName))
        {
            Debug.LogWarning("Main menu scene name is empty in PlayerDeathManager.");
            return;
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

    // --------------------
    // Player
    // --------------------
    void StopPlayerGameplay()
    {
        if (fpsController != null)
        {
            fpsController.SetInputLocked(true);
            fpsController.SetMovementLocked(true);
            fpsController.enabled = false;
        }

        if (combatScript != null)
        {
            combatScript.SetCombatBlocked(true);
            combatScript.enabled = false;
        }

        if (playerHitbox != null)
        {
            playerHitbox.enabled = false;
        }
    }

    void RestorePlayerGameplay()
    {
        if (playerHitbox != null)
        {
            playerHitbox.enabled = true;
        }

        if (combatScript != null)
        {
            combatScript.enabled = true;
            combatScript.SetCombatBlocked(false);
        }

        if (fpsController != null)
        {
            fpsController.enabled = true;
            fpsController.SetInputLocked(false);
            fpsController.SetMovementLocked(false);
        }
    }

    void HealPlayerToFull()
    {
        if (gameManager == null)
        {
            return;
        }

        int missingHealth = gameManager.maxHealth - gameManager.health;

        if (missingHealth > 0)
        {
            gameManager.Heal(missingHealth);
        }
    }

    // --------------------
    // Animator
    // --------------------
    void TriggerDeathAnimation()
    {
        if (playerAnimator == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(deathTriggerName))
        {
            return;
        }

        if (!HasAnimatorParameter(deathTriggerName, AnimatorControllerParameterType.Trigger))
        {
            Debug.LogWarning("Animator nema Trigger parametr '" + deathTriggerName + "'.");
            return;
        }

        playerAnimator.ResetTrigger(deathTriggerName);
        playerAnimator.SetTrigger(deathTriggerName);
    }

    void ResetAnimatorAfterRespawn()
    {
        if (!resetAnimatorOnRespawn || playerAnimator == null)
        {
            return;
        }

        playerAnimator.Rebind();
        playerAnimator.Update(0f);
    }

    bool HasAnimatorParameter(string parameterName, AnimatorControllerParameterType parameterType)
    {
        AnimatorControllerParameter[] parameters = playerAnimator.parameters;

        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].name == parameterName && parameters[i].type == parameterType)
            {
                return true;
            }
        }

        return false;
    }

    // --------------------
    // Kamera
    // --------------------
    void FreezeCamera()
    {
        if (!freezeCameraOnDeath)
        {
            return;
        }

        if (cameraToFreeze == null)
        {
            cameraToFreeze = Camera.main;
        }

        if (cameraToFreeze == null)
        {
            return;
        }

        if (disableCinemachineOnDeath)
        {
            DisableCinemachineBehaviours();
        }

        frozenCameraPosition = cameraToFreeze.transform.position;
        frozenCameraRotation = cameraToFreeze.transform.rotation;
    }

    void RestoreCamera()
    {
        RestoreCinemachineBehaviours();
    }

    void DisableCinemachineBehaviours()
    {
        disabledCinemachineBehaviours.Clear();

        Behaviour[] behaviours = FindObjectsByType<Behaviour>(FindObjectsSortMode.None);

        for (int i = 0; i < behaviours.Length; i++)
        {
            Behaviour behaviour = behaviours[i];

            if (behaviour != null && behaviour.enabled && IsCinemachineBehaviour(behaviour))
            {
                disabledCinemachineBehaviours.Add(behaviour);
                behaviour.enabled = false;
            }
        }
    }

    void RestoreCinemachineBehaviours()
    {
        for (int i = 0; i < disabledCinemachineBehaviours.Count; i++)
        {
            Behaviour behaviour = disabledCinemachineBehaviours[i];

            if (behaviour != null)
            {
                behaviour.enabled = true;
            }
        }

        disabledCinemachineBehaviours.Clear();
    }

    bool IsCinemachineBehaviour(Behaviour behaviour)
    {
        string typeName = behaviour.GetType().FullName;
        return !string.IsNullOrEmpty(typeName) && typeName.Contains("Cinemachine");
    }

    // --------------------
    // UI
    // --------------------
    void SetupButtons()
    {
        FindDeathButtons();

        if (respawnButton != null)
        {
            respawnButton.onClick.RemoveAllListeners();
            respawnButton.onClick.AddListener(RespawnPlayer);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(LoadMainMenu);
        }
    }

    void SetDeathPanelActive(bool activeState)
    {
        if (deathPanel != null)
        {
            deathPanel.SetActive(activeState);
        }
    }

    void EnsureUiCanReceiveClicks()
    {
        EnsureEventSystem();
        EnsurePanelCanvasCanReceiveClicks(deathPanel);
    }

    void EnsureEventSystem()
    {
        EventSystem eventSystem = EventSystem.current;

        if (eventSystem == null)
        {
            eventSystem = FindAnyObjectByType<EventSystem>();
        }

        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystem = eventSystemObject.AddComponent<EventSystem>();
        }

        EventSystem.current = eventSystem;
        eventSystem.enabled = true;

        InputSystemUIInputModule inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();

        if (inputModule == null)
        {
            inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        inputModule.enabled = true;
        inputModule.AssignDefaultActions();
    }

    void EnsurePanelCanvasCanReceiveClicks(GameObject panel)
    {
        if (panel == null)
        {
            return;
        }

        Canvas canvas = panel.GetComponentInParent<Canvas>(true);

        if (canvas == null)
        {
            return;
        }

        if (canvas.GetComponent<GraphicRaycaster>() == null)
        {
            canvas.gameObject.AddComponent<GraphicRaycaster>();
        }
    }

    void DisablePauseMenu()
    {
        if (!disablePauseMenuOnDeath || pauseMenuManager == null)
        {
            return;
        }

        if (pauseMenuManager.pausePanel != null)
        {
            pauseMenuManager.pausePanel.SetActive(false);
        }

        if (pauseMenuManager.settingsPanel != null)
        {
            pauseMenuManager.settingsPanel.SetActive(false);
        }

        pauseMenuManager.enabled = false;
    }

    void RestorePauseMenu()
    {
        if (!disablePauseMenuOnDeath || pauseMenuManager == null)
        {
            return;
        }

        pauseMenuManager.enabled = true;
    }

    void UnlockCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void SelectButton(Button button)
    {
        if (EventSystem.current == null || button == null)
        {
            return;
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button.gameObject);
    }

    void ClearSelectedButton()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    // --------------------
    // Automaticke dohledani referenci
    // --------------------
    void CacheReferences()
    {
        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (fpsController == null)
        {
            fpsController = FindPlayerComponent<FPSController>(playerObject);
        }

        if (combatScript == null)
        {
            combatScript = FindPlayerComponent<CombatScript>(playerObject);
        }

        if (playerHitbox == null)
        {
            playerHitbox = FindPlayerComponent<PlayerHitbox>(playerObject);
        }

        if (respawn == null)
        {
            respawn = FindPlayerComponent<Respawn>(playerObject);
        }

        if (playerAnimator == null)
        {
            playerAnimator = FindPlayerComponent<Animator>(playerObject);
        }

        if (pauseMenuManager == null)
        {
            pauseMenuManager = FindAnyObjectByType<PauseMenuManager>();
        }

        if (deathPanel == null)
        {
            Transform deathPanelTransform = FindTransformByName("DeathPanel");

            if (deathPanelTransform != null)
            {
                deathPanel = deathPanelTransform.gameObject;
            }
        }

        FindDeathButtons();
    }

    T FindPlayerComponent<T>(GameObject playerObject) where T : Component
    {
        if (playerObject != null)
        {
            T component = playerObject.GetComponentInChildren<T>(true);

            if (component != null)
            {
                return component;
            }
        }

        return FindAnyObjectByType<T>();
    }

    void FindDeathButtons()
    {
        if (deathPanel == null)
        {
            return;
        }

        Button[] buttons = deathPanel.GetComponentsInChildren<Button>(true);

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].gameObject.name == "DeathRespawnButton")
            {
                respawnButton = buttons[i];
            }
            else if (buttons[i].gameObject.name == "DeathMainMenuButton")
            {
                mainMenuButton = buttons[i];
            }
        }
    }

    Transform FindTransformByName(string targetName)
    {
        Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i].name == targetName)
            {
                return transforms[i];
            }
        }

        return null;
    }
}
