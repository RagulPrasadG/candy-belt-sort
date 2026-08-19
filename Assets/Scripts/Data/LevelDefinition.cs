using UnityEngine;

namespace CandyBeltSort
{
    [System.Serializable]
    public class LevelDefinition
    {
        public int Index;
        public int WorldIndex;
        public int LevelInWorld;
        public int ColorCount = 2;
        public int OpenSlots = 2;
        public int BoxCapacity = 3;
        public int QuotaBoxes = 6;
        public float BeltSpeed = 1.2f;
        public float SpawnInterval = 1.1f;
        public float FrozenChance;
        public int LockedBoxCount;
        public float HiddenChance;
        public float BombChance;
        public int Seed = 1;

        public string WorldName => WorldCatalog.Get(WorldIndex).Name;
        public string DisplayName => $"{WorldName} {LevelInWorld + 1}";

        public int CandyNeeded => QuotaBoxes * BoxCapacity;
    }

    [System.Serializable]
    public class WorldInfo
    {
        public int Index;
        public string Name;
        public string Tagline;
        public Color Accent;
        public Color Floor;
        public Color Wall;
    }
}
