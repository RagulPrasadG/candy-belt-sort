#if GOOGLE_MOBILE_ADS
using System;
using GoogleMobileAds.Api;
using UnityEngine;

namespace CandyBeltSort
{
    public class AdMobProvider : MonoBehaviour, IAdProvider
    {
        public const string TestAppId = "ca-app-pub-3940256099942544~3347511713";
        public const string TestInterstitial = "ca-app-pub-3940256099942544/1033173712";
        public const string TestRewarded = "ca-app-pub-3940256099942544/5224354917";

        RewardedAd _rewarded;
        InterstitialAd _interstitial;
        bool _ready;

        public bool RewardedReady => _rewarded != null && _rewarded.CanShowAd();
        public bool InterstitialReady => _interstitial != null && _interstitial.CanShowAd();

        public void Initialize(Action onReady)
        {
            MobileAds.Initialize(_ =>
            {
                _ready = true;
                LoadRewarded();
                LoadInterstitial();
                onReady?.Invoke();
            });
        }

        public void ShowRewarded(string placement, Action<bool> onComplete)
        {
            if (!RewardedReady)
            {
                LoadRewarded();
                onComplete?.Invoke(false);
                return;
            }

            bool earned = false;
            _rewarded.OnAdFullScreenContentClosed += () =>
            {
                LoadRewarded();
                onComplete?.Invoke(earned);
            };
            _rewarded.OnAdFullScreenContentFailed += _ =>
            {
                LoadRewarded();
                onComplete?.Invoke(false);
            };
            _rewarded.Show(_ => earned = true);
        }

        public void ShowInterstitial(Action onClosed)
        {
            if (!InterstitialReady)
            {
                LoadInterstitial();
                onClosed?.Invoke();
                return;
            }

            _interstitial.OnAdFullScreenContentClosed += () =>
            {
                LoadInterstitial();
                onClosed?.Invoke();
            };
            _interstitial.OnAdFullScreenContentFailed += _ =>
            {
                LoadInterstitial();
                onClosed?.Invoke();
            };
            _interstitial.Show();
        }

        void LoadRewarded()
        {
            RewardedAd.Load(TestRewarded, new AdRequest(), (ad, error) =>
            {
                if (error != null)
                {
                    Debug.LogWarning("[Ads] Rewarded load failed: " + error);
                    return;
                }
                _rewarded = ad;
            });
        }

        void LoadInterstitial()
        {
            InterstitialAd.Load(TestInterstitial, new AdRequest(), (ad, error) =>
            {
                if (error != null)
                {
                    Debug.LogWarning("[Ads] Interstitial load failed: " + error);
                    return;
                }
                _interstitial = ad;
            });
        }
    }
}
#else
using UnityEngine;

namespace CandyBeltSort
{
    public class AdMobProvider : MockAdProvider
    {
        public const string TestAppId = "ca-app-pub-3940256099942544~3347511713";
        public const string TestInterstitial = "ca-app-pub-3940256099942544/1033173712";
        public const string TestRewarded = "ca-app-pub-3940256099942544/5224354917";
    }
}
#endif
