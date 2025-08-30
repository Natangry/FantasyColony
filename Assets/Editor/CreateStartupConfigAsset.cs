using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

public static class CreateStartupConfigAsset
{
    const string ResourcesDir = "Assets/Resources";
    const string AssetPath = ResourcesDir + "/GameStartupConfig.asset";

    [MenuItem("Tools/Fantasy Colony/Create Startup Config (Resources)")]
    public static void Create()
    {
        if (!AssetDatabase.IsValidFolder(ResourcesDir))
        {
            Directory.CreateDirectory(ResourcesDir);
            AssetDatabase.Refresh();
        }
        var cfg = ScriptableObject.CreateInstance<GameStartupConfig>();
        // Default to current scene path if in build settings
        var active = SceneManager.GetActiveScene();
        if (active.IsValid())
        {
            // Only set if included in build
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                if (SceneUtility.GetScenePathByBuildIndex(i) == active.path)
                {
                    cfg.introScenePath = active.path;
                    break;
                }
            }
        }
        AssetDatabase.CreateAsset(cfg, AssetPath);
        AssetDatabase.SaveAssets();
        Selection.activeObject = cfg;
        EditorGUIUtility.PingObject(cfg);
        Debug.Log("[StartupConfig] Created at " + AssetPath + ". Set 'introScenePath' to your intro/title scene (as listed in Build Settings).");
    }
}

