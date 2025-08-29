using UnityEngine;

/// <summary>
/// Ensures the main camera follows the currently controlled pawn and supports free WASD panning.
/// </summary>
public static class CameraBootstrap
{
    private static bool added;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureCameraFollower()
    {
        if (added) return;
        var cam = Camera.main;
        if (cam == null)
        {
            // Minimal fallback camera; WorldBootstrap may reconfigure later.
            var go = new GameObject("Main Camera");
            cam = go.AddComponent<Camera>();
            go.tag = "MainCamera";
            cam.orthographic = true;
            cam.orthographicSize = 10f;
            cam.transform.position = new Vector3(0f, 10f, 0f);
            cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }

        if (cam.GetComponent<FollowControlledPawn>() == null)
            cam.gameObject.AddComponent<FollowControlledPawn>();

        // Add free camera controls for when no pawn is controlled.
        if (cam.GetComponent<FreeCameraController>() == null)
            cam.gameObject.AddComponent<FreeCameraController>();

        added = true;
    }
}
