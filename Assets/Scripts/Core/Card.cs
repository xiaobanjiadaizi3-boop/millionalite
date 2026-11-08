using System;

namespace Millionalite.Core
{
    public enum Suit
    {
        None = 0,
        Hearts,
        Spades,
        Diamonds,
        Clubs
    }

    /// <summary>
    /// 1枚のカード。強さは数値で持つ: 3..14 が 3..A、15 が「2」、16 が JOKER。
    /// Id はカードごとの通し番号で、卓上で一意になる（将来シールや強化を個別に載せるため）。
    /// </summary>
    public sealed class Card
    {
        public const int LowestRank = 3;
        public const int TwoRank = 15;
        public const int JokerRank = 16;

        public int Id { get; }
        public Suit Suit { get; }
        public int Rank { get; }

        public Card(int id, Suit suit, int rank)
        {
            if (rank < LowestRank || rank > JokerRank)
            {
                throw new ArgumentOutOfRangeException(nameof(rank), rank, "ランクは 3〜16 の範囲");
            }

            Id = id;
            Suit = suit;
            Rank = rank;
        }

        public bool IsJoker => Rank == JokerRank;

        public string RankLabel
        {
            get
            {
                switch (Rank)
                {
                    case 11: return "J";
                    case 12: return "Q";
                    case 13: return "K";
                    case 14: return "A";
                    case TwoRank: return "2";
                    case JokerRank: return "JOKER";
                    default: return Rank.ToString();
                }
            }
        }

        public string SuitSymbol
        {
            get
            {
                switch (Suit)
                {
                    case Suit.Hearts: return "♥";
                    case Suit.Spades: return "♠";
                    case Suit.Diamonds: return "♦";
                    case Suit.Clubs: return "♣";
                    default: return "★";
                }
            }
        }

        /// <summary>赤いマーク（♥ / ♦）かどうか。表示色に使う。</summary>
        public bool IsRedSuit => Suit == Suit.Hearts || Suit == Suit.Diamonds;

        public string DisplayName => IsJoker ? "JOKER" : SuitSymbol + RankLabel;

        public override string ToString() => DisplayName;
    }
}
