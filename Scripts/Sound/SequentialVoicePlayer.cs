using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SequentialVoicePlayer
{
    /// <summary>
    /// 指定したAudioTypeの配列の中からランダムに抽選してボイスを再生する
    /// 範囲の中にボイス以外のタイプが含まれている場合、ワーニングを残し終わる
    /// </summary>
    /// <param name="manager">SoundManagerの参照</param>
    /// <param name="types">再生するボイスの配列</param>
    /// <returns>再生したサウンドのタイプ</returns>
    public async void PlayVoiceSequential(SoundManager manager, AudioType[] types, CancellationToken cancellationToken)
    {
        if (manager == null) return;
        if (types == null)
        {
            Debug.LogWarning($"{typeof(RandomInRangeVoicePlayer).Name}:再生するボイスの配列が存在しません");
            return;
        }

        foreach (var type in types)
        {
            if (type < AudioType.voice_logocall_all)
            {
                Debug.LogWarning($"{typeof(RandomInRangeVoicePlayer).Name}:ボイスのタイプの範囲外が指定されています");
                return;
            }

            var source = manager?.PlayVoice(type);
            try
            {
                await UniTask.WaitUntil(() => !source.isPlaying, cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("非同期処理のキャンセル");
                return;
            }
        }
    }
}
