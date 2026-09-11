using Cysharp.Threading.Tasks;
using UnityEngine;
using Common.SingleTon;
using NaughtyAttributes;
using UnityEngine.UI;
using System;

/// <summary>
/// 曲選択画面のシーン遷移など、シーン自体の統括を行うクラス
/// シングルトンクラス
/// </summary>
public class MusicSelectSceneManager : SingletonMonoBehaviour<MusicSelectSceneManager>
{
    protected override bool dontDestroyOnLoad => false;

    [SerializeField, Label("楽曲ディスプレイ管理者")]
    private MusicDisplaysManager _displayManager = null;

    [SerializeField, Label("メインメニューシーン名")]
    private string _mainMenuScene = "MainMenuScene";

    [SerializeField, Label("戻るボタン")]
    private OnButtonClickAdministrator _returnButton = null;

    public event Action OnLeaveSceneEvent;

    void Start()
    {
        // リズムゲームの情報のリセット
        RhythmGameInfomation.ResetInfomations();
        ConstructScene();
    }

    private void ConstructScene()
    {
        if (_displayManager == null)
        {
            Debug.LogError($"{this.name}:[_displayManager]がアタッチされていません。");
            return;
        }
        _displayManager.Initialize();
        _displayManager.StartSelectMusic(true);
        ResetReturnButton();
    }

    public void SetReturnButtonProcess(Action process)
    {
        if (_returnButton == null)
        {
            Debug.LogError($"{this.name}:[_returnButton]がアタッチされていません。");
            return;
        }
        _returnButton.SetReturnProcess
        (
            () =>
            {
                process?.Invoke();
                SoundManager.Instance?.PlaySE(AudioType.SE_backbuttan);
            }
        );
    }

    public void ResetReturnButton()
    {
        SetReturnButtonProcess(ReturnToMainMenuScene);
    }

    private void ReturnToMainMenuScene()
    {
        LeaveScene(_mainMenuScene);
    }

    public void StartMainGame(MusicData musicData, MusicDifficulty difficulty)
    {
        if (musicData == null)
        {
            Debug.LogError($"{this.name}：楽曲データが正しくない形で渡されています。");
            return;
        }

        // リズムゲームの情報のリセット
        RhythmGameInfomation.ResetInfomations();
        // 難易度に応じたデータの選定
        ScoreData3Lane score3Lane = musicData.GetScore3Lane;
        ScoreData4Lane score4Lane = musicData.GetScore4Lane(difficulty);
        TextAsset seniorMove = musicData.GetSeniorMoveDatas(difficulty).MovementData;
        RhythmGameInfomation.SetInGameInfomations
        (
            musicData.Name,
            musicData.Type,
            difficulty,
            musicData.BPM,
            musicData.MaxScore(RhythmGameInfomation.GameMode, difficulty),
            musicData.SpecialNotePrefab,
            score3Lane,
            score4Lane,
            musicData.Jacket,
            musicData.Audio,
            musicData.AudioType,
            musicData.InstAudioType,
            musicData.Movie,
            seniorMove,
            musicData.SuperChatCSV,
            musicData.CharaAsset
        );

        string sceneName = string.Empty;
        if (RhythmGameInfomation.GameMode == RhythmGameMode.FourLane)
            sceneName = "MainGameScene_4Lane";
        else
            sceneName = "MainGameScene_3Lane";

        // メインシーンへ移動
        // タイトルで決定したモードに設定
        LeaveScene(sceneName);
    }

    private void LeaveScene(string sceneName)
    {
        SetReturnButtonProcess(null);
        OnLeaveSceneEvent?.Invoke();
        SceneTransitionManager.Instance.ChangeSceneAsync(sceneName).Forget();
    }
}
