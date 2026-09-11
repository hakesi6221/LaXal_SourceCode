using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AppealAssetsDatas", menuName = "ScriptableObjects/AppealAssetsDatas")]
public class AppealAssetsDatas : ScriptableObject
{
    [SerializeField]
    private AppealAssetsData[] _assetDatas = null;

    public IReadOnlyCollection<AppealAssetsData> AssetDatas => _assetDatas;
}