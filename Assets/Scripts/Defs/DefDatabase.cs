using System.Collections.Generic;
using UnityEngine;

namespace FantasyColony.Defs
{
    /// <summary>
    /// In-memory store for all loaded defs. Single source of truth.
    /// </summary>
    public static class DefDatabase
    {
        private static readonly List<Visual2DDef> _visuals = new List<Visual2DDef>();
        private static readonly List<BuildingDef> _buildings = new List<BuildingDef>();

        public static IReadOnlyList<Visual2DDef> Visuals => _visuals;
        public static IReadOnlyList<BuildingDef> Buildings => _buildings;

        public static int TotalCount => _visuals.Count + _buildings.Count;

        /// <summary>
        /// Called from BuildBootstrap.Ensure() during startup.
        /// </summary>
        public static void LoadAll()
        {
            _visuals.Clear();
            _buildings.Clear();

            try
            {
                Xml.XmlDefLoader.LoadAll(_visuals, _buildings);
                Debug.Log($"[Defs] Loaded {TotalCount} defs (visuals: {_visuals.Count}, buildings: {_buildings.Count}).");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Defs] LoadAll failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}

