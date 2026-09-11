using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// リズムゲーム判定の処理部分を担当するクラス
/// RhythmJudgeManagerでインスタンスを生成し、判定処理に使用
/// </summary>
[Serializable]
public class RhythmJudgeModel
{
    // PERFECT判定の閾値
    [SerializeField, Label("PERFECT判定の閾値（単位：秒）")]
    private double _perfectThreshold = 0.3;

    [SerializeField, Label("アピール成功の閾値（単位：秒）")]
    private double _appealSuccessThreshold = 0.3;

    [SerializeField, Label("ロングノーツ中の判定")]
    private JudgeResult _whileLongNoteJudge = JudgeResult.Great;

    // 判定結果ごとの判定数保存用連想配列
    private Dictionary<JudgeResult, int> _judgeCount = new Dictionary<JudgeResult, int>();

    [SerializeField]
    // ロングノーツの判定中かのフラグ配列(レーンの数分)
    private bool[] _longNoteJudging;

    // アピールチャンスの判定タイミングの経過時間
    private double _appealJudgeElapsedSec = 0.0;

    /// <summary>
    /// ロングノーツの判定中かのフラグ配列(レーンの数分)
    /// </summary>
    public bool[] LongNoteJudging => _longNoteJudging;

    /// <summary>
    /// 判定結果ごとの判定数保存用連想配列
    /// </summary>
    public Dictionary<JudgeResult, int> JudgeCount
    {
        get
        {
            if (_judgeCount == null || !_judgeCount.Any())
            {
                Debug.LogError($"{typeof(RhythmGameInfomation)}:判定結果のカウント配列が初期化されていません");
                return null;
            }

            return _judgeCount;
        }
    }

    /// <summary>
    /// 初期化関数
    /// 判定結果の配列を初期化する
    /// </summary>
    public void Initialize(int laneAmount)
    {
        // 判定結果の種類文要素を作る
        foreach (JudgeResult judge in Enum.GetValues(typeof(JudgeResult)))
        {
            if (!_judgeCount.TryAdd(judge, 0))
            {
                Debug.LogWarning($"{typeof(JudgeResult)}:同じ名前の判定結果が複数存在しています。");
            }
        }
        // ロングノーツの判定中かのフラグ配列を初期化
        _longNoteJudging = new bool[laneAmount];
        for (int i = 0; i < laneAmount; i++)
        {
            _longNoteJudging[i] = false;
        }
    }

    /// <summary>
    /// 判定結果のカウントの1増やす関数
    /// 判定時に呼ばれる想定
    /// </summary>
    /// <param name="result">判定結果</param>
    private void IncreaseJudgeCount(JudgeResult result)
    {
        if (!_judgeCount.TryGetValue(result, out int value))
        {
            Debug.LogError($"{typeof(RhythmJudgeModel)}:判定結果のカウント配列の初期化に失敗しています。");
            return;
        }

        _judgeCount[result] = value + 1;
    }

    public JudgeResult Judge(int laneAmount, int laneIndex)
    {
        switch (laneAmount)
        {
            case 3:
                return Judge_3Lane(laneIndex);
            case 4:
                return Judge_4Lane(laneIndex);
            default:
                return JudgeResult.None;
        }
    }

    /// <summary>
    /// ノーツが判定ラインに乗った際の判定処理 4レーンバージョン
    /// </summary>
    /// <param name="laneIndex">ノーツのレーン番号</param>
    /// <returns></returns>
    public JudgeResult Judge_4Lane(int laneIndex)
    {
        JudgeResult result = JudgeResult.None;
        if (laneIndex == GameManager.Instance.CurrentLaneIndex)
            result = JudgeResult.Miss;
        else
        {
            Debug.Log($"成功判定：{GameManager.Instance.TimeSecOnMoveLane}");
            if (laneIndex == GameManager.Instance.PrevLaneIndex
                && GameManager.Instance.TimeSecOnMoveLane < _perfectThreshold)
                result = JudgeResult.Great;
            else
                result = JudgeResult.Perfect;
        }

        IncreaseJudgeCount(result);
        return result;
    }

    /// <summary>
    /// ノーツが判定ラインに乗った際の判定処理 3レーンバージョン
    /// </summary>
    /// <param name="laneIndex">ノーツのレーン番号</param>
    /// <returns></returns>
    public JudgeResult Judge_3Lane(int laneIndex)
    {
        JudgeResult result = JudgeResult.None;
        if (laneIndex == GameManager.Instance.CurrentLaneIndex)
            result = JudgeResult.Miss;
        else
            result = JudgeResult.Perfect;

        IncreaseJudgeCount(result);
        return result;
    }

    /// <summary>
    /// ロングノーツが来た時の処理
    /// ロングノーツが来たレーンのフラグのオンオフを切り替える
    /// </summary>
    /// <param name="laneIndex"></param>
    /// <param name="isJudge"></param>
    public void OnLongNote(int laneIndex, bool isJudge)
    {
        _longNoteJudging[laneIndex] = isJudge;
    }

    /// <summary>
    /// ロングノーツ中の判定処理
    /// 事前に定義した判定を返す
    /// </summary>
    /// <returns></returns>
    public JudgeResult LongNotesJudge(int laneIndex)
    {
        JudgeResult result = (GameManager.Instance.CurrentLaneIndex != laneIndex)
                            ? _whileLongNoteJudge
                            : JudgeResult.Miss;
        IncreaseJudgeCount(result);
        return result;
    }

    /// <summary>
    /// アピールチャンスの判定タイミングを設定する
    /// アピールチャンス突入時に呼ぶ想定
    /// </summary>
    /// <param name="judgeElapsedSec">アピールチャンスの判定タイミングの経過時間</param>
    public void SetAppealJudgeSec(double judgeElapsedSec)
    {
        _appealJudgeElapsedSec = judgeElapsedSec;
    }

    /// <summary>
    /// アピールチャンスの判定を行う
    /// プレイヤーがアピールをした際に呼ぶ想定
    /// あらかじめ設定されたタイミングとの誤差が+-で閾値いないなら成功とし、
    /// 成功したか否かをbool値で返す
    /// </summary>
    /// <param name="elapsed">アピールしたときの経過時間</param>
    public bool AppealChanceJudge(double elapsed)
    {
        // 判定タイミングとの差
        double gap = Mathf.Abs((float)(_appealJudgeElapsedSec - elapsed));
        // 誤判定を防ぐため判定タイミングをリセット
        _appealJudgeElapsedSec = 0.0;
        return gap <= _appealSuccessThreshold;
    }
}
