using System;
using System.Collections;
using UnityEngine;

namespace CandyBeltSort
{
    public static class Tweens
    {
        public static IEnumerator Move(Transform t, Vector3 from, Vector3 to, float duration, AnimationCurve curve, Action done = null)
        {
            float elapsed = 0f;
            while (elapsed < duration && t != null)
            {
                elapsed += Time.deltaTime;
                float u = curve.Evaluate(Mathf.Clamp01(elapsed / duration));
                t.position = Vector3.LerpUnclamped(from, to, u);
                yield return null;
            }
            if (t != null) t.position = to;
            done?.Invoke();
        }

        public static IEnumerator Arc(Transform t, Vector3 from, Vector3 to, float height, float duration, Action done = null)
        {
            float elapsed = 0f;
            while (elapsed < duration && t != null)
            {
                elapsed += Time.deltaTime;
                float u = Mathf.Clamp01(elapsed / duration);
                u = u * u * (3f - 2f * u);
                var p = Vector3.Lerp(from, to, u);
                p.y += Mathf.Sin(u * Mathf.PI) * height;
                t.position = p;
                t.localScale = Vector3.one * Mathf.Lerp(1f, 0.82f, u);
                yield return null;
            }
            if (t != null) t.position = to;
            done?.Invoke();
        }

        public static IEnumerator PunchScale(Transform t, float punch, float duration)
        {
            if (t == null) yield break;
            var baseScale = t.localScale;
            float elapsed = 0f;
            while (elapsed < duration && t != null)
            {
                elapsed += Time.deltaTime;
                float u = elapsed / duration;
                float s = 1f + Mathf.Sin(u * Mathf.PI) * punch;
                t.localScale = baseScale * s;
                yield return null;
            }
            if (t != null) t.localScale = baseScale;
        }

        public static IEnumerator Shake(Transform t, float amount, float duration)
        {
            if (t == null) yield break;
            var origin = t.localPosition;
            float elapsed = 0f;
            while (elapsed < duration && t != null)
            {
                elapsed += Time.deltaTime;
                t.localPosition = origin + UnityEngine.Random.insideUnitSphere * amount;
                yield return null;
            }
            if (t != null) t.localPosition = origin;
        }
    }
}
