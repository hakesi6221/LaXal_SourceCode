using System;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 曲選択画面の楽曲ディスプレイへの操作を行い、他クラスとつながるためのクラス
/// 管理クラスから指示を受け、ここを仲介し見た目担当のクラスにも指示を送る
/// </summary>
[RequireComponent(typeof(MusicDisplayView))]
[RequireComponent(typeof(AudioSource))]
public class MusicDisplayObj : ArrangedUIElementBase
{
    [SerializeField, Label("Image参照")]
    private Image _image = null;

    [SerializeField, Label("CanvasGroup")]
    private CanvasGroup _canvasGroup = null;

    // 楽曲ディスプレイの見た目管理コンポーネント参照
    private MusicDisplayView _view = null;

    // 担当楽曲のBGM用AudioSource
    private AudioSource _audioSource = null;

    // このディスプレイに割り振られた楽曲データ
    private MusicData _musicData = null;

    private GrabEffect _grabEffect = null;

    /// <summary>
    /// このディスプレイに割り振られた楽曲データ
    /// </summary>
    public MusicData MusicData => _musicData;

    /// <summary>
    /// MMusicDisplaysManager経由での初期化処理
    /// 楽曲データの受け取りとアタッチ、選択状態の初期化を行う
    /// また、基本的な初期化処理も行う
    /// </summary>
    /// <param name="musicData">楽曲データ</param>
    /// <param name="isSelected">選択かどうか</param>
    public void InitializeDisplay(MusicData musicData, bool isSelected)
    {
        if (musicData == null)
        {
            Debug.LogError($"{this.name}：楽曲データが正しくない形で渡されています。");
            return;
        }

        _view = GetComponent<MusicDisplayView>();
        _audioSource = GetComponent<AudioSource>();
        _grabEffect = GetComponent<GrabEffect>();
        _musicData = musicData;
        _view.Initialize(musicData.Jacket, isSelected);
        UpdateState(isSelected);
    }

    /// <summary>
    /// このディスプレイに対応した楽曲データのBGMの再生状態を選択状態に応じて更新
    /// </summary>
    /// <param name="isSelected">選択状態かどうか</param>
    private void UpdateBGMState(bool isSelected)
    {
        if (_musicData == null)
        {
            Debug.LogError($"{this.name}:対応する楽曲データが設定されていません");
            return;
        }
        if (isSelected)
            SoundManager.Instance?.PlaySoundWithFadeIn(_audioSource, _musicData.Audio, 0f, 0.5f, 0.5f);
        else
            SoundManager.Instance?.StopSoundWithFadeOut(_audioSource, 0f, 0.5f);
    }

    protected override void OnUpdateState(bool isSelected)
    {
        _view?.OnUpdateSelectState(isSelected);
        if (_image == null)
        {
            Debug.LogError($"{this.name}:[_image]がアタッチされていません。");
            return;
        }
        _image.raycastTarget = isSelected;
        UpdateBGMState(isSelected);
    }

    /// <summary>
    /// 選択する楽曲をクリックで確定したときの処理
    /// 操作を無効にし、直前の操作可否を保存する(復帰後のために)
    /// フェードして消える
    /// 選択中のものだった場合、少し待機してから見た目の処理へ移行
    /// </summary>
    /// <param name="fadeDuration">フェードアウトの所要秒</param>
    /// <param name="musicData">選択された楽曲データ</param>
    /// <returns></returns>
    public async UniTask OnDicisionMusic(float fadeDuration, MusicData musicData)
    {
        var token = this.GetCancellationTokenOnDestroy();
        if (_image == null)
        {
            Debug.LogError($"{this.name}:[_image]がアタッチされていません。");
            return;
        }
        if (_canvasGroup == null)
        {
            Debug.LogError($"{this.name}:[_canvasGroup]がアタッチされていません。");
            return;
        }
        _image.raycastTarget = false;

        // 選択された楽曲のディスプレイだったら、二倍待つ
        try
        {
            await _view.OnDicisionMusic(_canvasGroup, fadeDuration);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("非同期処理をキャンセル");
            return;
        }
    }

    public async UniTask OnReStartSelect(float fadeDuration)
    {
        var token = this.GetCancellationTokenOnDestroy();
        if (_image == null)
        {
            Debug.LogError($"{this.name}:[_image]がアタッチされていません。");
            return;
        }
        if (_canvasGroup == null)
        {
            Debug.LogError($"{this.name}:[_canvasGroup]がアタッチされていません。");
            return;
        }
        _image.raycastTarget = false;
        await _view.OnReStartSelect(_canvasGroup, fadeDuration);
        _image.raycastTarget = IsSelected;
    }

    public override void OnUpdateGrabState(bool isGrabed)
    {
        if (isGrabed)
        {
            Debug.Log($"掴んでいる{this.name}");
            _grabEffect.Play();
            Debug.Log($"{this.name}");
        }
        
        if (!isGrabed)
        {
            Debug.Log("離した");
            _grabEffect.Stop();
        }
    }
}
