using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyBeltSort
{
    public class BoxRack : MonoBehaviour
    {
        readonly List<SortBox> _slots = new List<SortBox>();
        int _lockedRemaining;
        LevelDefinition _level;

        public int Completed { get; private set; }
        public int Quota { get; private set; }

        int _laneCount = 1;

        public void Build(LevelDefinition level)
        {
            _level = level;
            _laneCount = Mathf.Max(1, level.LaneCount);
            foreach (var s in _slots)
            {
                if (s != null) Destroy(s.gameObject);
            }
            _slots.Clear();
            Completed = 0;
            Quota = level.QuotaBoxes;
            _lockedRemaining = level.LockedBoxCount;

            int slots = Mathf.Max(1, level.OpenSlots);
            for (int i = 0; i < slots; i++)
                CreateSlot();

            Layout();

            if (_lockedRemaining > 0 && _slots.Count > 0)
                _slots[_slots.Count - 1].SetLocked(true);
        }

        public void UnlockExtraSlot()
        {
            if (_slots.Count >= 4) return;
            CreateSlot();
            Layout();
        }

        public SortBox TryMatching(CandyColor color)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];
                if (slot != null && slot.HasHue && slot.CanAccept(color))
                    return slot;
            }

            return null;
        }

        public IEnumerator ShipFilled(SortBox box)
        {
            if (box == null) yield break;
            int index = _slots.IndexOf(box);
            box.Seal();
            Completed++;
            if (_lockedRemaining > 0)
            {
                foreach (var slot in _slots)
                {
                    if (slot == null || !slot.Locked) continue;
                    slot.SetLocked(false);
                    _lockedRemaining--;
                    break;
                }
            }

            var col = box.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            yield return box.CloseAndPack();
            if (box == null) yield break;

            var home = box.transform.localPosition;
            yield return Tweens.Shake(box.transform, 0.1f, 0.18f);
            if (box == null) yield break;
            box.transform.localPosition = home;
            box.StopBillboard();
            box.transform.SetParent(null, true);

            if (index >= 0)
                _slots[index] = null;

            var arena = GetComponentInParent<FactoryArena>();
            var from = box.transform.position;
            var dock = arena != null ? arena.ShipDock : from + new Vector3(6.5f, 0.8f, -0.5f);
            var startScale = box.transform.localScale;
            float elapsed = 0f;
            const float shipDuration = 0.62f;
            while (elapsed < shipDuration && box != null)
            {
                elapsed += Time.deltaTime;
                float u = Mathf.SmoothStep(0f, 1f, elapsed / shipDuration);
                box.transform.position = Vector3.Lerp(from, dock, u);
                box.transform.localScale = Vector3.Lerp(startScale, startScale * 0.45f, u);
                yield return null;
            }

            if (box != null)
                Destroy(box.gameObject);

            if (Completed >= Quota || index < 0) yield break;

            var incoming = MakeBox();
            incoming.transform.localPosition = home + new Vector3(0f, 1.8f, 0.9f);
            incoming.transform.localScale = Vector3.one * 0.55f;
            _slots[index] = incoming;

            elapsed = 0f;
            const float arriveDuration = 0.3f;
            var startPos = incoming.transform.localPosition;
            while (elapsed < arriveDuration && incoming != null)
            {
                elapsed += Time.deltaTime;
                float u = Mathf.SmoothStep(0f, 1f, elapsed / arriveDuration);
                incoming.transform.localPosition = Vector3.Lerp(startPos, home, u);
                incoming.transform.localScale = Vector3.Lerp(Vector3.one * 0.55f, Vector3.one, u);
                yield return null;
            }

            if (incoming != null)
            {
                incoming.transform.localPosition = home;
                incoming.transform.localScale = Vector3.one;
            }
        }

        public Vector3 SlotWorld(SortBox box) => box.transform.position + Vector3.up * 0.55f;

        void CreateSlot()
        {
            _slots.Add(MakeBox());
        }

        SortBox MakeBox()
        {
            var go = new GameObject($"Box_{_slots.Count}");
            var box = go.AddComponent<SortBox>();
            box.Build(transform, Vector3.zero, _level.BoxCapacity);
            box.ResetEmpty();
            return box;
        }

        void Layout()
        {
            int n = _slots.Count;
            for (int i = 0; i < n; i++)
            {
                if (_slots[i] == null) continue;
                _slots[i].transform.localPosition = SlotLocal(n, i, _laneCount);
            }
        }

        static Vector3 SlotLocal(int n, int i, int lanes)
        {
            float x = lanes >= 2 ? 3.2f : 2.45f;
            if (n <= 2)
                return new Vector3(i == 0 ? -x : x, 0f, 1.15f);
            if (n == 3)
            {
                if (i == 0) return new Vector3(-x, 0f, 1.85f);
                if (i == 1) return new Vector3(x, 0f, 1.85f);
                return new Vector3(-x, 0f, -0.15f);
            }

            float sx = (i % 2 == 0) ? -x : x;
            float z = i < 2 ? 2.15f : 0.35f;
            return new Vector3(sx, 0f, z);
        }
    }
}
