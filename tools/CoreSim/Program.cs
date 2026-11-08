using System;
using System.Collections.Generic;
using System.Linq;
using Millionalite.Core;

namespace Millionalite.CoreSim
{
    /// <summary>
    /// Assets/Scripts/Core の対局ロジックを Unity なしで回す検証用プログラム。
    /// HTML版で通したのと同じ不変条件を確認する。
    ///   dotnet run --project tools/CoreSim [ラウンド数] [乱数シード]
    /// </summary>
    internal static class Program
    {
        private static readonly string[] Names = { "あなた", "CPU 甲", "CPU 乙", "CPU 丙" };

        private static int Main(string[] args)
        {
            var rounds = args.Length > 0 ? int.Parse(args[0]) : 200;
            var seed = args.Length > 1 ? int.Parse(args[1]) : 20260101;

            var rng = new Random(seed);
            var problems = new List<string>();
            var waves = new List<int>();
            var myRanks = new int[4];
            var roundsWithJoker = 0;

            DeckFactory.ResetIds();
            var playerDeck = DeckFactory.BuildPlayerDeck();   // ラン中ずっと保持するデッキ

            for (var r = 1; r <= rounds; r++)
            {
                var round = new DaifugoRound(rng);
                round.Deal(Names, playerDeck, DeckFactory.BuildCpuDeck);

                ValidateDeal(round, r, problems);
                if (round.Players[0].Hand.Any(c => c.IsJoker)) roundsWithJoker++;

                var steps = 0;
                while (!round.IsOver)
                {
                    if (++steps > 10_000)
                    {
                        problems.Add($"R{r}: 進行が止まった");
                        break;
                    }

                    var player = round.CurrentPlayer;
                    var card = SimpleCpu.ChooseCard(round, player);   // 人間側も同じ方針で自動打ち
                    if (card != null) round.Play(player, card);
                    else if (round.FieldCard != null) round.Pass(player);
                    else
                    {
                        problems.Add($"R{r}: 場が空なのに出せる札がない");
                        break;
                    }
                }

                ValidateEnd(round, r, problems);
                waves.Add(round.Wave);
                var myRank = round.Players[0].Rank;
                if (myRank >= 1 && myRank <= 4) myRanks[myRank - 1]++;
            }

            waves.Sort();
            Console.WriteLine($"ラウンド数      : {rounds}（シード {seed}）");
            Console.WriteLine($"不変条件の違反  : {problems.Count} 件");
            foreach (var p in problems.Take(10)) Console.WriteLine($"  - {p}");
            Console.WriteLine($"ウェーブ数      : 最小 {waves.First()} / 中央 {waves[waves.Count / 2]} / 最大 {waves.Last()}");
            Console.WriteLine($"自分の順位分布  : 大富豪 {myRanks[0]} / 富豪 {myRanks[1]} / 貧民 {myRanks[2]} / 大貧民 {myRanks[3]}");
            Console.WriteLine($"初手にJOKER     : {roundsWithJoker} 回（期待値 約 {rounds * 9.0 / 53.0:F0} 回）");

            return problems.Count == 0 ? 0 : 1;
        }

        private static void ValidateDeal(DaifugoRound round, int r, List<string> problems)
        {
            var ids = new HashSet<int>();
            var total = 0;

            foreach (var p in round.Players)
            {
                foreach (var c in p.Hand.Concat(p.Library))
                {
                    if (!ids.Add(c.Id)) problems.Add($"R{r}: カードIDが重複 ({c.Id})");
                    total++;
                }

                if (p.Hand.Count != DaifugoRound.HandSize)
                    problems.Add($"R{r}: {p.Name} の手札が {p.Hand.Count} 枚");

                var expect = p.Index == 0 ? 53 : 52;
                if (p.Hand.Count + p.Library.Count != expect)
                    problems.Add($"R{r}: {p.Name} のデッキ総数 {p.Hand.Count + p.Library.Count}（期待 {expect}）");

                var jokers = p.Hand.Concat(p.Library).Count(c => c.IsJoker);
                var expectJokers = p.Index == 0 ? 1 : 0;
                if (jokers != expectJokers)
                    problems.Add($"R{r}: {p.Name} のジョーカー {jokers} 枚（期待 {expectJokers}）");

                for (var i = 1; i < p.Hand.Count; i++)
                    if (p.Hand[i - 1].Rank > p.Hand[i].Rank)
                        problems.Add($"R{r}: {p.Name} の手札が弱い順に並んでいない");
            }

            if (total != 53 + 52 * 3) problems.Add($"R{r}: 卓上のカード総数 {total}");
            if (round.Wave != 1) problems.Add($"R{r}: 開始ウェーブ {round.Wave}");
            if (round.FieldCard != null) problems.Add($"R{r}: 開始時に場が空でない");
        }

        private static void ValidateEnd(DaifugoRound round, int r, List<string> problems)
        {
            var ranks = round.Players.Select(p => p.Rank).OrderBy(x => x).ToArray();
            if (!ranks.SequenceEqual(new[] { 1, 2, 3, 4 }))
                problems.Add($"R{r}: 順位が [{string.Join(",", ranks)}]");

            foreach (var p in round.Players.Where(p => p.Rank < 4))
                if (p.Hand.Count != 0)
                    problems.Add($"R{r}: {p.Name}（{p.Rank}位）に手札が {p.Hand.Count} 枚残っている");

            if (round.FinishedOrder.Count != 4)
                problems.Add($"R{r}: 上がり順の記録が {round.FinishedOrder.Count} 人分");
        }
    }
}
