using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// SNES-style pawn that idly wanders around. Generates its own sprite and
/// supports simple chat interactions with other pawns.
/// </summary>
[AddComponentMenu("Units/Sprite Pawn (Test)")]
public class SpritePawn : MonoBehaviour
{
    // Registry used by selection and interaction systems
    public static readonly HashSet<SpritePawn> Instances = new HashSet<SpritePawn>();

    [Header("Sprite")]
    [SerializeField] private int spriteWidthPx = 16;
    [SerializeField] private int spriteHeightPx = 24;
    [SerializeField] private int pixelsPerUnit = 16;

    [Header("Palette")]
    [SerializeField] private Color body = new Color(0.82f, 0.80f, 0.65f, 1f);
    [SerializeField] private Color shade = new Color(0.62f, 0.60f, 0.48f, 1f);
    [SerializeField] private Color accent = new Color(0.35f, 0.42f, 0.65f, 1f);
    [SerializeField] private Color outline = new Color(0.10f, 0.10f, 0.10f, 1f);

    [Header("Selection Visual")]
    [SerializeField] private Color ringColor = new Color(1f, 0.92f, 0.25f, 1f);

    [Header("Movement")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float margin = 1.25f;

    [Header("Detection & Timing")]
    [SerializeField] private float collisionRadius = -1f; // <=0 => auto from sprite width
    [SerializeField] private float interactionCooldown = 2f;
    [SerializeField] private Vector2 chatDurationRange = new Vector2(2.5f, 4f);

    [Header("Chat Formation")]
    [SerializeField] private float chatLateral = 0.8f;           // side-by-side offset
    [SerializeField] private float chatApproachSmoothing = 8f;   // follower slot smoothing

    [Header("Idle Wander")]
    [SerializeField] private float wanderArriveRadius = 0.12f;
    [SerializeField] private float wanderPickMargin = 1f;
    [SerializeField] private float wanderMinWait = 0.4f;
    [SerializeField] private float wanderMaxWait = 1.2f;
    [SerializeField] private float wanderRepickSeconds = 6f;

    [Header("Manual Control")]
    [SerializeField] private float manualAccel = 20f;
    [SerializeField] private float manualDecel = 30f;
    private Vector2 manualInput;
    private Vector3 manualVel;

    // Public status used by manager
    public bool IsControlled => isControlled;
    // Allow controlled pawns to participate in interactions (as leaders); manager enforces follower rule.
    public bool IsInteractable => interactionState == InteractionState.None && Time.unscaledTime >= cooldownUntilUnscaled;
    public float CollisionRadius => (collisionRadius > 0f ? collisionRadius : Mathf.Max(0.2f, (float)spriteWidthPx / Mathf.Max(1, pixelsPerUnit) * 0.6f));
    [Header("Sprint")]
    [SerializeField] private float sprintMultiplier = 1.6f;
    private bool isSprinting;
    public bool IsSprinting => isSprinting && isControlled;

    // Internal state
    private Camera cam;
    private GameObject quadGO; private Material mat;
    private GameObject ringGO; private Material ringMat;
    private bool isControlled, isSelected;
    private Vector3 logicalPos;
    private Vector3 lastWorldPos, lastVelocity;

    // Wander
    private Vector3 wanderTarget;
    private bool hasWanderTarget;
    private float nextWanderPickUnscaled;
    private float wanderTargetSetUnscaled;
    private SimpleGridMap gridCache;

    // Interaction
    private enum InteractionState { None, ChatLeader, ChatFollower, ReturnToPoint }
    private InteractionState interactionState = InteractionState.None;
    private SpritePawn chatPartner;
    private float chatUntilUnscaled;
    private int chatSide = 1; // +1 right, -1 left relative to leader forward
    private float cooldownUntilUnscaled;
    private Vector3 returnPoint;

#if ENABLE_INPUT_SYSTEM
    private InputAction _moveAction;
#endif

    private void Awake()
    {
        cam = Camera.main;
        if (cam == null)
        {
#if UNITY_2022_2_OR_NEWER
            cam = UnityEngine.Object.FindAnyObjectByType<Camera>();
#else
            cam = UnityEngine.Object.FindObjectOfType<Camera>();
#endif
        }

        CreateVisual();
        EnsureCollider();
        CreateSelectionRing();

        logicalPos = transform.position;
        if (gridCache == null)
        {
#if UNITY_2022_2_OR_NEWER
            gridCache = UnityEngine.Object.FindAnyObjectByType<SimpleGridMap>();
#else
            gridCache = UnityEngine.Object.FindObjectOfType<SimpleGridMap>();
#endif
        }
        if (gridCache != null)
        {
            float w = gridCache.width * gridCache.tileSize;
            float h = gridCache.height * gridCache.tileSize;
            logicalPos = new Vector3(w * 0.5f, 0.02f, h * 0.5f);
        }
        else if (cam != null)
        {
            var b = PixelCameraHelper.OrthoWorldBounds(cam);
            logicalPos = new Vector3((b.minX + b.maxX) * 0.5f, 0.02f, (b.minZ + b.maxZ) * 0.5f);
        }
        transform.position = PixelCameraHelper.SnapToPixelGrid(logicalPos, cam);
        PickNewWanderTarget(true);
        FinalizeTransform();
    }

    private void OnEnable()
    {
        Instances.Add(this);
#if ENABLE_INPUT_SYSTEM
        if (_moveAction == null)
        {
            _moveAction = new InputAction("PawnMove", type: InputActionType.Value, binding: "2DVector");
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/s").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/a").With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/d").With("Right", "<Keyboard>/rightArrow");
            _moveAction.AddBinding("<Gamepad>/leftStick");
        }
        _moveAction.Enable();
#endif
    }

