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

        // Program-window layout: top toolbar + large blank stage
        private RectTransform _toolbar;
        private RectTransform _stage;

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

            // Board: tiled wood background + padded content area
            var board = UIFactory.CreateBoardScreen(_root, padding: 24, spacing: 0);

            // Root vertical stack: Toolbar (fixed height) + Stage (fills remaining)
            var rootCol = UIFactory.CreateCol(board.Content, spacing: 8f);
            var rootVL = rootCol.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
            if (rootVL != null)
            {
                rootVL.spacing = 8f;
                rootVL.childControlHeight = true;
                rootVL.childForceExpandHeight = true;
                // Stretch children horizontally to full width of the board
                rootVL.childControlWidth = true;
                rootVL.childForceExpandWidth = true;
            }

            // --- Toolbar ---
            _toolbar = UIFactory.CreatePanelSurface(rootCol, "Toolbar");
            var tLe = _toolbar.gameObject.GetComponent<LayoutElement>() ?? _toolbar.gameObject.AddComponent<LayoutElement>();
            tLe.minHeight = 40f; tLe.preferredHeight = 40f; tLe.flexibleHeight = 0f;
            var bar = UIFactory.CreateRow(_toolbar, spacing: 8f);
            // Ensure the row does NOT force-expand children horizontally; spacer will absorb extra space.
            var hl = bar.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            if (hl != null)
            {
                hl.childControlWidth = true;
                hl.childForceExpandWidth = false;
                hl.childControlHeight = true;
                hl.childForceExpandHeight = false;
                hl.childAlignment = TextAnchor.MiddleLeft;
            }

            // Fixed-size menu buttons
            CreateFixedMenuButton(bar, "File", 96f, null);
            CreateFixedMenuButton(bar, "Edit", 96f, null);
            CreateFixedMenuButton(bar, "View", 96f, null);
            CreateFixedMenuButton(bar, "Tools", 96f, null);
            CreateFixedMenuButton(bar, "Help", 96f, null);

            // Spacer pushes Close to the right
            CreateFlexSpacer(bar, 1f);

            var closeBtn = UIFactory.CreateButtonSecondary(bar, "Close", () => UIRouter.Current?.Pop());
            var closeLe = closeBtn.GetComponent<LayoutElement>();
            if (closeLe == null) closeLe = closeBtn.gameObject.AddComponent<LayoutElement>();
            closeLe.preferredWidth = 120f;
            closeLe.flexibleWidth = 0f;

            // --- Stage ---
            _stage = UIFactory.CreatePanelSurface(rootCol, "CanvasStage");
            var sLe = _stage.gameObject.GetComponent<LayoutElement>() ?? _stage.gameObject.AddComponent<LayoutElement>();
            sLe.flexibleHeight = 1f; sLe.flexibleWidth = 1f; sLe.minWidth = 0f;
            // Remove header label for a blank stage look
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
        private static Button CreateFixedMenuButton(Transform parent, string label, float width, Action onClick)
        {
            var btn = UIFactory.CreateButtonSecondary(parent, label, onClick);
            var le = btn.GetComponent<LayoutElement>() ?? btn.gameObject.AddComponent<LayoutElement>();
            le.minWidth = width;
            le.preferredWidth = width;
            le.flexibleWidth = 0f;
            return btn;
        }

        private static void CreateFlexSpacer(Transform parent, float flex)
        {
            UIFactory.CreateSpacer(parent, flex);
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
