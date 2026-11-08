using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Millionalite.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Millionalite.Unity
{
    /// <summary>
    /// millionalite の起点。空のシーンにこのコンポーネントが1つあれば動く。
    /// 画面はすべてコードから組み立てるので、シーンに UI を並べる必要はない。
    ///
    /// 現在の実装範囲は Phase 1（素の大富豪1ラウンド）。
    /// コイン・21ラウンド進行・ショップ・パッシブ・ボスは未実装。
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        public const string Version = "v0.3.0";
        public const string PhaseLabel = "PHASE 1 — 大富豪エンジン";

        private const float CpuDelay = 0.75f;
        private const int TotalRounds = 21;

        private static readonly string[] Names = { "あなた", "CPU 甲", "CPU 乙", "CPU 丙" };

        // ラン全体の状態
        private List<Card> _playerDeck;
        private int _roundNumber;
        private int _coins;

        // 対局の状態
        private DaifugoRound _round;
        private int _selectedCardId = -1;
        private Coroutine _cpuRoutine;
        private readonly List<string> _log = new List<string>();

        // 画面
        private GameObject _titleRoot;
        private GameObject _gameRoot;
        private GameObject _overlayRoot;
        private Text _topBarText;
        private Text _turnNote;
        private Text _handLabel;
        private Transform _opponentRow;
        private Transform _fieldSlot;
        private Transform _handRow;
        private Transform _rankList;
        private Button _playButton;
        private Button _passButton;

        private void Awake()
        {
            EnsureCamera();
            EnsureEventSystem();
            BuildUi();
            ShowTitle();
        }

        private void OnDestroy()
        {
            if (_cpuRoutine != null) StopCoroutine(_cpuRoutine);
        }

        // ---------- 下ごしらえ ----------

        private static void EnsureCamera()
        {
            if (Camera.main != null) return;

            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            var camera = go.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Theme.Bg;
            camera.orthographic = true;
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;

            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        private void BuildUi()
        {
            var canvasGo = new GameObject("Canvas", typeof(RectTransform));
            canvasGo.transform.SetParent(transform, false);

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            var background = UiFactory.Box("Background", canvasGo.transform, Theme.Bg);
            UiFactory.Stretch(background);

            BuildTitleScreen(canvasGo.transform);
            BuildGameScreen(canvasGo.transform);
            BuildRoundEndOverlay(canvasGo.transform);
        }

        // ---------- タイトル画面 ----------

        private void BuildTitleScreen(Transform parent)
        {
            _titleRoot = UiFactory.Create("TitleScreen", parent);
            UiFactory.Stretch(_titleRoot);
            UiFactory.VerticalLayout(_titleRoot, 18, new RectOffset(60, 60, 120, 120), TextAnchor.MiddleCenter);

            var suits = UiFactory.Label("Suits", _titleRoot.transform, "♠   ♥   ♦   ♣", 54, Theme.BloodHi);
            UiFactory.Size(suits.gameObject, 80f);

            var logo = UiFactory.Label("Logo", _titleRoot.transform, "millionalite", 128, Theme.Gold,
                TextAnchor.MiddleCenter, FontStyle.Bold);
            UiFactory.Size(logo.gameObject, 170f);

            var tagline = UiFactory.Label("Tagline", _titleRoot.transform, "大富豪 × ローグライト", 36, Theme.InkDim);
            UiFactory.Size(tagline.gameObject, 60f);

            var spacer = UiFactory.Create("Spacer", _titleRoot.transform);
            UiFactory.Size(spacer, 40f);

            var start = UiFactory.TextButton("StartButton", _titleRoot.transform, "ゲーム開始", 44, 120f, true);
            start.onClick.AddListener(StartRun);

            AddLockedButton("デッキ選択");
            AddLockedButton("実績");
            AddLockedButton("設定");

            var version = UiFactory.Label("Version", _titleRoot.transform, Version + " · " + PhaseLabel, 26, Theme.GoldDim);
            UiFactory.Size(version.gameObject, 60f);
        }

        private void AddLockedButton(string text)
        {
            var button = UiFactory.TextButton(text + "Button", _titleRoot.transform, text, 38, 110f);
            button.interactable = false;   // Phase 7 で解放
        }

        // ---------- 対局画面 ----------

        private void BuildGameScreen(Transform parent)
        {
            _gameRoot = UiFactory.Create("GameScreen", parent);
            UiFactory.Stretch(_gameRoot);
            UiFactory.VerticalLayout(_gameRoot, 20, new RectOffset(36, 36, 48, 48));

            var topBar = UiFactory.Box("TopBar", _gameRoot.transform, Theme.BgRaise);
            UiFactory.Size(topBar, 92f);
            _topBarText = UiFactory.Label("TopBarText", topBar.transform, "", 32, Theme.Ink);
            UiFactory.Stretch(_topBarText.gameObject);

            var opponents = UiFactory.Create("Opponents", _gameRoot.transform);
            UiFactory.HorizontalLayout(opponents, 14, new RectOffset(0, 0, 0, 0));
            UiFactory.Size(opponents, 150f);
            _opponentRow = opponents.transform;

            var board = UiFactory.Box("Board", _gameRoot.transform, Theme.BgRaise);
            UiFactory.Size(board, 420f, 1f);
            UiFactory.VerticalLayout(board, 24, new RectOffset(24, 24, 24, 24), TextAnchor.MiddleCenter);

            var fieldSlot = UiFactory.Create("FieldSlot", board.transform);
            var fieldLayout = UiFactory.HorizontalLayout(fieldSlot, 12, new RectOffset(0, 0, 0, 0));
            // 場のカードは自分のサイズを保つ（横いっぱいに引き伸ばさない）
            fieldLayout.childControlWidth = false;
            fieldLayout.childForceExpandWidth = false;
            UiFactory.Size(fieldSlot, CardView.FieldSize.y);
            _fieldSlot = fieldSlot.transform;

            _turnNote = UiFactory.Label("TurnNote", board.transform, "", 30, Theme.InkDim);
            UiFactory.Size(_turnNote.gameObject, 50f);

            _handLabel = UiFactory.Label("HandLabel", _gameRoot.transform, "", 28, Theme.InkDim);
            UiFactory.Size(_handLabel.gameObject, 44f);

            var hand = UiFactory.Create("Hand", _gameRoot.transform);
            var grid = hand.AddComponent<GridLayoutGroup>();
            grid.cellSize = CardView.HandSize;
            grid.spacing = new Vector2(14f, 14f);
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 6;
            UiFactory.Size(hand, CardView.HandSize.y * 2f + 14f);
            _handRow = hand.transform;

            var actions = UiFactory.Create("Actions", _gameRoot.transform);
            UiFactory.HorizontalLayout(actions, 24, new RectOffset(0, 0, 0, 0));
            UiFactory.Size(actions, 130f);

            _playButton = UiFactory.TextButton("PlayButton", actions.transform, "出す", 40, 120f, true);
            _playButton.onClick.AddListener(OnPlayClicked);

            _passButton = UiFactory.TextButton("PassButton", actions.transform, "パス", 40, 120f);
            _passButton.onClick.AddListener(OnPassClicked);
        }

        private void BuildRoundEndOverlay(Transform parent)
        {
            _overlayRoot = UiFactory.Box("RoundEndOverlay", parent, Theme.WithAlpha(Color.black, 0.88f));
            UiFactory.Stretch(_overlayRoot);
            // 背面のカードを触れないようにクリックを受け止める
            _overlayRoot.GetComponent<Image>().raycastTarget = true;
            UiFactory.VerticalLayout(_overlayRoot, 0, new RectOffset(80, 80, 0, 0), TextAnchor.MiddleCenter);

            var box = UiFactory.Box("Box", _overlayRoot.transform, Theme.BgPanel);
            UiFactory.VerticalLayout(box, 18, new RectOffset(40, 40, 40, 40), TextAnchor.UpperCenter);
            UiFactory.Size(box, 760f);

            var outline = box.AddComponent<Outline>();
            outline.effectColor = Theme.GoldDim;
            outline.effectDistance = new Vector2(2f, -2f);

            var title = UiFactory.Label("Title", box.transform, "ラウンド終了", 52, Theme.Gold);
            UiFactory.Size(title.gameObject, 80f);

            var list = UiFactory.Create("RankList", box.transform);
            UiFactory.VerticalLayout(list, 8, new RectOffset(0, 0, 0, 0));
            UiFactory.Size(list, 280f);
            _rankList = list.transform;

            var again = UiFactory.TextButton("AgainButton", box.transform, "もう一度", 40, 110f, true);
            again.onClick.AddListener(StartRound);

            var back = UiFactory.TextButton("TitleButton", box.transform, "タイトルへ", 40, 110f);
            back.onClick.AddListener(ShowTitle);
        }

        // ---------- 画面遷移 ----------

        private void ShowTitle()
        {
            if (_cpuRoutine != null) { StopCoroutine(_cpuRoutine); _cpuRoutine = null; }
            _round = null;
            _titleRoot.SetActive(true);
            _gameRoot.SetActive(false);
            _overlayRoot.SetActive(false);
        }

        private void StartRun()
        {
            Debug.Log("start");
            DeckFactory.ResetIds();
            _playerDeck = DeckFactory.BuildPlayerDeck();   // スタンダード・ブルー 52枚 + ジョーカー1枚
            _roundNumber = 1;                              // 21ラウンド進行は Phase 2
            _coins = 0;
            _log.Clear();
            StartRound();
        }

        private void StartRound()
        {
            if (_cpuRoutine != null) { StopCoroutine(_cpuRoutine); _cpuRoutine = null; }

            _round = new DaifugoRound(new System.Random());
            _round.Logged += OnLogged;
            _round.Deal(Names, _playerDeck, DeckFactory.BuildCpuDeck);

            _selectedCardId = -1;
            _titleRoot.SetActive(false);
            _gameRoot.SetActive(true);
            _overlayRoot.SetActive(false);

            OnLogged($"ラウンド {_roundNumber} 開始");
            Render();
            ScheduleCpu();
        }

        private void OnLogged(string message)
        {
            _log.Add(message);
            Debug.Log("[log] " + message);
        }

        // ---------- 手番 ----------

        private bool IsMyTurn()
        {
            return _round != null && !_round.IsOver && _round.Turn == 0 && !_round.Players[0].HasFinished;
        }

        private void OnCardClicked(Card card)
        {
            if (!IsMyTurn()) return;
            _selectedCardId = _selectedCardId == card.Id ? -1 : card.Id;
            Render();
        }

        private void OnPlayClicked()
        {
            if (!IsMyTurn() || _selectedCardId < 0) return;

            var me = _round.Players[0];
            var card = me.Hand.FirstOrDefault(c => c.Id == _selectedCardId);
            if (card == null || !_round.CanPlay(card)) return;

            _round.Play(me, card);
            AfterAction();
        }

        private void OnPassClicked()
        {
            if (!IsMyTurn() || _round.FieldCard == null) return;

            _round.Pass(_round.Players[0]);
            AfterAction();
        }

        private void AfterAction()
        {
            _selectedCardId = -1;
            Render();

            if (_round.IsOver) ShowRoundEnd();
            else ScheduleCpu();
        }

        private void ScheduleCpu()
        {
            if (_cpuRoutine != null) { StopCoroutine(_cpuRoutine); _cpuRoutine = null; }
            if (_round == null || _round.IsOver) return;
            if (!_round.CurrentPlayer.IsCpu) return;

            _cpuRoutine = StartCoroutine(CpuTurn());
        }

        private IEnumerator CpuTurn()
        {
            yield return new WaitForSeconds(CpuDelay);
            _cpuRoutine = null;

            if (_round == null || _round.IsOver) yield break;

            var player = _round.CurrentPlayer;
            if (!player.IsCpu) yield break;

            var card = SimpleCpu.ChooseCard(_round, player);
            if (card != null) _round.Play(player, card);
            else _round.Pass(player);

            AfterAction();
        }

        // ---------- 描画 ----------

        private void Render()
        {
            if (_round == null) return;

            _topBarText.text = $"ROUND {_roundNumber} / {TotalRounds}        WAVE {_round.Wave}        ● {_coins}";
            RenderOpponents();
            RenderField();
            RenderHand();

            var selected = _selectedCardId >= 0;
            _playButton.interactable = IsMyTurn() && selected;
            _passButton.interactable = IsMyTurn() && _round.FieldCard != null;
        }

        private void RenderOpponents()
        {
            UiFactory.Clear(_opponentRow);

            foreach (var player in _round.Players.Where(p => p.Index != 0))
            {
                var panel = UiFactory.Box("Opp_" + player.Index, _opponentRow, Theme.BgRaise);
                UiFactory.VerticalLayout(panel, 6, new RectOffset(8, 8, 16, 16), TextAnchor.MiddleCenter);

                var outline = panel.AddComponent<Outline>();
                var isTurn = !_round.IsOver && _round.Turn == player.Index;
                outline.effectColor = isTurn ? Theme.Gold : Theme.Line;
                outline.effectDistance = new Vector2(2f, -2f);

                var name = UiFactory.Label("Name", panel.transform, player.Name, 28, Theme.Ink);
                UiFactory.Size(name.gameObject, 36f);

                string stateText;
                Color stateColor;
                if (player.HasFinished)
                {
                    stateText = DaifugoRound.RankTitles[player.Rank - 1];
                    stateColor = Theme.Gold;
                }
                else if (player.Passed)
                {
                    stateText = $"パス（手札 {player.Hand.Count}）";
                    stateColor = Theme.BloodHi;
                }
                else
                {
                    stateText = $"手札 {player.Hand.Count} ／ 山札 {player.Library.Count}";
                    stateColor = Theme.InkDim;
                }

                var state = UiFactory.Label("State", panel.transform, stateText, 24, stateColor);
                UiFactory.Size(state.gameObject, 32f);
            }
        }

        private void RenderField()
        {
            UiFactory.Clear(_fieldSlot);

            if (_round.FieldCard != null)
            {
                CardView.Create(_round.FieldCard, _fieldSlot, CardView.FieldSize);
            }
            else
            {
                var note = UiFactory.Label("Empty", _fieldSlot, "場は空 — 好きなカードを出せます", 28, Theme.InkDim);
                UiFactory.Size(note.gameObject, 60f);
            }

            _turnNote.text = BuildTurnNote();
        }

        private string BuildTurnNote()
        {
            var me = _round.Players[0];

            if (_round.IsOver) return "ラウンド終了";
            if (me.HasFinished) return $"あなたは上がり（{DaifugoRound.RankTitles[me.Rank - 1]}）— 決着を待っています";
            if (!IsMyTurn()) return $"{_round.CurrentPlayer.Name} の番…";

            return _round.FieldCard != null
                ? $"あなたの番 — {_round.FieldCard.DisplayName} より強い1枚、または パス"
                : "あなたの番 — 好きな1枚を出してください";
        }

        private void RenderHand()
        {
            UiFactory.Clear(_handRow);

            var me = _round.Players[0];
            _handLabel.text = $"自分の手札 {me.Hand.Count} 枚　／　山札 {me.Library.Count} 枚";

            foreach (var card in me.Hand)
            {
                var captured = card;
                CardView.Create(
                    card,
                    _handRow,
                    CardView.HandSize,
                    IsMyTurn() && _round.CanPlay(card),
                    _selectedCardId == card.Id,
                    () => OnCardClicked(captured));
            }
        }

        private void ShowRoundEnd()
        {
            UiFactory.Clear(_rankList);

            foreach (var player in _round.Players.OrderBy(p => p.Rank))
            {
                var row = UiFactory.Create("Row_" + player.Index, _rankList);
                UiFactory.HorizontalLayout(row, 12, new RectOffset(8, 8, 0, 0));
                UiFactory.Size(row, 56f);

                var title = UiFactory.Label("Title", row.transform,
                    DaifugoRound.RankTitles[player.Rank - 1], 32, Theme.Gold, TextAnchor.MiddleLeft);
                UiFactory.Size(title.gameObject, 56f);

                var name = UiFactory.Label("Name", row.transform, player.Name, 32,
                    player.Index == 0 ? Theme.GoldHi : Theme.Ink, TextAnchor.MiddleRight);
                UiFactory.Size(name.gameObject, 56f);
            }

            _overlayRoot.SetActive(true);
        }
    }
}
