using UnityEngine;

namespace CandyBeltSort
{
    public static class Sfx
    {
        static AudioSource _source;
        static AudioClip _tap;
        static AudioClip _box;
        static AudioClip _seal;
        static AudioClip _win;
        static AudioClip _fail;
        static AudioClip _reject;
        static AudioClip _bomb;

        public static void Init(Transform parent)
        {
            var go = new GameObject("Sfx");
            go.transform.SetParent(parent, false);
            _source = go.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;
            _tap = Tone(880, 0.07f, 0.18f);
            _box = Tone(523, 0.11f, 0.2f);
            _seal = Chord(392, 523, 0.18f, 0.16f);
            _win = Chord(523, 784, 0.32f, 0.2f);
            _fail = Tone(196, 0.28f, 0.2f);
            _reject = Tone(220, 0.08f, 0.16f);
            _bomb = Tone(140, 0.16f, 0.22f);
        }

        public static void Tap() => Play(_tap);
        public static void Box() => Play(_box);
        public static void Seal() => Play(_seal);
        public static void Win() => Play(_win);
        public static void Fail() => Play(_fail);
        public static void Reject() => Play(_reject);
        public static void Bomb() => Play(_bomb);

        static void Play(AudioClip clip)
        {
            if (!ProgressSave.SfxOn || _source == null || clip == null) return;
            _source.PlayOneShot(clip);
        }

        static AudioClip Tone(float freq, float duration, float volume)
        {
            int rate = 44100;
            int samples = Mathf.Max(1, (int)(rate * duration));
            var clip = AudioClip.Create("tone", samples, 1, rate, false);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)rate;
                float env = Mathf.Clamp01(1f - t / duration);
                env *= Mathf.Clamp01(t / 0.008f);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * volume * env;
            }
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip Chord(float a, float b, float duration, float volume)
        {
            int rate = 44100;
            int samples = Mathf.Max(1, (int)(rate * duration));
            var clip = AudioClip.Create("chord", samples, 1, rate, false);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)rate;
                float env = Mathf.Clamp01(1f - t / duration);
                env *= Mathf.Clamp01(t / 0.01f);
                data[i] = (Mathf.Sin(2f * Mathf.PI * a * t) + Mathf.Sin(2f * Mathf.PI * b * t)) * 0.5f * volume * env;
            }
            clip.SetData(data, 0);
            return clip;
        }
    }
}
