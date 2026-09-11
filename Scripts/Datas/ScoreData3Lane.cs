using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Score3_Music", menuName = "ScriptableObjects/ScoreData/3Lane")]

public class ScoreData3Lane : ScriptableObject
{
    [SerializeField, Header("楽曲名")]
    private MusicType _name = MusicType.None;

    [Header("譜面データ")]
    // この楽曲のすべての譜面データ参照
    [SerializeField]
    private TextAsset _scores1Lane = null;
    [SerializeField]
    private TextAsset _scores2Lane = null;
    [SerializeField]
    private TextAsset _scores3Lane = null;

    /// <summary>
    /// 楽曲名
    /// </summary>
    public MusicType Name => _name;

    /// <summary>
    /// 指定したレーン番号の譜面データを取得する
    /// 1～3
    /// </summary>
    /// <param name="laneIndex"></param>
    /// <returns></returns>
    public TextAsset GetScoreData(int laneIndex)
    {
        TextAsset result = null;

        switch (laneIndex)
        {
            case 1:
                result = _scores1Lane;
                break;
            case 2:
                result = _scores2Lane;
                break;
            case 3:
                result = _scores3Lane;
                break;
            default:
                Debug.LogError($"{typeof(ScoreData3Lane)}:[GetScoreData(int laneIndex)]範囲外の譜面を参照しようとしています。name={_name}");
                return result;
        }

        if (result == null)
            Debug.LogError($"{typeof(ScoreData3Lane)}:[GetScoreData(int laneIndex)]指定されたレーンの譜面が存在しませんでした。name={_name}");

        return result;
    }
}
