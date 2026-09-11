/// <summary>
/// メインゲームの進行状態を示すEnum
/// </summary>
public enum ProgressStatus
{
    None = -1,
    PreStart,   // 開始前
    Main,       // 楽曲中
    Finished,   // 楽曲終了後
}