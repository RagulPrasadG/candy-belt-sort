using System.Collections.Generic;
using UnityEngine;

namespace CandyBeltSort
{
    public class ConveyorBelt : MonoBehaviour
    {
        public float StartZ = 7.32f;
        public float EndZ = -1.35f;
        public float Height = 0.5f;
        public float Speed = 1.2f;
        public bool Frozen;
        public int Lane;

        readonly List<CandyItem> _candies = new List<CandyItem>();
        Transform _rollers;
        Transform _slats;
        public IReadOnlyList<CandyItem> Candies => _candies;
        public float Length => StartZ - EndZ;

        public void BindMotion(Transform rollers, Transform slats)
        {
            _rollers = rollers;
            _slats = slats;
        }

        public void Setup(LevelDefinition level)
        {
            Speed = level.BeltSpeed;
            Frozen = false;
        }

        public void Add(CandyItem candy, float distance = 0f)
        {
            candy.Distance = distance;
            candy.OnBelt = true;
            candy.Lane = Lane;
            candy.transform.position = PointAt(candy.Distance);
            _candies.Add(candy);
            if (distance <= 0.05f)
                candy.BeginEmerge();
            RefreshTakeable();
        }

        public void InsertAsFront(CandyItem candy)
        {
            float max = 0f;
            for (int i = 0; i < _candies.Count; i++)
            {
                var c = _candies[i];
                if (c != null && c.OnBelt && !c.Collecting && c.Distance > max)
                    max = c.Distance;
            }

            Add(candy, Mathf.Min(Length - 0.12f, max + 0.95f));
        }

        public void Remove(CandyItem candy)
        {
            candy.OnBelt = false;
            candy.SetTakeable(false);
            candy.SetSelected(false);
            _candies.Remove(candy);
            RefreshTakeable();
        }

        public CandyItem FrontCandy()
        {
            CandyItem front = null;
            float best = -1f;
            for (int i = 0; i < _candies.Count; i++)
            {
                var c = _candies[i];
                if (c == null || !c.OnBelt || c.Collecting || c.Emerging) continue;
                if (c.Distance >= best)
                {
                    best = c.Distance;
                    front = c;
                }
            }

            return front;
        }

        public void RefreshTakeable()
        {
            var front = FrontCandy();
            for (int i = 0; i < _candies.Count; i++)
            {
                var c = _candies[i];
                if (c == null || !c.OnBelt || c.Collecting) continue;
                c.SetTakeable(c == front);
            }
        }

        public bool SpawnBlocked()
        {
            for (int i = 0; i < _candies.Count; i++)
            {
                if (_candies[i].OnBelt && _candies[i].Distance < 0.92f)
                    return true;
            }
            return false;
        }

        public Vector3 PointAt(float distance)
        {
            float z = Mathf.Lerp(StartZ, EndZ, Mathf.Clamp01(distance / Length));
            return transform.TransformPoint(new Vector3(0f, Height, z));
        }

        public CandyItem CandyReachedEnd()
        {
            for (int i = 0; i < _candies.Count; i++)
            {
                var c = _candies[i];
                if (c != null && c.OnBelt && !c.Collecting && !c.Emerging && c.Distance >= Length)
                    return c;
            }
            return null;
        }

        void Update()
        {
            if (!Frozen)
            {
                float dt = Time.deltaTime * Speed;
                for (int i = 0; i < _candies.Count; i++)
                {
                    var c = _candies[i];
                    if (c == null || !c.OnBelt || c.Collecting) continue;
                    if (c.Emerging) continue;
                    c.Distance += dt;
                    c.transform.position = PointAt(c.Distance);
                }

                SpinVisuals(dt);
            }
            RefreshTakeable();
        }

        void SpinVisuals(float dt)
        {
            if (_rollers != null)
            {
                float spin = Speed * 140f * Time.deltaTime;
                for (int i = 0; i < _rollers.childCount; i++)
                    _rollers.GetChild(i).Rotate(spin, 0f, 0f, Space.Self);
            }

            if (_slats == null) return;
            float span = StartZ - EndZ - 0.4f;
            for (int i = 0; i < _slats.childCount; i++)
            {
                var slat = _slats.GetChild(i);
                var p = slat.localPosition;
                p.z -= dt;
                if (p.z < EndZ + 0.35f) p.z += span;
                slat.localPosition = p;
            }
        }
    }
}
