using UnityEngine;
using LitMotion;
using LitMotion.Extensions;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

/// <summary>
/// プレイヤーの動作ロジックを担当するクラス
/// MonoBehaviourを継承しておらず、移動ロジックを使用したいクラスでインスタンスを生成し使用する想定
///
/// レーンの移動やアピール行動の非同期関数をもっている
/// </summary>
public class PlayerMove
{
    // プレイヤーのTransform
    private Transform _transform;

    // 移動：レーン間移動の所要時間
    private float _durationSecMov;

    // 移動：ジャンプしたときの最高高度と通常時の高さとの差
    private float _distanceByJumpMov;

    // アピール：アピールの所要時間
    private float _durationSecAP;

    // アピール：ジャンプしたときの最高高度と通常時の高さとの差
    private float _distanceByJumpAP;

    // 初期状態でのYワールド座標。ジャンプ時の処理に使用
    private float _defaultPosY = 0f;

    // 移動処理用のCTS
    private CancellationTokenSource _ctsMov = new CancellationTokenSource();

    // アピール処理用のCTS
    private CancellationTokenSource _ctsAP = new CancellationTokenSource();

    // 移動完了時に起こるイベント
    private event Action _movedEvent;

    // 今移動中か
    private bool _isMove = false;

    /// <summary>
    /// 今移動中か
    /// </summary>
    public bool IsMove => _isMove;

    public void AddMovedEvent(Action cb)
    {
        if (cb == null)
        {
            Debug.LogError($"{typeof(PlayerMove).Name}:移動完了後のコールバックが不正な形で渡されています");
            return;
        }
        _movedEvent += cb;
    }

    public void RemoveMovedEvent(Action cb)
    {
        if (cb == null)
        {
            Debug.LogError($"{typeof(PlayerMove).Name}:移動完了後のコールバックが不正な形で渡されています");
            return;
        }
        _movedEvent -= cb;
    }

    /// <summary>
    /// プレイヤーの動作ロジックを担当するクラス
    /// </summary>
    /// <param name="transform">プレイヤーのTransform</param>
    /// <param name="durationSecMov">移動：レーン間移動の所要時間</param>
    /// <param name="distanceByJumpMov">移動：ジャンプしたときの最高高度と通常時の高さとの差</param>
    /// <param name="durationSecAP">アピール：アピールの所要時間</param>
    /// <param name="distanceByJumpAP">アピール：ジャンプしたときの最高高度と通常時の高さとの差</param>
    public PlayerMove(Transform transform, float durationSecMov, float distanceByJumpMov, float durationSecAP, float distanceByJumpAP)
    {
        _transform = transform;
        _durationSecMov = durationSecMov;
        _distanceByJumpMov = distanceByJumpMov;
        _durationSecAP = durationSecAP;
        _distanceByJumpAP = distanceByJumpAP;
        if (_transform == null)
        {
            Debug.LogError("PlayerMove：インスタンス生成時に渡されたTransformがnullになっています。確認してください。");
            return;
        }

        _defaultPosY = transform.position.y;
    }

    /// <summary>
    /// ジャンプのモーションを行う
    /// 待機可能
    /// </summary>
    /// <param name="totalDuration">モーション全体の所要時間</param>
    /// <returns></returns>
    private async UniTask Jump(float totalDuration, float jumpDistance)
    {
        var jumpUpHandle = LMotion.Create(_defaultPosY, _defaultPosY + jumpDistance, totalDuration / 2f)
                                .WithEase(Ease.OutExpo)
                                .BindToPositionY(_transform);
        var jumpDownHandle = LMotion.Create(_defaultPosY + jumpDistance, _defaultPosY, totalDuration / 2f)
                                    .WithEase(Ease.InExpo)
                                    .BindToPositionY(_transform);
        var jumpSequence = LSequence.Create()
                                    .Append(jumpUpHandle)
                                    .Append(jumpDownHandle);

        await jumpSequence.Run()
                        .ToUniTask();
    }

    /// <summary>
    /// レーン移動の行動
    /// ジャンプをしつつ、隣のレーンまで移動する
    /// レーンの移動は、引数に移動したいレーンのワールド座標を渡すことで行う
    /// </summary>
    /// <param name="nextLanePos">隣のレーンのワールド座標</param>
    /// <param name="cancellationToken">Unitaskキャンセルトークン</param>
    public async void MoveLaneWithJump(Vector3 nextLanePos, CancellationToken cancellationToken)
    {
        _isMove = true;
        _ctsMov.Cancel();
        _ctsMov = new CancellationTokenSource();
        var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(_ctsMov.Token, cancellationToken);
        var linkedToken = linkedSource.Token;

        var moveHandle = LMotion.Create(_transform.position.x, nextLanePos.x, _durationSecMov)
                                .WithEase(Ease.Linear)
                                .BindToPositionX(_transform)
                                .ToUniTask();

        try
        {
            await UniTask.WhenAll(moveHandle, Jump(_durationSecMov, _distanceByJumpMov))
                        .AttachExternalCancellation(linkedToken);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning($"プレイヤーのレーン移動中に動作がキャンセルされました");
        }
        if (linkedToken.IsCancellationRequested)
        {
            _isMove = false;
            return;
        }

        _movedEvent?.Invoke();
        _isMove = false;
    }

    /// <summary>
    /// アピールの行動
    /// レーンは動かずその場でジャンプする
    /// 一回転はPresenter側でViewから呼ぶ
    /// </summary>
    /// <param name="cancellationToken"></param>
    public async void AppealOnLane(CancellationToken cancellationToken)
    {
        _isMove = true;
        _ctsAP.Cancel();
        _ctsAP = new CancellationTokenSource();
        var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(_ctsAP.Token, cancellationToken);
        var linkedToken = linkedSource.Token;

        try
        {
            await Jump(_durationSecAP, _distanceByJumpAP).AttachExternalCancellation(linkedToken);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning($"プレイヤーのアピール中に動作がキャンセルされました");
        }
        if (linkedToken.IsCancellationRequested)
        {
            _isMove = false;
            return;
        }

        _isMove = false;
    }
}
