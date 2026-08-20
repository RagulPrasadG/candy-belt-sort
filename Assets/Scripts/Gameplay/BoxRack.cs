using System.Collections.Generic;
using UnityEngine;

namespace CandyBeltSort
{
    public class BoxRack : MonoBehaviour
    {
        readonly List<SortBox> _slots = new List<SortBox>();
        readonly Queue<CandyColor> _queue = new Queue<CandyColor>();
        int _lockedRemaining;
        LevelDefinition _level;

        public int Completed { get; private set; }
        public int Quota { get; private set; }

        public void Build(LevelDefinition level, IList<CandyColor> boxQueue)
        {
            _level = level;
            foreach (var s in _slots)
            {
                if (s != null) Destroy(s.gameObject);
            }
            _slots.Clear();
            _queue.Clear();
            Completed = 0;
            Quota = level.QuotaBoxes;
            _lockedRemaining = level.LockedBoxCount;

            foreach (var c in boxQueue)
                _queue.Enqueue(c);

            int slots = Mathf.Max(1, level.OpenSlots);
            for (int i = 0; i < slots; i++)
                CreateSlot();

            Layout();
            AssignWaitingSlots();

            if (_lockedRemaining > 0 && _slots.Count > 0)
                _slots[_slots.Count - 1].SetLocked(true);
        }

        public void UnlockExtraSlot()
        {
            if (_slots.Count >= 4) return;
            CreateSlot();
            Layout();
            AssignWaitingSlots();
        }

        public SortBox TryAccept(CandyColor color)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i] != null && _slots[i].CanAccept(color))
                    return _slots[i];
            }
            return null;
        }

        public void NotifyFilled(SortBox box)
        {
            if (box == null) return;
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

            RecycleSlot(box);
        }

        public Vector3 SlotWorld(SortBox box) => box.transform.position + Vector3.up * 0.55f;

        void CreateSlot()
        {
            var go = new GameObject($"Box_{_slots.Count}");
            var box = go.AddComponent<SortBox>();
            box.Build(transform, Vector3.zero, _level.BoxCapacity);
            box.Active = false;
            _slots.Add(box);
        }

        void Layout()
        {
            int n = _slots.Count;
            for (int i = 0; i < n; i++)
            {
                if (_slots[i] == null) continue;
                Vector3 pos;
                if (n <= 2)
                    pos = new Vector3(i == 0 ? -2.45f : 2.45f, 0f, 1.15f);
                else if (n == 3)
                {
                    if (i == 0) pos = new Vector3(-2.55f, 0f, 1.85f);
                    else if (i == 1) pos = new Vector3(2.55f, 0f, 1.85f);
                    else pos = new Vector3(0f, 0f, -0.45f);
                }
                else
                {
                    float x = (i % 2 == 0) ? -2.55f : 2.55f;
                    float z = i < 2 ? 2.15f : 0.35f;
                    pos = new Vector3(x, 0f, z);
                }

                _slots[i].transform.localPosition = pos;
            }
        }

        void AssignWaitingSlots()
        {
            foreach (var slot in _slots)
            {
                if (slot == null || slot.Active) continue;
                if (_queue.Count == 0) return;
                slot.ResetEmpty();
                slot.SetColor(_queue.Dequeue());
            }
        }

        void RecycleSlot(SortBox box)
        {
            if (_queue.Count == 0)
            {
                box.Active = false;
                return;
            }

            box.ResetEmpty();
            box.SetColor(_queue.Dequeue());
        }
    }
}
