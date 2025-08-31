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
            var img = go.GetComponent<Image>();

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

            // --- Border (9-slice) on root Image ---
            var border = Resources.Load<Sprite>(BaseUIStyle.DarkBorder9SPath);
            if (border != null) { img.sprite = border; img.type = Image.Type.Sliced; img.color = Color.white; }
            else { img.color = BaseUIStyle.PanelSurface; }

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

            var img = go.GetComponent<Image>();
            var btn = go.GetComponent<Button>();

            // --- Background fill (wood, tiled) ---
            var fillGO = new GameObject("BG_Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
            fillGO.transform.SetParent(go.transform, false);
            var fillRt = fillGO.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = Vector2.zero; fillRt.offsetMax = Vector2.zero;
            var fillImg = fillGO.GetComponent<Image>();
            fillImg.raycastTarget = false;
            fillGO.GetComponent<LayoutElement>().ignoreLayout = true;
            var wood = Resources.Load<Sprite>(BaseUIStyle.WoodTilePath);
            if (wood != null) { fillImg.sprite = wood; fillImg.type = Image.Type.Tiled; }
            else { fillImg.color = fill; }

            // --- Border (9-slice) on root image ---
            var border = Resources.Load<Sprite>(BaseUIStyle.DarkBorder9SPath);
            if (border != null) { img.sprite = border; img.type = Image.Type.Sliced; img.color = Color.white; }
            else { img.color = fill; }

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
            return btn;
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
