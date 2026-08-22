using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CandyBeltSort
{
    // The first screen the player sees: logo, a big Play button, and settings.
    // Play opens the level-select screen (WorldMapView).
    public class MainMenuView : MonoBehaviour
    {
        public static MainMenuView I { get; private set; }

        Canvas _canvas;
        Text _coins;
        Text _streak;
        readonly List<RectTransform> _bobbers = new List<RectTransform>();
        readonly List<float> _bobPhase = new List<float>();

        public void Build(Transform parent)
        {
            I = this;
            _canvas = UiKit.Canvas("MainMenu", parent);
            _canvas.sortingOrder = 22;

            // Background — themed art if present, otherwise a warm candy gradient.
            var bg = SpriteFactory.TryNamed("bg_main_menu");
            if (bg != null)
            {
                UiKit.SpriteImage(_canvas.transform, "Bg", bg, Color.white, Vector2.zero, Vector2.one)
                    .preserveAspect = false;
            }
            else
            {
                UiKit.Panel(_canvas.transform, "BgTop", new Vector2(0f, 0.5f), Vector2.one, Palette.Hex("FFC1D9"));
                UiKit.Panel(_canvas.transform, "BgBottom", Vector2.zero, new Vector2(1f, 0.5f), Palette.Hex("FFE3B3"));
            }

            _coins = UiKit.Label(_canvas.transform, "Coins", "Coins 0", 34, Palette.Ink, TextAnchor.MiddleLeft);
            _coins.rectTransform.anchorMin = new Vector2(0.05f, 0.92f);
            _coins.rectTransform.anchorMax = new Vector2(0.5f, 0.98f);

            _streak = UiKit.Label(_canvas.transform, "Streak", "Streak 1", 34, Palette.Ink, TextAnchor.MiddleRight);
            _streak.rectTransform.anchorMin = new Vector2(0.5f, 0.92f);
            _streak.rectTransform.anchorMax = new Vector2(0.95f, 0.98f);

            // Logo — art if present, else a stacked text treatment.
            var logo = SpriteFactory.TryNamed("logo");
            if (logo != null)
            {
                UiKit.SpriteImage(_canvas.transform, "Logo", logo, Color.white,
                    new Vector2(0.1f, 0.62f), new Vector2(0.9f, 0.9f));
            }
            else
            {
                var title = UiKit.Label(_canvas.transform, "Title", "Candy\nBelt Sort", 110, Color.white);
                title.rectTransform.anchorMin = new Vector2(0.06f, 0.66f);
                title.rectTransform.anchorMax = new Vector2(0.94f, 0.9f);
                title.fontStyle = FontStyle.Bold;
                title.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(4f, -4f);
                var sub = UiKit.Label(_canvas.transform, "Sub", "Sort the sweets on the belt!", 34, Palette.Ink);
                sub.rectTransform.anchorMin = new Vector2(0.1f, 0.6f);
                sub.rectTransform.anchorMax = new Vector2(0.9f, 0.66f);
            }

            BuildCandyRow();

            UiKit.Button(_canvas.transform, "Play", "PLAY", Palette.Good, () => GameFlow.I.ShowLevels(),
                new Vector2(0.18f, 0.3f), new Vector2(0.82f, 0.42f));
            UiKit.Button(_canvas.transform, "Levels", "LEVELS", Palette.Hex("FF6F91"), () => GameFlow.I.ShowLevels(),
                new Vector2(0.24f, 0.2f), new Vector2(0.76f, 0.28f));
            UiKit.Button(_canvas.transform, "Settings", "Settings", Palette.Hex("8D6E63"), () => SettingsView.I.Show(),
                new Vector2(0.32f, 0.07f), new Vector2(0.68f, 0.14f));
        }

        void BuildCandyRow()
        {
            var colors = new[] { CandyColor.Pink, CandyColor.Mint, CandyColor.Lemon, CandyColor.Blueberry, CandyColor.Grape };
            for (int i = 0; i < colors.Length; i++)
            {
                var sprite = SpriteFactory.Candy(colors[i], false);
                float x0 = 0.1f + i * 0.16f;
                var img = UiKit.SpriteImage(_canvas.transform, $"Candy{i}", sprite, Color.white,
                    new Vector2(x0, 0.46f), new Vector2(x0 + 0.14f, 0.58f));
                _bobbers.Add(img.rectTransform);
                _bobPhase.Add(i * 0.6f);
            }
        }

        void Update()
        {
            for (int i = 0; i < _bobbers.Count; i++)
            {
                if (_bobbers[i] == null) continue;
                float y = Mathf.Sin(Time.unscaledTime * 2.2f + _bobPhase[i]) * 10f;
                var p = _bobbers[i].anchoredPosition;
                p.y = y;
                _bobbers[i].anchoredPosition = p;
            }
        }

        public void Show()
        {
            Refresh();
            _canvas.gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (_canvas != null) _canvas.gameObject.SetActive(false);
        }

        public void Refresh()
        {
            UiKit.SetText(_coins, $"Coins {ProgressSave.CoinsOnHand}");
            UiKit.SetText(_streak, $"Streak {ProgressSave.DailyStreak}");
        }
    }
}
