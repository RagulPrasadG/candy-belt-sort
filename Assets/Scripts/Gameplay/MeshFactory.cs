using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CandyBeltSort
{
    public static class MeshFactory
    {
        static readonly Dictionary<long, Material> LitCache = new Dictionary<long, Material>();
        static readonly Dictionary<long, Material> TexCache = new Dictionary<long, Material>();
        static Shader _lit;

        static Shader LitShader
        {
            get
            {
                if (_lit != null) return _lit;
                _lit = Shader.Find("Universal Render Pipeline/Lit");
                if (_lit == null) _lit = Shader.Find("Universal Render Pipeline/Simple Lit");
                if (_lit == null) _lit = Shader.Find("Standard");
                return _lit;
            }
        }

        public static Material Lit(Color color, float smoothness = 0.32f)
        {
            long key = ColorKey(color, smoothness);
            if (LitCache.TryGetValue(key, out var cached) && cached != null) return cached;
            var mat = new Material(LitShader);
            ApplyColor(mat, color);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", smoothness > 0.75f ? 0.12f : smoothness > 0.6f ? 0.18f : 0.02f);
            if (smoothness >= 0.8f && mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color * 0.08f);
            }
            LitCache[key] = mat;
            return mat;
        }

        public static Material Textured(Texture tex, Color tint, float smoothness = 0.22f, float tileX = 1f, float tileY = 1f)
        {
            int tx = Mathf.RoundToInt(tileX * 8f);
            int ty = Mathf.RoundToInt(tileY * 8f);
            long key = (tex != null ? tex.GetInstanceID() : 0) * 397L ^ ColorKey(tint, smoothness) ^ ((long)tx << 32) ^ ((long)ty << 40);
            if (TexCache.TryGetValue(key, out var cached) && cached != null) return cached;
            var mat = new Material(LitShader);
            ApplyColor(mat, tint);
            if (tex != null)
            {
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
                mat.SetTextureScale("_BaseMap", new Vector2(tileX, tileY));
                mat.SetTextureScale("_MainTex", new Vector2(tileX, tileY));
            }
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", smoothness > 0.45f ? 0.22f : 0.04f);
            TexCache[key] = mat;
            return mat;
        }

        public static GameObject Solid(string name, Transform parent, Vector3 pos, Vector3 scale, Color color, float smoothness = 0.28f, PrimitiveType type = PrimitiveType.Cube)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = Lit(color, smoothness);
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
            return go;
        }

        public static GameObject TexturedSolid(string name, Transform parent, Vector3 pos, Vector3 scale, Texture tex, Color tint, float smoothness = 0.2f, float tileX = 1f, float tileY = 1f)
        {
            var go = Solid(name, parent, pos, scale, tint, smoothness);
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = Textured(tex, tint, smoothness, tileX, tileY);
            return go;
        }

        public static GameObject TexturedBlock(string name, Transform parent, Vector3 pos, Vector3 size, Texture tex, Color color, float smoothness = 0.28f, float tileX = 2f, float tileY = 2f)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = pos;
            TexturedSolid("Body", root.transform, Vector3.zero, size, tex, Shade(color, 0.9f), smoothness, tileX, tileY);
            TexturedSolid("Top", root.transform, new Vector3(0f, size.y * 0.5f + 0.012f, 0f),
                new Vector3(size.x * 0.985f, Mathf.Max(0.03f, size.y * 0.045f), size.z * 0.985f),
                tex, Shade(color, 1.16f), smoothness + 0.08f, tileX, tileY);
            return root;
        }

        public static GameObject Block(string name, Transform parent, Vector3 pos, Vector3 size, Color color, float smoothness = 0.28f)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = pos;
            Solid("Body", root.transform, Vector3.zero, size, Shade(color, 0.9f), smoothness);
            Solid("Top", root.transform, new Vector3(0f, size.y * 0.5f + 0.012f, 0f),
                new Vector3(size.x * 0.985f, Mathf.Max(0.03f, size.y * 0.045f), size.z * 0.985f),
                Shade(color, 1.16f), smoothness + 0.08f);
            return root;
        }

        public static Material Overlay(Texture tex)
        {
            int id = tex != null ? tex.GetInstanceID() : 0;
            long key = id * 991L + 7;
            if (TexCache.TryGetValue(key, out var cached) && cached != null) return cached;

            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Transparent");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            var mat = new Material(shader);
            ApplyColor(mat, Color.white);
            if (tex != null)
            {
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
            }
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
            if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0f);
            if (mat.HasProperty("_AlphaClip")) mat.SetFloat("_AlphaClip", 1f);
            if (mat.HasProperty("_Cutoff")) mat.SetFloat("_Cutoff", 0.12f);
            mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.SetFloat("_ZTest", (float)CompareFunction.Always);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.EnableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.renderQueue = 4000;
            TexCache[key] = mat;
            return mat;
        }

        public static GameObject OverlayQuad(string name, Transform parent, Vector3 pos, Vector2 size, Texture tex)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = Overlay(tex);
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.allowOcclusionWhenDynamic = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            return go;
        }

        public static Color Shade(Color c, float mul)
        {
            return new Color(Mathf.Clamp01(c.r * mul), Mathf.Clamp01(c.g * mul), Mathf.Clamp01(c.b * mul), c.a);
        }

        static void ApplyColor(Material mat, Color color)
        {
            mat.color = color;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        }

        static long ColorKey(Color c, float smoothness)
        {
            int r = Mathf.RoundToInt(c.r * 31f);
            int g = Mathf.RoundToInt(c.g * 31f);
            int b = Mathf.RoundToInt(c.b * 31f);
            int a = Mathf.RoundToInt(c.a * 15f);
            int s = Mathf.RoundToInt(smoothness * 20f);
            return r | (g << 6) | (b << 12) | (a << 18) | ((long)s << 24);
        }
    }
}
