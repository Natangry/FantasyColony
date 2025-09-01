using System.Collections.Generic;
using UnityEngine;

namespace FantasyColony.UI.Util
{
    public static class GrayscaleSpriteCache
    {
        // Cache by source texture instanceID + rect
        private static readonly Dictionary<string, Sprite> _cache = new();

        public static Sprite Get(Sprite source)
        {
            if (source == null) return null;
            var key = MakeKey(source);
            if (_cache.TryGetValue(key, out var s)) return s;

            var rect = source.rect;
            var tex = source.texture;
            var x = Mathf.RoundToInt(rect.x);
            var y = Mathf.RoundToInt(rect.y);
            var w = Mathf.RoundToInt(rect.width);
            var h = Mathf.RoundToInt(rect.height);

            // Read pixels from source region
            var tmp = new Texture2D(w, h, TextureFormat.RGBA32, false, false);
            var pixels = tex.GetPixels(x, y, w, h);
            for (int i = 0; i < pixels.Length; i++)
            {
                var c = pixels[i];
                float g = (0.299f * c.r) + (0.587f * c.g) + (0.114f * c.b);
                pixels[i] = new Color(g, g, g, c.a);
            }
            tmp.SetPixels(pixels);
            tmp.Apply(false, true);

            // Create a sprite with same PPU and pivot
            var sp = Sprite.Create(tmp, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), source.pixelsPerUnit, 0, SpriteMeshType.FullRect);
            _cache[key] = sp;
            return sp;
        }

        private static string MakeKey(Sprite s)
        {
            var r = s.rect;
            return s.texture.GetInstanceID().ToString() + ":" + r.x + "," + r.y + "," + r.width + "," + r.height + ":" + s.pixelsPerUnit;
        }
    }
}

