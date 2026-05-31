using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    // --------------------
    // Panely ve scene
    // --------------------
    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject settingsPanel;

    // --------------------
    // Buttony ve scene
    // --------------------
    [Header("Pause Buttons")]
    public Button resumeButton;
    public Button settingsButton;
    public Button mainMenuButton;
    public Button quitButton;

    [Header("Settings Buttons")]
    public Button volumeButton;
    public Button fullscreenButton;
    public Button settingsBackButton;

    // --------------------
    // Texty ve scene
    // --------------------
    [Header("Settings Texts")]
    public TMP_Text volumeText;
    public TMP_Text fullscreenText;

    // --------------------
    // Nastaveni
    // --------------------
    [Header("Settings")]
    public string mainMenuSceneName = "MainMenu";
    public float volumeStep = 0.25f;

    private bool isPaused = false;

    // --------------------
    // Unity zivotni cyklus
    // --------------------
    void Start()
    {
        Time.timeScale = 1f;

        LoadSavedSettings();
        EnsureUiCanReceiveClicks();
        SetupButtons();
        HidePauseMenu();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        if (isPaused)
        {
            UnlockCursor();
        }
    }

    // --------------------
    // Pauza
    // --------------------
    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;

        ShowPausePanel();
        EnsureUiCanReceiveClicks();
        SetupButtons();
        UnlockCursor();
        SelectButton(resumeButton);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;

        HidePauseMenu();
        LockCursor();
        ClearSelectedButton();
    }

    public void OpenSettings()
    {
        SetPanelActive(pausePanel, false);
        SetPanelActive(settingsPanel, true);
        EnsureUiCanReceiveClicks();
        RefreshSettingsTexts();
        SelectButton(volumeButton);
    }

    public void BackToPauseMenu()
    {
        ShowPausePanel();
        EnsureUiCanReceiveClicks();
        SelectButton(resumeButton);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (string.IsNullOrEmpty(mainMenuSceneName))
        {
            Debug.LogWarning("Main menu scene name is empty in PauseMenuManager.");
            return;
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Debug.Log("Quit button pressed.");

#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif

        Application.Quit();
    }

    // --------------------
    // Settings
    // --------------------
    public void ChangeVolume()
    {
        float newVolume = AudioListener.volume + volumeStep;

        if (newVolume > 1f)
        {
            newVolume = 0f;
        }

        SetVolume(newVolume);
        RefreshSettingsTexts();
    }

    public void ToggleFullscreen()
    {
        GameSettings.ToggleFullscreen();
        RefreshSettingsTexts();
    }

    // --------------------
    // Pomocne metody
    // --------------------
    private void SetupButtons()
    {
        SetupButton(resumeButton, ResumeGame);
        SetupButton(settingsButton, OpenSettings);
        SetupButton(mainMenuButton, LoadMainMenu);
        SetupButton(quitButton, QuitGame);
        SetupButton(volumeButton, ChangeVolume);
        SetupButton(fullscreenButton, ToggleFullscreen);
        SetupButton(settingsBackButton, BackToPauseMenu);
    }

    private void SetupButton(Button button, UnityAction action)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private void ShowPausePanel()
    {
        SetPanelActive(pausePanel, true);
        SetPanelActive(settingsPanel, false);
    }

    private void HidePauseMenu()
    {
        SetPanelActive(pausePanel, false);
        SetPanelActive(settingsPanel, false);
    }

    private void SetPanelActive(GameObject panel, bool activeState)
    {
        if (panel != null)
        {
            panel.SetActive(activeState);
        }
    }

    private void EnsureUiCanReceiveClicks()
    {
        EnsureEventSystem();
        EnsurePanelCanvasCanReceiveClicks(pausePanel);
        EnsurePanelCanvasCanReceiveClicks(settingsPanel);
    }

    private void EnsureEventSystem()
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

    private void EnsurePanelCanvasCanReceiveClicks(GameObject panel)
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

    private void SelectButton(Button button)
    {
        if (EventSystem.current == null || button == null)
        {
            return;
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button.gameObject);
    }

    private void ClearSelectedButton()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void RefreshSettingsTexts()
    {
        TMP_Text currentVolumeText = volumeText;

        if (currentVolumeText == null && volumeButton != null)
        {
            currentVolumeText = volumeButton.GetComponentInChildren<TMP_Text>(true);
        }

        TMP_Text currentFullscreenText = fullscreenText;

        if (currentFullscreenText == null && fullscreenButton != null)
        {
            currentFullscreenText = fullscreenButton.GetComponentInChildren<TMP_Text>(true);
        }

        if (currentVolumeText != null)
        {
            int volumePercent = Mathf.RoundToInt(AudioListener.volume * 100f);
            currentVolumeText.text = "VOLUME: " + volumePercent + "%";
        }

        if (currentFullscreenText != null)
        {
            if (Screen.fullScreen)
            {
                currentFullscreenText.text = "FULLSCREEN: ON";
            }
            else
            {
                currentFullscreenText.text = "FULLSCREEN: OFF";
            }
        }
    }

    private void SetVolume(float newVolume)
    {
        GameSettings.SetVolume(newVolume);
    }

    private void LoadSavedSettings()
    {
        GameSettings.ApplySavedSettings();
        RefreshSettingsTexts();
    }

    private void UnlockCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
