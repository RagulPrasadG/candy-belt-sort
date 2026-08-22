using UnityEngine;
using UnityEngine.UI;

namespace CandyBeltSort
{
    public class ResultView : MonoBehaviour
    {
        public static ResultView I { get; private set; }

        Canvas _canvas;
        Text _title;
        Text _body;
        Button _primary;
        Button _secondary;
        Text _primaryLabel;
        Text _secondaryLabel;
        int _levelIndex;
        bool _won;

        public void Build(Transform parent)
        {
            I = this;
            _canvas = UiKit.Canvas("Result", parent);
            _canvas.sortingOrder = 40;
            _canvas.gameObject.SetActive(false);

            UiKit.Panel(_canvas.transform, "Dim", Vector2.zero, Vector2.one, new Color(0f, 0f, 0f, 0.45f));
            var card = UiKit.Panel(_canvas.transform, "Card", new Vector2(0.08f, 0.28f), new Vector2(0.92f, 0.72f), Palette.Paper);

            _title = UiKit.Label(card.transform, "Title", "Nice!", 72, Palette.Ink);
            _title.rectTransform.anchorMin = new Vector2(0.06f, 0.68f);
            _title.rectTransform.anchorMax = new Vector2(0.94f, 0.95f);

            _body = UiKit.Label(card.transform, "Body", "", 36, Palette.Ink);
            _body.rectTransform.anchorMin = new Vector2(0.08f, 0.38f);
            _body.rectTransform.anchorMax = new Vector2(0.92f, 0.68f);

            _primary = UiKit.Button(card.transform, "Primary", "Next", Palette.Good, OnPrimary, new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.22f));
            _primaryLabel = _primary.GetComponentInChildren<Text>();
            _secondary = UiKit.Button(card.transform, "Secondary", "Map", Palette.Hex("8D6E63"), OnSecondary, new Vector2(0.08f, 0.24f), new Vector2(0.92f, 0.36f));
            _secondaryLabel = _secondary.GetComponentInChildren<Text>();
        }

        public void ShowWin(int levelIndex, int coins)
        {
            _won = true;
            _levelIndex = levelIndex;
            _canvas.gameObject.SetActive(true);
            UiKit.SetText(_title, "Order packed!");
            UiKit.SetText(_body, $"{LevelCatalog.Get(levelIndex).DisplayName}\n+{coins} coins");
            UiKit.SetText(_primaryLabel, levelIndex + 1 < LevelCatalog.Count ? "Next level" : "Map");
            UiKit.SetText(_secondaryLabel, "World map");
        }

        public void ShowFail(int levelIndex)
        {
            _won = false;
            _levelIndex = levelIndex;
            _canvas.gameObject.SetActive(true);
            UiKit.SetText(_title, "Belt jammed");
            UiKit.SetText(_body, "Watch a short video to keep this order,\nor retry from the start.");
            UiKit.SetText(_primaryLabel, "Continue");
            UiKit.SetText(_secondaryLabel, "Retry");
        }

        public void Hide() => _canvas.gameObject.SetActive(false);

        void OnPrimary()
        {
            if (_won)
            {
                Hide();
                GameFlow.I.AfterWinContinue(_levelIndex);
            }
            else
            {
                Hide();
                GameFlow.I.ContinueFromFail(_levelIndex);
            }
        }

        void OnSecondary()
        {
            Hide();
            if (_won) GameFlow.I.ReturnToMap();
            else GameFlow.I.RetryLevel(_levelIndex);
        }
    }
}
