using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FantasyColony.UI.Router; // IScreen lives here
using FantasyColony.UI.Widgets; // UIFactory
using FantasyColony.UI.Style;   // BaseUIStyle paths & themes

namespace FantasyColony.UI.Screens
{
    /// <summary>
    /// Runtime-built Mods screen scaffold (no data wiring yet).
    /// Left column is labeled "Mods"; right column title reflects the selected mod's name.
    /// If no mod is selected, the snapshot shows aggregated view of all active mods (placeholder for now).
    /// </summary>
    public class ModsScreen : IScreen
    {
        private RectTransform _root;

        // Left column
        private RectTransform _leftColumn;
        private InputField _leftSearch;
        private RectTransform _inactiveListContent;
        private RectTransform _activeListContent;

        // Right column
        private RectTransform _centerColumn;
        private RectTransform _rightColumn;
        private Text _centerTitle;
        private InputField _snapshotSearch;
        private RectTransform _scriptContent;
        private RectTransform _defsContent;

        // Flexible height weights for header/content rows
        private const float H_HEADER = 1f;
        private const float H_CONTENT = 5f;
        // Column widths are ratio-only; the board HLG will distribute the remaining width
        private const float LEFT_RATIO = 1.0f;
        private const float CENTER_RATIO = 2.2f;
        private const float RIGHT_RATIO = 0.8f;

        private const float LeftWidth = 380f;
        private const float RightWidth = 320f;
        // Selection state
        private string _selectedModName = null; // null => aggregated view of all active mods

