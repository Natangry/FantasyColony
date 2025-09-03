using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UnityObject = UnityEngine.Object;
using System.Linq;
using FantasyColony; // for GetHierarchyPath()
using FantasyColony.UI.Style;
using FantasyColony.UI.Util;
using TintTheme = FantasyColony.UI.Style.BaseUIStyle.TintTheme;

namespace FantasyColony.UI.Widgets
{
    public enum PanelSizing { Flexible, AutoHeight, AutoWidth, AutoBoth }

    public static class UIFactory
    {
        // ===== Cached assets =====
        private static Sprite _woodTile;

        private static Sprite GetWoodTile()
        {
            if (_woodTile == null)
                _woodTile = Resources.Load<Sprite>(BaseUIStyle.WoodTilePath);
            return _woodTile;
        }

        // ===== Layout utilities =====
        public static void SetAnchorsPercent(RectTransform rt, float xMin, float xMax, float yMin, float yMax)
        {
            if (rt == null) return;
            rt.anchorMin = new Vector2(xMin, yMin);
            rt.anchorMax = new Vector2(xMax, yMax);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        public static LayoutElement EnsureLayoutElement(GameObject go)
        {
            var le = go.GetComponent<LayoutElement>();
            if (le == null) le = go.AddComponent<LayoutElement>();
            return le;
        }

        // Cache a symmetrized version of the dark 9-slice border so L/R and T/B are equal.
        private static Sprite _darkBorderSymmetric;
        private static Material _grayscaleTintMat;

        // Tracks CPU-generated grayscale sprites so they can be released.
        private class GrayscaleTracker : MonoBehaviour
        {
            public Sprite Source;
            public Sprite Gray;

            private void OnDestroy()
            {
                if (Source != null)
                {
                    GrayscaleSpriteCache.Release(Source);
                    Source = null;
                    Gray = null;
                }
            }
        }

        // ---- Button sizing helpers (safe defaults when outside a LayoutGroup) ----
        /// <summary>
        /// Returns true if any parent has a LayoutGroup (Horizontal/Vertical/Grid).
        /// </summary>
        public static bool ParentHasLayoutGroup(Transform t)
        {
            if (t == null) return false;
            Transform p = t.parent;
            while (p != null)
            {
                if (p.GetComponent<HorizontalLayoutGroup>() != null ||
                    p.GetComponent<VerticalLayoutGroup>()   != null ||
                    p.GetComponent<GridLayoutGroup>()       != null)
                    return true;
                p = p.parent;
            }
            return false;
        }

        public static RectTransform[] SplitPercentH(Transform parent, params float[] percents)
        {
            if (parent == null || percents == null || percents.Length == 0) return Array.Empty<RectTransform>();
            float total = 0f; foreach (var f in percents) total += Mathf.Max(0f, f);
            if (total <= 0f) return Array.Empty<RectTransform>();
            float x = 0f;
            var list = new List<RectTransform>(percents.Length);
            for (int i = 0; i < percents.Length; i++)
            {
                float w = Mathf.Max(0f, percents[i]) / total;
                var go = new GameObject($"H{i}", typeof(RectTransform));
                var rt = go.GetComponent<RectTransform>();
                rt.SetParent(parent, false);
                SetAnchorsPercent(rt, x, x + w, 0f, 1f);
                list.Add(rt);
                x += w;
            }
            return list.ToArray();
        }

        public static RectTransform[] SplitPercentV(Transform parent, params float[] percents)
        {
            if (parent == null || percents == null || percents.Length == 0) return Array.Empty<RectTransform>();
            float total = 0f; foreach (var f in percents) total += Mathf.Max(0f, f);
            if (total <= 0f) return Array.Empty<RectTransform>();
            float y = 0f;
            var list = new List<RectTransform>(percents.Length);
            for (int i = 0; i < percents.Length; i++)
            {
                float h = Mathf.Max(0f, percents[i]) / total;
                var go = new GameObject($"V{i}", typeof(RectTransform));
                var rt = go.GetComponent<RectTransform>();
                rt.SetParent(parent, false);
                SetAnchorsPercent(rt, 0f, 1f, y, y + h);
                list.Add(rt);
                y += h;
            }
            return list.ToArray();
        }

        /// <summary>
        /// Apply sane default sizing to a Button's RectTransform when not controlled by a LayoutGroup.
        /// In a free (no-layout) container, set explicit size; in a layout, only ensure a stable height.
        /// Idempotent.
        /// </summary>
        public static void ApplyDefaultButtonSizing(RectTransform rt, Vector2? freeSize = null)
        {
            if (rt == null) return;
            var size = freeSize ?? new Vector2(240f, 64f);
            var le = rt.GetComponent<LayoutElement>();
            if (le == null) le = rt.gameObject.AddComponent<LayoutElement>();

            if (!ParentHasLayoutGroup(rt))
            {
                // Free stage: give it an explicit, visible size centered on the stage.
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = size;
                rt.anchoredPosition = Vector2.zero;
                // Allow later manual resizing by authoring tools
                le.minWidth = 0f; le.preferredWidth = 0f; le.flexibleWidth = 0f;
                le.minHeight = 0f; le.preferredHeight = size.y; le.flexibleHeight = 0f;
            }
            else
            {
                // In a LayoutGroup: don't force width; provide a sensible preferred height for the row.
                le.minWidth = 0f; le.preferredWidth = 0f; le.flexibleWidth = 0f;
                le.minHeight = 0f; le.preferredHeight = 48f; le.flexibleHeight = 0f;
            }
        }

        public static void ApplyDefaultPanelSizing(RectTransform rt, Vector2? freeSize = null)
        {
            if (rt == null) return;
            var size = freeSize ?? new Vector2(480f, 320f);
            var le = rt.GetComponent<LayoutElement>(); if (le == null) le = rt.gameObject.AddComponent<LayoutElement>();
            if (!ParentHasLayoutGroup(rt))
            {
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = size;
                rt.anchoredPosition = Vector2.zero;
            }
            le.minWidth = 0f; le.preferredWidth = 0f; le.flexibleWidth = 0f;
            le.minHeight = 0f; le.preferredHeight = size.y; le.flexibleHeight = 0f;
        }

        // ---- Dropdown Menu ----
        public struct MenuItem
        {
            public string Label;
            public Action OnClick;
            public bool Separator;

            public MenuItem(string label, Action onClick)
            {
                Label = label;
                OnClick = onClick;
                Separator = false;
            }

            public static MenuItem Sep() => new MenuItem { Label = string.Empty, OnClick = null, Separator = true };
        }

        /// <summary>
        /// Build a dropdown menu under an anchor. The overlay must be a full-screen RectTransform with no LayoutGroup.
        /// Returns the dropdown root (caller manages its lifetime).
        /// </summary>
        public static RectTransform CreateDropdownMenu(
            RectTransform overlay,
            RectTransform anchor,
            IList<MenuItem> items,
            float rowHeight = 32f,
            float minWidth = 160f,
            bool matchAnchorWidth = true)
        {
            if (overlay == null || anchor == null) return null;

            var root = new GameObject("Dropdown", typeof(RectTransform)).GetComponent<RectTransform>();
            root.SetParent(overlay, false);

            var canvas = overlay.GetComponentInParent<Canvas>();

            // Position near the anchor (below it, left-aligned) using bottom-left corner of the button
            var anchorRT = anchor.GetComponent<RectTransform>();

            // Dropdown root measures from overlay top-left
            root.anchorMin = root.anchorMax = new Vector2(0f, 1f);
            root.pivot = new Vector2(0f, 1f);

            // Get world-space corners of the anchor; index 0 = bottom-left
            Vector3[] corners = new Vector3[4];
            anchorRT.GetWorldCorners(corners);
            Vector3 worldBL = corners[0];

            // Convert to overlay local space (ScreenSpaceOverlay => cam null)
            Vector2 screenBL = RectTransformUtility.WorldToScreenPoint(null, worldBL);
            Vector2 localBL;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(overlay, screenBL, null, out localBL);

            // Convert overlay-local (pivot) to anchoredPosition in top-left anchor space
            Vector2 overlayTopLeft = new Vector2(-overlay.rect.width * overlay.pivot.x, overlay.rect.height * (1f - overlay.pivot.y));
            root.anchoredPosition = localBL - overlayTopLeft;
            root.SetAsLastSibling();

            // Panel with VLG + CSF (Preferred Y)
            var panel = CreatePanelSurface(root, "Panel");
            var vl = panel.gameObject.GetComponent<VerticalLayoutGroup>() ?? panel.gameObject.AddComponent<VerticalLayoutGroup>();
            vl.childControlWidth = true; vl.childForceExpandWidth = true;
            vl.childControlHeight = true; vl.childForceExpandHeight = false;
            vl.spacing = 4f; vl.padding = new RectOffset(6,6,6,6);

            var fitter = panel.gameObject.GetComponent<ContentSizeFitter>() ?? panel.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            float width = Mathf.Max(minWidth, matchAnchorWidth ? anchor.rect.width : minWidth);
            var prt = panel.GetComponent<RectTransform>();
            prt.sizeDelta = new Vector2(width, 0f);

            foreach (var it in items)
            {
                if (it.Separator)
                {
                    var sep = new GameObject("Sep", typeof(RectTransform), typeof(Image));
                    var srt = sep.GetComponent<RectTransform>();
                    srt.SetParent(panel, false);
                    var le = sep.AddComponent<LayoutElement>();
                    var scale = canvas != null ? Mathf.Max(1f, canvas.scaleFactor) : 1f;
                    le.preferredHeight = 1f / scale; // crisp 1 physical pixel
                    continue;
                }
                var row = CreateButtonSecondary(panel, it.Label, () => it.OnClick?.Invoke());
                var le2 = row.GetComponent<LayoutElement>();
                if (le2 == null) le2 = row.gameObject.AddComponent<LayoutElement>();
                le2.preferredHeight = rowHeight;
                le2.flexibleWidth = 1f;
            }
            return root;
        }

        // Ensure the 9-slice border has equal left/right and top/bottom edge sizes.
        // This avoids visual asymmetry when Unity stretches the sliced edges.
        private static Sprite GetSymmetricDarkBorder()
        {
            if (_darkBorderSymmetric != null)
                return _darkBorderSymmetric;

            var src = Resources.Load<Sprite>(BaseUIStyle.DarkBorder9SPath);
            if (src == null)
                return null;

            _darkBorderSymmetric = MakeUniformBorderFromTopBottom(src);
            return _darkBorderSymmetric;
        }

        private static Material GetGrayscaleTintMaterial()
        {
            if (_grayscaleTintMat != null) return _grayscaleTintMat;
            // Load from Resources to ensure inclusion in builds
            var sh = Resources.Load<Shader>("Shaders/UIGrayscaleTint");
            if (sh == null)
            {
                Debug.LogWarning("UIFactory: Shader 'UI/GrayscaleTint' not found in Resources. Will fall back to CPU grayscale.");
                return null;
            }
            _grayscaleTintMat = new Material(sh)
            {
                hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor
            };
            return _grayscaleTintMat;
        }

        private static void ApplyTintMaterial(Image img)
        {
            if (img == null) return;
            if (!BaseUIStyle.UseGrayscaleTint)
                return; // use default
            var tracker = img.GetComponent<GrayscaleTracker>();
            var src = img.sprite;

            if (tracker != null)
            {
                // If this image currently uses a tracked grayscale sprite, recover the original source.
                if (tracker.Gray != null && img.sprite == tracker.Gray)
                    src = tracker.Source;
                // If sprite changed away from tracked grayscale, release previous.
                else if (tracker.Source != null && tracker.Gray != null && img.sprite != tracker.Gray)
                {
                    GrayscaleSpriteCache.Release(tracker.Source);
                    tracker.Source = null;
                    tracker.Gray = null;
                }
            }

            var mat = GetGrayscaleTintMaterial();
            if (mat == null)
            {
                // Fallback: CPU grayscale sprite, then let UI/Default tint it
                var gray = (tracker != null && tracker.Source == src && tracker.Gray != null)
                    ? tracker.Gray
                    : GrayscaleSpriteCache.Get(src);
                if (gray != null)
                {
                    if (tracker == null) tracker = img.gameObject.AddComponent<GrayscaleTracker>();

                    img.sprite = gray;
                    img.material = null;
                    tracker.Source = src;
                    tracker.Gray = gray;
                }
                else if (tracker != null && tracker.Source == null)
                {
                    UnityObject.Destroy(tracker);
                }
                Debug.LogWarning($"UIFactory: Using CPU grayscale fallback on {img.transform.GetHierarchyPath()}.");
                return;
            }

            // Using shader-based tinting; ensure any tracked sprite is released
            if (tracker != null && tracker.Source != null)
            {
                GrayscaleSpriteCache.Release(tracker.Source);
                tracker.Source = null;
                tracker.Gray = null;
                UnityObject.Destroy(tracker);
                tracker = null;
            }

            img.material = new Material(mat); // per-image instance
            img.SetMaterialDirty();
            // Runtime verification (no editor guard):
            var actual = img.materialForRendering != null ? img.materialForRendering.shader.name : "<null>";
            if (actual != "UI/GrayscaleTint")
                Debug.LogWarning($"UIFactory: Expected 'UI/GrayscaleTint' but got '{actual}' on {img.transform.GetHierarchyPath()}.");
            else
                Debug.Log($"UIFactory: GrayscaleTint OK on {img.transform.GetHierarchyPath()}.");
        }

        private static Sprite MakeUniformBorderFromTopBottom(Sprite src)
        {
            // Unity's Sprite.border order: (left, bottom, right, top)
            // Ensure we compare Top (w) and Bottom (y).
            var b = src.border;
            // Force all four edges to match the thinner of Top/Bottom to keep corners safe
            float tb = Mathf.Min(b.w, b.y);
            
            // Sprite.Create expects pivot relative to rect (0..1). Convert existing pixel pivot.
            var rect = src.rect;
            var pivotNormalized = new Vector2(
                rect.width  > 0 ? src.pivot.x / rect.width  : 0.5f,
                rect.height > 0 ? src.pivot.y / rect.height : 0.5f
            );

            var sym = Sprite.Create(
                src.texture,
                rect,
                pivotNormalized,
                src.pixelsPerUnit,
                0,
                SpriteMeshType.FullRect,
                new Vector4(tb, tb, tb, tb)
            );
            return sym;
        }

        // Compute a pixelsPerUnitMultiplier that yields a precise on-screen thickness in pixels for the 9-slice edges.
        // Assumes the Sprite has equal borders on all sides (enforced by GetSymmetricDarkBorder).
        // Formula: displayed_px ≈ slice_px / (spritePPU * multiplier) * canvasRefPPU * canvasScaleFactor
        // We solve for multiplier so displayed_px == targetPixels.
        private static float ComputeBorderScale(Sprite s, float targetPixels, float canvasRefPPU, float canvasScaleFactor)
        {
            if (s == null) return 1f;
            float borderPx = s.border.w; // any side; they're equal
            float tpx = Mathf.Max(0.0001f, targetPixels);
            float spritePPU = Mathf.Max(0.0001f, s.pixelsPerUnit);
            float refPPU = Mathf.Max(0.0001f, canvasRefPPU);
            float scale = Mathf.Max(0.0001f, canvasScaleFactor);
            // Solve: targetPixels = borderPx / (spritePPU * m) * refPPU * scale
            float m = (borderPx * refPPU * scale) / (spritePPU * tpx);
            return Mathf.Max(0.0001f, m);
        }

        // Find nearest parent Canvas to get the current Canvas.scaleFactor (for pixel-accurate computation)
        private static float GetCanvasScaleFactor(Transform t)
        {
            var canvas = t.GetComponentInParent<Canvas>();
            if (canvas != null) return canvas.scaleFactor <= 0f ? 1f : canvas.scaleFactor;
            return 1f;
        }

        private static float GetCanvasRefPPU(Transform t)
        {
            var canvas = t.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                return canvas.referencePixelsPerUnit <= 0f ? 100f : canvas.referencePixelsPerUnit;
            }
            return 100f;
        }

