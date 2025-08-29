using UnityEngine;

/// <summary>
/// Base component for placeable buildings.
/// </summary>
public class Building : MonoBehaviour
{
    public string id = "building";
    public string displayName = "Building";
    public bool uniquePerMap = false;
    public Vector2Int size = Vector2Int.one; // tiles wide (X) / deep (Z)

    [Header("Runtime")]
    [SerializeField] protected Vector2Int gridPos; // bottom-left tile of footprint
    [SerializeField] protected float tileSize = 1f;

    public Vector2Int GridPos => gridPos;

    public virtual void OnPlaced(Vector2Int grid, float tile)
    {
        gridPos = grid;
        tileSize = tile;

        // Add/adjust a 3D collider that sits on the XZ plane
        var col3 = GetComponent<BoxCollider>();
        if (col3 == null) col3 = gameObject.AddComponent<BoxCollider>();
        float w = size.x * tileSize;
        float d = size.y * tileSize;
        col3.size = new Vector3(w, 0.1f, d);
        col3.center = new Vector3(w * 0.5f, 0.05f, d * 0.5f);
    }

    public virtual void OnRemoved() { }

    public bool Occupies(Vector2Int tile)
    {
        return tile.x >= gridPos.x && tile.x < gridPos.x + size.x && tile.y >= gridPos.y && tile.y < gridPos.y + size.y;
    }
}
