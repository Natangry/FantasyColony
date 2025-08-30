using System.Collections.Generic;
using UnityEngine;

namespace FantasyColony.Defs
{
    /// <summary>
    /// In-memory store for all loaded defs. Single source of truth.
    /// </summary>
    public static class DefDatabase
    {
        // Primary stores keyed by defName for fast lookup
        private static readonly Dictionary<string, Visual2DDef> _visualsByName = new Dictionary<string, Visual2DDef>();
        private static readonly Dictionary<string, BuildingDef> _buildingsByName = new Dictionary<string, BuildingDef>();

        // Legacy-style lists (for simple iteration) - derived views
        private static readonly List<Visual2DDef> _visualsList = new List<Visual2DDef>();
        private static readonly List<BuildingDef> _buildingsList = new List<BuildingDef>();

        public static IReadOnlyDictionary<string, Visual2DDef> VisualsByName => _visualsByName;
        public static IReadOnlyDictionary<string, BuildingDef> BuildingsByName => _buildingsByName;

        public static IReadOnlyList<Visual2DDef> Visuals => _visualsList;
        public static IReadOnlyList<BuildingDef> Buildings => _buildingsList;

        public static int TotalCount => _visualsByName.Count + _buildingsByName.Count;

        /// <summary>
        /// Called from BuildBootstrap.Ensure() during startup.
        /// </summary>
        public static void LoadAll()
        {
            _visualsByName.Clear();
            _buildingsByName.Clear();
            _visualsList.Clear();
            _buildingsList.Clear();

            try
            {
                var visualsTmp = new List<Visual2DDef>();
                var buildingsTmp = new List<BuildingDef>();
                Xml.XmlDefLoader.LoadAll(visualsTmp, buildingsTmp);

                foreach (var v in visualsTmp)
                {
                    if (string.IsNullOrEmpty(v.defName)) continue;
                    _visualsByName[v.defName] = v;
                }
                foreach (var b in buildingsTmp)
                {
                    if (string.IsNullOrEmpty(b.defName)) continue;
                    _buildingsByName[b.defName] = b;
                }
                _visualsList.AddRange(_visualsByName.Values);
                _buildingsList.AddRange(_buildingsByName.Values);

                Debug.Log($"[Defs] Loaded {TotalCount} defs (visuals: {_visualsByName.Count}, buildings: {_buildingsByName.Count}).");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Defs] LoadAll failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }

    /// <summary>Small helpers to keep legacy call sites compiling during migration.</summary>
    public static class DefListExtensions
    {
        public static bool TryGetValue(this IReadOnlyList<Visual2DDef> list, string defName, out Visual2DDef def)
        {
            def = null;
            if (string.IsNullOrEmpty(defName)) return false;
            foreach (var v in list)
            {
                if (v != null && v.defName == defName) { def = v; return true; }
            }
            return false;
        }
        public static bool TryGetValue(this IReadOnlyList<BuildingDef> list, string defName, out BuildingDef def)
        {
            def = null;
            if (string.IsNullOrEmpty(defName)) return false;
            foreach (var b in list)
            {
                if (b != null && b.defName == defName) { def = b; return true; }
            }
            return false;
        }
    }
}
