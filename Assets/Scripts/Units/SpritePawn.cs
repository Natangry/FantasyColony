using UnityEngine;

/// <summary>
/// A simple SNES-style sprite pawn that patrols a rectangle within the camera view.
/// Generates its own low-res texture and keeps pixels crisp via point filtering + pixel snapping.
/// </summary>
[AddComponentMenu("Units/Sprite Pawn (Test)")]
public class SpritePawn : MonoBehaviour
{
    [Header("Sprite")]
    [SerializeField] private int spriteWidthPx = 16;
    [SerializeField] private int spriteHeightPx = 24;
    [SerializeField] private int pixelsPerUnit = 16;

    [Header("Palette")]
    [SerializeField] private Color body = new Color(0.82f, 0.80f, 0.65f, 1f);
    [SerializeField] private Color shade = new Color(0.62f, 0.60f, 0.48f, 1f);
    [SerializeField] private Color accent = new Color(0.35f, 0.42f, 0.65f, 1f);
    [SerializeField] private Color outline = new Color(0.10f, 0.10f, 0.10f, 1f);

    [Header("Movement")]
    [SerializeField] private float speed = 3.0f;
    [SerializeField] private float margin = 1.25f;

    // Patrol corners (world-space)
    private Vector3[] corners = new Vector3[4];
    private int cornerIndex = 0;
    private Camera cam;

    // Visuals
    private GameObject quadGO;
    private Material mat;

    private void Awake()
    {
        cam = Camera.main;
        if (cam == null)
        {
            // Minimal fallback camera if none exists.
            var go = new GameObject("Main Camera");
            cam = go.AddComponent<Camera>();
            go.tag = "MainCamera";
            cam.orthographic = true;
            cam.orthographicSize = 10f;
            cam.transform.position = new Vector3(0f, 10f, 0f);
            cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }

        CreateVisual();
        BuildPatrolFromCamera();
        // Start near the first corner so movement is immediately visible
        transform.position = corners[0];
    }