        public void Enter(Transform parent)
        {

            _root = CreateUIObject("ModsScreenRoot", parent);
            Stretch(_root);
            // Full-screen board
            var board = UIFactory.CreateBoardScreen(_root, padding:32, spacing:0); // spacing=0 so equalities hold exactly

            // Top-level three columns; use flexible weights to satisfy w1 + w4 + w6 = screenWidth
            var left = UIFactory.CreateColumn(board.Content, "Left",  preferredWidth: -1, flexibleWidth: LEFT_RATIO).GetComponent<RectTransform>();
            var center= UIFactory.CreateColumn(board.Content, "Center",preferredWidth: -1, flexibleWidth: CENTER_RATIO).GetComponent<RectTransform>();
            var right = UIFactory.CreateColumn(board.Content, "Right", preferredWidth: -1, flexibleWidth: RIGHT_RATIO).GetComponent<RectTransform>();
            UIFactory.JoinHorizontal(left, center);
            UIFactory.JoinHorizontal(center, right);

            // LEFT COLUMN
            _leftColumn = left;
            UIFactory.SetPanelDecorVisible(_leftColumn, true);
            var leftLE = _leftColumn.GetComponent<LayoutElement>() ?? _leftColumn.gameObject.AddComponent<LayoutElement>();
            leftLE.preferredWidth = -1f;   // use ratio
            leftLE.flexibleWidth = LEFT_RATIO;

            var leftVL = _leftColumn.GetComponent<VerticalLayoutGroup>();
            if (leftVL != null)
            {
                leftVL.spacing = 0; // to honor exact equalities; inner panels have their own padding
                leftVL.padding = new RectOffset(12, 12, 12, 12);
                leftVL.childControlHeight = true;
                leftVL.childForceExpandHeight = true;
            }

            // Panel #1 (header)
            var p1 = UIFactory.CreatePanelSurface(_leftColumn, "P1_Header").GetComponent<RectTransform>();
            var p1LE = p1.GetComponent<LayoutElement>() ?? p1.gameObject.AddComponent<LayoutElement>();
            p1LE.flexibleHeight = H_HEADER; // h1
            BuildLeftHeader(p1);

            // Row for #2 and #3 (equal widths)
            var row23 = CreateUIObject("Row23", _leftColumn);
            var row23HL = row23.gameObject.AddComponent<HorizontalLayoutGroup>();
            row23HL.spacing = 0; row23HL.childForceExpandWidth = true; row23HL.childForceExpandHeight = true;
            var row23LE = row23.gameObject.AddComponent<LayoutElement>();
            row23LE.flexibleHeight = H_CONTENT; // this enforces h2 = h3 = h5 later

            var leftControls = CreateUIObject("LeftControls", p1);
            var lcHL = leftControls.gameObject.AddComponent<HorizontalLayoutGroup>();
            lcHL.childAlignment = TextAnchor.MiddleLeft;
            lcHL.spacing = 8f;
            lcHL.childForceExpandWidth = false;

            _leftSearch = CreateSearchField(leftControls, "Search mods...");
            var searchLE = _leftSearch.GetComponent<LayoutElement>();
            if (searchLE == null) searchLE = _leftSearch.gameObject.AddComponent<LayoutElement>();
            searchLE.preferredWidth = 220f;

            UIFactory.CreateButtonSecondary(leftControls, "Sort", () => { /* TODO: sort menu */ });

            // Panel #2 (inactive) – half width of #1
            var p2 = UIFactory.CreatePanelSurface(row23, "P2_Inactive").GetComponent<RectTransform>();
            var p2LE = p2.GetComponent<LayoutElement>() ?? p2.gameObject.AddComponent<LayoutElement>();
            p2LE.flexibleWidth = 1; // w2
            _inactiveListContent = CreateTitledScrollPanel_Styled(p2, "inactive mod list");
            CreateEmptyState(_inactiveListContent, "No inactive mods");

            // Panel #3 (active) – equals #2
            var p3 = UIFactory.CreatePanelSurface(row23, "P3_Active").GetComponent<RectTransform>();
            var p3LE = p3.GetComponent<LayoutElement>() ?? p3.gameObject.AddComponent<LayoutElement>();
            p3LE.flexibleWidth = 1; // w3, ensures w2 = w3 and w2 + w3 = w1
            _activeListContent = CreateTitledScrollPanel_Styled(p3, "active mod list");
            CreateEmptyState(_activeListContent, "No active mods");

            // CENTER COLUMN (snapshot)
            _centerColumn = center;
            UIFactory.SetPanelDecorVisible(_centerColumn, true);
            var centerLE = _centerColumn.GetComponent<LayoutElement>() ?? _centerColumn.gameObject.AddComponent<LayoutElement>();
            centerLE.preferredWidth = -1f; // use ratio
            centerLE.flexibleWidth = CENTER_RATIO;  // flexible middle column
            var centerVL = _centerColumn.GetComponent<VerticalLayoutGroup>();
            if (centerVL != null)
            {
                centerVL.spacing = 0;
                centerVL.padding = new RectOffset(12, 12, 12, 12);
                centerVL.childControlHeight = true;
                centerVL.childForceExpandHeight = true;
            }

            // Panel #4 (snapshot header)
            var p4 = UIFactory.CreatePanelSurface(_centerColumn, "P4_SnapshotHeader").GetComponent<RectTransform>();
            var p4LE = p4.GetComponent<LayoutElement>() ?? p4.gameObject.AddComponent<LayoutElement>();
            p4LE.flexibleHeight = H_HEADER; // h4 = h1
            var centerHeader = CreateUIObject("CenterHeader", p4);
            var chHL = centerHeader.gameObject.AddComponent<HorizontalLayoutGroup>();
            chHL.childAlignment = TextAnchor.MiddleLeft;
            chHL.spacing = 8f;
            chHL.childForceExpandWidth = true;

            _centerTitle = CreateTitleLabel(centerHeader, "All Active Mods");
            var spacerC = CreateFlexibleSpace(centerHeader);
            _snapshotSearch = CreateSearchField(centerHeader, "search snapshot...");
            var snapLE = _snapshotSearch.gameObject.AddComponent<LayoutElement>();
            snapLE.preferredWidth = 260f;

            // Panel #5 (snapshot content)
            var snapshotPanel = UIFactory.CreatePanelSurface(_centerColumn, "P5_Snapshot").GetComponent<RectTransform>();
            var p5LE = snapshotPanel.GetComponent<LayoutElement>() ?? snapshotPanel.gameObject.AddComponent<LayoutElement>();
            p5LE.flexibleHeight = H_CONTENT; // h5 = h2 = h3
            var spVL = snapshotPanel.GetComponent<VerticalLayoutGroup>();
            if (spVL != null)
            {
                spVL.spacing = 8f;
                spVL.padding = new RectOffset(12, 12, 12, 12);
                spVL.childControlHeight = false;
                spVL.childForceExpandHeight = false;
            }

            CreateLabel(snapshotPanel, "Snapshot (shows the contents of the mod in a user-friendly list)", 14, FontStyle.Italic, TextAnchor.MiddleLeft);

            // Foldouts
            CreateFoldout(snapshotPanel, "script +", out _scriptContent);
            CreateDivider(_scriptContent);
            CreateDivider(_scriptContent);
            CreateDivider(_scriptContent);
            UIFactory.CreateRuleHorizontal(snapshotPanel, 2f, 0.7f); // visual break inside panel #5 (this is 5b)
            CreateFoldout(snapshotPanel, "defs +", out _defsContent);
            CreateDivider(_defsContent);
            CreateDivider(_defsContent);
            CreateDivider(_defsContent);

            // RIGHT COLUMN (actions)
            _rightColumn = right;
            UIFactory.SetPanelDecorVisible(_rightColumn, true);
            var rightLE = _rightColumn.GetComponent<LayoutElement>() ?? _rightColumn.gameObject.AddComponent<LayoutElement>();
            rightLE.preferredWidth = -1f;
            rightLE.flexibleWidth = RIGHT_RATIO; // ratio-based right rail
            var rightVL = _rightColumn.GetComponent<VerticalLayoutGroup>();
            if (rightVL != null)
            {
                rightVL.spacing = 12f;
                rightVL.padding = new RectOffset(12, 12, 12, 12);
                rightVL.childControlHeight = false;
                rightVL.childForceExpandHeight = false;
            }

            UIFactory.CreateButtonSecondary(_rightColumn, "save mod list", () => { /* TODO */ });
            UIFactory.CreateButtonSecondary(_rightColumn, "load mod list", () => { /* TODO */ });
            // push primary button to bottom
            CreateFlexibleSpace(_rightColumn);
            UIFactory.CreateButtonPrimary(_rightColumn, "apply/restart", () =>
            {
                try { AppFlowCommands.Restart(); } catch { Debug.Log("Restart requested (AppFlowCommands.Restart missing in editor)"); }
            });

            // Initial snapshot state (aggregated view)
            RefreshSnapshot();
        }

