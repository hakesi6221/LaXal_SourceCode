using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 楽曲選択ディスプレイたちの状態を管理するマネージャークラス
/// 各ディスプレイへの描画の更新や、サウンドの更新を行う
/// </summary>
[RequireComponent(typeof(MusicDisplayHeaderManager))]
public class MusicDisplaysManager : MonoBehaviour
{
    [SerializeField, Header("右操作入力")]
    private InputActionProperty _rightInput;

    [SerializeField, Header("左操作入力")]
    private InputActionProperty _leftInput;

    [SerializeField, Label("楽曲データテーブル")]
    private MusicDatasTable _musicDatasTable = null;

    [SerializeField, Label("楽曲エディット画面")]
    private MusicEditorsObj _editUI = null;

    [SerializeField, Label("楽曲ディスプレイ親")]
    private GameObject _displaysParent = null;

    [SerializeField, Label("楽曲タイトル表示管理")]
    private SelectedMusicTitleView _titleView = null;

    [SerializeField, Label("楽曲確定時のフェード時間")]
    private float _ondicisionFadeDuration = 1.0f;

    [SerializeField, Label("スクロール管理")]
    private MusicDisplaysScroller _scroller = null;

    // 楽曲ディスプレイの先頭が何かを管理するコンポーネント参照
    private MusicDisplayHeaderManager _headerManager = null;

    [SerializeField]
    // 楽曲表示のディスプレイたち
    private MusicDisplayObj[] _displays = null;

    // ランダムなボイス再生クラス
    private RandomInRangeVoicePlayer _voicePlayer = new RandomInRangeVoicePlayer();

    /// <summary>
    /// それぞれの楽曲ディスプレイに楽曲データをアタッチする
    /// </summary>
    /// <param name="displays">楽曲ディスプレイの配列</param>
    private void SetMusicDataToDisplay(MusicDisplayObj[] displays)
    {
        int musicIndex = 0;
        for (int i = 0; i < displays.Length; i++)
        {
            MusicDisplayObj presenter = displays[i];
            if (_musicDatasTable.GetTableLength <= musicIndex)
            {
                Debug.LogError($"{this.name}:存在しない楽曲にアクセスしようとしています。");
                continue;
            }
            presenter.InitializeDisplay(_musicDatasTable.GetMusicData(musicIndex), i == 0);
            musicIndex++;
            musicIndex = (int)Mathf.Repeat(musicIndex, _musicDatasTable.GetTableLength);
        }
    }

    /// <summary>
    /// 初期化処理
    /// ・スクロール担当の初期化
    /// ・すべての楽曲ディスプレイの取得&保持
    /// 等を行う
    /// </summary>
    public void Initialize()
    {
        _headerManager = GetComponent<MusicDisplayHeaderManager>();
        if (_scroller == null)
        {
            Debug.LogError($"{this.name}:[{typeof(MusicDisplaysScroller).Name}]がアタッチされていません。");
            return;
        }
        _scroller.Initialize(transform.forward);
        _scroller.OnDisplaysUpdateCallBack += OnUpdateDisplays;
        _scroller.OnDecisionedMusicCallBack += OnDicisionMusic;
        _displays = _scroller.Children.ToArray();
        _headerManager.Initialize(_displays);
        _headerManager.OnUpdateEvent += OnUpdateHeaderDisplay;
        SetMusicDataToDisplay(_displays);
    }

    /// <summary>
    /// 楽曲選択操作の開始
    /// </summary>
    public void StartSelectMusic(bool resetSelectedUI)
    {
        _scroller?.Activate(resetSelectedUI);
        _rightInput.action.performed += _ => _scroller.RotateToNext(true, this.GetCancellationTokenOnDestroy()).Forget();
        _leftInput.action.performed += _ => _scroller.RotateToNext(false, this.GetCancellationTokenOnDestroy()).Forget();
    }

    /// <summary>
    /// 楽曲選択操作の停止
    /// </summary>
    public void FinishSelectMusic()
    {
        _rightInput.action.performed -= _ => _scroller.RotateToNext(true, this.GetCancellationTokenOnDestroy()).Forget();
        _leftInput.action.performed -= _ => _scroller.RotateToNext(false, this.GetCancellationTokenOnDestroy()).Forget();
        _scroller?.Deactivate();
    }

    private void OnUpdateHeaderDisplay(MusicDisplayObj musicDisplay)
    {
        if (musicDisplay == null) return;

        // TODO:音声素材のインポートなどが終わり次第しっかり実装
        SoundManager.Instance?.PlaySE(AudioType.SE_musicslide);
    }

    private UniTask[] CreateOnDicisionMusicActHandles(MusicData selectedMusic)
    {
        UniTask[] handles = new UniTask[_displays.Length];
        for (int i = 0; i < _displays.Length; i++)
        {
            var presenter = _displays[i];
            var handle = presenter.OnDicisionMusic(_ondicisionFadeDuration, selectedMusic);
            handles[i] = handle;
        }

        return handles;
    }

    private UniTask[] CreateReStartSelectActHandles()
    {
        UniTask[] handles = new UniTask[_displays.Length];
        for (int i = 0; i < _displays.Length; i++)
        {
            var presenter = _displays[i];
            var handle = presenter.OnReStartSelect(_ondicisionFadeDuration);
            handles[i] = handle;
        }

        return handles;
    }

    private async void OnRestartSelect()
    {
        var handles = CreateReStartSelectActHandles();
        try
        {
            await UniTask.WhenAll(handles);
            // _displaysParent?.SetActive(true);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("非同期処理をキャンセル");
            return;
        }
        StartSelectMusic(false);
        MusicSelectSceneManager.Instance.ResetReturnButton();
    }

    /// <summary>
    /// 楽曲選択時に呼ばれる処理
    /// 楽曲エディット画面の操作を開始する
    /// </summary>
    /// <param name="musicDisplay">決定された楽曲データ</param>
    private async void OnDicisionMusic(MusicDisplayObj musicDisplay)
    {
        if (_editUI == null)
        {
            Debug.LogError($"{this.name}:[_editUI]がアタッチされていません。");
            return;
        }
        MusicData musicData = musicDisplay.MusicData;
        if (musicData == null) return;
        FinishSelectMusic();
        SoundManager.Instance?.PlaySE(AudioType.SE_musicselection);
        _voicePlayer?.PlayVoiceRandomInRange(SoundManager.Instance, AudioType.voice_musicselection_konomi, AudioType.voice_musicselection_nagisa);
        MusicSelectSceneManager.Instance.SetReturnButtonProcess(null);

        var handles = CreateOnDicisionMusicActHandles(musicData);
        try
        {
            // _displaysParent?.SetActive(false);
            await UniTask.WhenAll(handles);
            _editUI?.StartEdit(musicDisplay, OnRestartSelect);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("非同期処理をキャンセル");
            return;
        }
    }

    /// <summary>
    /// 楽曲ディスプレイが更新されたときの処理
    /// </summary>
    /// <param name="musicDisplay">選択された楽曲データ</param>
    private void OnUpdateDisplays(MusicDisplayObj musicDisplay)
    {
        if (_titleView != null)
            _titleView.UpdateMusicTitleUI(musicDisplay);
    }

    void OnDestroy()
    {
        _rightInput.action.performed -= _ => _scroller.RotateToNext(true, this.GetCancellationTokenOnDestroy()).Forget();
        _leftInput.action.performed -= _ => _scroller.RotateToNext(false, this.GetCancellationTokenOnDestroy()).Forget();
    }
}
