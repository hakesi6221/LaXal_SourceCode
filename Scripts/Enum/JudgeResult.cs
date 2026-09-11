/// <summary>
/// 判定した結果の種類
/// 1から、インデックスが高い順にランクが高い
/// </summary>
public enum JudgeResult
{
    None = 0,       // 例外処理用
    Perfect = 1,    // 最も高い評価
    Great = 2,      // 2番目に高い評価
    Miss = 3,       // 失敗判定
}