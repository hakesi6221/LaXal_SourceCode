using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// 画面奥で再生するミュージックビデオの管理を行うクラス
/// 曲選択後に、その曲のMVを投影し、流している曲の経過時間に同期する形でMVを再生
/// </summary>
// TODO：選択された曲のMVをとってくる処理を追記
public class MusicVideoPlayer : MonoBehaviour
{
    [SerializeField, Label("Video Playerコンポーネント参照")]
    private VideoPlayer _player = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var clip = RhythmGameInfomation.Movie;
        if (clip == null)
        {
            Debug.LogError($"{this.name}:指定された曲のMVデータが存在していません。");
            return;
        }
        _player.clip = clip;
        // 動画の再生時間を外部の値依存に設定
        _player.timeReference = VideoTimeReference.ExternalTime;
        // 動画再生コンポーネントの準備
        _player.Prepare();
        // 準備が完了したら、動画を再生する
        _player.prepareCompleted += PlayMusicVideo;
    }

    void Update()
    {
        if (_player == null)
        {
            Debug.LogError($"{typeof(MusicVideoPlayer).Name}:[_player]がアタッチされていません。");
            return;
        }

        // 曲が始まってからわずかな時間がたち、かつ動画の総時間よりも短い場合は、曲の経過時間と動画の再生時間を同期させる
        if (0.05d < BeatManager.Instance.elapsed && BeatManager.Instance.elapsed <= _player.clip.length)
        {
            _player.externalReferenceTime = BeatManager.Instance.elapsed;
        }
    }

    /// <summary>
    /// MVを再生するVideoPlayerに再生の指示を送る
    /// </summary>
    /// <param name="source"></param>
    private void PlayMusicVideo(VideoPlayer source)
    {
        if (_player == null)
        {
            Debug.LogError($"{typeof(MusicVideoPlayer).Name}:[_player]がアタッチされていません。");
            return;
        }

        _player.Play();
    }
}
