using System;
using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// 複数の楽曲データを1つの配列として保持しておくクラス
/// ScriptableObjectとしてアセットを作成できる
/// </summary>
[Serializable]
[CreateAssetMenu(fileName = "MusicDatasTable", menuName = "ScriptableObject/CreatMusicDatasTable")]
public class MusicDatasTable : ScriptableObject
{
    [SerializeField, Label("楽曲データ")]
    private MusicData[] _musicDatas = null;

    /// <summary>
    /// 全ての楽曲データ配列
    /// </summary>
    public MusicData[] MusicDatas => _musicDatas;

    /// <summary>
    /// 楽曲データの数
    /// </summary>
    public int GetTableLength => _musicDatas.Length;

    /// <summary>
    /// 指定の楽曲データを取得する
    /// </summary>
    /// <param name="index">配列のインデックス</param>
    /// <returns></returns>
    public MusicData GetMusicData(int index)
    {
        if (_musicDatas.Length <= index)
        {
            Debug.LogError($"{typeof(MusicDatasTable).Name}:[GetMusicData(int index)]楽曲データの個数以上のインデックスを参照しようとしています。");
            return null;
        }
        return _musicDatas[index];
    }

    /// <summary>
    /// 指定の楽曲データを取得する
    /// </summary>
    /// <param name="name">取得したい楽曲名</param>
    /// <returns></returns>
    public MusicData GetMusicData(MusicType name)
    {
        MusicData result = null;
        foreach (MusicData data in _musicDatas)
        {
            if (data.Type == name)
            {
                if (result != null)
                    Debug.LogWarning($"{typeof(MusicDatasTable).Name}:[GetMusicData(MusicName name)]指定された曲名の楽曲データが複数存在しています。");
                result = data;
            }

        }

        if (result == null)
            Debug.LogError($"{typeof(MusicDatasTable).Name}:[GetMusicData(MusicName name)]指定された曲名の楽曲データが存在しません。");
        
        return result;
    }
}
