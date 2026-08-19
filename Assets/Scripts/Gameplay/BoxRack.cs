using System.Collections.Generic;
using UnityEngine;

namespace CandyBeltSort
{
    public class BoxRack : MonoBehaviour
    {
        readonly List<SortBox> _slots = new List<SortBox>();
        readonly Queue<CandyColor> _queue = new Queue<CandyColor>();
        int _lockedRemaining;
        Transform _parent;
        LevelDefinition _level;

        public int Completed { get; private set; }
        public int Quota { get; private set; }

        public void Build(Transform parent, LevelDefinition level, IList<CandyColor> boxQueue)
        {
            _parent = parent;
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

            int slots = level.OpenSlots;
            for (int i = 0; i < slots; i++)
                CreateSlot();

            Layout();
            AssignWaitingSlots();

            if (_lockedRemaining > 0)
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
                if (_slots[i].CanAccept(color))
                    return _slots[i];
            }
            return null;
        }

        public void NotifyFilled(SortBox box)
        {
            box.Seal();
            Completed++;
            if (_lockedRemaining > 0)
            {
                foreach (var slot in _slots)
                {
                    if (!slot.Locked) continue;
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
            box.Build(_parent, Vector3.zero, _level.BoxCapacity);
            box.Active = false;
            _slots.Add(box);
        }

        void Layout()
        {
            int slots = _slots.Count;
            float spacing = slots >= 4 ? 1.35f : 1.55f;
            float startX = -((slots - 1) * spacing) * 0.5f;
            for (int i = 0; i < slots; i++)
                _slots[i].transform.position = new Vector3(startX + i * spacing, 0f, -2.35f);
        }

        void AssignWaitingSlots()
        {
            foreach (var slot in _slots)
            {
                if (slot.Active) continue;
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
