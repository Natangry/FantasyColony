using UnityEngine;
using UnityEngine.EventSystems;

namespace FantasyColony.UI.Creator.Editing
{
    /// <summary>
    /// Drag to move a RectTransform within a stage (top-left anchored space).
    /// Assumes target pivot=(0,1) and anchors fixed to stage (0,1).
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class UIDragMove : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private RectTransform _target; // optional; defaults to self
        [SerializeField] private RectTransform _stage;
        private Vector2 _startPos;
        private Vector2 _startMouseLocal;

        private void Awake()
        {
            if (_target == null) _target = GetComponent<RectTransform>();
            if (_stage == null && _target != null) _stage = _target.parent as RectTransform;
        }

        public void Init(RectTransform target, RectTransform stage)
        {
            _target = target; _stage = stage;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_target == null || _stage == null) return;
            _startPos = _target.anchoredPosition;
            ScreenToStageLocal(eventData, out _startMouseLocal);
            Debug.Log($"[UICreator] Drag begin: {_target.name} pos={_startPos}");
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_target == null || _stage == null) return;
            Vector2 mouseLocal;
            ScreenToStageLocal(eventData, out mouseLocal);
            var delta = mouseLocal - _startMouseLocal;
            var pos = _startPos + delta;

            if (GridPrefs.SnapEnabled && !IsAltHeld())
            {
                pos.x = Mathf.Round(pos.x / GridPrefs.CellSize) * GridPrefs.CellSize;
                pos.y = Mathf.Round(pos.y / GridPrefs.CellSize) * GridPrefs.CellSize;
            }

            var w = _target.rect.width;
            var h = _target.rect.height;
            var sw = _stage.rect.width;
            var sh = _stage.rect.height;

            pos.x = Mathf.Clamp(pos.x, 0f, Mathf.Max(0f, sw - w));
            pos.y = Mathf.Clamp(pos.y, -Mathf.Max(0f, sh - h), 0f);

            _target.anchoredPosition = pos;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_target == null) return;
            Debug.Log($"[UICreator] Drag end: {_target.name} pos={_target.anchoredPosition}");
        }

        private void ScreenToStageLocal(PointerEventData ev, out Vector2 local)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_stage, ev.position, null, out local);
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

