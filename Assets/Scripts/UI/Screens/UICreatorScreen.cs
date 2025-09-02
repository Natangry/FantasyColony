using System;
using UnityEngine;
using UnityEngine.UI;
using FantasyColony.UI.Router;
using FantasyColony.UI.Widgets;
using UnityObject = UnityEngine.Object;

namespace FantasyColony.UI.Screens
{
    /// <summary>
    /// UI Creator (Stub) — Step 1: dev entry & shell scaffold.
    /// Runtime-only, built via UIFactory. Structural containers use PanelSizing.Flexible.
    /// </summary>
    public sealed class UICreatorScreen : IScreen
    {
        public static bool IsOpen { get; private set; }
        private RectTransform _root;

        // Program-window layout: Top toolbar (percent height) + large blank stage
        private RectTransform _toolbar;
        private RectTransform _stage;
        private const float TOOLBAR_FRAC = 0.05f; // 5% of screen height

        public void Enter(Transform parent)
        {
            // Root (full-screen)
            var go = new GameObject("UICreator", typeof(RectTransform));
            _root = go.GetComponent<RectTransform>();
            _root.SetParent(parent, false);
            _root.anchorMin = Vector2.zero;
            _root.anchorMax = Vector2.one;
            _root.offsetMin = Vector2.zero;
            _root.offsetMax = Vector2.zero;

            Debug.Log("[UICreator] Enter");

            // Board: neutral dev look; padded content for borders, but anchors must live under a layout-free layer.
            var board = UIFactory.CreateBoardScreen(_root, padding: 8, spacing: 0);

            // Create a layout-free absolute layer under BoardRoot to host anchor-based regions
            var absGO = new GameObject("AbsoluteLayer", typeof(RectTransform));
            var absRT = absGO.GetComponent<RectTransform>();
            absRT.SetParent(board.Root, false);
            absRT.anchorMin = Vector2.zero;
            absRT.anchorMax = Vector2.one;
            absRT.offsetMin = Vector2.zero;
            absRT.offsetMax = Vector2.zero;

            // Percent-height anchors for Toolbar/Stage (under the absolute, layout-free layer)
            // Toolbar: make it OWN the HorizontalLayoutGroup; avoid nested layout parents that collapse width
            var toolbarGO = new GameObject("Toolbar", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
            _toolbar = toolbarGO.GetComponent<RectTransform>();
            var toolbarImg = toolbarGO.GetComponent<Image>();
            toolbarImg.color = new Color(0f, 0f, 0f, 1f); // solid bar; adjust in future if needed
            toolbarImg.raycastTarget = true;
            var toolbarHL = toolbarGO.GetComponent<HorizontalLayoutGroup>();
            toolbarHL.padding = new RectOffset(0,0,0,0);
            toolbarHL.spacing = 0f;
            toolbarHL.childControlWidth = true;
            toolbarHL.childForceExpandWidth = true; // split width evenly via button flexibleWidth=1
            toolbarHL.childControlHeight = true;
            toolbarHL.childForceExpandHeight = true;
            toolbarHL.childAlignment = TextAnchor.MiddleCenter;
            _toolbar.SetParent(absRT, false);

            // Stage should be a BLANK surface (no frame/border) to avoid artifacts in the center
            var stageGO = new GameObject("CanvasStage", typeof(RectTransform), typeof(Image));
            _stage = stageGO.GetComponent<RectTransform>();
            var stageImg = stageGO.GetComponent<Image>();
            stageImg.color = new Color(0f, 0f, 0f, 0f); // transparent
            stageImg.raycastTarget = false;
            _stage.SetParent(absRT, false);

            // Anchors: toolbar = top 5% height, stage = remaining 95%
            SetAnchorsPercent(_toolbar, xMin:0f, xMax:1f, yMin:1f-TOOLBAR_FRAC, yMax:1f);
            SetAnchorsPercent(_stage,   xMin:0f, xMax:1f, yMin:0f,              yMax:1f-TOOLBAR_FRAC);

            // --- Toolbar content: equal-width buttons across full width ---
            // Create equal-width buttons directly under the toolbar (no nested Row)
            CreateFlexMenuButton(_toolbar, "File",  () => {});
            CreateFlexMenuButton(_toolbar, "Edit",  () => {});
            CreateFlexMenuButton(_toolbar, "View",  () => {});
            CreateFlexMenuButton(_toolbar, "Tools", () => {});
            CreateFlexMenuButton(_toolbar, "Help",  () => {});
            CreateFlexMenuButton(_toolbar, "Close", () => UIRouter.Current?.Pop());

            IsOpen = true;
        }

        public void Exit()
        {
            if (_root != null)
            {
                Debug.Log("[UICreator] Exit");
                UnityObject.Destroy(_root.gameObject);
                _root = null;
            }
            IsOpen = false;
        }

        // --- helpers ---
        private static void CreateFlexMenuButton(Transform parent, string label, Action onClick)
        {
            var btn = UIFactory.CreateButtonPrimary(parent, label, onClick);
            var le = btn.GetComponent<LayoutElement>() ?? btn.gameObject.AddComponent<LayoutElement>();
            le.minWidth = 0f;
            le.preferredWidth = 0f;
            le.flexibleWidth = 1f; // Equal slice of the row

            var txt = btn.GetComponentInChildren<Text>();
            if (txt != null)
            {
                txt.resizeTextForBestFit = true;
                txt.resizeTextMinSize = 12;
                txt.resizeTextMaxSize = 28;
            }
        }

        private static void SetAnchorsPercent(RectTransform rt, float xMin, float xMax, float yMin, float yMax)
        {
            if (rt == null) return;
            rt.anchorMin = new Vector2(xMin, yMin);
            rt.anchorMax = new Vector2(xMax, yMax);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void RemoveHeaderIfExists(RectTransform panel)
        {
            if (panel == null) return;
            var header = panel.Find("Header");
            if (header != null)
            {
                UnityObject.Destroy(header.gameObject);
            }
        }

        private static void ConfigureColumn(RectTransform col, RectOffset padding, float spacing)
        {
            var vl = col.GetComponent<VerticalLayoutGroup>();
            if (vl != null)
            {
                vl.padding = padding;
                vl.spacing = spacing;
                vl.childControlHeight = true;
                vl.childForceExpandHeight = true;
            }
        }

        private static void EnsureMinWidth(RectTransform col, float minWidth)
        {
            var le = col.GetComponent<LayoutElement>();
            if (le == null) le = col.gameObject.AddComponent<LayoutElement>();
            if (le != null)
            {
                if (le.minWidth < minWidth) le.minWidth = minWidth;
            }
        }

        private static Text CreateHeaderLabel(Transform parent, string text)
        {
            var go = new GameObject("Header", typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.text = text;
            t.fontSize = 20;
            t.fontStyle = FontStyle.Bold;
            t.alignment = TextAnchor.MiddleLeft;
            t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.color = new Color(0.92f, 0.88f, 0.78f, 1f);
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = 28f;
            return t;
        }
    }
}
