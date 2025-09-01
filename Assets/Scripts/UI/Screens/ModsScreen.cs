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

        private const float LeftWidth = 380f;
        private const float RightWidth = 320f;
        // Selection state
        private string _selectedModName = null; // null => aggregated view of all active mods

        public void Enter(Transform parent)
        {

            // Root container (stretches to parent)
            _root = CreateUIObject("ModsScreenRoot", parent).GetComponent<RectTransform>();
            Stretch(_root);

            // Use new board helpers
            var board = UIFactory.CreateBoardScreen(_root, padding:32, spacing:16);
            var trio = UIFactory.CreateThreeColumnBoard(board.Content, leftWidth:380f, rightWidth:320f, joinDecor:true);

            // LEFT COLUMN
            _leftColumn = trio.left;
            var leftLE = _leftColumn.GetComponent<LayoutElement>() ?? _leftColumn.gameObject.AddComponent<LayoutElement>();
            leftLE.preferredWidth = LeftWidth; // fixed width column
            leftLE.minWidth = LeftWidth - 60f;
            leftLE.flexibleWidth = 0f; // fixed width column
            // Panel already has a VerticalLayoutGroup from UIFactory; adjust its spacing/padding if needed
            var leftVL = _leftColumn.GetComponent<VerticalLayoutGroup>();
            if (leftVL != null)
            {
                leftVL.spacing = 12f;
                leftVL.padding = new RectOffset(12, 12, 12, 12);
                leftVL.childControlHeight = false;
                leftVL.childForceExpandHeight = false;
            }

            // Left title: "Mods"
            CreateHeaderLabel(_leftColumn, "Mods");

            // Search + Sort row
            var leftControls = CreateUIObject("LeftControls", _leftColumn).GetComponent<RectTransform>();
            var lcHL = leftControls.gameObject.AddComponent<HorizontalLayoutGroup>();
            lcHL.childAlignment = TextAnchor.MiddleLeft;
            lcHL.spacing = 8f;
            lcHL.childForceExpandWidth = false;

            _leftSearch = CreateSearchField(leftControls, "Search mods...");
            var searchLE = _leftSearch.GetComponent<LayoutElement>();
            if (searchLE == null) searchLE = _leftSearch.gameObject.AddComponent<LayoutElement>();
            searchLE.preferredWidth = 220f;

            UIFactory.CreateButtonSecondary(leftControls, "Sort", () => { /* TODO: sort menu */ });

            // Inactive list panel
            _inactiveListContent = CreateTitledScrollPanel_Styled(_leftColumn, "inactive mod list");
            CreateEmptyState(_inactiveListContent, "No inactive mods");

            // Active list panel
            _activeListContent = CreateTitledScrollPanel_Styled(_leftColumn, "active mod list");
            CreateEmptyState(_activeListContent, "No active mods");

            // CENTER COLUMN (snapshot)
            _centerColumn = trio.center;
            var centerLE = _centerColumn.GetComponent<LayoutElement>() ?? _centerColumn.gameObject.AddComponent<LayoutElement>();
            centerLE.preferredWidth = -1f; // let layout decide
            centerLE.flexibleWidth = 1f;  // flexible middle column
            var centerVL = _centerColumn.GetComponent<VerticalLayoutGroup>();
            if (centerVL != null)
            {
                centerVL.spacing = 12f;
                centerVL.padding = new RectOffset(12, 12, 12, 12);
                centerVL.childControlHeight = false;
                centerVL.childForceExpandHeight = false;
            }

            // Center header row (Dynamic title + snapshot search)
            var centerHeader = CreateUIObject("CenterHeader", _centerColumn).GetComponent<RectTransform>();
            var chHL = centerHeader.gameObject.AddComponent<HorizontalLayoutGroup>();
            chHL.childAlignment = TextAnchor.MiddleLeft;
            chHL.spacing = 8f;
            chHL.childForceExpandWidth = true;

            _centerTitle = CreateTitleLabel(centerHeader, "All Active Mods");
            var spacerC = CreateFlexibleSpace(centerHeader);
            _snapshotSearch = CreateSearchField(centerHeader, "search snapshot...");
            var snapLE = _snapshotSearch.gameObject.AddComponent<LayoutElement>();
            snapLE.preferredWidth = 260f;

            // Snapshot panel with foldouts
            var snapshotPanel = UIFactory.CreatePanelSurface(_centerColumn, "SnapshotPanel");
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

            CreateFoldout(snapshotPanel, "defs +", out _defsContent);
            CreateDivider(_defsContent);
            CreateDivider(_defsContent);
            CreateDivider(_defsContent);

            // RIGHT COLUMN (actions)
            _rightColumn = trio.right;
            var rightLE = _rightColumn.GetComponent<LayoutElement>() ?? _rightColumn.gameObject.AddComponent<LayoutElement>();
            rightLE.preferredWidth = RightWidth;
            rightLE.minWidth = RightWidth - 60f;
            rightLE.flexibleWidth = 0f; // fixed width column
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
        private static GameObject CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
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
            var t = go.AddComponent<Text>();
            t.text = text;
            t.fontSize = size;
            t.alignment = anchor;
            t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.color = new Color(0.92f, 0.88f, 0.78f, 1f); // light text (fits base style)
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = 24f;
            return t;
        }

        private static Text CreateHeaderLabel(Transform parent, string text)
        {
            return CreateLabel(parent, text, 22, FontStyle.Bold, TextAnchor.MiddleLeft);
        }

        private static Text CreateTitleLabel(Transform parent, string text)
        {
            return CreateLabel(parent, text, 20, FontStyle.Bold, TextAnchor.MiddleLeft);
        }

        private static InputField CreateSearchField(Transform parent, string placeholder)
        {
            var container = CreateUIObject("SearchField", parent);
            var bg = container.AddComponent<Image>();
            bg.color = new Color(0.16f, 0.14f, 0.10f, 0.85f);
            var le = container.AddComponent<LayoutElement>();
            le.preferredHeight = 32f;
            le.flexibleWidth = 0f;

            var textGO = CreateUIObject("Text", container.transform);
            var text = textGO.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.color = new Color(0.92f, 0.88f, 0.78f, 1f);
            text.alignment = TextAnchor.MiddleLeft;
            var textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0, 0);
            textRT.anchorMax = new Vector2(1, 1);
            textRT.offsetMin = new Vector2(8, 6);
            textRT.offsetMax = new Vector2(-8, -6);

            var phGO = CreateUIObject("Placeholder", container.transform);
            var placeholderText = phGO.AddComponent<Text>();
            placeholderText.text = placeholder;
            placeholderText.font = text.font;
            placeholderText.color = new Color(1f, 1f, 1f, 0.35f);
            placeholderText.alignment = TextAnchor.MiddleLeft;
            var phRT = phGO.GetComponent<RectTransform>();
            phRT.anchorMin = new Vector2(0, 0);
            phRT.anchorMax = new Vector2(1, 1);
            phRT.offsetMin = new Vector2(8, 6);
            phRT.offsetMax = new Vector2(-8, -6);

            var input = container.AddComponent<InputField>();
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

            var scrollRoot = CreateUIObject("Scroll", container).GetComponent<RectTransform>();
            var le = scrollRoot.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 220f;
            var scroll = scrollRoot.gameObject.AddComponent<ScrollRect>();
            var viewport = CreateUIObject("Viewport", scrollRoot).GetComponent<RectTransform>();
            var vpImg = viewport.gameObject.AddComponent<Image>();
            vpImg.color = new Color(0, 0, 0, 0.1f);
            viewport.anchorMin = new Vector2(0, 0);
            viewport.anchorMax = new Vector2(1, 1);
            viewport.offsetMin = new Vector2(0, 0);
            viewport.offsetMax = new Vector2(0, 0);
            var mask = viewport.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            var content = CreateUIObject("Content", viewport).GetComponent<RectTransform>();
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
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
            var go = CreateUIObject("FlexibleSpace", parent);
            var le = go.AddComponent<LayoutElement>();
            le.flexibleWidth = 1f;
            return go.GetComponent<RectTransform>();
        }

        private static void CreateDivider(Transform parent)
        {
            var go = CreateUIObject("Divider", parent);
            var img = go.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.25f);
            var le = go.AddComponent<LayoutElement>();
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
            var header = CreateUIObject(title + "_Header", parent).GetComponent<RectTransform>();
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
            var vl = content.gameObject.AddComponent<VerticalLayoutGroup>();
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
