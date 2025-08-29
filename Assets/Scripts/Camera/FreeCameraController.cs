using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// WASD/Arrow camera panning when no pawn is controlled.
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

    private Camera _cam;
    private SimpleGridMap _grid;

#if ENABLE_INPUT_SYSTEM
    private InputAction _move;
    private InputAction _boost;
#endif

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        if (_cam == null) _cam = Camera.main;
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

#if ENABLE_INPUT_SYSTEM
    private void OnEnable()
    {
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
        if (_boost == null)
        {
            _boost = new InputAction("CamBoost", binding: "<Keyboard>/shift");
        }
        _move.Enable();
        _boost.Enable();
    }

    private void OnDisable()
    {
        _move?.Disable();
        _boost?.Disable();
    }
#endif

    private void Update()
    {
        // Only active when no pawn is controlled
        if (ControlManager.Controlled != null) return;
        if (_cam == null) return;

        Vector2 input = ReadMoveInput();
        if (input.sqrMagnitude > 1f) input.Normalize();
        bool boost = ReadBoost();

        float speed = moveSpeed * (boost ? boostMultiplier : 1f);
        Vector3 delta = new Vector3(input.x, 0f, input.y) * speed * Time.unscaledDeltaTime;
        Vector3 pos = _cam.transform.position + delta;

        if (clampToGrid)
        {
            TryFindGrid();
            if (_grid != null)
            {
                ClampToGrid(ref pos, _cam, _grid, clampMargin);
            }
        }

        _cam.transform.position = pos;
    }

    private static void ClampToGrid(ref Vector3 camPos, Camera cam, SimpleGridMap grid, float margin)
    {
        float wWorld = grid.width * grid.tileSize;
        float hWorld = grid.height * grid.tileSize;
        float minX = 0f + margin;
        float minZ = 0f + margin;
        float maxX = wWorld - margin;
        float maxZ = hWorld - margin;

        // Subtract half view so camera doesn't show outside the grid.
        float halfH = cam.orthographicSize;
        float halfW = halfH * ((Screen.height > 0) ? (Screen.width / (float)Screen.height) : (16f / 9f));

        float clampMinX = minX + halfW;
        float clampMaxX = maxX - halfW;
        float clampMinZ = minZ + halfH;
        float clampMaxZ = maxZ - halfH;

        // If the grid is smaller than the viewport, center the camera.
        if (clampMinX > clampMaxX)
        {
            float cx = (minX + maxX) * 0.5f;
            camPos.x = cx;
        }
        else
        {
            camPos.x = Mathf.Clamp(camPos.x, clampMinX, clampMaxX);
        }

        if (clampMinZ > clampMaxZ)
        {
            float cz = (minZ + maxZ) * 0.5f;
            camPos.z = cz;
        }
        else
        {
            camPos.z = Mathf.Clamp(camPos.z, clampMinZ, clampMaxZ);
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
}

