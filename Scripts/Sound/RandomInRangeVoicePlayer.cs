using UnityEngine;

public class RandomInRangeVoicePlayer
{
    /// <summary>
    /// 指定した2つのタイプの間の中からランダムに抽選したボイスを鳴らす
    /// 最大値は抽選の範囲内
    /// 範囲の中にボイス以外のタイプが含まれている場合、ワーニングを残し終わる
    /// </summary>
    /// <param name="manager">SoundManagerの参照</param>
    /// <param name="minType">最小値となるタイプ</param>
    /// <param name="maxType">最大値となるタイプ</param>
    /// <returns>再生したサウンドのタイプ</returns>
    public AudioType PlayVoiceRandomInRange(SoundManager manager, AudioType minType, AudioType maxType)
    {
        if (manager == null) return AudioType.None;

        AudioType playType = (AudioType)Random.Range((int)minType, (int)(maxType + 1));
        if (playType < AudioType.voice_logocall_all)
        {
            Debug.LogWarning($"{typeof(RandomInRangeVoicePlayer).Name}:ボイスのタイプの範囲外が指定されています");
            return AudioType.None;
        }

        manager?.PlayVoice(playType);
        return playType;
    }

    /// <summary>
    /// 指定したAudioTypeの配列の中からランダムに抽選してボイスを再生する
    /// 範囲の中にボイス以外のタイプが含まれている場合、ワーニングを残し終わる
    /// </summary>
    /// <param name="manager">SoundManagerの参照</param>
    /// <param name="types">再生するボイスの配列</param>
    /// <returns>再生したサウンドのタイプ</returns>
    public AudioType PlayVoiceRandomInRange(SoundManager manager, AudioType[] types)
    {
        if (manager == null) return AudioType.None;
        if (types == null)
        {
            Debug.LogWarning($"{typeof(RandomInRangeVoicePlayer).Name}:再生するボイスの配列が存在しません");
            return AudioType.None;
        }

        int playIndex = Random.Range(0, types.Length);
        AudioType playType = types[playIndex];
        if (playType < AudioType.voice_logocall_all)
        {
            Debug.LogWarning($"{typeof(RandomInRangeVoicePlayer).Name}:ボイスのタイプの範囲外が指定されています");
            return AudioType.None;
        }

        manager?.PlayVoice(playType);
        return playType;
    }
}
