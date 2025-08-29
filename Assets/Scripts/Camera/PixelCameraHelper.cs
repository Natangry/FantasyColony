using UnityEngine;

/// <summary>
/// Utilities for pixel-perfect math with an orthographic camera.
/// </summary>
public static class PixelCameraHelper
{
    /// <summary>
    /// World units per on-screen pixel for the given camera.
    /// </summary>
    public static float WorldUnitsPerPixel(Camera cam)
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return 1f / 100f; // safe fallback
        // For an ortho camera, vertical world size is 2 * orthographicSize.
        // Divide by screen height (pixels) to get units-per-pixel.
        float h = Screen.height > 0 ? Screen.height : 1080f;
        return (2f * cam.orthographicSize) / h;
    }

    /// <summary>
    /// Snap a world position to the camera's pixel grid (X/Z for top-down).
    /// </summary>
    public static Vector3 SnapToPixelGrid(Vector3 worldPos, Camera cam)
    {
        float upp = Mathf.Max(1e-5f, WorldUnitsPerPixel(cam));
        worldPos.x = Mathf.Round(worldPos.x / upp) * upp;
        worldPos.z = Mathf.Round(worldPos.z / upp) * upp;
        return worldPos;
    }

    /// <summary>
    /// Returns the world-space rectangle (minX,maxX,minZ,maxZ) visible by the camera.
    /// </summary>
    public static (float minX, float maxX, float minZ, float maxZ) OrthoWorldBounds(Camera cam)
    {
        float halfH = cam.orthographicSize;
        float halfW = halfH * ((Screen.height > 0) ? (Screen.width / (float)Screen.height) : (16f/9f));
        return (cam.transform.position.x - halfW, cam.transform.position.x + halfW, cam.transform.position.z - halfH, cam.transform.position.z + halfH);
    }
}

