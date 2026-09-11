using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using NaughtyAttributes;
using UnityEngine;

public class AnimationSequenceUIFadeAndScale : AnimationSeqenceElementBase
{
    [SerializeField, Label("アニメーションする対象")]
    private CanvasGroup _target = null;

    [SerializeField, Label("アニメーション開始までのディレイ：秒")]
    private float _delayDuration = 0f;

    [SerializeField, Label("アニメーション時間：秒")]
    private float _fadeDuration = 0.5f;

    [SerializeField, Label("Scaleの拡縮回数")]
    private int _animNum = 2;

    [SerializeField, Label("ベースのScale")]
    private Vector3 _baseScale = Vector3.one;

    [SerializeField, Label("開始Scale値倍率")]
    private float _startScaleMult = 1f;

    [SerializeField, Label("終了Scale値倍率")]
    private float _finishScaleMult = 1.5f;

    [SerializeField, Label("開始α値"), Range(0.0f, 1.0f)]
    private float _startAlpha = 0.0f;

    [SerializeField, Label("終了α値"), Range(0.0f, 1.0f)]
    private float _finishAlpha = 1.0f;

    [SerializeField, Label("イージング")]
    private Ease _fadeEase = Ease.InQuad;

    // フェードアニメーションクラス
    private ScaleAnimation_Zoom<Transform> _scaleAnimation;

    // フェードアニメーションクラス
    private FadeAnimationCanvasGroup _fadeAnimation = new FadeAnimationCanvasGroup();

    public override void Initialize()
    {
        if (_target == null)
        {
            Debug.LogError($"{this.name}：[_canvasGroup]がアタッチされていません。Inspectorを確認してください");
            return;
        }

        _scaleAnimation = new ScaleAnimation_Zoom<Transform>
        (
            _target.transform,
            _startScaleMult,
            _finishScaleMult,
            _fadeDuration,
            _fadeEase
        );
        _target.transform.localScale = _baseScale;
        _target.alpha = _startAlpha;
    }

    public async override UniTask PlayAnimationAsync(CancellationToken cancellationToken = default)
    {
        var token = this.GetCancellationTokenOnDestroy();
        if (_target == null)
        {
            Debug.LogError($"{this.name}：[_canvasGroup]がアタッチされていません。Inspectorを確認してください");
            return;
        }

        _target.alpha = _startAlpha;
        // フェード
        try
        {
            var scaleHandle = _scaleAnimation.ZoomAnim_Single(token, _animNum);
            var fadeHandle =  _fadeAnimation.PlayAnim(_target
                                        , _finishAlpha
                                        , _fadeDuration
                                        , _delayDuration
                                        , _fadeEase
                                        , token);

            await UniTask.WhenAll(scaleHandle, fadeHandle);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning($"{this.name}：キャンセルされました。");
            return;
        }
    }
}
