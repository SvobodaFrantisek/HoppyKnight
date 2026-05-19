using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;
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

    private const string VolumeKey = "MainMenu_Volume";
    private const string FullscreenKey = "MainMenu_Fullscreen";

    // --------------------
    // Unity zivotni cyklus
    // --------------------
    void Start()
    {
        Time.timeScale = 1f;

        LoadSavedSettings();
        SetupButtons();
        HidePauseMenu();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
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
        UnlockCursor();
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;

        HidePauseMenu();
        LockCursor();
    }

    public void OpenSettings()
    {
        SetPanelActive(pausePanel, false);
        SetPanelActive(settingsPanel, true);
        RefreshSettingsTexts();
    }

    public void BackToPauseMenu()
    {
        ShowPausePanel();
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
        if (Screen.fullScreen)
        {
            Screen.fullScreen = false;
        }
        else
        {
            Screen.fullScreen = true;
        }

        int fullscreenValue = 0;

        if (Screen.fullScreen)
        {
            fullscreenValue = 1;
        }

        PlayerPrefs.SetInt(FullscreenKey, fullscreenValue);
        PlayerPrefs.Save();
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
        AudioListener.volume = Mathf.Clamp01(newVolume);
        PlayerPrefs.SetFloat(VolumeKey, AudioListener.volume);
        PlayerPrefs.Save();
    }

    private void LoadSavedSettings()
    {
        LoadVolume();
        LoadFullscreen();
        RefreshSettingsTexts();
    }

    private void LoadVolume()
    {
        if (!PlayerPrefs.HasKey(VolumeKey))
        {
            return;
        }

        AudioListener.volume = PlayerPrefs.GetFloat(VolumeKey);
    }

    private void LoadFullscreen()
    {
        if (!PlayerPrefs.HasKey(FullscreenKey))
        {
            return;
        }

        int fullscreenValue = PlayerPrefs.GetInt(FullscreenKey);

        if (fullscreenValue == 1)
        {
            Screen.fullScreen = true;
        }
        else
        {
            Screen.fullScreen = false;
        }
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
