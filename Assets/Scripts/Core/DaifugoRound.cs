using System;
using System.Collections.Generic;
using System.Linq;

namespace Millionalite.Core
{
    /// <summary>
    /// 大富豪の1ラウンド。UnityEngine には依存しないので単体で検証できる。
    ///
    /// Phase 1 の範囲:
    ///   - 出し方はシングル（1枚出し）のみ
    ///   - 場が空なら何でも、場に札があればより強い1枚だけ
    ///   - 一度パスした人は場が流れるまで参加できない
    ///   - 全員パスで場が流れてウェーブが進む
    ///   - 3人が上がったら残った1人が大貧民
    /// </summary>
    public sealed class DaifugoRound
    {
        public const int HandSize = 9;

        public static readonly string[] RankTitles = { "大富豪", "富豪", "貧民", "大貧民" };

        private readonly List<PlayerState> _players = new List<PlayerState>();
        private readonly List<int> _finishedOrder = new List<int>();
        private readonly Random _rng;

        public IReadOnlyList<PlayerState> Players => _players;
        public IReadOnlyList<int> FinishedOrder => _finishedOrder;

        /// <summary>場のカード。null なら場は空。</summary>
        public Card FieldCard { get; private set; }

        /// <summary>場に出した人。場が空のときは意味を持たない。</summary>
        public int FieldOwner { get; private set; }

        public int Wave { get; private set; }
        public int Turn { get; private set; }
        public bool IsOver { get; private set; }

        /// <summary>進行ログ。UI側でメモ帳に流す。</summary>
        public event Action<string> Logged;

        public DaifugoRound(Random rng)
        {
            _rng = rng ?? throw new ArgumentNullException(nameof(rng));
        }

        /// <summary>
        /// ラウンドを開始する。各自の山札をシャッフルし、上から9枚ずつ取る。
        /// playerDeck はプレイヤーのデッキ（ラン中ずっと保持しているもの）。
        /// </summary>
        public void Deal(IReadOnlyList<string> names, IReadOnlyList<Card> playerDeck, Func<List<Card>> cpuDeckFactory)
        {
            if (names == null) throw new ArgumentNullException(nameof(names));
            if (playerDeck == null) throw new ArgumentNullException(nameof(playerDeck));
            if (cpuDeckFactory == null) throw new ArgumentNullException(nameof(cpuDeckFactory));

            _players.Clear();
            _finishedOrder.Clear();

            for (var i = 0; i < names.Count; i++)
            {
                var player = new PlayerState(i, names[i], i != 0);
                var library = i == 0 ? new List<Card>(playerDeck) : cpuDeckFactory();
                DeckFactory.Shuffle(library, _rng);

                player.Hand.AddRange(library.Take(HandSize));
                library.RemoveRange(0, HandSize);
                SortHand(player.Hand);
                player.Library.AddRange(library);

                _players.Add(player);
            }

            FieldCard = null;
            FieldOwner = 0;
            Wave = 1;
            Turn = 0;
            IsOver = false;
        }

        public static void SortHand(List<Card> hand)
        {
            hand.Sort((a, b) => a.Rank != b.Rank ? a.Rank - b.Rank : a.Suit.CompareTo(b.Suit));
        }

        public PlayerState CurrentPlayer => _players[Turn];

        public IEnumerable<PlayerState> ActivePlayers => _players.Where(p => !p.HasFinished);

        /// <summary>場が空なら何でも、場に札があれば「より強い1枚」だけ出せる。</summary>
        public bool CanPlay(Card card)
        {
            if (card == null) return false;
            return FieldCard == null || card.Rank > FieldCard.Rank;
        }

        public void Play(PlayerState player, Card card)
        {
            if (IsOver) return;
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (player.Index != Turn) throw new InvalidOperationException($"{player.Name} の手番ではない");
            if (card == null) throw new ArgumentNullException(nameof(card));
            // 状態を触る前に全部弾く。先に手札から抜くと、弾いたときに手札が消えてしまう
            if (!player.Hand.Contains(card)) throw new InvalidOperationException("手札にないカード");
            if (!CanPlay(card)) throw new InvalidOperationException($"{card} は場に出せない");

            player.Hand.Remove(card);
            FieldCard = card;
            FieldOwner = player.Index;
            Log($"{player.Name} が {card.DisplayName} を出した");

            if (player.Hand.Count == 0) Finish(player);
            AfterAction();
        }

        public void Pass(PlayerState player)
        {
            if (IsOver) return;
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (player.Index != Turn) throw new InvalidOperationException($"{player.Name} の手番ではない");
            if (FieldCard == null) throw new InvalidOperationException("場が空のときはパスできない");

            player.Passed = true;
            Log($"{player.Name} がパス");
            AfterAction();
        }

        private void Finish(PlayerState player)
        {
            player.Rank = _finishedOrder.Count + 1;
            _finishedOrder.Add(player.Index);
            Log($"{player.Name} が上がり（{RankTitles[player.Rank - 1]}）");
        }

        private void AfterAction()
        {
            if (ActivePlayers.Count() <= 1)
            {
                EndRound();
                return;
            }

            if (ShouldClearField()) EndWave();
            else AdvanceTurn();
        }

        /// <summary>場に出した本人以外が全員パス（または上がり）なら場が流れる。</summary>
        private bool ShouldClearField()
        {
            if (FieldCard == null) return false;

            var contenders = _players.Where(p => !p.HasFinished && !p.Passed).ToList();
            if (contenders.Count == 0) return true;
            return contenders.Count == 1 && contenders[0].Index == FieldOwner;
        }

        private void EndWave()
        {
            var owner = FieldCard != null ? FieldOwner : Turn;
            FieldCard = null;
            Wave++;
            foreach (var p in _players) p.Passed = false;

            // 最後に出した人から。上がっていれば次の生存者から
            Turn = _players[owner].HasFinished ? NextActiveFrom(owner) : owner;
            Log($"場が流れた（ウェーブ {Wave}）");
        }

        private int NextActiveFrom(int from)
        {
            for (var i = 1; i <= _players.Count; i++)
            {
                var index = (from + i) % _players.Count;
                if (!_players[index].HasFinished) return index;
            }

            return from;
        }

        private void AdvanceTurn()
        {
            for (var i = 1; i <= _players.Count; i++)
            {
                var index = (Turn + i) % _players.Count;
                var p = _players[index];
                if (!p.HasFinished && !p.Passed)
                {
                    Turn = index;
                    return;
                }
            }
        }

        private void EndRound()
        {
            var last = _players.FirstOrDefault(p => !p.HasFinished);
            if (last != null) Finish(last);   // 残った1人が大貧民

            IsOver = true;
            Log("ラウンド終了");
        }

        private void Log(string message) => Logged?.Invoke(message);
    }
}
