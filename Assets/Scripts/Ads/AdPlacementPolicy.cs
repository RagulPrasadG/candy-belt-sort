using UnityEngine;

namespace CandyBeltSort
{
    public static class AdPlacementPolicy
    {
        public const int NoInterstitialBeforeLevel = 3;
        public const float RewardedCooldownSeconds = 8f;

        // Show an interstitial after every third level completed (past the early tutorial levels).
        public const int InterstitialEveryWins = 3;

        public static int InterstitialEvery(int levelIndex) => InterstitialEveryWins;

        public static bool CanShowInterstitial(int completedLevelIndex, int winsSinceInterstitial, float secondsSinceRewarded, float secondsSinceInterstitial)
        {
            if (completedLevelIndex < NoInterstitialBeforeLevel) return false;
            if (secondsSinceRewarded < RewardedCooldownSeconds) return false;
            if (secondsSinceInterstitial < 6f) return false;
            return winsSinceInterstitial >= InterstitialEvery(completedLevelIndex);
        }
    }

    public interface IAdProvider
    {
        void Initialize(System.Action onReady);
        bool RewardedReady { get; }
        bool InterstitialReady { get; }
        void ShowRewarded(string placement, System.Action<bool> onComplete);
        void ShowInterstitial(System.Action onClosed);
    }
}
