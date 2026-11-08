using Millionalite.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Millionalite.Unity
{
    /// <summary>1枚のカードの見た目。手札でも場でも同じものを使う。</summary>
    public static class CardView
    {
        public static readonly Vector2 HandSize = new Vector2(150f, 215f);
        public static readonly Vector2 FieldSize = new Vector2(190f, 272f);

        /// <summary>
        /// カードを作る。onClick が null なら表示だけ（場のカードなど）。
        /// playable が false のカードは暗く表示し、押せなくする。
        /// </summary>
        public static GameObject Create(Card card, Transform parent, Vector2 size,
            bool playable = true, bool selected = false, UnityEngine.Events.UnityAction onClick = null)
        {
            var go = UiFactory.Create("Card_" + card.DisplayName, parent);
            var rect = UiFactory.Rect(go);
            rect.sizeDelta = size;

            var face = go.AddComponent<Image>();
            face.color = card.IsJoker ? Theme.CardFaceGold : Theme.CardFace;
            if (!playable) face.color = Color.Lerp(face.color, Theme.CardDisabled, 0.55f);

            if (selected) face.color = Color.Lerp(face.color, Theme.GoldHi, 0.5f);

            var outline = go.AddComponent<Outline>();
            outline.effectColor = selected ? Theme.Gold : Theme.CardInk;
            outline.effectDistance = selected ? new Vector2(6f, -6f) : new Vector2(2f, -2f);

            var inkColor = card.IsJoker
                ? Theme.Hex(0x4A3A12)
                : (card.IsRedSuit ? Theme.CardInkRed : Theme.CardInk);
            if (!playable) inkColor = Color.Lerp(inkColor, Theme.CardDisabled, 0.5f);

            var rankSize = card.IsJoker ? 28 : 54;
            var rank = UiFactory.Label("Rank", go.transform, card.RankLabel, rankSize, inkColor);
            var rankRect = UiFactory.Rect(rank.gameObject);
            rankRect.anchorMin = new Vector2(0f, 0.45f);
            rankRect.anchorMax = new Vector2(1f, 1f);
            rankRect.offsetMin = Vector2.zero;
            rankRect.offsetMax = Vector2.zero;

            var suit = UiFactory.Label("Suit", go.transform, card.SuitSymbol, 46, inkColor);
            var suitRect = UiFactory.Rect(suit.gameObject);
            suitRect.anchorMin = new Vector2(0f, 0f);
            suitRect.anchorMax = new Vector2(1f, 0.45f);
            suitRect.offsetMin = Vector2.zero;
            suitRect.offsetMax = Vector2.zero;

            // レイアウトグループに入れても潰れないよう、希望サイズを明示する
            var element = go.AddComponent<LayoutElement>();
            element.minWidth = size.x;
            element.preferredWidth = size.x;
            element.minHeight = size.y;
            element.preferredHeight = size.y;
            element.flexibleWidth = 0f;
            element.flexibleHeight = 0f;

            if (onClick != null)
            {
                var button = go.AddComponent<Button>();
                button.targetGraphic = face;
                button.interactable = playable;
                button.onClick.AddListener(onClick);
            }

            return go;
        }
    }
}
