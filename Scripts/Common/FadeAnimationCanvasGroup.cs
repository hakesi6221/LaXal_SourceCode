using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

/// <summary>
/// UICanvasGroupに対してフェードアニメーションさせるクラス
/// </summary>
public class FadeAnimationCanvasGroup
{
    /// <summary>
    /// フェードアニメーションを再生
    /// </summary>
    /// <param name="canvasGroup">再生対象</param>
    /// <param name="finishAlpha">アルファ遷移目標値</param>
    /// <param name="duration">アニメーションの所要時間</param>
    /// <param name="ease">アニメーションのイージング</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns></returns>
    public async UniTask PlayAnim
    (
        CanvasGroup canvasGroup,
        float finishAlpha,
        float duration,
        float delay = 0f,
        Ease ease = Ease.Linear,
        CancellationToken cancellationToken = default
    )
    {
        if (canvasGroup == null)
        {
            Debug.LogError($"{typeof(FadeAnimationCanvasGroup).Name}：[canvasGroup]がアタッチされていません。Inspectorを確認してください");
            return;
        }

        float startAlpha = canvasGroup.alpha;
        try
        {
            await LMotion.Create(startAlpha, finishAlpha, duration)
                        .WithEase(ease)
                        .WithDelay(delay)
                        .BindToAlpha(canvasGroup)
                        .ToUniTask(cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("非同期処理のキャンセル");
            return;
        }
    }
}
