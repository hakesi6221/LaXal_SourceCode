using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 曲選択時のエディットウィンドウへの操作を行い、他クラスとつながるためのクラス
/// 曲選択側から指示を受け、ここを仲介し見た目担当のクラスにも指示を送る
/// </summary>
[RequireComponent(typeof(MusicEditorsView))]
public class MusicEditorsObj : MonoBehaviour
{
    [SerializeField, Header("右操作入力")]
    private InputActionProperty _rightInput;

    [SerializeField, Header("左操作入力")]
    private InputActionProperty _leftInput;

    [SerializeField, Label("スクロール担当")]
    private MusicEditorsScroller _scroller;

    [SerializeField, Label("UIのルートオブジェクト")]
    private GameObject _uiRoot = null;

    [SerializeField, Label("難易度ボタンの親")]
    private GameObject _diffButtonsParent = null;

    [SerializeField, Label("難易度ボタン")]
    private DifficultyEditButton[] _diffucultyButtons = null;

    [SerializeField, Label("決定ボタン")]
    private Button _dicisionButton = null;

    [SerializeField, Label("初期設定難易度")]
    private MusicDifficulty _defaultDifficulty = MusicDifficulty.None;

    // 見た目担当のクラス参照
    private MusicEditorsView _view = null;

    // 現在選択されていた楽曲ディスプレイ
    private MusicDisplayObj _musicDisplay = null;

    private MusicDifficulty _currentDifficulty = MusicDifficulty.None;

    private event Action _onCloseEvent;

    private MusicEditerWindow[] _children = null;

    // ランダムなボイス再生クラス
    private RandomInRangeVoicePlayer _voicePlayer = new RandomInRangeVoicePlayer();

    private void OnEnable()
    {
        _scroller.Initialize(Vector3.forward);
        _children = _scroller.Children.ToArray();
    }

    /// <summary>
    /// 楽曲エディット画面の操作を開始する処理
    /// 楽曲データを受け取り、見た目への反映や保持を行う
    /// </summary>
    /// <param name="musicData">楽曲データ</param>
    public async void StartEdit(MusicDisplayObj musicData, Action onCloseEvent)
    {
        if (musicData == null)
        {
            Debug.LogError($"{this.name}：楽曲データが正しくない形で渡されています。");
            return;
        }

        if (_uiRoot == null)
        {
            Debug.LogError($"{this.name}：[_uiRoot]がアタッチされていません。Inspectorを確認してください");
            return;
        }
        if (_dicisionButton == null)
        {
            Debug.LogError($"{this.name}：[_dicisionButton]がアタッチされていません。Inspectorを確認してください");
            return;
        }

        _view = GetComponent<MusicEditorsView>();
        _musicDisplay = musicData;
        ChangeDifficulty(_defaultDifficulty);
        InitDifficultyButtons();

        // 各ボタンや登録が必要な処理への登録処理
        _dicisionButton.onClick.AddListener(OnDicision);
        _onCloseEvent += onCloseEvent;
        _scroller.ResetScroll();
        _uiRoot.SetActive(true);
        _rightInput.action.performed += _ => _scroller.RotateToNext(true, this.GetCancellationTokenOnDestroy()).Forget();
        _leftInput.action.performed += _ => _scroller.RotateToNext(false, this.GetCancellationTokenOnDestroy()).Forget();
        try
        {
            await _view.OnStartEdit(_children, _musicDisplay.MusicData);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("非同期処理をキャンセル");
            return;
        }
        MusicSelectSceneManager.Instance.SetReturnButtonProcess(OnReturn);
        _scroller.Activate();
    }

    /// <summary>
    /// 全ての難易度選択ボタンに対し、プレイモードに応じた初期化処理を施す
    /// 初期化時に呼ぶ想定
    /// </summary>
    private void InitDifficultyButtons()
    {
        RhythmGameMode gameMode = RhythmGameInfomation.GameMode;
        if (gameMode == RhythmGameMode.FourLane)
        {
            _diffButtonsParent?.SetActive(true);
            foreach (DifficultyEditButton button in _diffucultyButtons)
                SetActToDifficultyButton(button);
        }
        else
        {
            _diffButtonsParent?.SetActive(false);
        }
    }

