using UnityEngine;
using UnityEngine.UI;
using FantasyColony.UI.Style;

namespace FantasyColony.UI.Widgets
{
    // Pixel-precise frame that guarantees identical edge thickness on all sides.
    // Renders four edge Images cut from the source 9-slice sprite; optional corner Images can be added later if needed.
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class UIFrame : MonoBehaviour
    {
        [SerializeField] private Sprite _sourceNineSlice; // your dark border 9-slice
        [SerializeField] private float _targetBorderPx = 1f; // in screen pixels
        [SerializeField] private Color _tint = Color.white;
        public bool ShowLeft = true, ShowRight = true, ShowTop = true, ShowBottom = true;

        // child images
        Image _top, _bottom, _left, _right;
        RectTransform _rt;
        Canvas _canvas;

        public void Init(Sprite nineSlice, float targetPx, Color tint)
        {
            _sourceNineSlice = nineSlice;
            _targetBorderPx = targetPx;
            _tint = tint;
            EnsureChildren();
            SetEdges(ShowLeft, ShowRight, ShowTop, ShowBottom);
            ApplyLook();
            LayoutNow();
        }

        void OnEnable()
        {
            _rt = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
            EnsureChildren();
            SetEdges(ShowLeft, ShowRight, ShowTop, ShowBottom);
            ApplyLook();
            LayoutNow();
        }

        void OnRectTransformDimensionsChange() => LayoutNow();
        void Update()
        {
#if UNITY_EDITOR
            // keep live in editor
            LayoutNow();
#endif
        }

        void EnsureChildren()
        {
            if (_rt == null) _rt = GetComponent<RectTransform>();
            if (_top == null) _top = CreateEdge("Top");
            if (_bottom == null) _bottom = CreateEdge("Bottom");
            if (_left == null) _left = CreateEdge("Left");
            if (_right == null) _right = CreateEdge("Right");
        }

        Image CreateEdge(string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(transform, false);
            var img = go.GetComponent<Image>();
            img.raycastTarget = false;
            return img;
        }

        void ApplyLook()
        {
            if (_sourceNineSlice == null) return;
            // Build sub-sprites for edges from the 9-slice source using its border rect.
            // Unity's Sprite.border order: (left, bottom, right, top)
            var b = _sourceNineSlice.border;
            var rect = _sourceNineSlice.rect;
            var tex = _sourceNineSlice.texture;
            var ppu = _sourceNineSlice.pixelsPerUnit;

            // Top strip (height = b.w, y starts at rect.yMax - b.w)
            _top.sprite = Sprite.Create(tex, new Rect(rect.x, rect.yMax - b.w, rect.width, b.w), new Vector2(0.5f, 1f), ppu, 0, SpriteMeshType.FullRect);
            // Bottom strip (height = b.y)
            _bottom.sprite = Sprite.Create(tex, new Rect(rect.x, rect.y, rect.width, b.y), new Vector2(0.5f, 0f), ppu, 0, SpriteMeshType.FullRect);
            // Left strip (width = b.x)
            _left.sprite = Sprite.Create(tex, new Rect(rect.x, rect.y, b.x, rect.height), new Vector2(0f, 0.5f), ppu, 0, SpriteMeshType.FullRect);
            // Right strip (width = b.z, x starts at rect.xMax - b.z)
            _right.sprite = Sprite.Create(tex, new Rect(rect.xMax - b.z, rect.y, b.z, rect.height), new Vector2(1f, 0.5f), ppu, 0, SpriteMeshType.FullRect);

            _top.color = _bottom.color = _left.color = _right.color = _tint;

            _top.type = _bottom.type = _left.type = _right.type = Image.Type.Tiled;
            _top.preserveAspect = _bottom.preserveAspect = _left.preserveAspect = _right.preserveAspect = false;
        }

        void LayoutNow()
        {
            if (_rt == null) _rt = GetComponent<RectTransform>();
            if (_rt == null || _top == null) return;

            float px = Mathf.Max(0.0f, _targetBorderPx);
            if (_canvas == null) _canvas = GetComponentInParent<Canvas>();
            float sf = (_canvas != null && _canvas.scaleFactor > 0f) ? _canvas.scaleFactor : 1f;
            // RectTransform sizes are specified in reference pixels. Under CanvasScaler.ScaleWithScreenSize,
            // device pixels = reference pixels * scaleFactor. Therefore to get N device pixels of thickness,
            // we set sizeDelta = N / scaleFactor. Do NOT include referencePixelsPerUnit here.
            float u = Mathf.Round(px) / sf;

            // Top
            var trt = _top.rectTransform;
            trt.anchorMin = new Vector2(0f, 1f);
            trt.anchorMax = new Vector2(1f, 1f);
            trt.pivot = new Vector2(0.5f, 1f);
            trt.anchoredPosition = Vector2.zero;
            trt.sizeDelta = new Vector2(0f, u);

            // Bottom
            var brt = _bottom.rectTransform;
            brt.anchorMin = new Vector2(0f, 0f);
            brt.anchorMax = new Vector2(1f, 0f);
            brt.pivot = new Vector2(0.5f, 0f);
            brt.anchoredPosition = Vector2.zero;
            brt.sizeDelta = new Vector2(0f, u);

            // Left
            var lrt = _left.rectTransform;
            lrt.anchorMin = new Vector2(0f, 0f);
            lrt.anchorMax = new Vector2(0f, 1f);
            lrt.pivot = new Vector2(0f, 0.5f);
            lrt.anchoredPosition = Vector2.zero;
            lrt.sizeDelta = new Vector2(u, 0f);

            // Right
            var rrt = _right.rectTransform;
            rrt.anchorMin = new Vector2(1f, 0f);
            rrt.anchorMax = new Vector2(1f, 1f);
            rrt.pivot = new Vector2(1f, 0.5f);
            rrt.anchoredPosition = Vector2.zero;
            rrt.sizeDelta = new Vector2(u, 0f);
        }

        public void SetEdges(bool left, bool right, bool top, bool bottom)
        {
            ShowLeft = left; ShowRight = right; ShowTop = top; ShowBottom = bottom;
            if (_left) _left.gameObject.SetActive(ShowLeft);
            if (_right) _right.gameObject.SetActive(ShowRight);
            if (_top) _top.gameObject.SetActive(ShowTop);
            if (_bottom) _bottom.gameObject.SetActive(ShowBottom);
        }
    }
}
