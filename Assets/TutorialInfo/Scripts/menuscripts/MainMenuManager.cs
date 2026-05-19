using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    // --------------------
    // Sceny
    // --------------------
    [Header("Scenes")]
    public string gameSceneName = "Level_01";
    public string tutorialSceneName = "Test";

    // --------------------
    // Buttony ve scene
    // --------------------
    [Header("Buttons")]
    public Button playButton;
    public Button tutorialButton;
    public Button settingsButton;
    public Button exitButton;

    // --------------------
    // Tutorial panel ve scene
    // --------------------
    [Header("Tutorial")]
    public GameObject tutorialPanel;
    public Button tutorialBackButton;
    public GameObject[] mainMenuObjects;

    // --------------------
    // Settings hodnoty
    // --------------------
    [Header("Settings")]
    public float volumeStep = 0.25f;

    private const string VolumeKey = "MainMenu_Volume";
    private const string FullscreenKey = "MainMenu_Fullscreen";

    // --------------------
    // Unity zivotni cyklus
    // --------------------
    void Start()
    {
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        LoadSavedSettings();
        ShowMainMenu();
    }

    // --------------------
    // Hlavni menu akce
    // --------------------
    public void ShowMainMenu()
    {
        SetTutorialPanelActive(false);
        SetMainMenuObjectsActive(true);
        SetButtonActive(exitButton, true);

        SetupButton(playButton, "PLAY", PlayGame);
        SetupButton(tutorialButton, "TUTORIAL", OpenTutorial);
        SetupButton(settingsButton, "SETTINGS", OpenSettings);
        SetupButton(exitButton, "EXIT", ExitGame);

        SetupButton(tutorialBackButton, "BACK", ShowMainMenu);
    }

    public void PlayGame()
    {
        LoadScene(gameSceneName);
    }

    public void OpenTutorial()
    {
        SetMainMenuObjectsActive(false);
        SetTutorialPanelActive(true);
        SetupButton(tutorialBackButton, "BACK", ShowMainMenu);
    }

    public void OpenSettings()
    {
        RefreshSettingsMenu();
    }

    public void ExitGame()
    {
        Debug.Log("Exit button pressed.");

#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif

        Application.Quit();
    }

    // --------------------
    // Settings akce
    // --------------------
    public void ChangeVolume()
    {
        float newVolume = AudioListener.volume + volumeStep;

        if (newVolume > 1f)
        {
            newVolume = 0f;
        }

        SetVolume(newVolume);
        RefreshSettingsMenu();
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
        RefreshSettingsMenu();
    }

    // --------------------
    // Pomocne metody
    // --------------------
    private void RefreshSettingsMenu()
    {
        SetTutorialPanelActive(false);
        SetMainMenuObjectsActive(true);

        SetupButton(playButton, GetVolumeText(), ChangeVolume);
        SetupButton(tutorialButton, GetFullscreenText(), ToggleFullscreen);
        SetupButton(settingsButton, "BACK", ShowMainMenu);
        SetButtonActive(exitButton, false);
    }

    private void SetupButton(Button button, string labelText, UnityAction action)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);

        TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);

        if (text != null)
        {
            text.text = labelText;
        }
    }

    private void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("Scene name is empty in MainMenuManager.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    private void SetTutorialPanelActive(bool activeState)
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(activeState);
        }
    }

    private void SetMainMenuObjectsActive(bool activeState)
    {
        if (mainMenuObjects == null)
        {
            return;
        }

        int i = 0;

        while (i < mainMenuObjects.Length)
        {
            if (mainMenuObjects[i] != null)
            {
                mainMenuObjects[i].SetActive(activeState);
            }

            i++;
        }
    }

    private string GetVolumeText()
    {
        int volumePercent = Mathf.RoundToInt(AudioListener.volume * 100f);
        return "VOLUME: " + volumePercent + "%";
    }

    private string GetFullscreenText()
    {
        if (Screen.fullScreen)
        {
            return "FULLSCREEN: ON";
        }

        return "FULLSCREEN: OFF";
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

    private void SetButtonActive(Button button, bool activeState)
    {
        if (button != null)
        {
            button.gameObject.SetActive(activeState);
        }
    }
}