    private void OnDisable()
    {
        Instances.Remove(this);
#if ENABLE_INPUT_SYSTEM
        _moveAction?.Disable();
#endif
    }

    private void Update()
    {
        if (isControlled && interactionState != InteractionState.None)
            EndInteraction();

        if (interactionState == InteractionState.ChatFollower && chatPartner != null)
        {
            UpdateChatFollower();
            FinalizeTransform();
            return;
        }
        if (interactionState == InteractionState.ReturnToPoint)
        {
            UpdateReturnToPoint();
            FinalizeTransform();
            return;
        }

        // Manual control overrides wandering
        if (isControlled)
        {
            UpdateManualControl();
            FinalizeTransform();
            return;
        }

        // Default idle wandering movement
        UpdateIdleWander();

        // Leader state: allow normal movement while timer runs
        if (interactionState == InteractionState.ChatLeader && Time.unscaledTime >= chatUntilUnscaled)
        {
            EndInteraction();
        }

        FinalizeTransform();
    }

    // ---------------- Manual control ----------------
    private void UpdateManualControl()
    {
        // Toggle sprint with Shift (works on both input systems)
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null &&
            (Keyboard.current.leftShiftKey.wasPressedThisFrame || Keyboard.current.rightShiftKey.wasPressedThisFrame))
        {
            isSprinting = !isSprinting;
        }
#else
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) isSprinting = !isSprinting;
#endif

        Vector2 input = ReadMoveInput();
        if (input.sqrMagnitude > 1f) input.Normalize();

        // target velocity in world XZ plane at "speed"
        float mult = isSprinting ? sprintMultiplier : 1f;
        Vector3 targetVel = new Vector3(input.x, 0f, input.y) * speed * mult;

        // accelerate/decelerate toward target
        Vector3 delta = targetVel - manualVel;
        float accel = (targetVel.sqrMagnitude > manualVel.sqrMagnitude) ? manualAccel : manualDecel;
        Vector3 change = Vector3.ClampMagnitude(delta, accel * Time.deltaTime);
        manualVel += change;

        // move
        logicalPos += manualVel * Time.deltaTime;

        // clamp to grid if available
        if (gridCache == null)
        {
#if UNITY_2022_2_OR_NEWER
            gridCache = UnityEngine.Object.FindAnyObjectByType<SimpleGridMap>();
#else
            gridCache = UnityEngine.Object.FindObjectOfType<SimpleGridMap>();
#endif
        }
        if (gridCache != null)
        {
            float minX = margin, minZ = margin;
            float maxX = gridCache.width * gridCache.tileSize - margin;
            float maxZ = gridCache.height * gridCache.tileSize - margin;
            logicalPos.x = Mathf.Clamp(logicalPos.x, minX, maxX);
            logicalPos.z = Mathf.Clamp(logicalPos.z, minZ, maxZ);
        }

        transform.position = PixelCameraHelper.SnapToPixelGrid(logicalPos, cam);
    }

    // --------------------------------------------------
    // Idle wandering
    // --------------------------------------------------
    private void UpdateIdleWander()
    {
        if (!hasWanderTarget || Time.unscaledTime >= nextWanderPickUnscaled ||
            (Time.unscaledTime - wanderTargetSetUnscaled) > wanderRepickSeconds ||
            Vector3.SqrMagnitude(new Vector3(wanderTarget.x - logicalPos.x, 0f, wanderTarget.z - logicalPos.z)) <= wanderArriveRadius * wanderArriveRadius)
        {
            PickNewWanderTarget(false);
        }

        Vector3 to = new Vector3(wanderTarget.x - logicalPos.x, 0f, wanderTarget.z - logicalPos.z);
        float dist = to.magnitude;
        if (dist > 1e-4f)
        {
            Vector3 dir = to / dist;
            float step = Mathf.Min(speed * Time.deltaTime, dist);
            logicalPos += dir * step;
        }
        transform.position = PixelCameraHelper.SnapToPixelGrid(logicalPos, cam);
    }

    private void PickNewWanderTarget(bool first)
    {
        if (gridCache == null)
        {
#if UNITY_2022_2_OR_NEWER
            gridCache = UnityEngine.Object.FindAnyObjectByType<SimpleGridMap>();
#else
            gridCache = UnityEngine.Object.FindObjectOfType<SimpleGridMap>();
#endif
        }

        float minX, maxX, minZ, maxZ;
        if (gridCache != null)
        {
            minX = margin + wanderPickMargin;
            minZ = margin + wanderPickMargin;
            maxX = gridCache.width * gridCache.tileSize - (margin + wanderPickMargin);
            maxZ = gridCache.height * gridCache.tileSize - (margin + wanderPickMargin);
        }
        else if (cam != null)
        {
            var b = PixelCameraHelper.OrthoWorldBounds(cam);
            minX = b.minX + (margin + wanderPickMargin);
            maxX = b.maxX - (margin + wanderPickMargin);
            minZ = b.minZ + (margin + wanderPickMargin);
            maxZ = b.maxZ - (margin + wanderPickMargin);
        }
        else
        {
            minX = -5f; maxX = 5f; minZ = -5f; maxZ = 5f;
        }

        if (minX > maxX) { float c = (minX + maxX) * 0.5f; minX = maxX = c; }
        if (minZ > maxZ) { float c = (minZ + maxZ) * 0.5f; minZ = maxZ = c; }

        float x = Random.Range(minX, maxX);
        float z = Random.Range(minZ, maxZ);
        wanderTarget = new Vector3(x, 0.02f, z);
        hasWanderTarget = true;
        wanderTargetSetUnscaled = Time.unscaledTime;
        float wait = first ? 0f : Random.Range(wanderMinWait, wanderMaxWait);
        nextWanderPickUnscaled = Time.unscaledTime + wait;
    }

    // --------------------------------------------------
    // Interaction API & behaviour
    // --------------------------------------------------
    public void BeginChatLeader(SpritePawn follower, float seconds, Vector3 collisionPoint)
    {
        interactionState = InteractionState.ChatLeader;
        chatPartner = follower;
        chatUntilUnscaled = Time.unscaledTime + Mathf.Clamp(seconds, chatDurationRange.x, chatDurationRange.y);
    }

    public void BeginChatFollower(SpritePawn leader, float seconds, Vector3 collisionPoint)
    {
        if (isControlled) return; // cannot be follower if controlled
        interactionState = InteractionState.ChatFollower;
        chatPartner = leader;
        chatUntilUnscaled = Time.unscaledTime + Mathf.Clamp(seconds, chatDurationRange.x, chatDurationRange.y);
        chatSide = Random.value < 0.5f ? -1 : +1;
        returnPoint = collisionPoint;
    }

    private void EndInteraction()
    {
        interactionState = InteractionState.None;
        chatPartner = null;
        cooldownUntilUnscaled = Time.unscaledTime + interactionCooldown;
    }

    private void UpdateChatFollower()
    {
        if (chatPartner == null)
        {
            EndInteraction();
            return;
        }

        Vector3 lp = chatPartner.transform.position;
        Vector3 fwd = chatPartner.lastVelocity.sqrMagnitude > 1e-6f
            ? chatPartner.lastVelocity.normalized
            : new Vector3(1f, 0f, 0f);
        Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;
        Vector3 slot = lp + right * (chatSide * chatLateral);

        Vector3 to = new Vector3(slot.x - logicalPos.x, 0f, slot.z - logicalPos.z);
        float dist = to.magnitude;
        if (dist > 1e-4f)
        {
            Vector3 dir = to / dist;
            float approach = Mathf.Min(speed, dist * chatApproachSmoothing) * Time.deltaTime;
            logicalPos += dir * approach;
        }
        transform.position = PixelCameraHelper.SnapToPixelGrid(logicalPos, cam);

        if (Time.unscaledTime >= chatUntilUnscaled)
        {
            interactionState = InteractionState.ReturnToPoint;
            chatPartner = null;
        }
    }

    private void UpdateReturnToPoint()
    {
        Vector3 to = new Vector3(returnPoint.x - logicalPos.x, 0f, returnPoint.z - logicalPos.z);
        float dist = to.magnitude;
        float upp = cam != null ? PixelCameraHelper.WorldUnitsPerPixel(cam) : 0.01f;
        float arriveEps = Mathf.Max(upp, 0.05f);
        if (dist <= arriveEps)
        {
            logicalPos = new Vector3(returnPoint.x, 0.02f, returnPoint.z);
            transform.position = PixelCameraHelper.SnapToPixelGrid(logicalPos, cam);
            interactionState = InteractionState.None;
            cooldownUntilUnscaled = Time.unscaledTime + interactionCooldown;
            hasWanderTarget = false;
            PickNewWanderTarget(false);
            return;
        }
        if (dist > 1e-4f)
        {
            Vector3 dir = to / dist;
            float step = Mathf.Min(speed * Time.deltaTime, dist);
            logicalPos += dir * step;
        }
        transform.position = PixelCameraHelper.SnapToPixelGrid(logicalPos, cam);
    }

    // --------------------------------------------------
    // Visual helpers
    // --------------------------------------------------
    private void FinalizeTransform()
    {
        Vector3 wp = transform.position;
        lastVelocity = (Time.deltaTime > 1e-6f) ? (wp - lastWorldPos) / Time.deltaTime : lastVelocity;
        lastVelocity.y = 0f;
        lastWorldPos = wp;
    }

    private Vector2 ReadMoveInput()
    {
#if ENABLE_INPUT_SYSTEM
        if (_moveAction != null) return _moveAction.ReadValue<Vector2>();
        return Vector2.zero;
#else
        float x = 0f, y = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y -= 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y += 1f;
        return new Vector2(x, y);
#endif
    }

    private void CreateVisual()
    {
        int W = Mathf.Max(8, spriteWidthPx);
        int H = Mathf.Max(8, spriteHeightPx);
        var tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;

        var px = new Color32[W * H];
        Color32 cBody = body; Color32 cShade = shade; Color32 cOut = outline; Color32 cAcc = accent;
        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {
                bool border = (x == 0 || y == 0 || x == W - 1 || y == H - 1);
                Color32 c = border ? cOut : (y > H * 0.65f ? cShade : cBody);
                if (!border && y == (int)(H * 0.45f) && x > W * 0.2f && x < W * 0.8f) c = cAcc;
                px[y * W + x] = c;
            }
        }
        tex.SetPixels32(px); tex.Apply(false, false);

        var shader = Shader.Find("Unlit/Transparent") ?? Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Texture");
        mat = new Material(shader);
        if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
        if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", Color.white);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);

        quadGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quadGO.name = "SpriteQuad";
        quadGO.transform.SetParent(transform, false);
        quadGO.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        float worldW = (float)W / Mathf.Max(1, pixelsPerUnit);
        float worldH = (float)H / Mathf.Max(1, pixelsPerUnit);
        quadGO.transform.localScale = new Vector3(worldW, worldH, 1f);
        var r = quadGO.GetComponent<MeshRenderer>(); r.sharedMaterial = mat;
        var ccol = quadGO.GetComponent<Collider>(); if (ccol) Destroy(ccol);
    }

    private void EnsureCollider()
    {
        var col = gameObject.GetComponent<BoxCollider>();
        if (col == null) col = gameObject.AddComponent<BoxCollider>();
        float worldW = (float)spriteWidthPx / Mathf.Max(1, pixelsPerUnit);
        float worldH = (float)spriteHeightPx / Mathf.Max(1, pixelsPerUnit);
        col.center = new Vector3(0f, 0.05f, 0f);
        col.size = new Vector3(worldW, 0.1f, worldH);
        col.isTrigger = false;
    }

    private void CreateSelectionRing()
    {
        const int S = 64;
        var tex = new Texture2D(S, S, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        var px = new Color32[S * S];
        for (int i = 0; i < px.Length; i++) px[i] = new Color32(0, 0, 0, 0);
        float cx = (S - 1) * 0.5f, cy = (S - 1) * 0.5f;
        float rOuter = S * 0.48f;
        float rInner = S * 0.32f;
        for (int y = 0; y < S; y++)
            for (int x = 0; x < S; x++)
            {
                float dx = x - cx, dy = y - cy;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                if (d <= rOuter && d >= rInner) px[y * S + x] = ringColor;
            }
        tex.SetPixels32(px); tex.Apply(false, false);

        var shader = Shader.Find("Unlit/Transparent") ?? Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Texture");
        ringMat = new Material(shader);
        if (ringMat.HasProperty("_BaseMap")) ringMat.SetTexture("_BaseMap", tex);
        if (ringMat.HasProperty("_MainTex")) ringMat.SetTexture("_MainTex", tex);
        if (ringMat.HasProperty("_Color")) ringMat.SetColor("_Color", Color.white);
        if (ringMat.HasProperty("_BaseColor")) ringMat.SetColor("_BaseColor", Color.white);
        ringMat.renderQueue = 3000;

        ringGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
        ringGO.name = "SelectionRing";
        ringGO.transform.SetParent(transform, false);
        ringGO.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        ringGO.transform.localPosition = new Vector3(0f, 0.025f, 0f);
        float worldW = (float)spriteWidthPx / Mathf.Max(1, pixelsPerUnit);
        float scale = worldW * 1.6f;
        ringGO.transform.localScale = new Vector3(scale, scale, 1f);
        var rr = ringGO.GetComponent<MeshRenderer>(); rr.sharedMaterial = ringMat;
        var rc = ringGO.GetComponent<Collider>(); if (rc) Destroy(rc);
        ringGO.SetActive(false);
    }

    // --------------------------------------------------
    // Selection / control visuals
    // --------------------------------------------------
    public void SetControlled(bool on)
    {
        isControlled = on;
        // If we release control, clear sprint state so next time starts normal
        if (!on) { isSprinting = false; manualVel = Vector3.zero; }
        // Visual cue: brighten ring when controlled
        if (ringGO != null && ringMat != null)
        {
            float worldW = (float)spriteWidthPx / Mathf.Max(1, pixelsPerUnit);
            float scale = worldW * (on ? 1.9f : 1.6f);
            ringGO.transform.localScale = new Vector3(scale, scale, 1f);

            var col = ringColor;
            if (on) col = Color.Lerp(ringColor, Color.white, 0.3f);
            if (ringMat.HasProperty("_Color")) ringMat.SetColor("_Color", col);
            if (ringMat.HasProperty("_BaseColor")) ringMat.SetColor("_BaseColor", col);
        }
        if (on && interactionState != InteractionState.None)
            EndInteraction();
        // If we just started sprinting pre-control, ensure interactions won't linger
        if (isControlled && isSprinting && interactionState != InteractionState.None)
            EndInteraction();
        if (ringGO != null) ringGO.SetActive(on || isSelected);
    }

    public void SetSelected(bool on)
    {
        isSelected = on;
        if (ringGO != null) ringGO.SetActive(on || isControlled);
    }
}
