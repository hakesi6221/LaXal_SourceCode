using UnityEngine;
using NaughtyAttributes;
using LitMotion;
using System;
using LitMotion.Extensions;
using Cysharp.Threading.Tasks;

public class FlyingNoteMove : NoteMove
{
    [SerializeField, Label("生成時のローカルZ座標(高さ)")]
    private float _initialLocalZ = 10.0f;

    [SerializeField, Label("急降下し始めるローカルY座標")]
    private float _fallingLocalY = 70.0f;

    [SerializeField, Label("急降下の所要時間")]
    private float _fallDurationSec = 2.0f;

    [SerializeField, Label("移動時のイージング")]
    private Ease  _fallEase = Ease.InSine;

    // 急降下済みか
    private bool _hasFallen = false;

    private float _generatedLocalZ = 0.0f;

    protected override void UpdatePosition()
    {
        base.UpdatePosition();

        float localPosY = transform.localPosition.y;

        // 一度もスライドしていないかつ、座標の条件を満たしているのであれば、別のレーンにスライド移動する
        if (localPosY <= _fallingLocalY && !_hasFallen)
        {
            _hasFallen = true;
            SlideToTargetLane();
        }
    }

    private async void SlideToTargetLane()
    {
        try
        {
            await LMotion.Create(_initialLocalZ
                                , _generatedLocalZ
                                , _fallDurationSec)
                        .WithEase(_fallEase)
                        .BindToLocalPositionZ(transform)
                        .ToUniTask(this.GetCancellationTokenOnDestroy());
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("非同期処理のキャンセル");
            return;
        }
    }

    // ノーツが最終的に到着する点は分かっている。
    // 最初の座標を決定する
    public override void Initialize(Transform generatePos, float hitTime, float noteSpeed, Vector3 moveDirection, int laneIndex, int noteIndex, float visuableDistance)
    {
        base.Initialize(generatePos, hitTime, noteSpeed, moveDirection, laneIndex, noteIndex, visuableDistance);
        Vector3 currentNotePos = transform.localPosition;
        _generatedLocalZ = currentNotePos.z;
        currentNotePos.z = _initialLocalZ;
        transform.localPosition = currentNotePos;
        Debug.Log("特殊ノーツ：エンドレススカイ");
    }
}
