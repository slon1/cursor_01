using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Config;
using Game.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.View
{
    public sealed class GameScreenPresenter : MonoBehaviour, IAppScreen
    {
        const float BoardSize = 912f;
        const float Gap = 8f;
        const float NativeCell = 176f;
        static readonly Color Cream = new Color(0.953f, 0.898f, 0.710f);

        [SerializeField] RectTransform stage;
        [SerializeField] Image background;
        [SerializeField] Image alarmFrame;
        [SerializeField] Image pauseImage;
        [SerializeField] Image barFrame;
        [SerializeField] Image alarmFill;
        [SerializeField] TMP_Text alarmLabel;
        [SerializeField] TMP_Text goldLabel;
        [SerializeField] Button pauseButton;
        [SerializeField] RectTransform board;
        [SerializeField] RectTransform highlightRoot;
        [SerializeField] RectTransform lionAToken;
        [SerializeField] RectTransform lionBToken;
        [SerializeField] Button lionAButton;
        [SerializeField] Image lionAButtonImage;
        [SerializeField] Button lionBButton;
        [SerializeField] Image lionBButtonImage;
        [SerializeField] Button extractButton;
        [SerializeField] Image extractImage;
        [SerializeField] GameObject endOverlay;
        [SerializeField] CanvasGroup endGroup;
        [SerializeField] TMP_Text endLabel;
        [SerializeField] Button endButton;
        [SerializeField] GameObject pauseOverlay;
        [SerializeField] Button resumeButton;
        [SerializeField] Button restartButton;
        [SerializeField] Button pauseMenuButton;
        [SerializeField] GameObject winOverlay;
        [SerializeField] TMP_Text winLoot;
        [SerializeField] TMP_Text winTarget;
        [SerializeField] Button winNext;
        [SerializeField] Button winSelect;
        [SerializeField] GameObject loseOverlay;
        [SerializeField] TMP_Text loseLoot;
        [SerializeField] TMP_Text loseTarget;
        [SerializeField] Button loseRetry;
        [SerializeField] Button loseSelect;

        public event System.Action<GameResult> Finished;

        GameSession _session;
        LevelConfig _level;
        AppContext _app;
        bool _popup;
        Vector2 _winNextHome;
        Vector2 _winSelectHome;
        SpriteCatalog _catalog;
        GameSettingsConfig _settings;
        EventPlayer _player;
        readonly Dictionary<CellPos, CellView> _cells = new Dictionary<CellPos, CellView>();
        CancellationTokenSource _play;
        bool _inputLocked;
        bool _moveAnimating;
        bool _pauseWhenIdle;
        float _cellSize = NativeCell;

        void Awake()
        {
            pauseButton.onClick.AddListener(() => { Tap(); OnPause(); });
            lionAButton.onClick.AddListener(() => { Tap(); OnSelectLion(LionId.A); });
            lionBButton.onClick.AddListener(() => { Tap(); OnSelectLion(LionId.B); });
            extractButton.onClick.AddListener(() => OnExtract().Forget());
            endButton.onClick.AddListener(() => { Tap(); OnRestart(); });
            if (resumeButton != null)
                resumeButton.onClick.AddListener(() => { Tap(); ClosePause(); });
            if (restartButton != null)
                restartButton.onClick.AddListener(() => { Tap(); RestartFromPause(); });
            if (pauseMenuButton != null)
                pauseMenuButton.onClick.AddListener(() => { Tap(); LeaveToMenu().Forget(); });
            if (winNext != null)
            {
                _winNextHome = ((RectTransform)winNext.transform).anchoredPosition;
                _winSelectHome = ((RectTransform)winSelect.transform).anchoredPosition;
                winNext.onClick.AddListener(() => { Tap(); OpenLevel(_level.Number + 1).Forget(); });
                winSelect.onClick.AddListener(() => { Tap(); OpenSelect().Forget(); });
                loseRetry.onClick.AddListener(() => { Tap(); OnRestart(); });
                loseSelect.onClick.AddListener(() => { Tap(); OpenSelect().Forget(); });
            }
        }

        void OnDestroy()
        {
            _play?.Cancel();
            _play?.Dispose();
            pauseButton.onClick.RemoveAllListeners();
            lionAButton.onClick.RemoveAllListeners();
            lionBButton.onClick.RemoveAllListeners();
            extractButton.onClick.RemoveAllListeners();
            endButton.onClick.RemoveAllListeners();
        }

        public void Open(AppContext app, object args)
        {
            _app = app;
            if (app.Run != null)
                Finished += app.Run.Apply;
            Show((LevelConfig)args, app.Catalog, app.Settings, app.Theme);
        }

        public void Show(LevelConfig level, SpriteCatalog catalog, GameSettingsConfig settings, UiTheme theme = null)
        {
            _level = level;
            _catalog = catalog;
            _settings = settings;
            _player = new EventPlayer(settings, ResolveSfx());
            _session = new GameSession(level.ToLevelData());
            _popup = false;
            endOverlay.SetActive(false);
            if (pauseOverlay != null)
                pauseOverlay.SetActive(false);
            if (winOverlay != null)
                winOverlay.SetActive(false);
            if (loseOverlay != null)
                loseOverlay.SetActive(false);
            ApplyChrome();
            if (theme != null)
            {
                alarmLabel.font = theme.Heading;
                goldLabel.font = theme.Heading;
                endLabel.font = theme.Heading;
                alarmLabel.color = theme.Cream;
                goldLabel.color = theme.Gold;
            }
            highlightRoot.gameObject.SetActive(false);
            RebuildBoard();
            Refresh(_session.State);
        }

        void RebuildBoard()
        {
            for (var i = board.childCount - 1; i >= 0; i--)
            {
                var child = board.GetChild(i);
                if (child == highlightRoot || child == lionAToken || child == lionBToken)
                    continue;
                Destroy(child.gameObject);
            }

            _cells.Clear();
            var state = _session.State;
            _cellSize = CellSize(state.Width, state.Height);
            var spanX = state.Width * _cellSize + (state.Width - 1) * Gap;
            var spanY = state.Height * _cellSize + (state.Height - 1) * Gap;
            var originX = -spanX * 0.5f + _cellSize * 0.5f;
            var originY = spanY * 0.5f - _cellSize * 0.5f;

            for (var y = 0; y < state.Height; y++)
            {
                for (var x = 0; x < state.Width; x++)
                {
                    var pos = new CellPos(x, y);
                    var view = CellView.Create(board);
                    view.Bind(pos);
                    var rect = (RectTransform)view.transform;
                    rect.sizeDelta = new Vector2(_cellSize, _cellSize);
                    rect.anchoredPosition = new Vector2(originX + x * (_cellSize + Gap), originY - y * (_cellSize + Gap));
                    view.Clicked += OnCellClicked;
                    _cells.Add(pos, view);
                }
            }

            FitToken(lionAToken, _catalog.Get(SpriteId.LionA_Token));
            FitToken(lionBToken, _catalog.Get(SpriteId.LionB_Token));
            OrderTokens(_session.ActiveLion);
        }

        void Refresh(GameState state)
        {
            foreach (var pair in _cells)
            {
                var cell = state.CellAt(pair.Key);
                pair.Value.SetVisual(Ground(cell), ItemSprite(cell));
            }

            PlaceToken(lionAToken, LionId.A, state);
            PlaceToken(lionBToken, LionId.B, state);
            OrderTokens(_session.ActiveLion);
            alarmLabel.text = state.Alarm + "/" + state.AlarmThreshold;
            alarmFill.fillAmount = FillOf(state.Alarm, state.AlarmThreshold);
            goldLabel.text = LootText(_session.CollectedCoins);
            lionAButtonImage.sprite = _catalog.Get(_session.ActiveLion == LionId.A ? SpriteId.LionA_Icon_Active : SpriteId.LionA_Icon_Inactive);
            lionBButtonImage.sprite = _catalog.Get(_session.ActiveLion == LionId.B ? SpriteId.LionB_Icon_Active : SpriteId.LionB_Icon_Inactive);
            extractImage.sprite = _catalog.Get(_session.ExitReady ? SpriteId.Btn_Extract : SpriteId.Btn_Extract_Locked);
            ShowHighlights();
        }

        void ShowHighlights()
        {
            foreach (var pair in _cells)
                pair.Value.SetHighlight(null, 0f);

            var targets = _session.Targets;
            for (var i = 0; i < targets.Count; i++)
            {
                var sprite = _catalog.Get(targets[i].Kind == MoveKind.Jump ? SpriteId.Hl_Jump : SpriteId.Hl_Step);
                _cells[targets[i].Pos].SetHighlight(sprite, _settings.HighlightAlpha);
            }
        }

        void OnCellClicked(CellPos pos)
        {
            if (_inputLocked || _session == null)
                return;
            Play(pos).Forget();
        }

        async UniTask Play(CellPos pos)
        {
            var startA = _session.State.LionA;
            var startB = _session.State.LionB;
            var wasReady = _session.ExitReady;
            var result = _session.TapCell(pos);
            if (!result.Accepted)
                return;
            await PlayEvents(result.Events, startA, startB, wasReady);
        }

        async UniTask OnExtract()
        {
            if (_inputLocked || _session == null)
                return;

            if (!_session.ExitReady)
            {
                _inputLocked = true;
                SetButtonsInteractable(false);
                try
                {
                    var rect = (RectTransform)extractButton.transform;
                    await rect.DOShakeAnchorPos(_settings.PopupDuration, new Vector2(18f, 0f), 16)
                        .Await(this.GetCancellationTokenOnDestroy());
                }
                finally
                {
                    ReleaseAfterAnimation();
                }

                return;
            }

            var startA = _session.State.LionA;
            var startB = _session.State.LionB;
            var result = _session.TryExtract();
            if (result.Accepted)
                await PlayEvents(result.Events, startA, startB, true);
        }

        async UniTask PlayEvents(IReadOnlyList<GameEvent> events, CellPos startA, CellPos startB, bool exitWasReady)
        {
            _inputLocked = true;
            _moveAnimating = true;
            SetButtonsInteractable(false);
            _play?.Cancel();
            _play?.Dispose();
            _play = new CancellationTokenSource();
            var cancellation = CancellationTokenSource.CreateLinkedTokenSource(_play.Token, this.GetCancellationTokenOnDestroy());
            try
            {
                Buzz(events);
                await _player.Play(events, BuildStage(), cancellation.Token, startA, startB);
                Refresh(_session.State);
                if (!exitWasReady && _session.ExitReady)
                    await _player.Pulse(extractButton.transform, cancellation.Token);
                if (_session.State.Status == GameStatus.Playing && !_pauseWhenIdle)
                    SetButtonsInteractable(true);
                else if (_session.State.Status != GameStatus.Playing)
                    ShowResult(_session.State.Status == GameStatus.Won);
            }
            finally
            {
                _moveAnimating = false;
                cancellation.Dispose();
                ReleaseAfterAnimation();
            }
        }

        void ReleaseAfterAnimation()
        {
            if (_session == null)
                return;
            if (_pauseWhenIdle && _session.State.Status == GameStatus.Playing && !PauseOpen && !RoundOver)
            {
                _pauseWhenIdle = false;
                _inputLocked = false;
                OnPause();
                return;
            }

            _inputLocked = _session.State.Status != GameStatus.Playing;
            if (_session.State.Status == GameStatus.Playing && !PauseOpen)
                SetButtonsInteractable(true);
        }

        public bool MoveBusy => _moveAnimating;
        public bool PauseOpen => pauseOverlay != null && pauseOverlay.activeSelf;
        public bool RoundOver =>
            (winOverlay != null && winOverlay.activeSelf) ||
            (loseOverlay != null && loseOverlay.activeSelf);

        public void OpenPauseFromBack() => OnPause();

        public void ResumeFromBack()
        {
            if (PauseOpen)
                ClosePause();
        }

        public void SuspendMatch()
        {
            if (_session == null || _session.State.Status != GameStatus.Playing || RoundOver || PauseOpen)
                return;
            if (_moveAnimating || _inputLocked)
            {
                _pauseWhenIdle = true;
                return;
            }

            OnPause();
        }

        void OnPause()
        {
            if (_inputLocked || _popup || _session == null || _session.State.Status != GameStatus.Playing)
                return;

            _popup = true;
            _inputLocked = true;
            SetButtonsInteractable(false);
            pauseOverlay.SetActive(true);
        }

        void ClosePause()
        {
            pauseOverlay.SetActive(false);
            _popup = false;
            _inputLocked = false;
            SetButtonsInteractable(true);
        }

        void RestartFromPause()
        {
            pauseOverlay.SetActive(false);
            _popup = false;
            OnRestart();
        }

        void ShowResult(bool won)
        {
            _popup = true;
            _inputLocked = true;
            SetButtonsInteractable(false);
            var points = _settings.GoldPointsPerCoin;
            var loot = (_session.CollectedCoins * points).ToString();
            var target = (_session.MinCoins * points).ToString();
            Finished?.Invoke(new GameResult(won, _level.Number, _session.CollectedCoins));

            if (won)
            {
                Style(winLoot, _app);
                Style(winTarget, _app);
                winLoot.text = loot;
                winTarget.text = target;
                var next = _app != null && _app.Run.HasNext(_level.Number);
                winNext.gameObject.SetActive(next);
                ((RectTransform)winSelect.transform).anchoredPosition = next ? _winSelectHome : _winNextHome;
                winOverlay.SetActive(true);
            }
            else
            {
                Style(loseLoot, _app);
                Style(loseTarget, _app);
                loseLoot.text = loot;
                loseTarget.text = target;
                loseOverlay.SetActive(true);
            }
        }

        static void Style(TMP_Text label, AppContext app)
        {
            if (app == null)
                return;
            label.font = app.Theme.Heading;
            label.color = app.Theme.Gold;
        }

        async UniTask OpenLevel(int number)
        {
            if (_app == null)
                return;
            await _app.Run.Next(number);
        }

        async UniTask OpenSelect()
        {
            if (_app == null)
                return;
            await _app.Run.Select();
        }

        async UniTask LeaveToMenu()
        {
            if (_app == null)
                return;
            await _app.Run.Menu();
        }

        void Buzz(IReadOnlyList<GameEvent> events)
        {
            if (_app == null || !_app.Options.Data.Vibration)
                return;

            for (var i = 0; i < events.Count; i++)
            {
                if (events[i] is TrapTriggered || events[i] is GameLost)
                    Vibrate();
            }
        }

        static void Vibrate()
        {
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }

        SoundManager ResolveSfx()
        {
            if (_app != null && _app.Sfx != null)
                return _app.Sfx;
            return FindAnyObjectByType<SoundManager>();
        }

        void Tap() => ResolveSfx()?.Play(SfxId.Tap);

        void OnSelectLion(LionId lion)
        {
            if (_inputLocked || _session == null || _session.State.Status != GameStatus.Playing)
                return;
            _session.SelectLion(lion);
            Refresh(_session.State);
        }

        void OnRestart()
        {
            _session.Restart();
            endOverlay.SetActive(false);
            if (winOverlay != null)
                winOverlay.SetActive(false);
            if (loseOverlay != null)
                loseOverlay.SetActive(false);
            _popup = false;
            _inputLocked = false;
            RebuildBoard();
            Refresh(_session.State);
            SetButtonsInteractable(true);
        }

        void SetButtonsInteractable(bool enabled)
        {
            pauseButton.interactable = enabled;
            lionAButton.interactable = enabled;
            lionBButton.interactable = enabled;
            extractButton.interactable = enabled;
        }

        PlayStage BuildStage()
        {
            return new PlayStage
            {
                Token = lion => lion == LionId.A ? lionAToken : lionBToken,
                CellCenter = pos => _cells[pos].GetComponent<RectTransform>().anchoredPosition,
                SlotAnchor = SlotAnchor,
                SlotScale = SlotScale,
                BringToFront = OrderTokens,
                Cell = pos => _cells[pos],
                SpriteFor = SpriteFor,
                OpenSprite = _catalog.Get(SpriteId.Cell_Open),
                Fill = alarmFill,
                FillAmount = value => FillOf(value, _session.State.AlarmThreshold),
                SetAlarm = value => alarmLabel.text = value + "/" + _session.State.AlarmThreshold,
                SetGold = total => goldLabel.text = LootText(total),
                Extract = extractButton.transform,
                EndGroup = endGroup,
                ShowEnd = title =>
                {
                    endOverlay.SetActive(true);
                    endLabel.text = title + "\ntap to restart";
                },
                CellSize = _cellSize
            };
        }

        void PlaceToken(RectTransform token, LionId lion, GameState state)
        {
            var pos = lion == LionId.A ? state.LionA : state.LionB;
            var slot = TokenSlots.GetSlot(lion, state.LionA, state.LionB);
            token.anchoredPosition = SlotAnchor(pos, slot);
            token.localScale = SlotScale(slot);
        }

        Vector2 SlotAnchor(CellPos pos, TokenSlot slot)
        {
            var center = _cells[pos].GetComponent<RectTransform>().anchoredPosition;
            var dx = 0f;
            if (slot == TokenSlot.Left)
                dx = -_settings.SharedTokenOffset * _cellSize;
            else if (slot == TokenSlot.Right)
                dx = _settings.SharedTokenOffset * _cellSize;
            return new Vector2(center.x + dx, center.y);
        }

        Vector3 SlotScale(TokenSlot slot)
        {
            var fit = _cellSize / NativeCell;
            var factor = slot == TokenSlot.Center ? 1f : _settings.SharedTokenScale;
            return Vector3.one * (fit * factor);
        }

        void OrderTokens(LionId front)
        {
            highlightRoot.SetAsLastSibling();
            var back = front == LionId.A ? lionBToken : lionAToken;
            var top = front == LionId.A ? lionAToken : lionBToken;
            back.SetAsLastSibling();
            top.SetAsLastSibling();
        }

        void FitToken(RectTransform token, Sprite sprite)
        {
            var image = token.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            token.sizeDelta = sprite.rect.size;
            token.localScale = Vector3.one * (_cellSize / NativeCell);
        }

        void ApplyChrome()
        {
            background.sprite = _catalog.Get(SpriteId.Bg_Game);
            alarmFrame.sprite = _catalog.Get(SpriteId.Alarm_Frame);
            alarmFrame.preserveAspect = true;
            pauseImage.sprite = _catalog.Get(SpriteId.Btn_Pause);
            pauseImage.preserveAspect = true;
            barFrame.sprite = _catalog.Get(SpriteId.Alarm_BarFrame);
            barFrame.preserveAspect = true;
            alarmFill.sprite = _catalog.Get(SpriteId.Alarm_BarFill);
            alarmFill.type = Image.Type.Filled;
            alarmFill.fillMethod = Image.FillMethod.Horizontal;
            alarmFill.fillOrigin = (int)Image.OriginHorizontal.Left;

            var alarmRect = (RectTransform)alarmLabel.transform;
            alarmRect.anchorMin = alarmRect.anchorMax = new Vector2(0.5f, 0.5f);
            alarmRect.pivot = new Vector2(0f, 0.5f);
            alarmRect.sizeDelta = new Vector2(220f, 72f);
            alarmRect.anchoredPosition = new Vector2(138f, -22f);
            alarmLabel.alignment = TextAlignmentOptions.MidlineLeft;
            alarmLabel.fontSize = 52f;
            alarmLabel.textWrappingMode = TextWrappingModes.NoWrap;

            var goldRect = (RectTransform)goldLabel.transform;
            goldRect.anchorMin = goldRect.anchorMax = new Vector2(0.5f, 0.5f);
            goldRect.pivot = new Vector2(0.5f, 0.5f);
            goldRect.sizeDelta = new Vector2(280f, 56f);
            goldRect.anchoredPosition = new Vector2(30f, -22f);
            goldLabel.alignment = TextAlignmentOptions.Center;
            goldLabel.fontSize = 36f;
            goldLabel.textWrappingMode = TextWrappingModes.NoWrap;

            var fillRect = (RectTransform)alarmFill.transform;
            fillRect.anchoredPosition = new Vector2(0f, 42f);
        }

        Sprite Ground(Cell cell) => _catalog.Get(cell.IsOpen ? SpriteId.Cell_Open : SpriteId.Cell_Closed);

        Sprite ItemSprite(Cell cell) => cell.IsOpen ? SpriteFor(cell.Item) : null;

        Sprite SpriteFor(ItemType item)
        {
            switch (item)
            {
                case ItemType.Trap: return _catalog.Get(SpriteId.Item_Trap);
                case ItemType.Gate: return _catalog.Get(SpriteId.Item_Gate);
                case ItemType.Lever: return _catalog.Get(SpriteId.Item_Lever);
                case ItemType.Gold: return _catalog.Get(SpriteId.Item_Gold);
                case ItemType.Chest: return _catalog.Get(SpriteId.Item_Chest);
                case ItemType.Exit: return _catalog.Get(SpriteId.Item_Exit);
                default: return null;
            }
        }

        static float CellSize(int columns, int rows)
        {
            var byWidth = (BoardSize - (columns - 1) * Gap) / columns;
            var byHeight = (BoardSize - (rows - 1) * Gap) / rows;
            return Mathf.Min(NativeCell, byWidth, byHeight);
        }

        string LootText(int coins) =>
            coins * _settings.GoldPointsPerCoin + "/" + _session.MinCoins * _settings.GoldPointsPerCoin;

        static float FillOf(int alarm, int threshold)
        {
            if (threshold <= 0)
                return 0f;
            return Mathf.Clamp01(alarm / (float)threshold);
        }
    }
}
