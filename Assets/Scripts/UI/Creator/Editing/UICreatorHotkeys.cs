using System;
using UnityEngine;

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

        public void Init(RectTransform stage, RectTransform layerBG, RectTransform layerPanels, RectTransform layerControls, Action onMenuRefresh)
        {
            _stage = stage; _layerBG = layerBG; _layerPanels = layerPanels; _layerControls = layerControls; _onMenuRefresh = onMenuRefresh;
            Debug.Log("[UICreator] Hotkeys ready");
        }

        private void Update()
        {
            // Toggle grid (G) and cycle size (Ctrl+G)
            if (Input.GetKeyDown(KeyCode.G))
            {
                bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
                if (ctrl)
                {
                    GridPrefs.CycleCellSize();
                }
                else
                {
                    GridPrefs.GridVisible = !GridPrefs.GridVisible;
                    Debug.Log($"[UICreator] Grid {(GridPrefs.GridVisible ? "on" : "off")}");
                }
                _onMenuRefresh?.Invoke();
            }

            // Snap toggle (F4)
            if (Input.GetKeyDown(KeyCode.F4))
            {
                GridPrefs.SnapEnabled = !GridPrefs.SnapEnabled;
                Debug.Log($"[UICreator] Snap {(GridPrefs.SnapEnabled ? "on" : "off")}");
            }

            // Delete selected (Del/Backspace)
            if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
            {
                var sel = UISelectionBox.CurrentTarget;
                if (sel == null)
                {
                    Debug.Log("[UICreator] Delete: nothing selected");
                    return;
                }
                if (sel == _stage || sel == _layerBG || sel == _layerPanels || sel == _layerControls)
                {
                    Debug.Log("[UICreator] Delete: refusing to delete stage/layer");
                    return;
                }
                var name = sel.name;
                Destroy(sel.gameObject);
                Debug.Log($"[UICreator] Deleted: {name}");
                _onMenuRefresh?.Invoke();
            }
        }
    }
}

