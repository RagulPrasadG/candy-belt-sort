using UnityEngine;
using UnityEngine.UI;

namespace CandyBeltSort
{
    public class WorldMapView : MonoBehaviour
    {
        public static WorldMapView I { get; private set; }

        Canvas _canvas;
        Text _coins;
        Text _streak;
        readonly System.Collections.Generic.List<LevelDot> _dots = new System.Collections.Generic.List<LevelDot>();

        struct LevelDot
        {
            public Button Button;
            public Image Image;
            public int Index;
            public Color Accent;
        }

        public void Build(Transform parent)
        {
            I = this;
            _canvas = UiKit.Canvas("WorldMap", parent);
            _canvas.sortingOrder = 20;

            UiKit.Panel(_canvas.transform, "Bg", Vector2.zero, Vector2.one, Palette.Hex("FFF3E8"));
            var header = UiKit.Panel(_canvas.transform, "Header", new Vector2(0f, 0.88f), Vector2.one, Palette.Hex("FF6F91"));
            UiKit.Label(header.transform, "Title", "Candy Belt Sort", 64, Color.white);

            _coins = UiKit.Label(_canvas.transform, "Coins", "Coins 0", 36, Palette.Ink, TextAnchor.MiddleLeft);
            _coins.rectTransform.anchorMin = new Vector2(0.04f, 0.82f);
            _coins.rectTransform.anchorMax = new Vector2(0.5f, 0.88f);

            _streak = UiKit.Label(_canvas.transform, "Streak", "Streak 1", 36, Palette.Ink, TextAnchor.MiddleRight);
            _streak.rectTransform.anchorMin = new Vector2(0.5f, 0.82f);
            _streak.rectTransform.anchorMax = new Vector2(0.96f, 0.88f);

            UiKit.Button(_canvas.transform, "Settings", "Settings", Palette.Hex("8D6E63"), () => SettingsView.I.Show(), new Vector2(0.7f, 0.015f), new Vector2(0.97f, 0.07f));

            var scrollGo = new GameObject("Scroll");
            scrollGo.transform.SetParent(_canvas.transform, false);
            var scrollRt = scrollGo.AddComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0.04f, 0.09f);
            scrollRt.anchorMax = new Vector2(0.96f, 0.81f);
            scrollRt.offsetMin = Vector2.zero;
            scrollRt.offsetMax = Vector2.zero;
            var scroll = scrollGo.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;

            var viewport = UiKit.Panel(scrollGo.transform, "Viewport", Vector2.zero, Vector2.one, new Color(0, 0, 0, 0));
            viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            scroll.viewport = viewport.rectTransform;

            var content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            var contentRt = content.AddComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.sizeDelta = new Vector2(0f, WorldCatalog.WorldCount * 520f);
            var layout = content.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.spacing = 24f;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = contentRt;

            for (int w = 0; w < WorldCatalog.WorldCount; w++)
                BuildWorldCard(content.transform, w);
        }

        public void Show()
        {
            Refresh();
            _canvas.gameObject.SetActive(true);
        }

        public void Hide() => _canvas.gameObject.SetActive(false);

        public void Refresh()
        {
            UiKit.SetText(_coins, $"Coins {ProgressSave.CoinsOnHand}");
            UiKit.SetText(_streak, $"Streak {ProgressSave.DailyStreak}");
            for (int i = 0; i < _dots.Count; i++)
            {
                var dot = _dots[i];
                bool unlocked = ProgressSave.IsUnlocked(dot.Index);
                dot.Button.interactable = unlocked;
                dot.Image.color = unlocked ? dot.Accent : Palette.Hex("B0BEC5");
            }
        }

        void BuildWorldCard(Transform parent, int worldIndex)
        {
            var world = WorldCatalog.Get(worldIndex);
            var card = new GameObject($"World_{worldIndex}");
            card.transform.SetParent(parent, false);
            var le = card.AddComponent<LayoutElement>();
            le.minHeight = 480f;
            le.preferredHeight = 480f;
            var bg = card.AddComponent<Image>();
            bg.color = Color.Lerp(world.Accent, Color.white, 0.55f);

            var title = UiKit.Label(card.transform, "Name", world.Name, 48, Palette.Ink, TextAnchor.UpperLeft);
            title.rectTransform.anchorMin = new Vector2(0.04f, 0.78f);
            title.rectTransform.anchorMax = new Vector2(0.96f, 0.98f);

            var tag = UiKit.Label(card.transform, "Tag", world.Tagline, 28, Palette.Ink, TextAnchor.UpperLeft);
            tag.rectTransform.anchorMin = new Vector2(0.04f, 0.66f);
            tag.rectTransform.anchorMax = new Vector2(0.96f, 0.8f);

            for (int i = 0; i < WorldCatalog.LevelsPerWorld; i++)
            {
                int levelIndex = worldIndex * WorldCatalog.LevelsPerWorld + i;
                int col = i % 6;
                int row = i / 6;
                float x0 = 0.04f + col * 0.16f;
                float y1 = 0.58f - row * 0.28f;
                var captured = levelIndex;
                var btn = UiKit.Button(card.transform, $"L{i}", (i + 1).ToString(), world.Accent, () => TryPlay(captured), new Vector2(x0, y1 - 0.22f), new Vector2(x0 + 0.14f, y1));
                var image = btn.GetComponent<Image>();
                _dots.Add(new LevelDot { Button = btn, Image = image, Index = levelIndex, Accent = world.Accent });
            }
        }

        void TryPlay(int levelIndex)
        {
            if (!ProgressSave.IsUnlocked(levelIndex)) return;
            GameFlow.I.PlayLevel(levelIndex);
        }
    }
}