        public void Exit()
        {
            if (_root != null)
            {
                GameObject.Destroy(_root.gameObject);
                _root = null;
            }
        }

        // --- Selection and snapshot ---
        private void SetSelectedMod(string modName)
        {
            _selectedModName = modName;
            _centerTitle.text = string.IsNullOrEmpty(_selectedModName) ? "All Active Mods" : _selectedModName;
            RefreshSnapshot();
        }

        private void RefreshSnapshot()
        {
            ClearChildren(_scriptContent);
            ClearChildren(_defsContent);

            if (string.IsNullOrEmpty(_selectedModName))
            {
                // Aggregated from active list (placeholder – no real data yet)
                CreateLabel(_scriptContent, "(Aggregated) scripts from all active mods", 12, FontStyle.Normal, TextAnchor.MiddleLeft);
                for (int i = 0; i < 3; i++) CreateDivider(_scriptContent);

                CreateLabel(_defsContent, "(Aggregated) defs from all active mods", 12, FontStyle.Normal, TextAnchor.MiddleLeft);
                for (int i = 0; i < 3; i++) CreateDivider(_defsContent);
            }
            else
            {
                // Selected mod only (placeholder)
                CreateLabel(_scriptContent, $"scripts in '{_selectedModName}'", 12, FontStyle.Normal, TextAnchor.MiddleLeft);
                for (int i = 0; i < 3; i++) CreateDivider(_scriptContent);

                CreateLabel(_defsContent, $"defs in '{_selectedModName}'", 12, FontStyle.Normal, TextAnchor.MiddleLeft);
                for (int i = 0; i < 3; i++) CreateDivider(_defsContent);
            }
        }

        // --- UI building helpers (lean, rely on layout components) ---
        private static RectTransform CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            return rt;
        }

