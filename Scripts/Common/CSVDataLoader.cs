using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public static class CSVDataLoader
{
    /// <summary>
    /// CSVの行を分割し、string型の配列として返す
    /// カンマで区切るが、カンマが"の間に囲まれていた場合は無視する
    /// </summary>
    /// <param name="line">分割したい行</param>
    /// <returns></returns>
    private static string[] SplitCSVLine(string line)
    {
        // 1区切りの文字列を作るためのリスト
        StringBuilder currentColumn = new StringBuilder();
        // 分割した文字列を格納しておく、戻り値用のリスト
        List<string> splitedLine = new List<string>();
        // "の間の文字を確認しているかどうかのフラグ
        bool inDoubleQuotes = false;

        // 行の文字列を1文字ずつ確認していく
        foreach (char @char in line)
        {
            // "があった場合、フラグを反転させる
            if (@char == '"')
            {
                inDoubleQuotes = !inDoubleQuotes;
                continue;
            }

            // ,があり、さらに"で囲まれていない位置ならば、そこまでの文字で1区切りとする
            if (@char == ',' && !inDoubleQuotes)
            {
                splitedLine.Add(currentColumn.ToString());
                currentColumn.Clear();
                continue;
            }

            // それ以外の文字列は、1区切りの文字列として保管しておく
            currentColumn.Append(@char);
        }

        // 最後の列を追加する
        splitedLine.Add(currentColumn.ToString());
        currentColumn.Clear();
        return splitedLine.ToArray();
    }

    /// <summary>
    /// CSVファイルのすべての行をstring配列の列挙形式で取得する
    /// </summary>
    /// <param name="textAsset">対象のCSVファイル</param>
    /// <returns></returns>
    public static IEnumerable<string[]> GetAllLines(TextAsset textAsset)
    {
        if (textAsset == null) yield break;

        StringReader reader = new StringReader(textAsset.text);

        // 1行の文字列の箱
        string raw = string.Empty;

        // すべての行を見終わるまで回す
        while (raw != null)
        {
            // 行を読み込む
            raw = reader.ReadLine();

            // 行が終わった場合は終了
            if (string.IsNullOrEmpty(raw)) yield break;
            // その行を分割
            string[] columns = SplitCSVLine(raw);

            yield return columns;
        }
    }

    /// <summary>
    /// 指定の行数の行をstringの列挙形式で取得する
    /// 1行目は0
    /// </summary>
    /// <param name="textAsset">対象のCSVファイル</param>
    /// <param name="targetRaw">対象の行数</param>
    /// <returns></returns>
    public static IEnumerable<string> GetTargetRaw(TextAsset textAsset, int targetRaw)
    {
        if (textAsset == null) yield break;
        if (targetRaw < 0) yield break;

        StringReader reader = new StringReader(textAsset.text);

        // 1行の文字列の箱
        string raw = string.Empty;

        int rawIndex = 0;
        // すべての行を見終わるまで回す
        while (raw != null)
        {
            // 行を読み込む
            raw = reader.ReadLine();
            // 行が終わった場合は終了
            if (string.IsNullOrEmpty(raw)) yield break;

            // 対象の行番号を読んでいるのであれば、返す
            if (targetRaw == rawIndex)
            {
                // その行を分割
                string[] columns = SplitCSVLine(raw);

                foreach (string column in columns)
                    yield return column;

                // 見つかったなら終了
                yield break;
            }

            rawIndex++;
        }
    }

    /// <summary>
    /// 指定の行数の列をstringの列挙形式で取得する
    /// 1行目は0
    /// </summary>
    /// <param name="textAsset">対象のCSVファイル</param>
    /// <param name="targetColumn">対象の列数</param>
    /// <returns></returns>
    public static IEnumerable<string> GetTargetColumn(TextAsset textAsset, int targetColumn)
    {
        if (textAsset == null) yield break;
        if (targetColumn < 0) yield break;

        StringReader reader = new StringReader(textAsset.text);

        // 1行の文字列の箱
        string raw = string.Empty;

        // すべての行を見終わるまで回す
        while (raw != null)
        {
            // 行を読み込む
            raw = reader.ReadLine();

            // 行が終わった場合は終了
            if (string.IsNullOrEmpty(raw)) yield break;
            // その行を分割
            string[] columns = SplitCSVLine(raw);

            for (int columnIndex = 0; columnIndex < columns.Length; columnIndex++)
            {
                // 対象の列番号なら返す
                if (columnIndex == targetColumn)
                {
                    yield return columns[columnIndex];
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 指定の場所の文字列をstringで取得する
    /// 1行目は0
    /// </summary>
    /// <param name="textAsset">対象のCSVファイル</param>
    /// <param name="targetRaw">対象の行数</param>
    /// <param name="targetColumn">対象の列数</param>
    /// <returns></returns>
    public static string GetTargetCell(TextAsset textAsset, int targetRaw, int targetColumn)
    {
        if (textAsset == null) return string.Empty;
        if (targetRaw < 0) return string.Empty;
        if (targetColumn < 0) return string.Empty;

        StringReader reader = new StringReader(textAsset.text);

        // 1行の文字列の箱
        string raw = string.Empty;

        int rawIndex = 0;
        // すべての行を見終わるまで回す
        while (raw != null)
        {
            // 行を読み込む
            raw = reader.ReadLine();

            // 行が終わった場合は終了
            if (string.IsNullOrEmpty(raw)) return string.Empty;

            // 対象の行ではないならスキップ
            if (rawIndex != targetRaw) continue;
            // その行を分割
            string[] columns = SplitCSVLine(raw);

            // 行で回す
            for (int columnIndex = 0; columnIndex < columns.Length; columnIndex++)
            {
                // 対象の列番号なら返す
                if (columnIndex == targetColumn)
                    return columns[columnIndex];
            }
            rawIndex++;
        }

        // 見つからなかった場合は空のstringを返す
        return string.Empty;
    }
}
