using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Single-entry bridge the Intro menu calls to start the existing test map.
/// Tries the project's direct world bootstrap first, then robust fallbacks.
/// Verifies grid exists and optionally spawns test pawns.
/// </summary>
public static class TestMapStarter
{
    /// <summary>
    /// Start the test map with the given dimensions. Returns true on success.
    /// </summary>
    public static bool StartTestMap(int width, int height)
    {
        // Unpause just in case
        if (Time.timeScale != 1f) Time.timeScale = 1f;

        // Prefer a direct call path first if present in this project:
        // WorldBootstrap.GenerateDefaultGrid(int,int)
        if (InvokeIfExists("WorldBootstrap", "GenerateDefaultGrid", new object[] { width, height }, new[] { typeof(int), typeof(int) }))
        {
            Debug.Log($"[TestMapStarter] Started via WorldBootstrap.GenerateDefaultGrid({width},{height})");
            if (!VerifyGridUp()) return false;
            TrySpawnTestPawns();
            return true;
        }

        // Robust ordered search across common types and method names/signatures
        if (TryStartWorldRobust(width, height, out var selected))
        {
            Debug.Log("[TestMapStarter] Started via " + selected);
            if (!VerifyGridUp()) return false;
            TrySpawnTestPawns();
            return true;
        }

        Debug.LogError("[TestMapStarter] Could not find a world start method. Nothing was started.");
        return false;
    }

    // -------- helpers --------

    static bool VerifyGridUp()
    {
        // Wait one frame-equivalent: caller (Intro UI) will call us synchronously;
        // in practice most generators finish same frame. We just check presence.
        var grid = UnityEngine.Object.FindFirstObjectByType<SimpleGridMap>();
        if (grid == null)
        {
            Debug.LogError("[TestMapStarter] SimpleGridMap not found after start. Did the generator run?");
            return false;
        }
        return true;
    }

    static void TrySpawnTestPawns()
    {
        // If any pawns/agents already exist, skip.
        var anyPawn = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .Any(mb =>
            {
                var n = mb.GetType().Name.ToLowerInvariant();
                return n.Contains("pawn") || n.Contains("agent") || n.Contains("actor");
            });
        if (anyPawn) return;

        // Try a few likely entrypoints to spawn default/test pawns
        string[] typeHints = { "Pawn", "Actor", "Agent", "Spawner", "Bootstrap" };
        string[] methodNames = { "SpawnTestPawns", "SpawnDefaultPawns", "SpawnPawns", "CreateTestActors", "CreateDefaultPawns" };
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try { types = asm.GetTypes(); } catch { continue; }
            foreach (var t in types)
            {
                var tn = t.Name.ToLowerInvariant();
                if (!typeHints.Any(h => tn.Contains(h.ToLowerInvariant()))) continue;
                foreach (var mn in methodNames)
                {
                    // Try (int) then ()
                    var mi_i = t.GetMethod(mn, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(int) }, null);
                    if (mi_i != null) { mi_i.Invoke(null, new object[] { 2 }); Debug.Log($"[TestMapStarter] {t.Name}.{mn}(2)"); return; }
                    var mi_0 = t.GetMethod(mn, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, Type.EmptyTypes, null);
                    if (mi_0 != null) { mi_0.Invoke(null, null); Debug.Log($"[TestMapStarter] {t.Name}.{mn}()"); return; }
                }
            }
        }
        // Not fatal if we couldn't spawn them.
        Debug.Log("[TestMapStarter] No pawn spawner found; map started without test pawns.");
    }

    static bool TryStartWorldRobust(int w, int h, out string selected)
    {
        selected = null;
        string[] typeOrder =
        {
            "WorldBootstrap","GameBootstrap","WorldBuilder","MapGenerator","GridBootstrap","WorldInitializer","GameInit"
        };
        string[] methodOrder =
        {
            "GenerateDefaultGrid","StartNewGame","StartGame","GenerateWorld","CreateWorld","CreateGrid","InitWorld","BootWorld"
        };

        var asms = AppDomain.CurrentDomain.GetAssemblies();
        // Preferred type list first
        foreach (var tName in typeOrder)
        {
            var t = FindExactType(asms, tName);
            if (t == null) continue;
            foreach (var mName in methodOrder)
            {
                if (InvokeMatchedSignature(t, mName, w, h, out selected)) return true;
            }
        }
        // Broad sweep across *Bootstrap/*Generator/*Builder
        foreach (var asm in asms)
        {
            Type[] types;
            try { types = asm.GetTypes(); } catch { continue; }
            foreach (var t in types)
            {
                var tn = t.Name.ToLowerInvariant();
                if (!tn.Contains("bootstrap") && !tn.Contains("generator") && !tn.Contains("builder")) continue;
                foreach (var mName in methodOrder)
                {
                    if (InvokeMatchedSignature(t, mName, w, h, out selected)) return true;
                }
            }
        }
        return false;
    }

    static bool InvokeMatchedSignature(Type t, string method, int w, int h, out string selected)
    {
        selected = null;
        // (int,int)
        var mi_ii = t.GetMethod(method, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(int), typeof(int) }, null);
        if (mi_ii != null) { mi_ii.Invoke(null, new object[] { w, h }); selected = $"{t.Name}.{method}(int,int)"; return true; }
        // (Vector2Int)
        var v2 = typeof(Vector2Int);
        var mi_v = t.GetMethod(method, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, new[] { v2 }, null);
        if (mi_v != null) { mi_v.Invoke(null, new object[] { new Vector2Int(w, h) }); selected = $"{t.Name}.{method}(Vector2Int)"; return true; }
        // ()
        var mi_0 = t.GetMethod(method, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, Type.EmptyTypes, null);
        if (mi_0 != null) { mi_0.Invoke(null, null); selected = $"{t.Name}.{method}()"; return true; }
        // (string preset)
        var mi_s = t.GetMethod(method, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(string) }, null);
        if (mi_s != null) { mi_s.Invoke(null, new object[] { "Large" }); selected = $"{t.Name}.{method}(string)"; return true; }
        return false;
    }

    static bool InvokeIfExists(string typeName, string methodName, object[] args, Type[] sig)
    {
        var asms = AppDomain.CurrentDomain.GetAssemblies();
        var t = FindExactType(asms, typeName);
        if (t == null) return false;
        var mi = t.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, sig, null);
        if (mi == null) return false;
        mi.Invoke(null, args);
        return true;
    }

    static Type FindExactType(Assembly[] asms, string name)
    {
        foreach (var asm in asms)
        {
            Type[] types;
            try { types = asm.GetTypes(); } catch { continue; }
            foreach (var t in types)
            {
                if (t.Name.Equals(name, StringComparison.Ordinal)) return t;
            }
        }
        return null;
    }
}