        private static void AttachPixelSnap(Component c)
        {
            if (c == null) return;
            if (c.gameObject.GetComponent<UIPixelSnap>() == null)
                c.gameObject.AddComponent<UIPixelSnap>();
        }

        private static RectTransform CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            go.transform.SetParent(parent, false);
            return rt;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        public static LayoutElement SetFlex(RectTransform rt, float flexW = 0f, float flexH = 0f, float prefW = -1f, float prefH = -1f)
        {
            var le = rt.GetComponent<LayoutElement>() ?? rt.gameObject.AddComponent<LayoutElement>();
            le.flexibleWidth = flexW;
            le.flexibleHeight = flexH;
            le.preferredWidth = prefW;
            le.preferredHeight = prefH;
            return le;
        }

        public static (RectTransform root, RectTransform content) CreateScreenOverlay(Transform parent, string name = "ScreenOverlay", bool dim = false)
        {
            var root = CreateUIObject(name, parent);
            Stretch(root);
            var img = root.gameObject.AddComponent<Image>();
            var woodBg = GetWoodTile();
            if (woodBg == null)
                Debug.LogWarning($"UIFactory: Wood tile sprite missing at {BaseUIStyle.WoodTilePath} for {root.GetHierarchyPath()}");
            img.sprite = woodBg;
            img.type = Image.Type.Tiled;
            img.raycastTarget = true;
            img.color = dim ? new Color(0f, 0f, 0f, 0.5f) : Color.white;
            var content = CreateUIObject("Content", root);
            Stretch(content);
            return (root, content);
        }

