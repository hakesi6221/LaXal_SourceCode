using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// カットインオブジェクトを管理するクラス
/// </summary>
public class CutInObj : MonoBehaviour
{
    [SerializeField, Label("オブジェクトルートCanvasGroup"), BoxGroup("Component")]
    private CanvasGroup _rootGroup = null;

    [SerializeField, Label("キャラクターImage"), BoxGroup("Component")]
    private Image _charImage = null;

    [SerializeField, Label("カットイン移動のEase"), BoxGroup("Parameters")]
    private Ease _moveEase = Ease.Linear;

    [SerializeField, Label("入場の所要秒数"), BoxGroup("Parameters")]
    private float _inDuration = 0.2f;

    [SerializeField, Label("カットインの待機所要秒数"), BoxGroup("Parameters")]
    private float _waitDuration = 1.0f;

    [SerializeField, Label("退場の待機所要秒数"), BoxGroup("Parameters")]
    private float _outDuration = 0.2f;

    [SerializeField, Label("カットインの移動元"), BoxGroup("Parameters")]
    private Vector3 _cutinFromPos = Vector3.zero;

    [SerializeField, Label("入場時の移動先"), BoxGroup("Parameters")]
    private Vector3 _inDestinationPos = Vector3.zero;

    [SerializeField, Label("退場時の移動先"), BoxGroup("Parameters")]
    private Vector3 _outDestinationPos = Vector3.zero;

    // アニメーションキャンセル用のCTS
    private CancellationTokenSource _cts = new CancellationTokenSource();

    // スライドアニメーション用クラス
    private SlideAnimationTransform _slideAnimation = new SlideAnimationTransform();

    // フェードアニメーション用クラス
    private FadeAnimationCanvasGroup _fadeAnimation = new FadeAnimationCanvasGroup();

    [Button]
    private async void test()
    {
        if (_charImage == null || _rootGroup == null)
        {
            Debug.LogError($"{this.name}:カットイン再生に必要な情報が不足しています");
            return;
        }
        var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy()
                                                                            , _cts.Token);
        var token = linkedSource.Token;

        _rootGroup.transform.localPosition = _cutinFromPos;
        _rootGroup.gameObject.SetActive(true);
        var slideInHandle = _slideAnimation.PlayAnim(_rootGroup.transform
                                                , _inDestinationPos
                                                , _inDuration
                                                , ease: _moveEase
                                                , cancellationToken: token);
        var fadeInHandle = _fadeAnimation.PlayAnim(_rootGroup
                                                , 1.0f
                                                , _inDuration
                                                , ease: _moveEase
                                                , cancellationToken: token);

        try
        {
            // スライドしつつフェードイン
            await UniTask.WhenAll(slideInHandle, fadeInHandle)
                            .AttachExternalCancellation(token);
            // 出現時間分待機
            await UniTask.WaitForSeconds(_waitDuration, cancellationToken: token);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("非同期処理のキャンセル");
            return;
        }
        var slideOutHandle = _slideAnimation.PlayAnim(_rootGroup.transform
                                                    , _outDestinationPos
                                                    , _outDuration / 2
                                                    , _outDuration / 2
                                                    , _moveEase
                                                    , token);
        var fadeOutHandle = _fadeAnimation.PlayAnim(_rootGroup
                                                , 0.0f
                                                , _outDuration
                                                , ease: _moveEase
                                                , cancellationToken: token);

        try
        {
            // スライドしつつフェードイン
            await UniTask.WhenAll(slideOutHandle, fadeOutHandle)
                            .AttachExternalCancellation(token);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("非同期処理のキャンセル");
            return;
        }
    }

    /// <summary>
    /// カットインを再生する
    /// スライドイン⇒待機⇒スライドアウトの順番で動く
    /// </summary>
    /// <param name="charSprite"></param>
    /// <returns></returns>
    public async UniTask PlayCutIn(Sprite charSprite)
    {
        if (_charImage == null || _rootGroup == null)
        {
            Debug.LogError($"{this.name}:カットイン再生に必要な情報が不足しています");
            return;
        }

        // キャラクターのイメージのSpriteをキャラクターのものに変更
        _charImage.sprite = charSprite;
        var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy()
                                                                            , _cts.Token);
        var token = linkedSource.Token;

        _rootGroup.transform.localPosition = _cutinFromPos;
        _rootGroup.gameObject.SetActive(true);
        var slideInHandle = _slideAnimation.PlayAnim(_rootGroup.transform
                                                , _inDestinationPos
                                                , _inDuration
                                                , ease: _moveEase
                                                , cancellationToken: token);
        var fadeInHandle = _fadeAnimation.PlayAnim(_rootGroup
                                                , 1.0f
                                                , _inDuration
                                                , ease: _moveEase
                                                , cancellationToken: token);

        try
        {
            // スライドしつつフェードイン
            await UniTask.WhenAll(slideInHandle, fadeInHandle)
                            .AttachExternalCancellation(token);
            // 出現時間分待機
            await UniTask.WaitForSeconds(_waitDuration, cancellationToken: token);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("非同期処理のキャンセル");
            return;
        }
        var slideOutHandle = _slideAnimation.PlayAnim(_rootGroup.transform
                                                    , _outDestinationPos
                                                    , _outDuration / 2
                                                    , _outDuration / 2
                                                    , _moveEase
                                                    , token);
        var fadeOutHandle = _fadeAnimation.PlayAnim(_rootGroup
                                                , 0.0f
                                                , _outDuration
                                                , ease: _moveEase
                                                , cancellationToken: token);

        try
        {
            // スライドしつつフェードイン
            await UniTask.WhenAll(slideOutHandle, fadeOutHandle)
                            .AttachExternalCancellation(token);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("非同期処理のキャンセル");
            return;
        }
    }
}
