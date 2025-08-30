using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Lightweight pause menu overlay. ESC toggles. Provides Restart and Exit.
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    public static bool IsPaused { get; private set; }
    const string CanvasName = "PauseCanvas (Auto)";

    Canvas _canvas;
    GameObject _root;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        // Create a driver that survives scene loads
        PauseMenuController existing = null;
#if UNITY_2023_1_OR_NEWER
        existing = FindFirstObjectByType<PauseMenuController>();
#else
        existing = FindObjectOfType<PauseMenuController>();
#endif
        if (existing == null)
        {
            var go = new GameObject("PauseMenuController");
            DontDestroyOnLoad(go);
            go.AddComponent<PauseMenuController>();
        }
    }

    void Update()
    {
        // Esc toggles pause
        bool esc = false;
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null) esc |= Keyboard.current.escapeKey.wasPressedThisFrame;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER || !ENABLE_INPUT_SYSTEM
        esc |= Input.GetKeyDown(KeyCode.Escape);
#endif
        if (esc) Toggle();
    }

    public void Toggle()
    {
        if (!IsPaused) Show();
        else Hide();
    }

    public void Show()
    {
        EnsureUI();
        _root.SetActive(true);
        Time.timeScale = 0f;
        IsPaused = true;
        Debug.Log("[Pause] Shown");
    }

    public void Hide()
    {
        if (_root != null) _root.SetActive(false);
        Time.timeScale = 1f;
        IsPaused = false;
        Debug.Log("[Pause] Hidden");
    }

    void EnsureUI()
    {
        if (_root != null) return;

        // Ensure EventSystem
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem));
            es.AddComponent<StandaloneInputModule>();
            DontDestroyOnLoad(es);
        }

        _root = new GameObject(CanvasName, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        DontDestroyOnLoad(_root);
        _canvas = _root.GetComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 9000;
        var scaler = _root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Backdrop
        var backdrop = new GameObject("Backdrop", typeof(Image));
        backdrop.transform.SetParent(_root.transform, false);
        var bImg = backdrop.GetComponent<Image>();
        bImg.color = new Color(0f, 0f, 0f, 0.5f);
        var bRt = backdrop.GetComponent<RectTransform>();
        bRt.anchorMin = Vector2.zero; bRt.anchorMax = Vector2.one;
        bRt.offsetMin = Vector2.zero; bRt.offsetMax = Vector2.zero;

        // Panel
        var panel = new GameObject("Panel", typeof(Image));
        panel.transform.SetParent(_root.transform, false);
        var pImg = panel.GetComponent<Image>();
        pImg.color = new Color(0.15f, 0.15f, 0.18f, 0.95f);
        var pRt = panel.GetComponent<RectTransform>();
        pRt.sizeDelta = new Vector2(520, 280);
        pRt.anchorMin = new Vector2(0.5f, 0.5f);
        pRt.anchorMax = new Vector2(0.5f, 0.5f);
        pRt.pivot = new Vector2(0.5f, 0.5f);
        pRt.anchoredPosition = Vector2.zero;

        // Title
        var title = BuildText(panel.transform, "Paused", 36, new Vector2(0, 60));

        // Buttons
        var restart = BuildButton(panel.transform, "Restart", new Vector2(0, -10));
        restart.onClick.AddListener(() =>
        {
            Hide();
            HardRestart.RebootToIntro();
        });
        var exit = BuildButton(panel.transform, "Exit", new Vector2(0, -80));
        exit.onClick.AddListener(() =>
        {
#if UNITY_EDITOR
            Debug.Log("[Pause] Exit requested (Application.Quit ignored in Editor).");
#else
            Application.Quit();
#endif
        });

        _root.SetActive(false);
    }

    Button BuildButton(Transform parent, string label, Vector2 offset)
    {
        var go = new GameObject(label + "Button", typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(360, 56);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = offset;
        var img = go.GetComponent<Image>();
        img.color = new Color(0.8f, 0.84f, 0.92f, 1f);
        var txt = BuildText(go.transform, label, 24, Vector2.zero);
        txt.color = Color.black;
        return go.GetComponent<Button>();
    }

    Text BuildText(Transform parent, string s, int size, Vector2 offset)
    {
        var go = new GameObject("Text", typeof(Text));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 60);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
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
}

