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

            int colorCount = world switch
            {
                0 => 2,
                1 => 3,
                2 => 3,
                3 => 4,
                4 => 4,
                5 => 4,
                6 => 4,
                7 => 5,
                8 => 5,
                _ => 5
            };
            if (world == 0 && inWorld >= 2)
                colorCount = 3;

            int slots = world <= 4 ? 2 : 3;
            if (world == 0 && inWorld < 4)
                slots = 2;

            int quota = 4 + world * 2 + inWorld / 2;
            if (index == 0) quota = 3;
            if (index == 1) quota = 4;
            quota = Mathf.Min(quota, 16);

            float beltSpeed;
            if (index < 2) beltSpeed = 1.05f;
            else beltSpeed = 1.45f + world * 0.16f + inWorld * 0.045f;

            float spawnInterval;
            if (index < 2) spawnInterval = 0.72f;
            else spawnInterval = Mathf.Max(0.32f, 0.52f - world * 0.045f - inWorld * 0.012f);

            float frozen = world < 2 ? 0f : Mathf.Clamp01((world - 1) * 0.05f + inWorld * 0.006f);
            int locked = 0;
            if (world >= 3 && inWorld % 4 == 3) locked = 1;
            if (world >= 7 && inWorld >= 8) locked = 1;

            float hidden = world < 4 ? 0f : Mathf.Clamp01(0.06f + (world - 4) * 0.035f + inWorld * 0.004f);
            float bomb = world < 6 ? 0f : Mathf.Clamp01(0.04f + (world - 6) * 0.025f);

            return new LevelDefinition
            {
                Index = index,
                WorldIndex = world,
                LevelInWorld = inWorld,
                ColorCount = colorCount,
                OpenSlots = slots,
                LaneCount = world == 0 ? 1 : 2,
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
            if (Count != 120)
            {
                issues++;
                sb.AppendLine($"Expected 120 levels, got {Count}");
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
