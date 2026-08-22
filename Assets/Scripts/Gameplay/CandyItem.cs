using UnityEngine;

namespace CandyBeltSort
{
    public class CandyItem : MonoBehaviour
    {
        public CandyColor Hue;
        public bool Frozen;
        public bool Hidden;
        public bool Bomb;
        public float Distance;
        public bool Collecting;
        public bool OnBelt = true;
        public int Lane;
        public bool Emerging { get; private set; }
        public bool Takeable { get; private set; }
        public bool Selected { get; private set; }

        MeshRenderer _art;
        Transform _shadow;
        Vector3 _baseScale;
        Vector3 _artRest = new Vector3(0f, 0.14f, 0f);
        Vector3 _artScale = Vector3.one;
        Color _artTint = Color.white;
        float _emerge;
        MaterialPropertyBlock _block;

        public void Build(SpawnSpec spec)
        {
            Hue = spec.Color;
            Frozen = spec.Frozen;
            Hidden = spec.Hidden;
            Bomb = spec.Bomb;
            Lane = spec.Lane;
            _baseScale = Vector3.one * 0.9f;
            transform.localScale = _baseScale;
            _block = new MaterialPropertyBlock();

            var sprite = SpriteFactory.Candy(Hue, Bomb);
            var size = sprite != null ? (Vector2)sprite.bounds.size : Vector2.one;
            if (size.x < 0.2f) size = Vector2.one * 0.9f;
            var artGo = MeshFactory.OverlayQuad("Art", transform, _artRest, size, sprite != null ? sprite.texture : null);
            _art = artGo.GetComponent<MeshRenderer>();
            _artScale = artGo.transform.localScale;

            var shadowGo = SpriteFactory.Ground("Shadow", transform, new Vector3(0f, -0.4f, 0.04f), new Vector2(0.7f, 0.4f), Color.white, 2);
            shadowGo.GetComponent<SpriteRenderer>().sprite = SpriteFactory.Shadow();
            _shadow = shadowGo.transform;

            var hit = gameObject.AddComponent<SphereCollider>();
            hit.radius = 0.5f;
            hit.center = new Vector3(0f, 0.12f, 0f);
            Takeable = false;
            ApplyVisual();
        }

        public void BeginEmerge()
        {
            Emerging = true;
            _emerge = 0f;
            if (_art != null) _art.transform.localScale = _artScale * 0.12f;
        }

        public void Reveal()
        {
            Hidden = false;
            ApplyVisual();
        }

        public void Thaw()
        {
            Frozen = false;
            ApplyVisual();
        }

        public void SetTakeable(bool takeable)
        {
            if (Takeable == takeable) return;
            Takeable = takeable;
            ApplyVisual();
        }

        public void SetSelected(bool selected)
        {
            Selected = selected;
        }

        public void ApplyVisual()
        {
            Color tint = Color.white;
            if (Hidden) tint = Palette.HiddenFoil;
            else if (Frozen && !Bomb) tint = Color.Lerp(Color.white, Palette.FrozenTint, 0.4f);
            if (!Takeable && !Emerging) tint = new Color(tint.r * 0.84f, tint.g * 0.84f, tint.b * 0.84f, 1f);
            _artTint = tint;
            PushTint();
        }

        void PushTint()
        {
            if (_art == null) return;
            if (_block == null) _block = new MaterialPropertyBlock();
            _art.GetPropertyBlock(_block);
            _block.SetColor("_BaseColor", _artTint);
            _block.SetColor("_Color", _artTint);
            _art.SetPropertyBlock(_block);
        }

        void Update()
        {
            if (Emerging)
            {
                _emerge += Time.deltaTime / 0.28f;
                float u = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(_emerge));
                if (_art != null) _art.transform.localScale = _artScale * Mathf.Lerp(0.12f, 1f, u);
                if (_shadow != null)
                    _shadow.localScale = new Vector3(Mathf.Lerp(0.2f, 0.7f, u), Mathf.Lerp(0.12f, 0.4f, u), 1f);
                if (u >= 1f)
                {
                    Emerging = false;
                    ApplyVisual();
                }
            }

            if (Collecting || !OnBelt) return;
            float pulse;
            if (Emerging) pulse = 1f;
            else if (!Takeable) pulse = 0.94f;
            else if (Selected) pulse = 1.12f + Mathf.Sin(Time.time * 10f) * 0.05f;
            else pulse = 1.05f + Mathf.Sin(Time.time * 8f) * 0.03f;
            transform.localScale = _baseScale * pulse;
        }

        void LateUpdate()
        {
            if (_art == null) return;
            var cam = Camera.main;
            if (cam == null) return;

            float pop = Emerging ? (1f - Mathf.Clamp01(_emerge)) * 0.45f : 0f;
            var world = transform.TransformPoint(_artRest + new Vector3(0f, 0f, pop));
            _art.transform.position = world - cam.transform.forward * 0.35f;
            _art.transform.rotation = cam.transform.rotation;
        }
    }
}