        private static RectTransform AsRT(Component c)
        {
            return c is RectTransform r ? r : c.GetComponent<RectTransform>();
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static Text CreateLabel(Transform parent, string text, int size, FontStyle style, TextAnchor anchor)
        {
            var go = CreateUIObject("Label", parent);
            var t = go.gameObject.AddComponent<Text>();
            t.text = text;
            t.fontSize = size;
            t.alignment = anchor;
            t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.color = new Color(0.92f, 0.88f, 0.78f, 1f); // light text (fits base style)
            var le = go.gameObject.AddComponent<LayoutElement>();
            le.minHeight = 24f;
            return t;
        }

        private static Text CreateHeaderLabel(Transform parent, string text)
        {
            return CreateLabel(parent, text, 22, FontStyle.Bold, TextAnchor.MiddleLeft);
        }

        private void BuildLeftHeader(Transform parent)
        {
            CreateHeaderLabel(parent, "Mods");
        }

        private static Text CreateTitleLabel(Transform parent, string text)
        {
            return CreateLabel(parent, text, 20, FontStyle.Bold, TextAnchor.MiddleLeft);
        }

        private static InputField CreateSearchField(Transform parent, string placeholder)
        {
            var container = CreateUIObject("SearchField", parent);
            var bg = container.gameObject.AddComponent<Image>();
            bg.color = new Color(0.16f, 0.14f, 0.10f, 0.85f);
            var le = container.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 32f;
            le.flexibleWidth = 0f;

            var textGO = CreateUIObject("Text", container);
            var text = textGO.gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.color = new Color(0.92f, 0.88f, 0.78f, 1f);
            text.alignment = TextAnchor.MiddleLeft;
            var textRT = textGO;
            textRT.anchorMin = new Vector2(0, 0);
            textRT.anchorMax = new Vector2(1, 1);
            textRT.offsetMin = new Vector2(8, 6);
            textRT.offsetMax = new Vector2(-8, -6);

            var phGO = CreateUIObject("Placeholder", container);
            var placeholderText = phGO.gameObject.AddComponent<Text>();
            placeholderText.text = placeholder;
            placeholderText.font = text.font;
            placeholderText.color = new Color(1f, 1f, 1f, 0.35f);
            placeholderText.alignment = TextAnchor.MiddleLeft;
            var phRT = phGO;
            phRT.anchorMin = new Vector2(0, 0);
            phRT.anchorMax = new Vector2(1, 1);
            phRT.offsetMin = new Vector2(8, 6);
            phRT.offsetMax = new Vector2(-8, -6);

            var input = container.gameObject.AddComponent<InputField>();
            input.textComponent = text;
            input.placeholder = placeholderText;
            return input;
        }

        private static RectTransform CreateTitledScrollPanel_Styled(Transform parent, string title)
        {
            var container = UIFactory.CreatePanelSurface(parent, title + "_Panel");
            var vl = container.GetComponent<VerticalLayoutGroup>();
            if (vl != null)
            {
                vl.childControlHeight = false;
                vl.childForceExpandHeight = false;
                vl.spacing = 6f;
                vl.padding = new RectOffset(8, 8, 8, 8);
            }

            CreateLabel(container, title, 14, FontStyle.Bold, TextAnchor.MiddleLeft);

            var scrollRoot = CreateUIObject("Scroll", container);
            var le = scrollRoot.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 220f;
            var scroll = scrollRoot.gameObject.AddComponent<ScrollRect>();
            var viewport = CreateUIObject("Viewport", scrollRoot);
            var vpImg = viewport.gameObject.AddComponent<Image>();
            vpImg.color = new Color(0, 0, 0, 0.1f);
            viewport.anchorMin = new Vector2(0, 0);
            viewport.anchorMax = new Vector2(1, 1);
            viewport.offsetMin = new Vector2(0, 0);
            viewport.offsetMax = new Vector2(0, 0);
            var mask = viewport.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            var content = CreateUIObject("Content", viewport);
            // The viewport content needs its own VerticalLayoutGroup; ensure only one per object.
            var layout = content.GetComponent<VerticalLayoutGroup>() ?? content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 4f;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;

            return content;
        }

        private static RectTransform CreateFlexibleSpace(Transform parent)
        {
            var rt = CreateUIObject("FlexibleSpace", parent);
            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.flexibleWidth = 1f;
            return rt;
        }

        private static void CreateDivider(Transform parent)
        {
            var go = CreateUIObject("Divider", parent);
            var img = go.gameObject.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.25f);
            var le = go.gameObject.AddComponent<LayoutElement>();
            le.minHeight = 2f;
        }

        private static void CreateEmptyState(Transform parent, string message)
        {
            var label = CreateLabel(parent, message, 12, FontStyle.Italic, TextAnchor.MiddleCenter);
            label.color = new Color(1f, 1f, 1f, 0.5f);
        }

        private static void CreateFoldout(Transform parent, string title, out RectTransform content)
        {
            // Header
            var header = CreateUIObject(title + "_Header", parent);
            var hl = header.gameObject.AddComponent<HorizontalLayoutGroup>();
            hl.childAlignment = TextAnchor.MiddleLeft;
            hl.spacing = 6f;
            hl.childForceExpandWidth = true;
            var label = CreateLabel(header, title, 16, FontStyle.Bold, TextAnchor.MiddleLeft);
            var plusMinus = CreateLabel(header, "+", 18, FontStyle.Bold, TextAnchor.MiddleRight);
            var spacer = CreateFlexibleSpace(header);
            var btn = header.gameObject.AddComponent<Button>();

            // Content
            content = UIFactory.CreatePanelSurface(parent, title + "_Content");
            var localContent = content;
            // Avoid duplicate LayoutGroups: use existing or add if missing
            var vl = content.GetComponent<VerticalLayoutGroup>() ?? content.gameObject.AddComponent<VerticalLayoutGroup>();
            vl.childControlWidth = true;
            vl.childControlHeight = false;
            vl.childForceExpandWidth = true;
            vl.childForceExpandHeight = false;
            vl.spacing = 4f;
            vl.padding = new RectOffset(8, 8, 8, 8);

            // Default expanded
            bool expanded = true;
            plusMinus.text = expanded ? "–" : "+";

            btn.onClick.AddListener(() =>
            {
                expanded = !expanded;
                localContent.gameObject.SetActive(expanded);
                plusMinus.text = expanded ? "–" : "+";
            });
        }

        private static void ClearChildren(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--)
            {
                GameObject.Destroy(t.GetChild(i).gameObject);
            }
        }

    }
}
