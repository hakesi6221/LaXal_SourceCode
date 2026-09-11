using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// MonoBehaviourAnimSequencerの要素となるアニメーションを作るための派生クラス
/// CanvasGroupに対して指定の値間でのフェードを行う
/// </summary>
public class AnimationSequenceUIFade : AnimationSeqenceElementBase
{
    [SerializeField, Label("フェードするCanvasGroup")]
    private CanvasGroup _canvasGroup = null;

    [SerializeField, Label("フェード開始までのディレイ：秒")]
    private float _delayDuration = 0f;

    [SerializeField, Label("フェード時間：秒")]
    private float _fadeDuration = 0.5f;

    [SerializeField, Label("開始α値"), Range(0.0f, 1.0f)]
    private float _startAlpha = 0.0f;

    [SerializeField, Label("終了α値"), Range(0.0f, 1.0f)]
    private float _finishAlpha = 1.0f;

    [SerializeField, Label("イージング")]
    private Ease _fadeEase = Ease.InQuad;

    // フェードアニメーションクラス
    private FadeAnimationCanvasGroup _fadeAnimation = new FadeAnimationCanvasGroup();

    public override void Initialize()
    {
        if (_canvasGroup == null)
        {
            Debug.LogError($"{this.name}：[_canvasGroup]がアタッチされていません。Inspectorを確認してください");
            return;
        }

        _canvasGroup.alpha = _startAlpha;
    }

    public async override UniTask PlayAnimationAsync(CancellationToken cancellationToken = default)
    {
        var token = this.GetCancellationTokenOnDestroy();
        if (_canvasGroup == null)
        {
            Debug.LogError($"{this.name}：[_canvasGroup]がアタッチされていません。Inspectorを確認してください");
            return;
        }

        _canvasGroup.alpha = _startAlpha;
        // フェード
        try
        {
            await _fadeAnimation.PlayAnim(_canvasGroup
                                        , _finishAlpha
                                        , _fadeDuration
                                        , _delayDuration
                                        , _fadeEase
                                        , token);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning($"{this.name}：キャンセルされました。");
            return;
        }

        _canvasGroup.alpha = _finishAlpha;
    }
}