        public static (RectTransform a, RectTransform b) SplitHorizontal(RectTransform parent, float leftPct, int gapPx = 0)
        {
            leftPct = Mathf.Clamp01(leftPct);
            var a = CreateUIObject("Left", parent);
            var b = CreateUIObject("Right", parent);
            a.anchorMin = new Vector2(0,0); a.anchorMax = new Vector2(leftPct,1);
            a.offsetMin = new Vector2(0,0); a.offsetMax = new Vector2(-gapPx/2f,0);
            b.anchorMin = new Vector2(leftPct,0); b.anchorMax = new Vector2(1,1);
            b.offsetMin = new Vector2(gapPx/2f,0); b.offsetMax = new Vector2(0,0);
            return (a,b);
        }

        public static (RectTransform a, RectTransform b) SplitVertical(RectTransform parent, float topPct, int gapPx = 0)
        {
            topPct = Mathf.Clamp01(topPct);
            var a = CreateUIObject("Top", parent);
            var b = CreateUIObject("Bottom", parent);
            a.anchorMin = new Vector2(0,1-topPct); a.anchorMax = new Vector2(1,1);
            a.offsetMin = new Vector2(0,gapPx/2f); a.offsetMax = new Vector2(0,0);
            b.anchorMin = new Vector2(0,0); b.anchorMax = new Vector2(1,1-topPct);
            b.offsetMin = new Vector2(0,0); b.offsetMax = new Vector2(0,-gapPx/2f);
            return (a,b);
        }

