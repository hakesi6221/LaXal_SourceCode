using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// MonoBehaviourAnimSequencerの要素となるアニメーションを作るための派生クラス
/// 自身と全ての子オブジェクトのSpriteRendererに対して指定に値間でのフェードを行う
/// </summary>
public class AnimationSequenceSpritesFade : AnimationSeqenceElementBase
{
    [SerializeField, Label("フェードするオブジェクトのルート")]
    private Transform _rootObject = null;

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

    private SpriteRenderer[] _spriteRenderers = null;

    private async UniTask FadeInSpritesAsync(CancellationToken cancellationToken = default)
    {
        // ディレイ
        if (_delayDuration > 0f)
        {
            try
            {
                await UniTask.WaitForSeconds(_delayDuration
                                            , cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"{this.name}：キャンセルされました。");
                return;
            }
        }

        List<UniTask> handles = new List<UniTask>();

        // 全てのSpriteRendererのアルファ値を0から1に変化させる非同期処理を保存
        foreach (var sr in _spriteRenderers)
        {
            var token = sr.GetCancellationTokenOnDestroy();
            handles.Add
            (
                LMotion.Create(_startAlpha, _finishAlpha, _fadeDuration)
                        .WithEase(_fadeEase)
                        .WithOnComplete(() => sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, _finishAlpha))
                        .Bind(alpha => sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha))
                        .ToUniTask(cancellationToken: token)
            );
        }

        // フェードイン
        try
        {
            await UniTask.WhenAll(handles.ToArray());
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning($"{this.name}：キャンセルされました。");
            return;
        }
    }

    public override void Initialize()
    {
        if (_rootObject == null)
        {
            Debug.LogError($"{this.name}：[_rootObject]がアタッチされていません。Inspectorを確認してください");
            return;
        }

        _spriteRenderers = _rootObject.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in _spriteRenderers)
        {
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, _startAlpha);
        }
    }

    public async override UniTask PlayAnimationAsync(CancellationToken cancellationToken = default)
    {
        var token = this.GetCancellationTokenOnDestroy();
        try
        {
            await FadeInSpritesAsync(token);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning($"{this.name}：キャンセルされました。");
            return;
        }
    }
}
