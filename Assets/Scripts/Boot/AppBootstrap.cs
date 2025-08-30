using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// App-level bootstrap that ensures an Intro overlay exists/appears at boot *without* relying on Unity scenes.
/// It discovers your project's real Intro/Title/Menu MonoBehaviour via reflection and shows it. If none
/// exists, it spawns a tiny fallback overlay so you're never stuck on a blank scene.
/// </summary>
public static class AppBootstrap
{
    static readonly string[] IntroTypeHints = { "intro", "title", "menu" };
    static readonly string[] VisibleFieldNames = { "showMenu", "isVisible", "visible", "IsVisible" };
    static readonly string[] ShowMethodNames = { "Show", "Open", "Enable", "ShowMenu", "ShowIntro", "SetVisible", "SetActive" };
    static readonly string[] HideMethodNames = { "Hide", "Close", "Disable", "HideMenu", "HideIntro", "SetVisible", "SetActive" };

    // Runs very early; still do a delayed check after first frame in case other boot code spawns the overlay a bit later.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    static void EnsureIntroEarly()
    {
        MakeRunner(); // ensures we also re-check shortly after boot
        TryShowIntroOverlay("AfterAssembliesLoaded");
        SceneManager.activeSceneChanged += (_, __) => TryShowIntroOverlay("activeSceneChanged");
    }

    /// <summary>Returns true if an intro-like overlay is visible.</summary>
    public static bool IsIntroVisible()
    {
        var comp = FindIntroComponentInScene();
        if (comp == null) return false;
        // Heuristics: visible flag or active+enabled
        if (TryGetVisibleFlag(comp, out bool vis)) return vis;
        return comp.isActiveAndEnabled && comp.gameObject.activeInHierarchy;
    }

    /// <summary>Shows the intro overlay (prefer the project's real one; otherwise spawn a fallback).</summary>
    public static void ShowIntroOverlay()
    {
        if (TryShowIntroOverlay("explicit call")) return;
        // If project has no intro overlay type, create a simple fallback UI.
        IntroOverlayFallback.SpawnIfNeeded();
        Debug.Log("[AppBootstrap] Spawned IntroOverlayFallback (no real intro overlay found).");
    }

    /// <summary>Hides any intro overlay (real or fallback).</summary>
    public static void HideIntroOverlay()
    {
        var comp = FindIntroComponentInScene();
        if (comp != null)
        {
            if (TryInvokeHide(comp)) return;
            TrySetVisibleFlag(comp, false);
            comp.gameObject.SetActive(false);
            return;
        }
        IntroOverlayFallback.HideIfPresent();
    }

    // --- internals ---

    static bool TryShowIntroOverlay(string reason)
    {
        var comp = FindIntroComponentInScene();
        if (comp == null)
        {
            // Maybe the type exists but no instance yet; try instantiating the first matching type
            var t = FindIntroType();
            if (t != null)
            {
                var go = new GameObject(t.Name + " (Auto)");
                comp = (MonoBehaviour)go.AddComponent(t);
            }
        }
        if (comp == null) return false;

        bool handled = TryInvokeShow(comp);
        if (!handled) handled |= TrySetVisibleFlag(comp, true);
        if (!handled)
        {
            comp.gameObject.SetActive(true);
            handled = true;
        }
        if (handled)
            Debug.Log($"[AppBootstrap] Intro overlay shown via '{comp.GetType().Name}' ({reason}).");
        return handled;
    }

    static MonoBehaviour FindIntroComponentInScene()
    {
        var all = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var mb in all)
        {
            var n = mb.GetType().Name.ToLowerInvariant();
            if (IntroTypeHints.Any(h => n.Contains(h))) return mb;
        }
        return null;
    }

    static Type FindIntroType()
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try { types = asm.GetTypes(); } catch { continue; }
            foreach (var t in types)
            {
                if (!typeof(MonoBehaviour).IsAssignableFrom(t)) continue;
                var n = t.Name.ToLowerInvariant();
                if (IntroTypeHints.Any(h => n.Contains(h))) return t;
            }
        }
        return null;
    }

    static bool TryGetVisibleFlag(MonoBehaviour comp, out bool value)
    {
        value = false;
        var t = comp.GetType();
        foreach (var name in VisibleFieldNames)
        {
            var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null && f.FieldType == typeof(bool))
            {
                value = (bool)f.GetValue(comp);
                return true;
            }
            var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null && p.PropertyType == typeof(bool) && p.CanRead)
            {
                value = (bool)p.GetValue(comp, null);
                return true;
            }
        }
        return false;
    }

    static bool TrySetVisibleFlag(MonoBehaviour comp, bool visible)
    {
        var t = comp.GetType();
        foreach (var name in VisibleFieldNames)
        {
            var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null && f.FieldType == typeof(bool))
            {
                f.SetValue(comp, visible);
                return true;
            }
            var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null && p.PropertyType == typeof(bool) && p.CanWrite)
            {
                p.SetValue(comp, visible, null);
                return true;
            }
        }
        return false;
    }

    static bool TryInvokeShow(MonoBehaviour comp)
    {
        var t = comp.GetType();
        foreach (var m in ShowMethodNames)
        {
            var mi = t.GetMethod(m, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            if (mi != null) { mi.Invoke(comp, null); return true; }

            var miBool = t.GetMethod(m, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(bool) }, null);
            if (miBool != null) { miBool.Invoke(comp, new object[] { true }); return true; }
        }
        return false;
    }

    static bool TryInvokeHide(MonoBehaviour comp)
    {
        var t = comp.GetType();
        foreach (var m in HideMethodNames)
        {
            var mi = t.GetMethod(m, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            if (mi != null) { mi.Invoke(comp, null); return true; }

            var miBool = t.GetMethod(m, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(bool) }, null);
            if (miBool != null) { miBool.Invoke(comp, new object[] { false }); return true; }
        }
        return false;
    }

    static void MakeRunner()
    {
        if (UnityEngine.Object.FindFirstObjectByType<AppBootstrapRunner>() != null) return;
        var go = new GameObject("AppBootstrap (Auto)");
        UnityEngine.Object.DontDestroyOnLoad(go);
        go.AddComponent<AppBootstrapRunner>();
    }

    /// <summary>Hidden runner to perform a delayed re-check after boot (next frame).</summary>
    sealed class AppBootstrapRunner : MonoBehaviour
    {
        void Start() { StartCoroutine(DelayedCheck()); }
        System.Collections.IEnumerator DelayedCheck()
        {
            yield return null; // next frame
            TryShowIntroOverlay("DelayedCheck");
        }
    }
}

