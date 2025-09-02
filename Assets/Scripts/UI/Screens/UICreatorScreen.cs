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

            // Board: neutral dev look; padded content
            var board = UIFactory.CreateBoardScreen(_root, padding: 8, spacing: 0);

            // Percent-height anchors for Toolbar/Stage
            _toolbar = UIFactory.CreatePanelSurface(board.Content, "Toolbar");
            _stage   = UIFactory.CreatePanelSurface(board.Content, "CanvasStage");

            // Anchors: toolbar = top 5% height, stage = remaining 95%
            SetAnchorsPercent(_toolbar,   xMin:0f, xMax:1f, yMin:1f-TOOLBAR_FRAC, yMax:1f);
            SetAnchorsPercent(_stage,     xMin:0f, xMax:1f, yMin:0f,              yMax:1f-TOOLBAR_FRAC);

            // --- Toolbar content: equal-width buttons across full width ---
            var bar = UIFactory.CreateRow(_toolbar, spacing: 0f);
            var hl = bar.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            if (hl != null)
            {
                hl.padding = new RectOffset(0,0,0,0);
                hl.childControlWidth = true;
                hl.childForceExpandWidth = true; // divide width evenly via flexible widths
                hl.childControlHeight = true;
                hl.childForceExpandHeight = true;
                hl.childAlignment = TextAnchor.MiddleCenter;
            }

            // Create equal-width buttons (width/N)
            CreateFlexMenuButton(bar, "File",  () => {});
            CreateFlexMenuButton(bar, "Edit",  () => {});
            CreateFlexMenuButton(bar, "View",  () => {});
            CreateFlexMenuButton(bar, "Tools", () => {});
            CreateFlexMenuButton(bar, "Help",  () => {});
            CreateFlexMenuButton(bar, "Close", () => UIRouter.Current?.Pop());

            // Stage: remove header for blank area and ensure it fills
            RemoveHeaderIfExists(_stage);

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
        private static Button CreateFlexMenuButton(Transform parent, string label, Action onClick)
        {
            var btn = UIFactory.CreateButtonSecondary(parent, label, onClick);
            var le = btn.GetComponent<LayoutElement>() ?? btn.gameObject.AddComponent<LayoutElement>();
            le.flexibleWidth = 1f;
            le.minWidth = 0f;
            return btn;
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
