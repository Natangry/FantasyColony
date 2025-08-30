using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Minimal uGUI intro overlay used only if a real intro overlay cannot be found.
/// Provides Start (hides overlay, tries to call common "start game" bootstraps via reflection) and Quit.
/// </summary>
public class IntroOverlayFallback : MonoBehaviour
{
    static IntroOverlayFallback _instance;

    public static void SpawnIfNeeded()
    {
        if (_instance != null) { _instance.gameObject.SetActive(true); return; }
        var root = new GameObject("IntroOverlayFallback", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        DontDestroyOnLoad(root);
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10000;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        var back = new GameObject("Backdrop", typeof(Image));
        back.transform.SetParent(root.transform, false);
        var br = back.GetComponent<RectTransform>();
        br.anchorMin = Vector2.zero; br.anchorMax = Vector2.one;
        br.offsetMin = Vector2.zero; br.offsetMax = Vector2.zero;
        back.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);

        var panel = new GameObject("Panel", typeof(Image));
        panel.transform.SetParent(root.transform, false);
        panel.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.18f, 0.95f);
        var pr = panel.GetComponent<RectTransform>();
        pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
        pr.pivot = new Vector2(0.5f, 0.5f);
        pr.sizeDelta = new Vector2(520, 300);
        pr.anchoredPosition = Vector2.zero;

        var title = BuildText(panel.transform, "Fantasy Colony — Intro (Fallback)", 30, new Vector2(0, 70));

        var startBtn = BuildButton(panel.transform, "Start", new Vector2(0, 10));
        startBtn.onClick.AddListener(() =>
        {
            TryStartWorld();
            root.SetActive(false);
        });

        var quitBtn = BuildButton(panel.transform, "Quit", new Vector2(0, -60));
        quitBtn.onClick.AddListener(() =>
        {
#if UNITY_EDITOR
            Debug.Log("[IntroFallback] Quit requested (ignored in Editor).");
#else
            Application.Quit();
#endif
        });

        _instance = root.AddComponent<IntroOverlayFallback>();
    }

    public static void HideIfPresent()
    {
        if (_instance != null) _instance.gameObject.SetActive(false);
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
        rt.sizeDelta = new Vector2(400, 60);
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

    /// <summary>
    /// Try to invoke a "start world" call in your project, if one exists.
    /// Searches for common bootstrap method names and runs the first match.
    /// </summary>
    static void TryStartWorld()
    {
        string[] methodNames =
        {
            "GenerateDefaultGrid","StartNewGame","StartGame","NewGame","GenerateWorld","CreateWorld","BootWorld"
        };
        // Prefer types that look like world/game bootstraps
        Func<Type, bool> typeFilter = t =>
        {
            var n = t.Name.ToLowerInvariant();
            return (n.Contains("world") && n.Contains("bootstrap")) || n.Contains("gamebootstrap") || n.Contains("bootstrapworld");
        };
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try { types = asm.GetTypes(); } catch { continue; }
            foreach (var t in types.Where(typeFilter))
            {
                foreach (var mn in methodNames)
                {
                    var mi = t.GetMethod(mn, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, Type.EmptyTypes, null);
                    if (mi != null)
                    {
                        Debug.Log($"[IntroFallback] Invoking {t.Name}.{mn}()");
                        mi.Invoke(null, null);
                        return;
                    }
                }
            }
        }
        Debug.LogWarning("[IntroFallback] No world bootstrap method found. Overlay hidden only.");
    }
}

