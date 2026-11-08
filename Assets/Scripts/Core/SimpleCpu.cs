using System.Linq;

namespace Millionalite.Core
{
    /// <summary>Phase 1 の超簡易AI: 出せる中で最弱を出し、出せなければパス。</summary>
    public static class SimpleCpu
    {
        /// <summary>出すカードを選ぶ。null を返したらパス。</summary>
        public static Card ChooseCard(DaifugoRound round, PlayerState player)
        {
            // 手札は弱い順に並んでいるので、最初に見つかったものが最弱
            return player.Hand.FirstOrDefault(round.CanPlay);
        }
    }
}
