using System.Collections;
using UnityEngine;

namespace CandyBeltSort
{
    public class MockAdProvider : MonoBehaviour, IAdProvider
    {
        public bool RewardedReady => true;
        public bool InterstitialReady => true;

        public void Initialize(System.Action onReady) => onReady?.Invoke();

        public void ShowRewarded(string placement, System.Action<bool> onComplete)
        {
            Debug.Log($"[Ads] Mock rewarded: {placement}");
            StartCoroutine(Delay(() => onComplete?.Invoke(true), 0.35f));
        }

        public void ShowInterstitial(System.Action onClosed)
        {
            Debug.Log("[Ads] Mock interstitial");
            StartCoroutine(Delay(() => onClosed?.Invoke(), 0.25f));
        }

        static IEnumerator Delay(System.Action action, float t)
        {
            yield return new WaitForSecondsRealtime(t);
            action();
        }
    }
}
