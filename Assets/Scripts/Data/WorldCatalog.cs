using UnityEngine;

namespace CandyBeltSort
{
    public static class WorldCatalog
    {
        public const int WorldCount = 25;
        public const int LevelsPerWorld = 20;
        public const int TotalLevels = WorldCount * LevelsPerWorld;

        static readonly WorldInfo[] Worlds =
        {
            new WorldInfo { Index = 0,  Theme = "sugar_bakery",     Name = "Sugar Bakery",      Tagline = "Warm ovens, easy orders",     Accent = Hex("F48FB1"), Floor = Hex("FFF3E0"), Wall = Hex("FFCDD2") },
            new WorldInfo { Index = 1,  Theme = "gummy_lab",        Name = "Gummy Lab",         Tagline = "Bouncy colors, tighter timing", Accent = Hex("69F0AE"), Floor = Hex("E8F5E9"), Wall = Hex("B2DFDB") },
            new WorldInfo { Index = 2,  Theme = "chocolate_mill",   Name = "Chocolate Mill",    Tagline = "Richer mix, first freezes",   Accent = Hex("A1887F"), Floor = Hex("EFEBE9"), Wall = Hex("D7CCC8") },
            new WorldInfo { Index = 3,  Theme = "cotton_cloud",     Name = "Cotton Cloud",      Tagline = "Four flavors in the air",     Accent = Hex("80D8FF"), Floor = Hex("E3F2FD"), Wall = Hex("B3E5FC") },
            new WorldInfo { Index = 4,  Theme = "berry_factory",    Name = "Berry Factory",     Tagline = "Wrappers hide the flavor",    Accent = Hex("F06292"), Floor = Hex("FCE4EC"), Wall = Hex("F8BBD0") },
            new WorldInfo { Index = 5,  Theme = "mint_works",       Name = "Mint Works",        Tagline = "Three slots, colder belt",    Accent = Hex("1DE9B6"), Floor = Hex("E0F2F1"), Wall = Hex("B2DFDB") },
            new WorldInfo { Index = 6,  Theme = "lollipop_lane",    Name = "Lollipop Lane",     Tagline = "Bombs roll in — tap them off",Accent = Hex("FF8A80"), Floor = Hex("FFF8E1"), Wall = Hex("FFE0B2") },
            new WorldInfo { Index = 7,  Theme = "caramel_core",     Name = "Caramel Core",      Tagline = "Fast belt, locked lids",      Accent = Hex("FFB74D"), Floor = Hex("FFF3E0"), Wall = Hex("FFCC80") },
            new WorldInfo { Index = 8,  Theme = "rainbow_mix",      Name = "Rainbow Mix",       Tagline = "All five colors at once",     Accent = Hex("CE93D8"), Floor = Hex("F3E5F5"), Wall = Hex("E1BEE7") },
            new WorldInfo { Index = 9,  Theme = "honey_hive",       Name = "Honey Hive",        Tagline = "Sticky, golden and busy",     Accent = Hex("FFC107"), Floor = Hex("FFF8E1"), Wall = Hex("FFE082") },
            new WorldInfo { Index = 10, Theme = "marshmallow_bay",  Name = "Marshmallow Bay",   Tagline = "Soft pillows, sharp timing",  Accent = Hex("F8BBD0"), Floor = Hex("FCE4EC"), Wall = Hex("F48FB1") },
            new WorldInfo { Index = 11, Theme = "toffee_town",      Name = "Toffee Town",       Tagline = "Buttery and brisk",           Accent = Hex("D79A5B"), Floor = Hex("EFEBE9"), Wall = Hex("BCAAA4") },
            new WorldInfo { Index = 12, Theme = "bubblegum_yard",   Name = "Bubblegum Yard",    Tagline = "Pop, stretch, sort",          Accent = Hex("FF80AB"), Floor = Hex("FCE4EC"), Wall = Hex("F48FB1") },
            new WorldInfo { Index = 13, Theme = "licorice_line",    Name = "Licorice Line",     Tagline = "Dark twists, cold snaps",     Accent = Hex("7E57C2"), Floor = Hex("EDE7F6"), Wall = Hex("B39DDB") },
            new WorldInfo { Index = 14, Theme = "fudge_foundry",    Name = "Fudge Foundry",     Tagline = "Molten, heavy, relentless",   Accent = Hex("8D6E63"), Floor = Hex("EFEBE9"), Wall = Hex("A1887F") },
            new WorldInfo { Index = 15, Theme = "sherbet_springs",  Name = "Sherbet Springs",   Tagline = "Fizzy pastels everywhere",    Accent = Hex("B2FF59"), Floor = Hex("F1F8E9"), Wall = Hex("DCEDC8") },
            new WorldInfo { Index = 16, Theme = "peppermint_pole",  Name = "Peppermint Pole",   Tagline = "Icy belts, frozen swirls",    Accent = Hex("EF5350"), Floor = Hex("E1F5FE"), Wall = Hex("B3E5FC") },
            new WorldInfo { Index = 17, Theme = "nougat_nook",      Name = "Nougat Nook",       Tagline = "Nutty layers stack up",       Accent = Hex("D4A373"), Floor = Hex("FFF3E0"), Wall = Hex("E0C79A") },
            new WorldInfo { Index = 18, Theme = "praline_palace",   Name = "Praline Palace",    Tagline = "Rich, regal, ruthless",       Accent = Hex("C79A6B"), Floor = Hex("EFEBE9"), Wall = Hex("BCAAA4") },
            new WorldInfo { Index = 19, Theme = "jellybean_junction",Name = "Jellybean Junction",Tagline = "Every color, full speed",    Accent = Hex("FF7043"), Floor = Hex("FBE9E7"), Wall = Hex("FFAB91") },
            new WorldInfo { Index = 20, Theme = "macaron_atelier",  Name = "Macaron Atelier",   Tagline = "Delicate, precise, fast",     Accent = Hex("CE93D8"), Floor = Hex("F3E5F5"), Wall = Hex("E1BEE7") },
            new WorldInfo { Index = 21, Theme = "donut_depot",      Name = "Donut Depot",       Tagline = "Glazed chaos on rails",       Accent = Hex("FF8A65"), Floor = Hex("FFF3E0"), Wall = Hex("FFCCBC") },
            new WorldInfo { Index = 22, Theme = "popsicle_pier",    Name = "Popsicle Pier",     Tagline = "Frozen rush by the sea",      Accent = Hex("4FC3F7"), Floor = Hex("E1F5FE"), Wall = Hex("81D4FA") },
            new WorldInfo { Index = 23, Theme = "starlight_sweets",  Name = "Starlight Sweets",  Tagline = "The night shift never sleeps",Accent = Hex("9575CD"), Floor = Hex("E8EAF6"), Wall = Hex("9FA8DA") },
            new WorldInfo { Index = 24, Theme = "galaxy_confectionery",Name = "Galaxy Confectionery",Tagline = "The grand finale factory",Accent = Hex("7C4DFF"), Floor = Hex("1A237E"), Wall = Hex("311B92") }
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
