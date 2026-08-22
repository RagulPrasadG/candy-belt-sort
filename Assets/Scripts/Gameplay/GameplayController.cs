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
        readonly List<SpawnSpec> _spawns = new List<SpawnSpec>();
        readonly List<SpawnSpec>[] _laneQueues = { new List<SpawnSpec>(), new List<SpawnSpec>() };
        readonly int[] _laneSpawnAt = new int[2];
        readonly Stack<UndoRecord> _undos = new Stack<UndoRecord>();
        CandyItem _selected;
        int _nextLane;
        float _spawnTimer;
        int _freeUndos = 2;
        bool _extraSlotUsed;
        bool _resolving;
        int _busy;
        float _deadlockTimer;

        const float BombBlast = 1.35f;
        const float DeadlockGrace = 0.7f;

        struct UndoRecord
        {
            public SpawnSpec Spec;
            public float Distance;
            public int Lane;
        }

        public void EnsureArena()
        {
            if (_arena == null)
            {
                var go = new GameObject("FactoryArena");
                go.transform.SetParent(transform, false);
                _arena = go.AddComponent<FactoryArena>();
            }

            if (_input == null)
            {
                _input = gameObject.GetComponent<TapInput>();
                if (_input == null) _input = gameObject.AddComponent<TapInput>();
            }

            _input.OnCandyTapped = HandleTap;
            _input.OnBoxTapped = HandleBoxTap;
        }

        public void StartLevel(int index)
        {
            EnsureArena();
            _arena.gameObject.SetActive(true);
            Level = LevelCatalog.Get(index);
            var world = WorldCatalog.Get(Level.WorldIndex);
            _arena.Build(world, Level.LaneCount);
            foreach (var belt in _arena.Belts)
                belt.Setup(Level);
            LevelSequencer.Build(Level, _spawns);
            SplitLaneQueues();
            _arena.Rack.Build(Level);
            _input.SetCamera(_arena.Cam);
            _spawnTimer = 0.12f;
            _nextLane = 0;
            _selected = null;
            _freeUndos = index < 8 ? 3 : 2;
            _extraSlotUsed = false;
            _resolving = false;
            _busy = 0;
            _deadlockTimer = 0f;
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

            if (_arena.Belts != null)
            {
                for (int i = 0; i < _arena.Belts.Length; i++)
                {
                    var fallen = _arena.Belts[i].CandyReachedEnd();
                    if (fallen != null)
                    {
                        StartCoroutine(FailRoutine(fallen));
                        return;
                    }
                }
            }

            DeadlockTick();
        }

        // Detects the soft-lock the belt can reach: every candy has spawned, nothing is in
        // flight, and no legal move remains (fronts fit no open box and no empty slot exists).
        // Instead of freezing, we resolve it as a fail so the player can retry or continue.
        void DeadlockTick()
        {
            if (_busy > 0 || !SpawnsDone() || AnyPending())
            {
                _deadlockTimer = 0f;
                return;
            }

            if (LegalMoveExists())
            {
                _deadlockTimer = 0f;
                return;
            }

            _deadlockTimer += Time.deltaTime;
            if (_deadlockTimer >= DeadlockGrace)
            {
                _deadlockTimer = 0f;
                StartCoroutine(DeadlockRoutine());
            }
        }

        bool SpawnsDone()
        {
            for (int i = 0; i < _laneQueues.Length; i++)
            {
                if (_laneSpawnAt[i] < _laneQueues[i].Count) return false;
            }
            return true;
        }

        // True while any candy is still emerging or in a collect/explode animation.
        bool AnyPending()
        {
            if (_arena == null || _arena.Belts == null) return true;
            foreach (var belt in _arena.Belts)
            {
                if (belt == null) continue;
                foreach (var c in belt.Candies)
                {
                    if (c != null && (c.Emerging || c.Collecting)) return true;
                }
            }
            return false;
        }

        bool LegalMoveExists()
        {
            if (_arena == null || _arena.Belts == null || _arena.Rack == null) return true;

            bool anyFront = false;
            for (int i = 0; i < _arena.Belts.Length; i++)
            {
                var front = _arena.Belts[i].FrontCandy();
                if (front == null) continue;
                anyFront = true;
                // Bombs can always be cleared, hidden can be revealed, frozen will thaw/advance.
                if (front.Bomb || front.Hidden || front.Frozen) return true;
                if (_arena.Rack.TryMatching(front.Hue) != null) return true;
            }

            // Any empty unlocked slot can accept a fresh colour for a waiting front candy.
            if (anyFront && _arena.Rack.HasEmptySlot()) return true;
            return false;
        }

        IEnumerator DeadlockRoutine()
        {
            _resolving = true;
            Playing = false;
            ClearSelection();
            Sfx.Fail();
            Haptics.Light();
            yield return new WaitForSeconds(0.35f);
            GameFlow.I.HandleFail(Level.Index);
        }

        void SplitLaneQueues()
        {
            for (int i = 0; i < _laneQueues.Length; i++)
            {
                _laneQueues[i].Clear();
                _laneSpawnAt[i] = 0;
            }

            int lanes = Mathf.Max(1, Level.LaneCount);
            for (int i = 0; i < _spawns.Count; i++)
            {
                int lane = Mathf.Clamp(_spawns[i].Lane, 0, lanes - 1);
                _laneQueues[lane].Add(_spawns[i]);
            }
        }

        void SpawnTick()
        {
            if (_arena.Belts == null) return;
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer > 0f) return;

            int lanes = _arena.Belts.Length;
            for (int n = 0; n < lanes; n++)
            {
                int lane = (_nextLane + n) % lanes;
                if (_laneSpawnAt[lane] >= _laneQueues[lane].Count) continue;
                var belt = _arena.Belts[lane];
                if (belt.SpawnBlocked()) continue;

                var spec = _laneQueues[lane][_laneSpawnAt[lane]++];
                var candy = CreateCandy(spec);
                candy.Lane = lane;
                belt.Add(candy);
                var puffColor = spec.Bomb ? Palette.Bomb : Palette.Of(spec.Color);
                SpawnPuff(candy.transform.position + new Vector3(0f, 0.2f, 0.15f), puffColor);
                _nextLane = (lane + 1) % lanes;
                _spawnTimer = Level.SpawnInterval;
                return;
            }
        }

        CandyItem CreateCandy(SpawnSpec spec)
        {
            var go = new GameObject(spec.Bomb ? "Bomb" : "Candy");
            go.transform.SetParent(_arena.CandyRoot, false);
            var candy = go.AddComponent<CandyItem>();
            candy.Build(spec);
            candy.Lane = spec.Lane;
            return candy;
        }

        void ThawTick()
        {
            if (_arena.Belts == null) return;
            foreach (var belt in _arena.Belts)
            {
                foreach (var candy in belt.Candies)
                {
                    if (candy != null && candy.Frozen && candy.Distance > 1.6f)
                        candy.Thaw();
                }
            }
        }

        ConveyorBelt BeltOf(CandyItem candy)
        {
            if (candy == null || _arena.Belts == null) return null;
            return _arena.BeltAt(candy.Lane);
        }

        bool IsFront(CandyItem candy)
        {
            var belt = BeltOf(candy);
            return belt != null && belt.FrontCandy() == candy;
        }

        void SelectCandy(CandyItem candy)
        {
            if (_selected != null && _selected != candy)
                _selected.SetSelected(false);
            _selected = candy;
            if (_selected != null)
                _selected.SetSelected(true);
        }

        void ClearSelection()
        {
            if (_selected != null)
                _selected.SetSelected(false);
            _selected = null;
        }

        List<CandyItem> ReadyFronts(SortBox box)
        {
            var list = new List<CandyItem>(2);
            if (_arena.Belts == null || box == null) return list;
            for (int i = 0; i < _arena.Belts.Length; i++)
            {
                var front = _arena.Belts[i].FrontCandy();
                if (front == null || front.Collecting) continue;
                if (front.Bomb || front.Hidden || front.Frozen) continue;
                if (box.CanAccept(front.Hue))
                    list.Add(front);
            }

            return list;
        }

        void HandleTap(CandyItem candy)
        {
            if (!Playing || _resolving || candy == null || candy.Collecting) return;

            if (!IsFront(candy))
            {
                Sfx.Reject();
                StartCoroutine(Tweens.Shake(candy.transform, 0.06f, 0.12f));
                return;
            }

            if (candy.Bomb)
            {
                StartCoroutine(ExplodeBomb(candy));
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

            var match = _arena.Rack.TryMatching(candy.Hue);
            bool dual = Level != null && Level.LaneCount > 1;
            if (!dual && match != null)
            {
                StartCoroutine(Collect(candy, match));
                return;
            }

            if (_selected == candy && match != null)
            {
                StartCoroutine(Collect(candy, match));
                return;
            }

            SelectCandy(candy);
            Sfx.Tap();
            Haptics.Light();
        }

        void HandleBoxTap(SortBox box)
        {
            if (!Playing || _resolving || box == null) return;

            if (_selected != null && IsFront(_selected) && !_selected.Collecting
                && !_selected.Bomb && !_selected.Hidden && !_selected.Frozen
                && box.CanAccept(_selected.Hue))
            {
                StartCoroutine(Collect(_selected, box));
                return;
            }

            var fits = ReadyFronts(box);
            if (fits.Count == 1)
            {
                StartCoroutine(Collect(fits[0], box));
                return;
            }

            Sfx.Reject();
            StartCoroutine(Tweens.Shake(box.transform, 0.1f, 0.2f));
        }

        IEnumerator Collect(CandyItem candy, SortBox box)
        {
            _busy++;
            try
            {
                yield return CollectRoutine(candy, box);
            }
            finally
            {
                _busy = Mathf.Max(0, _busy - 1);
            }
        }

        IEnumerator CollectRoutine(CandyItem candy, SortBox box)
        {
            candy.Collecting = true;
            if (_selected == candy) ClearSelection();
            var belt = BeltOf(candy);
            if (belt != null) belt.Remove(candy);
            _undos.Push(new UndoRecord
            {
                Spec = new SpawnSpec { Color = candy.Hue, Frozen = false, Hidden = false, Bomb = false, Lane = candy.Lane },
                Distance = Mathf.Max(0.4f, candy.Distance - 0.6f),
                Lane = candy.Lane
            });

            Sfx.Tap();
            Haptics.Light();
            var from = candy.transform.position;
            var to = _arena.Rack.SlotWorld(box);
            yield return Tweens.Arc(candy.transform, from, to, 1.4f, 0.28f);
            if (candy == null || box == null) yield break;
            var color = candy.Hue;
            Destroy(candy.gameObject);
            box.AddOne(color);
            Sfx.Box();
            SpawnConfetti(to, Palette.Of(color));
            HudView.I.Refresh();

            if (box.IsFull)
            {
                Sfx.Seal();
                yield return _arena.Rack.ShipFilled(box);
                HudView.I.Refresh();
                if (_arena.Rack.Completed >= Level.QuotaBoxes)
                {
                    yield return WinRoutine();
                    yield break;
                }
            }
            else
            {
                StartCoroutine(Tweens.PunchScale(box.transform, 0.12f, 0.18f));
            }
        }

        // Tapping a bomb detonates it: it clears every candy within the blast radius on its
        // belt (a strategic way to bust a jam — but it will take out good candies too).
        IEnumerator ExplodeBomb(CandyItem bomb)
        {
            _busy++;
            try
            {
                bomb.Collecting = true;
                if (_selected == bomb) ClearSelection();
                int lane = bomb.Lane;
                float at = bomb.Distance;
                var center = bomb.transform.position;

                var belt = _arena.BeltAt(lane);
                if (belt != null) belt.Remove(bomb);

                Sfx.Bomb();
                Haptics.Light();
                SpawnExplosion(center);
                if (_arena.Cam != null)
                    StartCoroutine(Tweens.Shake(_arena.Cam.transform, 0.14f, 0.24f));

                var caught = new List<CandyItem>();
                if (belt != null)
                {
                    foreach (var c in belt.Candies)
                    {
                        if (c == null || c == bomb || c.Collecting) continue;
                        if (Mathf.Abs(c.Distance - at) <= BombBlast)
                            caught.Add(c);
                    }
                }

                if (bomb != null) Destroy(bomb.gameObject);

                for (int i = 0; i < caught.Count; i++)
                {
                    var c = caught[i];
                    if (c == null) continue;
                    var pos = c.transform.position;
                    var col = c.Bomb ? Palette.Bomb : Palette.Of(c.Hue);
                    if (belt != null) belt.Remove(c);
                    c.Collecting = true;
                    SpawnPuff(pos + new Vector3(0f, 0.2f, 0.1f), col);
                    Destroy(c.gameObject);
                }

                HudView.I.Refresh();
                yield return new WaitForSeconds(0.06f);
            }
            finally
            {
                _busy = Mathf.Max(0, _busy - 1);
            }
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
            if (_selected == fallen) ClearSelection();
            var belt = BeltOf(fallen);
            if (belt != null) belt.Remove(fallen);
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
            candy.Lane = record.Lane;
            var belt = _arena.BeltAt(record.Lane);
            if (belt != null) belt.InsertAsFront(candy);
            SelectCandy(candy);
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
            if (_arena.Belts != null)
            {
                foreach (var belt in _arena.Belts)
                    belt.Frozen = true;
            }

            yield return new WaitForSeconds(seconds);
            if (_arena != null && _arena.Belts != null)
            {
                foreach (var belt in _arena.Belts)
                    belt.Frozen = false;
            }
        }

        static void SpawnPuff(Vector3 pos, Color color)
        {
            var go = new GameObject("SpawnPuff");
            go.transform.position = pos;
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.45f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.6f, 1.6f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.1f);
            main.startColor = new ParticleSystem.MinMaxGradient(Color.white, color);
            main.maxParticles = 18;
            main.gravityModifier = 0.4f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 12) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.12f;
            ps.Play();
            Object.Destroy(go, 1.1f);
        }

        static void SpawnExplosion(Vector3 pos)
        {
            var go = new GameObject("BombBlast");
            go.transform.position = pos + Vector3.up * 0.1f;
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(3.5f, 7.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.34f);
            main.startColor = new ParticleSystem.MinMaxGradient(Palette.Hex("FFE082"), Palette.Hex("FF5252"));
            main.maxParticles = 80;
            main.gravityModifier = 0.6f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 48) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.18f;
            var colorLife = ps.colorOverLifetime;
            colorLife.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Palette.Hex("FF8A65"), 0.4f), new GradientColorKey(Palette.Hex("616161"), 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.9f, 0.5f), new GradientAlphaKey(0f, 1f) });
            colorLife.color = gradient;
            var sizeLife = ps.sizeOverLifetime;
            sizeLife.enabled = true;
            sizeLife.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.4f, 1f, 1.4f));
            ps.Play();
            Object.Destroy(go, 1.4f);
        }

        static void SpawnConfetti(Vector3 pos, Color color)
        {
            var go = new GameObject("Confetti");
            go.transform.position = pos + Vector3.up * 0.2f;
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.6f, 5.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.16f);
            main.startColor = new ParticleSystem.MinMaxGradient(color, Color.Lerp(color, Color.white, 0.75f));
            main.maxParticles = 56;
            main.gravityModifier = 1.8f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 36) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 42f;
            shape.radius = 0.14f;
            shape.rotation = new Vector3(-90f, 0f, 0f);
            var colorLife = ps.colorOverLifetime;
            colorLife.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(color, 0.35f), new GradientColorKey(Color.Lerp(color, Palette.Lemon, 0.4f), 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.85f, 0.45f), new GradientAlphaKey(0f, 1f) });
            colorLife.color = gradient;
            var sizeLife = ps.sizeOverLifetime;
            sizeLife.enabled = true;
            sizeLife.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.15f));
            ps.Play();
            Destroy(go, 1.8f);
        }
    }
}
