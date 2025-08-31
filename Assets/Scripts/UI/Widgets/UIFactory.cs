using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FantasyColony.UI.Style;
using System;

namespace FantasyColony.UI.Widgets
{
    public static class UIFactory
    {
        // PANEL (Textured wood fill + dark 9-slice border)
        public static RectTransform CreatePanelSurface(Transform parent, string name = "Panel")
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
            if (wood != null) { fillImg.sprite = wood; fillImg.type = Image.Type.Tiled; }
            else { fillImg.color = BaseUIStyle.SecondaryFill; }

            // --- Border (9-slice) as sibling ABOVE fill ---
            var borderGO = new GameObject("BG_Border", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
            borderGO.transform.SetParent(go.transform, false);
            var borderRt = borderGO.GetComponent<RectTransform>();
            borderRt.anchorMin = Vector2.zero; borderRt.anchorMax = Vector2.one;
            borderRt.offsetMin = Vector2.zero; borderRt.offsetMax = Vector2.zero;
            var borderImg = borderGO.GetComponent<Image>();
            borderImg.raycastTarget = false;
            borderImg.preserveAspect = false;
            borderGO.GetComponent<LayoutElement>().ignoreLayout = true;
            var border = Resources.Load<Sprite>(BaseUIStyle.DarkBorder9SPath);
            if (border != null) { borderImg.sprite = border; borderImg.type = Image.Type.Sliced; borderImg.color = Color.white; }
            else { borderImg.color = BaseUIStyle.PanelSurface; }

            // Root image no longer draws visuals; keep it disabled but present for easy toggling
            rootImg.enabled = false;
            rootImg.raycastTarget = false;

            // Ensure BG_Fill under BG_Border
            var fill = go.transform.Find("BG_Fill");
            var borderT = go.transform.Find("BG_Border");
            if (fill) fill.SetSiblingIndex(0);
            if (borderT) borderT.SetSiblingIndex(1);

            return rt;
        }

        // BUTTONS
        public static Button CreateButtonPrimary(Transform parent, string label, Action onClick) => CreateButton(parent, label, BaseUIStyle.Gold, BaseUIStyle.TextSecondary, onClick);
        public static Button CreateButtonSecondary(Transform parent, string label, Action onClick) => CreateButton(parent, label, BaseUIStyle.SecondaryFill, BaseUIStyle.TextPrimary, onClick);
        public static Button CreateButtonDanger(Transform parent, string label, Action onClick) => CreateButton(parent, label, BaseUIStyle.Danger, BaseUIStyle.TextSecondary, onClick, isDanger:true);

        // Textured button: tiled wood fill + 9-slice dark border + overlay for state tints
        private static Button CreateButton(Transform parent, string label, Color fill, Color textColor, Action onClick, bool isDanger = false)
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
            fillRt.offsetMin = Vector2.zero; fillRt.offsetMax = Vector2.zero;
            var fillImg = fillGO.GetComponent<Image>();
            fillImg.raycastTarget = false;
            fillImg.preserveAspect = false;
            fillGO.GetComponent<LayoutElement>().ignoreLayout = true;
            var wood = Resources.Load<Sprite>(BaseUIStyle.WoodTilePath);
            if (wood != null) { fillImg.sprite = wood; fillImg.type = Image.Type.Tiled; }
            else { fillImg.color = fill; }

            // --- Border (9-slice) as child ABOVE fill ---
            var borderGO = new GameObject("BG_Border", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
            borderGO.transform.SetParent(go.transform, false);
            var borderRt = borderGO.GetComponent<RectTransform>();
            borderRt.anchorMin = Vector2.zero; borderRt.anchorMax = Vector2.one;
            borderRt.offsetMin = Vector2.zero; borderRt.offsetMax = Vector2.zero;
            var borderImg = borderGO.GetComponent<Image>();
            borderImg.raycastTarget = false;
            borderImg.preserveAspect = false;
            borderGO.GetComponent<LayoutElement>().ignoreLayout = true;
            var border = Resources.Load<Sprite>(BaseUIStyle.DarkBorder9SPath);
            if (border != null) { borderImg.sprite = border; borderImg.type = Image.Type.Sliced; borderImg.color = Color.white; }
            else { borderImg.color = fill; }

            // --- Overlay for state tints (Button.targetGraphic) ---
            var overlayGO = new GameObject("Overlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
            overlayGO.transform.SetParent(go.transform, false);
            var overlayRt = overlayGO.GetComponent<RectTransform>();
            overlayRt.anchorMin = Vector2.zero; overlayRt.anchorMax = Vector2.one;
            overlayRt.offsetMin = Vector2.zero; overlayRt.offsetMax = Vector2.zero;
            var overlayImg = overlayGO.GetComponent<Image>();
            overlayImg.color = new Color(1,1,1,0f);
            overlayImg.raycastTarget = false;
            overlayGO.GetComponent<LayoutElement>().ignoreLayout = true;

            // Button setup
            btn.transition = Selectable.Transition.ColorTint;
            btn.targetGraphic = overlayImg; // only overlay is tinted by ColorBlock
            var colors = btn.colors;
            colors.normalColor = new Color(1,1,1,0f); // transparent overlay
            colors.highlightedColor = BaseUIStyle.HoverOverlay;
            colors.pressedColor = BaseUIStyle.PressedOverlay;
            colors.selectedColor = colors.normalColor;
            colors.disabledColor = BaseUIStyle.DisabledOverlay;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            btn.colors = colors;
            btn.onClick.AddListener(() => onClick?.Invoke());

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
            trt.offsetMin = new Vector2(16, 0);
            trt.offsetMax = new Vector2(-16, 0);
            var txt = textGO.GetComponent<Text>();
            txt.text = label;
            txt.alignment = TextAnchor.MiddleLeft;
            txt.fontSize = BaseUIStyle.ButtonFontSize;
            txt.color = textColor;
            txt.raycastTarget = false;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Truncate;

            // Explicit layer order (back -> front)
            fillGO.transform.SetSiblingIndex(0);
            borderGO.transform.SetSiblingIndex(1);
            overlayGO.transform.SetSiblingIndex(2);
            textGO.transform.SetSiblingIndex(3);

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
            var border = panel.Find("BG_Border");
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

        private static Color Multiply(Color c, float f) => new Color(c.r * f, c.g * f, c.b * f, c.a);

        private static void ClearSelectionIfMouseClick()
        {
            var es = EventSystem.current;
            if (es != null && Input.mousePresent)
            {
                es.SetSelectedGameObject(null);
            }
        }
    }
}
