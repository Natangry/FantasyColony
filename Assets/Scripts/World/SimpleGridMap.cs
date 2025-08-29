using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Procedurally builds a flat checkerboard grid as a single mesh with two submeshes (A/B colors).
/// Keeps dependencies minimal and works without any art assets.
/// </summary>
[AddComponentMenu("World/Simple Grid Map")]
public class SimpleGridMap : MonoBehaviour
{
    [Header("Size")]
    [Min(1)] public int width = 32;
    [Min(1)] public int height = 32;
    [Min(0.1f)] public float tileSize = 1f;

    [Header("Palette")]
    public Color colorA = new Color(0.42f, 0.48f, 0.33f); // muted grass
    public Color colorB = new Color(0.36f, 0.42f, 0.28f); // darker tile

    MeshFilter _filter;
    MeshRenderer _renderer;

    /// <summary>Rebuilds using serialized dimensions.</summary>
    public void Build()
    {
        Build(width, height, tileSize, colorA, colorB);
    }

    /// <summary>Build grid with explicit parameters.</summary>
    public void Build(int w, int h, float size, Color a, Color b)
    {
        width = Mathf.Max(1, w);
        height = Mathf.Max(1, h);
        tileSize = Mathf.Max(0.1f, size);
        colorA = a;
        colorB = b;

        EnsureComponents();
        var mesh = GenerateMesh(width, height, tileSize);
        _filter.sharedMesh = mesh;

        // Two materials for the two submeshes
        var matA = CreateMaterial();
        var matB = CreateMaterial();
        SetMaterialColor(matA, colorA);
        SetMaterialColor(matB, colorB);
        _renderer.sharedMaterials = new[] { matA, matB };
    }

    void EnsureComponents()
    {
        if (_filter == null)
            _filter = gameObject.GetComponent<MeshFilter>() ?? gameObject.AddComponent<MeshFilter>();
        if (_renderer == null)
            _renderer = gameObject.GetComponent<MeshRenderer>() ?? gameObject.AddComponent<MeshRenderer>();
    }

    Mesh GenerateMesh(int w, int h, float t)
    {
        int tiles = w * h;
        int vCount = tiles * 4;

        var verts = new Vector3[vCount];
        var uvs = new Vector2[vCount];
        var norms = new Vector3[vCount];

        // Two submeshes for checkerboard coloring
        var trisA = new List<int>(tiles * 6 / 2 + 6);
        var trisB = new List<int>(tiles * 6 / 2 + 6);

        int vi = 0;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float x0 = x * t;
                float x1 = (x + 1) * t;
                float z0 = y * t;
                float z1 = (y + 1) * t;

                //  v2---v3
                //  |  / |
                //  v0---v1
                int v0 = vi + 0;
                int v1 = vi + 1;
                int v2 = vi + 2;
                int v3 = vi + 3;

                verts[v0] = new Vector3(x0, 0f, z0);
                verts[v1] = new Vector3(x1, 0f, z0);
                verts[v2] = new Vector3(x0, 0f, z1);
                verts[v3] = new Vector3(x1, 0f, z1);

                uvs[v0] = new Vector2(0f, 0f);
                uvs[v1] = new Vector2(1f, 0f);
                uvs[v2] = new Vector2(0f, 1f);
                uvs[v3] = new Vector2(1f, 1f);

                norms[v0] = Vector3.up;
                norms[v1] = Vector3.up;
                norms[v2] = Vector3.up;
                norms[v3] = Vector3.up;

                // Triangles (front face up)
                // v0, v1, v2 and v2, v1, v3 (clockwise when viewed from above)
                var which = ((x + y) & 1) == 0 ? trisA : trisB;
                which.Add(v0); which.Add(v1); which.Add(v2);
                which.Add(v2); which.Add(v1); which.Add(v3);

                vi += 4;
            }
        }

        var mesh = new Mesh();
#if UNITY_2017_3_OR_NEWER
        if (vCount > 65000) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
#endif
        mesh.name = "SimpleGridMap";
        mesh.vertices = verts;
        mesh.uv = uvs;
        mesh.normals = norms;
        mesh.subMeshCount = 2;
        mesh.SetTriangles(trisA, 0, true);
        mesh.SetTriangles(trisB, 1, true);
        mesh.RecalculateBounds();
        return mesh;
    }

    static Material CreateMaterial()
    {
        Shader shader = FindFirstShader(
#if UNITY_2021_2_OR_NEWER
            "Universal Render Pipeline/Unlit",
#endif
            "Unlit/Color",
            "HDRP/Unlit",
            "Sprites/Default",
            "Standard"
        );
        var mat = new Material(shader);
        // Make it a bit less shiny if Standard is used
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0f);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0f);
        return mat;
    }

    static Shader FindFirstShader(params string[] names)
    {
        foreach (var n in names)
        {
            var s = Shader.Find(n);
            if (s != null) return s;
        }
        // Fallback to any available shader
        return Shader.Find("Standard") ?? Shader.Find("Sprites/Default");
    }

    static void SetMaterialColor(Material m, Color c)
    {
        // Try common color property names across pipelines
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Color")) m.SetColor("_Color", c);
        if (m.HasProperty("_TintColor")) m.SetColor("_TintColor", c);
    }
}

