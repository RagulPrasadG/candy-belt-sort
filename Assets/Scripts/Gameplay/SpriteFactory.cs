using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CandyBeltSort
{
    public static class SpriteFactory
    {
        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();
        static readonly Dictionary<string, Texture2D> TexCache = new Dictionary<string, Texture2D>();
        static Sprite _white;

        public static Sprite White
        {
            get
            {
                if (_white != null) return _white;
                _white = FromTex(Solid(8, 8, Color.white), 8f);
                return _white;
            }
        }

        public static GameObject Ground(string name, Transform parent, Vector3 pos, Vector2 size, Color color, int order = 0)
        {
            return Make(name, parent, pos, new Vector3(90f, 0f, 0f), size, White, color, order);
        }

        public static GameObject Billboard(string name, Transform parent, Vector3 pos, Vector2 size, Sprite sprite, Color color, int order = 8)
        {
            return Make(name, parent, pos, Vector3.zero, size, sprite, color, order);
        }

        static Material _spriteMat;

        public static Material SpriteMat
        {
            get
            {
                if (_spriteMat != null) return _spriteMat;
                var shader = Shader.Find("Sprites/Default");
                if (shader == null) shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                if (shader == null) shader = Shader.Find("Unlit/Transparent");
                _spriteMat = new Material(shader);
                _spriteMat.SetInt("_ZWrite", 0);
                _spriteMat.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                _spriteMat.renderQueue = 3000;
                return _spriteMat;
            }
        }

        public static GameObject Make(string name, Transform parent, Vector3 pos, Vector3 euler, Vector2 size, Sprite sprite, Color color, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.Euler(euler);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite != null ? sprite : White;
            sr.sharedMaterial = SpriteMat;
            sr.color = color;
            sr.sortingOrder = order;
            sr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return go;
        }

        public static Sprite Candy(CandyColor color, bool bomb)
        {
            string key = bomb ? "candy_bomb" : "candy_" + color.ToString().ToLowerInvariant();
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;

            var tex = LoadPng(key);
            if (tex != null)
            {
                tex = PrepareCandy(tex);
                float max = Mathf.Max(tex.width, tex.height);
                var sprite = FromTex(tex, max / 0.92f);
                Cache[key] = sprite;
                return sprite;
            }

            return CandyFallback(key);
        }

        public static Sprite Shadow() => Drawn("blob_shadow", DrawShadow);
        public static Sprite Glow() => Drawn("soft_glow", DrawGlow);
        public static Sprite Window() => Drawn("window_sky", DrawWindow);

        public static Texture2D FloorTex(Color grout, Color a, Color b)
        {
            string key = "floor_" + ColorUtility.ToHtmlStringRGB(a) + ColorUtility.ToHtmlStringRGB(b);
            return Tex(key, () => DrawFloor(grout, a, b));
        }

        public static Texture2D WallTex(Color grout, Color tile)
        {
            string key = "wall_" + ColorUtility.ToHtmlStringRGB(tile);
            return Tex(key, () => DrawWall(grout, tile));
        }

        public static Texture2D WoodTex() => Tex("wood_plank", DrawWood);
        public static Texture2D EnamelTex() => Tex("enamel_panel", DrawEnamel);
        public static Texture2D BrickTex() => Tex("clay_brick", DrawBrick);
        public static Texture2D CardboardTex() => Tex("cardboard", DrawCardboard);
        public static Texture2D BeltTex() => Tex("belt_rubber", DrawBelt);
        public static Texture2D MarbleTex() => Tex("marble", DrawMarble);
        public static Texture2D MetalTex() => Tex("brushed_metal", DrawMetal);

        static Sprite _buttonSkin;
        static bool _buttonSkinTried;

        // A 9-sliced glossy button skin (tintable, neutral) or null when the PNG is absent.
        public static Sprite ButtonSkin()
        {
            if (_buttonSkinTried) return _buttonSkin;
            _buttonSkinTried = true;
            var tex = LoadPng("ui_button");
            if (tex == null) { _buttonSkin = null; return _buttonSkin; }
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            float b = Mathf.Min(tex.width, tex.height) * 0.33f;
            var border = new Vector4(b, b, b, b);
            _buttonSkin = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), tex.width * 0.95f, 0, SpriteMeshType.FullRect, border);
            return _buttonSkin;
        }

        // Returns a sprite from a bundled PNG, or null when the art has not been added yet
        // (so UI can gracefully fall back to a flat colour instead of a candy blob).
        public static Sprite TryNamed(string key, bool punch = false)
        {
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;
            var tex = LoadPng(key);
            if (tex == null) return null;
            if (punch) PunchBackdrop(tex);
            var sprite = FromTex(tex, tex.width * 0.95f);
            Cache[key] = sprite;
            return sprite;
        }

        public static Sprite Named(string key)
        {
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;
            var tex = LoadPng(key);
            if (tex != null)
            {
                PunchBackdrop(tex);
                var sprite = FromTex(tex, tex.width * 0.95f);
                Cache[key] = sprite;
                return sprite;
            }

            return CandyFallback(key);
        }

        static Sprite Drawn(string key, System.Func<Texture2D> bake)
        {
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;
            var sprite = FromTex(bake(), 128f);
            Cache[key] = sprite;
            return sprite;
        }

        static Texture2D Tex(string key, System.Func<Texture2D> bake)
        {
            if (TexCache.TryGetValue(key, out var cached) && cached != null) return cached;
            var tex = bake();
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            TexCache[key] = tex;
            return tex;
        }

        static Sprite FromTex(Texture2D tex, float ppu)
        {
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), ppu);
        }

        static Texture2D Solid(int w, int h, Color c)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var px = new Color[w * h];
            for (int i = 0; i < px.Length; i++) px[i] = c;
            tex.SetPixels(px);
            tex.Apply();
            return tex;
        }

        static Texture2D LoadPng(string key)
        {
            var path = Path.Combine(Application.dataPath, "Resources", "CandyBelt", "Sprites", key + ".png");
            if (!File.Exists(path)) return null;
            var bytes = File.ReadAllBytes(path);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.LoadImage(bytes);
            return tex;
        }

        static void PunchBackdrop(Texture2D tex)
        {
            var px = tex.GetPixels();
            if (px.Length == 0) return;
            var samples = new[] { px[0], px[tex.width - 1], px[px.Length - 1], px[px.Length - tex.width] };
            for (int i = 0; i < px.Length; i++)
            {
                bool punch = px[i].a < 0.08f;
                if (!punch)
                {
                    for (int s = 0; s < samples.Length; s++)
                    {
                        if (samples[s].a > 0.2f && ColorDistance(px[i], samples[s]) < 0.18f)
                        {
                            punch = true;
                            break;
                        }
                    }
                }

                if (punch) px[i] = Color.clear;
            }

            tex.SetPixels(px);
            tex.Apply();
        }

        static float ColorDistance(Color a, Color b)
        {
            float dr = a.r - b.r, dg = a.g - b.g, db = a.b - b.b;
            return Mathf.Sqrt(dr * dr + dg * dg + db * db);
        }

        static Texture2D PrepareCandy(Texture2D src)
        {
            PunchEdgeBackdrop(src);
            var cropped = CropOpaque(src, 8);
            return cropped;
        }

        static void PunchEdgeBackdrop(Texture2D tex)
        {
            var px = tex.GetPixels();
            int w = tex.width, h = tex.height;
            if (px.Length == 0) return;
            var corners = new[] { px[0], px[w - 1], px[px.Length - 1], px[px.Length - w] };

            bool Backdrop(Color c)
            {
                if (c.a < 0.14f) return true;
                for (int s = 0; s < corners.Length; s++)
                {
                    if (corners[s].a > 0.14f && ColorDistance(c, corners[s]) < 0.22f)
                        return true;
                }
                return false;
            }

            var seen = new bool[px.Length];
            var stack = new Stack<int>();
            void TryPush(int i)
            {
                if ((uint)i >= (uint)px.Length || seen[i] || !Backdrop(px[i])) return;
                seen[i] = true;
                stack.Push(i);
            }

            for (int x = 0; x < w; x++)
            {
                TryPush(x);
                TryPush((h - 1) * w + x);
            }
            for (int y = 0; y < h; y++)
            {
                TryPush(y * w);
                TryPush(y * w + (w - 1));
            }

            while (stack.Count > 0)
            {
                int i = stack.Pop();
                px[i] = Color.clear;
                int x = i % w;
                int y = i / w;
                if (x > 0) TryPush(i - 1);
                if (x + 1 < w) TryPush(i + 1);
                if (y > 0) TryPush(i - w);
                if (y + 1 < h) TryPush(i + w);
            }

            tex.SetPixels(px);
            tex.Apply();
        }

        static Texture2D CropOpaque(Texture2D tex, int pad)
        {
            var px = tex.GetPixels();
            int w = tex.width, h = tex.height;
            int minX = w, minY = h, maxX = 0, maxY = 0;
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                if (px[y * w + x].a < 0.06f) continue;
                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;
            }

            if (maxX <= minX || maxY <= minY) return tex;
            minX = Mathf.Max(0, minX - pad);
            minY = Mathf.Max(0, minY - pad);
            maxX = Mathf.Min(w - 1, maxX + pad);
            maxY = Mathf.Min(h - 1, maxY + pad);
            int nw = maxX - minX + 1;
            int nh = maxY - minY + 1;
            var cropped = new Texture2D(nw, nh, TextureFormat.RGBA32, false);
            cropped.SetPixels(tex.GetPixels(minX, minY, nw, nh));
            cropped.Apply();
            return cropped;
        }

        static Sprite CandyFallback(string key)
        {
            var hue = Palette.Pink;
            if (key.Contains("mint")) hue = Palette.Mint;
            else if (key.Contains("lemon")) hue = Palette.Lemon;
            else if (key.Contains("blue")) hue = Palette.Blueberry;
            else if (key.Contains("grape")) hue = Palette.Grape;
            else if (key.Contains("bomb")) hue = Palette.Bomb;
            var p = new Paint(256);
            p.SoftCircle(128, 118, 96, MeshFactory.Shade(hue, 0.5f));
            p.SoftCircle(128, 132, 88, hue);
            p.SoftCircle(96, 168, 28, new Color(1f, 1f, 1f, 0.6f));
            var sprite = FromTex(p.Bake(), 256f / 0.92f);
            Cache[key] = sprite;
            return sprite;
        }

        static Texture2D DrawShadow()
        {
            var p = new Paint(128);
            p.SoftEllipse(64, 64, 56, 28, new Color(0.08f, 0.05f, 0.04f, 0.45f));
            return p.Bake();
        }

        static Texture2D DrawGlow()
        {
            var p = new Paint(128);
            p.SoftCircle(64, 64, 60, new Color(1f, 0.92f, 0.55f, 0.35f));
            p.SoftCircle(64, 64, 28, new Color(1f, 0.98f, 0.85f, 0.55f));
            return p.Bake();
        }

        static Texture2D DrawWindow()
        {
            var p = new Paint(160);
            p.RoundRect(8, 8, 144, 144, 10, Palette.Hex("F5E6D3"));
            p.Rect(18, 18, 124, 124, Palette.Hex("7EC8F5"));
            for (int y = 18; y < 142; y++)
            {
                float u = (y - 18) / 124f;
                var sky = Color.Lerp(Palette.Hex("B3E5FC"), Palette.Hex("FFF3C4"), u);
                p.Rect(18, y, 124, 1, sky);
            }
            p.SoftCircle(108, 108, 18, new Color(1f, 0.95f, 0.6f, 0.85f));
            p.Rect(76, 8, 10, 144, Palette.Hex("EFEBE9"));
            p.Rect(8, 76, 144, 10, Palette.Hex("EFEBE9"));
            p.Rect(8, 8, 144, 12, Palette.Hex("D7CCC8"));
            return p.Bake();
        }

        static Texture2D DrawFloor(Color grout, Color a, Color b)
        {
            const int n = 256;
            var p = new Paint(n);
            int tile = 16;
            Color light = Color.Lerp(a, Color.white, 0.08f);
            Color dark = Color.Lerp(b, Palette.Hex("D7CCC8"), 0.35f);
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                int tx = x / tile;
                int ty = y / tile;
                bool gro = x % tile < 2 || y % tile < 2;
                Color c = gro ? grout : (((tx + ty) & 1) == 0 ? light : dark);
                float nse = (Noise(x, y) - 0.5f) * 0.07f;
                c = new Color(c.r + nse, c.g + nse * 0.85f, c.b + nse * 0.6f, 1f);
                int lx = x % tile, ly = y % tile;
                if (!gro && (lx == 2 || ly == 2)) c = Color.Lerp(c, Color.white, 0.16f);
                if (!gro && (lx > tile - 3 || ly > tile - 3)) c = Color.Lerp(c, Color.black, 0.08f);
                p.Set(x, y, c);
            }
            return p.Bake();
        }

        static Texture2D DrawWall(Color grout, Color tile)
        {
            const int n = 256;
            var p = new Paint(n);
            int tw = 36, th = 16;
            var shine = Color.Lerp(tile, Color.white, 0.28f);
            var shade = Color.Lerp(tile, grout, 0.25f);
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                int row = y / th;
                int ox = (row & 1) == 0 ? 0 : tw / 2;
                int lx = (x + ox) % tw;
                int ly = y % th;
                bool line = lx < 2 || ly < 2;
                Color c;
                if (line) c = grout;
                else
                {
                    float u = lx / (float)tw;
                    c = Color.Lerp(shine, shade, u * 0.45f + ly / (float)th * 0.2f);
                    if (ly == 3 || lx == 3) c = Color.Lerp(c, Color.white, 0.2f);
                }
                p.Set(x, y, c);
            }
            return p.Bake();
        }

        static Texture2D DrawCardboard()
        {
            var p = new Paint(256);
            var kraft = Palette.Hex("D7B899");
            var flute = Palette.Hex("C4A882");
            var edge = Palette.Hex("A1887F");
            for (int y = 0; y < 256; y++)
            for (int x = 0; x < 256; x++)
            {
                float wave = 0.5f + 0.5f * Mathf.Sin(x * 0.7f);
                var c = Color.Lerp(kraft, flute, wave * 0.5f);
                if (y % 42 < 2) c = edge;
                float nse = (Noise(x, y) - 0.5f) * 0.1f;
                p.Set(x, y, new Color(c.r + nse, c.g + nse * 0.7f, c.b + nse * 0.4f, 1f));
            }
            return p.Bake();
        }

        static Texture2D DrawBelt()
        {
            var p = new Paint(256);
            var rubber = Palette.Hex("2E3A40");
            var chevron = Palette.Hex("F4D03F");
            var groove = Palette.Hex("1C2529");
            for (int y = 0; y < 256; y++)
            for (int x = 0; x < 256; x++)
            {
                var c = rubber;
                int band = (y * 2 + x / 2) % 40;
                if (band < 8) c = chevron;
                else if (band < 11) c = groove;
                if (x < 14 || x > 241) c = Palette.Hex("1A2328");
                float nse = (Noise(x, y) - 0.5f) * 0.05f;
                p.Set(x, y, new Color(c.r + nse, c.g + nse, c.b + nse, 1f));
            }
            return p.Bake();
        }

        static Texture2D DrawMarble()
        {
            var p = new Paint(256);
            var basec = Palette.Hex("F7F0E8");
            for (int y = 0; y < 256; y++)
            for (int x = 0; x < 256; x++)
            {
                float v = Mathf.PerlinNoise(x * 0.03f, y * 0.055f);
                float v2 = Mathf.PerlinNoise(x * 0.09f + 8f, y * 0.025f);
                var c = Color.Lerp(basec, Palette.Hex("E4D5C8"), v * 0.55f);
                if (v2 > 0.58f) c = Color.Lerp(c, Palette.Hex("A1887F"), (v2 - 0.58f) * 1.6f);
                if (v2 > 0.72f) c = Color.Lerp(c, Palette.Hex("8D6E63"), 0.35f);
                p.Set(x, y, c);
            }
            return p.Bake();
        }

        static Texture2D DrawMetal()
        {
            var p = new Paint(256);
            for (int y = 0; y < 256; y++)
            for (int x = 0; x < 256; x++)
            {
                float u = x / 256f;
                var c = Color.Lerp(Palette.Hex("78909C"), Palette.Hex("ECEFF1"), 0.3f + 0.28f * Mathf.Sin(u * 22f));
                c = Color.Lerp(c, Palette.Hex("455A64"), Noise(x, y) * 0.18f);
                if (y % 64 < 2) c = Palette.Hex("546E7A");
                p.Set(x, y, c);
            }
            return p.Bake();
        }

        static Texture2D DrawWood()
        {
            var p = new Paint(256);
            var light = Palette.Hex("C4A574");
            var dark = Palette.Hex("8D6E4F");
            var knot = Palette.Hex("6D4C41");
            for (int y = 0; y < 256; y++)
            for (int x = 0; x < 256; x++)
            {
                float grain = Mathf.PerlinNoise(x * 0.12f, y * 0.012f);
                float ring = 0.5f + 0.5f * Mathf.Sin(y * 0.08f + grain * 4f);
                var c = Color.Lerp(light, dark, ring * 0.7f + grain * 0.3f);
                if (x % 64 < 2) c = knot;
                p.Set(x, y, c);
            }
            return p.Bake();
        }

        static Texture2D DrawEnamel()
        {
            var p = new Paint(256);
            var basec = Palette.Hex("B0BEC5");
            var seam = Palette.Hex("607D8B");
            var rivet = Palette.Hex("90A4AE");
            for (int y = 0; y < 256; y++)
            for (int x = 0; x < 256; x++)
            {
                var c = Color.Lerp(basec, Color.white, 0.08f + 0.12f * Mathf.PerlinNoise(x * 0.04f, y * 0.04f));
                if (x % 64 < 3 || y % 64 < 3) c = seam;
                int dx = (x % 64) - 8, dy = (y % 64) - 8;
                if (dx * dx + dy * dy < 10) c = rivet;
                if (x % 64 == 4 || y % 64 == 4) c = Color.Lerp(c, Color.white, 0.25f);
                p.Set(x, y, c);
            }
            return p.Bake();
        }

        static Texture2D DrawBrick()
        {
            const int n = 256;
            var p = new Paint(n);
            var brickA = Palette.Hex("E57373");
            var brickB = Palette.Hex("EF9A9A");
            var mortar = Palette.Hex("EFEBE9");
            int tw = 40, th = 18;
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                int row = y / th;
                int ox = (row & 1) == 0 ? 0 : tw / 2;
                int lx = (x + ox) % tw;
                int ly = y % th;
                bool line = lx < 3 || ly < 3;
                Color c = line ? mortar : Color.Lerp(brickA, brickB, Noise(x / 4, y / 4));
                if (!line && ly == 4) c = Color.Lerp(c, Color.white, 0.18f);
                p.Set(x, y, c);
            }
            return p.Bake();
        }

        static float Noise(int x, int y)
        {
            float n = Mathf.Sin(x * 12.9898f + y * 78.233f) * 43758.5453f;
            return n - Mathf.Floor(n);
        }

        struct Paint
        {
            public readonly int Size;
            readonly Color[] Px;

            public Paint(int size)
            {
                Size = size;
                Px = new Color[size * size];
            }

            public void Set(int x, int y, Color c)
            {
                if ((uint)x >= (uint)Size || (uint)y >= (uint)Size) return;
                Px[y * Size + x] = c;
            }

            public void Blend(int x, int y, Color c)
            {
                if ((uint)x >= (uint)Size || (uint)y >= (uint)Size) return;
                int i = y * Size + x;
                var d = Px[i];
                float a = c.a;
                Px[i] = new Color(d.r + (c.r - d.r) * a, d.g + (c.g - d.g) * a, d.b + (c.b - d.b) * a, Mathf.Max(d.a, a));
            }

            public void Rect(int x, int y, int w, int h, Color c)
            {
                int x2 = x + w, y2 = y + h;
                for (int yy = y; yy < y2; yy++)
                for (int xx = x; xx < x2; xx++)
                    Set(xx, yy, c);
            }

            public void RoundRect(int x, int y, int w, int h, int r, Color c)
            {
                Rect(x + r, y, w - r * 2, h, c);
                Rect(x, y + r, w, h - r * 2, c);
                SoftCircle(x + r, y + r, r, c);
                SoftCircle(x + w - r, y + r, r, c);
                SoftCircle(x + r, y + h - r, r, c);
                SoftCircle(x + w - r, y + h - r, r, c);
            }

            public void SoftCircle(int cx, int cy, int radius, Color c)
            {
                int r2 = radius * radius;
                int fade = Mathf.Max(2, radius / 8);
                for (int y = cy - radius; y <= cy + radius; y++)
                for (int x = cx - radius; x <= cx + radius; x++)
                {
                    int d2 = (x - cx) * (x - cx) + (y - cy) * (y - cy);
                    if (d2 > r2) continue;
                    float edge = Mathf.Sqrt(r2) - Mathf.Sqrt(d2);
                    float a = c.a * Mathf.Clamp01(edge / fade);
                    Blend(x, y, new Color(c.r, c.g, c.b, a));
                }
            }

            public void SoftEllipse(int cx, int cy, int rx, int ry, Color c)
            {
                for (int y = cy - ry; y <= cy + ry; y++)
                for (int x = cx - rx; x <= cx + rx; x++)
                {
                    float u = (x - cx) / (float)rx;
                    float v = (y - cy) / (float)ry;
                    float d = u * u + v * v;
                    if (d > 1f) continue;
                    float a = c.a * Mathf.Clamp01(1.3f - d * 1.3f);
                    Blend(x, y, new Color(c.r, c.g, c.b, a));
                }
            }

            public Texture2D Bake()
            {
                var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
                tex.SetPixels(Px);
                tex.Apply();
                return tex;
            }
        }
    }
}
