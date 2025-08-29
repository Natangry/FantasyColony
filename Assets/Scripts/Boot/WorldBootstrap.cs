using UnityEngine;

/// <summary>
/// One-time helper to generate a small default grid world and frame the camera.
/// </summary>
public static class WorldBootstrap
{
    public static void GenerateDefaultGrid(int w = 128, int h = 128, float tile = 1f)
    {
        // Create/find root
        var root = GameObject.Find("World");
        if (root == null)
        {
            root = new GameObject("World");
        }

        // Ensure Buildings container exists so placed stations have a parent
        var buildings = root.transform.Find("Buildings");
        if (buildings == null)
        {
            var goBuild = new GameObject("Buildings");
            goBuild.transform.SetParent(root.transform, false);
            goBuild.transform.localPosition = Vector3.zero;
        }

        // Create/find grid
        var grid = root.GetComponentInChildren<SimpleGridMap>();
        if (grid == null)
        {
            var gridGO = new GameObject("Grid");
            gridGO.transform.SetParent(root.transform, false);
            grid = gridGO.AddComponent<SimpleGridMap>();
        }

        grid.Build(w, h, tile, grid.colorA, grid.colorB);

        EnsureDirectionalLight();
        FrameCameraToGrid(grid);
    }

    static void EnsureDirectionalLight()
    {
        Light dir = null;
#if UNITY_2022_2_OR_NEWER
        var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (var l in lights) { if (l != null && l.type == LightType.Directional) { dir = l; break; } }
#else
        var lights = Object.FindObjectsOfType<Light>();
        foreach (var l in lights) { if (l != null && l.type == LightType.Directional) { dir = l; break; } }
#endif
        if (dir == null)
        {
            var go = new GameObject("Directional Light");
            dir = go.AddComponent<Light>();
            dir.type = LightType.Directional;
            dir.intensity = 1.0f;
            go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }
    }

    static void FrameCameraToGrid(SimpleGridMap grid)
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var camGO = new GameObject("Main Camera");
            cam = camGO.AddComponent<Camera>();
            // Make sure it's tagged correctly so Camera.main works later.
            camGO.tag = "MainCamera";
        }

        // Orthographic top-down
        cam.orthographic = true;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;
        cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // Compute world center
        float wWorld = grid.width * grid.tileSize;
        float hWorld = grid.height * grid.tileSize;
        var center = new Vector3(
            (grid.width * grid.tileSize) * 0.5f - (grid.tileSize * 0.5f),
            0f,
            (grid.height * grid.tileSize) * 0.5f - (grid.tileSize * 0.5f)
        );

        // Position camera above center
        cam.transform.position = new Vector3(center.x, 10f, center.z);

        // Fit orthographic size to show most of the grid with a small margin
        float aspect = (Screen.height > 0) ? (Screen.width / (float)Screen.height) : (16f / 9f);
        float halfHeight = hWorld * 0.5f;
        float halfWidth = wWorld * 0.5f;
        float sizeToFit = Mathf.Max(halfHeight, halfWidth / Mathf.Max(0.1f, aspect));
        // Keep a consistent default start zoom across all map sizes.
        // Do not auto-fit; use the camera's existing orthographicSize configured in the scene.
    }
}

