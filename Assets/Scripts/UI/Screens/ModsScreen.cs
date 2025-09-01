using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FantasyColony.UI.Router; // IScreen lives here
using FantasyColony.UI.Widgets;
using System.Reflection;
using System.Linq;

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
        private RectTransform _rightColumn;
        private Text _rightTitle;
        private InputField _snapshotSearch;
        private RectTransform _scriptContent;
        private RectTransform _defsContent;

        // Selection state
        private string _selectedModName = null; // null => aggregated view of all active mods

        public void Enter(Transform parent)
        {

            // Root container (stretches to parent)
            _root = CreateUIObject("ModsScreenRoot", parent).GetComponent<RectTransform>();
            Stretch(_root);

            // Fullscreen background via UIFactory (opaque to hide menu)
            UIFactory_CreateFullscreenBackground(parent);

            // Two-column layout
            var row = _root.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.childForceExpandHeight = true;
            row.childForceExpandWidth = false;
            row.childAlignment = TextAnchor.UpperLeft;
            row.spacing = 16f;

            // LEFT COLUMN
            // Left column wood panel
            _leftColumn = UIFactory_CreatePanelSurface(_root, "LeftColumnPanel");
            var leftLE = _leftColumn.gameObject.AddComponent<LayoutElement>();
            leftLE.preferredWidth = 380f; // ~360–400 px
            leftLE.minWidth = 320f;
            var leftVL = _leftColumn.gameObject.AddComponent<VerticalLayoutGroup>();
            leftVL.childControlWidth = true;
            leftVL.childControlHeight = false;
            leftVL.childForceExpandWidth = true;
            leftVL.childForceExpandHeight = false;
            leftVL.spacing = 12f;
            leftVL.padding = new RectOffset(12, 12, 12, 12);

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

            UIFactory_CreateButtonSecondary(leftControls, "Sort", () => { /* TODO: sort menu */ });

            // Inactive list panel
            _inactiveListContent = CreateTitledScrollPanel_Styled(_leftColumn, "inactive mod list");
            CreateEmptyState(_inactiveListContent, "No inactive mods");

            // Active list panel
            _activeListContent = CreateTitledScrollPanel_Styled(_leftColumn, "active mod list");
            CreateEmptyState(_activeListContent, "No active mods");

            // RIGHT COLUMN
            _rightColumn = UIFactory_CreatePanelSurface(_root, "RightColumnPanel");
            var rightVL = _rightColumn.gameObject.AddComponent<VerticalLayoutGroup>();
            rightVL.childControlWidth = true;
            rightVL.childControlHeight = false;
            rightVL.childForceExpandWidth = true;
            rightVL.childForceExpandHeight = false;
            rightVL.spacing = 12f;
            rightVL.padding = new RectOffset(12, 12, 12, 12);

            // Right column header row (Dynamic title + snapshot search + Save/Load)
            var rightHeader = CreateUIObject("RightHeader", _rightColumn).GetComponent<RectTransform>();
            var rhHL = rightHeader.gameObject.AddComponent<HorizontalLayoutGroup>();
            rhHL.childAlignment = TextAnchor.MiddleLeft;
            rhHL.spacing = 8f;
            rhHL.childForceExpandWidth = true;

            _rightTitle = CreateTitleLabel(rightHeader, "All Active Mods");
            var spacer = CreateFlexibleSpace(rightHeader);
            _snapshotSearch = CreateSearchField(rightHeader, "search snapshot...");
            var snapLE = _snapshotSearch.gameObject.AddComponent<LayoutElement>();
            snapLE.preferredWidth = 260f;
            UIFactory_CreateButtonSecondary(rightHeader, "save mod list", () => { /* TODO */ });
            UIFactory_CreateButtonSecondary(rightHeader, "load mod list", () => { /* TODO */ });

            // Snapshot panel with foldouts
            var snapshotPanel = UIFactory_CreatePanelSurface(_rightColumn, "SnapshotPanel");
            var spVL = snapshotPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            spVL.childControlWidth = true;
            spVL.childControlHeight = false;
            spVL.childForceExpandWidth = true;
            spVL.childForceExpandHeight = false;
            spVL.spacing = 8f;
            spVL.padding = new RectOffset(12, 12, 12, 12);

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

            UIFactory_CreateBottomRightPrimary(_root, "apply/restart", () =>
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
            _rightTitle.text = string.IsNullOrEmpty(_selectedModName) ? "All Active Mods" : _selectedModName;
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

        // Replaces flat Image panels with UIFactory wood/bordered panels
        private static RectTransform UIFactory_CreatePanelSurface(Transform parent, string name)
        {
            // Try all public static overloads of UIFactory.CreatePanelSurface and match supported signatures.
            var overloads = typeof(UIFactory).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(x => x.Name == "CreatePanelSurface").ToArray();

            foreach (var m in overloads)
            {
                var ps = m.GetParameters();
                object[] args = null;

                // Supported patterns:
                // (Transform parent)
                if (ps.Length == 1 && typeof(Transform).IsAssignableFrom(ps[0].ParameterType))
                    args = new object[] { parent };

                // (Transform parent, string name)
                else if (ps.Length == 2 && typeof(Transform).IsAssignableFrom(ps[0].ParameterType) && ps[1].ParameterType == typeof(string))
                    args = new object[] { parent, name };

                // (Transform parent, string name, bool decorated)
                else if (ps.Length == 3 && typeof(Transform).IsAssignableFrom(ps[0].ParameterType) && ps[1].ParameterType == typeof(string) && ps[2].ParameterType == typeof(bool))
                    args = new object[] { parent, name, true };

                // Skip unsupported shapes
                if (args == null) continue;

                try
                {
                    var result = m.Invoke(null, args) as RectTransform;
                    if (result != null) return result;
                }
                catch (TargetParameterCountException) { /* try next overload */ }
                catch (ArgumentException) { /* try next overload */ }
            }
            // Fallback: simple dark panel
            var panel = CreateUIObject(name, parent).GetComponent<RectTransform>();
            var img = panel.gameObject.AddComponent<Image>();
            img.color = new Color(0.12f, 0.10f, 0.08f, 0.97f);
            return panel;
        }

        private static RectTransform CreateTitledScrollPanel_Styled(Transform parent, string title)
        {
            var container = UIFactory_CreatePanelSurface(parent, title + "_Panel");
            var vl = container.gameObject.AddComponent<VerticalLayoutGroup>();
            vl.childControlWidth = true;
            vl.childControlHeight = false;
            vl.childForceExpandWidth = true;
            vl.childForceExpandHeight = false;
            vl.spacing = 6f;
            vl.padding = new RectOffset(8, 8, 8, 8);

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

        // Button helpers that delegate to UIFactory if present
        private static Button UIFactory_CreateButtonPrimary(Transform parent, string text, Action onClick)
        {
            var method = typeof(UIFactory).GetMethod("CreateButtonPrimary", BindingFlags.Public | BindingFlags.Static);
            if (method != null)
            {
                var btn = (Button)method.Invoke(null, new object[] { parent, text, (Action)onClick });
                if (btn != null) return btn;
            }
            // Fallback to local style
            var fb = CreateBasicButton(parent, text);
            var colors = fb.colors;
            colors.normalColor = new Color(0.84f, 0.72f, 0.38f, 1f);
            colors.highlightedColor = new Color(0.92f, 0.80f, 0.45f, 1f);
            fb.colors = colors;
            fb.onClick.AddListener(() => onClick?.Invoke());
            return fb;
        }

        private static Button UIFactory_CreateButtonSecondary(Transform parent, string text, Action onClick)
        {
            var method = typeof(UIFactory).GetMethod("CreateButtonSecondary", BindingFlags.Public | BindingFlags.Static);
            if (method != null)
            {
                var btn = (Button)method.Invoke(null, new object[] { parent, text, (Action)onClick });
                if (btn != null) return btn;
            }
            var fb = CreateBasicButton(parent, text);
            var colors = fb.colors;
            colors.normalColor = new Color(0.22f, 0.20f, 0.16f, 1f);
            colors.highlightedColor = new Color(0.28f, 0.26f, 0.22f, 1f);
            fb.colors = colors;
            fb.onClick.AddListener(() => onClick?.Invoke());
            return fb;
        }

        private static Button CreateBasicButton(Transform parent, string label)
        {
            var go = CreateUIObject("Button", parent);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.22f, 0.20f, 0.16f, 1f);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 34f;
            var btn = go.AddComponent<Button>();

            var txt = CreateLabel(go.transform, label, 14, FontStyle.Bold, TextAnchor.MiddleCenter);
            var tRT = txt.GetComponent<RectTransform>();
            tRT.anchorMin = Vector2.zero;
            tRT.anchorMax = Vector2.one;
            tRT.offsetMin = new Vector2(8, 4);
            tRT.offsetMax = new Vector2(-8, -4);
            return btn;
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
            content = UIFactory_CreatePanelSurface(parent, title + "_Content");
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

        private static void UIFactory_CreateFullscreenBackground(Transform parent)
        {
            // Prefer UIFactory background (with wood/cloth and vignette)
            var method = typeof(UIFactory).GetMethod("CreateFullscreenBackground", BindingFlags.Public | BindingFlags.Static);
            if (method != null)
            {
                try
                {
                    method.Invoke(null, new object[] { parent });
                    return;
                }
                catch (TargetParameterCountException)
                {
                    // Try variant with (Transform parent, bool dim)
                    try
                    {
                        method.Invoke(null, new object[] { parent, true });
                        return;
                    }
                    catch { /* fall through to fallback */ }
                }
            }
            // Fallback: opaque dark
            var bg = CreateUIObject("Background", parent).GetComponent<RectTransform>();
            Stretch(bg);
            var img = bg.gameObject.AddComponent<Image>();
            img.color = new Color(0.06f, 0.05f, 0.04f, 0.98f);
            bg.SetAsFirstSibling();
        }

        private static void UIFactory_CreateBottomRightPrimary(Transform screenRoot, string text, Action onClick)
        {
            // If UIFactory exposes a bottom-right stack, use it; else anchor a standalone button.
            var method = typeof(UIFactory).GetMethod("CreateBottomRightStack", BindingFlags.Public | BindingFlags.Static);
            if (method != null)
            {
                RectTransform stack = null;
                try { stack = (RectTransform)method.Invoke(null, new object[] { screenRoot }); }
                catch (TargetParameterCountException)
                {
                    // Try variant with an extra name parameter
                    try { stack = (RectTransform)method.Invoke(null, new object[] { screenRoot, "BottomRight" }); }
                    catch { /* ignore, will fallback */ }
                }
                if (stack != null)
                {
                    UIFactory_CreateButtonPrimary(stack, text, onClick);
                    return;
                }
            }
            // Fallback: absolute-anchored button in bottom-right
            var holder = CreateUIObject("BottomRightHolder", screenRoot).GetComponent<RectTransform>();
            holder.anchorMin = holder.anchorMax = new Vector2(1f, 0f);
            holder.pivot = new Vector2(1f, 0f);
            holder.anchoredPosition = new Vector2(-16f, 16f);
            var hl = holder.gameObject.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 8f;
            hl.childAlignment = TextAnchor.LowerRight;
            UIFactory_CreateButtonPrimary(holder, text, onClick);
        }
    }
}
