using UnityEngine;

/// <summary>
/// When a pawn is assumed controlled, center the camera on it and smoothly follow.
/// Stops following when control is released.
/// </summary>
[AddComponentMenu("Camera/Follow Controlled Pawn")]
public class FollowControlledPawn : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField] private float smoothTime = 0.15f;
    [SerializeField] private bool snapOnAcquire = true;
    [SerializeField] private bool pixelSnap = false; // disable by default to avoid double-quantization jitter
    [Tooltip("Optional XZ world offset from the pawn center.")]
    [SerializeField] private Vector2 offset = Vector2.zero;
    [Header("Stability")]
    [SerializeField] private bool useDeadZone = true;
    [SerializeField] private float deadZonePixels = 0.75f;

    private Camera _cam;
    private Transform _target;
    private Vector3 _vel; // SmoothDamp velocity

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        if (_cam == null) _cam = Camera.main;
    }

    private void OnEnable()
    {
        ControlManager.OnControlledChanged += OnControlledChanged;
        // If a pawn is already controlled at startup, target it immediately.
        if (ControlManager.Controlled != null)
        {
            _target = ControlManager.Controlled.transform;
            if (snapOnAcquire) SnapToTarget();
        }
    }

    private void OnDisable()
    {
        ControlManager.OnControlledChanged -= OnControlledChanged;
    }

    private void OnControlledChanged(SpritePawn pawn)
    {
        _target = pawn ? pawn.transform : null;
        if (_target != null && snapOnAcquire)
        {
            SnapToTarget();
        }
    }

    private void LateUpdate()
    {
        if (_target == null || _cam == null) return;

        // Desired position keeps current camera Y & rotation; moves X/Z toward target.
        Vector3 desired = new Vector3(
            _target.position.x + offset.x,
            _cam.transform.position.y,
            _target.position.z + offset.y
        );

        Vector3 current = _cam.transform.position;

        // Optional dead-zone measured in on-screen pixels to prevent micro hunting.
        if (useDeadZone)
        {
            float dx = desired.x - current.x;
            float dz = desired.z - current.z;
            float dist = Mathf.Sqrt(dx * dx + dz * dz);
            float upp = Mathf.Max(1e-6f, PixelCameraHelper.WorldUnitsPerPixel(_cam));
            float threshold = upp * Mathf.Max(0f, deadZonePixels);
            if (dist <= threshold)
            {
                var snapPos = pixelSnap ? SnapPosToPixelGrid(desired, _cam) : desired;
                _cam.transform.position = snapPos;
                _vel = Vector3.zero;
                return;
            }
        }

        Vector3 pos = Vector3.SmoothDamp(current, desired, ref _vel, Mathf.Max(0.0001f, smoothTime));

        if (pixelSnap)
            pos = SnapPosToPixelGrid(pos, _cam);

        _cam.transform.position = pos;
    }

    private void SnapToTarget()
    {
        if (_target == null || _cam == null) return;
        var pos = new Vector3(
            _target.position.x + offset.x,
            _cam.transform.position.y,
            _target.position.z + offset.y
        );
        if (pixelSnap)
            pos = SnapPosToPixelGrid(pos, _cam);
        _cam.transform.position = pos;
        _vel = Vector3.zero;
    }

    private static Vector3 SnapPosToPixelGrid(Vector3 worldPos, Camera cam)
    {
        float upp = Mathf.Max(1e-6f, PixelCameraHelper.WorldUnitsPerPixel(cam));
        worldPos.x = Mathf.Round(worldPos.x / upp) * upp;
        worldPos.z = Mathf.Round(worldPos.z / upp) * upp;
        return worldPos;
    }
}
