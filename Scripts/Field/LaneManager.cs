using System;
using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// プレイヤーの現在のレーン番号管理を行うクラス
/// レーン番号変数の更新と、レーン移動時のイベント発火を行う
/// </summary>
[Serializable]
public class LaneManager
{
    [SerializeField, Label("各レーンの座標を示す仮オブジェクト")]
    private Transform[] _laneObjects = null;

    [SerializeField, Label("現在のレーン番号：1番左が0,右に行けば1ずつ上がる")]
    private int _laneIndex = 1;

    // レーン移動してからの経過時間カウント変数
    // レーン移動するたびに0.0からカウントし直す
    private double _timeSecOnMoveLane = 0.0;

    // 移動直前のレーン番号を保持する変数
    // リズムゲーム判定に使用
    //
    // 初期値は存在しないレーン番号にしておく
    private int _prevLaneIndex = -1;

    /// <summary>
    /// レーン移動時に呼ぶイベント変数
    /// 第一引数：移動前のレーン番号
    /// 第二引数：移動後のレーン番号
    /// </summary>
    public event Action<int, int> OnLaneChangeEvent;

    /// <summary>
    /// 現在のレーン番号：1番左が0,右に行けば1ずつ上がる
    /// </summary>
    public int LaneIndex => _laneIndex;

    /// <summary>
    /// 移動直前のレーン番号
    /// </summary>
    public int PrevLaneIndex => _prevLaneIndex;

    /// <summary>
    /// レーン移動してからの経過時間カウント変数
    /// レーン移動するたびに0.0からカウントし直す
    /// </summary>
    public double TimeSecOnMoveLane { get { return _timeSecOnMoveLane; } set { _timeSecOnMoveLane = value; } }

    /// <summary>
    /// 一つ右のレーンにレーン番号を移動する
    /// 移動してしまうと範囲外になる場合、数値を変えずfalseを返す
    /// 移動可能な場合、数値を更新してtrueを返す
    /// </summary>
    /// <returns>移動が可能かどうか</returns>
    public bool MoveToRightLane()
    {
        if (_laneObjects.Length - 1 <= _laneIndex)
            return false;

        _prevLaneIndex = _laneIndex;
        _laneIndex++;
        OnLaneChangeEvent?.Invoke(_prevLaneIndex, _laneIndex);
        return true;
    }

    /// <summary>
    /// 一つ左のレーンにレーン番号を移動する
    /// 移動してしまうと範囲外になる場合、数値を変えずfalseを返す
    /// 移動可能な場合、数値を更新してtrueを返す
    /// </summary>
    /// <returns>移動が可能かどうか</returns>
    public bool MoveToLeftLane()
    {
        if (_laneIndex <= 0)
            return false;

        _prevLaneIndex = _laneIndex;
        _laneIndex--;
        OnLaneChangeEvent?.Invoke(_prevLaneIndex, _laneIndex);
        return true;
    }

    /// <summary>
    /// レーン番号を引数で与えられた番号に移動する
    /// 移動してしまうと範囲外になる場合、数値を変えずfalseを返す
    /// 移動可能な場合、数値を更新してtrueを返す
    /// </summary>
    /// <param name="laneIndex">移動先のレーン番号</param>
    /// <returns>移動が可能かどうか</returns>
    public bool MoveToLane(int laneIndex)
    {
        if (laneIndex < 0 || _laneObjects.Length <= laneIndex)
            return false;
        if (laneIndex == _laneIndex)
            return true;

        _prevLaneIndex = _laneIndex;
        _laneIndex = laneIndex;

        OnLaneChangeEvent?.Invoke(_prevLaneIndex, _laneIndex);
        return true;
    }

    /// <summary>
    /// レーン番号からレーンの座標を取得する関数
    /// レーンオブジェクトが確認できない、または範囲外の番号が指定された場合はエラーを残して現在位置を返す
    /// </summary>
    /// <param name="laneIndex"></param>
    /// <returns></returns>
    public Vector3 GetTargetLanePosition(int laneIndex)
    {
        if (_laneObjects == null)
        {
            Debug.LogError("レーンオブジェクトが何もアタッチされていません。");
            return default;
        }
        if (laneIndex < 0 || _laneObjects.Length <= laneIndex)
        {
            Debug.LogError($"存在しない範囲のレーンにアクセスしようとしています。value={laneIndex}");
            return default;
        }

        return _laneObjects[laneIndex].position;
    }
}
