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
        private RectTransform _btnFile, _btnEdit, _btnView, _btnTools, _btnClose;
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
            UIFactory.SetAnchorsPercent(_toolbar, xMin:0f, xMax:1f, yMin:1f - TOOLBAR_FRAC, yMax:1f);
            UIFactory.SetAnchorsPercent(_stage, xMin:0f, xMax:1f, yMin:0f, yMax:1f - TOOLBAR_FRAC);

            // --- Toolbar content: equal-width buttons across full width ---
            // Create equal-width buttons directly under the toolbar (no nested Row)
            _btnFile  = CreateFlexMenuButton(_toolbar, "File",  OnFileMenu);
            _btnEdit  = CreateFlexMenuButton(_toolbar, "Edit",  OnEditMenu);
            _btnView  = CreateFlexMenuButton(_toolbar, "View",  OnViewMenu);
            _btnTools = CreateFlexMenuButton(_toolbar, "Tools", OnToolsMenu);
            // Remove Help per request
            _btnClose = CreateFlexMenuButton(_toolbar, "Close", () => UIRouter.Current?.Pop());

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
        private static RectTransform CreateFlexMenuButton(Transform parent, string label, System.Action onClick)
        {
            var btn = UIFactory.CreateButtonPrimary(parent, label, onClick);
            // Equal slice of width, regardless of label length
            var le = btn.GetComponent<LayoutElement>();
            if (le == null) le = btn.gameObject.AddComponent<LayoutElement>();
            le.minWidth = 0f;
            le.preferredWidth = 0f;
            le.flexibleWidth = 1f; // ratio slice

            var txt = btn.GetComponentInChildren<Text>();
            if (txt != null)
            {
                txt.resizeTextForBestFit = true;
                txt.resizeTextMinSize = 12;
                txt.resizeTextMaxSize = 28;
            }
            return btn.GetComponent<RectTransform>();
        }

        // --- Simple dropdown framework (stub for Step 1) ---
        private RectTransform _menuOverlay;
        private RectTransform _openMenu;

        private void EnsureMenuOverlay(Transform parent)
        {
            if (_menuOverlay != null) return;
            var go = new GameObject("MenuOverlay", typeof(RectTransform), typeof(Image));
            _menuOverlay = go.GetComponent<RectTransform>();
            var img = go.GetComponent<Image>();
            img.color = new Color(0f,0f,0f,0f);
            img.raycastTarget = true; // capture outside clicks
            _menuOverlay.SetParent(parent, false);
            _menuOverlay.anchorMin = Vector2.zero; _menuOverlay.anchorMax = Vector2.one;
            _menuOverlay.offsetMin = Vector2.zero; _menuOverlay.offsetMax = Vector2.zero;
            go.AddComponent<Button>().onClick.AddListener(CloseMenus);
            go.SetActive(false);
        }

        private void CloseMenus()
        {
            if (_openMenu != null)
            {
                UnityObject.Destroy(_openMenu.gameObject);
                _openMenu = null;
            }
            if (_menuOverlay != null) _menuOverlay.gameObject.SetActive(false);
        }

        private void ShowMenu(RectTransform anchor, params (string label, System.Action onClick)[] items)
        {
            CloseMenus();
            EnsureMenuOverlay(_toolbar.parent); // overlay lives under AbsoluteLayer
            _menuOverlay.gameObject.SetActive(true);

            var list = new System.Collections.Generic.List<UIFactory.MenuItem>(items.Length);
            foreach (var it in items)
                list.Add(new UIFactory.MenuItem(it.label, () => { it.onClick?.Invoke(); CloseMenus(); }));

            _openMenu = UIFactory.CreateDropdownMenu(_menuOverlay, anchor, list, rowHeight: 32f, minWidth: 160f, matchAnchorWidth: true);
        }

        // --- Menu actions (stubs) ---
        private void OnFileMenu()
        {
            ShowMenu(_btnFile, ("New", ()=>Debug.Log("[UICreator] File/New")),
                              ("Save", ()=>Debug.Log("[UICreator] File/Save (stub)")),
                              ("Load", ()=>Debug.Log("[UICreator] File/Load (stub)")));
        }

        private void OnEditMenu() { Debug.Log("[UICreator] Edit (stub)"); }

        private void OnViewMenu()
        {
            ShowMenu(_btnView, ("Fullscreen Work Area", ()=> ToggleFullscreenWorkArea()));
        }

        private void OnToolsMenu()
        {
            ShowMenu(_btnTools,
                ("Add Primary Button", ()=> SpawnButton("UI_PrimaryButton", UIFactory.CreateButtonPrimary)),
                ("Add Secondary Button", ()=> SpawnButton("UI_SecondaryButton", UIFactory.CreateButtonSecondary)),
                ("Add Danger Button", ()=> SpawnButton("UI_DangerButton", UIFactory.CreateButtonDanger)),
                ("Add Panel", ()=> SpawnPanel(false)),
                ("Add Background Panel", ()=> SpawnPanel(true))
            );
        }

        private void ToggleFullscreenWorkArea()
        {
            bool isActive = _toolbar.gameObject.activeSelf;
            _toolbar.gameObject.SetActive(!isActive);
            Debug.Log($"[UICreator] View/Fullscreen Work Area: {!isActive}");
        }

        // --- Spawn helpers (center of stage; placement tools come later) ---
        private void SpawnButton(string name, System.Func<Transform, string, System.Action, UnityEngine.UI.Button> ctor)
        {
            var btn = ctor(_stage, name.Replace("UI_", string.Empty), ()=>{});
            btn.gameObject.name = name;
            var rt = btn.GetComponent<RectTransform>();
            // Apply factory defaults so buttons spawned outside a LayoutGroup are visible and usable
            UIFactory.ApplyDefaultButtonSizing(rt);
            rt.anchoredPosition = Vector2.zero;
            Debug.Log($"[UICreator] Spawn {name}");
        }

        private void SpawnPanel(bool background)
        {
            if (background)
            {
                var go = new GameObject("UI_BackgroundPanel", typeof(RectTransform), typeof(Image));
                var rt = go.GetComponent<RectTransform>();
                var img = go.GetComponent<Image>();
                img.color = new Color(0f,0f,0f,0f);
                img.raycastTarget = false;
                rt.SetParent(_stage, false);
                rt.anchorMin = new Vector2(0.1f, 0.1f);
                rt.anchorMax = new Vector2(0.9f, 0.9f);
                rt.offsetMin = rt.offsetMax = Vector2.zero;
                Debug.Log("[UICreator] Spawn UI_BackgroundPanel");
                return;
            }

            var panel = UIFactory.CreatePanelSurface(_stage, "UI_Panel");
            var prt = panel;
            UIFactory.ApplyDefaultPanelSizing(prt);
            Debug.Log("[UICreator] Spawn UI_Panel");
        }
    }
}
