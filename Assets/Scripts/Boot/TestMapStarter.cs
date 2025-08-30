using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Bridge used by the new Intro screen's Start button.
/// Creates/loads a simple test world and guarantees two wandering SpritePawns are present.
/// It prefers WorldBootstrap.GenerateDefaultGrid(int,int,float) but falls back to other common starters.
/// Returns true on success so the Intro overlay can hide itself.
/// </summary>
public static class TestMapStarter
{
    public static bool StartTestMap(int width, int height)
    {
        try
        {
            if (!TryStartWorld(width, height))
            {
                Debug.LogError("[TestMapStarter] Failed to start world (no suitable world bootstrap method found).");
                return false;
            }

            EnsurePawnInteractionManager();
            SpawnTwoTestPawnsNearCenter(width, height);

            Debug.Log("[TestMapStarter] Test map started with two SpritePawns.");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[TestMapStarter] Exception during StartTestMap: {ex}");
            return false;
        }
    }

    // --- World start --------------------------------------------------------

    private static bool TryStartWorld(int w, int h)
    {
        // 1) Prefer an explicit WorldBootstrap.GenerateDefaultGrid(int,int,float) if present.
        var worldType = FindTypeByPreferredNames(new[]
        {
            "WorldBootstrap","GameBootstrap","WorldBuilder","MapGenerator","GridBootstrap","WorldInitializer","GameInit"
        });

        if (worldType != null)
        {
            var methods = worldType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

            // Try the most likely signatures first.
            var direct =
                FindMethod(methods, "GenerateDefaultGrid", typeof(void), new[] { typeof(int), typeof(int), typeof(float) }) ??
                FindMethod(methods, "GenerateDefaultGrid", typeof(void), new[] { typeof(int), typeof(int) }) ??
                FindMethod(methods, "StartNewGame",      typeof(void), new[] { typeof(int), typeof(int), typeof(float) }) ??
                FindMethod(methods, "StartNewGame",      typeof(void), new[] { typeof(int), typeof(int) }) ??
                FindMethod(methods, "GenerateWorld",     typeof(void), Type.EmptyTypes) ??
                FindMethod(methods, "CreateWorld",       typeof(void), Type.EmptyTypes) ??
                FindMethod(methods, "CreateGrid",        typeof(void), new[] { typeof(int), typeof(int) }) ??
                FindMethod(methods, "InitWorld",         typeof(void), Type.EmptyTypes) ??
                FindMethod(methods, "BootWorld",         typeof(void), Type.EmptyTypes);

            if (direct != null)
            {
                InvokeStarter(worldType, direct, w, h);
                return true;
            }
        }

        // 2) Robust sweep across all assemblies/types/methods using preferred name fragments.
        var typeHints   = new[] { "WorldBootstrap", "GameBootstrap", "WorldBuilder", "MapGenerator", "GridBootstrap", "WorldInitializer", "GameInit" };
        var methodHints = new[] { "GenerateDefaultGrid", "StartNewGame", "StartGame", "GenerateWorld", "CreateWorld", "CreateGrid", "InitWorld", "BootWorld" };

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var t in SafeGetTypes(asm))
            {
                if (!typeHints.Any(h => t.Name.IndexOf(h, StringComparison.OrdinalIgnoreCase) >= 0))
                    continue;

                foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                {
                    if (!methodHints.Any(h => m.Name.IndexOf(h, StringComparison.OrdinalIgnoreCase) >= 0))
                        continue;

                    // Try (int,int,float), then (int,int), then ().
                    if (MatchesSignature(m, typeof(void), new[] { typeof(int), typeof(int), typeof(float) }))
                    {
                        InvokeStarter(t, m, w, h);
                        return true;
                    }
                    if (MatchesSignature(m, typeof(void), new[] { typeof(int), typeof(int) }))
                    {
                        InvokeStarter(t, m, w, h);
                        return true;
                    }
                    if (MatchesSignature(m, typeof(void), Type.EmptyTypes))
                    {
                        InvokeStarter(t, m);
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private static void InvokeStarter(Type type, MethodInfo method, int w = 0, int h = 0)
    {
        object instance = method.IsStatic ? null : Activator.CreateInstance(type);
        var ps = method.GetParameters();

        object[] args;
        if (ps.Length == 3) args = new object[] { w, h, 1f };     // (int,int,float) default tile size
        else if (ps.Length == 2) args = new object[] { w, h };     // (int,int)
        else args = Array.Empty<object>();                         // ()

        method.Invoke(instance, args);
        Debug.Log($"[TestMapStarter] Started via {type.FullName}.{method.Name}({string.Join(",", ps.Select(p => p.ParameterType.Name))})");
    }

    private static MethodInfo FindMethod(IEnumerable<MethodInfo> methods, string name, Type returnType, Type[] parameters)
        => methods.FirstOrDefault(m => m.Name == name && MatchesSignature(m, returnType, parameters));

    private static bool MatchesSignature(MethodInfo m, Type returnType, IReadOnlyList<Type> parameters)
    {
        if (m.ReturnType != returnType) return false;
        var ps = m.GetParameters();
        if (ps.Length != parameters.Count) return false;
        for (int i = 0; i < ps.Length; i++)
        {
            if (ps[i].ParameterType != parameters[i]) return false;
        }
        return true;
    }

    private static Type FindTypeByPreferredNames(IEnumerable<string> preferredNames)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var t in SafeGetTypes(asm))
            {
                if (preferredNames.Any(n => t.Name.Equals(n, StringComparison.Ordinal) || t.FullName.EndsWith("." + n, StringComparison.Ordinal)))
                    return t;
            }
        }
        return null;
    }

    private static IEnumerable<Type> SafeGetTypes(Assembly a)
    {
        try { return a.GetTypes(); }
        catch { return Array.Empty<Type>(); }
    }

    // --- Managers & pawns ---------------------------------------------------

    private static void EnsurePawnInteractionManager()
    {
        var existing = UnityEngine.Object.FindObjectOfType<MonoBehaviour>(true);
        var pim = UnityEngine.Object.FindObjectOfType<PawnInteractionManager>(true);
        if (pim != null) return;

        var root = GameObject.Find("GameSystems (Auto)") ?? new GameObject("GameSystems (Auto)");
        if (root.scene.name == null) UnityEngine.Object.DontDestroyOnLoad(root);

        root.AddComponent<PawnInteractionManager>();
        Debug.Log("[TestMapStarter] Ensured PawnInteractionManager.");
    }

    private static void SpawnTwoTestPawnsNearCenter(int width, int height)
    {
        // Heuristic center: try camera center projection; otherwise origin.
        Vector3 center = Vector3.zero;

        var cam = Camera.main;
        if (cam != null)
        {
            // Project to ground plane at y = 0
            var ray = cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));
            if (new Plane(Vector3.up, Vector3.zero).Raycast(ray, out var dist))
                center = ray.GetPoint(dist);
        }

        float tile = 1f;
        Vector3 aPos = center + new Vector3(-0.5f * tile, 0f, 0f);
        Vector3 bPos = center + new Vector3(+0.5f * tile, 0f, 0f);

        CreateSpritePawn("TestPawn_A", aPos);
        CreateSpritePawn("TestPawn_B", bPos);
    }

    private static void CreateSpritePawn(string name, Vector3 position)
    {
        var go = new GameObject(name);
        go.transform.position = position;

        // Ensure selectable/collidable with existing 3D ray-based selection.
        var col = go.AddComponent<SphereCollider>();
        col.radius = 0.25f;

        var rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        go.AddComponent<SpritePawn>();
    }
}
