using UnityEngine;

namespace CandyBeltSort
{
    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (Object.FindFirstObjectByType<GameFlow>() != null) return;

            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            ProgressSave.Load();

            var root = new GameObject("CandyBeltApp");
            Object.DontDestroyOnLoad(root);
            root.AddComponent<AdManager>().Init();
            root.AddComponent<GameFlow>().Init();
        }
    }
}
