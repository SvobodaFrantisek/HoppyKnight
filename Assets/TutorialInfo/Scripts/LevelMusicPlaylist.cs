using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelMusicPlaylist : MonoBehaviour
{
    private const string SettingsResourcePath = "LevelMusicSettings";

    private static LevelMusicPlaylist instance;
    private static AudioClip lastPlayedClip;
    private static bool sceneHookRegistered;

    public LevelMusicSettings settings;
    public AudioSource audioSource;

    private readonly List<AudioClip> playQueue = new List<AudioClip>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        RegisterSceneHook();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        CacheReferences();
    }

    private void Update()
    {
        if (AudioListener.pause)
        {
            return;
        }

        if (audioSource == null || settings == null)
        {
            return;
        }

        if (!audioSource.isPlaying)
        {
            PlayNextTrack();
        }
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
        LevelMusicSettings loadedSettings = Resources.Load<LevelMusicSettings>(SettingsResourcePath);

        if (loadedSettings == null || !loadedSettings.IsLevelScene(scene.name))
        {
            DestroyInstance();
            return;
        }

        EnsureInstance(loadedSettings);
    }

    private static void EnsureInstance(LevelMusicSettings loadedSettings)
    {
        if (instance != null)
        {
            instance.settings = loadedSettings;
            instance.CacheReferences();
            return;
        }

        GameObject musicObject = new GameObject("Level Music Playlist");
        LevelMusicPlaylist playlist = musicObject.AddComponent<LevelMusicPlaylist>();
        playlist.settings = loadedSettings;
        playlist.CacheReferences();
    }

    private static void DestroyInstance()
    {
        if (instance == null)
        {
            return;
        }

        Destroy(instance.gameObject);
        instance = null;
    }

    private void CacheReferences()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;

        if (settings != null)
        {
            audioSource.volume = settings.volume;
        }
    }

    private void PlayNextTrack()
    {
        if (playQueue.Count == 0)
        {
            RefillQueue();
        }

        if (playQueue.Count == 0)
        {
            return;
        }

        AudioClip nextClip = playQueue[0];
        playQueue.RemoveAt(0);

        lastPlayedClip = nextClip;
        audioSource.clip = nextClip;
        audioSource.volume = settings.volume;
        audioSource.Play();
    }

    private void RefillQueue()
    {
        playQueue.Clear();

        if (settings == null || settings.tracks == null)
        {
            return;
        }

        for (int i = 0; i < settings.tracks.Length; i++)
        {
            if (settings.tracks[i] != null)
            {
                playQueue.Add(settings.tracks[i]);
            }
        }

        ShuffleQueue();
        MoveLastPlayedAwayFromStart();
    }

    private void ShuffleQueue()
    {
        for (int i = 0; i < playQueue.Count; i++)
        {
            int randomIndex = Random.Range(i, playQueue.Count);
            AudioClip temporaryClip = playQueue[i];
            playQueue[i] = playQueue[randomIndex];
            playQueue[randomIndex] = temporaryClip;
        }
    }

    private void MoveLastPlayedAwayFromStart()
    {
        if (playQueue.Count <= 1 || playQueue[0] != lastPlayedClip)
        {
            return;
        }

        int swapIndex = Random.Range(1, playQueue.Count);
        AudioClip temporaryClip = playQueue[0];
        playQueue[0] = playQueue[swapIndex];
        playQueue[swapIndex] = temporaryClip;
    }
}
