using UnityEngine;
using UnityEngine.UI;
using FantasyColony.UI.Style;
using System;

namespace FantasyColony.UI.Widgets
{
    public static class UIFactory
    {
        // PANEL
        public static RectTransform CreatePanelSurface(Transform parent, string name = "Panel")
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            var img = go.GetComponent<Image>();
            img.color = BaseUIStyle.PanelSurface;

            var layout = go.GetComponent<VerticalLayoutGroup>();
            layout.spacing = BaseUIStyle.StackSpacing;
            layout.padding = new RectOffset(BaseUIStyle.PanelPadding, BaseUIStyle.PanelPadding, BaseUIStyle.PanelPadding, BaseUIStyle.PanelPadding);
            // Ensure children (buttons) get proper space and panel wraps to content reliably
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var fitter = go.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return rt;
        }

        // BUTTONS
        public static Button CreateButtonPrimary(Transform parent, string label, Action onClick) =>
            CreateButton(parent, label, BaseUIStyle.Gold, BaseUIStyle.TextSecondary, onClick);

        public static Button CreateButtonSecondary(Transform parent, string label, Action onClick) =>
            CreateButton(parent, label, BaseUIStyle.SecondaryFill, BaseUIStyle.TextPrimary, onClick);

        public static Button CreateButtonDanger(Transform parent, string label, Action onClick) =>
            CreateButton(parent, label, BaseUIStyle.Danger, BaseUIStyle.TextSecondary, onClick, isDanger:true);

        private static Button CreateButton(Transform parent, string label, Color fill, Color textColor, Action onClick, bool isDanger = false)
        {
            var go = new GameObject($"Button_{label}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(380, BaseUIStyle.ButtonHeight);

            var img = go.GetComponent<Image>();
            img.color = fill;

            var btn = go.GetComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = fill;
            colors.highlightedColor = isDanger ? BaseUIStyle.DangerHover : (fill == BaseUIStyle.Gold ? BaseUIStyle.GoldHover : Multiply(fill, 1.06f));
            colors.pressedColor = isDanger ? BaseUIStyle.DangerPressed : (fill == BaseUIStyle.Gold ? BaseUIStyle.GoldPressed : Multiply(fill, 0.90f));
            colors.disabledColor = new Color(fill.r, fill.g, fill.b, 0.4f);
            colors.colorMultiplier = 1f;
            btn.colors = colors;
            btn.onClick.AddListener(() => onClick?.Invoke());

            // Layout sizing so buttons are visible in the stack
            var le = go.GetComponent<LayoutElement>();
            le.preferredWidth = 420;
            le.minHeight = BaseUIStyle.ButtonHeight;
            le.flexibleWidth = 0; le.flexibleHeight = 0;

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
            // Use default Arial to avoid TMP dependency; can swap later.
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

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
                img.preserveAspect = true; // Placeholder background scaling; fills via CanvasScaler
                img.color = Color.white;
            }
            else
            {
                img.color = fallbackColor;
            }
            return img;
        }

        private static Color Multiply(Color c, float f) => new Color(c.r * f, c.g * f, c.b * f, c.a);
    }
}
