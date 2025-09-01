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
        // Secondary (dark) fill used for standard buttons - BRIGHTENED for better visibility on dark backgrounds
        // Old values kept in comments for reference
        // Previous: #3B3329 (0.231, 0.200, 0.161)
        public static Color32 SecondaryFill   = Hex("#5E5345"); // #5E5345
        // Secondary (dark) explicit states for visible hover/press
        // Hover: noticeably lighter than base; Pressed: slightly darker than new base
        public static Color32 SecondaryHover  = Hex("#726555"); // #726555
        public static Color32 SecondaryPressed= Hex("#4A4033"); // Slightly darker than new base
        public static Color32 Keyline         = new Color32(0x5A,0x4C,0x38, (byte)(0.60f * 255));
        public static Color32 TextPrimary     = Hex("#F1E9D2");
        public static Color32 TextSecondary   = Hex("#C9BDA2");
        public static Color32 Danger          = Hex("#B34844");
        public static Color32 DangerHover     = Hex("#C8625E");
        public static Color32 DangerPressed   = Hex("#953A37");

        // --- Textured UI assets ---
        // Resource paths (under Assets/Resources)
        public const string WoodTilePath = "ui/sprites/tile/wood_soft_tile";
        public const string DarkBorder9SPath = "ui/sprites/9slice/border_dark_9s";

        // Desired on-screen border thickness in device pixels for sliced borders
        // This single value controls both panels and buttons.
        public const float TargetBorderPx = 1f; // try 0.75f for subtler, 2f for thicker

        // Overlay tints for Button.state (applied to a transparent overlay Image)
        // Keep subtle so textures are not washed out
        public static readonly Color HoverOverlay   = new Color(1f, 1f, 1f, 0.06f);
        public static readonly Color PressedOverlay = new Color(0f, 0f, 0f, 0.10f);
        public static readonly Color DisabledOverlay= new Color(0f, 0f, 0f, 0.40f);

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
