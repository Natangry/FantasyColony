using UnityEngine;

/// <summary>
/// Helper to spawn a single SNES-style sprite pawn into the world.
/// </summary>
public static class PawnBootstrap
{
    public static GameObject SpawnSpritePawn()
    {
        // World root
        var root = GameObject.Find("World");
        if (root == null)
        {
            root = new GameObject("World");
        }

        // Avoid duplicates if already spawned.
#if UNITY_2022_2_OR_NEWER
        var existing = Object.FindAnyObjectByType<SpritePawn>();
#else
        var existing = Object.FindObjectOfType<SpritePawn>();
#endif
        if (existing != null)
        {
            return existing.gameObject;
        }

        var pawnGO = new GameObject("TestPawn");
        pawnGO.transform.SetParent(root.transform, false);
        pawnGO.AddComponent<SpritePawn>();
        return pawnGO;
    }
}

