using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using FantasyColony.UI.Widgets;

namespace FantasyColony.UI.Creator.Editing
{
    /// <summary>
    /// Single-selection box with 8 resize handles for a placeable.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class UISelectionBox : MonoBehaviour, IPointerDownHandler
    {
        private static UISelectionBox _current;

        public static RectTransform CurrentTarget => _current != null ? _current._target : null;

        [SerializeField] private RectTransform _target; // self by default
        [SerializeField] private RectTransform _stage;
        [SerializeField] private RectTransform _overlay; // visual box parent

        private void Awake()
        {
            if (_target == null) _target = GetComponent<RectTransform>();
            if (_stage == null) _stage = _target.parent as RectTransform;
        }

        public void Init(RectTransform target, RectTransform stage)
        {
            _target = target; _stage = stage;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Select();
        }

        public void Select()
        {
            if (_current == this) { Refresh(); return; }
            if (_current != null) _current.ClearOverlay();
            _current = this;
            BuildOverlay();
            Debug.Log($"[UICreator] Select: {_target.name}");
        }

        private void BuildOverlay()
        {
            ClearOverlay();
            _overlay = new GameObject("SelectionOverlay", typeof(RectTransform)).GetComponent<RectTransform>();
            _overlay.SetParent(_target, false);
            _overlay.anchorMin = new Vector2(0, 1);
            _overlay.anchorMax = new Vector2(0, 1);
            _overlay.pivot = new Vector2(0, 1);
            _overlay.anchoredPosition = Vector2.zero;
            _overlay.sizeDelta = _target.rect.size;

            var border = _overlay.gameObject.AddComponent<UIFrame>();
            border.SetEdges(true, true, true, true);

            CreateHandle(UIResizeHandle.Side.N, new Vector2(0.5f, 1f));
            CreateHandle(UIResizeHandle.Side.S, new Vector2(0.5f, 0f));
            CreateHandle(UIResizeHandle.Side.E, new Vector2(1f, 1f));
            CreateHandle(UIResizeHandle.Side.W, new Vector2(0f, 1f));
            CreateHandle(UIResizeHandle.Side.NE, new Vector2(1f, 1f));
            CreateHandle(UIResizeHandle.Side.NW, new Vector2(0f, 1f));
            CreateHandle(UIResizeHandle.Side.SE, new Vector2(1f, 0f));
            CreateHandle(UIResizeHandle.Side.SW, new Vector2(0f, 0f));
        }

        private void CreateHandle(UIResizeHandle.Side side, Vector2 anchor)
        {
            var go = new GameObject($"Handle_{side}", typeof(RectTransform), typeof(Image), typeof(UIResizeHandle));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(_overlay, false);
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(12, 12);
            var img = go.GetComponent<Image>(); var c = img.color; c.a = 0.6f; img.color = c;

            var h = go.GetComponent<UIResizeHandle>();
            h.Init(_target, _stage, side);
        }

        private void LateUpdate()
        {
            if (_overlay != null)
            {
                _overlay.sizeDelta = _target.rect.size;
            }
        }

        private void Refresh()
        {
            if (_overlay == null) BuildOverlay();
        }

        private void OnDisable() => ClearOverlay();

        private void OnDestroy() => ClearOverlay();

        private void ClearOverlay()
        {
            if (_overlay != null)
            {
                Destroy(_overlay.gameObject);
                _overlay = null;
            }
        }
    }
}

