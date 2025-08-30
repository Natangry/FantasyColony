using System;
using System.Collections.Generic;

namespace FantasyColony.Defs
{
    /// <summary>
    /// 2D sprite visual definition. References art by path; registry will resolve at runtime.
    /// </summary>
    [Serializable]
    public class Visual2DDef : Def
    {
        public string spritePath;   // e.g., "Sprites/Stations/ConstructionBoard"
        public string sortingLayer = "Default";
        public int sortingOrder = 0;
        public float pivotX = 0.5f;
        public float pivotY = 0.0f;
        public float scale = 1.0f;

        // Plane & depth
        public string plane = "XY"; // XY or XZ
        public float z_lift = 0f;   // legacy-friendly name kept

        // Material & color (kept for legacy VisualFactory code)
        public string shader_hint;         // optional material/shader hint
        public string color_rgba = "1,1,1,1";

        // Optional variant sprites
        public List<Variant> variants;

        [Serializable]
        public class Variant
        {
            public string spritePath;
            public int weight = 1;
            public string conditionTag;
        }
    }
}

namespace FantasyColony.Defs
{
    public static class Visual2DDefKinds
    {
        public const string Root = "Visual2DDef";
    }
}

