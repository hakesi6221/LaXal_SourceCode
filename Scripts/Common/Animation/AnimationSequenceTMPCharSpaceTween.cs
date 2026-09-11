using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

/// <summary>
/// MonoBehaviourAnimSequencerの要素となるアニメーションを作るための派生クラス
/// 設定されたTMPオブジェクトの文字の間隔を少しずつ変化させる
/// </summary>
public class AnimationSequenceTMPCharSpaceTween : AnimationSeqenceElementBase
{
    [SerializeField, Label("対象のTMP")]
    private TextMeshProUGUI _tmp = null;

    [SerializeField, Label("開始までのディレイ：秒")]
    private float _delayDuration = 0f;

    [SerializeField, Label("所要時間：秒")]
    private float _duration = 0.5f;

    [SerializeField, Label("開始幅値")]
    private float _startSpace = 0.0f;

    [SerializeField, Label("終了幅値")]
    private float _finishSpace = 10.0f;

    [SerializeField, Label("イージング")]
    private Ease _fadeEase = Ease.InQuad;

    private CharSpaceAnimTmp _anim = new CharSpaceAnimTmp();

    public override void Initialize()
    {
        if (_tmp == null)
        {
            Debug.LogError($"{this.name}：[_tmp]がアタッチされていません。Inspectorを確認してください");
            return;
        }

        _tmp.characterSpacing = _startSpace;
    }

    public async override UniTask PlayAnimationAsync(CancellationToken cancellationToken = default)
    {
        var token = this.GetCancellationTokenOnDestroy();
        if (_tmp == null)
        {
            Debug.LogError($"{this.name}：[_tmp]がアタッチされていません。Inspectorを確認してください");
            return;
        }

        await _anim.PlayAnim
                    (
                        _tmp,
                        _finishSpace,
                        _duration,
                        _delayDuration,
                        _fadeEase,
                        token
                    );

        _tmp.characterSpacing = _finishSpace;
    }
}
