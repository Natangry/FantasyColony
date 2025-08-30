using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Creates a uGUI Build toggle (Canvas + Button) at runtime for gameplay scenes only.
/// Avoids IMGUI artifacts and scales with resolution automatically.
/// </summary>
public static class BuildUIBootstrap
{
    const string CanvasName = "BuildCanvas (Auto)";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        SceneManager.activeSceneChanged += OnSceneChanged;
        EnsureForCurrentScene();
    }

    static void OnSceneChanged(Scene from, Scene to)
    {
        EnsureForCurrentScene();
    }

    static bool IsIntroLike(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return false;
        var s = sceneName.ToLowerInvariant();
        return s.Contains("intro") || s.Contains("menu") || s.Contains("title");
    }

    static void EnsureForCurrentScene()
    {
        var active = SceneManager.GetActiveScene().name;
        var canvas = GameObject.Find(CanvasName);
        if (IsIntroLike(active))
        {
            // Destroy if present in intro/menu scenes
            if (canvas != null) Object.Destroy(canvas);
            return;
        }

        if (canvas == null)
        {
            BuildToggleCanvas();
        }
    }

    static void BuildToggleCanvas()
    {
        // Ensure there is an EventSystem
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Object.DontDestroyOnLoad(es);
        }

        var go = new GameObject(CanvasName, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Object.DontDestroyOnLoad(go);

        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5000; // draw on top of world

        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // Build button
        var btnGO = new GameObject("BuildButton", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(go.transform, false);
        var rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(16, -16);
        rt.sizeDelta = new Vector2(180, 48);

        var img = btnGO.GetComponent<Image>();
        img.color = new Color(0.85f, 0.85f, 0.85f, 0.9f);

        var btn = btnGO.GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            BuildBootstrap.Ensure();
            if (BuildModeController.Instance != null)
                BuildModeController.Instance.ToggleBuildMode();
        });

        // Label
        var txtGO = new GameObject("Text", typeof(RectTransform));
        txtGO.transform.SetParent(btnGO.transform, false);
        var trt = txtGO.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        // Use legacy Text if available; otherwise leave default button visuals
        var legacyText = txtGO.AddComponent<Text>();
        legacyText.text = "Build  [B]";
        legacyText.alignment = TextAnchor.MiddleCenter;
        legacyText.color = Color.black;
        legacyText.fontSize = 24;
        legacyText.raycastTarget = false;
        legacyText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    }
}
