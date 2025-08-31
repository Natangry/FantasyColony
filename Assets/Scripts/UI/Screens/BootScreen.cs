using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using FantasyColony.UI.Router;
using FantasyColony.Boot;

namespace FantasyColony.UI.Screens
{
    // Boot screen: shows a cover and phase text while the app boot pipeline runs
    public class BootScreen : IScreen
    {
        public string Title = "Loading";
        public GameObject Root { get; private set; }
        private Text _phaseText;

        public void Enter(Transform parent)
        {
            Root = new GameObject("BootScreen");
            var rt = Root.AddComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            // Solid dark cover (prevents flash)
            var bg = new GameObject("Background");
            var bgrt = bg.AddComponent<RectTransform>();
            bgrt.SetParent(rt, false);
            bgrt.anchorMin = Vector2.zero; bgrt.anchorMax = Vector2.one;
            bgrt.offsetMin = Vector2.zero; bgrt.offsetMax = Vector2.zero;
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.10f, 0.09f, 0.08f, 1f);
            bgImg.raycastTarget = true;

            // Centered title
            var titleGO = new GameObject("Title");
            var tr = titleGO.AddComponent<RectTransform>();
            tr.SetParent(rt, false);
            tr.anchorMin = tr.anchorMax = new Vector2(0.5f, 0.5f);
            tr.anchoredPosition = new Vector2(0f, 24f);
            tr.sizeDelta = new Vector2(640f, 80f);
            var text = titleGO.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 24; text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.text = string.IsNullOrEmpty(Title) ? "Loading" : Title;

            // Phase text (small subtitle)
            var phaseGO = new GameObject("Phase");
            var pr = phaseGO.AddComponent<RectTransform>();
            pr.SetParent(rt, false);
            pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
            pr.anchoredPosition = new Vector2(0f, -16f);
            pr.sizeDelta = new Vector2(720f, 40f);
            _phaseText = phaseGO.AddComponent<Text>();
            _phaseText.font = text.font;
            _phaseText.fontSize = 18; _phaseText.alignment = TextAnchor.MiddleCenter;
            _phaseText.color = new Color(0.92f, 0.92f, 0.92f, 0.95f);
            _phaseText.horizontalOverflow = HorizontalWrapMode.Overflow;
            _phaseText.verticalOverflow = VerticalWrapMode.Truncate;
            _phaseText.text = string.Empty;

            // Kick off the boot pipeline on the next frame
            var runner = Root.AddComponent<Runner>();
            runner.Begin(() => runner.StartCoroutine(BootCo()));
        }

        public void Exit()
        {
            if (Root != null)
            {
                UnityEngine.Object.Destroy(Root);
                Root = null;
            }
        }

        private IEnumerator BootCo()
        {
            yield return null; // render once so the cover is visible
            yield return AppBootstrap.Run(SetPhase);
            Time.timeScale = 1f;
            UIRouter.Current?.ResetTo<MainMenuScreen>();
        }

        private void SetPhase(string s)
        {
            if (_phaseText != null) _phaseText.text = s ?? string.Empty;
        }

        private class Runner : MonoBehaviour
        {
            public void Begin(Action onReady) { onReady?.Invoke(); }
        }
    }
}
