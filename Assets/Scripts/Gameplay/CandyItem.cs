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

        MeshRenderer _body;
        MeshRenderer _wrap;
        Material _bodyMat;
        Vector3 _baseScale;

        public void Build(SpawnSpec spec)
        {
            Hue = spec.Color;
            Frozen = spec.Frozen;
            Hidden = spec.Hidden;
            Bomb = spec.Bomb;
            _baseScale = Vector3.one * 0.72f;
            transform.localScale = _baseScale;

            var body = MeshFactory.Primitive(PrimitiveType.Sphere, "Body", transform, Vector3.zero, Vector3.one, Color.white);
            body.GetComponent<Collider>().enabled = false;
            _body = body.GetComponent<MeshRenderer>();
            _bodyMat = _body.material;

            var wrap = MeshFactory.Primitive(PrimitiveType.Cylinder, "Band", transform, Vector3.zero, new Vector3(1.05f, 0.12f, 1.05f), Color.white);
            wrap.GetComponent<Collider>().enabled = false;
            _wrap = wrap.GetComponent<MeshRenderer>();

            var hit = gameObject.AddComponent<SphereCollider>();
            hit.radius = 0.55f;
            gameObject.layer = 0;
            ApplyVisual();
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

        public void ApplyVisual()
        {
            Color color;
            if (Bomb) color = Palette.Bomb;
            else if (Hidden) color = Palette.HiddenFoil;
            else color = Palette.Of(Hue);

            if (Frozen && !Bomb) color = Color.Lerp(color, Palette.FrozenTint, 0.55f);

            if (_bodyMat != null)
            {
                _bodyMat.color = color;
                if (_bodyMat.HasProperty("_BaseColor")) _bodyMat.SetColor("_BaseColor", color);
            }

            if (_wrap != null)
            {
                var band = Bomb ? Palette.Hex("212121") : Color.Lerp(color, Color.white, 0.35f);
                var mat = _wrap.material;
                mat.color = band;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", band);
            }
        }
    }
}
