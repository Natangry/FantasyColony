using UnityEngine;

public enum BuildTool
{
    None = 0,
    PlaceConstructionBoard = 1,
}

public class BuildModeController : MonoBehaviour
{
    public static BuildModeController Instance { get; private set; }

    [SerializeField] private bool isActive;
    [SerializeField] private BuildTool currentTool = BuildTool.None;

    public bool IsActive => isActive;
    public BuildTool CurrentTool => currentTool;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleBuildMode();
        }

        // ESC cancels tool or exits build mode
        if (isActive && Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentTool != BuildTool.None)
                SetTool(BuildTool.None);
            else
                SetActive(false);
        }
    }

    public void ToggleBuildMode() => SetActive(!isActive);

    public void SetActive(bool active)
    {
        isActive = active;
        if (!isActive) SetTool(BuildTool.None);
    }

    public void SetTool(BuildTool tool)
    {
        currentTool = tool;
        var toolComp = GetComponent<BuildPlacementTool>();
        if (toolComp == null) toolComp = gameObject.AddComponent<BuildPlacementTool>();
        toolComp.SetTool(tool);
    }

    public static bool UniqueBuildingExists<T>() where T : Building
    {
        // "Any" is acceptable and faster in 2023+; fallback for older Unity.
#if UNITY_2023_1_OR_NEWER
        return Object.FindAnyObjectByType<T>() != null;
#else
        return Object.FindObjectOfType<T>() != null;
#endif
    }
}
