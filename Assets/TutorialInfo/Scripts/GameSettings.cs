using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameSettings
{
    public const string VolumeKey = "MainMenu_Volume";
    public const string FullscreenKey = "MainMenu_Fullscreen";

    private static bool sceneHookRegistered;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        ApplySavedSettings();
        RegisterSceneHook();
    }

    public static void ApplySavedSettings()
    {
        ApplySavedVolume();
        ApplySavedFullscreen();
    }

    public static void SetVolume(float volume)
    {
        AudioListener.volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(VolumeKey, AudioListener.volume);
        PlayerPrefs.Save();
    }

    public static void ApplySavedVolume()
    {
        if (!PlayerPrefs.HasKey(VolumeKey))
        {
            return;
        }

        AudioListener.volume = Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey));
    }

    public static void ToggleFullscreen()
    {
        SetFullscreen(!Screen.fullScreen);
    }

    public static void SetFullscreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
        PlayerPrefs.SetInt(FullscreenKey, fullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static void ApplySavedFullscreen()
    {
        if (!PlayerPrefs.HasKey(FullscreenKey))
        {
            return;
        }

        Screen.fullScreen = PlayerPrefs.GetInt(FullscreenKey) == 1;
    }

    private static void RegisterSceneHook()
    {
        if (sceneHookRegistered)
        {
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        sceneHookRegistered = true;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplySavedSettings();
    }
}
