using System;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using FantasyColony.UI.Router;
using FantasyColony.UI.Widgets;
using FantasyColony.UI.Util;
using FantasyColony.UI.Creator.Editing;
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
        private UIStageGrid _grid;
        private RectTransform _layerBackground, _layerPanels, _layerControls;
        private RectTransform _btnFile, _btnEdit, _btnView, _btnTools, _btnClose;
        private const float TOOLBAR_FRAC = 0.05f; // 5% of screen height

        // Attach controller when screen becomes active to manage stage fullscreen toggling
        private void OnEnable()
        {
            try
            {
                var rootRT = _root != null ? _root.GetComponent<RectTransform>() : null;
                if (rootRT != null && _toolbar != null && _stage != null && _root.GetComponent<UICreatorStageController>() == null)
                {
                    var ctl = _root.gameObject.AddComponent<UICreatorStageController>();
                    ctl.Initialize(_toolbar, _stage, TOOLBAR_FRAC);
                    Debug.Log("[UICreator] Controller attached");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[UICreator] Controller attach failed: {e.Message}");
            }
        }

        // Ensure toolbar is visible if this screen is being closed
        private void OnDisable()
        {
            var ctl = _root != null ? _root.GetComponent<UICreatorStageController>() : null;
            if (ctl != null)
            {
                ctl.SetStageFullscreen(false);
            }
        }

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

              // Use top-left origin for deterministic editing math
              _stage.pivot = new Vector2(0f, 1f);
              _stage.anchoredPosition = Vector2.zero;

              // Make stage click-through so it never blocks selection of panels
              var stageImg2 = _stage.GetComponent<Image>();
              if (stageImg2 != null) stageImg2.raycastTarget = false;

              _grid = _stage.gameObject.GetComponent<UIStageGrid>();
              if (_grid == null) _grid = _stage.gameObject.AddComponent<UIStageGrid>();
              Debug.Log($"[UICreator] Grid:on size={GridPrefs.CellSize}");
              EnsureLayers();

            // --- Toolbar content: equal-width buttons across full width ---
            // Create equal-width buttons directly under the toolbar (no nested Row)
            _btnFile  = CreateFlexMenuButton(_toolbar, "File",  OnFileMenu);
            _btnEdit  = CreateFlexMenuButton(_toolbar, "Edit",  OnEditMenu);
            _btnView  = CreateFlexMenuButton(_toolbar, "View",  OnViewMenu);
            _btnTools = CreateFlexMenuButton(_toolbar, "Tools", OnToolsMenu);
            // Remove Help per request
            _btnClose = CreateFlexMenuButton(_toolbar, "Close", () => UIRouter.Current?.Pop());

            // Reset toolbar ON whenever entering the Creator
            OnEnable();
            var ctl = _root != null ? _root.GetComponent<UICreatorStageController>() : null;
            if (ctl != null) ctl.SetStageFullscreen(false);

            IsOpen = true;
        }

        public void Exit()
        {
            if (_root != null)
            {
                Debug.Log("[UICreator] Exit");
                OnDisable();
                // Cleanup any open menus/overlays
                CloseMenus();
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
            var ctl = _root != null ? _root.GetComponent<UICreatorStageController>() : null;
            if (ctl != null)
            {
                bool target = !ctl.IsStageFullscreen;
                ctl.SetStageFullscreen(target);
                Debug.Log($"[UICreator] View/Fullscreen Work Area: {target}");
            }
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
            NormalizePlaceable(rt);
            AttachEditing(rt);
            Debug.Log($"[UICreator] Spawn {name}");
        }

        private void SpawnPanel(bool background)
        {
            if (background)
            {
                Debug.Log("[UICreator] Add Background Panel requested");
                // Background placeable should also be manually resizable
                var panelGO = UIFactory.CreatePanelSurface(_stage, "UI_BackgroundPanel", sizing: PanelSizing.Flexible);
                var rt = panelGO != null ? panelGO.GetComponent<RectTransform>() : null;
                if (rt != null)
                {
                    UIFactory.EnsureRaycastTarget(rt);
                    NormalizePlaceable(rt, PlaceableLayer.Background);
                    AttachEditing(rt);
                    Debug.Log($"[UICreator] BackgroundPanel created size={rt.rect.size} pos={rt.anchoredPosition} sib={rt.GetSiblingIndex()}");
                }
                return;
            }

            Debug.Log("[UICreator] Add Panel requested");
            // Placeables must be manually resizable -> use Flexible sizing (no ContentSizeFitter)
            var panelGO2 = UIFactory.CreatePanelSurface(_stage, "UI_Panel", sizing: PanelSizing.Flexible);
            var prt = panelGO2 != null ? panelGO2.GetComponent<RectTransform>() : null;
            if (prt != null)
            {
                UIFactory.EnsureRaycastTarget(prt);
                NormalizePlaceable(prt, PlaceableLayer.Panel);
                AttachEditing(prt);
                Debug.Log($"[UICreator] Panel created size={prt.rect.size} pos={prt.anchoredPosition} sib={prt.GetSiblingIndex()}");
            }
        }

        private void NormalizePlaceable(RectTransform rt)
        {
            if (rt.GetComponent<Button>() != null) NormalizePlaceable(rt, PlaceableLayer.Control);
            else if (rt.name.Contains("Background")) NormalizePlaceable(rt, PlaceableLayer.Background);
            else NormalizePlaceable(rt, PlaceableLayer.Panel);
        }

        private enum PlaceableLayer { Background, Panel, Control }

        private void NormalizePlaceable(RectTransform rt, PlaceableLayer layer)
        {
            var t = rt;
            t.anchorMin = new Vector2(0, 1);
            t.anchorMax = new Vector2(0, 1);
            t.pivot = new Vector2(0, 1);
            var parent = layer == PlaceableLayer.Control ? _layerControls : layer == PlaceableLayer.Panel ? _layerPanels : _layerBackground;
            if (t.parent != parent) t.SetParent(parent, false);
            if (t.sizeDelta == Vector2.zero) t.sizeDelta = new Vector2(400, 240);
            if (t.anchoredPosition == Vector2.zero) t.anchoredPosition = new Vector2(128, -128);
            t.anchoredPosition = new Vector2(
                Mathf.Round(t.anchoredPosition.x / GridPrefs.CellSize) * GridPrefs.CellSize,
                Mathf.Round(t.anchoredPosition.y / GridPrefs.CellSize) * GridPrefs.CellSize);

            var csf = t.GetComponent<ContentSizeFitter>();
            if (csf != null)
            {
                UnityObject.Destroy(csf);
                Debug.Log("[UICreator] Removed ContentSizeFitter on placeable root");
            }
        }

        private void EnsureLayers()
        {
            _layerBackground = EnsureLayer(_stage, "_Layer_Background");
            _layerPanels = EnsureLayer(_stage, "_Layer_Panels");
            _layerControls = EnsureLayer(_stage, "_Layer_Controls");
            _layerBackground.SetSiblingIndex(0);
            _layerPanels.SetSiblingIndex(1);
            _layerControls.SetSiblingIndex(2);
        }

        private static RectTransform EnsureLayer(RectTransform parent, string name)
        {
            var t = parent.Find(name) as RectTransform;
            if (t == null)
            {
                var go = new GameObject(name, typeof(RectTransform));
                t = go.GetComponent<RectTransform>();
                t.SetParent(parent, false);
                t.anchorMin = Vector2.zero; t.anchorMax = Vector2.one; t.pivot = new Vector2(0.5f, 0.5f);
                t.offsetMin = Vector2.zero; t.offsetMax = Vector2.zero;
            }
            return t;
        }

        private void AttachEditing(RectTransform rt)
        {
            if (rt == null) return;
            if (rt.GetComponent<UIPixelSnap>() == null) rt.gameObject.AddComponent<UIPixelSnap>();

            // Create a full-rect HitArea child to receive pointer events reliably
            var hit = new GameObject("HitArea", typeof(RectTransform), typeof(Image));
            var hitRT = hit.GetComponent<RectTransform>();
            hitRT.SetParent(rt, false);
            hitRT.anchorMin = Vector2.zero; hitRT.anchorMax = Vector2.one; hitRT.pivot = new Vector2(0.5f, 0.5f);
            hitRT.offsetMin = Vector2.zero; hitRT.offsetMax = Vector2.zero;
            var hitImg = hit.GetComponent<Image>(); hitImg.color = new Color(0,0,0,0); hitImg.raycastTarget = true;
            var hitLE = hit.AddComponent<LayoutElement>(); hitLE.ignoreLayout = true; // do not be controlled by parent VLG

            var mover = hit.AddComponent<UIDragMove>();
            mover.Init(rt, _stage);

            var sel = hit.AddComponent<UISelectionBox>();
            sel.Init(rt, _stage);
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            bool g = Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame;
            bool ctrl = Keyboard.current != null && (Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed);
            bool f4 = Keyboard.current != null && Keyboard.current.f4Key.wasPressedThisFrame;
            bool iKey = Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame;
#else
            bool g = Input.GetKeyDown(KeyCode.G);
            bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            bool f4 = Input.GetKeyDown(KeyCode.F4);
            bool iKey = Input.GetKeyDown(KeyCode.I);
#endif
            if (g)
            {
                if (ctrl)
                {
                    GridPrefs.CycleCellSize();
                    _grid?.MarkDirty();
                }
                else
                {
                    GridPrefs.GridVisible = !GridPrefs.GridVisible;
                    Debug.Log($"[UICreator] Grid {(GridPrefs.GridVisible ? "on" : "off")}");
                }
            }
            if (f4)
            {
                GridPrefs.SnapEnabled = !GridPrefs.SnapEnabled;
                Debug.Log($"[UICreator] Snap {(GridPrefs.SnapEnabled ? "on" : "off")}");
            }

            if (iKey)
            {
                var sel = UISelectionBox.CurrentTarget;
                if (sel != null)
                {
                    InstructionDialog.Show(_stage, sel);
                }
                else
                {
                    Debug.Log("[UICreator] No selection for Instructions dialog");
                }
            }
        }
    }

    // Local controller that handles F11/Escape and stage fullscreen toggling.
    internal sealed class UICreatorStageController : MonoBehaviour
    {
        private RectTransform _toolbar;
        private RectTransform _stage;
        private float _toolbarFrac = 0.05f;
        private bool _isFullscreen;

        public bool IsStageFullscreen => _isFullscreen;

        public void Initialize(RectTransform toolbar, RectTransform stage, float toolbarFrac)
        {
            _toolbar = toolbar;
            _stage = stage;
            _toolbarFrac = Mathf.Clamp01(toolbarFrac);
        }

        private void Update()
        {
            if (_toolbar == null || _stage == null) return;
#if ENABLE_INPUT_SYSTEM
            bool f11 = Keyboard.current != null && Keyboard.current.f11Key.wasPressedThisFrame;
            bool esc = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
            bool f11 = Input.GetKeyDown(KeyCode.F11);
            bool esc = Input.GetKeyDown(KeyCode.Escape);
#endif
            if (f11)
            {
                SetStageFullscreen(!_isFullscreen);
            }
            else if (esc && _isFullscreen)
            {
                SetStageFullscreen(false);
            }
        }

        public void SetStageFullscreen(bool on)
        {
            if (_toolbar == null || _stage == null) return;
            _isFullscreen = on;

            if (on)
            {
                if (_toolbar.gameObject.activeSelf) _toolbar.gameObject.SetActive(false);
                // Stage fills entire area
                SetAnchors(_stage, new Vector2(0f, 0f), new Vector2(1f, 1f));
                Debug.Log("[UICreator] View: Stage Fullscreen ON");
            }
            else
            {
                if (!_toolbar.gameObject.activeSelf) _toolbar.gameObject.SetActive(true);
                // Toolbar on top strip; stage below it.
                SetAnchors(_toolbar, new Vector2(0f, 1f - _toolbarFrac), new Vector2(1f, 1f));
                SetAnchors(_stage, new Vector2(0f, 0f), new Vector2(1f, 1f - _toolbarFrac));
                Debug.Log("[UICreator] View: Stage Fullscreen OFF");
            }
        }

        private static void SetAnchors(RectTransform rt, Vector2 min, Vector2 max)
        {
            var t = rt;
            t.anchorMin = min;
            t.anchorMax = max;
            t.offsetMin = Vector2.zero;
            t.offsetMax = Vector2.zero;
            t.anchoredPosition = Vector2.zero;
            t.localScale = Vector3.one;
        }

        private void OnDestroy()
        {
            // If screen closes while fullscreen, ensure toolbar is not left hidden in scene play mode.
            if (_toolbar != null) _toolbar.gameObject.SetActive(true);
        }
    }
}