        public static RectTransform Inset(RectTransform parent, int l = 0, int r = 0, int t = 0, int b = 0)
        {
            var c = CreateUIObject("Inset", parent);
            c.anchorMin = Vector2.zero; c.anchorMax = Vector2.one;
            c.offsetMin = new Vector2(l, b); c.offsetMax = new Vector2(-r, -t);
            return c;
        }

        // ===== Board helpers (snap panels together) =====
        public readonly struct Board
        {
            public readonly RectTransform Root;    // fullscreen container (HorizontalLayoutGroup lives elsewhere)
            public readonly RectTransform Content; // padded inner area
            public Board(RectTransform root, RectTransform content) { Root = root; Content = content; }
        }

        /// <summary>
        /// Creates a full-screen tiled wood background and returns a padded content area for placing columns.
        /// </summary>
        public static Board CreateBoardScreen(Transform parent, float padding = 32f, float spacing = 16f)
        {
            var root = CreateUIObject("BoardRoot", parent);
            Stretch(root);

            // Background (tiled wood) that ignores layout
            var woodBg = GetWoodTile();
            if (woodBg == null)
                Debug.LogWarning($"UIFactory: Wood tile sprite missing at {BaseUIStyle.WoodTilePath} for {root.GetHierarchyPath()}");
            var bg = CreateFullscreenBackground(root, woodBg, new Color(0.05f, 0.04f, 0.03f, 1f));
            bg.type = Image.Type.Tiled;
            var bgLE = bg.gameObject.AddComponent<LayoutElement>();
            bgLE.ignoreLayout = true;
            bg.rectTransform.SetAsFirstSibling();

            // Padded content
            var content = CreateUIObject("BoardContent", root);
            var hl = content.gameObject.AddComponent<HorizontalLayoutGroup>();
            hl.childForceExpandWidth = true;
            hl.childForceExpandHeight = true;
            hl.spacing = spacing;
            hl.padding = new RectOffset((int)padding, (int)padding, (int)padding, (int)padding);
            return new Board(root, content);
        }

