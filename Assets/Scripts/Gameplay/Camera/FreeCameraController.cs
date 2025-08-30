using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
// ReSharper disable Unity.InefficientPropertyAccess
// ReSharper disable Unity.PerformanceCriticalCodeInvocation

/// <summary>
/// Free camera: mouse-wheel zoom (always) + WASD/Arrow panning when no pawn is controlled.
/// Uses unscaled time so you can pan while paused. Top-down, XZ only.
/// </summary>
[AddComponentMenu("Camera/Free Camera Controller")]
public class FreeCameraController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float boostMultiplier = 2f;
    [SerializeField] private bool clampToGrid = true;
    [SerializeField] private float clampMargin = 1f;
    [Header("Visibility Rule")]
    [SerializeField, Tooltip("Keep at least this many on-screen pixels of the grid visible on each axis. Set to 1 for 'some part of grid must remain visible'.")] private float minVisiblePixels = 1f;
    
    [Header("Zoom")]
    [SerializeField] private float minOrtho = 3f;
    [SerializeField] private float maxOrtho = 200f;
    [SerializeField] private float zoomSpeed = 8f;          // how fast orthographicSize changes per wheel notch
    [SerializeField] private bool smoothZoom = true;
    [SerializeField] private float zoomSmoothTime = 0.08f;  // unscaled seconds

    private Camera _cam;
    private SimpleGridMap _grid;
    private float _targetOrtho;
    private float _zoomVel;
    private float _skipClampUntil; // unscaled time

#if ENABLE_INPUT_SYSTEM
    private InputAction _move;
    private InputAction _boost;
    private InputAction _scroll;
#endif

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        if (_cam == null) _cam = Camera.main;
        if (_cam != null)
            _targetOrtho = Mathf.Max(0.01f, _cam.orthographicSize);
    }

    private void TryFindGrid()
    {
        if (_grid != null) return;
#if UNITY_2022_2_OR_NEWER
        _grid = UnityEngine.Object.FindAnyObjectByType<SimpleGridMap>();
#else
        _grid = UnityEngine.Object.FindObjectOfType<SimpleGridMap>();
#endif
    }

    private void OnEnable()
    {
        ControlManager.OnControlledChanged += OnControlledChanged;
        if (_cam != null)
            _targetOrtho = Mathf.Max(0.01f, _cam.orthographicSize);

#if ENABLE_INPUT_SYSTEM
        if (_move == null)
        {
            // 2D composite: WASD + Arrows + gamepad stick
            _move = new InputAction("CamMove", type: InputActionType.Value, binding: "2DVector");
            _move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/s").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/a").With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/d").With("Right", "<Keyboard>/rightArrow");
            _move.AddBinding("<Gamepad>/leftStick");
        }
        if (_boost == null) _boost = new InputAction("CamBoost", binding: "<Keyboard>/shift");
        if (_scroll == null) _scroll = new InputAction("CamScroll", binding: "<Mouse>/scroll"); // Vector2 (x,y)
        _move.Enable(); _boost.Enable(); _scroll.Enable();
