using UnityEngine;
using UnityEngine.UI;

namespace FantasyColony.UI.Util
{
    // Keep RectTransforms aligned to the pixel grid to avoid uneven 1px borders on 9-slice images.
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class UIPixelSnap : MonoBehaviour
    {
        RectTransform _rt;
        Canvas _canvas;

        void OnEnable()
        {
            _rt = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
            Snap();
        }

        void LateUpdate() => Snap();
        void OnRectTransformDimensionsChange() => Snap();

        void Snap()
        {
            if (_rt == null) return;
            if (_canvas == null) _canvas = GetComponentInParent<Canvas>();
            if (_canvas == null) return;

            float refPPU = _canvas.referencePixelsPerUnit <= 0 ? 100f : _canvas.referencePixelsPerUnit;
            float sf = _canvas.scaleFactor <= 0 ? 1f : _canvas.scaleFactor;
            float unitsPerPixel = 1f / (refPPU * sf);

            // Round anchored position
            var ap = _rt.anchoredPosition;
            ap.x = Mathf.Round(ap.x / unitsPerPixel) * unitsPerPixel;
            ap.y = Mathf.Round(ap.y / unitsPerPixel) * unitsPerPixel;
            _rt.anchoredPosition = ap;

            // Round sizeDelta
            var sd = _rt.sizeDelta;
            sd.x = Mathf.Round(sd.x / unitsPerPixel) * unitsPerPixel;
            sd.y = Mathf.Round(sd.y / unitsPerPixel) * unitsPerPixel;
            _rt.sizeDelta = sd;
        }
    }
}

