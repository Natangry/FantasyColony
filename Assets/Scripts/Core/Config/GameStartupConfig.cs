using UnityEngine;

/// <summary>
/// Simple ScriptableObject that lets us explicitly pick the Intro scene for restarts.
/// Create/inspect at Assets/Resources/GameStartupConfig.asset
/// </summary>
[CreateAssetMenu(menuName = "FantasyColony/Game Startup Config", fileName = "GameStartupConfig")]
public class GameStartupConfig : ScriptableObject
{
    [Tooltip("Scene path (as in Build Settings) to use for 'Restart'. Leave empty to auto-detect.")]
    public string introScenePath;

    public static GameStartupConfig Load()
    {
        // The asset is optional; returns null if not present.
        return Resources.Load<GameStartupConfig>("GameStartupConfig");
    }
}

