using UnityEngine;

/// <summary>
/// 先輩の動作と難易度を紐づけたデータを配列として保持するアセットクラス
/// </summary>
[CreateAssetMenu(fileName = "SeniorMovementDatas", menuName = "ScriptableObjects/SeniorMovementDatas")]
public class SeniorMovementDatas : ScriptableObject
{
    [SerializeField]
    private SeniorMovementData[] _datas = null;

    /// <summary>
    /// データ
    /// </summary>
    public SeniorMovementData[] Datas => _datas;
}
