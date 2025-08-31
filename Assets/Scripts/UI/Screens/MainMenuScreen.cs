using UnityEngine;
using UnityEngine.UI;
// using DevTools; // not required; DevLogOverlay is in the global namespace
using System;
using FantasyColony.UI.Router;
using FantasyColony.UI.Widgets;
using FantasyColony.UI.Style;
using FantasyColony.Core.Services;
using UnityObject = UnityEngine.Object;
using FantasyColony.Boot;

namespace FantasyColony.UI.Screens
{
    /// <summary>
    /// Main Menu (non-functional for now). Uses Base UI Style, bottom-right vertical stack.
    /// Order: Log, Start, Continue, Load, Options, Mods, Creator, Restart, Quit
    /// </summary>
    public sealed class MainMenuScreen : UIScreenBase
    {
        public override void Enter(Transform parent)
        {
            // Root
            var go = new GameObject("MainMenu", typeof(RectTransform));
            Root = go.GetComponent<RectTransform>();
            Root.SetParent(parent, false);
            Root.anchorMin = Vector2.zero;
            Root.anchorMax = Vector2.one;
            Root.offsetMin = Vector2.zero;
            Root.offsetMax = Vector2.zero;

            // Background (image if present, else solid)
            var bgSprite = Resources.Load<Sprite>("ui/menu/main_menu_bg");
            UIFactory.CreateFullscreenBackground(Root, bgSprite, new Color32(18, 15, 12, 255));

            // Panel stack (bottom-right)
            var panel = UIFactory.CreateBottomRightStack(Root, "MenuPanel");
            panel.SetAsLastSibling();

            // Buttons (log "Not implemented")
            void NotImpl(string name) => Debug.Log($"BUTTON: {name} (not implemented)");

            UIFactory.CreateButtonSecondary(panel, "Log",        DevLogOverlay.Show);
            UIFactory.CreateButtonPrimary(panel,   "Start",      () => NotImpl("Start"));
            var btnContinue = UIFactory.CreateButtonSecondary(panel, "Continue", () => NotImpl("Continue"));
            var btnLoad     = UIFactory.CreateButtonSecondary(panel, "Load",     () => NotImpl("Load"));
            UIFactory.CreateButtonSecondary(panel, "Options",    () => NotImpl("Options"));
            UIFactory.CreateButtonSecondary(panel, "Mods",       () => NotImpl("Mods"));
            UIFactory.CreateButtonSecondary(panel, "Creator",    () => NotImpl("Creator"));
            UIFactory.CreateButtonSecondary(panel, "Restart",    ShowRestartConfirm);
            UIFactory.CreateButtonDanger(panel,     "Quit",      ShowQuitConfirm);

            // Disabled rules for now (no save system yet)
            btnContinue.interactable = false;
            btnLoad.interactable = false;
        }

        public override void Exit()
        {
            if (Root != null)
            {
                UnityObject.Destroy(Root.gameObject);
                Root = null;
            }
        }

        private void ShowRestartConfirm()
        {
            UIRouter.Current?.Push<ConfirmDialogScreen>(d =>
            {
                d.Title = "Restart Game?";
                d.Message = "This will restart the application flow.";
                d.ConfirmLabel = "Restart";
                d.CancelLabel = "Cancel";
                d.OnConfirm = () =>
                {
                    UIRouter.Current?.Push<BootScreen>(b => { b.Title = "Loading"; });
                };
            });
        }

        private void ShowQuitConfirm()
        {
            UIRouter.Current?.Push<ConfirmDialogScreen>(d =>
            {
                d.Title = "Quit Game?";
                d.Message = "Are you sure you want to exit?";
                d.ConfirmLabel = "Quit";
                d.CancelLabel = "Cancel";
                d.OnConfirm = QuitGame;
            });
        }

        private void RestartGame()
        {
            // (Old direct restart path retained for reference; unused now)
            Time.timeScale = 1f;
            UIRouter.Current?.ResetTo<MainMenuScreen>();
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }

        private void QuitGame()
        {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
        #else
            Application.Quit();
        #endif
        }
    }
}
