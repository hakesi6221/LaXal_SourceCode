using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class ResultVoiceData
{
    [Label("曲")]
    public MusicType Music;

    [Label("評価")]
    public ResultGrade Grade;

    [Label("Voice 1")]
    public AudioType Voice1;

    [Label("Voice 2")]
    public AudioType Voice2;
}

/// <summary>
/// リザルト画面の進行管理を担当するクラス
/// 遷移した後に、UIへの情報の引き渡しと、
/// 入力を受けた時に曲選択画面に戻る処理を保持する
/// </summary>
public class ResultSceneManager : MonoBehaviour
{
    [SerializeField, Label("リザルトUI管理")]
    private ResultUIsView _resultUI = null;

    [SerializeField, Label("操作開始までの時間")]
    private float _startControllDelay = 2.0f;

    [SerializeField, Label("シーン終了遷移対応入力")]
    private InputAction _action;

    [SerializeField, Label("演出アニメーション")]
    private MonoBehaviourAnimSequencer _anim = new MonoBehaviourAnimSequencer();

    [SerializeField, Label("リザルトボイス設定")]
    private List<ResultVoiceData> _resultVoices = new List<ResultVoiceData>();

    // リザルト評価を計算するクラス
    private ResultGradeCalculator _gradeCalc = new ResultGradeCalculator();

    // リザルト評価保存用メンバ変数
    private ResultGrade _resultGrade = ResultGrade.None;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OnStartResultScene();
    }

    /// <summary>
    /// リザルト画面が始まった時に呼ぶ処理
    /// 情報をUIに表示し、指定秒待った後、リザルト画面を抜けるための入力対応を開始
    /// </summary>
    private void OnStartResultScene()
    {
        GetResultInfoAndSetToUI();
        StartControll();
    }

    private void GetResultInfoAndSetToUI()
    {
        string musicName = RhythmGameInfomation.Name;
        int maxScore = RhythmGameInfomation.MaxScore;
        MusicDifficulty difficulty = RhythmGameInfomation.Difficulty;
        JacketSpriteData jacketData = RhythmGameInfomation.Jacket;
        int finalScore = RhythmGameInfomation.FinalScore;
        Dictionary<JudgeResult, int> judgeCount = RhythmGameInfomation.JudgeNums;
        _resultGrade = _gradeCalc.CalcResultGrade(maxScore, finalScore);

        if (_resultUI == null)
        {
            Debug.LogError($"{this.name}:[_resultUI]がアタッチされていません。");
            return;
        }

        _anim?.Initialize();
        _resultUI?.DisplayInfomations
        (
            musicName,
            difficulty,
            jacketData,
            finalScore,
            judgeCount,
            _resultGrade
        );

        SoundManager.Instance?.PlayBGMWithFadeIn(RhythmGameInfomation.InstAudioType, 0f, 0.5f, 0.5f);
    }

    /// <summary>
    /// 操作開始処理
    /// 指定秒待って、操作を開始する
    /// </summary>
    /// <returns></returns>
    private async void StartControll()
    {
        var token = this.GetCancellationTokenOnDestroy();

        try
        {
            // 演出開始と共に
            PlayResultVoice();
            await _anim.PlayAnimSequence(this.GetCancellationTokenOnDestroy());

            // リザルト画面終了処理を登録
            _action.performed += OnFinishResult;
            _action.Enable();
            Debug.Log("操作開始");
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("非同期処理のキャンセル");
            return;
        }
    }


    /// <summary>
    /// 現在の曲と評価に対応したボイスをランダムで再生する
    /// 2026/08/28 菅原慶太 が追加
    /// </summary>
    private void PlayResultVoice()
    {
        MusicType musicType = RhythmGameInfomation.Type;

        int maxScore = RhythmGameInfomation.MaxScore;
        int finalScore = RhythmGameInfomation.FinalScore;

        // 曲と評価が一致する設定を探す
        ResultVoiceData voiceData = _resultVoices.Find(
            data =>
                data.Music == musicType &&
                data.Grade == _resultGrade
        );

        if (voiceData == null)
        {
            Debug.LogWarning(
                $"リザルトボイスが設定されていません。 " +
                $"Music: {musicType}, Grade: {_resultGrade}"
            );
            return;
        }

        // 0 or 1をランダム
        AudioType selectedVoice = UnityEngine.Random.Range(0, 2) == 0
            ? voiceData.Voice1
            : voiceData.Voice2;

        SoundManager.Instance?.PlayVoice(selectedVoice);
    }

    /// <summary>
    /// リザルト画面終了時の処理
    /// 入力を終了し、曲選択画面に遷移する
    /// </summary>
    /// <param name="context"></param>
    private void OnFinishResult(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        SoundManager.Instance?.PlaySE(AudioType.SE_tapbuttan);
        SoundManager.Instance?.StopBGMWithFadeOut(RhythmGameInfomation.InstAudioType);
        _action.Disable();
        _action.performed -= OnFinishResult;
        _resultUI.OnFinishResult();
        SceneTransitionManager.Instance.ChangeSceneAsync("MusicSelectScene").Forget();
    }
}