    /// <summary>
    /// 難易度変更ボタンに対し、難易度変更の処理を登録する
    /// </summary>
    /// <param name="button">登録対象のボタン</param>
    private void SetActToDifficultyButton(DifficultyEditButton button)
    {
        EventTrigger trigger = button.Trigger;
        trigger.triggers.Clear();
        if (trigger == null)
        {
            Debug.LogError($"{this.name}:難易度変更ボタンにEventTriggerがアタッチされていません。 name={button.gameObject.name}");
            return;
        }
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        // 難易度変更処理を登録
        entry.callback.AddListener(_ => ChangeDifficulty(button.Difficulty));

        trigger.triggers.Add(entry);
    }

    /// <summary>
    /// 選択されている難易度を変更する関数
    /// 各難易度ボタンにこの処理を登録する必要あり
    /// </summary>
    /// <param name="difficulty">選択された難易度</param>
    public void ChangeDifficulty(MusicDifficulty difficulty)
    {
        if (difficulty == MusicDifficulty.None)
        {
            Debug.LogError($"{this.name}:難易度が不正な形で指定されています。");
            return;
        }
        if (_view == null)
        {
            Debug.LogError($"{this.name}:[{typeof(MusicEditorsView).Name}がアタッチされていません。]");
            return;
        }

        _currentDifficulty = difficulty;
        _view.OnSelectedDifficulty(_diffucultyButtons, difficulty);
        SoundManager.Instance?.PlaySE(AudioType.SE_leveltapbuttan);
    }

    private void OnDicision()
    {
        if (_uiRoot == null)
        {
            Debug.LogError($"{this.name}：[_uiRoot]がアタッチされていません。Inspectorを確認してください");
            return;
        }

        _rightInput.action.performed -= _ => _scroller.RotateToNext(true, this.GetCancellationTokenOnDestroy()).Forget();
        _leftInput.action.performed -= _ => _scroller.RotateToNext(false, this.GetCancellationTokenOnDestroy()).Forget();
        _musicDisplay?.UpdateState(false);
        MusicSelectSceneManager.Instance.SetReturnButtonProcess(null);
        // 曲のデータを保存し、メイン画面へ
        MusicSelectSceneManager.Instance.StartMainGame(_musicDisplay.MusicData, _currentDifficulty);
        SoundManager.Instance?.PlaySE(AudioType.SE_tapbuttan);
        _voicePlayer?.PlayVoiceRandomInRange(SoundManager.Instance, AudioType.voice_maintransition_konomi, AudioType.voice_maintransition_all);
    }

    private async void OnReturn()
    {
        if (_uiRoot == null)
        {
            Debug.LogError($"{this.name}：[_uiRoot]がアタッチされていません。Inspectorを確認してください");
            return;
        }
        if (_view == null)
        {
            Debug.LogError($"{this.name}:[{typeof(MusicEditorsView).Name}がアタッチされていません。]");
            return;
        }

        _scroller.Deactivate();
        MusicSelectSceneManager.Instance.SetReturnButtonProcess(null);
        try
        {
            await _view.OnClose(_children);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("非同期処理をキャンセル");
            return;
        }
        
        _rightInput.action.performed -= _ => _scroller.RotateToNext(true, this.GetCancellationTokenOnDestroy()).Forget();
        _leftInput.action.performed -= _ => _scroller.RotateToNext(false, this.GetCancellationTokenOnDestroy()).Forget();
        _onCloseEvent?.Invoke();
        _uiRoot.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        _rightInput.action.performed -= _ => _scroller.RotateToNext(true, this.GetCancellationTokenOnDestroy()).Forget();
        _leftInput.action.performed -= _ => _scroller.RotateToNext(false, this.GetCancellationTokenOnDestroy()).Forget();
    }
}
