using UnityEngine;

namespace CandyBeltSort
{
    public static class ProgressSave
    {
        const string Highest = "cbs.highest";
        const string Coins = "cbs.coins";
        const string Sfx = "cbs.sfx";
        const string Haptics = "cbs.haptics";
        const string Streak = "cbs.streak";
        const string StreakDay = "cbs.streakDay";

        public static int HighestUnlocked { get; private set; }
        public static int CoinsOnHand { get; private set; }
        public static bool SfxOn { get; private set; } = true;
        public static bool HapticsOn { get; private set; } = true;
        public static int DailyStreak { get; private set; }

        public static void Load()
        {
            HighestUnlocked = PlayerPrefs.GetInt(Highest, 0);
            CoinsOnHand = PlayerPrefs.GetInt(Coins, 0);
            SfxOn = PlayerPrefs.GetInt(Sfx, 1) == 1;
            HapticsOn = PlayerPrefs.GetInt(Haptics, 1) == 1;
            DailyStreak = PlayerPrefs.GetInt(Streak, 0);
            TickStreak();
        }

        public static void UnlockThrough(int levelIndex)
        {
            int next = Mathf.Clamp(levelIndex + 1, 0, LevelCatalog.Count - 1);
            if (next > HighestUnlocked)
            {
                HighestUnlocked = next;
                PlayerPrefs.SetInt(Highest, HighestUnlocked);
                PlayerPrefs.Save();
            }
        }

        public static bool IsUnlocked(int levelIndex) => levelIndex <= HighestUnlocked;

        public static void AddCoins(int amount)
        {
            CoinsOnHand = Mathf.Max(0, CoinsOnHand + amount);
            PlayerPrefs.SetInt(Coins, CoinsOnHand);
            PlayerPrefs.Save();
        }

        public static bool SpendCoins(int amount)
        {
            if (CoinsOnHand < amount) return false;
            AddCoins(-amount);
            return true;
        }

        public static void SetSfx(bool on)
        {
            SfxOn = on;
            PlayerPrefs.SetInt(Sfx, on ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void SetHaptics(bool on)
        {
            HapticsOn = on;
            PlayerPrefs.SetInt(Haptics, on ? 1 : 0);
            PlayerPrefs.Save();
        }

        static void TickStreak()
        {
            string today = System.DateTime.UtcNow.ToString("yyyyMMdd");
            string last = PlayerPrefs.GetString(StreakDay, "");
            if (last == today) return;

            if (last == System.DateTime.UtcNow.AddDays(-1).ToString("yyyyMMdd"))
                DailyStreak += 1;
            else
                DailyStreak = 1;

            PlayerPrefs.SetInt(Streak, DailyStreak);
            PlayerPrefs.SetString(StreakDay, today);
            PlayerPrefs.Save();
        }
    }
}
