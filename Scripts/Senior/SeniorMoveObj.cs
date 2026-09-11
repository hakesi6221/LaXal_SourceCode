using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Cysharp.Threading.Tasks;

/// <summary>
/// 先輩の実際に動かすクラス
/// 動き一つ一つのノーツを生成、保持し、ノーツ側に処理の発火を任せる
/// </summary>
[RequireComponent(typeof(SeniorSpriteView))]
public class SeniorMoveObj : MonoBehaviour
{
    [SerializeField, Label("レーン移動所要秒"), Foldout("移動パラメータ")]
    private float _laneMoveDuration = 0.2f;

    [SerializeField, Label("レーン移動時ジャンプ高度"), Foldout("移動パラメータ")]
    private float _laneMoveJumpHeight = 1.0f;

    // 先輩の動作ノーツを保持するStack
    private Stack<SeniorMoveNote> _moveDatas = new Stack<SeniorMoveNote>();
    // 先輩の動作CSVデータ
    private TextAsset _moveMasterData = null;

    // 先輩の動作CSVを読み込むクラス
    private SeniorDataLoader _loader = new SeniorDataLoader();

    // Sprite管理
    private SeniorSpriteView _view = null;

    // 移動処理用クラス
    private PlayerMove _move = null;

    void OnEnable()
    {
        // 初期化はEnable時に行う
        Initialize();
    }

    /// <summary>
    /// 初期化処理
    /// 各クラスの定義とデータの格納
    /// </summary>
    public void Initialize()
    {
        _move = new PlayerMove(transform, _laneMoveDuration, _laneMoveJumpHeight, 0f, 0f);
        _view = GetComponent<SeniorSpriteView>();
        _view.Initialize();

        if (RhythmGameInfomation.GameMode != RhythmGameMode.FourLane) return;
        _moveMasterData = RhythmGameInfomation.SeniorMove;
        // マスターデータをロード、キューに格納
        _moveDatas = _loader.LoadSeniorMoveData(_moveMasterData);
        GameManager.Instance.AddGameStartEvent(StartMove);
    }

    /// <summary>
    /// 移動処理のカウントの開始
    /// </summary>
    public void StartMove()
    {
        if (_moveDatas.Count <= 0) return;

        // アピール用のノーツも生成
        GameManager.Instance.GenerateAppealNotes(_moveDatas.ToArray());
        foreach (var moveData in _moveDatas)
        {
            moveData.SetMoveOrderCallback(MovebyOrder);
            moveData.MoveOrderWithCount(this.GetCancellationTokenOnDestroy()).Forget();
        }
    }

    /// <summary>
    /// 移動指示番号に応じて行動を行う
    /// ノーツ側に渡して発火してもらう想定
    /// </summary>
    /// <param name="note">指示番号</param>
    private void MovebyOrder(SeniorMoveNote note)
    {
        if (note == null)
        {
            Debug.LogError($"{this.name}:先輩キャラの移動処理が不正に呼ばれました");
            return;
        }
        if (_moveDatas.Count <= 0) return;

        int order = note.MoveOrder;
        switch (order)
        {
            // 1～4はレーン移動
            case 1:
            case 2:
            case 3:
            case 4:
                MoveTargetLane(order - 1);
                break;
            // 5はアピール開始、6は終了
            case 5:
                AppealChanceStart(note);
                break;
            case 6:
                AppealChanceEnd();
                break;
            default:
                break;
        }
        // 行動後、ノーツをStackから削除、Dispose
        var moveData = _moveDatas.Pop();
        moveData.Dispose();
    }

    /// <summary>
    /// 指定のレーンに自信を移動させる
    /// </summary>
    /// <param name="laneIndex">レーン番号</param>
    private void MoveTargetLane(int laneIndex)
    {
        if (_move == null) return;

        // 移動先座標を取得
        Vector3 targetPos = GameManager.Instance.GetTargetLanePosition(laneIndex);
        _move.MoveLaneWithJump(targetPos, this.GetCancellationTokenOnDestroy());
    }

    /// <summary>
    /// アピールチャンス開始
    /// </summary>
    private void AppealChanceStart(SeniorMoveNote note)
    {
        // アピール突入ノーツじゃないもので呼ばれているならそれは想定外
        if (!(note is SeniorMoveNoteEnterAppeal))
        {
            Debug.LogError($"{this.name}:アピール突入処理が不正なタイミングで呼ばれました。ノーツ生成処理を確認してください");
            return;
        }

        SeniorMoveNoteEnterAppeal noteEnterAppeal = note as SeniorMoveNoteEnterAppeal;
        AppealChanceManager.Instance?.EnterAppealChance(noteEnterAppeal.AppealChanceJudgeSec);
    }

    /// <summary>
    /// アピールチャンス終了
    /// </summary>
    private void AppealChanceEnd()
    {
        AppealChanceManager.Instance?.ExitAppealChance();
    }
}
