using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
// Only available when the new Input System package/define is enabled
using UnityEngine.InputSystem.UI;
#endif

namespace FantasyColony.UI.Root
{
    public sealed class UIRoot : MonoBehaviour
    {
        public RectTransform ScreenParent { get; private set; }

        public static event System.Action OnUpdate;

        public static UIRoot Create(Transform parent)
        {
            var go = new GameObject("UIRoot");
            go.transform.SetParent(parent, false);
            var root = go.AddComponent<UIRoot>();
            root.Build();
            return root;
        }

        private void Build()
        {
            // Canvas
            var canvasGO = new GameObject("Canvas");
            canvasGO.transform.SetParent(transform, false);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // Ensure pixel-accurate rounding to avoid uneven 1px borders on sliced images
            canvas.pixelPerfect = true; // ScreenSpaceOverlay only
            // Ensure a GraphicRaycaster exists
            if (canvasGO.GetComponent<GraphicRaycaster>() == null)
                canvasGO.AddComponent<GraphicRaycaster>();

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // EventSystem if missing
            bool hasEventSystem = EventSystem.current != null;
#if UNITY_2023_1_OR_NEWER
            if (!hasEventSystem)
                hasEventSystem = (Object.FindFirstObjectByType<EventSystem>() != null);
#else
            if (!hasEventSystem)
                hasEventSystem = (FindObjectOfType<EventSystem>() != null);
#endif
            // Create or sanitize EventSystem + input module (exactly one module)
            EventSystem esys = EventSystem.current;
            if (!hasEventSystem)
            {
                esys = new GameObject("EventSystem").AddComponent<EventSystem>();
                Debug.Log("[UIRoot] Created EventSystem");
            }

#if ENABLE_INPUT_SYSTEM
            // New Input System
            var oldMod = esys.GetComponent<StandaloneInputModule>();
            if (oldMod != null) Destroy(oldMod);
            if (esys.GetComponent<InputSystemUIInputModule>() == null)
                esys.gameObject.AddComponent<InputSystemUIInputModule>();
#else
            // Legacy Input (Standalone)
            // Remove any leftover InputSystemUIInputModule via reflection so this compiles
            // even if the new Input System package is not installed.
            var newInputModuleType = System.Type.GetType(
                "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (newInputModuleType != null)
            {
                var newMod = esys.GetComponent(newInputModuleType);
                if (newMod != null)
                    Destroy(newMod);
            }
            if (esys.GetComponent<StandaloneInputModule>() == null)
                esys.gameObject.AddComponent<StandaloneInputModule>();
#endif

            // Screen parent
            var screenParentGO = new GameObject("Screens");
            screenParentGO.transform.SetParent(canvasGO.transform, false);
            ScreenParent = screenParentGO.AddComponent<RectTransform>();
            ScreenParent.anchorMin = Vector2.zero;
            ScreenParent.anchorMax = Vector2.one;
            ScreenParent.offsetMin = Vector2.zero;
            ScreenParent.offsetMax = Vector2.zero;
        }

        private void Update()
        {
            OnUpdate?.Invoke();
        }
    }
}
