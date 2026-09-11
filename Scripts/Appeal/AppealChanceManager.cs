using System;
using Common.SingleTon;
using UnityEngine;

/// <summary>
/// アピールチャンスの進行などの管理を行うシングルトンクラス
/// </summary>
public class AppealChanceManager : SingletonMonoBehaviour<AppealChanceManager>
{
    protected override bool dontDestroyOnLoad => false;

    // アピールチャンス突入時のイベント
    // 本アピールチャンスの判定タイミングを渡す
    private event Action<double> OnEnterAppealChance;

    // アピールチャンス終了時のイベント
    // 本アピールチャンスの最終成否判定を渡す
    private event Action<bool> OnExitAppealChance;

    // アピールチャンス中の成功判定
    // アピールタイム終了時、最終的な判定を成否判定としてイベント側に渡す
    private bool _appealJudgeResult = false;

    /// <summary>
    /// アピールチャンス突入時のイベントに処理を追加
    /// </summary>
    /// <param name="cb"></param>
    public void AddEnterAppealChanceEvent(Action<double> cb)
    {
        if (cb == null)
        {
            Debug.LogError($"[AddEnterAppealChanceEvent]で渡された値がnullです。");
            return;
        }

        OnEnterAppealChance += cb;
    }

    /// <summary>
    /// アピールチャンス突入時のイベントから処理を削除
    /// </summary>
    /// <param name="cb"></param>
    public void RemoveEnterAppealChanceEvent(Action<double> cb)
    {
        if (cb == null)
        {
            Debug.LogError($"[RemoveEnterAppealChanceEvent]で渡された値がnullです。");
            return;
        }

        OnEnterAppealChance -= cb;
    }

    /// <summary>
    /// アピールチャンス終了時のイベントに処理を追加
    /// </summary>
    /// <param name="cb"></param>
    public void AddExitAppealChanceEvent(Action<bool> cb)
    {
        if (cb == null)
        {
            Debug.LogError($"[AddExitAppealChanceEvent]で渡された値がnullです。");
            return;
        }

        OnExitAppealChance += cb;
    }

    /// <summary>
    /// アピールチャンス終了時のイベントから処理を削除
    /// </summary>
    /// <param name="cb"></param>
    public void RemoveExitAppealChanceEvent(Action<bool> cb)
    {
        if (cb == null)
        {
            Debug.LogError($"[RemoveExitAppealChanceEvent]で渡された値がnullです。");
            return;
        }

        OnExitAppealChance -= cb;
    }

    /// <summary>
    /// アピールチャンスに突入する
    /// </summary>
    /// <param name="elapsed">本アピールチャンスの判定タイミング</param>
    public void EnterAppealChance(double elapsed)
    {
        OnEnterAppealChance?.Invoke(elapsed);
        Debug.Log("アピールタイム：突入");
    }

    /// <summary>
    /// アピールチャンス終了
    /// </summary>
    public void ExitAppealChance()
    {
        bool result = _appealJudgeResult;
        OnExitAppealChance?.Invoke(result);
        Debug.Log($"アピールタイム：{(result ? "成功" : "失敗")}");
        // 終了処理が終わったら判定はリセット
        _appealJudgeResult = false;
    }

    /// <summary>
    /// アピールチャンス中の成否判定を切り替える
    /// アピールチャンス中の判定処理が行われたときに呼ぶ想定
    /// </summary>
    /// <param name="result">判定結果</param>
    public void SetAppealChanceJudge(bool result)
    {
        _appealJudgeResult = result;
    }
}
