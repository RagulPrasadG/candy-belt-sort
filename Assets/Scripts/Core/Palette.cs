using UnityEngine;

namespace CandyBeltSort
{
    public static class Palette
    {
        public static readonly Color Pink = Hex("F48FB1");
        public static readonly Color Mint = Hex("69F0AE");
        public static readonly Color Lemon = Hex("FFD54F");
        public static readonly Color Blueberry = Hex("4FC3F7");
        public static readonly Color Grape = Hex("CE93D8");
        public static readonly Color HiddenFoil = Hex("B0BEC5");
        public static readonly Color FrozenTint = Hex("E1F5FE");
        public static readonly Color Bomb = Hex("FF5252");
        public static readonly Color Ink = Hex("3E2723");
        public static readonly Color Paper = Hex("FFF8F0");
        public static readonly Color Accent = Hex("FF6F91");
        public static readonly Color Good = Hex("00C853");
        public static readonly Color Warn = Hex("FF8A65");

        public static Color Of(CandyColor color)
        {
            switch (color)
            {
                case CandyColor.Pink: return Pink;
                case CandyColor.Mint: return Mint;
                case CandyColor.Lemon: return Lemon;
                case CandyColor.Blueberry: return Blueberry;
                default: return Grape;
            }
        }

        public static string Label(CandyColor color)
        {
            return color.ToString();
        }

        public static Color Hex(string hex)
        {
            if (!hex.StartsWith("#")) hex = "#" + hex;
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }
    }
}
