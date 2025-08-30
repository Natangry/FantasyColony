using System;

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
    }
}

namespace FantasyColony.Defs
{
    public static class Visual2DDefKinds
    {
        public const string Root = "Visual2DDef";
    }
}

