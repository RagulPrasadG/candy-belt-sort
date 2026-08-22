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

        const float CardHeight = 440f;
        const float CardGap = 24f;

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

            UiKit.Button(_canvas.transform, "Play", "PLAY", Palette.Good, PlayNext, new Vector2(0.08f, 0.70f), new Vector2(0.92f, 0.81f));

            var how = UiKit.Label(_canvas.transform, "How", "Tap the glowing candy, or tap a box to send it.\nWrong empty box locks the wrong color.", 32, Palette.Ink, TextAnchor.MiddleCenter);
            how.rectTransform.anchorMin = new Vector2(0.06f, 0.60f);
            how.rectTransform.anchorMax = new Vector2(0.94f, 0.70f);

            UiKit.Button(_canvas.transform, "Settings", "Settings", Palette.Hex("8D6E63"), () => SettingsView.I.Show(), new Vector2(0.7f, 0.015f), new Vector2(0.97f, 0.07f));

            BuildLevelScroll();
        }

        void BuildLevelScroll()
        {
            var scrollGo = new GameObject("Scroll");
            scrollGo.transform.SetParent(_canvas.transform, false);
            var scrollRt = scrollGo.AddComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0.04f, 0.09f);
            scrollRt.anchorMax = new Vector2(0.96f, 0.59f);
            scrollRt.offsetMin = Vector2.zero;
            scrollRt.offsetMax = Vector2.zero;

            var scroll = scrollGo.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 40f;

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            viewport.transform.SetParent(scrollGo.transform, false);
            var viewportRt = viewport.GetComponent<RectTransform>();
            viewportRt.anchorMin = Vector2.zero;
            viewportRt.anchorMax = Vector2.one;
            viewportRt.offsetMin = Vector2.zero;
            viewportRt.offsetMax = Vector2.zero;
            var viewportImage = viewport.GetComponent<Image>();
            viewportImage.color = new Color(1f, 1f, 1f, 0.04f);
            viewportImage.raycastTarget = true;
            scroll.viewport = viewportRt;

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            float totalH = WorldCatalog.WorldCount * (CardHeight + CardGap) + 16f;
            contentRt.sizeDelta = new Vector2(0f, totalH);
            contentRt.anchoredPosition = Vector2.zero;
            scroll.content = contentRt;

            for (int w = 0; w < WorldCatalog.WorldCount; w++)
                BuildWorldCard(contentRt, w);
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

        void BuildWorldCard(RectTransform parent, int worldIndex)
        {
            var world = WorldCatalog.Get(worldIndex);
            var card = new GameObject($"World_{worldIndex}", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(parent, false);
            var rt = card.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, CardHeight);
            rt.anchoredPosition = new Vector2(0f, -16f - worldIndex * (CardHeight + CardGap));
            card.GetComponent<Image>().color = Color.Lerp(world.Accent, Color.white, 0.45f);

            var title = UiKit.Label(card.transform, "Name", world.Name, 48, Palette.Ink, TextAnchor.UpperLeft);
            title.rectTransform.anchorMin = new Vector2(0.04f, 0.78f);
            title.rectTransform.anchorMax = new Vector2(0.96f, 0.96f);

            var tag = UiKit.Label(card.transform, "Tag", world.Tagline, 28, Palette.Ink, TextAnchor.UpperLeft);
            tag.rectTransform.anchorMin = new Vector2(0.04f, 0.64f);
            tag.rectTransform.anchorMax = new Vector2(0.96f, 0.78f);

            for (int i = 0; i < WorldCatalog.LevelsPerWorld; i++)
            {
                int levelIndex = worldIndex * WorldCatalog.LevelsPerWorld + i;
                int col = i % 6;
                int row = i / 6;
                float x0 = 0.04f + col * 0.16f;
                float y1 = 0.56f - row * 0.26f;
                int captured = levelIndex;
                var btn = UiKit.Button(card.transform, $"L{i}", (i + 1).ToString(), world.Accent, () => TryPlay(captured), new Vector2(x0, y1 - 0.22f), new Vector2(x0 + 0.14f, y1));
                var image = btn.GetComponent<Image>();
                _dots.Add(new LevelDot { Button = btn, Image = image, Index = levelIndex, Accent = world.Accent });
            }
        }

        void PlayNext()
        {
            TryPlay(ProgressSave.HighestUnlocked);
        }

        void TryPlay(int levelIndex)
        {
            if (!ProgressSave.IsUnlocked(levelIndex)) return;
            GameFlow.I.PlayLevel(levelIndex);
        }
    }
}
