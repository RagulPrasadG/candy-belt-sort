using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace CandyBeltSort
{
    public class HudView : MonoBehaviour
    {
        public static HudView I { get; private set; }

        Canvas _canvas;
        Text _progress;
        Text _undo;
        Text _hint;
        GameplayController _game;
        Coroutine _hintHide;

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

            _hint = UiKit.Label(_canvas.transform, "Hint", "", 32, Palette.Ink, TextAnchor.MiddleCenter);
            _hint.rectTransform.anchorMin = new Vector2(0.06f, 0.80f);
            _hint.rectTransform.anchorMax = new Vector2(0.94f, 0.89f);
        }

        public void Show(GameplayController game)
        {
            _game = game;
            _canvas.gameObject.SetActive(true);
            if (_hintHide != null) StopCoroutine(_hintHide);
            Refresh();
            ShowLevelHint();
        }

        public void Hide()
        {
            if (_hintHide != null)
            {
                StopCoroutine(_hintHide);
                _hintHide = null;
            }
            if (_canvas != null) _canvas.gameObject.SetActive(false);
        }

        public void Refresh()
        {
            if (_game == null) return;
            UiKit.SetText(_progress, $"{_game.BoxesDone}/{_game.Quota} boxes");
            string extra = _game.ExtraSlotUsed ? "Slot used" : "Watch ad: extra slot";
            UiKit.SetText(_undo, _game.FreeUndos > 0 ? $"Free undos: {_game.FreeUndos}" : extra);
        }

        void ShowLevelHint()
        {
            if (_hint == null) return;
            int level = _game != null && _game.Level != null ? _game.Level.Index : -1;
            if (level == 0)
            {
                _hint.gameObject.SetActive(true);
                _hint.text = "Only the glowing candy.\nTap a box to lock its color.";
                _hintHide = StartCoroutine(HideHintAfter(6.5f));
            }
            else if (level == 12)
            {
                _hint.gameObject.SetActive(true);
                _hint.text = "Two belts. Pick a glowing candy, then a box.";
                _hintHide = StartCoroutine(HideHintAfter(6.5f));
            }
            else
            {
                _hint.gameObject.SetActive(false);
            }
        }

        IEnumerator HideHintAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (_hint != null) _hint.gameObject.SetActive(false);
            _hintHide = null;
        }
    }
}
