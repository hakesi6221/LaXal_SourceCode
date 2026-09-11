using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 先輩の動作CSVを読む担当のクラス
/// </summary>
public class SeniorDataLoader
{
    // 行動時間を示す列番号
    private const int MOVESEC_IDX = 0;
    // 行動指示を示す列番号
    private const int MOVEORDER_IDX = 1;

    private const int ENTERAPPEAL_IDX = 5;
    private const int EXITAPPEAL_IDX = 6;

    /// <summary>
    /// 先輩キャラの動作CSVを読み、構造体のキューとして返す
    /// </summary>
    /// <param name="moveMasterData">先輩キャラの動作CSVデータ</param>
    /// <returns></returns>
    public Stack<SeniorMoveNote> LoadSeniorMoveData(TextAsset moveMasterData)
    {
        if (moveMasterData == null)
        {
            Debug.LogError("先輩の動作CSVが設定されていません");
            return new Stack<SeniorMoveNote>();
        }
        string[][] allLines = CSVDataLoader.GetAllLines(moveMasterData).ToArray();

        Stack<SeniorMoveNote> results = new Stack<SeniorMoveNote>();
        // 直前のアピール終了ノーツの時間を保持
        // CSVの特性上、最後のものから見ていくので保持したものをそのままアピールと突入ノーツに渡せる
        double prevAppealExitSec = 0.0;
        // 念のため配列外参照対策
        if (allLines.FirstOrDefault().Length <= MOVESEC_IDX
            || allLines.FirstOrDefault().Length <= MOVEORDER_IDX) return results;

        foreach (string[] line in allLines)
        {
            // 各要素を変数に抽出
            string moveSecSt = line[MOVESEC_IDX];
            string moveOrderSt = line[MOVEORDER_IDX];

            if (!double.TryParse(moveSecSt, out double moveSec))
                Debug.LogError("先輩の動作CSVのロードに失敗しました。移動秒数の値が不正です");
            if (!int.TryParse(moveOrderSt, out int moveOrder))
                Debug.LogError("先輩の動作CSVのロードに失敗しました。移動指示の値が不正です");

            SeniorMoveNote moveData = null;

            // アピール突入ノーツが来たら、保持していたアピール終了ノーツの時間を渡す
            // CSVの特性上、最後のものから見ていくので保持したものをそのままアピール突入ノーツに渡せる
            if (moveOrder == ENTERAPPEAL_IDX)
                moveData = new SeniorMoveNoteEnterAppeal(moveSec, moveOrder, prevAppealExitSec);
            else
                moveData = new SeniorMoveNote(moveSec, moveOrder);

            // アピール終了ノーツが来たら、時間を保持
            if (moveOrder == EXITAPPEAL_IDX)
                prevAppealExitSec = moveSec;

            results.Push(moveData);
        }
        return results;
    }
}
