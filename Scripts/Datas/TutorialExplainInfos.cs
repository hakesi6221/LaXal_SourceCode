using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "TutorialExplainInfos", menuName = "ScriptableObjects/TutorialExplainInfos")]
public class TutorialExplainInfos : ScriptableObject
{
    [SerializeField, Label("チュートリアル説明情報")]
    private TutorialExplainInfo[] _explainInfo = null;

    public IReadOnlyCollection<TutorialExplainInfo> ExplainInfo => _explainInfo;
}
