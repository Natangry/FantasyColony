using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// One-per-map station that offers the Builder job via slots.
/// </summary>
public class ConstructionBoard : Building
{
    [SerializeField, Min(0)] private int builderSlots = 1;
    private bool _showInspector;

    private void Start()
    {
        id = "construction_board";
        displayName = string.IsNullOrEmpty(displayName) ? "Construction Board" : displayName;
        uniquePerMap = true;
        size = Vector2Int.one;
    }

    private void OnMouseUpAsButton()
    {
        _showInspector = !_showInspector;
    }

    public override void OnPlaced(Vector2Int grid, float tile)
    {
        base.OnPlaced(grid, tile);
        JobService js;
#if UNITY_2023_1_OR_NEWER
        js = UnityEngine.Object.FindFirstObjectByType<JobService>();
#else
        js = FindObjectOfType<JobService>();
#endif
        if (js != null)
        {
            js.SetSlots(this, JobType.Builder, builderSlots);
        }

        // Visual stub: tint a quad so it's visible
        var mr = GetComponent<MeshRenderer>();
        if (mr == null)
        {
            var quad = GetComponent<MeshFilter>() == null ? GameObject.CreatePrimitive(PrimitiveType.Quad) : gameObject;
            if (quad != gameObject)
            {
                quad.transform.SetParent(transform, false);
                quad.transform.localPosition = Vector3.zero;
            }
            mr = GetComponentInChildren<MeshRenderer>();
        }
        if (mr != null)
        {
            if (mr.sharedMaterial == null) mr.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            mr.sharedMaterial.color = new Color(0.95f, 0.85f, 0.35f, 1f);
        }
    }

    private void OnGUI()
    {
        if (!_showInspector) return;

        JobService js;
#if UNITY_2023_1_OR_NEWER
        js = UnityEngine.Object.FindFirstObjectByType<JobService>();
#else
        js = FindObjectOfType<JobService>();
#endif
        if (js == null) return;

        Rect r = new Rect(20f, Screen.height * 0.6f, Mathf.Max(260f, Screen.width * 0.22f), Screen.height * 0.35f);
        GUILayout.BeginArea(r, displayName, GUI.skin.window);

        GUILayout.Label("Builder job", GUI.skin.label);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("-", GUILayout.Width(32f)))
        {
            builderSlots = Mathf.Max(0, builderSlots - 1);
            js.SetSlots(this, JobType.Builder, builderSlots);
        }
        GUILayout.Label($"Slots: {builderSlots}", GUILayout.Width(120f));
        if (GUILayout.Button("+", GUILayout.Width(32f)))
        {
            builderSlots = Mathf.Min(99, builderSlots + 1);
            js.SetSlots(this, JobType.Builder, builderSlots);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(6f);
        var assigned = js.AssignedFor(this, JobType.Builder);
        GUILayout.Label($"Assigned ({assigned.Count})");
        int show = Mathf.Min(assigned.Count, 5);
        for (int i = 0; i < show; i++)
        {
            GUILayout.Label($"- {assigned[i].name}");
        }
        if (assigned.Count > show)
        {
            GUILayout.Label($"...and {assigned.Count - show} more");
        }

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Close")) _showInspector = false;
        GUILayout.EndArea();
    }
}
