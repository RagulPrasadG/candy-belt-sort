using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CandyBeltSort
{
    public static class UiKit
    {
        public static Font Font;

        public static void Init()
        {
            Font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (Font == null) Font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        public static Canvas Canvas(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        public static Image Panel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.color = color;
            var rt = image.rectTransform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return image;
        }

        public static Text Label(Transform parent, string name, string text, int size, Color color, TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var ui = go.AddComponent<Text>();
            ui.font = Font;
            ui.text = text;
            ui.fontSize = size;
            ui.color = color;
            ui.alignment = anchor;
            ui.horizontalOverflow = HorizontalWrapMode.Wrap;
            ui.verticalOverflow = VerticalWrapMode.Overflow;
            var rt = ui.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return ui;
        }

        public static Button Button(Transform parent, string name, string text, Color bg, UnityAction click, Vector2 anchorMin, Vector2 anchorMax)
        {
            var image = Panel(parent, name, anchorMin, anchorMax, bg);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(click);
            Label(image.transform, "Label", text, 42, Color.white);
            return button;
        }

        public static void SetText(Text label, string value)
        {
            if (label != null) label.text = value;
        }
    }
}
