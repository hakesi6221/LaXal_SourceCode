using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

/// <summary>
/// コンポーネントとしてアタッチしたUIオブジェクトに、単発の拡縮アニメーションをさせる関数を持ったクラス
/// </summary>
public class ScaleAnimation_Zoom<T> where T : Transform
{
    // アタッチしたオブジェクトのRectTransform参照
    private T _transform;

    // デフォルトのlocalScaleへの倍率
    private float _defaultScaleMult;

    // 単発アニメ後のlocalScaleへの倍率
    private float _zoomedScaleMult;

    // 単発アニメの長さ：秒
    private float _zoomAnimDuration;

    // 単発アニメのEasing
    private Ease _zoomAnimEase;

    // 元のlocalScale
    private Vector3 _defaultScale;

    // キャンセルトークンソース
    private CancellationTokenSource _cts = new CancellationTokenSource();

    public ScaleAnimation_Zoom(T transform, float defaultScaleMult, float zoomedScaleMult, float duration, Ease ease)
    {
        _transform = transform;
        _defaultScale = _transform.localScale;
        _defaultScaleMult = defaultScaleMult;
        _zoomedScaleMult = zoomedScaleMult;
        _zoomAnimDuration = duration;
        _zoomAnimEase = ease;
    }

    /// <summary>
    /// ズームアニメーションを単発で行う
    /// アニメーション再生中にに再び呼ばれた場合、前のものをキャンセルして再生する
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public async UniTask ZoomAnim_Single(CancellationToken token, int loopNum = 2)
    {
        _cts.Cancel();
        _cts = new CancellationTokenSource();
        var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, token);
        var linkedToken = linkedSource.Token;

        try
        {
            await LMotion.Create(_defaultScale * _defaultScaleMult, _defaultScale * _zoomedScaleMult, _zoomAnimDuration)
                        .WithLoops(loopNum, LoopType.Yoyo)
                        .WithEase(_zoomAnimEase)
                        .BindToLocalScale(_transform)
                        .ToUniTask(cancellationToken: linkedToken);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("UIAnimation_Zoom：UIズームアニメーションがキャンセルされました");
            return;
        }
    }

}
