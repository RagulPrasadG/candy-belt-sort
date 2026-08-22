using UnityEngine;

namespace CandyBeltSort
{
    public class AdManager : MonoBehaviour
    {
        public static AdManager I { get; private set; }

        IAdProvider _provider;
        int _winsSinceInterstitial;
        float _lastRewardedTime = -999f;
        float _lastInterstitialTime = -999f;

        public void Init()
        {
            I = this;
#if UNITY_ANDROID && !UNITY_EDITOR && GOOGLE_MOBILE_ADS
            _provider = gameObject.AddComponent<AdMobProvider>();
#else
            _provider = gameObject.AddComponent<MockAdProvider>();
#endif
            _provider.Initialize(() => Debug.Log("[Ads] Ready"));
        }

        public void ShowRewarded(string placement, System.Action<bool> onComplete)
        {
            _provider.ShowRewarded(placement, ok =>
            {
                if (ok) _lastRewardedTime = Time.unscaledTime;
                onComplete?.Invoke(ok);
            });
        }

        public void TryInterstitialAfterWin(int completedLevelIndex, System.Action then)
        {
            float sinceRewarded = Time.unscaledTime - _lastRewardedTime;
            float sinceInterstitial = Time.unscaledTime - _lastInterstitialTime;
            if (!AdPlacementPolicy.CanShowInterstitial(completedLevelIndex, _winsSinceInterstitial, sinceRewarded, sinceInterstitial))
            {
                then?.Invoke();
                return;
            }

            _winsSinceInterstitial = 0;
            _lastInterstitialTime = Time.unscaledTime;
            _provider.ShowInterstitial(then);
        }

        public void RegisterWin() => _winsSinceInterstitial++;
    }
}
