using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FantasyColony.UI.Creator.Editing
{
    /// <summary>
    /// MonoBehaviour that captures keyboard shortcuts for the UI Creator.
    /// Attach to the stage so Update() runs; delegates actions back to the screen.
    /// </summary>
    public sealed class UICreatorHotkeys : MonoBehaviour
    {
        private RectTransform _stage, _layerBG, _layerPanels, _layerControls;
        private Action _onMenuRefresh;
        private Action _toggleFullscreen, _toggleGrid, _cycleGridSize, _toggleSnap, _deleteSelected;
        private Func<bool> _canDelete;

        public void Init(
            RectTransform stage,
            RectTransform layerBG,
            RectTransform layerPanels,
            RectTransform layerControls,
            Action onMenuRefresh,
            Action toggleFullscreen,
            Action toggleGrid,
            Action cycleGridSize,
            Action toggleSnap,
            Func<bool> canDelete,
            Action deleteSelected)
        {
            _stage = stage; _layerBG = layerBG; _layerPanels = layerPanels; _layerControls = layerControls;
            _onMenuRefresh = onMenuRefresh;
            _toggleFullscreen = toggleFullscreen; _toggleGrid = toggleGrid; _cycleGridSize = cycleGridSize; _toggleSnap = toggleSnap;
            _canDelete = canDelete; _deleteSelected = deleteSelected;
            Debug.Log("[UICreator] Hotkeys ready");
        }

        private void Update()
        {
            // Don't fire hotkeys while typing into an InputField/TMP_InputField
            var go = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            if (go != null && (go.GetComponent<UnityEngine.UI.InputField>() != null || HasTMPInput(go))) return;

#if ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb == null) return;

            // F11: fullscreen toggle
            if (kb.f11Key != null && kb.f11Key.wasPressedThisFrame) { _toggleFullscreen?.Invoke(); return; }

            // Ctrl+G cycles grid size, G toggles
            bool ctrl = (kb.leftCtrlKey != null && kb.leftCtrlKey.isPressed) || (kb.rightCtrlKey != null && kb.rightCtrlKey.isPressed);
            if (kb.gKey != null && kb.gKey.wasPressedThisFrame)
            {
                if (ctrl) _cycleGridSize?.Invoke(); else _toggleGrid?.Invoke();
                _onMenuRefresh?.Invoke();
                return;
            }

            // F4: snap toggle
            if (kb.f4Key != null && kb.f4Key.wasPressedThisFrame) { _toggleSnap?.Invoke(); return; }

            // Delete / Backspace
            bool del = kb.deleteKey != null && kb.deleteKey.wasPressedThisFrame;
            bool back = kb.backspaceKey != null && kb.backspaceKey.wasPressedThisFrame;
            if (del || back)
            {
                if (_canDelete == null || _canDelete()) _deleteSelected?.Invoke();
                return;
            }
#else
            // Legacy Input path
            if (Input.GetKeyDown(KeyCode.F11)) { _toggleFullscreen?.Invoke(); return; }
            if (Input.GetKeyDown(KeyCode.G))
            {
                bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
                if (ctrl) _cycleGridSize?.Invoke(); else _toggleGrid?.Invoke();
                _onMenuRefresh?.Invoke();
                return;
            }
            if (Input.GetKeyDown(KeyCode.F4)) { _toggleSnap?.Invoke(); return; }
            if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
            {
                if (_canDelete == null || _canDelete()) _deleteSelected?.Invoke();
                return;
            }
#endif
        }

        private static bool HasTMPInput(GameObject go)
        {
#if TMP_PRESENT
            return go.GetComponent<TMPro.TMP_InputField>() != null;
#else
            return false;
#endif
        }
    }
}

