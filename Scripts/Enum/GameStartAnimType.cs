using UnityEngine;

/// <summary>
/// ゲーム開始時アニメーションの再生の仕方を定義するためのEnum
/// </summary>
public enum GameStartAnimType
{
    None,       // 例外用
    Join,       // 一つ前のアニメーションの頭と同じタイミングで再生
    Append,     // 一つ前のアニメーションの最後に続けて再生
}
