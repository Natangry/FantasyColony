using UnityEngine;

namespace FantasyColony.UI.Creator.Editing
{
    /// <summary>
    /// Global grid/snapping preferences for the Creator. Runtime-only.
    /// </summary>
    public static class GridPrefs
    {
        public static bool SnapEnabled = true;
        public static bool GridVisible = true;
        public static int CellSize = 16; // pixels

        public static int CycleCellSize()
        {
            CellSize = CellSize == 8 ? 16 : CellSize == 16 ? 32 : 8;
            Debug.Log($"[UICreator] Grid size = {CellSize}");
            return CellSize;
        }
    }
}

