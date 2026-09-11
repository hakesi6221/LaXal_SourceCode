using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// リズムゲームの判定処理を担当するクラス
/// 多くの場所から呼ばれる想定なので、シングルトンクラス
/// ここで判定結果を定義し、それをエフェクト生成やスコア管理に渡すことでエフェクト生成とスコアの更新を行う
///
/// 呼ばれたタイミングで
/// </summary>
[Serializable]
public class RhythmJudgeManager
{
    [SerializeField, Label("リズムゲーム判定の処理クラス")]
    private RhythmJudgeModel _model = new RhythmJudgeModel();

    // レーン番号
    private int _laneNum = 4;

    // 判定処理が行われた際に、その判定結果を引数として発火するコールバックイベント
    private event Action<JudgeResult> _onJudgeEvent;

    /// <summary>
    /// 判定結果ごとの判定数保存用連想配列
    /// </summary>
    public Dictionary<JudgeResult, int> JudgeCount => _model.JudgeCount;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public void Initialize(int laneNum)
    {
        _laneNum = laneNum;
        _model.Initialize(_laneNum);
        BeatManager.Instance.AddOnBeatEvent(() => LongNotesJudgeOnLanes());
    }

    /// <summary>
    /// 判定時コールバックイベントに処理を登録する
    /// 戻り値なし/JudgeResult型の引数/の二条件を満たす処理を入れる必要あり
    /// </summary>
    /// <param name="cb">登録したい処理</param>
    public void AddOnJudgeEvent(Action<JudgeResult> cb)
    {
        if (cb == null)
        {
            if (cb == null)
            {
                Debug.LogError($"{typeof(RhythmJudgeManager).Name}:判定時のコールバックが正しくない形で渡されています。");
                return;
            }

            return;
        }
        _onJudgeEvent += cb;
    }

    /// <summary>
    /// 判定時コールバックイベントから処理を削除する
    /// 戻り値なし/JudgeResult型の引数/の二条件を満たす処理を入れる必要あり
    /// </summary>
    /// <param name="cb">登録したい処理</param>
    public void RemoveOnJudgeEvent(Action<JudgeResult> cb)
    {
        if (cb == null)
        {
            if (cb == null)
            {
                Debug.LogError($"{typeof(RhythmJudgeManager).Name}:判定時のコールバックが正しくない形で渡されています。");
                return;
            }

            return;
        }
        _onJudgeEvent -= cb;
    }

    private void LongNotesJudgeOnLanes()
    {
        bool[] flags = _model.LongNoteJudging;
        if (flags == null) return;

        for (int i = 0; i < flags.Length; i++)
        {
            if (flags[i])
                LongNotesJudge(i);
        }
    }

    /// <summary>
    /// ノーツが判定ラインに乗った際に呼ばれる判定処理
    /// 引数で渡されたノーツのレーン番号と今のプレイヤーのレーン番号が一致していないのであればGreat(今はPerfect)
    /// 一致していればMiss
    ///
    /// その判定をもとに、エフェクトの生成とスコアの更新の関数を呼ぶ
    /// </summary>
    /// <param name="noteIndex">ノーツのCSV番号</param>
    /// <param name="laneIndex">ノーツのレーン番号</param>
    public void JudgeOnNotes(int noteIndex, int laneIndex)
    {
        var result = _model.Judge(_laneNum, laneIndex);

        AfterNoteProcress(noteIndex, laneIndex);
        EffectGenerator.Instance?.PlayJudgeEffect(result);
        _onJudgeEvent?.Invoke(result);
        PlayJudgeSound(result);
    }

    /// <summary>
    /// アピールチャンスの判定タイミングを設定する
    /// アピールチャンス突入時に呼ぶ想定
    /// </summary>
    /// <param name="judgeElapsedSec">アピールチャンスの判定タイミングの経過時間</param>
    public void SetAppealJudgeSec(double judgeElapsedSec)
    {
        _model.SetAppealJudgeSec(judgeElapsedSec);
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
        return _model.AppealChanceJudge(elapsed);
    }

    /// <summary>
    /// ロングノーツ時の判定処理
    /// </summary>
    private void LongNotesJudge(int laneIndex)
    {
        JudgeResult result = _model.LongNotesJudge(laneIndex);
        EffectGenerator.Instance?.PlayJudgeEffect(result);
        _onJudgeEvent?.Invoke(result);
    }

    /// <summary>
    /// ノーツの判定を行った後の事後処理を行う
    /// ノーツの種類によって変化する
    /// （ロングノーツの処理など）
    /// </summary>
    /// <param name="noteIndex"></param>
    private void AfterNoteProcress(int noteIndex, int laneIndex)
    {
        NoteType noteType = (NoteType)noteIndex;

        // Debug.Log($"判定:{noteType}");
        switch (noteType)
        {
            case NoteType.Long_Start:
                Debug.Log($"ロングノーツ開始:{laneIndex}レーン");
                _model.OnLongNote(laneIndex, true);
                break;
            case NoteType.Long_End:
                Debug.Log($"ロングノーツ終了:{laneIndex}レーン");
                _model.OnLongNote(laneIndex, false);
                SoundManager.Instance?.StopSE(AudioType.SE_longnotes);
                break;
            default:
                break;
        }
    }

    private void PlayJudgeSound(JudgeResult result)
    {
        switch (result)
        {
            case JudgeResult.Miss:
                SoundManager.Instance.PlaySE(AudioType.SE_miss);
                break;
        }

    }
}
