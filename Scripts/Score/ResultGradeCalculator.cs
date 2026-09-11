using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// スコアでの評価を計算するクラス
/// </summary>
public class ResultGradeCalculator
{
    // スコア評価とボーダーの倍率を保持する連想配列
    // そこまで大きなデータではないため、ハードコードで対応
    private readonly Dictionary<ResultGrade, float> _resultBorderRatios = new Dictionary<ResultGrade, float>
    {
        { ResultGrade.SS, 101f },
        { ResultGrade.S, 91f },
        { ResultGrade.A, 71f },
        { ResultGrade.B, 51f },
        { ResultGrade.F, 0f },
    };

    /// <summary>
    /// 最終スコアからリザルト評価を計算する
    /// </summary>
    /// <param name="maxScore"></param>
    /// <param name="finalScore"></param>
    /// <returns></returns>
    public ResultGrade CalcResultGrade(int maxScore, int finalScore)
    {
        ResultGrade result = ResultGrade.None;

        float resultRatio = ((float)finalScore / (float)maxScore) * 100f;

        foreach (var border in _resultBorderRatios)
        {
            // 見ているランクのボーダーを超えているのであれば、そのランクを結果とする
            if (border.Value <= resultRatio)
            {
                result = border.Key;
                break;
            }
        }

        return result;
    }
}
