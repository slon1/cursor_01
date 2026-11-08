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
        const float BoardSize = 940f;
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

        GameSession _session;
        SpriteCatalog _catalog;
        GameSettingsConfig _settings;
        EventPlayer _player;
        readonly Dictionary<CellPos, CellView> _cells = new Dictionary<CellPos, CellView>();
        CancellationTokenSource _play;
        bool _inputLocked;
        float _cellSize = NativeCell;

        void Awake()
        {
            pauseButton.onClick.AddListener(OnPause);
            lionAButton.onClick.AddListener(() => OnSelectLion(LionId.A));
            lionBButton.onClick.AddListener(() => OnSelectLion(LionId.B));
            extractButton.onClick.AddListener(() => OnExtract().Forget());
            endButton.onClick.AddListener(OnRestart);
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
            Show((LevelConfig)args, app.Catalog, app.Settings, app.Theme);
        }

        public void Show(LevelConfig level, SpriteCatalog catalog, GameSettingsConfig settings, UiTheme theme = null)
        {
            _catalog = catalog;
            _settings = settings;
            _player = new EventPlayer(settings);
            _session = new GameSession(level.ToLevelData());
            endOverlay.SetActive(false);
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

            highlightRoot.SetAsLastSibling();
            lionAToken.SetAsLastSibling();
            lionBToken.SetAsLastSibling();
            FitToken(lionAToken, _catalog.Get(SpriteId.LionA_Token));
            FitToken(lionBToken, _catalog.Get(SpriteId.LionB_Token));
        }

        void Refresh(GameState state)
        {
            foreach (var pair in _cells)
            {
                var cell = state.CellAt(pair.Key);
                pair.Value.SetVisual(Ground(cell), ItemSprite(cell));
            }

            PlaceToken(lionAToken, state.LionA);
            PlaceToken(lionBToken, state.LionB);
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
            var result = _session.TapCell(pos);
            if (!result.Accepted)
                return;
            await PlayEvents(result.Events);
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
                    if (_session.State.Status == GameStatus.Playing)
                    {
                        _inputLocked = false;
                        SetButtonsInteractable(true);
                    }
                }

                return;
            }

            var result = _session.TryExtract();
            if (result.Accepted)
                await PlayEvents(result.Events);
        }

        async UniTask PlayEvents(IReadOnlyList<GameEvent> events)
        {
            _inputLocked = true;
            SetButtonsInteractable(false);
            _play?.Cancel();
            _play?.Dispose();
            _play = new CancellationTokenSource();
            var cancellation = CancellationTokenSource.CreateLinkedTokenSource(_play.Token, this.GetCancellationTokenOnDestroy());
            try
            {
                await _player.Play(events, BuildStage(), cancellation.Token);
                if (_session.State.Status == GameStatus.Playing)
                {
                    Refresh(_session.State);
                    SetButtonsInteractable(true);
                }
                else
                {
                    Refresh(_session.State);
                    endOverlay.SetActive(true);
                }
            }
            finally
            {
                cancellation.Dispose();
                _inputLocked = _session.State.Status != GameStatus.Playing;
            }
        }

        void OnPause()
        {
            if (_inputLocked)
                return;
            Debug.Log("Пауза");
        }

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
                Cell = pos => _cells[pos],
                ItemSprite = pos => ItemSprite(_session.State.CellAt(pos)),
                OpenSprite = _catalog.Get(SpriteId.Cell_Open),
                Fill = alarmFill,
                FillAmount = value => FillOf(value, _session.State.AlarmThreshold),
                SetAlarm = value => alarmLabel.text = value + "/" + _session.State.AlarmThreshold,
                SetGold = total => goldLabel.text = LootText(total),
                Extract = extractButton.transform,
                ChestItem = FindChestItem(),
                EndGroup = endGroup,
                ShowEnd = title =>
                {
                    endOverlay.SetActive(true);
                    endLabel.text = title + "\ntap to restart";
                },
                CellSize = _cellSize
            };
        }

        Transform FindChestItem()
        {
            foreach (var pair in _cells)
            {
                if (_session.State.CellAt(pair.Key).Item == ItemType.Chest)
                    return pair.Value.ItemImage.transform;
            }

            return null;
        }

        void PlaceToken(RectTransform token, CellPos pos) =>
            token.anchoredPosition = _cells[pos].GetComponent<RectTransform>().anchoredPosition;

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
            alarmRect.anchorMin = new Vector2(0.2f, 0.18f);
            alarmRect.anchorMax = new Vector2(0.8f, 0.82f);
            alarmRect.offsetMin = Vector2.zero;
            alarmRect.offsetMax = Vector2.zero;
            alarmLabel.alignment = TextAlignmentOptions.Center;
            alarmLabel.fontSize = 46f;
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

        Sprite ItemSprite(Cell cell)
        {
            if (!cell.IsOpen)
                return null;

            switch (cell.Item)
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
