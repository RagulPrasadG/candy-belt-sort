using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace CandyBeltSort
{
    public class GameFlow : MonoBehaviour
    {
        public static GameFlow I { get; private set; }

        MainMenuView _menu;
        WorldMapView _map;
        HudView _hud;
        ResultView _result;
        SettingsView _settings;
        GameplayController _gameplay;
        int _currentLevel;

        public void Init()
        {
            I = this;
            EnsureEventSystem();
            UiKit.Init();
            Sfx.Init(transform);

            _menu = gameObject.AddComponent<MainMenuView>();
            _map = gameObject.AddComponent<WorldMapView>();
            _hud = gameObject.AddComponent<HudView>();
            _result = gameObject.AddComponent<ResultView>();
            _settings = gameObject.AddComponent<SettingsView>();
            _gameplay = gameObject.AddComponent<GameplayController>();

            _menu.Build(transform);
            _map.Build(transform);
            _hud.Build(transform);
            _result.Build(transform);
            _settings.Build(transform);

            ShowMainMenu();
        }

        public void PlayLevel(int index)
        {
            _currentLevel = index;
            _menu.Hide();
            _map.Hide();
            _result.Hide();
            _settings.Hide();
            _gameplay.gameObject.SetActive(true);
            _gameplay.StartLevel(index);
        }

        public void HandleWin(int levelIndex)
        {
            int coins = 10 + WorldCatalog.Get(LevelCatalog.Get(levelIndex).WorldIndex).Index;
            ProgressSave.AddCoins(coins);
            ProgressSave.UnlockThrough(levelIndex);
            AdManager.I.RegisterWin();
            HudView.I.Hide();
            ResultView.I.ShowWin(levelIndex, coins);
        }

        public void HandleFail(int levelIndex)
        {
            HudView.I.Hide();
            ResultView.I.ShowFail(levelIndex);
        }

        public void AfterWinContinue(int levelIndex)
        {
            AdManager.I.TryInterstitialAfterWin(levelIndex, () =>
            {
                if (levelIndex + 1 < LevelCatalog.Count)
                    PlayLevel(levelIndex + 1);
                else
                    ReturnToMap();
            });
        }

        public void ContinueFromFail(int levelIndex)
        {
            AdManager.I.ShowRewarded("continue", ok =>
            {
                if (!ok)
                {
                    ResultView.I.ShowFail(levelIndex);
                    return;
                }
                HudView.I.Show(_gameplay);
                _gameplay.ContinueAfterFail();
            });
        }

        public void RetryLevel(int levelIndex) => PlayLevel(levelIndex);

        public void ReturnToMap()
        {
            _gameplay.StopLevel();
            _result.Hide();
            ShowLevels();
        }

        public void ShowMainMenu()
        {
            _hud.Hide();
            _map.Hide();
            _result.Hide();
            _settings.Hide();
            _menu.Show();
        }

        public void ShowLevels()
        {
            _hud.Hide();
            _menu.Hide();
            _map.Show();
        }

        void ShowMap() => ShowLevels();

        static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            try
            {
#if ENABLE_INPUT_SYSTEM
                go.AddComponent<InputSystemUIInputModule>();
#else
                go.AddComponent<StandaloneInputModule>();
#endif
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[CandyBelt] EventSystem module fallback: " + ex.Message);
                if (go.GetComponent<StandaloneInputModule>() == null)
                    go.AddComponent<StandaloneInputModule>();
            }
        }
    }
}
