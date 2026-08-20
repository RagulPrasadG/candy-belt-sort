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
            Screen.orientation = ScreenOrientation.Portrait;

            ProgressSave.Load();
            StripLeftoverSceneObjects();
            var sceneCam = GameObject.Find("Main Camera");
            if (sceneCam != null) sceneCam.SetActive(false);

            var root = new GameObject("CandyBeltApp");
            Object.DontDestroyOnLoad(root);
            root.AddComponent<AdManager>().Init();
            root.AddComponent<GameFlow>().Init();
        }

        static void StripLeftoverSceneObjects()
        {
            string[] leftovers = { "Knife", "MeshCutter", "Vegetable" };
            for (int i = 0; i < leftovers.Length; i++)
            {
                var go = GameObject.Find(leftovers[i]);
                if (go != null) Object.Destroy(go);
            }
        }
    }
}
