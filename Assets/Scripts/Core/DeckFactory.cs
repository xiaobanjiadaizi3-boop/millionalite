using System;
using System.Collections.Generic;

namespace Millionalite.Core
{
    /// <summary>
    /// 山札を作る。山札は1人1デッキで、プレイヤーとCPUがそれぞれ自分の山札を持つ。
    /// </summary>
    public static class DeckFactory
    {
        private static readonly Suit[] Suits =
        {
            Suit.Hearts, Suit.Spades, Suit.Diamonds, Suit.Clubs
        };

        private static int _nextId;

        /// <summary>テストで通し番号を 1 から振り直す。</summary>
        public static void ResetIds() => _nextId = 0;

        public static Card NewCard(Suit suit, int rank) => new Card(++_nextId, suit, rank);

        /// <summary>通常のトランプ52枚（withJoker なら + ジョーカー1枚）。</summary>
        public static List<Card> BuildStandardDeck(bool withJoker)
        {
            var deck = new List<Card>(withJoker ? 53 : 52);
            foreach (var suit in Suits)
            {
                for (var rank = Card.LowestRank; rank <= Card.TwoRank; rank++)
                {
                    deck.Add(NewCard(suit, rank));
                }
            }

            if (withJoker)
            {
                deck.Add(NewCard(Suit.None, Card.JokerRank));
            }

            return deck;
        }

        /// <summary>プレイヤーのデッキ「スタンダード・ブルー」= 52枚 + ジョーカー1枚。</summary>
        public static List<Card> BuildPlayerDeck() => BuildStandardDeck(true);

        /// <summary>CPUのデッキ。1〜5Rは普通のトランプ52枚（SPEC 第XIII章）。</summary>
        public static List<Card> BuildCpuDeck() => BuildStandardDeck(false);

        /// <summary>フィッシャー–イェーツ。</summary>
        public static void Shuffle<T>(IList<T> items, Random rng)
        {
            if (rng == null) throw new ArgumentNullException(nameof(rng));

            for (var i = items.Count - 1; i > 0; i--)
            {
                var j = rng.Next(i + 1);
                (items[i], items[j]) = (items[j], items[i]);
            }
        }
    }
}
