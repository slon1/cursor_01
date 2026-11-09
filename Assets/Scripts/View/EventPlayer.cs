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
    public sealed class EventPlayer
    {
        readonly GameSettingsConfig _settings;

        public EventPlayer(GameSettingsConfig settings) => _settings = settings;

        public async UniTask Play(IReadOnlyList<GameEvent> events, PlayStage stage, CancellationToken cancellation)
        {
            for (var i = 0; i < events.Count; i++)
                await PlayOne(events[i], stage, cancellation);
        }

        async UniTask PlayOne(GameEvent gameEvent, PlayStage stage, CancellationToken cancellation)
        {
            switch (gameEvent)
            {
                case LionMoved moved:
                    await MoveToken(stage.Token(moved.Lion), stage.CellCenter(moved.To), moved.IsJump, stage, cancellation);
                    break;
                case LionsSwapped swapped:
                    await Swap(swapped, stage, cancellation);
                    break;
                case CellOpened opened:
                    await OpenCell(stage.Cell(opened.Pos), stage, cancellation);
                    break;
                case TrapTriggered trap:
                    await ShakeToken(stage.Token(trap.Lion), true, cancellation);
                    break;
                case GateBlocked blocked:
                    await ShakeToken(stage.Token(blocked.Lion), false, cancellation);
                    break;
                case GateBroken broken:
                    await HideItem(stage.Cell(broken.Pos), false, cancellation);
                    break;
                case GoldCollected gold:
                    await HideItem(stage.Cell(gold.Pos), true, cancellation);
                    stage.SetGold(gold.Total);
                    break;
                case LeverUsed lever:
                    await HideItem(stage.Cell(lever.Pos), false, cancellation);
                    break;
                case AlarmChanged alarm:
                    await AnimateAlarm(alarm, stage, cancellation);
                    break;
                case ChestOpened:
                    await Pulse(stage.ChestItem, cancellation);
                    break;
                case ExitAttempted:
                    await Pulse(stage.Extract, cancellation);
                    break;
                case GameWon:
                case GameLost:
                    break;
            }
        }

        async UniTask MoveToken(RectTransform token, Vector2 target, bool jump, PlayStage stage, CancellationToken cancellation)
        {
            if (!jump)
            {
                await token.DOAnchorPos(target, _settings.MoveDuration).SetEase(Ease.OutQuad).Await(cancellation);
                return;
            }

            var start = token.anchoredPosition;
            var scale = token.localScale;
            var height = stage.CellSize * 0.45f;
            await DOTween.To(() => 0f, t =>
            {
                var pos = Vector2.Lerp(start, target, t);
                pos.y += Mathf.Sin(t * Mathf.PI) * height;
                token.anchoredPosition = pos;
                var bump = 1f + Mathf.Sin(t * Mathf.PI) * 0.12f;
                token.localScale = scale * bump;
            }, 1f, _settings.MoveDuration).SetEase(Ease.Linear).Await(cancellation);
            token.localScale = scale;
        }

        async UniTask Swap(LionsSwapped swapped, PlayStage stage, CancellationToken cancellation)
        {
            var first = MoveToken(stage.Token(LionId.A), stage.CellCenter(swapped.PosA), swapped.Initiator == LionId.A && swapped.IsJump, stage, cancellation);
            var second = MoveToken(stage.Token(LionId.B), stage.CellCenter(swapped.PosB), swapped.Initiator == LionId.B && swapped.IsJump, stage, cancellation);
            await UniTask.WhenAll(first, second);
        }

        async UniTask OpenCell(CellView cell, PlayStage stage, CancellationToken cancellation)
        {
            var rect = (RectTransform)cell.transform;
            var scale = rect.localScale;
            await rect.DOScaleX(0f, _settings.FadeDuration * 0.35f).Await(cancellation);
            cell.SetVisual(stage.OpenSprite, stage.ItemSprite(cell.Pos));
            await rect.DOScaleX(scale.x, _settings.FadeDuration * 0.35f).Await(cancellation);
            if (cell.ItemImage.sprite == null)
                return;

            cell.ItemImage.color = new Color(1f, 1f, 1f, 0f);
            await cell.ItemImage.DOFade(1f, _settings.FadeDuration * 0.4f).Await(cancellation);
        }

        async UniTask ShakeToken(RectTransform token, bool red, CancellationToken cancellation)
        {
            var duration = _settings.PopupDuration;
            var shake = token.DOShakeAnchorPos(duration, new Vector2(12f, 8f), 18).Await(cancellation);
            if (!red)
            {
                await shake;
                return;
            }

            var image = token.GetComponent<Image>();
            var flash = image.DOColor(new Color(1f, 0.35f, 0.35f), duration * 0.5f).SetLoops(2, LoopType.Yoyo).Await(cancellation);
            await UniTask.WhenAll(shake, flash);
        }

        async UniTask HideItem(CellView cell, bool floatUp, CancellationToken cancellation)
        {
            var item = cell.ItemImage;
            if (item.sprite == null)
                return;

            var fade = item.DOFade(0f, _settings.FadeDuration).Await(cancellation);
            if (!floatUp)
            {
                await UniTask.WhenAll(fade, item.rectTransform.DOScale(0.6f, _settings.FadeDuration).Await(cancellation));
            }
            else
            {
                var rect = item.rectTransform;
                await UniTask.WhenAll(fade, rect.DOAnchorPosY(rect.anchoredPosition.y + 28f, _settings.FadeDuration).Await(cancellation));
            }

            item.enabled = false;
            item.rectTransform.localScale = Vector3.one;
        }

        async UniTask AnimateAlarm(AlarmChanged alarm, PlayStage stage, CancellationToken cancellation)
        {
            var duration = _settings.FadeDuration;
            var fill = stage.Fill.DOFillAmount(stage.FillAmount(alarm.NewValue), duration).Await(cancellation);
            var text = DOTween.To(() => alarm.OldValue, value => stage.SetAlarm(value), alarm.NewValue, duration).Await(cancellation);
            await UniTask.WhenAll(fill, text);
        }

        async UniTask Pulse(Transform target, CancellationToken cancellation)
        {
            if (target == null)
                return;

            var scale = target.localScale;
            await target.DOScale(scale * 1.12f, _settings.PopupDuration * 0.5f).SetLoops(2, LoopType.Yoyo).Await(cancellation);
            target.localScale = scale;
        }

    }

    public sealed class PlayStage
    {
        public System.Func<LionId, RectTransform> Token { get; set; }
        public System.Func<CellPos, Vector2> CellCenter { get; set; }
        public System.Func<CellPos, CellView> Cell { get; set; }
        public System.Func<CellPos, Sprite> ItemSprite { get; set; }
        public Sprite OpenSprite { get; set; }
        public Image Fill { get; set; }
        public System.Func<int, float> FillAmount { get; set; }
        public System.Action<int> SetAlarm { get; set; }
        public System.Action<int> SetGold { get; set; }
        public Transform Extract { get; set; }
        public Transform ChestItem { get; set; }
        public CanvasGroup EndGroup { get; set; }
        public System.Action<string> ShowEnd { get; set; }
        public float CellSize { get; set; }
    }
}
