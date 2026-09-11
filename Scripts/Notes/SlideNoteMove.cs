using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using NaughtyAttributes;
using UnityEngine;


/// <summary>
/// NoteMoveを継承した、スライド特殊ノーツの移動用クラス
/// TODO:スライド処理及びそのパラメータの体裁を整えたい
/// </summary>
public class SlideNoteMove : NoteMove
{
    [SerializeField, Label("横移動するローカルY座標")]
    private float _slideLocalY = 40.0f;

    [SerializeField, Label("横移動の所要秒数")]
    private float _slideDurationSec = 2.0f;

    [SerializeField, Label("移動時のイージング")]
    private Ease  _slideEase = Ease.InSine;

    // スライドした回数
    private bool _slided = false;

    // 特殊ノーツの生成位置
    private int _initLaneIndex;

    protected override void UpdatePosition()
    {
        base.UpdatePosition();

        float localPosY = transform.localPosition.y;

        // 一度もスライドしていないかつ、座標の条件を満たしているのであれば、別のレーンにスライド移動する
        if (localPosY <= _slideLocalY && !_slided)
        {
            _slided = true;
            SlideToTargetLane(_initLaneIndex, _laneIndex);
        }
    }

    private async void SlideToTargetLane(int fromLaneIndex, int tolaneIndex)
    {
        float fromLanePosX = GameManager.Instance.GetTargetLanePosition(fromLaneIndex).x;
        float targetLanePosX = GameManager.Instance.GetTargetLanePosition(tolaneIndex).x;

        try
        {
            await LMotion.Create(fromLanePosX
                                , targetLanePosX
                                , _slideDurationSec)
                        .WithEase(_slideEase)
                        .BindToPositionX(transform)
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
        Vector3 currentNotePos = transform.position;

        switch (laneIndex)
        {
            case 0:
                _initLaneIndex = 3;
                break;
            case 1:
                _initLaneIndex = 2;
                break;
            case 2:
                _initLaneIndex = 1;
                break;
            case 3:
                _initLaneIndex = 0;
                break;
        }
        float fromLanePosX = GameManager.Instance.GetTargetLanePosition(_initLaneIndex).x;
        currentNotePos.x = fromLanePosX;
        transform.position = currentNotePos;
        base.Initialize(generatePos, hitTime, noteSpeed, moveDirection, laneIndex, noteIndex, visuableDistance);
        Debug.Log("特殊:ノーツ発生中");

    }
}
