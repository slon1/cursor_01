using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace Game.View
{
    static class TweenAwait
    {
        public static async UniTask Await(this Tween tween, CancellationToken cancellationToken)
        {
            if (tween == null || !tween.active)
                return;

            try
            {
                while (tween.IsActive() && !tween.IsComplete())
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                if (tween.IsActive())
                    tween.Kill();
                throw;
            }
        }
    }
}
