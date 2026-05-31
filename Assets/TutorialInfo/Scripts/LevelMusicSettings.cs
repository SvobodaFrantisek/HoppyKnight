using UnityEngine;

[CreateAssetMenu(menuName = "HoppyKnight/Level Music Settings")]
public class LevelMusicSettings : ScriptableObject
{
    public AudioClip[] tracks;
    [Range(0f, 1f)] public float volume = 0.4f;
    public string[] levelSceneNames = { "Level_01", "Level_02", "Level_03" };

    public bool IsLevelScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            return false;
        }

        if (levelSceneNames == null || levelSceneNames.Length == 0)
        {
            return sceneName.StartsWith("Level_");
        }

        for (int i = 0; i < levelSceneNames.Length; i++)
        {
            if (sceneName == levelSceneNames[i])
            {
                return true;
            }
        }

        return false;
    }
}
