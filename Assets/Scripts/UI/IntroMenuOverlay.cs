using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using UnityEngine.EventSystems;

/// <summary>
/// Scene-less Intro overlay (uGUI) with Map Size presets and Start/Quit.
/// Large is default; Huge is available.
/// </summary>
public class IntroMenuOverlay : MonoBehaviour
{
    public static bool IsOpen => _root != null && _root.activeSelf;

    enum MapSize { Small, Medium, Large, Huge }
    static readonly Vector2Int Small = new Vector2Int(64, 64);
    static readonly Vector2Int Medium = new Vector2Int(96, 96);
    static readonly Vector2Int Large = new Vector2Int(128, 128); // default
    static readonly Vector2Int Huge  = new Vector2Int(192, 192);

    static GameObject _root;
    static IntroMenuOverlay _beh; // runner for coroutines
    static MapSize _selected = MapSize.Large; // default

    public static void Ensure()
    {
        if (_root != null) return;
        EnsureEventSystem();
        BuildUI();
    }

    public static void Show()
    {
        HidePauseIfOpen();
        Ensure();
        _root.SetActive(true);
    }

    public static void Hide()
    {
        if (_root != null) _root.SetActive(false);
    }

    static void EnsureEventSystem()
    {
        var es = UnityEngine.Object.FindFirstObjectByType<EventSystem>();
        if (es == null)
        {
            var go = new GameObject("EventSystem", typeof(EventSystem));
            UnityEngine.Object.DontDestroyOnLoad(go);
            es = go.GetComponent<EventSystem>();
        }
        // Ensure appropriate input module
#if ENABLE_INPUT_SYSTEM
        var old = es.GetComponent<StandaloneInputModule>();
        if (old != null) UnityEngine.Object.Destroy(old);
        if (es.GetComponent<InputSystemUIInputModule>() == null)
            es.gameObject.AddComponent<InputSystemUIInputModule>();
#else
        if (es.GetComponent<StandaloneInputModule>() == null)
            es.gameObject.AddComponent<StandaloneInputModule>();
#endif
    }

    static void HidePauseIfOpen()
    {
        var pm = UnityEngine.Object.FindFirstObjectByType<PauseMenuController>();
        if (pm != null)
        {
            try { pm.Hide(); } catch { /* ignore */ }
        }
        // Ensure unpaused
        if (Time.timeScale != 1f)
            Time.timeScale = 1f;
    }

