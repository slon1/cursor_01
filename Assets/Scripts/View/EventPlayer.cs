using System;
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
        readonly SoundManager _sfx;
        bool _revealed;

        public EventPlayer(GameSettingsConfig settings, SoundManager sfx = null)
        {
            _settings = settings;
            _sfx = sfx;
        }

        public async UniTask Play(IReadOnlyList<GameEvent> events, PlayStage stage, CancellationToken cancellation)
        {
            _revealed = false;
            for (var i = 0; i < events.Count; i++)
                await PlayOne(events[i], stage, cancellation);
        }

        async UniTask PlayOne(GameEvent gameEvent, PlayStage stage, CancellationToken cancellation)
        {
            switch (gameEvent)
            {
                case LionMoved moved:
                    Cue(moved.IsJump ? SfxId.Jump : SfxId.Step);
                    await MoveToken(stage.Token(moved.Lion), stage.CellCenter(moved.To), moved.IsJump, stage, cancellation);
                    break;
                case CellOpened opened:
                    Cue(SfxId.Open);
                    await OpenCell(stage.Cell(opened.Pos), opened.Item, stage, cancellation);
                    break;
                case TrapTriggered trap:
                    Cue(SfxId.Hit);
                    await ShakeToken(stage.Token(trap.Lion), true, cancellation);
                    break;
                case GateBlocked blocked:
                    Cue(SfxId.Hit);
                    await ShakeToken(stage.Token(blocked.Lion), false, cancellation);
                    break;
                case GateBroken broken:
                    Cue(SfxId.Break);
                    await WaitToConsume(cancellation);
                    await HideItem(stage.Cell(broken.Pos), false, cancellation);
                    break;
                case GoldCollected gold:
                    Cue(SfxId.Gold);
                    await WaitToConsume(cancellation);
                    stage.SetGold(gold.Total);
                    await HideItem(stage.Cell(gold.Pos), true, cancellation);
                    break;
                case LeverUsed lever:
                    Cue(SfxId.Lever);
                    await WaitToConsume(cancellation);
                    await HideItem(stage.Cell(lever.Pos), false, cancellation);
                    break;
                case AlarmChanged alarm:
                    await AnimateAlarm(alarm, stage, cancellation);
                    break;
                case ChestTaken taken:
                    Cue(SfxId.Chest);
                    await WaitToConsume(cancellation);
                    await TakeChest(stage.Cell(taken.Pos), cancellation);
                    break;
                case GameWon:
                    Cue(SfxId.Win);
                    break;
                case GameLost:
                    Cue(SfxId.Lose);
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

        async UniTask OpenCell(CellView cell, ItemType item, PlayStage stage, CancellationToken cancellation)
        {
            var rect = (RectTransform)cell.transform;
            var scale = rect.localScale;
            await rect.DOScaleX(0f, _settings.FadeDuration * 0.35f).Await(cancellation);
            cell.SetVisual(stage.OpenSprite, item == ItemType.None ? null : stage.SpriteFor(item));
            await rect.DOScaleX(scale.x, _settings.FadeDuration * 0.35f).Await(cancellation);
            if (item == ItemType.None)
                return;

            var image = cell.ItemImage;
            image.color = new Color(1f, 1f, 1f, 0f);
            image.rectTransform.localScale = Vector3.one * 0.8f;
            await UniTask.WhenAll(
                image.DOFade(1f, _settings.FadeDuration * 0.4f).Await(cancellation),
                image.rectTransform.DOScale(1f, _settings.FadeDuration * 0.4f).Await(cancellation));
            _revealed = true;
            await UniTask.Delay(TimeSpan.FromSeconds(_settings.RevealPause), cancellationToken: cancellation);
        }

        async UniTask WaitToConsume(CancellationToken cancellation)
        {
            if (_revealed)
                return;

            await UniTask.Delay(TimeSpan.FromSeconds(_settings.ConsumePause), cancellationToken: cancellation);
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

        async UniTask TakeChest(CellView cell, CancellationToken cancellation)
        {
            var item = cell.ItemImage;
            if (item.sprite == null)
                return;

            var rect = item.rectTransform;
            await UniTask.WhenAll(
                item.DOFade(0f, _settings.FadeDuration).Await(cancellation),
                rect.DOScale(1.3f, _settings.FadeDuration).Await(cancellation));
            item.enabled = false;
            rect.localScale = Vector3.one;
        }

        void Cue(SfxId id) => _sfx?.Play(id);

    }

    public sealed class PlayStage
    {
        public System.Func<LionId, RectTransform> Token { get; set; }
        public System.Func<CellPos, Vector2> CellCenter { get; set; }
        public System.Func<CellPos, CellView> Cell { get; set; }
        public System.Func<ItemType, Sprite> SpriteFor { get; set; }
        public Sprite OpenSprite { get; set; }
        public Image Fill { get; set; }
        public System.Func<int, float> FillAmount { get; set; }
        public System.Action<int> SetAlarm { get; set; }
        public System.Action<int> SetGold { get; set; }
        public Transform Extract { get; set; }
        public CanvasGroup EndGroup { get; set; }
        public System.Action<string> ShowEnd { get; set; }
        public float CellSize { get; set; }
    }
}
