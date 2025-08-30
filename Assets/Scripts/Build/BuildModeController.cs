using UnityEngine;
using System;
using System.Linq;
// Assumes BuildingDef exists in your Defs namespace
// If your project uses a different namespace, adjust the using accordingly.
using FantasyColony.Defs;
using UnityObject = UnityEngine.Object;

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
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    public bool IsBuildModeEnabled => _buildModeEnabled;
    // Back-compat for existing HUD scripts
    public bool IsActive => _buildModeEnabled;
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
#if UNITY_2023_1_OR_NEWER
        return UnityObject.FindAnyObjectByType<T>() != null;
#elif UNITY_2022_2_OR_NEWER
        return UnityObject.FindFirstObjectByType<T>() != null;
#else
#pragma warning disable 618
        return UnityObject.FindObjectOfType<T>() != null;
#pragma warning restore 618
#endif
    }

    private void Update()
    {
        // Self-heal: if Instance was lost (scene change), ensure systems exist
        if (Instance == null)
        {
            BuildBootstrap.Ensure();
            Instance = this;
        }

        // Basic hotkey (B) to toggle build mode
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleBuildMode();
            Debug.Log("[Build] Toggled via B → " + (_buildModeEnabled ? "ON" : "OFF"));
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
