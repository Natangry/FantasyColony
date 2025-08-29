public enum GridPlane
{
    XY,
    XZ
}

/// <summary>
/// Global grid-space hint so placement/visuals/colliders can agree on the plane.
/// </summary>
public static class GridSpace
{
    public static GridPlane Plane = GridPlane.XY; // default to classic 2D
}


