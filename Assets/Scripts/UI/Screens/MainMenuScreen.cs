using UnityEngine;
using UnityEngine.UI;
using FantasyColony.UI.Router;
using FantasyColony.UI.Widgets;
using FantasyColony.UI.Style;
using FantasyColony.Core.Services;

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
            // Make panel background transparent (no shadow panel), keep layout behavior
            var panelImg = panel.GetComponent<Image>();
            if (panelImg) panelImg.color = new Color(0,0,0,0);
            panel.SetAsLastSibling();

            // Buttons (log "Not implemented")
            void NotImpl(string name) => Debug.Log($"BUTTON: {name} (not implemented)");

            UIFactory.CreateButtonSecondary(panel, "Log",        () => NotImpl("Log"));
            UIFactory.CreateButtonPrimary(panel,   "Start",      () => NotImpl("Start"));
            var btnContinue = UIFactory.CreateButtonSecondary(panel, "Continue", () => NotImpl("Continue"));
            var btnLoad     = UIFactory.CreateButtonSecondary(panel, "Load",     () => NotImpl("Load"));
            UIFactory.CreateButtonSecondary(panel, "Options",    () => NotImpl("Options"));
            UIFactory.CreateButtonSecondary(panel, "Mods",       () => NotImpl("Mods"));
            UIFactory.CreateButtonSecondary(panel, "Creator",    () => NotImpl("Creator"));
            UIFactory.CreateButtonSecondary(panel, "Restart",    () => NotImpl("Restart"));
            UIFactory.CreateButtonDanger(panel,     "Quit",      () => NotImpl("Quit"));

            // Disabled rules for now (no save system yet)
            btnContinue.interactable = false;
            btnLoad.interactable = false;
        }

        public override void Exit()
        {
            if (Root != null)
            {
                Object.Destroy(Root.gameObject);
                Root = null;
            }
        }
    }
}
