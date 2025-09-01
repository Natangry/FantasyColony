using UnityEngine;
using UnityEngine.UI;
using FantasyColony.UI.Style;
using System;
using FantasyColony.UI.Util;
using TintTheme = FantasyColony.UI.Style.BaseUIStyle.TintTheme;

namespace FantasyColony.UI.Widgets
{
    public static class UIFactory
    {
        // Cache a symmetrized version of the dark 9-slice border so L/R and T/B are equal.
        private static Sprite _darkBorderSymmetric;
        private static Material _grayscaleTintMat;

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
            var sh = Shader.Find("UI/GrayscaleTint");
            if (sh != null)
            {
                _grayscaleTintMat = new Material(sh);
            }
            return _grayscaleTintMat;
        }

        private static Sprite MakeUniformBorderFromTopBottom(Sprite src)
        {
            // Unity's Sprite.border order: (left, right, top, bottom)
            var b = src.border;
            // Force all four edges to match the thinner of Top/Bottom to keep corners safe
            float tb = Mathf.Min(b.z, b.w);

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

        // PANEL (Textured wood fill + dark 9-slice border)
        public static RectTransform CreatePanelSurface(Transform parent, string name = "Panel", TintTheme? theme = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
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
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

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
            var wood = Resources.Load<Sprite>(BaseUIStyle.WoodTilePath);
            if (wood != null)
            {
                fillImg.sprite = wood;
                fillImg.type = Image.Type.Tiled;
            }
            var mat = GetGrayscaleTintMaterial();
            if (mat != null) fillImg.material = mat;
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
            var wood = Resources.Load<Sprite>(BaseUIStyle.WoodTilePath);
            if (wood != null)
            {
                fillImg.sprite = wood;
                fillImg.type = Image.Type.Tiled;
            }
            var mat = GetGrayscaleTintMaterial();
            if (mat != null) fillImg.material = mat;
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

        // STACK CONTAINER (Bottom-right MenuPanel)
        public static RectTransform CreateBottomRightStack(Transform parent, string name = "MenuPanel")
        {
            var panel = CreatePanelSurface(parent, name);
            panel.anchorMin = new Vector2(1, 0);
            panel.anchorMax = new Vector2(1, 0);
            panel.pivot = new Vector2(1, 0);
            panel.anchoredPosition = new Vector2(-BaseUIStyle.EdgeOffset, BaseUIStyle.EdgeOffset);

            var layout = panel.GetComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;

            var le = panel.GetComponent<LayoutElement>();
            if (le == null) le = panel.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = 420;
            le.flexibleWidth = 0;
            return panel;
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
