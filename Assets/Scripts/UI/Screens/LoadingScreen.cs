using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using FantasyColony.UI.Router;

namespace FantasyColony.UI.Screens
{
    // Minimal loading cover; reboots the app flow on the next frame (after a short delay)
    public class LoadingScreen : IScreen
    {
        public string Title = "Restarting…";
        public GameObject Root { get; private set; }

        public void Enter(Transform parent)
        {
            Root = new GameObject("LoadingScreen");
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
            tr.anchoredPosition = Vector2.zero;
            var text = titleGO.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 24; text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = string.IsNullOrEmpty(Title) ? "Loading…" : Title;

            // Kick off the restart on the next frame (with a tiny delay to ensure display)
            var runner = Root.AddComponent<Runner>();
            runner.Begin(() => {
                Time.timeScale = 1f;
                UIRouter.Current?.ResetTo<MainMenuScreen>();
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

        private class Runner : MonoBehaviour
        {
            public void Begin(Action onReady) { StartCoroutine(BeginCo(onReady)); }
            private IEnumerator BeginCo(Action onReady)
            {
                yield return null; // render once
                yield return new WaitForSeconds(0.15f); // anti-flicker; replace with real phases later
                onReady?.Invoke();
            }
        }
    }
}
