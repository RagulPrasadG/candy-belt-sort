using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace CandyBeltSort
{
    public class SortBox : MonoBehaviour
    {
        public CandyColor Hue;
        public bool HasHue;
        public int Capacity = 3;
        public int Fill;
        public bool Locked;
        public bool Active = true;

        Transform[] _flaps;
        Vector3[] _flapOpen;
        Vector3[] _flapClosed;
        Transform _tape;
        Vector3 _tapeScale;
        Transform _liner;
        MeshRenderer[] _cardboard;
        TextMesh _label;
        GameObject _lockBadge;

        public bool IsFull => Fill >= Capacity;
        public bool IsEmptySlot => Active && !Locked && !HasHue && Fill == 0;

        public bool CanAccept(CandyColor color)
        {
            if (!Active || Locked || IsFull) return false;
            if (!HasHue) return true;
            return Hue == color;
        }

        public void Build(Transform parent, Vector3 pos, int capacity)
        {
            transform.SetParent(parent, false);
            transform.position = pos;
            Capacity = capacity;

            var kraft = Palette.Hex("E0C09A");
            var kraftSide = Palette.Hex("C4A074");
            var kraftDark = Palette.Hex("8D6E4F");
            var cardboard = SpriteFactory.CardboardTex();

            SpriteFactory.Ground("Shadow", transform, new Vector3(0f, 0.02f, 0.05f), new Vector2(1.35f, 1.05f), new Color(0.12f, 0.08f, 0.05f, 0.4f), 1);

            MeshFactory.TexturedSolid("Bottom", transform, new Vector3(0f, 0.06f, 0f), new Vector3(1.12f, 0.1f, 1.12f), cardboard, kraftDark, 0.12f);
            var front = MeshFactory.TexturedSolid("Front", transform, new Vector3(0f, 0.42f, 0.52f), new Vector3(1.12f, 0.72f, 0.09f), cardboard, kraft, 0.14f);
            var back = MeshFactory.TexturedSolid("Back", transform, new Vector3(0f, 0.42f, -0.52f), new Vector3(1.12f, 0.72f, 0.09f), cardboard, kraftDark, 0.14f);
            var left = MeshFactory.TexturedSolid("Left", transform, new Vector3(-0.52f, 0.42f, 0f), new Vector3(0.09f, 0.72f, 0.95f), cardboard, kraftSide, 0.14f);
            var right = MeshFactory.TexturedSolid("Right", transform, new Vector3(0.52f, 0.42f, 0f), new Vector3(0.09f, 0.72f, 0.95f), cardboard, kraftSide, 0.14f);

            _liner = MeshFactory.Solid("Liner", transform, new Vector3(0f, 0.13f, 0f), new Vector3(0.92f, 0.03f, 0.92f), Palette.Hex("FFF8E1"), 0.2f).transform;

            _flaps = new Transform[4];
            _flapOpen = new[] { new Vector3(24f, 0f, 0f), new Vector3(-24f, 0f, 0f), new Vector3(0f, 0f, 24f), new Vector3(0f, 0f, -24f) };
            _flapClosed = new[] { new Vector3(-88f, 0f, 0f), new Vector3(88f, 0f, 0f), new Vector3(0f, 0f, -88f), new Vector3(0f, 0f, 88f) };
            _flaps[0] = MakeFlap("FlapF", new Vector3(0f, 0.78f, 0.52f), new Vector3(1.02f, 0.42f, 0.07f), _flapOpen[0], kraft);
            _flaps[1] = MakeFlap("FlapB", new Vector3(0f, 0.78f, -0.52f), new Vector3(1.02f, 0.42f, 0.07f), _flapOpen[1], kraftDark);
            _flaps[2] = MakeFlap("FlapL", new Vector3(-0.52f, 0.78f, 0f), new Vector3(0.07f, 0.42f, 0.88f), _flapOpen[2], kraftSide);
            _flaps[3] = MakeFlap("FlapR", new Vector3(0.52f, 0.78f, 0f), new Vector3(0.07f, 0.42f, 0.88f), _flapOpen[3], kraftSide);

            var tapeGo = new GameObject("Tape");
            tapeGo.transform.SetParent(transform, false);
            tapeGo.transform.localPosition = new Vector3(0f, 0.88f, 0f);
            MeshFactory.Solid("StripA", tapeGo.transform, Vector3.zero, new Vector3(1.08f, 0.05f, 0.2f), Palette.Hex("F7DC6F"), 0.4f);
            MeshFactory.Solid("StripB", tapeGo.transform, Vector3.zero, new Vector3(0.2f, 0.05f, 1.08f), Palette.Hex("F4D03F"), 0.4f);
            _tape = tapeGo.transform;
            _tape.gameObject.SetActive(false);
            _tapeScale = Vector3.one;

            _cardboard = new[]
            {
                front.GetComponent<MeshRenderer>(),
                back.GetComponent<MeshRenderer>(),
                left.GetComponent<MeshRenderer>(),
                right.GetComponent<MeshRenderer>()
            };

            MeshFactory.Solid("Paper", transform, new Vector3(0f, 0.5f, 0.58f), new Vector3(0.7f, 0.38f, 0.02f), Palette.Hex("FFF8E1"), 0.12f);
            var hit = gameObject.AddComponent<BoxCollider>();
            hit.center = new Vector3(0f, 0.45f, 0f);
            hit.size = new Vector3(1.25f, 1.05f, 1.25f);

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(transform, false);
            labelGo.transform.localPosition = new Vector3(0f, 1.28f, 0f);
            _label = labelGo.AddComponent<TextMesh>();
            _label.anchor = TextAnchor.MiddleCenter;
            _label.alignment = TextAlignment.Center;
            _label.characterSize = 0.08f;
            _label.fontSize = 44;
            _label.fontStyle = FontStyle.Bold;
            _label.color = Palette.Ink;
            var meshRenderer = labelGo.GetComponent<MeshRenderer>();
            if (meshRenderer != null) meshRenderer.shadowCastingMode = ShadowCastingMode.Off;

            _lockBadge = MeshFactory.Solid("Lock", transform, new Vector3(0f, 0.55f, 0.58f), new Vector3(0.28f, 0.32f, 0.1f), Palette.Hex("78909C"), 0.4f);
            MeshFactory.Solid("Shackle", _lockBadge.transform, new Vector3(0f, 0.22f, 0f), new Vector3(0.16f, 0.18f, 0.06f), Palette.Hex("B0BEC5"), 0.5f);
            _lockBadge.SetActive(false);

            ResetEmpty();
        }

        Transform MakeFlap(string name, Vector3 pivot, Vector3 board, Vector3 open, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = pivot;
            go.transform.localEulerAngles = open;
            MeshFactory.TexturedSolid("Board", go.transform, new Vector3(0f, board.y * 0.5f, 0f), board, SpriteFactory.CardboardTex(), color, 0.14f);
            return go.transform;
        }

        public void Commit(CandyColor color)
        {
            HasHue = true;
            Hue = color;
            ApplyColor();
            RefreshLabel();
        }

        public void SetLocked(bool locked)
        {
            Locked = locked;
            if (_lockBadge != null) _lockBadge.SetActive(locked);
            ApplyColor();
            RefreshLabel();
        }

        public void AddOne(CandyColor color)
        {
            if (!HasHue) Commit(color);
            Fill++;
            RefreshLabel();
        }

        public void Seal()
        {
            Active = false;
            if (_label != null) _label.text = "PACKED";
        }

        public IEnumerator CloseAndPack()
        {
            Seal();
            if (_flaps != null)
            {
                float t = 0f;
                while (t < 0.38f)
                {
                    t += Time.deltaTime;
                    float u = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.38f));
                    for (int i = 0; i < _flaps.Length; i++)
                    {
                        if (_flaps[i] == null) continue;
                        float delay = i * 0.08f;
                        float fu = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((u * 1.15f) - delay));
                        _flaps[i].localRotation = Quaternion.Slerp(
                            Quaternion.Euler(_flapOpen[i]),
                            Quaternion.Euler(_flapClosed[i]),
                            fu);
                    }
                    yield return null;
                }
            }

            if (_tape != null)
            {
                _tape.gameObject.SetActive(true);
                var from = Vector3.Scale(_tapeScale, new Vector3(0.12f, 1f, 0.35f));
                float t = 0f;
                while (t < 0.18f && _tape != null)
                {
                    t += Time.deltaTime;
                    float u = Mathf.SmoothStep(0f, 1f, t / 0.18f);
                    _tape.localScale = Vector3.Lerp(from, _tapeScale, u);
                    yield return null;
                }
                if (_tape != null) _tape.localScale = _tapeScale;
            }

            yield return Tweens.PunchScale(transform, 0.12f, 0.2f);
        }

        public void ResetEmpty()
        {
            Fill = 0;
            HasHue = false;
            Active = true;
            Locked = false;
            if (_flaps != null)
            {
                for (int i = 0; i < _flaps.Length; i++)
                {
                    if (_flaps[i] != null)
                        _flaps[i].localEulerAngles = _flapOpen[i];
                }
            }
            if (_tape != null) _tape.gameObject.SetActive(false);
            if (_lockBadge != null) _lockBadge.SetActive(false);
            ApplyColor();
            RefreshLabel();
        }

        public void StopBillboard() { }

        void ApplyColor()
        {
            if (_liner != null)
            {
                var mr = _liner.GetComponent<MeshRenderer>();
                Color liner = Palette.Hex("FFF8E1");
                if (Locked) liner = Palette.Hex("B0BEC5");
                else if (HasHue) liner = Color.Lerp(Color.white, Palette.Of(Hue), 0.55f);
                if (mr != null) mr.sharedMaterial = MeshFactory.Lit(liner, 0.18f);
            }

            if (_cardboard == null) return;
            Color tint = Color.white;
            if (Locked) tint = Palette.Hex("CFD8DC");
            else if (HasHue) tint = Color.Lerp(Color.white, Palette.Of(Hue), 0.18f);
            var card = SpriteFactory.CardboardTex();
            var faces = new[] { Palette.Hex("E0C09A"), Palette.Hex("8D6E4F"), Palette.Hex("C4A074"), Palette.Hex("C4A074") };
            for (int i = 0; i < _cardboard.Length; i++)
            {
                if (_cardboard[i] == null) continue;
                _cardboard[i].sharedMaterial = MeshFactory.Textured(card, faces[i] * tint, 0.14f);
            }
        }

        void RefreshLabel()
        {
            if (_label == null) return;
            if (Locked) _label.text = "LOCKED";
            else if (!HasHue) _label.text = "EMPTY";
            else _label.text = $"{Fill}/{Capacity}";
        }

        void LateUpdate()
        {
            var cam = Camera.main;
            if (cam == null || _label == null) return;
            var dir = _label.transform.position - cam.transform.position;
            if (dir.sqrMagnitude > 0.001f)
                _label.transform.rotation = cam.transform.rotation;
        }
    }
}