        /// <summary>
        /// Creates three columns that snap decor together: left/right fixed widths, center flexible.
        /// </summary>
        public static (RectTransform left, RectTransform center, RectTransform right) CreateThreeColumnBoard(RectTransform parent, float leftWidth, float rightWidth, bool joinDecor = true)
        {
            var left = CreateColumn(parent, "LeftColumn", preferredWidth: leftWidth, flexibleWidth: 0);
            var center = CreateColumn(parent, "CenterColumn", preferredWidth: -1, flexibleWidth: 1);
            var right = CreateColumn(parent, "RightColumn", preferredWidth: rightWidth, flexibleWidth: 0);

            if (joinDecor)
            {
                JoinHorizontal(left, center);
                JoinHorizontal(center, right);
            }
            return (left, center, right);
        }

        /// <summary>
        /// Creates a panel-surface column with width behavior already configured.
        /// </summary>
        public static RectTransform CreateColumn(Transform parent, string name, float preferredWidth, float flexibleWidth, bool showFrame = true)
        {
            // Columns are structural: they must fill their slot. Use Flexible sizing explicitly.
            var panel = CreatePanelSurface(parent, name, sizing: PanelSizing.Flexible);
            SetPanelDecorVisible(panel, true);
            if (!showFrame) SetPanelBorders(panel, false, false, false, false);

            var le = panel.GetComponent<LayoutElement>() ?? panel.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth  = preferredWidth;
            le.flexibleWidth   = flexibleWidth;
            le.minWidth        = preferredWidth > 0 ? preferredWidth - 60f : 0f;
            return panel;
        }

