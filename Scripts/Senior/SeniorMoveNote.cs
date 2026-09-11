using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 先輩の動作1つをノーツのように扱うためのクラス
/// 行動時間と指示を保持し、このクラス内で指示発火までのカウントを行う
/// Objクラスの方でこのインスタンスを移動の数分保持し、こちらに発火する行動の処理を渡す
///
/// オーバーライド可能オブジェクトあり
/// </summary>
public record SeniorMoveNote : IDisposable
{
    // このノーツの行動時間
    protected double _moveSec;
    // このノーツの行動指示番号
    protected int _moveOrder;
    // objクラスから受け取る行動発火処理
    protected Action<SeniorMoveNote> _onMoveOrder;

    /// <summary>
    /// 先輩の動作1つをノーツのように扱うためのクラス
    /// </summary>
    /// <param name="moveSec">このノーツの行動時間</param>
    /// <param name="moveOrder">このノーツの行動指示番号</param>
    public SeniorMoveNote(double moveSec, int moveOrder)
    {
        _moveSec = moveSec;
        _moveOrder = moveOrder;
    }

    /// <summary>
    /// このノーツの行動時間
    /// </summary>
    public double MoveSec => _moveSec;
    /// <summary>
    /// このノーツの行動指示番号
    /// </summary>
    public int MoveOrder => _moveOrder;

    /// <summary>
    /// obj側の行動処理を登録する
    /// </summary>
    /// <param name="onMoveOrder">行動コールバック処理</param>
    public void SetMoveOrderCallback(Action<SeniorMoveNote> onMoveOrder)
    {
        _onMoveOrder = onMoveOrder;
    }

    public void Dispose()
    {
        _onMoveOrder = null;
    }

    /// <summary>
    /// 番号に応じた行動処理を発火する
    /// </summary>
    protected virtual void FireMoveOrder()
    {
        _onMoveOrder?.Invoke(this);
    }

    /// <summary>
    /// 楽曲の経過時間が、行動予定時間に達したとき、行動を発火する
    /// 移動処理開始時に呼ぶ想定
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async UniTaskVoid MoveOrderWithCount(CancellationToken cancellationToken)
    {
        try
        {
            await UniTask.WaitUntil(() => _moveSec <= BeatManager.Instance.elapsed
                                    , cancellationToken: cancellationToken);
            FireMoveOrder();
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("先輩の動作ノーツのカウントがキャンセルされました");
            return;
        }
    }
}
