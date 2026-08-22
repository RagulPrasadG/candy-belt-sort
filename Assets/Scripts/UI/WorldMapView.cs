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

        const float CardHeight = 560f;
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
            var header = UiKit.Panel(_canvas.transform, "Header", new Vector2(0f, 0.9f), Vector2.one, Palette.Hex("FF6F91"));
            UiKit.Label(header.transform, "Title", "Select a Level", 54, Color.white);

            UiKit.Button(_canvas.transform, "Back", "Menu", Palette.Hex("8D6E63"), () => GameFlow.I.ShowMainMenu(),
                new Vector2(0.03f, 0.915f), new Vector2(0.2f, 0.975f));
            UiKit.Button(_canvas.transform, "Continue", "Continue", Palette.Good, PlayNext,
                new Vector2(0.7f, 0.915f), new Vector2(0.97f, 0.975f));

            _coins = UiKit.Label(_canvas.transform, "Coins", "Coins 0", 34, Palette.Ink, TextAnchor.MiddleLeft);
            _coins.rectTransform.anchorMin = new Vector2(0.04f, 0.85f);
            _coins.rectTransform.anchorMax = new Vector2(0.5f, 0.9f);

            _streak = UiKit.Label(_canvas.transform, "Streak", "Streak 1", 34, Palette.Ink, TextAnchor.MiddleRight);
            _streak.rectTransform.anchorMin = new Vector2(0.5f, 0.85f);
            _streak.rectTransform.anchorMax = new Vector2(0.96f, 0.9f);

            BuildLevelScroll();
        }

        void BuildLevelScroll()
        {
            var scrollGo = new GameObject("Scroll");
            scrollGo.transform.SetParent(_canvas.transform, false);
            var scrollRt = scrollGo.AddComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0.03f, 0.02f);
            scrollRt.anchorMax = new Vector2(0.97f, 0.84f);
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

            var cardImage = card.GetComponent<Image>();
            var themeBg = SpriteFactory.TryNamed("bg_" + world.Theme);
            if (themeBg != null)
            {
                cardImage.sprite = themeBg;
                cardImage.type = Image.Type.Simple;
                cardImage.preserveAspect = false;
                cardImage.color = Color.white;
                // Legibility scrim so labels/buttons read over the art.
                UiKit.Panel(card.transform, "Scrim", Vector2.zero, Vector2.one, new Color(1f, 1f, 1f, 0.12f));
            }
            else
            {
                cardImage.color = Color.Lerp(world.Accent, Color.white, 0.45f);
            }

            var titleBand = UiKit.Panel(card.transform, "TitleBand", new Vector2(0f, 0.82f), new Vector2(1f, 1f),
                new Color(world.Accent.r, world.Accent.g, world.Accent.b, 0.85f));

            var title = UiKit.Label(titleBand.transform, "Name", $"World {worldIndex + 1} — {world.Name}", 40, Color.white, TextAnchor.MiddleLeft);
            title.rectTransform.anchorMin = new Vector2(0.04f, 0.45f);
            title.rectTransform.anchorMax = new Vector2(0.96f, 0.95f);
            title.fontStyle = FontStyle.Bold;

            var tag = UiKit.Label(titleBand.transform, "Tag", world.Tagline, 26, Palette.Paper, TextAnchor.MiddleLeft);
            tag.rectTransform.anchorMin = new Vector2(0.04f, 0.05f);
            tag.rectTransform.anchorMax = new Vector2(0.96f, 0.5f);

            const int cols = 5;
            for (int i = 0; i < WorldCatalog.LevelsPerWorld; i++)
            {
                int levelIndex = worldIndex * WorldCatalog.LevelsPerWorld + i;
                int col = i % cols;
                int row = i / cols;
                float x0 = 0.06f + col * 0.176f;
                float yTop = 0.7f - row * 0.165f;
                int captured = levelIndex;
                var btn = UiKit.Button(card.transform, $"L{i}", (i + 1).ToString(), world.Accent, () => TryPlay(captured),
                    new Vector2(x0, yTop - 0.13f), new Vector2(x0 + 0.14f, yTop));
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
