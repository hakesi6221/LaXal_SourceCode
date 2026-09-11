using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using TMPro;
using UnityEngine;

/// <summary>
/// Transformに対してスライドのアニメーションをさせるクラス
/// </summary>
public class CharSpaceAnimTmp
{
    /// <summary>
    /// TMPの文字間隔アニメーションを再生
    /// </summary>
    /// <param name="target">再生対象</param>
    /// <param name="finishSpace">再生後の値</param>
    /// <param name="duration">アニメーションの所要時間</param>
    /// <param name="delay">アニメーションの最初のディレイ</param>
    /// <param name="ease">アニメーションのイージング</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns></returns>
    public async UniTask PlayAnim
    (
        TextMeshProUGUI target,
        float finishSpace,
        float duration,
        float delay = 0f,
        Ease ease = Ease.Linear,
        CancellationToken cancellationToken = default
    )
    {
        if (target == null)
        {
            Debug.LogError($"{typeof(CharSpaceAnimTmp).Name}:スライドアニメーションの対象オブジェクトが正しく渡されていません。");
            return;
        }

        float startSpace = target.characterSpacing;

        try
        {
            await LMotion.Create(startSpace, finishSpace, duration)
                        .WithEase(ease)
                        .WithDelay(delay)
                        .Bind(space => target.characterSpacing = space)
                        .ToUniTask(cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning($"{typeof(CharSpaceAnimTmp).Name}：キャンセルされました。");
            return;
        }
    }
}
