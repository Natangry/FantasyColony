using UnityEngine;

namespace FantasyColony.UI.Style
{
    public static class BaseUIStyle
    {
        // Colors
        public static Color32 Gold            = Hex("#D6B25E");
        public static Color32 GoldHover       = Hex("#E4C77D");
        public static Color32 GoldPressed     = Hex("#B99443");
        public static Color32 PanelSurface    = new Color32(0x1F,0x1A,0x14, (byte)(0.95f * 255));
        // Secondary palette tuned for clearer hover/click contrast
        public static Color32 SecondaryFill   = Hex("#3B3329"); // Lightened from #2A231B
        public static Color32 SecondaryHover  = Hex("#4A4033"); // Noticeably lighter than base
        public static Color32 SecondaryPressed= Hex("#2A231B"); // Old base as pressed state
        public static Color32 Keyline         = new Color32(0x5A,0x4C,0x38, (byte)(0.60f * 255));
        public static Color32 TextPrimary     = Hex("#F1E9D2");
        public static Color32 TextSecondary   = Hex("#C9BDA2");
        public static Color32 Danger          = Hex("#B34844");
        public static Color32 DangerHover     = Hex("#C8625E");
        public static Color32 DangerPressed   = Hex("#953A37");

        // Sizes
        public const int ButtonHeight = 56;
        public const int ButtonFontSize = 24;
        public const int BodyFontSize = 20;
        public const int CaptionFontSize = 18;
        public const int PanelPadding = 24;
        public const int StackSpacing = 12;
        public const int EdgeOffset = 56;

        public static Color32 Hex(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out var c))
                return c;
            return Color.white;
        }
    }
}
