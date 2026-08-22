using UnityEngine;

namespace CandyBeltSort
{
    public static class Haptics
    {
        public static void Light()
        {
            if (!ProgressSave.HapticsOn) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }
    }
}
