using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// サウンドの情報を配列として保持するデータクラス
/// </summary>
[CreateAssetMenu(fileName = "AudioInfomations", menuName = "ScriptableObjects/AudioInfomations")]
public class AudioInfomations : ScriptableObject
{
    [SerializeField]
    private AudioInfomation[] _audios = null;

    /// <summary>
    /// サウンドの情報配列
    /// </summary>
    public IReadOnlyList<AudioInfomation> Audios => _audios;

    /// <summary>
    /// 指定したタイプの音声データを取得する
    /// </summary>
    /// <param name="type">取得するサウンドのタイプ</param>
    /// <returns></returns>
    public AudioInfomation GetAudioInfomation(AudioType type)
    {
        if (type == AudioType.None)
        {
            Debug.LogError($"サウンドデータの取得に失敗しました。type=>{type}");
            return null;
        }

        var datas = _audios.Where(data => data.Type == type).ToArray();
        if (datas.Length >= 2)
        {
            Debug.LogError($"同じサウンドデータが複数存在しています。type=>{type}");
            return null;
        }

        return datas.FirstOrDefault();
    }
}