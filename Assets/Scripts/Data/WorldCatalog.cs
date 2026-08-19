using UnityEngine;

namespace CandyBeltSort
{
    public static class WorldCatalog
    {
        public const int WorldCount = 10;
        public const int LevelsPerWorld = 12;
        public const int TotalLevels = WorldCount * LevelsPerWorld;

        static readonly WorldInfo[] Worlds =
        {
            new WorldInfo { Index = 0, Name = "Sugar Bakery", Tagline = "Warm ovens, easy orders", Accent = Hex("F48FB1"), Floor = Hex("FFF3E0"), Wall = Hex("FFCDD2") },
            new WorldInfo { Index = 1, Name = "Gummy Lab", Tagline = "Bouncy colors, tighter timing", Accent = Hex("69F0AE"), Floor = Hex("E8F5E9"), Wall = Hex("B2DFDB") },
            new WorldInfo { Index = 2, Name = "Chocolate Mill", Tagline = "Richer mix, first freezes", Accent = Hex("A1887F"), Floor = Hex("EFEBE9"), Wall = Hex("D7CCC8") },
            new WorldInfo { Index = 3, Name = "Cotton Cloud", Tagline = "Four flavors in the air", Accent = Hex("80D8FF"), Floor = Hex("E3F2FD"), Wall = Hex("B3E5FC") },
            new WorldInfo { Index = 4, Name = "Berry Factory", Tagline = "Wrappers hide the flavor", Accent = Hex("F06292"), Floor = Hex("FCE4EC"), Wall = Hex("F8BBD0") },
            new WorldInfo { Index = 5, Name = "Mint Works", Tagline = "Three slots, colder belt", Accent = Hex("1DE9B6"), Floor = Hex("E0F2F1"), Wall = Hex("B2DFDB") },
            new WorldInfo { Index = 6, Name = "Lollipop Lane", Tagline = "Bombs roll in — tap them off", Accent = Hex("FF8A80"), Floor = Hex("FFF8E1"), Wall = Hex("FFE0B2") },
            new WorldInfo { Index = 7, Name = "Caramel Core", Tagline = "Fast belt, locked lids", Accent = Hex("FFB74D"), Floor = Hex("FFF3E0"), Wall = Hex("FFCC80") },
            new WorldInfo { Index = 8, Name = "Rainbow Mix", Tagline = "All five colors at once", Accent = Hex("CE93D8"), Floor = Hex("F3E5F5"), Wall = Hex("E1BEE7") },
            new WorldInfo { Index = 9, Name = "Midnight Kettle", Tagline = "The factory after hours", Accent = Hex("7E57C2"), Floor = Hex("263238"), Wall = Hex("37474F") }
        };

        public static WorldInfo Get(int index)
        {
            index = Mathf.Clamp(index, 0, Worlds.Length - 1);
            return Worlds[index];
        }

        static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var c);
            return c;
        }
    }
}
