using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Central click-to-select controller. Left click selects a SpritePawn (if any), click empty clears.
/// </summary>
[AddComponentMenu("Systems/Selection Controller")]
public class SelectionController : MonoBehaviour
{
    public static SpritePawn Selected { get; private set; }
    public static event System.Action<SpritePawn> OnSelectionChanged;

#if ENABLE_INPUT_SYSTEM
    private InputAction _clickAction;
#endif

    private Camera _cam;

    private void Awake()
    {
        _cam = Camera.main;
        if (_cam == null)
        {
            // Fallback to any camera in scene
#if UNITY_2022_2_OR_NEWER
            var any = UnityEngine.Object.FindAnyObjectByType<Camera>();
#else
            var any = UnityEngine.Object.FindObjectOfType<Camera>();
#endif
            _cam = any;
        }
    }

    private void Update()
    {
#if ENABLE_INPUT_SYSTEM
        // New Input System uses the bound action in OnEnable().
#else
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 pos = Input.mousePosition;
            TrySelectFromScreen(pos);
        }
#endif
    }

    public static void SetSelected(SpritePawn pawn)
    {
        if (Selected == pawn) return;
        Selected = pawn;
        try { OnSelectionChanged?.Invoke(Selected); } catch { /* no-op */ }
    }

    private void TrySelectFromScreen(Vector3 screenPos)
    {
        if (_cam == null) return;
        var ray = _cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out var hit, 1000f, ~0, QueryTriggerInteraction.Ignore))
        {
            var pawn = hit.collider.GetComponentInParent<SpritePawn>();
            SetSelected(pawn);
        }
        else
        {
            SetSelected(null);
        }
    }

#if ENABLE_INPUT_SYSTEM
    private void OnEnable()
    {
        if (_clickAction == null)
        {
            _clickAction = new InputAction("SelectClick", binding: "<Mouse>/leftButton");
            _clickAction.performed += ctx =>
            {
                var mouse = Mouse.current;
                if (mouse == null) return;
                TrySelectFromScreen(mouse.position.ReadValue());
            };
        }
        _clickAction.Enable();
    }

    private void OnDisable()
    {
        if (_clickAction != null) _clickAction.Disable();
    }
#endif
}

