using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TutorialDataLoader
{
    // 指令時間を示す列番号
    private const int COMMANDSEC_IDX = 0;
    // 指示を示す列番号
    private const int COMMANDORDER_IDX = 1;

    /// <summary>
    /// 先輩キャラの動作CSVを読み、構造体のキューとして返す
    /// </summary>
    /// <param name="moveMasterData">先輩キャラの動作CSVデータ</param>
    /// <returns></returns>
    public Stack<TutorialCommandNote> LoadTutorialData(TextAsset moveMasterData, TutorialCommandContext context)
    {
        if (moveMasterData == null)
        {
            Debug.LogError("チュートリアル動作データが設定されていません。");
            return new Stack<TutorialCommandNote>();
        }
        string[][] allLines = CSVDataLoader.GetAllLines(moveMasterData).ToArray();

        Stack<TutorialCommandNote> results = new Stack<TutorialCommandNote>();

        // 念のため配列外参照対策
        if (allLines.FirstOrDefault().Length <= COMMANDSEC_IDX
            || allLines.FirstOrDefault().Length <= COMMANDORDER_IDX) return results;

        foreach (string[] line in allLines)
        {
            // 各要素を変数に抽出
            string commandSecSt = line[COMMANDSEC_IDX];
            string commandOrderSt = line[COMMANDORDER_IDX];

            if (!double.TryParse(commandSecSt, out double commandSec))
                Debug.LogError("先輩の動作CSVのロードに失敗しました。移動秒数の値が不正です");
            if (!int.TryParse(commandOrderSt, out int commandOrder))
                Debug.LogError("先輩の動作CSVのロードに失敗しました。移動指示の値が不正です");

            if (commandOrder == 0)
                continue;
            TutorialCommandNote command = new TutorialCommandNote
            (
                commandSec,
                commandOrder,
                context
            );
            results.Push(command);
        }
        return results;
    }
}