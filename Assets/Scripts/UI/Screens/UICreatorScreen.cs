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
        private RectTransform _root;

        // Columns
        private RectTransform _left;
        private RectTransform _center;
        private RectTransform _right;

        // Ratios for left/center/right widths (sum is arbitrary; layout normalizes)
        private const float LEFT_RATIO = 0.9f;
        private const float CENTER_RATIO = 2.2f;
        private const float RIGHT_RATIO = 0.9f;

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

            // Columns: Flexible surfaces joined to avoid double seams
            _left  = UIFactory.CreateColumn(board.Content, "Palette", preferredWidth: -1, flexibleWidth: LEFT_RATIO).GetComponent<RectTransform>();
            _center= UIFactory.CreateColumn(board.Content, "Canvas",  preferredWidth: -1, flexibleWidth: CENTER_RATIO).GetComponent<RectTransform>();
            _right = UIFactory.CreateColumn(board.Content, "Inspector",preferredWidth: -1, flexibleWidth: RIGHT_RATIO).GetComponent<RectTransform>();
            UIFactory.JoinHorizontal(_left, _center);
            UIFactory.JoinHorizontal(_center, _right);

            // Configure columns (VerticalLayoutGroups already exist; do not add duplicates)
            ConfigureColumn(_left,   padding: new RectOffset(12,12,12,12), spacing: 8f);
            ConfigureColumn(_center, padding: new RectOffset(12,12,12,12), spacing: 8f);
            ConfigureColumn(_right,  padding: new RectOffset(12,12,12,12), spacing: 8f);

            // Headers
            CreateHeaderLabel(_left,   "Palette");
            CreateHeaderLabel(_center, "Canvas");
            CreateHeaderLabel(_right,  "Inspector");

            // Right toolbar: Close button (also accessible via F10)
            var topRow = UIFactory.CreateRow(_right, spacing: 8f);
            var closeBtn = UIFactory.CreateButtonSecondary(topRow, "Close", () => UIRouter.Current?.Pop());
            var le = closeBtn.GetComponent<LayoutElement>();
            if (le == null) le = closeBtn.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = 120f;

            // Placeholders in each column (simple framed panels)
            UIFactory.CreatePanelSurface(_left, "PalettePanel");
            UIFactory.CreatePanelSurface(_center, "CanvasPanel");
            UIFactory.CreatePanelSurface(_right, "InspectorPanel");
        }

        public void Exit()
        {
            if (_root != null)
            {
                Debug.Log("[UICreator] Exit");
                UnityObject.Destroy(_root.gameObject);
                _root = null;
            }
        }

        // --- helpers ---
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
