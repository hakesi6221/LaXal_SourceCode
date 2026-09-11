using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// レーンの位置の差分によって、SpriteRendererのsortingorderを変えるクラス
/// 現在いるレーンから遠い位置にいればいるほど、奥に見える
///
/// フィールドに配置されるオブジェクトごとに番号を振るため、ノーツで使用している従来のレーン番号には、2倍し1を足すという処理を行う必要あり
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class FixOrderInlayerByLaneGap : MonoBehaviour
{
    [SerializeField, Label("左から何個目にあるものか")]
    private int _laneObjIndex = 0;
    private SpriteRenderer _renderer = null;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enabled = RhythmGameInfomation.GameMode == RhythmGameMode.ThreeLane;
        _renderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        int order = Mathf.Abs((GameManager.Instance.CurrentLaneIndex * 2 + 1) - _laneObjIndex);
        _renderer.sortingOrder = order * -1;
    }

    /// <summary>
    /// このオブジェクトのレーンオブジェクト番号をセットする
    /// レーンオブジェクト番号は、左から何個目にあるもの
    ///
    /// 従来のレーン番号を渡し、それを変換してレーンオブジェクト番号としてセットする関数
    /// </summary>
    /// <param name="laneIndex">従来のレーン番号</param>
    public void SetLaneIndexToObjIndex(int laneIndex)
    {
        _laneObjIndex = laneIndex * 2 + 1;
    }

    /// <summary>
    /// このオブジェクトのレーンオブジェクト番号をセットする
    /// レーンオブジェクト番号は、左から何個目にあるもの
    ///
    /// 直接レーンオブジェクト番号を渡してセットする関数
    /// </summary>
    /// <param name="laneObjIndex">レーンオブジェクト番号</param>
    public void SetObjIndex(int laneObjIndex)
    {
        _laneObjIndex = laneObjIndex;
    }
}
