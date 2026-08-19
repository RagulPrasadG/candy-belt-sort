using System.Collections.Generic;
using UnityEngine;

namespace CandyBeltSort
{
    public class ConveyorBelt : MonoBehaviour
    {
        public float StartZ = 7.2f;
        public float EndZ = -1.25f;
        public float Height = 0.9f;
        public float Speed = 1.2f;
        public bool Frozen;

        readonly List<CandyItem> _candies = new List<CandyItem>();
        public IReadOnlyList<CandyItem> Candies => _candies;
        public float Length => StartZ - EndZ;

        public void Setup(LevelDefinition level)
        {
            Speed = level.BeltSpeed;
            Frozen = false;
        }

        public void Add(CandyItem candy)
        {
            candy.Distance = 0f;
            candy.OnBelt = true;
            candy.transform.position = PointAt(0f);
            _candies.Add(candy);
        }

        public void Remove(CandyItem candy)
        {
            candy.OnBelt = false;
            _candies.Remove(candy);
        }

        public bool SpawnBlocked()
        {
            for (int i = 0; i < _candies.Count; i++)
            {
                if (_candies[i].OnBelt && _candies[i].Distance < 0.7f)
                    return true;
            }
            return false;
        }

        public Vector3 PointAt(float distance)
        {
            float z = Mathf.Lerp(StartZ, EndZ, Mathf.Clamp01(distance / Length));
            return new Vector3(0f, Height, z);
        }

        public CandyItem CandyReachedEnd()
        {
            for (int i = 0; i < _candies.Count; i++)
            {
                var c = _candies[i];
                if (c != null && c.OnBelt && !c.Collecting && c.Distance >= Length)
                    return c;
            }
            return null;
        }

        void Update()
        {
            if (Frozen) return;
            float dt = Time.deltaTime * Speed;
            for (int i = 0; i < _candies.Count; i++)
            {
                var c = _candies[i];
                if (c == null || !c.OnBelt || c.Collecting) continue;
                c.Distance += dt;
                c.transform.position = PointAt(c.Distance);
                c.transform.Rotate(Vector3.right, dt * 80f, Space.World);
            }
        }
    }
}
