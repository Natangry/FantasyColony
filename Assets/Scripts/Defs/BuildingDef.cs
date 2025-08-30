using System;
using System.Collections.Generic;

namespace FantasyColony.Defs
{
    /// <summary>
    /// Building metadata consumed by placement, UI, and visuals. All logic remains in C#.
    /// </summary>
    [Serializable]
    public class BuildingDef : Def
    {
        // Identity & UI
        public string label;
        public string description;
        public string category;
        public bool showInPalette = true;
        public bool unique = false;

        // Footprint & orientation
        public int width = 1;
        public int height = 1;
        /// <summary>Allowed headings, e.g., ["N","E","S","W"]. If null/empty, assume all four.</summary>
        public List<string> allowedRotations;
        public string defaultRotation = "N";

        // Placement rules
        /// <summary>Preferred plane for placement/preview; falls back to visual's plane if empty.</summary>
        public string plane;

        // Pathing
        /// <summary>If true, navgrid can pass through this footprint.</summary>
        public bool canPathThrough = false;
        /// <summary>Soft avoidance radius (in tiles) to discourage pathing too close unless interacting.</summary>
        public float avoidanceRadius = 0f;

        // Interactions
        public List<string> interactTags;

        // Cost & work (v1: work only)
        public float workToBuild = 0f;

        // Visual link
        public string visualRef; // defName of a Visual2DDef

        // --- Compatibility aliases (temporary during migration) ---
        // Old snake_case alias; maps to visualRef.
        public string visual_ref
        {
            get => visualRef;
            set => visualRef = value;
        }
    }
}

namespace FantasyColony.Defs
{
    public static class BuildingDefKinds
    {
        public const string Root = "BuildingDef";
    }
}

