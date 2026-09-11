using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TutorialCommandNote : IDisposable
{
    private double _commandSec;
    private int _commandOrder;
    private TutorialCommandContext _context;

    public double CommandSec => _commandSec;
    public int CommandOrder => _commandOrder;

    public void Dispose()
    {
        _context = null;
    }

    public TutorialCommandNote(double commandSec, int commandOrder, TutorialCommandContext context)
    {
        _commandSec = commandSec;
        _commandOrder = commandOrder;
        _context = context;
    }

    /// <summary>
    /// 番号に応じた行動処理を発火する
    /// </summary>
    protected virtual void FireMoveOrder(CancellationToken cancellationToken)
    {
        _context.FireCommand(_commandOrder, cancellationToken);
    }

    /// <summary>
    /// 楽曲の経過時間が、行動予定時間に達したとき、行動を発火する
    /// 移動処理開始時に呼ぶ想定
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async UniTaskVoid MoveOrderWithCount(CancellationToken cancellationToken)
    {
        if (BeatManager.Instance == null) return;
        try
        {
            await UniTask.WaitUntil(() => _commandSec <= BeatManager.Instance.elapsed
                                    , cancellationToken: cancellationToken);
            FireMoveOrder(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("先輩の動作ノーツのカウントがキャンセルされました");
            return;
        }
    }
}