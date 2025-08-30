using System;

namespace FantasyColony.Defs
{
    /// <summary>
    /// Building metadata consumed by placement, UI, and visuals. All logic remains in C#.
    /// </summary>
    [Serializable]
    public class BuildingDef : Def
    {
        public int width = 1;
        public int height = 1;
        public bool unique = false;
        public bool showInPalette = true;
        public string visualRef; // defName of a Visual2DDef
    }
}

namespace FantasyColony.Defs
{
    public static class BuildingDefKinds
    {
        public const string Root = "BuildingDef";
    }
}