    void CreateVisual()
    {
        // Create a tiny texture with a simple FF6-like silhouette.
        var tex = new Texture2D(spriteWidthPx, spriteHeightPx, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;

        // Fill transparent
        var pixels = new Color32[spriteWidthPx * spriteHeightPx];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(0,0,0,0);

        // Draw a rounded capsule-ish body with outline and a bit of accent
        // Body bounds
        int x0 = 2, x1 = spriteWidthPx - 3;
        int y0 = 3, y1 = spriteHeightPx - 3;

        for (int y = y0; y <= y1; y++)
        {
            for (int x = x0; x <= x1; x++)
            {
                // Rounded mask
                float nx = Mathf.InverseLerp(x0, x1, x) * 2f - 1f;
                float ny = Mathf.InverseLerp(y0, y1, y) * 2f - 1f;
                float r = Mathf.Sqrt(nx * nx * 0.85f + ny * ny);
                if (r <= 1.0f)
                {
                    // Simple shading: darker toward bottom-right
                    float shadeAmt = Mathf.Clamp01(0.35f + 0.35f * (Mathf.InverseLerp(y0, y1, y)));
                    Color c = Color.Lerp(body, shade, shadeAmt * 0.6f);
                    pixels[y * spriteWidthPx + x] = c;
                }
            }
        }

        // Add an accent "cloak" band
        int bandY = Mathf.RoundToInt(Mathf.Lerp(y0, y1, 0.55f));
        for (int x = x0; x <= x1; x++)
        {
            for (int yy = bandY - 2; yy <= bandY + 1; yy++)
            {
                var idx = yy * spriteWidthPx + x;
                if (idx >= 0 && idx < pixels.Length && pixels[idx].a > 0f)
                    pixels[idx] = Color.Lerp(pixels[idx], accent, 0.7f);
            }
        }

        // Outline
        for (int y = y0 - 1; y <= y1 + 1; y++)
        {
            for (int x = x0 - 1; x <= x1 + 1; x++)
            {
                bool isBody = InsideBody(x, y, x0, x1, y0, y1);
                if (!isBody)
                {
                    // If any neighbor is body, draw outline
                    bool neighborBody =
                        InsideBody(x+1,y,x0,x1,y0,y1) || InsideBody(x-1,y,x0,x1,y0,y1) ||
                        InsideBody(x,y+1,x0,x1,y0,y1) || InsideBody(x,y-1,x0,x1,y0,y1);
                    if (neighborBody)
                    {
                        if (x>=0 && x<spriteWidthPx && y>=0 && y<spriteHeightPx)
                            pixels[y * spriteWidthPx + x] = outline;
                    }
                }
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply(false, false);

        // Create material
        Shader shader = Shader.Find(
#if UNITY_2021_2_OR_NEWER
            "Universal Render Pipeline/Unlit"
#else
            "Unlit/Transparent"
#endif
        );
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Texture");
        }
        mat = new Material(shader);
        if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
        if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", Color.white);

        // Create quad and orient it flat on the ground (XZ plane) so top-down camera sees it.
        quadGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quadGO.name = "SpriteQuad";
        quadGO.transform.SetParent(transform, false);
        // Lay flat: rotate -90° around X so the quad faces up (normal = +Y).
        quadGO.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

        // Assign material and remove collider
        var renderer = quadGO.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = mat;
        var col = quadGO.GetComponent<Collider>();
        if (col) Object.Destroy(col);

        // Scale quad to match pixel size / pixels-per-unit
        float worldW = (float)spriteWidthPx / Mathf.Max(1, pixelsPerUnit);
        float worldH = (float)spriteHeightPx / Mathf.Max(1, pixelsPerUnit);
        quadGO.transform.localScale = new Vector3(worldW, worldH, 1f);
    }

    bool InsideBody(int x, int y, int x0, int x1, int y0, int y1)
    {
        if (x < x0 || x > x1 || y < y0 || y > y1) return false;
        float nx = Mathf.InverseLerp(x0, x1, x) * 2f - 1f;
        float ny = Mathf.InverseLerp(y0, y1, y) * 2f - 1f;
        float r = Mathf.Sqrt(nx * nx * 0.85f + ny * ny);
        return r <= 1.0f;
    }

    void BuildPatrolFromCamera()
    {
        var (minX, maxX, minZ, maxZ) = PixelCameraHelper.OrthoWorldBounds(cam);
        minX += margin; maxX -= margin; minZ += margin; maxZ -= margin;
        // Clamp in case margin exceeds bounds
        if (minX > maxX) { float c = (minX + maxX) * 0.5f; minX = maxX = c; }
        if (minZ > maxZ) { float c = (minZ + maxZ) * 0.5f; minZ = maxZ = c; }

        corners[0] = new Vector3(minX, 0f, minZ);
        corners[1] = new Vector3(maxX, 0f, minZ);
        corners[2] = new Vector3(maxX, 0f, maxZ);
        corners[3] = new Vector3(minX, 0f, maxZ);
        cornerIndex = 1; // head toward the second corner first
    }

    private void Update()
    {
        if (corners == null || corners.Length < 4) return;
        Vector3 pos = transform.position;
        Vector3 target = corners[cornerIndex];
        Vector3 to = target - pos;
        float dist = to.magnitude;
        if (dist < 0.01f)
        {
            cornerIndex = (cornerIndex + 1) % 4;
            target = corners[cornerIndex];
            to = target - pos;
        }
        Vector3 dir = (to.sqrMagnitude > 1e-8f) ? (to / to.magnitude) : Vector3.zero;
        pos += dir * speed * Time.deltaTime;

        // Pixel snap after moving.
        pos = PixelCameraHelper.SnapToPixelGrid(pos, cam);
        transform.position = pos;
    }
}

