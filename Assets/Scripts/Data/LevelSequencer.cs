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
    }

    public static class LevelSequencer
    {
        public static void Build(LevelDefinition level, List<CandyColor> boxQueue, List<SpawnSpec> candies)
        {
            boxQueue.Clear();
            candies.Clear();

            var rng = new System.Random(level.Seed);
            int palette = Mathf.Clamp(level.ColorCount, 2, 5);

            for (int i = 0; i < level.QuotaBoxes; i++)
            {
                CandyColor color;
                if (i < palette)
                    color = (CandyColor)i;
                else
                    color = (CandyColor)rng.Next(0, palette);

                if (i >= level.OpenSlots)
                {
                    var recent = new HashSet<CandyColor>();
                    for (int r = Mathf.Max(0, boxQueue.Count - (level.OpenSlots - 1)); r < boxQueue.Count; r++)
                        recent.Add(boxQueue[r]);
                    int guard = 0;
                    while (recent.Count >= level.OpenSlots && recent.Contains(color) && guard++ < 12)
                        color = (CandyColor)rng.Next(0, palette);
                }

                boxQueue.Add(color);
            }

            var raw = new List<SpawnSpec>(level.CandyNeeded);
            foreach (var color in boxQueue)
            {
                for (int n = 0; n < level.BoxCapacity; n++)
                    raw.Add(new SpawnSpec { Color = color });
            }

            int window = Mathf.Clamp(level.OpenSlots * level.BoxCapacity + 2, 6, 12);
            WindowShuffle(raw, window, rng);

            foreach (var spec in raw)
            {
                if (!spec.Bomb && rng.NextDouble() < level.BombChance)
                {
                    spec.Bomb = true;
                    spec.Frozen = false;
                    spec.Hidden = false;
                    continue;
                }

                if (rng.NextDouble() < level.FrozenChance)
                    spec.Frozen = true;
                if (rng.NextDouble() < level.HiddenChance)
                    spec.Hidden = true;
            }

            candies.AddRange(raw);
        }

        static void WindowShuffle(List<SpawnSpec> list, int window, System.Random rng)
        {
            for (int start = 0; start < list.Count; start += window / 2)
            {
                int end = Mathf.Min(list.Count, start + window);
                for (int i = end - 1; i > start; i--)
                {
                    int j = rng.Next(start, i + 1);
                    (list[i], list[j]) = (list[j], list[i]);
                }
            }
        }
    }
}