        /// <summary>
        /// Hides/shows specific frame edges so adjacent panels "join" without double seams.
        /// Assumes CreatePanelSurface creates children named: BG_Fill (Image), Frame (Image or container).
        /// If Frame is a single 9-slice Image, we mask edges by overlaying thin images.
        /// </summary>
        public static void SetPanelBorders(RectTransform panel, bool left = true, bool right = true, bool top = true, bool bottom = true)
        {
            var frame = panel.Find("Frame");
            if (frame == null) return;
            var frameImg = frame.GetComponent<Image>();
            if (frameImg != null)
            {
                // Simple approach: keep the 9-slice, but add masking quads on hidden edges
                EnsureEdgeMask(panel, "Mask_L", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(4f, 0f)).gameObject.SetActive(!left);
                EnsureEdgeMask(panel, "Mask_R", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(4f, 0f)).gameObject.SetActive(!right);
                EnsureEdgeMask(panel, "Mask_T", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 4f)).gameObject.SetActive(!top);
                EnsureEdgeMask(panel, "Mask_B", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 4f)).gameObject.SetActive(!bottom);
                return;
            }
        }

        private static RectTransform EnsureEdgeMask(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 size)
        {
            var t = parent.Find(name) as RectTransform;
            if (t == null)
            {
                t = CreateUIObject(name, parent);
                var img = t.gameObject.AddComponent<Image>();
                img.color = new Color(0f, 0f, 0f, 1f); // matches border color
            }
            t.anchorMin = anchorMin;
            t.anchorMax = anchorMax;
            t.sizeDelta = size;
            t.anchoredPosition = Vector2.zero;
            t.SetAsLastSibling();
            return t;
        }

        public static void JoinHorizontal(RectTransform left, RectTransform right)
        {
            var lf = left.GetComponent<UIFrame>();
            var rf = right.GetComponent<UIFrame>();
            if (lf != null || rf != null)
            {
                if (lf != null) lf.SetEdges(true, false, true, true);
                if (rf != null) rf.SetEdges(false, true, true, true);
            }
            else
            {
                SetPanelBorders(left,  left: true,  right: false, top: true, bottom: true);
                SetPanelBorders(right, left: false, right: true,  top: true, bottom: true);
            }
        }

        public static void JoinVertical(RectTransform top, RectTransform bottom)
        {
            var tf = top.GetComponent<UIFrame>();
            var bf = bottom.GetComponent<UIFrame>();
            if (tf != null || bf != null)
            {
                if (tf != null) tf.SetEdges(true, true, true, false);
                if (bf != null) bf.SetEdges(true, true, false, true);
            }
            else
            {
                SetPanelBorders(top, left: true, right: true, top: true, bottom: false);
                SetPanelBorders(bottom, left: true, right: true, top: false, bottom: true);
            }
        }

        public static RectTransform CreateRow(Transform parent, float spacing = 8f)
        {
            var row = new GameObject("Row", typeof(RectTransform));
            var rt = row.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            var h = row.AddComponent<HorizontalLayoutGroup>();
            h.spacing = spacing;
            h.padding = new RectOffset(0, 0, 0, 0);
            h.childAlignment = TextAnchor.MiddleLeft;
            h.childControlHeight = true;
            h.childForceExpandHeight = true;
            h.childControlWidth = true;
            h.childForceExpandWidth = true;
            return rt;
        }

        public static RectTransform CreateCol(Transform parent, float spacing = 8f)
        {
            var col = new GameObject("Col", typeof(RectTransform));
            var rt = col.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            var v = col.AddComponent<VerticalLayoutGroup>();
            v.spacing = spacing;
            v.padding = new RectOffset(0, 0, 0, 0);
            v.childAlignment = TextAnchor.UpperLeft;
            v.childControlHeight = true;
            v.childForceExpandHeight = true;
            v.childControlWidth = true;
            v.childForceExpandWidth = true;
            return rt;
        }

        public static RectTransform CreateSpacer(Transform parent, float flexW = 1f)
        {
            var go = new GameObject("Spacer", typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.flexibleWidth = flexW;
            le.minWidth = 0f;
            return rt;
        }

        /// <summary>
        /// Themed horizontal rule for visual separation. Uses theme border tone.
        /// </summary>
        public static Image CreateRuleHorizontal(Transform parent, float thickness = 2f, float alpha = 0.65f)
        {
            var rt = CreateUIObject("Rule_H", parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, alpha);
            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.minHeight = thickness;
            le.preferredHeight = thickness;
            le.flexibleHeight = 0f;
            // Stretch full width within its layout group
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return img;
        }

        // PANEL (Textured wood fill + dark 9-slice border)
        public static RectTransform CreatePanelSurface(Transform parent, string name = "Panel", TintTheme? theme = null, PanelSizing sizing = PanelSizing.AutoBoth)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(VerticalLayoutGroup));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            var rootImg = go.GetComponent<Image>();

            // Layout configuration (unchanged)
            var layout = go.GetComponent<VerticalLayoutGroup>();
            layout.spacing = BaseUIStyle.StackSpacing;
            layout.padding = new RectOffset(BaseUIStyle.PanelPadding, BaseUIStyle.PanelPadding, BaseUIStyle.PanelPadding, BaseUIStyle.PanelPadding);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            var fitter = go.GetComponent<ContentSizeFitter>();
            if (sizing == PanelSizing.Flexible)
            {
                if (fitter) UnityEngine.Object.DestroyImmediate(fitter);
                SetFlex(rt, flexW: 1f, flexH: 1f);
            }
            else
            {
                if (fitter == null) fitter = go.AddComponent<ContentSizeFitter>();
                fitter.horizontalFit = (sizing == PanelSizing.AutoBoth || sizing == PanelSizing.AutoWidth) ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;
                fitter.verticalFit   = (sizing == PanelSizing.AutoBoth || sizing == PanelSizing.AutoHeight) ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;
            }

            // --- Fill (tiled wood) ---
            var fillGO = new GameObject("BG_Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
            fillGO.transform.SetParent(go.transform, false);
            var fillRt = fillGO.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = Vector2.zero; fillRt.offsetMax = Vector2.zero;
            var fillImg = fillGO.GetComponent<Image>();
            fillImg.raycastTarget = false;
            var le = fillGO.GetComponent<LayoutElement>();
            le.ignoreLayout = true;
            var wood = GetWoodTile();
            if (wood == null)
                Debug.LogWarning($"UIFactory: Wood tile sprite missing at {BaseUIStyle.WoodTilePath} for {fillGO.transform.GetHierarchyPath()}");
            fillImg.sprite = wood;
            fillImg.type = Image.Type.Tiled;
            fillImg.preserveAspect = false;
            ApplyTintMaterial(fillImg);
            var panelTheme = theme ?? BaseUIStyle.SecondaryTheme;
            fillImg.color = panelTheme.Base;
            // --- Pixel-precise frame instead of 9-slice Image ---
            var borderGO = new GameObject("Frame", typeof(RectTransform), typeof(CanvasRenderer), typeof(LayoutElement), typeof(UIFrame));
            borderGO.transform.SetParent(go.transform, false);
            var borderRt = borderGO.GetComponent<RectTransform>();
            borderRt.anchorMin = Vector2.zero; borderRt.anchorMax = Vector2.one;
            borderRt.offsetMin = Vector2.zero; borderRt.offsetMax = Vector2.zero;
            borderGO.GetComponent<LayoutElement>().ignoreLayout = true;
            var frame = borderGO.GetComponent<UIFrame>();
            var border = GetSymmetricDarkBorder();
            frame.Init(border, BaseUIStyle.TargetBorderPx, Color.white);

            // Root image no longer draws visuals; keep it disabled but present for easy toggling
            rootImg.enabled = false;
            rootImg.raycastTarget = false;

            // Pixel snap to avoid uneven vertical vs horizontal edges
            AttachPixelSnap(go.transform);
            AttachPixelSnap(borderGO.transform);

            // Ensure BG_Fill under Frame
            var fill = go.transform.Find("BG_Fill");
            var borderT = go.transform.Find("Frame");
            if (fill) fill.SetSiblingIndex(0);
            if (borderT) borderT.SetSiblingIndex(1);

            return rt;
        }

        // BUTTONS
        public static Button CreateButtonPrimary(Transform parent, string label, Action onClick) => CreateButton(parent, label, BaseUIStyle.TextSecondary, onClick, BaseUIStyle.GoldTheme);
        public static Button CreateButtonSecondary(Transform parent, string label, Action onClick) => CreateButton(parent, label, BaseUIStyle.TextPrimary, onClick, BaseUIStyle.SecondaryTheme);
        public static Button CreateButtonDanger(Transform parent, string label, Action onClick) => CreateButton(parent, label, BaseUIStyle.TextSecondary, onClick, BaseUIStyle.DangerTheme);

        // Textured button: tiled wood fill + 9-slice dark border; wood itself tints for state changes
        private static Button CreateButton(Transform parent, string label, Color textColor, Action onClick, TintTheme theme)
        {
            var go = new GameObject($"Button_{label}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(380, BaseUIStyle.ButtonHeight);

            // Root image handles raycasts, but does not draw visuals
            var rootImg = go.GetComponent<Image>();
            rootImg.sprite = null; rootImg.color = new Color(0,0,0,0); rootImg.raycastTarget = true;
            var btn = go.GetComponent<Button>();

            // --- Background fill (wood, tiled) ---
            var fillGO = new GameObject("BG_Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
            fillGO.transform.SetParent(go.transform, false);
            var fillRt = fillGO.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = Vector2.zero; fillRt.offsetMax = Vector2.zero; // full-bleed wood
            var fillImg = fillGO.GetComponent<Image>();
            fillImg.raycastTarget = false;
            fillImg.preserveAspect = false;
            fillGO.GetComponent<LayoutElement>().ignoreLayout = true;
            var wood = GetWoodTile();
            if (wood == null)
                Debug.LogWarning($"UIFactory: Wood tile sprite missing at {BaseUIStyle.WoodTilePath} for {fillGO.transform.GetHierarchyPath()}");
            fillImg.sprite = wood;
            fillImg.type = Image.Type.Tiled;
            ApplyTintMaterial(fillImg);
            fillImg.color = theme.Base; // tint the wood itself
            // --- Pixel-precise frame instead of 9-slice Image ---
            var borderGO = new GameObject("Frame", typeof(RectTransform), typeof(CanvasRenderer), typeof(LayoutElement), typeof(UIFrame));
            borderGO.transform.SetParent(go.transform, false);
            var borderRt = borderGO.GetComponent<RectTransform>();
            borderRt.anchorMin = Vector2.zero; borderRt.anchorMax = Vector2.one;
            borderRt.offsetMin = Vector2.zero; borderRt.offsetMax = Vector2.zero;
            borderGO.GetComponent<LayoutElement>().ignoreLayout = true;
            var frame = borderGO.GetComponent<UIFrame>();
            var border = GetSymmetricDarkBorder();
            frame.Init(border, BaseUIStyle.TargetBorderPx, Color.white);

            // Button setup
            btn.transition = Selectable.Transition.ColorTint;
            // Make the wood fill the targetGraphic so the texture itself tints for state changes
            btn.targetGraphic = fillImg;
            var cb = btn.colors;
            cb.colorMultiplier = 1f;
            cb.fadeDuration = 0.08f;
            cb.normalColor      = theme.Base;
            cb.highlightedColor = theme.Hover;
            cb.pressedColor     = theme.Pressed;
            cb.selectedColor    = theme.Base;
            var baseCol = (Color)theme.Base;
            baseCol.a *= 0.6f;
            cb.disabledColor    = baseCol;
            btn.colors = cb;
            btn.onClick.AddListener(() => onClick?.Invoke());

            // Pixel snapping on root & border to keep 1px borders even
            AttachPixelSnap(go.transform);
            AttachPixelSnap(borderGO.transform);

            // Layout sizing so buttons are visible in the stack
            var le = go.GetComponent<LayoutElement>();
            le.preferredWidth = 420;
            le.minHeight = BaseUIStyle.ButtonHeight;
            le.flexibleWidth = 0;
            le.flexibleHeight = 0;

            // Label
            var textGO = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textGO.transform.SetParent(go.transform, false);
            var trt = textGO.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0, 0);
            trt.anchorMax = new Vector2(1, 1);
            trt.offsetMin = Vector2.zero; // true center
            trt.offsetMax = Vector2.zero;
            var txt = textGO.GetComponent<Text>();
            txt.text = label;
            txt.alignment = TextAnchor.MiddleCenter; // center-aligned labels
            txt.fontSize = BaseUIStyle.ButtonFontSize;
            txt.color = textColor;
            // Explicit font (avoid missing-font in builds)
            try { txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch {}
            txt.raycastTarget = false;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Truncate;
            // Subtle outline for readability over busy backgrounds
            var ol = textGO.AddComponent<Outline>();
            ol.effectColor = new Color(0,0,0,0.5f);
            ol.effectDistance = new Vector2(1f, -1f);

            // Explicit layer order (back -> front)
            fillGO.transform.SetSiblingIndex(0);
            borderGO.transform.SetSiblingIndex(1);
            textGO.transform.SetSiblingIndex(2);

            return btn;
        }

        // Toggle panel chrome (fill + border) without affecting layout or children
        public static void SetPanelDecorVisible(RectTransform panel, bool visible)
        {
            if (!panel) return;
            // Root image (should be disabled by CreatePanelSurface, but handle both cases)
            var rootImg = panel.GetComponent<Image>();
            if (rootImg) { rootImg.enabled = false; rootImg.raycastTarget = false; }

            // Child fill
            var fill = panel.Find("BG_Fill");
            if (fill) fill.gameObject.SetActive(visible);

            // Child border
            var border = panel.Find("Frame");
            if (border) border.gameObject.SetActive(visible);
        }

        /// <summary>
        /// Helper that creates a small bottom-right stacked panel (e.g., debug/info stack).
        /// Shrinks vertically to content.
        /// </summary>
        public static RectTransform CreateBottomRightStack(Transform parent, string name = "BottomRightStack")
        {
            // Explicit AutoHeight so behavior does not depend on CreatePanelSurface defaults
            var rt = CreatePanelSurface(parent, name, sizing: PanelSizing.AutoHeight);
            var le = rt.GetComponent<LayoutElement>() ?? rt.gameObject.AddComponent<LayoutElement>();
            le.flexibleWidth = 0f; le.flexibleHeight = 0f;
            // Anchor to bottom-right
            rt.anchorMin = new Vector2(1, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot     = new Vector2(1, 0);
            rt.anchoredPosition = new Vector2(-12, 12);
            return rt;
        }

        // BACKGROUND IMAGE (full screen)
        public static Image CreateFullscreenBackground(Transform parent, Sprite sprite, Color fallbackColor)
        {
            var go = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>();
            if (sprite != null)
            {
                img.sprite = sprite;
                img.preserveAspect = true; // Background should not intercept clicks
                img.color = Color.white;
            }
            else
            {
                img.color = fallbackColor;
            }
            img.raycastTarget = false; // never block UI
            return img;
        }

    }
}
