using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyBeltSort
{
    public class GameplayController : MonoBehaviour
    {
        public LevelDefinition Level { get; private set; }
        public bool Playing { get; private set; }

        FactoryArena _arena;
        TapInput _input;
        readonly List<CandyColor> _boxQueue = new List<CandyColor>();
        readonly List<SpawnSpec> _spawns = new List<SpawnSpec>();
        readonly Stack<UndoRecord> _undos = new Stack<UndoRecord>();
        int _spawnIndex;
        float _spawnTimer;
        int _freeUndos = 2;
        bool _extraSlotUsed;
        bool _resolving;

        struct UndoRecord
        {
            public SpawnSpec Spec;
            public float Distance;
        }

        public void EnsureArena()
        {
            if (_arena != null) return;
            var go = new GameObject("FactoryArena");
            go.transform.SetParent(transform, false);
            _arena = go.AddComponent<FactoryArena>();
            _input = gameObject.AddComponent<TapInput>();
            _input.OnCandyTapped = HandleTap;
        }

        public void StartLevel(int index)
        {
            EnsureArena();
            _arena.gameObject.SetActive(true);
            Level = LevelCatalog.Get(index);
            var world = WorldCatalog.Get(Level.WorldIndex);
            _arena.Build(world);
            _arena.Belt.Setup(Level);
            LevelSequencer.Build(Level, _boxQueue, _spawns);
            _arena.Rack.Build(_arena.transform.GetChild(0), Level, _boxQueue);
            _input.SetCamera(_arena.Cam);
            _spawnIndex = 0;
            _spawnTimer = 0.35f;
            _freeUndos = index < 8 ? 3 : 2;
            _extraSlotUsed = false;
            _resolving = false;
            Playing = true;
            _undos.Clear();
            HudView.I.Show(this);
        }

        public void StopLevel()
        {
            Playing = false;
            if (_arena != null)
                _arena.gameObject.SetActive(false);
            HudView.I.Hide();
        }

        public int BoxesDone => _arena != null ? _arena.Rack.Completed : 0;
        public int Quota => Level != null ? Level.QuotaBoxes : 0;
        public int FreeUndos => _freeUndos;
        public bool ExtraSlotUsed => _extraSlotUsed;

        void Update()
        {
            if (!Playing || _resolving || _arena == null) return;

            SpawnTick();
            ThawTick();

            var fallen = _arena.Belt.CandyReachedEnd();
            if (fallen != null)
                StartCoroutine(FailRoutine(fallen));
        }

        void SpawnTick()
        {
            if (_spawnIndex >= _spawns.Count) return;
            if (_arena.Belt.SpawnBlocked()) return;
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer > 0f) return;
            _spawnTimer = Level.SpawnInterval;
            var spec = _spawns[_spawnIndex++];
            var candy = CreateCandy(spec);
            _arena.Belt.Add(candy);
        }

        CandyItem CreateCandy(SpawnSpec spec)
        {
            var go = new GameObject(spec.Bomb ? "Bomb" : "Candy");
            go.transform.SetParent(_arena.CandyRoot, false);
            var candy = go.AddComponent<CandyItem>();
            candy.Build(spec);
            return candy;
        }

        void ThawTick()
        {
            foreach (var candy in _arena.Belt.Candies)
            {
                if (candy != null && candy.Frozen && candy.Distance > 1.6f)
                    candy.Thaw();
            }
        }

        void HandleTap(CandyItem candy)
        {
            if (!Playing || _resolving || candy == null || candy.Collecting) return;

            if (candy.Bomb)
            {
                StartCoroutine(DiscardBomb(candy));
                return;
            }

            if (candy.Hidden)
            {
                candy.Reveal();
                Sfx.Tap();
                Haptics.Light();
                return;
            }

            if (candy.Frozen)
            {
                Sfx.Reject();
                StartCoroutine(Tweens.Shake(candy.transform, 0.08f, 0.18f));
                return;
            }

            var box = _arena.Rack.TryAccept(candy.Hue);
            if (box == null)
            {
                Sfx.Reject();
                StartCoroutine(Tweens.Shake(candy.transform, 0.1f, 0.2f));
                return;
            }

            StartCoroutine(Collect(candy, box));
        }

        IEnumerator Collect(CandyItem candy, SortBox box)
        {
            candy.Collecting = true;
            _arena.Belt.Remove(candy);
            _undos.Push(new UndoRecord
            {
                Spec = new SpawnSpec { Color = candy.Hue, Frozen = false, Hidden = false, Bomb = false },
                Distance = Mathf.Max(0.4f, candy.Distance - 0.6f)
            });

            Sfx.Tap();
            Haptics.Light();
            var from = candy.transform.position;
            var to = _arena.Rack.SlotWorld(box);
            yield return Tweens.Arc(candy.transform, from, to, 1.4f, 0.28f);
            if (candy != null) Destroy(candy.gameObject);

            box.AddOne();
            Sfx.Box();
            StartCoroutine(Tweens.PunchScale(box.transform, 0.12f, 0.18f));
            HudView.I.Refresh();

            if (box.IsFull)
            {
                _arena.Rack.NotifyFilled(box);
                Sfx.Seal();
                SpawnPuff(to, Palette.Of(box.Hue));
                HudView.I.Refresh();
                if (_arena.Rack.Completed >= Level.QuotaBoxes)
                {
                    yield return WinRoutine();
                    yield break;
                }
            }
        }

        IEnumerator DiscardBomb(CandyItem candy)
        {
            candy.Collecting = true;
            _arena.Belt.Remove(candy);
            Sfx.Bomb();
            Haptics.Light();
            var from = candy.transform.position;
            var to = from + new Vector3(2.4f, 0.6f, 0.2f);
            yield return Tweens.Arc(candy.transform, from, to, 1.1f, 0.22f);
            if (candy != null) Destroy(candy.gameObject);
        }

        IEnumerator WinRoutine()
        {
            _resolving = true;
            Playing = false;
            Sfx.Win();
            yield return new WaitForSeconds(0.45f);
            GameFlow.I.HandleWin(Level.Index);
        }

        IEnumerator FailRoutine(CandyItem fallen)
        {
            _resolving = true;
            Playing = false;
            _arena.Belt.Remove(fallen);
            Sfx.Fail();
            Haptics.Light();
            yield return Tweens.Shake(fallen.transform, 0.15f, 0.25f);
            if (fallen != null) Destroy(fallen.gameObject);
            GameFlow.I.HandleFail(Level.Index);
        }

        public void RequestUndo()
        {
            if (!Playing || _undos.Count == 0) return;
            if (_freeUndos > 0)
            {
                _freeUndos--;
                PerformUndo();
                return;
            }

            AdManager.I.ShowRewarded("undo", ok =>
            {
                if (ok) PerformUndo();
            });
        }

        void PerformUndo()
        {
            if (_undos.Count == 0) return;
            var record = _undos.Pop();
            var candy = CreateCandy(record.Spec);
            candy.Distance = record.Distance;
            _arena.Belt.Add(candy);
            candy.transform.position = _arena.Belt.PointAt(record.Distance);
            HudView.I.Refresh();
        }

        public void RequestExtraSlot()
        {
            if (!Playing || _extraSlotUsed) return;
            AdManager.I.ShowRewarded("extra_slot", ok =>
            {
                if (!ok) return;
                _extraSlotUsed = true;
                _arena.Rack.UnlockExtraSlot();
                HudView.I.Refresh();
            });
        }

        public void ContinueAfterFail()
        {
            _resolving = false;
            Playing = true;
            if (!_extraSlotUsed)
            {
                _extraSlotUsed = true;
                _arena.Rack.UnlockExtraSlot();
            }
            StartCoroutine(FreezeBelt(2.4f));
            HudView.I.Refresh();
        }

        IEnumerator FreezeBelt(float seconds)
        {
            _arena.Belt.Frozen = true;
            yield return new WaitForSeconds(seconds);
            if (_arena != null) _arena.Belt.Frozen = false;
        }

        static void SpawnPuff(Vector3 pos, Color color)
        {
            var go = new GameObject("Puff");
            go.transform.position = pos;
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.35f;
            main.startSpeed = 1.6f;
            main.startSize = 0.16f;
            main.startColor = color;
            main.maxParticles = 18;
            main.gravityModifier = 0.4f;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 14) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.15f;
            Destroy(go, 1.2f);
        }
    }
}
