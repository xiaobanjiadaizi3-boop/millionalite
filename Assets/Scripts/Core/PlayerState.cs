using System.Collections.Generic;

namespace Millionalite.Core
{
    /// <summary>対局中の1人分の状態。山札は1人1デッキ。</summary>
    public sealed class PlayerState
    {
        public const int NotFinished = 0;

        public int Index { get; }
        public string Name { get; }
        public bool IsCpu { get; }

        /// <summary>手札。弱い順に並べて保持する。</summary>
        public List<Card> Hand { get; } = new List<Card>();

        /// <summary>自分の山札の残り。ウェーブごとの補充はしない（SPEC）。</summary>
        public List<Card> Library { get; } = new List<Card>();

        /// <summary>この場が流れるまでパスしたままかどうか。</summary>
        public bool Passed { get; set; }

        /// <summary>確定した順位。0 はまだ上がっていない。</summary>
        public int Rank { get; set; } = NotFinished;

        public bool HasFinished => Rank != NotFinished;

        public PlayerState(int index, string name, bool isCpu)
        {
            Index = index;
            Name = name;
            IsCpu = isCpu;
        }
    }
}
