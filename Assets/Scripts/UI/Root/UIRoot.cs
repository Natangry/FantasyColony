using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FantasyColony.UI.Root
{
    public sealed class UIRoot : MonoBehaviour
    {
        public RectTransform ScreenParent { get; private set; }

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
            canvasGO.AddComponent<GraphicRaycaster>();

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // EventSystem if missing
            bool hasEventSystem = EventSystem.current != null;
#if UNITY_2023_1_OR_NEWER
            if (!hasEventSystem)
            {
                hasEventSystem = (Object.FindFirstObjectByType<EventSystem>() != null);
            }
#else
            if (!hasEventSystem)
            {
                hasEventSystem = (FindObjectOfType<EventSystem>() != null);
            }
#endif

            if (!hasEventSystem)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }

            // Screen parent
            var screenParentGO = new GameObject("Screens");
            screenParentGO.transform.SetParent(canvasGO.transform, false);
            ScreenParent = screenParentGO.AddComponent<RectTransform>();
            ScreenParent.anchorMin = Vector2.zero;
            ScreenParent.anchorMax = Vector2.one;
            ScreenParent.offsetMin = Vector2.zero;
            ScreenParent.offsetMax = Vector2.zero;
        }
    }
}
