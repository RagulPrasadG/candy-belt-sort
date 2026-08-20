using UnityEngine;

namespace CandyBeltSort
{
    public class SortBox : MonoBehaviour
    {
        public CandyColor Hue;
        public int Capacity = 3;
        public int Fill;
        public bool Locked;
        public bool Active = true;

        Transform _lid;
        Transform _body;
        TextMesh _label;
        Material _bodyMat;
        Material _rimMat;

        public bool IsFull => Fill >= Capacity;
        public bool CanAccept(CandyColor color) => Active && !Locked && !IsFull && Hue == color;

        public void Build(Transform parent, Vector3 pos, int capacity)
        {
            transform.SetParent(parent, false);
            transform.position = pos;
            Capacity = capacity;

            _body = MeshFactory.Primitive(PrimitiveType.Cube, "Body", transform, new Vector3(0f, 0.28f, 0f), new Vector3(1.15f, 0.55f, 0.9f), Palette.Hex("D7CCC8")).transform;
            _body.GetComponent<Collider>().enabled = false;
            _bodyMat = _body.GetComponent<MeshRenderer>().material;

            var rim = MeshFactory.Primitive(PrimitiveType.Cube, "Rim", transform, new Vector3(0f, 0.58f, 0f), new Vector3(1.2f, 0.08f, 0.95f), Color.white);
            rim.GetComponent<Collider>().enabled = false;
            _rimMat = rim.GetComponent<MeshRenderer>().material;

            _lid = MeshFactory.Primitive(PrimitiveType.Cube, "Lid", transform, new Vector3(0f, 0.72f, -0.05f), new Vector3(1.18f, 0.06f, 0.92f), Palette.Hex("A1887F")).transform;
            _lid.GetComponent<Collider>().enabled = false;
            if (_lid != null) _lid.gameObject.SetActive(false);

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(transform, false);
            labelGo.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            labelGo.transform.localRotation = Quaternion.Euler(20f, 180f, 0f);
            _label = labelGo.AddComponent<TextMesh>();
            _label.anchor = TextAnchor.MiddleCenter;
            _label.alignment = TextAlignment.Center;
            _label.characterSize = 0.12f;
            _label.fontSize = 42;
            _label.color = Palette.Ink;
            _label.text = "0/3";
            var meshRenderer = labelGo.GetComponent<MeshRenderer>();
            if (meshRenderer != null) meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        public void SetColor(CandyColor color)
        {
            Hue = color;
            Fill = 0;
            Locked = false;
            Active = true;
            if (_lid != null) _lid.gameObject.SetActive(false);
            ApplyColor();
            RefreshLabel();
        }

        public void SetLocked(bool locked)
        {
            Locked = locked;
            ApplyColor();
            RefreshLabel();
        }

        public void AddOne()
        {
            Fill++;
            RefreshLabel();
        }

        public void Seal()
        {
            if (_lid != null) _lid.gameObject.SetActive(true);
            Active = false;
        }

        public void ResetEmpty()
        {
            Fill = 0;
            Active = true;
            Locked = false;
            if (_lid != null) _lid.gameObject.SetActive(false);
            RefreshLabel();
        }

        void ApplyColor()
        {
            var c = Locked ? Palette.Hex("90A4AE") : Palette.Of(Hue);
            if (_rimMat != null)
            {
                _rimMat.color = c;
                if (_rimMat.HasProperty("_BaseColor")) _rimMat.SetColor("_BaseColor", c);
            }
            if (_bodyMat != null)
            {
                var cardboard = Color.Lerp(Palette.Hex("EFEBE9"), c, 0.18f);
                _bodyMat.color = cardboard;
                if (_bodyMat.HasProperty("_BaseColor")) _bodyMat.SetColor("_BaseColor", cardboard);
            }
        }

        void RefreshLabel()
        {
            if (_label == null) return;
            if (Locked) _label.text = "LOCKED";
            else _label.text = $"{Fill}/{Capacity}";
        }
    }
}
