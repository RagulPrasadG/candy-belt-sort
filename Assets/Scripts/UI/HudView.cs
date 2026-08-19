using UnityEngine;
using UnityEngine.UI;

namespace CandyBeltSort
{
    public class HudView : MonoBehaviour
    {
        public static HudView I { get; private set; }

        Canvas _canvas;
        Text _progress;
        Text _undo;
        GameplayController _game;

        public void Build(Transform parent)
        {
            I = this;
            _canvas = UiKit.Canvas("HUD", parent);
            _canvas.sortingOrder = 30;
            _canvas.gameObject.SetActive(false);

            _progress = UiKit.Label(_canvas.transform, "Progress", "0/0", 52, Palette.Ink, TextAnchor.MiddleCenter);
            _progress.rectTransform.anchorMin = new Vector2(0.2f, 0.9f);
            _progress.rectTransform.anchorMax = new Vector2(0.8f, 0.98f);

            UiKit.Button(_canvas.transform, "Undo", "Undo", Palette.Hex("7E57C2"), () => _game?.RequestUndo(), new Vector2(0.04f, 0.02f), new Vector2(0.32f, 0.09f));
            UiKit.Button(_canvas.transform, "Slot", "Extra Slot", Palette.Hex("00897B"), () => _game?.RequestExtraSlot(), new Vector2(0.34f, 0.02f), new Vector2(0.66f, 0.09f));
            UiKit.Button(_canvas.transform, "Map", "Map", Palette.Hex("8D6E63"), () => GameFlow.I.ReturnToMap(), new Vector2(0.68f, 0.02f), new Vector2(0.96f, 0.09f));

            _undo = UiKit.Label(_canvas.transform, "UndoHint", "Free undos: 2", 28, Palette.Ink, TextAnchor.MiddleLeft);
            _undo.rectTransform.anchorMin = new Vector2(0.04f, 0.1f);
            _undo.rectTransform.anchorMax = new Vector2(0.6f, 0.15f);
        }

        public void Show(GameplayController game)
        {
            _game = game;
            _canvas.gameObject.SetActive(true);
            Refresh();
        }

        public void Hide()
        {
            if (_canvas != null) _canvas.gameObject.SetActive(false);
        }

        public void Refresh()
        {
            if (_game == null) return;
            UiKit.SetText(_progress, $"{_game.BoxesDone}/{_game.Quota} boxes");
            string extra = _game.ExtraSlotUsed ? "Slot used" : "Watch ad: extra slot";
            UiKit.SetText(_undo, _game.FreeUndos > 0 ? $"Free undos: {_game.FreeUndos}" : extra);
        }
    }
}
