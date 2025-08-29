using UnityEngine;

/// <summary>
/// Base component for placeable buildings.
/// </summary>
public class Building : MonoBehaviour
{
    public string id = "building";
    public string displayName = "Building";
    public bool uniquePerMap = false;
    public Vector2Int size = Vector2Int.one; // tiles wide/high

    [Header("Runtime")]
    [SerializeField] protected Vector2Int gridPos; // bottom-left tile of footprint
    [SerializeField] protected float tileSize = 1f;

    public Vector2Int GridPos => gridPos;

    public virtual void OnPlaced(Vector2Int grid, float tile)
    {
        gridPos = grid;
        tileSize = tile;

        // Add a collider for clicks if missing
        var col = GetComponent<BoxCollider2D>();
        if (col == null) col = gameObject.AddComponent<BoxCollider2D>();
        col.size = new Vector2(size.x * tileSize, size.y * tileSize);
        col.offset = Vector2.zero;
    }

    public virtual void OnRemoved() { }

    public bool Occupies(Vector2Int tile)
    {
        return tile.x >= gridPos.x && tile.x < gridPos.x + size.x && tile.y >= gridPos.y && tile.y < gridPos.y + size.y;
    }
}
