using UnityEngine;

namespace CandyBeltSort
{
    public static class LevelCatalog
    {
        public const int Count = WorldCatalog.TotalLevels;

        public static LevelDefinition Get(int index)
        {
            index = Mathf.Clamp(index, 0, Count - 1);
            int world = index / WorldCatalog.LevelsPerWorld;
            int inWorld = index % WorldCatalog.LevelsPerWorld;
            int perWorld = Mathf.Max(1, WorldCatalog.LevelsPerWorld);

            // Global progress 0..1 across every level, plus a smooth per-world ramp.
            float p = Count > 1 ? index / (float)(Count - 1) : 0f;
            float wp = perWorld > 1 ? inWorld / (float)(perWorld - 1) : 0f;

            // Colours ramp 2 -> 5 across the whole game; early worlds stay gentle.
            int colorCount = Mathf.Clamp(2 + Mathf.RoundToInt(p * 3f), 2, 5);
            if (world == 0) colorCount = inWorld < 2 ? 2 : 3;
            else if (world == 1) colorCount = 3;

            // Open slots grow 2 -> 4 so late worlds juggle more crates.
            int slots = Mathf.Clamp(2 + Mathf.FloorToInt(p * 2.99f), 2, 4);
            if (world == 0) slots = 2;

            // Boxes required per level ramp 3 -> 20 with a gentle in-world climb.
            int quota = Mathf.Clamp(4 + Mathf.RoundToInt(p * 14f) + inWorld / 5, 3, 20);
            if (index == 0) quota = 3;
            if (index == 1) quota = 4;

            // Belt speed and spawn cadence tighten the deeper you go.
            float beltSpeed = index < 2 ? 1.05f : Mathf.Clamp(1.25f + p * 1.35f + wp * 0.35f, 1.1f, 2.85f);
            float spawnInterval = index < 2 ? 0.78f : Mathf.Clamp(0.82f - p * 0.42f - wp * 0.08f, 0.32f, 0.9f);

            // Hazards unlock progressively and scale with depth.
            float frozen = world < 2 ? 0f : Mathf.Clamp01((world - 2) * 0.02f + wp * 0.12f) * 0.6f;
            float hidden = world < 4 ? 0f : Mathf.Clamp01((world - 4) * 0.015f + wp * 0.1f) * 0.55f;
            float bomb = world < 6 ? 0f : Mathf.Clamp01(0.05f + (world - 6) * 0.008f + wp * 0.08f) * 0.7f;

            int locked = 0;
            if (world >= 3 && (inWorld % 5) == 4) locked = 1;
            if (world >= 9 && inWorld >= 12) locked = 1;

            int laneCount = world == 0 ? 1 : 2;

            return new LevelDefinition
            {
                Index = index,
                WorldIndex = world,
                LevelInWorld = inWorld,
                ColorCount = colorCount,
                OpenSlots = slots,
                LaneCount = laneCount,
                BoxCapacity = 3,
                QuotaBoxes = quota,
                BeltSpeed = beltSpeed,
                SpawnInterval = spawnInterval,
                FrozenChance = frozen,
                LockedBoxCount = locked,
                HiddenChance = hidden,
                BombChance = bomb,
                Seed = 17031 + index * 97
            };
        }

        public static string ValidateAll()
        {
            var candies = new System.Collections.Generic.List<SpawnSpec>();
            int issues = 0;
            var sb = new System.Text.StringBuilder();
            const int expected = 500;
            if (Count != expected)
            {
                issues++;
                sb.AppendLine($"Expected {expected} levels, got {Count}");
            }

            for (int i = 0; i < Count; i++)
            {
                var level = Get(i);
                if (level.OpenSlots < 2)
                {
                    issues++;
                    sb.AppendLine($"L{i}: open slots {level.OpenSlots}");
                }

                if (level.LaneCount < 1 || level.LaneCount > 2)
                {
                    issues++;
                    sb.AppendLine($"L{i}: lanes {level.LaneCount}");
                }

                if (level.QuotaBoxes < 1 || level.BoxCapacity < 1)
                {
                    issues++;
                    sb.AppendLine($"L{i}: quota/capacity invalid");
                }

                LevelSequencer.Build(level, candies);
                int real = 0;
                for (int c = 0; c < candies.Count; c++)
                {
                    if (!candies[c].Bomb) real++;
                }

                if (real != level.QuotaBoxes * level.BoxCapacity)
                {
                    issues++;
                    sb.AppendLine($"L{i}: candies {real} != {level.QuotaBoxes * level.BoxCapacity}");
                }

                if (!LevelSequencer.IsGreedySolvable(level, candies))
                {
                    issues++;
                    sb.AppendLine($"L{i}: greedy play cannot finish");
                }
            }

            return issues == 0
                ? $"OK — {Count} levels, greedy-solvable front-queue sequences."
                : $"{issues} issue(s)\n{sb}";
        }
    }
}
