using UnityEngine;
using UnityEngine.UI;
using FantasyColony.UI.Widgets;

namespace FantasyColony.UI.Creator.Editing
{
    /// <summary>
    /// Runtime-built modal dialog to attach instructions to the selected item.
    /// </summary>
    public static class InstructionDialog
    {
        public static void Show(RectTransform parent, RectTransform target)
        {
            if (parent == null || target == null) { Debug.LogWarning("[UICreator] InstructionDialog missing parent/target"); return; }

            // Blocker overlay
            var overlayGO = new GameObject("InstructionDialog", typeof(RectTransform), typeof(Image));
            var overlay = overlayGO.GetComponent<RectTransform>();
            overlay.SetParent(parent, false);
            overlay.anchorMin = Vector2.zero; overlay.anchorMax = Vector2.one; overlay.pivot = new Vector2(0.5f, 0.5f);
            overlay.offsetMin = overlay.offsetMax = Vector2.zero;
            var blocker = overlayGO.GetComponent<Image>();
            blocker.color = new Color(0, 0, 0, 0.5f);
            blocker.raycastTarget = true;

            // Panel
            var panelGO = UIFactory.CreatePanelSurface(overlay, "Instructions", sizing: PanelSizing.AutoHeight);
            var panel = panelGO.GetComponent<RectTransform>();
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(520, 360);

            // Title
            var titleGO = new GameObject("Title", typeof(RectTransform), typeof(Text));
            var titleRT = titleGO.GetComponent<RectTransform>();
            titleRT.SetParent(panel, false);
            titleRT.anchorMin = new Vector2(0, 1); titleRT.anchorMax = new Vector2(1, 1); titleRT.pivot = new Vector2(0.5f, 1f);
            titleRT.offsetMin = new Vector2(16, -40); titleRT.offsetMax = new Vector2(-16, -8);
            var title = titleGO.GetComponent<Text>();
            title.alignment = TextAnchor.MiddleLeft;
            title.text = "Instructions";

            // Input background
            var inputBG = new GameObject("InputBG", typeof(RectTransform), typeof(Image));
            var inputBGRT = inputBG.GetComponent<RectTransform>();
            inputBGRT.SetParent(panel, false);
            inputBGRT.anchorMin = new Vector2(0, 0); inputBGRT.anchorMax = new Vector2(1, 1); inputBGRT.pivot = new Vector2(0.5f, 0.5f);
            inputBGRT.offsetMin = new Vector2(16, 64); inputBGRT.offsetMax = new Vector2(-16, 64);

            // Input field
            var inputGO = new GameObject("Input", typeof(RectTransform), typeof(Image), typeof(InputField));
            var inputRT = inputGO.GetComponent<RectTransform>();
            inputRT.SetParent(panel, false);
            inputRT.anchorMin = new Vector2(0, 0); inputRT.anchorMax = new Vector2(1, 1); inputRT.pivot = new Vector2(0.5f, 0.5f);
            inputRT.offsetMin = new Vector2(16, 64); inputRT.offsetMax = new Vector2(-16, 96);
            var inputImg = inputGO.GetComponent<Image>(); inputImg.raycastTarget = true;
            var field = inputGO.GetComponent<InputField>();
            field.lineType = InputField.LineType.MultiLineNewline;

            // Text components for InputField
            var textGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
            var textRT = textGO.GetComponent<RectTransform>();
            textRT.SetParent(inputGO.transform, false);
            textRT.anchorMin = new Vector2(0, 0); textRT.anchorMax = new Vector2(1, 1); textRT.pivot = new Vector2(0.5f, 0.5f);
            textRT.offsetMin = new Vector2(8, 8); textRT.offsetMax = new Vector2(-8, -8);
            var text = textGO.GetComponent<Text>();
            text.alignment = TextAnchor.UpperLeft;
            field.textComponent = text;

            var phGO = new GameObject("Placeholder", typeof(RectTransform), typeof(Text));
            var phRT = phGO.GetComponent<RectTransform>();
            phRT.SetParent(inputGO.transform, false);
            phRT.anchorMin = new Vector2(0, 0); phRT.anchorMax = new Vector2(1, 1); phRT.pivot = new Vector2(0.5f, 0.5f);
            phRT.offsetMin = new Vector2(8, 8); phRT.offsetMax = new Vector2(-8, -8);
            var ph = phGO.GetComponent<Text>();
            ph.alignment = TextAnchor.UpperLeft; ph.text = "Type instructions here...";
            ph.color = new Color(1, 1, 1, 0.5f);
            field.placeholder = ph;

            // Load existing note
            var note = target.GetComponent<UIInstructionNote>();
            if (note != null) field.text = note.Note;

            // Buttons container
            var btnBar = new GameObject("Buttons", typeof(RectTransform));
            var btnBarRT = btnBar.GetComponent<RectTransform>();
            btnBarRT.SetParent(panel, false);
            btnBarRT.anchorMin = new Vector2(0, 0); btnBarRT.anchorMax = new Vector2(1, 0); btnBarRT.pivot = new Vector2(0.5f, 0f);
            btnBarRT.sizeDelta = new Vector2(0, 48); btnBarRT.anchoredPosition = new Vector2(0, 16);

            // Save button
            var save = CreateButton(btnBarRT, "Save", new Vector2(1f, 0.5f), new Vector2(-16, 0));
            save.onClick.AddListener(() =>
            {
                var n = target.GetComponent<UIInstructionNote>();
                if (n == null) n = target.gameObject.AddComponent<UIInstructionNote>();
                n.Note = field.text ?? string.Empty;
                Object.Destroy(overlayGO);
                Debug.Log($"[UICreator] Instructions saved for {target.name}");
            });

            // Cancel button
            var cancel = CreateButton(btnBarRT, "Cancel", new Vector2(0f, 0.5f), new Vector2(16, 0));
            cancel.onClick.AddListener(() =>
            {
                Object.Destroy(overlayGO);
                Debug.Log("[UICreator] Instructions canceled");
            });
        }

        private static Button CreateButton(RectTransform parent, string label, Vector2 anchor, Vector2 margin)
        {
            var go = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(120, 36);
            rt.anchoredPosition = new Vector2(Mathf.Lerp(-parent.rect.width * 0.5f + 80, parent.rect.width * 0.5f - 80, anchor.x), 0) + margin;
            var btn = go.GetComponent<Button>();

            var txtGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
            var txtRT = txtGO.GetComponent<RectTransform>();
            txtRT.SetParent(rt, false);
            txtRT.anchorMin = new Vector2(0, 0); txtRT.anchorMax = new Vector2(1, 1); txtRT.pivot = new Vector2(0.5f, 0.5f);
            var txt = txtGO.GetComponent<Text>();
            txt.alignment = TextAnchor.MiddleCenter; txt.text = label;
            return btn;
        }
    }
}

