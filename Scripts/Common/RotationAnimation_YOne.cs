using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

public class RotationAnimation_YOne<T> where T : Transform
{
    // アタッチしたオブジェクトのRectTransform参照
    private T _transform;

    // 単発アニメの長さ：秒
    private float _zoomAnimDuration;

    // 単発アニメのEasing
    private Ease _zoomAnimEase;

    // キャンセルトークンソース
    private CancellationTokenSource _cts = new CancellationTokenSource();

    public RotationAnimation_YOne(T transform, float duration, Ease ease)
    {
        _transform = transform;
        _zoomAnimDuration = duration;
        _zoomAnimEase = ease;
    }

    /// <summary>
    /// ズームアニメーションを単発で行う
    /// アニメーション再生中にに再び呼ばれた場合、前のものをキャンセルして再生する
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public async UniTask RotAnim_One(CancellationToken token)
    {
        _cts.Cancel();
        _cts = new CancellationTokenSource();
        var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, token);
        var linkedToken = linkedSource.Token;

        try
        {
            await LMotion.Create(0f, 360f, _zoomAnimDuration)
                        .WithEase(_zoomAnimEase)
                        .BindToLocalEulerAnglesY(_transform)
                        .ToUniTask(cancellationToken: linkedToken);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("UIAnimation_Zoom：UIズームアニメーションがキャンセルされました");
            return;
        }

        _transform.localEulerAngles = Vector3.zero;
    }
}
