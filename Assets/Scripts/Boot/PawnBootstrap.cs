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
        pawnGO.AddComponent<SpritePawn>(); // default pattern from component
        return pawnGO;
    }

    /// <summary>
    /// Spawns a second pawn with a distinct movement pattern (figure-8 / Lissajous).
    /// Safe to call multiple times; only creates if not already present.
    /// </summary>
    public static GameObject SpawnSecondPawn()
    {
        var root = GameObject.Find("World");
        if (root == null)
        {
            root = new GameObject("World");
        }
        var existingGO = GameObject.Find("TestPawn_2");
        if (existingGO != null) return existingGO;

        var pawn2 = new GameObject("TestPawn_2");
        pawn2.transform.SetParent(root.transform, false);
        var sp = pawn2.AddComponent<SpritePawn>();
        // Configure a clearly different walk pattern & speed.
        sp.ConfigurePattern(SpritePawn.MovementPattern.Lissajous8, 2.4f);
        return pawn2;
    }
}

