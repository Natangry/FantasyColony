using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FantasyColony.UI.Creator.Editing
{
    /// <summary>
    /// Lightweight grid overlay for the Creator stage. Drawn with pooled 1px Images.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class UIStageGrid : MonoBehaviour
    {
        [SerializeField] private RectTransform _rt;
        private readonly List<Image> _lines = new List<Image>();
        private bool _dirty = true;

        private void Awake()
        {
            if (_rt == null) _rt = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            _dirty = true;
        }

        private void OnRectTransformDimensionsChange()
        {
            _dirty = true;
        }

        private void Update()
        {
            if (_dirty) Rebuild();
            foreach (var img in _lines)
            {
                if (img) img.enabled = GridPrefs.GridVisible;
            }
        }

        private void Rebuild()
        {
            _dirty = false;
            if (_rt.rect.width <= 0 || _rt.rect.height <= 0) return;

            int cell = Mathf.Max(2, GridPrefs.CellSize);
            int cols = Mathf.CeilToInt(_rt.rect.width / cell) + 1;
            int rows = Mathf.CeilToInt(_rt.rect.height / cell) + 1;
            int need = cols + rows;
            while (_lines.Count < need)
            {
                var go = new GameObject("grid-line", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(transform, false);
                var img = go.GetComponent<Image>();
                var c = img.color; c.a = 0.08f; img.color = c; // subtle tint
                _lines.Add(img);
            }
            for (int i = need; i < _lines.Count; i++)
            {
                if (_lines[i]) _lines[i].enabled = false;
            }

            int idx = 0;
            // Vertical lines
            for (int x = 0; x <= cols; x++)
            {
                var img = _lines[idx++];
                var rt = img.rectTransform;
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(1, _rt.rect.height);
                rt.anchoredPosition = new Vector2(x * cell, -_rt.rect.height * 0.5f);
                img.enabled = GridPrefs.GridVisible;
            }
            // Horizontal lines
            for (int y = 0; y <= rows; y++)
            {
                var img = _lines[idx++];
                var rt = img.rectTransform;
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(_rt.rect.width, 1);
                rt.anchoredPosition = new Vector2(_rt.rect.width * 0.5f, -y * cell);
                img.enabled = GridPrefs.GridVisible;
            }
        }

        public Vector2 Snap(Vector2 localTopLeft)
        {
            int cell = Mathf.Max(2, GridPrefs.CellSize);
            float x = Mathf.Round(localTopLeft.x / cell) * cell;
            float y = Mathf.Round(localTopLeft.y / cell) * cell;
            return new Vector2(x, y);
        }

        public void MarkDirty() => _dirty = true;
    }
}

