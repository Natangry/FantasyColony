using System.Text;
using UnityEngine;
using UnityEngine.UI;
using FantasyColony.UI.Router;

namespace FantasyColony.UI.Screens {
    /// <summary>
    /// Simple UI to present BootReport contents and first N validation messages.
    /// Dev-only utility; keep minimal.
    /// </summary>
    public sealed class BootReportScreen : UIScreenBase {
        private Text _title;
        private Text _body;
        private Button _copy;
        private Button _close;

        public override void Enter(Transform parent) {
            Root = new GameObject("BootReport").AddComponent<RectTransform>();
            Root.SetParent(parent, false);

            var panel = CreatePanel(Root, new Vector2(800,600));
            _title = CreateText(panel, "Boot Report", 24, TextAnchor.MiddleCenter);
            _title.rectTransform.anchoredPosition = new Vector2(0, 250);
            _body = CreateText(panel, "", 14, TextAnchor.UpperLeft);
            _body.rectTransform.sizeDelta = new Vector2(740, 430);
            _body.rectTransform.anchoredPosition = new Vector2(0, 20);
            _body.horizontalOverflow = HorizontalWrapMode.Wrap;
            _body.verticalOverflow = VerticalWrapMode.Truncate;

            _copy = CreateButton(panel, "Copy", OnCopyClicked);
            _copy.GetComponent<RectTransform>().anchoredPosition = new Vector2(-120, -250);
            _close = CreateButton(panel, "Close", () => UIRouter.Current?.Pop());
            _close.GetComponent<RectTransform>().anchoredPosition = new Vector2(120, -250);

            Refresh();
        }

        public override void Exit() {
            if (Root != null) {
                UnityEngine.Object.Destroy(Root.gameObject);
                Root = null;
            }
        }

        private void Refresh() {
            var report = FantasyColony.Boot.BootReport.Last;
            if (report == null) { _body.text = "(no report)"; return; }
            var sb = new StringBuilder();
            sb.AppendLine("Tasks:");
            for (int i=0;i<report.steps.Count;i++) {
                var s = report.steps[i];
                sb.AppendLine($" - {s.title}  ({s.seconds:0.000}s){(string.IsNullOrEmpty(s.warn)?"":"  ["+s.warn+"]")}");
            }
            _body.text = sb.ToString();
        }

        private void OnCopyClicked() {
            var report = FantasyColony.Boot.BootReport.Last;
            if (report == null) return;
            var sb = new StringBuilder();
            for (int i=0;i<report.steps.Count;i++) {
                var s = report.steps[i];
                sb.AppendLine($"{s.title}\t{s.seconds:0.000}\t{(s.warn??"").Replace('\n',' ')}");
            }
#if UNITY_EDITOR
            UnityEditor.EditorGUIUtility.systemCopyBuffer = sb.ToString();
#else
            GUIUtility.systemCopyBuffer = sb.ToString();
#endif
        }

        // --- tiny UI helpers (local, to avoid style dependencies) ---
        private RectTransform CreatePanel(Transform parent, Vector2 size) {
            var go = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = size;
            rt.anchoredPosition = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.color = new Color(0,0,0,0.8f);
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(1,1,1,0.05f);
            return rt;
        }
        private Text CreateText(RectTransform parent, string text, int size, TextAnchor anchor) {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.text = text; t.fontSize = size; t.alignment = anchor; t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            var rt = (RectTransform)go.transform; rt.sizeDelta = new Vector2(740,60); rt.anchoredPosition = Vector2.zero;
            return t;
        }
        private Button CreateButton(RectTransform parent, string text, System.Action onClick) {
            var go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform; rt.sizeDelta = new Vector2(160,40);
            var img = go.GetComponent<Image>(); img.color = new Color(1,1,1,0.1f);
            var btn = go.GetComponent<Button>(); btn.onClick.AddListener(() => onClick());
            var label = CreateText(rt, text, 16, TextAnchor.MiddleCenter); label.rectTransform.sizeDelta = rt.sizeDelta;
            return btn;
        }
    }
