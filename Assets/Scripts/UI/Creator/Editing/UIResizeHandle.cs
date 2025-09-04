using UnityEngine;
using UnityEngine.EventSystems;

namespace FantasyColony.UI.Creator.Editing
{
    /// <summary>
    /// Drag to resize a RectTransform. Assumes target pivot=(0,1) (top-left) and fixed anchors.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class UIResizeHandle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public enum Side { N, S, E, W, NE, NW, SE, SW }

        [SerializeField] private RectTransform _target;
        [SerializeField] private RectTransform _stage;
        [SerializeField] private Side _side;
        [SerializeField] private Vector2 _minSize = new Vector2(40, 24);

        private Vector2 _startMouseLocal;
        private Vector2 _startPos;
        private Vector2 _startSize;

        public void Init(RectTransform target, RectTransform stage, Side side)
        {
            _target = target; _stage = stage; _side = side;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_target == null || _stage == null) return;
            _startPos = _target.anchoredPosition;
            _startSize = _target.rect.size;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_stage, eventData.position, null, out _startMouseLocal);
            Debug.Log($"[UICreator] Resize begin: {_target.name} side={_side}");
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_target == null || _stage == null) return;
            Vector2 mouseLocal; RectTransformUtility.ScreenPointToLocalPointInRectangle(_stage, eventData.position, null, out mouseLocal);
            Vector2 d = mouseLocal - _startMouseLocal;

            float left = _startPos.x;
            float top = _startPos.y;
            float right = left + _startSize.x;
            float bottom = top - _startSize.y;

            if (_side == Side.E || _side == Side.NE || _side == Side.SE) right += d.x;
            if (_side == Side.W || _side == Side.NW || _side == Side.SW) left += d.x;
            if (_side == Side.N || _side == Side.NE || _side == Side.NW) top += d.y;
            if (_side == Side.S || _side == Side.SE || _side == Side.SW) bottom += d.y;

            if (GridPrefs.SnapEnabled && !IsAltHeld())
            {
                left = Mathf.Round(left / GridPrefs.CellSize) * GridPrefs.CellSize;
                right = Mathf.Round(right / GridPrefs.CellSize) * GridPrefs.CellSize;
                top = Mathf.Round(top / GridPrefs.CellSize) * GridPrefs.CellSize;
                bottom = Mathf.Round(bottom / GridPrefs.CellSize) * GridPrefs.CellSize;
            }

            float sw = _stage.rect.width;
            float sh = _stage.rect.height;
            left = Mathf.Clamp(left, 0f, sw);
            right = Mathf.Clamp(right, 0f, sw);
            top = Mathf.Clamp(top, -0f, 0f);
            bottom = Mathf.Clamp(bottom, -sh, 0f);

            float w = Mathf.Max(_minSize.x, right - left);
            float h = Mathf.Max(_minSize.y, top - bottom);
            right = left + w;
            bottom = top - h;

            _target.anchoredPosition = new Vector2(left, top);
            _target.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
            _target.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_target == null) return;
            Debug.Log($"[UICreator] Resize end: {_target.name} size={_target.rect.size}");
        }

        private static bool IsAltHeld()
        {
#if ENABLE_INPUT_SYSTEM
            return UnityEngine.InputSystem.Keyboard.current != null &&
                   (UnityEngine.InputSystem.Keyboard.current.leftAltKey.isPressed || UnityEngine.InputSystem.Keyboard.current.rightAltKey.isPressed);
#else
            return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
#endif
        }
    }
}

