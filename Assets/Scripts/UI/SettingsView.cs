using UnityEngine;
using UnityEngine.UI;

namespace CandyBeltSort
{
    public class SettingsView : MonoBehaviour
    {
        public static SettingsView I { get; private set; }

        Canvas _canvas;
        Text _sfx;
        Text _haptics;

        public void Build(Transform parent)
        {
            I = this;
            _canvas = UiKit.Canvas("Settings", parent);
            _canvas.sortingOrder = 50;
            _canvas.gameObject.SetActive(false);

            UiKit.Panel(_canvas.transform, "Dim", Vector2.zero, Vector2.one, new Color(0f, 0f, 0f, 0.45f));
            var card = UiKit.Panel(_canvas.transform, "Card", new Vector2(0.1f, 0.25f), new Vector2(0.9f, 0.75f), Palette.Paper);
            var title = UiKit.Label(card.transform, "Title", "Settings", 60, Palette.Ink);
            title.rectTransform.anchorMin = new Vector2(0.08f, 0.78f);
            title.rectTransform.anchorMax = new Vector2(0.92f, 0.96f);

            _sfx = MakeToggle(card.transform, "Sound", new Vector2(0.08f, 0.52f), new Vector2(0.92f, 0.7f), ToggleSfx);
            _haptics = MakeToggle(card.transform, "Haptics", new Vector2(0.08f, 0.32f), new Vector2(0.92f, 0.5f), ToggleHaptics);
            UiKit.Button(card.transform, "Close", "Close", Palette.Accent, Hide, new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.24f));
        }

        Text MakeToggle(Transform parent, string name, Vector2 min, Vector2 max, UnityEngine.Events.UnityAction click)
        {
            var btn = UiKit.Button(parent, name, name, Palette.Hex("AED581"), click, min, max);
            return btn.GetComponentInChildren<Text>();
        }

        public void Show()
        {
            Refresh();
            _canvas.gameObject.SetActive(true);
        }

        public void Hide() => _canvas.gameObject.SetActive(false);

        void ToggleSfx()
        {
            ProgressSave.SetSfx(!ProgressSave.SfxOn);
            Refresh();
        }

        void ToggleHaptics()
        {
            ProgressSave.SetHaptics(!ProgressSave.HapticsOn);
            Refresh();
        }

        void Refresh()
        {
            UiKit.SetText(_sfx, ProgressSave.SfxOn ? "Sound: On" : "Sound: Off");
            UiKit.SetText(_haptics, ProgressSave.HapticsOn ? "Haptics: On" : "Haptics: Off");
        }
    }
}