#endif
    }

    private void OnDisable()
    {
        ControlManager.OnControlledChanged -= OnControlledChanged;
#if ENABLE_INPUT_SYSTEM
        _move?.Disable();
        _boost?.Disable();
        _scroll?.Disable();
#endif
    }

    private void Update()
    {
        if (_cam == null) return;

        // --- Zoom (always available, even while controlling a pawn) ---
        float scrollDelta = ReadScrollDelta(); // >0 means wheel up

        // --- Zoom (center-based) ---
        if (Mathf.Abs(scrollDelta) > 0.0001f)
        {
            // Wheel up (positive) -> zoom in -> smaller ortho size
            _targetOrtho = Mathf.Clamp(_targetOrtho - scrollDelta * zoomSpeed, minOrtho, maxOrtho);
        }
        if (smoothZoom)
        {
            _cam.orthographicSize = Mathf.SmoothDamp(_cam.orthographicSize, _targetOrtho, ref _zoomVel, Mathf.Max(0.0001f, zoomSmoothTime), Mathf.Infinity, Time.unscaledDeltaTime);
        }
        else
        {
            _cam.orthographicSize = _targetOrtho;
        }

        // If a pawn is controlled, skip free panning (follow script will move camera). Zoom above already applied.
        if (ControlManager.Controlled != null) return;

        // --- Read move input (free cam only) ---
        Vector2 input = ReadMoveInput();
        if (input.sqrMagnitude > 1f) input.Normalize();
        bool boost = ReadBoost();

        // --- Pan (WASD/Arrows) ---
        float speed = moveSpeed * (boost ? boostMultiplier : 1f);
        Vector3 delta = new Vector3(input.x, 0f, input.y) * speed * Time.unscaledDeltaTime;
        Vector3 pos = _cam.transform.position + delta;

        if (clampToGrid)
        {
            TryFindGrid();
            if (_grid != null && Time.unscaledTime >= _skipClampUntil)
            {
                EnsureGridVisibility(ref pos, _cam, _grid, clampMargin, Mathf.Max(0f, minVisiblePixels));
            }
        }

        _cam.transform.position = pos;
    }

    private void OnControlledChanged(SpritePawn pawn)
    {
        // When control is released (pawn == null), give a short grace window so clamp doesn't snap us.
        if (pawn == null)
            _skipClampUntil = Time.unscaledTime + 0.25f;
    }

    /// <summary>
    /// Ensures that at least 'minVisiblePixels' of the grid remains visible on each axis.
    /// This allows near-infinite panning as long as a sliver of grid is still in view.
    /// </summary>
    private static void EnsureGridVisibility(ref Vector3 camPos, Camera cam, SimpleGridMap grid, float margin, float minVisiblePixels)
    {
        float wWorld = grid.width * grid.tileSize;
        float hWorld = grid.height * grid.tileSize;

        // Subtract half view so camera doesn't show outside the grid.
        float halfH = cam.orthographicSize;
        float aspect = (Screen.height > 0) ? (Screen.width / (float)Screen.height) : (16f / 9f);
        float halfW = halfH * aspect;

        // Grid rect (expanded by margin so we don't put the edge exactly on the viewport border)
        float gMinX = 0f + margin;
        float gMaxX = wWorld - margin;
        float gMinZ = 0f + margin;
        float gMaxZ = hWorld - margin;

        // Camera rect from its center position
        float cMinX = camPos.x - halfW;
        float cMaxX = camPos.x + halfW;
        float cMinZ = camPos.z - halfH;
        float cMaxZ = camPos.z + halfH;

        // Required overlap in world units based on pixels (cap to grid size to avoid impossible requirements)
        float upp = Mathf.Max(1e-6f, PixelCameraHelper.WorldUnitsPerPixel(cam));
        float reqOverlapX = Mathf.Min(minVisiblePixels * upp, Mathf.Max(0f, gMaxX - gMinX));
        float reqOverlapZ = Mathf.Min(minVisiblePixels * upp, Mathf.Max(0f, gMaxZ - gMinZ));

        // --- X axis ---
        {
            float overlapX = Mathf.Min(cMaxX, gMaxX) - Mathf.Max(cMinX, gMinX);
            if (overlapX < reqOverlapX)
            {
                // Compute minimal shift to achieve the required overlap.
                float gridCenterX = 0.5f * (gMinX + gMaxX);
                if (cMaxX <= gMinX) // camera fully left of grid
                {
                    float desiredCamMax = gMinX + reqOverlapX;
                    camPos.x = desiredCamMax - halfW;
                }
                else if (cMinX >= gMaxX) // camera fully right of grid
                {
                    float desiredCamMin = gMaxX - reqOverlapX;
                    camPos.x = desiredCamMin + halfW;
                }
                else // partial overlap: nudge toward grid center
                {
                    float dir = (camPos.x < gridCenterX) ? +1f : -1f;
                    camPos.x += dir * (reqOverlapX - Mathf.Max(0f, overlapX));
                }
            }
        }
        // --- Z axis ---
        {
            float overlapZ = Mathf.Min(cMaxZ, gMaxZ) - Mathf.Max(cMinZ, gMinZ);
            if (overlapZ < reqOverlapZ)
            {
                float gridCenterZ = 0.5f * (gMinZ + gMaxZ);
                if (cMaxZ <= gMinZ) camPos.z = (gMinZ + reqOverlapZ) - halfH;                // fully below grid
                else if (cMinZ >= gMaxZ) camPos.z = (gMaxZ - reqOverlapZ) + halfH;           // fully above grid
                else camPos.z += ((camPos.z < gridCenterZ) ? +1f : -1f) * (reqOverlapZ - Mathf.Max(0f, overlapZ));
            }
        }
    }

    private Vector2 ReadMoveInput()
    {
#if ENABLE_INPUT_SYSTEM
        return _move != null ? _move.ReadValue<Vector2>() : Vector2.zero;
#else
        float x = 0f, y = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y -= 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y += 1f;
        return new Vector2(x, y);
#endif
    }

    private bool ReadBoost()
    {
#if ENABLE_INPUT_SYSTEM
        return _boost != null && _boost.IsPressed();
#else
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
#endif
    }

    private float ReadScrollDelta()
    {
#if ENABLE_INPUT_SYSTEM
        // New Input System: mouse scroll is in "lines" per frame (y positive = scroll up)
        if (_scroll == null) return 0f;
        Vector2 v = _scroll.ReadValue<Vector2>();
        return v.y;
#else
        // Legacy Input: positive y = scroll up
        return Input.mouseScrollDelta.y;
#endif
    }
}

