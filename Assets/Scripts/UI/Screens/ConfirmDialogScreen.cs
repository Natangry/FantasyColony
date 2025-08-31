using System;
using UnityEngine;
using UnityEngine.UI;
using FantasyColony.UI.Router;
using FantasyColony.UI.Widgets;

namespace FantasyColony.UI.Screens
{
    // Simple reusable confirmation dialog with a dimming overlay
    public class ConfirmDialogScreen : IScreen
    {
        // Configure via UIRouter.Current.Push<ConfirmDialogScreen>(init => { ... })
        public string Title;
        public string Message;
        public string ConfirmLabel = "OK";
        public string CancelLabel = "Cancel";
        public Action OnConfirm;
        public Action OnCancel;

        public GameObject Root { get; private set; }

        public void Enter(Transform parent)
        {
            Root = new GameObject("ConfirmDialog");
            var rt = Root.AddComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            // Dimmer to block clicks and darken background
            var dim = new GameObject("Dimmer");
            var dimRt = dim.AddComponent<RectTransform>();
            dimRt.SetParent(rt, false);
            dimRt.anchorMin = Vector2.zero; dimRt.anchorMax = Vector2.one;
            dimRt.offsetMin = Vector2.zero; dimRt.offsetMax = Vector2.zero;
            var dimImg = dim.AddComponent<Image>();
            dimImg.color = new Color(0f, 0f, 0f, 0.55f);
            dimImg.raycastTarget = true;

            // Dialog panel
            var panel = new GameObject("Panel");
            var prt = panel.AddComponent<RectTransform>();
            prt.SetParent(rt, false);
            prt.sizeDelta = new Vector2(520, 280);
            prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 0.5f);
            prt.anchoredPosition = Vector2.zero;
            var panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0.231f, 0.200f, 0.161f); // matches SecondaryFill-ish

            var layout = panel.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = true; layout.childControlWidth = true;
            layout.childForceExpandHeight = false; layout.childForceExpandWidth = false;
            layout.padding = new RectOffset(24, 24, 24, 24);
            layout.spacing = 12;

            // Title
            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(panel.transform, false);
            var titleText = titleGO.AddComponent<Text>();
            titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            titleText.fontSize = 26; titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.white;
            titleText.horizontalOverflow = HorizontalWrapMode.Wrap;
            titleText.verticalOverflow = VerticalWrapMode.Truncate;
            titleText.text = string.IsNullOrEmpty(Title) ? "Confirm" : Title;

            // Message
            var msgGO = new GameObject("Message");
            msgGO.transform.SetParent(panel.transform, false);
            var msgText = msgGO.AddComponent<Text>();
            msgText.font = titleText.font;
            msgText.fontSize = 16; msgText.alignment = TextAnchor.MiddleCenter;
            msgText.color = new Color(0.9f, 0.9f, 0.9f, 0.95f);
            msgText.horizontalOverflow = HorizontalWrapMode.Wrap;
            msgText.verticalOverflow = VerticalWrapMode.Overflow;
            msgText.text = string.IsNullOrEmpty(Message) ? string.Empty : Message;

            // Buttons row
            var row = new GameObject("Buttons");
            row.transform.SetParent(panel.transform, false);
            var rowLayout = row.AddComponent<HorizontalLayoutGroup>();
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.spacing = 16;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;
            rowLayout.padding = new RectOffset(0, 0, 8, 0);

            // Cancel / Confirm buttons using UIFactory
            UIFactory.CreateButtonSecondary(row, string.IsNullOrEmpty(CancelLabel) ? "Cancel" : CancelLabel, () =>
            {
                UIRouter.Current?.Pop();
                OnCancel?.Invoke();
            });

            UIFactory.CreateButtonDanger(row, string.IsNullOrEmpty(ConfirmLabel) ? "OK" : ConfirmLabel, () =>
            {
                var router = UIRouter.Current;
                router?.Pop(); // close dialog first so new screens appear above
                OnConfirm?.Invoke();
            });
        }

        public void Exit()
        {
            if (Root != null)
            {
                UnityEngine.Object.Destroy(Root);
                Root = null;
            }
        }
    }
}
