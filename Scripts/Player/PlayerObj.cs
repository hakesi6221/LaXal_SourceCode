using System;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// プレイヤーの見た目と移動処理をつなげて一つの処理の関数として管理するクラス
/// ここでプレイヤーの動作を起こす関数を作り、Controllerからそれを呼ぶことで入力に処理を起こす
/// </summary>
public class PlayerObj : MonoBehaviour
{
    [SerializeField, Label("レーン間移動の所要時間"), BoxGroup("MoveParameters")]
    private float _durationSecMov;

    [SerializeField, Label("デフォルトのY座標"), BoxGroup("MoveParameters")]
    private float _defaultPosY = -4f;

    [SerializeField, Label("ジャンプしたときの最高高度と通常時の高さとの差"), BoxGroup("MoveParameters")]
    private float _distanceByJumpMov;

    [SerializeField, Label("レーン間移動の所要時間"), BoxGroup("MoveParameters")]
    private float _durationSecAP;

    [SerializeField, Label("ジャンプしたときの最高高度と通常時の高さとの差"), BoxGroup("MoveParameters")]
    private float _distanceByJumpAP;

    [SerializeField, Label("PlayerViewの参照"), BoxGroup("References")]
    private PlayerView _view = null;

    // 移動処理を担当するクラスのインスタンス
    private PlayerMove _move = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        Initialize();
    }

    /// <summary>
    /// 初期化処理
    /// </summary>
    private void Initialize()
    {
        Vector3 initialPos = Vector3.zero;
        if (GameManager.Instance != null)
            initialPos = GameManager.Instance.GetTargetLanePosition(GameManager.Instance.CurrentLaneIndex);
        initialPos.y = _defaultPosY;
        _view.transform.position = initialPos;
        _move = new PlayerMove(_view.transform, _durationSecMov, _distanceByJumpMov, _durationSecAP, _distanceByJumpAP);
        _move.AddMovedEvent(_view.OnMovedEffect);
    }

    /// <summary>
    /// 1つ右隣のレーンに移動する
    /// </summary>
    public void MoveToRightLane()
    {
        if (_move.IsMove)
            return;
        if (!GameManager.Instance.MoveToRightLane())
            return;

        var token = this.GetCancellationTokenOnDestroy();
        // TODO：移動時のアニメーションを作成&Viewを通して再生

        Vector3 targetPos = GameManager.Instance.GetTargetLanePosition(GameManager.Instance.CurrentLaneIndex);
        targetPos.y = _defaultPosY;
        _move.MoveLaneWithJump(targetPos, token);
    }

    /// <summary>
    /// 1つ左隣のレーンに移動する
    /// </summary>
    public void MoveToLeftLane()
    {
        if (_move.IsMove)
            return;
        if (!GameManager.Instance.MoveToLeftLane())
            return;

        var token = this.GetCancellationTokenOnDestroy();
        // TODO：移動時のアニメーションを作成&Viewを通して再生

        Vector3 targetPos = GameManager.Instance.GetTargetLanePosition(GameManager.Instance.CurrentLaneIndex);
        targetPos.y = _defaultPosY;
        _move.MoveLaneWithJump(targetPos, token);
    }

    /// <summary>
    /// レーンの移動を行わず、その場でアピールをする
    /// </summary>
    public void AppealOnLane()
    {
        // 4レーンモードではないならアピールアクションはできない
        if (RhythmGameInfomation.GameMode != RhythmGameMode.FourLane) return;
        if (_move.IsMove)
            return;
        var token = this.GetCancellationTokenOnDestroy();

        // アピールの判定を行う
        bool judge = GameManager.Instance.AppealChanceJudge(BeatManager.Instance.elapsed);
        AppealChanceManager.Instance?.SetAppealChanceJudge(judge);

        // アピールのアクション
        _move.AppealOnLane(token);
    }
}
