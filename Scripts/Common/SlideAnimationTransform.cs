using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

/// <summary>
/// Transformに対してスライドのアニメーションをさせるクラス
/// </summary>
public class SlideAnimationTransform
{
    /// <summary>
    /// スライドアニメーションを再生
    /// </summary>
    /// <param name="transform">再生対象</param>
    /// <param name="destination">再生後の移動目標座標</param>
    /// <param name="duration">アニメーションの所要時間</param>
    /// <param name="ease">アニメーションのイージング</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns></returns>
    public async UniTask PlayAnim
    (
        Transform transform,
        Vector3 destination,
        float duration,
        float delay = 0f,
        Ease ease = Ease.Linear,
        CancellationToken cancellationToken = default
    )
    {
        if (transform == null)
        {
            Debug.LogError($"{typeof(SlideAnimationTransform).Name}:スライドアニメーションの対象オブジェクトが正しく渡されていません。");
            return;
        }

        Vector3 from = transform.localPosition;

        try
        {
            await LMotion.Create(from, destination, duration)
                        .WithEase(ease)
                        .WithDelay(delay)
                        .BindToLocalPosition(transform)
                        .ToUniTask(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("非同期処理のキャンセル");
            return;
        }
    }
}
