/// <summary>
/// 先輩の動作1つをノーツのように扱うためのクラス
/// アピールチャンス突入ノーツ用の、子クラス
/// アピールチャンスの判定タイミングの変数とそのプロパティを持っている
/// </summary>
public record SeniorMoveNoteEnterAppeal : SeniorMoveNote
{
    // アピールチャンスの判定タイミング
    private double _appealChanceJudgeSec = 0.0;

    public SeniorMoveNoteEnterAppeal(double moveSec, int moveOrder, double appealChanceExitSec) : base(moveSec, moveOrder)
    {
        _appealChanceJudgeSec = appealChanceExitSec;
    }

    /// <summary>
    /// アピールチャンスの判定タイミング
    /// </summary>
    public double AppealChanceJudgeSec => _appealChanceJudgeSec;
}
