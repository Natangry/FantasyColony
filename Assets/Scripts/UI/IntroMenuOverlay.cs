using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

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
    static MapSize _selected = MapSize.Large; // default

    public static void Ensure()
    {
        if (_root != null) return;
        BuildUI();
    }

    public static void Show()
    {
        Ensure();
        _root.SetActive(true);
    }

    public static void Hide()
    {
        if (_root != null) _root.SetActive(false);
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
            TryStartWorld(dims.x, dims.y);
            Hide();
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

    static void TryStartWorld(int w, int h)
    {
        // Preferred path: WorldBootstrap.GenerateDefaultGrid(int,int)
        var wbType = FindTypeByNameContains("WorldBootstrap");
        if (wbType != null)
        {
            var mi = wbType.GetMethod("GenerateDefaultGrid", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(int), typeof(int) }, null);
            if (mi != null)
            {
                Debug.Log($"[IntroMenu] Starting world via WorldBootstrap.GenerateDefaultGrid({w},{h})");
                mi.Invoke(null, new object[] { w, h });
                return;
            }
        }

        // Fallbacks: common method names on any bootstrap-like type
        string[] names = { "GenerateWorld", "CreateWorld", "StartNewGame", "StartGame", "BootWorld" };
        var tb = FindTypeByNameContains("Bootstrap");
        if (tb != null)
        {
            foreach (var n in names)
            {
                var mi = tb.GetMethod(n, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(int), typeof(int) }, null)
                      ?? tb.GetMethod(n, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, Type.EmptyTypes, null);
                if (mi != null)
                {
                    Debug.Log($"[IntroMenu] Starting world via {tb.Name}.{n}({(mi.GetParameters().Length == 2 ? $"{w},{h}" : "")})");
                    var args = mi.GetParameters().Length == 2 ? new object[] { w, h } : null;
                    mi.Invoke(null, args);
                    return;
                }
            }
        }

        Debug.LogWarning("[IntroMenu] No world bootstrap method found; overlay will hide but no world was created.");
    }

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
}

