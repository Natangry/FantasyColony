// Editor-only: ensures sprites import with our project defaults (hi-res, crisp pixels).
// - Applies automatically to any texture under a folder named "Sprites" (e.g. Assets/Resources/Sprites/)
// - Adds menu items to (re)apply settings to selected textures or an entire folder
//
// Defaults:
//   TextureType: Sprite (2D and UI)
//   SpriteMode: Single
//   Pixels Per Unit: 64
//   Filter Mode: Point (no filter)
//   Compression: None (Uncompressed)
//   Wrap Mode: Clamp
//   MipMaps: Off
//   sRGB: On
//   Alpha Is Transparency: On (where supported)
//   Pivot: Bottom-Center (0.5, 0.0)

#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FantasyColony.EditorTools
{
    public class SpriteImportDefaults : AssetPostprocessor
    {
        const float DefaultPPU = 64f;
        const int   DefaultMaxSize = 1024;

        static bool IsSpritePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            path = path.Replace('\\', '/').ToLowerInvariant();
            return path.Contains("/sprites/");
        }

        void OnPreprocessTexture()
        {
            if (!IsSpritePath(assetPath)) return;
            var ti = (TextureImporter)assetImporter;
            ApplyDefaults(ti);
        }

        static void ApplyDefaults(TextureImporter ti)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = DefaultPPU;
            ti.mipmapEnabled = false;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.maxTextureSize = DefaultMaxSize;

            // Color space & alpha handling
#if UNITY_2019_3_OR_NEWER
            ti.sRGBTexture = true;
#endif
#if !UNITY_2023_1_OR_NEWER
            // In newer versions this field may be hidden/ignored, but it's safe to set where available.
            ti.alphaIsTransparency = true;
#endif
#if UNITY_2021_2_OR_NEWER
            ti.alphaSource = TextureImporterAlphaSource.FromInput;
#endif

            // Pivot: Bottom-Center
            ti.spriteAlignment = (int)SpriteAlignment.Custom;
            ti.spritePivot = new Vector2(0.5f, 0f);
        }

        // --- Context menus ---------------------------------------------------

        [MenuItem("Assets/Sprites/Apply Sprite Defaults (64 PPU)", true)]
        static bool ValidateApplyToSelection()
        {
            return Selection.assetGUIDs != null && Selection.assetGUIDs.Length > 0;
        }

        [MenuItem("Assets/Sprites/Apply Sprite Defaults (64 PPU)")]
        static void ApplyToSelection()
        {
            int count = 0;
            foreach (var guid in Selection.assetGUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path)) continue;
                if (!IsSpritePath(path)) continue;
                var ti = AssetImporter.GetAtPath(path) as TextureImporter;
                if (ti == null) continue;
                ApplyDefaults(ti);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                count++;
            }
            Debug.Log($"[SpriteImportDefaults] Applied to {count} asset(s).");
        }

        [MenuItem("Assets/Sprites/Apply Defaults To Folder (recursive)", true)]
        static bool ValidateApplyToFolder()
        {
            // Enable when a folder is selected
            return Selection.assetGUIDs.Any(guid =>
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                return AssetDatabase.IsValidFolder(path);
            });
        }

        [MenuItem("Assets/Sprites/Apply Defaults To Folder (recursive)")]
        static void ApplyToFolder()
        {
            int total = 0;
            foreach (var guid in Selection.assetGUIDs)
            {
                var folder = AssetDatabase.GUIDToAssetPath(guid);
                if (!AssetDatabase.IsValidFolder(folder)) continue;
                var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
                foreach (var texGuid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(texGuid);
                    if (!IsSpritePath(path)) continue;
                    var ti = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (ti == null) continue;
                    ApplyDefaults(ti);
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                    total++;
                }
            }
            Debug.Log($"[SpriteImportDefaults] Applied to {total} asset(s) in folder(s).");
        }
    }
}
#endif
