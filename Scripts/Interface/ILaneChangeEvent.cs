/// <summary>
/// レーン変更時のイベントを各クラスで実装するためのインターフェース
///
/// 継承したクラス内で、GameManagerのAddLaneChangeEventに登録する必要あり
/// </summary>
public interface ILaneChangeEvent
{
    /// <summary>
    /// レーン変更時のイベント
    /// ここで、レーン変更時に行いたい処理を実装
    /// </summary>
    /// <param name="prevLaneIndex">移動前のレーン番号</param>
    /// <param name="newLaneIndex">移動後のレーン番号</param>
    public void OnLaneChange(int prevLaneIndex, int newLaneIndex);
}
