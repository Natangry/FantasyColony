using System.Collections.Generic;
using UnityEngine;

namespace FantasyColony.UI.Util
{
    public static class GrayscaleSpriteCache
    {
        // Cache by source texture instanceID + rect
        private class Entry
        {
            public Sprite Sprite;
            public int RefCount;
        }
        private static readonly Dictionary<string, Entry> _cache = new();

        public static Sprite Get(Sprite source)
        {
            if (source == null) return null;
            var key = MakeKey(source);
            if (_cache.TryGetValue(key, out var e))
            {
                e.RefCount++;
                return e.Sprite;
            }

            var rect = source.rect;
            var tex = source.texture;
            if (tex == null)
                return null;
            // CPU fallback requires readable textures; atlased sprites in builds are often not readable.
            if (!tex.isReadable)
            {
                Debug.LogWarning($"[GrayscaleSpriteCache] Source texture not readable for '{source.name}', skipping CPU grayscale.");
                return null;
            }
            var x = Mathf.RoundToInt(rect.x);
            var y = Mathf.RoundToInt(rect.y);
            var w = Mathf.RoundToInt(rect.width);
            var h = Mathf.RoundToInt(rect.height);

            // Read pixels from source region
            var tmp = new Texture2D(w, h, TextureFormat.RGBA32, false, false)
            {
                hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor
            };
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
            _cache[key] = new Entry { Sprite = sp, RefCount = 1 };
            return sp;
        }

        public static void Release(Sprite source)
        {
            if (source == null) return;
            var key = MakeKey(source);
            if (!_cache.TryGetValue(key, out var e)) return;
            e.RefCount--;
            if (e.RefCount > 0) return;

            if (e.Sprite != null)
            {
                var tex = e.Sprite.texture;
                if (tex != null)
                    Object.Destroy(tex);
                Object.Destroy(e.Sprite);
            }
            _cache.Remove(key);
        }

        public static void Clear()
        {
            foreach (var e in _cache.Values)
            {
                var sp = e.Sprite;
                if (sp == null) continue;
                var tex = sp.texture;
                if (tex != null)
                    Object.Destroy(tex);
                Object.Destroy(sp);
            }
            _cache.Clear();
        }

        private static string MakeKey(Sprite s)
        {
            var r = s.rect;
            return s.texture.GetInstanceID().ToString() + ":" + r.x + "," + r.y + "," + r.width + "," + r.height + ":" + s.pixelsPerUnit;
        }
    }
}

