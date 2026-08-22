using System.Collections.Generic;
using UnityEngine;

namespace CandyBeltSort
{
    public class SpawnSpec
    {
        public CandyColor Color;
        public bool Frozen;
        public bool Hidden;
        public bool Bomb;
        public int Lane;
    }

    public static class LevelSequencer
    {
        public static void Build(LevelDefinition level, List<SpawnSpec> candies)
        {
            candies.Clear();
            var rng = new System.Random(level.Seed);
            int palette = Mathf.Clamp(level.ColorCount, 2, 5);
            int capacity = Mathf.Max(1, level.BoxCapacity);
            int liveSlots = Mathf.Max(1, level.OpenSlots - Mathf.Min(1, level.LockedBoxCount));

            var groups = new List<CandyColor>(level.QuotaBoxes);
            for (int i = 0; i < level.QuotaBoxes; i++)
                groups.Add((CandyColor)(i % palette));

            var need = new int[5];
            foreach (var color in groups)
                need[(int)color] += capacity;

            var active = new Dictionary<CandyColor, int>();
            bool fillOpenBoxFirst = level.Index == 0;
            CandyColor? last = null;
            int guard = level.QuotaBoxes * capacity * 4 + 8;
            while (Remaining(need) > 0 && guard-- > 0)
            {
                var legal = new List<CandyColor>(5);
                if (fillOpenBoxFirst)
                {
                    for (int c = 0; c < palette; c++)
                    {
                        if (need[c] <= 0) continue;
                        var color = (CandyColor)c;
                        if (active.ContainsKey(color))
                            legal.Add(color);
                    }
                }
                else if (active.Count < liveSlots)
                {
                    for (int c = 0; c < palette; c++)
                    {
                        if (need[c] <= 0) continue;
                        var color = (CandyColor)c;
                        if (!active.ContainsKey(color))
                            legal.Add(color);
                    }
                }

                if (legal.Count == 0)
                {
                    for (int c = 0; c < palette; c++)
                    {
                        if (need[c] <= 0) continue;
                        var color = (CandyColor)c;
                        if (active.ContainsKey(color) || active.Count < liveSlots)
                            legal.Add(color);
                    }
                }

                if (legal.Count == 0) break;
                if (last.HasValue && legal.Count > 1 && legal.Contains(last.Value) && rng.NextDouble() < 0.65)
                    legal.Remove(last.Value);

                var pick = legal[rng.Next(legal.Count)];
                last = pick;
                candies.Add(new SpawnSpec { Color = pick });
                need[(int)pick]--;

                if (active.TryGetValue(pick, out int left))
                {
                    left--;
                    if (left <= 0) active.Remove(pick);
                    else active[pick] = left;
                }
                else
                    active[pick] = capacity - 1;
            }

            foreach (var spec in candies)
            {
                if (rng.NextDouble() < level.FrozenChance)
                    spec.Frozen = true;
                if (rng.NextDouble() < level.HiddenChance)
                    spec.Hidden = true;
            }

            if (level.BombChance > 0f)
            {
                var withBombs = new List<SpawnSpec>(candies.Count + 8);
                foreach (var spec in candies)
                {
                    withBombs.Add(spec);
                    if (rng.NextDouble() < level.BombChance)
                    {
                        withBombs.Add(new SpawnSpec
                        {
                            Color = spec.Color,
                            Bomb = true
                        });
                    }
                }
                candies.Clear();
                candies.AddRange(withBombs);
            }

            DealLanes(level, candies);
        }

        static void DealLanes(LevelDefinition level, List<SpawnSpec> candies)
        {
            int lanes = Mathf.Max(1, level.LaneCount);
            if (lanes <= 1)
            {
                for (int i = 0; i < candies.Count; i++)
                    candies[i].Lane = 0;
                return;
            }

            var counts = new int[lanes];
            int prefer = 0;
            for (int i = 0; i < candies.Count; i++)
            {
                int lane = prefer;
                for (int l = 0; l < lanes; l++)
                {
                    if (counts[l] < counts[lane])
                        lane = l;
                }

                candies[i].Lane = lane;
                counts[lane]++;
                prefer = (prefer + 1) % lanes;
            }
        }

        public static bool IsGreedySolvable(LevelDefinition level, List<SpawnSpec> candies)
        {
            int capacity = Mathf.Max(1, level.BoxCapacity);
            int slots = Mathf.Max(1, level.OpenSlots);
            var fill = new int[slots];
            var hue = new CandyColor[slots];
            var hasHue = new bool[slots];
            var locked = new bool[slots];
            int lockedLeft = level.LockedBoxCount;
            if (lockedLeft > 0) locked[slots - 1] = true;
            int completed = 0;

            foreach (var spec in candies)
            {
                if (spec.Bomb) continue;

                int dest = -1;
                for (int i = 0; i < slots; i++)
                {
                    if (locked[i] || fill[i] >= capacity) continue;
                    if (hasHue[i] && hue[i] == spec.Color)
                    {
                        dest = i;
                        break;
                    }
                }

                if (dest < 0)
                {
                    for (int i = 0; i < slots; i++)
                    {
                        if (locked[i] || fill[i] >= capacity || hasHue[i]) continue;
                        dest = i;
                        break;
                    }
                }

                if (dest < 0) return false;

                if (!hasHue[dest])
                {
                    hasHue[dest] = true;
                    hue[dest] = spec.Color;
                }

                fill[dest]++;
                if (fill[dest] >= capacity)
                {
                    completed++;
                    fill[dest] = 0;
                    hasHue[dest] = false;
                    if (lockedLeft > 0)
                    {
                        for (int i = 0; i < slots; i++)
                        {
                            if (!locked[i]) continue;
                            locked[i] = false;
                            lockedLeft--;
                            break;
                        }
                    }
                }
            }

            return completed >= level.QuotaBoxes;
        }

        static int Remaining(int[] need)
        {
            int n = 0;
            for (int i = 0; i < need.Length; i++) n += need[i];
            return n;
        }
    }
}