    static void BuildUI()
    {
        _root = new GameObject("IntroMenuOverlay", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        UnityEngine.Object.DontDestroyOnLoad(_root);
        var canvas = _root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10000;
        var scaler = _root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        var backdrop = new GameObject("Backdrop", typeof(Image));
        backdrop.transform.SetParent(_root.transform, false);
        var backImg = backdrop.GetComponent<Image>();
        backImg.color = new Color(0f, 0f, 0f, 0.6f);
        var backRt = backdrop.GetComponent<RectTransform>();
        backRt.anchorMin = Vector2.zero; backRt.anchorMax = Vector2.one;
        backRt.offsetMin = Vector2.zero; backRt.offsetMax = Vector2.zero;

        var panel = new GameObject("Panel", typeof(Image));
        panel.transform.SetParent(_root.transform, false);
        var pImg = panel.GetComponent<Image>();
        pImg.color = new Color(0.15f, 0.15f, 0.18f, 0.95f);
        var pRt = panel.GetComponent<RectTransform>();
        pRt.sizeDelta = new Vector2(640, 420);
        pRt.anchorMin = pRt.anchorMax = new Vector2(0.5f, 0.5f);
        pRt.pivot = new Vector2(0.5f, 0.5f);
        pRt.anchoredPosition = Vector2.zero;

        BuildText(panel.transform, "Fantasy Colony", 40, new Vector2(0, 140));
        BuildText(panel.transform, "Map Size", 24, new Vector2(0, 70));

        // Size buttons row
        var sizes = new GameObject("SizeRow");
        sizes.transform.SetParent(panel.transform, false);
        var sRt = sizes.AddComponent<RectTransform>();
        sRt.sizeDelta = new Vector2(560, 60);
        sRt.anchorMin = sRt.anchorMax = new Vector2(0.5f, 0.5f);
        sRt.pivot = new Vector2(0.5f, 0.5f);
        sRt.anchoredPosition = new Vector2(0, 20);

        var bSmall  = BuildToggleButton(sizes.transform, "Small\n64×64",  new Vector2(-210, 0), () => SetSize(MapSize.Small));
        var bMedium = BuildToggleButton(sizes.transform, "Medium\n96×96", new Vector2(-70,  0), () => SetSize(MapSize.Medium));
        var bLarge  = BuildToggleButton(sizes.transform, "Large\n128×128",new Vector2( 70,  0), () => SetSize(MapSize.Large));
        var bHuge   = BuildToggleButton(sizes.transform, "Huge\n192×192", new Vector2( 210, 0), () => SetSize(MapSize.Huge));

        // Initial visual state (Large by default)
        SetSize(MapSize.Large);

        // Start / Quit
        var start = BuildButton(panel.transform, "Start", new Vector2(0, -80));
        start.onClick.AddListener(() =>
        {
            var dims = GetDims(_selected);
            if (_beh == null) _beh = _root.AddComponent<IntroMenuOverlay>();
            _beh.StartCoroutine(StartWorldFlow(dims.x, dims.y, _selected));
        });

        var quit = BuildButton(panel.transform, "Quit", new Vector2(0, -150));
        quit.onClick.AddListener(() =>
        {
#if UNITY_EDITOR
            Debug.Log("[IntroMenu] Quit requested (ignored in Editor).");
#else
            Application.Quit();
#endif
        });
    }

    static Vector2Int GetDims(MapSize s)
    {
        switch (s)
        {
            case MapSize.Small: return Small;
            case MapSize.Medium: return Medium;
            case MapSize.Large: return Large;
            case MapSize.Huge: return Huge;
        }
        return Large;
    }

    static void SetSize(MapSize s)
    {
        _selected = s;
        // Update visuals
        var row = _root.transform.Find("Panel/SizeRow");
        if (row == null) return;
        for (int i = 0; i < row.childCount; i++)
        {
            var btn = row.GetChild(i).GetComponent<Image>();
            if (btn == null) continue;
            bool on =
                (i == 0 && s == MapSize.Small) ||
                (i == 1 && s == MapSize.Medium) ||
                (i == 2 && s == MapSize.Large) ||
                (i == 3 && s == MapSize.Huge);
            Highlight(btn, on);
        }
    }

    static void Highlight(Image img, bool on)
    {
        if (img == null) return;
        img.color = on ? new Color(0.95f, 0.88f, 0.55f, 1f) : new Color(0.8f, 0.84f, 0.92f, 1f);
    }

    static Button BuildToggleButton(Transform parent, string label, Vector2 offset, Action onClick)
    {
        var go = new GameObject(label.Replace("\n", " ") + "Toggle", typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(120, 60);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = offset;
        var img = go.GetComponent<Image>();
        img.color = new Color(0.8f, 0.84f, 0.92f, 1f);
        var txt = BuildText(go.transform, label, 18, Vector2.zero);
        txt.color = Color.black;
        var btn = go.GetComponent<Button>();
        btn.onClick.AddListener(() => onClick?.Invoke());
        return btn;
    }

    static Button BuildButton(Transform parent, string label, Vector2 offset)
    {
        var go = new GameObject(label + "Button", typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(360, 56);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = offset;
        var img = go.GetComponent<Image>();
        img.color = new Color(0.8f, 0.84f, 0.92f, 1f);
        var txt = BuildText(go.transform, label, 24, Vector2.zero);
        txt.color = Color.black;
        return go.GetComponent<Button>();
    }

    static Text BuildText(Transform parent, string s, int size, Vector2 offset)
    {
        var go = new GameObject("Text", typeof(Text));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(440, 50);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = offset;
        var t = go.GetComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.fontSize = size;
        t.alignment = TextAnchor.MiddleCenter;
        t.text = s;
        t.raycastTarget = false;
        return t;
    }

    static string PresetName(MapSize s) => s.ToString();

    static Type FindTypeByNameContains(string token)
    {
        token = token.ToLowerInvariant();
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try { types = asm.GetTypes(); } catch { continue; }
            foreach (var t in types)
            {
                if (t.Name.ToLowerInvariant().Contains(token))
                    return t;
            }
        }
        return null;
    }

    // ---------------- Robust start flow ----------------

    IEnumerator StartWorldFlow(int w, int h, MapSize preset)
    {
        // 1) Try to start world using robust discovery
        string selected = null;
        bool invoked = TryStartWorldRobust(w, h, preset, out selected);
        if (!invoked)
        {
            Debug.LogError("[IntroMenu] Could not find a world start method. Intro will remain visible.");
            yield break;
        }
        Debug.Log("[IntroMenu] Invoked: " + selected);

        // 2) Wait a frame to let world construct
        yield return null;

        // 3) Verify grid exists
        var grid = UnityEngine.Object.FindFirstObjectByType<SimpleGridMap>();
        if (grid == null)
        {
            Debug.LogError("[IntroMenu] World grid not detected after start call. Intro will remain visible.");
            yield break;
        }

        // 4) Try to spawn test pawns if not already present
        if (!HasAnyPawns())
        {
            TrySpawnPawnsRobust(2);
            yield return null;
        }

        // 5) Hide intro (success)
        Hide();
    }

    static bool HasAnyPawns()
    {
        var all = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var mb in all)
        {
            var n = mb.GetType().Name.ToLowerInvariant();
            if (n.Contains("pawn") || n.Contains("agent") || n.Contains("actor"))
                return true;
        }
        return false;
    }

    static bool TryStartWorldRobust(int w, int h, MapSize preset, out string selected)
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

        // List candidates in deterministic order (typeOrder × methodOrder)
        var asms = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var tName in typeOrder)
        {
            var t = FindExactType(asms, tName);
            if (t == null) continue;
            foreach (var mName in methodOrder)
            {
                // Try signatures in order: (int,int), (Vector2Int), (), (string)
                var mi_ii = t.GetMethod(mName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(int), typeof(int) }, null);
                if (mi_ii != null)
                {
                    mi_ii.Invoke(null, new object[] { w, h });
                    selected = $"{t.Name}.{mName}(int,int)";
                    return true;
                }
                var vec2 = typeof(Vector2Int);
                var mi_v = t.GetMethod(mName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, new[] { vec2 }, null);
                if (mi_v != null)
                {
                    var size = new Vector2Int(w, h);
                    mi_v.Invoke(null, new object[] { size });
                    selected = $"{t.Name}.{mName}(Vector2Int)";
                    return true;
                }
                var mi_0 = t.GetMethod(mName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, Type.EmptyTypes, null);
                if (mi_0 != null)
                {
                    mi_0.Invoke(null, null);
                    selected = $"{t.Name}.{mName}()";
                    return true;
                }
                var mi_s = t.GetMethod(mName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(string) }, null);
                if (mi_s != null)
                {
                    mi_s.Invoke(null, new object[] { PresetName(preset) });
                    selected = $"{t.Name}.{mName}(string)";
                    return true;
                }
            }
        }

        // If nothing matched in the preferred type list, do a broader sweep across all types named *Bootstrap* or *Generator*
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
                    var mi_ii = t.GetMethod(mName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(int), typeof(int) }, null);
                    if (mi_ii != null)
                    {
                        mi_ii.Invoke(null, new object[] { w, h });
                        selected = $"{t.Name}.{mName}(int,int)";
                        return true;
                    }
                    var vec2 = typeof(Vector2Int);
                    var mi_v = t.GetMethod(mName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, new[] { vec2 }, null);
                    if (mi_v != null)
                    {
                        var size = new Vector2Int(w, h);
                        mi_v.Invoke(null, new object[] { size });
                        selected = $"{t.Name}.{mName}(Vector2Int)";
                        return true;
                    }
                    var mi_0 = t.GetMethod(mName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, Type.EmptyTypes, null);
                    if (mi_0 != null)
                    {
                        mi_0.Invoke(null, null);
                        selected = $"{t.Name}.{mName}()";
                        return true;
                    }
                    var mi_s = t.GetMethod(mName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(string) }, null);
                    if (mi_s != null)
                    {
                        mi_s.Invoke(null, new object[] { PresetName(preset) });
                        selected = $"{t.Name}.{mName}(string)";
                        return true;
                    }
                }
            }
        }

        return false;
    }

    static void TrySpawnPawnsRobust(int desiredCount)
    {
        string[] typeHints = { "Pawn", "Actor", "Agent", "Spawner", "Bootstrap" };
        string[] methodNames = { "SpawnTestPawns", "SpawnDefaultPawns", "SpawnPawns", "CreateTestActors", "CreateDefaultPawns" };

        var asms = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var asm in asms)
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
                    if (mi_i != null)
                    {
                        mi_i.Invoke(null, new object[] { desiredCount });
                        Debug.Log($"[IntroMenu] Invoked {t.Name}.{mn}(int) to spawn pawns.");
                        return;
                    }
                    var mi_0 = t.GetMethod(mn, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, Type.EmptyTypes, null);
                    if (mi_0 != null)
                    {
                        mi_0.Invoke(null, null);
                        Debug.Log($"[IntroMenu] Invoked {t.Name}.{mn}() to spawn pawns.");
                        return;
                    }
                }
            }
        }
        // Not fatal—map is up; just no test pawns
        Debug.Log("[IntroMenu] No pawn spawner entrypoint found (map started).");
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

