using UnityEngine;
using System;
using System.Linq;
// Assumes BuildingDef exists in your Defs namespace
// If your project uses a different namespace, adjust the using accordingly.
using FantasyColony.Defs;

public class BuildModeController : MonoBehaviour
{
    public static BuildModeController Instance { get; private set; }

    [Header("State")]
    [SerializeField] private bool _buildModeEnabled = false;
    [SerializeField] private BuildTool _activeTool = BuildTool.None;

    // Selected def for placement-oriented tools (e.g., PlaceConstructionBoard)
    [SerializeField] private BuildingDef _selectedBuildingDef;

    private void Awake()
    {
        Instance = this;
    }

    public bool IsBuildModeEnabled => _buildModeEnabled;
    public BuildTool ActiveTool => _activeTool;
    public BuildingDef SelectedBuildingDef => _selectedBuildingDef;

    public void ToggleBuildMode()
    {
        SetBuildMode(!_buildModeEnabled);
    }

    public void SetBuildMode(bool on)
    {
        _buildModeEnabled = on;
        if (!on) ClearTool();
    }

    public void SetPlacingBuilding(BuildingDef def)
    {
        _selectedBuildingDef = def;
        SetTool(BuildTool.PlaceConstructionBoard);
    }

    public void ClearTool()
    {
        _activeTool = BuildTool.None;
        var tool = GetComponent<BuildPlacementTool>();
        if (tool != null) tool.SetTool(BuildTool.None);
    }

    public void SetTool(BuildTool tool)
    {
        _activeTool = tool;
        var toolComp = GetComponent<BuildPlacementTool>();
        if (toolComp != null) toolComp.SetTool(tool);
    }

    // Utility used by placement tool to enforce uniqueness of special buildings
    public bool UniqueBuildingExists<T>() where T : Component
    {
        return FindObjectsOfType<T>().Length > 0;
    }

    private void Update()
    {
        // Basic hotkey (B) to toggle build mode
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleBuildMode();
        }
        // ESC cancels current tool
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClearTool();
        }
    }
}

public enum BuildTool
{
    None,
    PlaceConstructionBoard
}
