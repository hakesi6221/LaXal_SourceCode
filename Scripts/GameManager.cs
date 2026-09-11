using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;
using Common.SingleTon;

/// <summary>
/// メインリズムゲームの進行管理や画面遷移/情報管理を行う
/// スコア/レーン/コンボ/スタミナのゲーム進行にかかわる情報を管理する
///
/// シングルトンクラス
/// 2026/06/19 石垣翔哉 が更新
/// </summary>
public class GameManager : SingletonMonoBehaviour<GameManager>, ILaneChangeEvent
{
    protected override bool dontDestroyOnLoad => false;

    [SerializeField, Label("スコアマネージャー設定"), BoxGroup("Managers")]
    private ScoreManager _scoreManager = new ScoreManager();

    [SerializeField, Label("コンボマネージャー設定"), BoxGroup("Managers")]
    private ComboManager _comboManager = new ComboManager();

    [SerializeField, Label("レーン管理マネージャー設定"), BoxGroup("Managers")]
    private LaneManager _laneManager = new LaneManager();

    [SerializeField, Label("ノーツ管理マネージャー設定"), BoxGroup("Managers")]
    private NotesManager _notesManager = new NotesManager();

    [SerializeField, Label("判定管理マネージャー設定"), BoxGroup("Managers")]
    private RhythmJudgeManager _judgeManager = new RhythmJudgeManager();
    // コールバックによってボイスを再生
    private MainGameVoice _mainGameVoice = new MainGameVoice();
    // カウント関数用のCTS
    private CancellationTokenSource _cts = new CancellationTokenSource();

    // 現在のメインゲーム進行度
    private ProgressStatus _state = ProgressStatus.None;

    /// <summary>
    /// 現在のメインゲーム進行度
    /// </summary>
    public ProgressStatus State => _state;

    # region ゲーム進行コールバック
    // ゲームが開始するときに呼ぶイベント
    private event Action OnGameStartEvent;

    /// <summary>
    /// ゲーム開始時イベントに処理を登録
    /// </summary>
    /// <param name="action">登録する処理</param>
    public void AddGameStartEvent(Action action)
    {
        if (action == null)
        {
            ErrorLog("[AddGameStartEvent]処理が正しく渡されていません");
            return;
        }

        OnGameStartEvent += action;
    }

    /// <summary>
    /// ゲーム開始時イベントから処理を削除
    /// </summary>
    /// <param name="action">削除する処理</param>
    public void RemoveGameStartEvent(Action action)
    {
        if (action == null)
        {
            ErrorLog("[RemoveGameStartEvent]処理が正しく渡されていません");
            return;
        }

        OnGameStartEvent -= action;
    }
    // ゲームを終了するときに呼ぶイベント
    private event Action OnGameFinishEvent;

    /// <summary>
    /// ゲーム終了時イベントに処理を登録
    /// </summary>
    /// <param name="action">登録する処理</param>
    public void AddGameFinishEvent(Action action)
    {
        if (action == null)
        {
            ErrorLog("[AddGameFinishEvent]処理が正しく渡されていません");
            return;
        }

        OnGameFinishEvent += action;
    }

    /// <summary>
    /// ゲーム終了時イベントから処理を削除
    /// </summary>
    /// <param name="action">削除する処理</param>
    public void RemoveGameFinishEvent(Action action)
    {
        if (action == null)
        {
            ErrorLog("[RemoveGameFinishEvent]処理が正しく渡されていません");
            return;
        }

        OnGameFinishEvent -= action;
    }
    # endregion

    # region ゲーム進行
    /// <summary>
    /// メインゲームの初期化処理
    /// 各数値を持つマネージャーの初期化やコールバックの設定を行う
    /// </summary>
    public void GameConstractor()
    {
        RhythmGameMode gameMode = RhythmGameInfomation.GameMode;
        _scoreManager.Initialize();
        _notesManager.Initialize();
        _comboManager.Initialize();
        _judgeManager.Initialize((int)gameMode);
        _mainGameVoice.Initialize();
        // 4レーンのときのみ、アピールチャンス突入時に判定タイミングの設定と加点処理の登録を行う
        if (gameMode == RhythmGameMode.FourLane)
        {
            AppealChanceManager.Instance.AddEnterAppealChanceEvent(SetAppealJudgeSec);
            AppealChanceManager.Instance.AddExitAppealChanceEvent(_scoreManager.UpdateScoreByAppeal);
        }
        AddOnJudgeEvent(UpdateScoreByJudge);
        AddOnJudgeEvent(UpdateComboByJudge);
        AddLaneChangeEvent(OnLaneChange);
        _state = ProgressStatus.PreStart;
    }

    /// <summary>
    /// リズムゲームが開始されたときに呼ばれる処理
    /// </summary>
    public void OnStartRhythmGame(float startDelay)
    {
        BeatManager.Instance.PlayScheduledMusic(startDelay);
        _state = ProgressStatus.Main;
        OnGameStartEvent?.Invoke();
    }

    /// <summary>
    /// リズムゲームが終了したときに呼ばれる予定の処理
    ///
    /// BeatManager側で楽曲の終了を判定し、その条件を満たしたらこの関数を呼ぶ想定
    /// </summary>
    /// <returns></returns>
    public async void OnFinishRhythmGame()
    {
        if (_state != ProgressStatus.Main)
            return;

        _state = ProgressStatus.Finished;
        var token = this.GetCancellationTokenOnDestroy();

        RhythmGameInfomation.SetResultInfomations(_scoreManager.CurrentScore, _judgeManager.JudgeCount);
        OnGameFinishEvent?.Invoke();
    }
    #endregion

    #region ScoreManager
    /// <summary>
    /// プレイ中の曲の現在のスコア
    /// </summary>
    public int CurrentScore
    {
        get
        {
            if (_scoreManager == null)
            {
                ErrorLog("ScoreManagerが存在しませんでした");
                return 0;
            }

            return _scoreManager.CurrentScore;
        }
    }

    /// <summary>
    /// プレイ中の曲の現状の最高スコア
    /// </summary>
    public int HighScore
    {
        get
        {
            if (_scoreManager == null)
            {
                ErrorLog("ScoreManagerが存在しませんでした");
                return 0;
            }

            return _scoreManager.HighScore;
        }
    }

    /// <summary>
    /// プレイ中の曲の現在のスコアに引数で与えた値を加算する
    /// 負の値を渡された場合、減算されず処理を行わずreturnする
    /// </summary>
    /// <param name="amount">スコア加算量</param>
    public void AddScore(int amount)
    {
        if (_scoreManager == null)
        {
            ErrorLog("ScoreManagerが存在しませんでした");
            return;
        }

        _scoreManager.AddScore(amount);
    }

    /// <summary>
    /// 音ゲー判定に応じたスコアの加算を行う
    /// </summary>
    /// <param name="result">判定結果</param>
    public void UpdateScoreByJudge(JudgeResult result)
    {
        if (_scoreManager == null)
        {
            ErrorLog("ScoreManagerが存在しませんでした");
            return;
        }

        _scoreManager.UpdateScoreByJudge(result);
    }

    /// <summary>
    /// プレイ中の現在のスコアをリセットする
    /// </summary>
    public void ResetScore()
    {
        if (_scoreManager == null)
        {
            ErrorLog("ScoreManagerが存在しませんでした");
            return;
        }

        _scoreManager.ResetScore();
    }

    /// <summary>
    /// プレイ中の曲の現状の最高スコアをリセットする
    /// </summary>
    public void ResetHighScore()
    {
        if (_scoreManager == null)
        {
            ErrorLog("ScoreManagerが存在しませんでした");
            return;
        }

        _scoreManager.ResetHighScore();
    }

    /// <summary>
    /// スコアの値が更新された際に発火されるコールバック関数に処理を追加する
    /// 引数にintの値を一つ含んだvoid関数が登録可能
    /// </summary>
    /// <param name="cb">追加するAction変数</param>
    public void AddOnScoreChanged(Action<int> cb)
    {
        if (_scoreManager == null)
        {
            ErrorLog("ScoreManagerが存在しませんでした");
            return;
        }
        if (cb == null)
        {
            ErrorLog("[AddOnScoreChanged]で渡された値がnullです。");
            return;
        }

        _scoreManager.OnScoreChanged += cb;
    }

    /// <summary>
    /// スコアの値が更新された際に発火されるコールバック関数から処理を削除する
    /// 引数にintの値を一つ含んだvoid関数が登録可能
    /// </summary>
    /// <param name="cb">追加するAction変数</param>
    public void RemoveOnScoreChanged(Action<int> cb)
    {
        if (_scoreManager == null)
        {
            ErrorLog("ScoreManagerが存在しませんでした");
            return;
        }
        if (cb == null)
        {
            ErrorLog("[RemoveOnScoreChanged]で渡された値がnullです。");
            return;
        }

        _scoreManager.OnScoreChanged -= cb;
    }

    /// <summary>
    /// ハイスコアの値が更新された際に発火されるコールバック関数に処理を追加する
    /// 引数にintの値を一つ含んだvoid関数が登録可能
    /// </summary>
    /// <param name="cb">追加するAction変数</param>
    public void AddOnHighScoreChanged(Action<int> cb)
    {
        if (_scoreManager == null)
        {
            ErrorLog("ScoreManagerが存在しませんでした");
            return;
        }
        if (cb == null)
        {
            ErrorLog("[AddOnHighScoreChanged]で渡された値がnullです。");
            return;
        }

        _scoreManager.OnHighScoreChanged += cb;
    }


    /// <summary>
    /// ハイスコアの値が更新された際に発火されるコールバック関数から処理を削除する
    /// 引数にintの値を一つ含んだvoid関数が登録可能
    /// </summary>
    /// <param name="cb">追加するAction変数</param>
    public void RemoveOnHighScoreChanged(Action<int> cb)
    {
        if (_scoreManager == null)
        {
            ErrorLog("ScoreManagerが存在しませんでした");
            return;
        }
        if (cb == null)
        {
            ErrorLog("[RemoveOnHighScoreChanged]で渡された値がnullです。");
            return;
        }

        _scoreManager.OnHighScoreChanged -= cb;
    }
    #endregion

    #region Lane

    /// <summary>
    /// 現在のレーン番号：1番左が0,右に行けば1ずつ上がる
    /// </summary>
    public int CurrentLaneIndex => _laneManager.LaneIndex;

    /// <summary>
    /// 移動直前のレーン番号
    /// </summary>
    public int PrevLaneIndex => _laneManager.PrevLaneIndex;

    /// <summary>
    /// レーン移動してからの経過時間カウント変数
    /// レーン移動するたびに0.0からカウントし直す
    /// </summary>
    public double TimeSecOnMoveLane => _laneManager.TimeSecOnMoveLane;

    /// <summary>
    /// メインのリズムゲームを開始するときに発火する処理を登録する関数
    /// ・曲の再生
    /// ・ノーツの移動開始
    /// ・経過時間のカウントの開始
    /// ・MVの再生
    /// など
    /// 第一引数：移動前のレーン番号
    /// 第二引数：移動後のレーン番号
    /// </summary>
    /// <param name="cb">登録したい処理</param>
    public void AddLaneChangeEvent(Action<int, int> cb)
    {
        if (cb == null)
        {
            ErrorLog("レーン移動時のコールバックが正しくない形で渡されています。");
            return;
        }

        _laneManager.OnLaneChangeEvent += cb;
    }


    /// <summary>
    /// メインのリズムゲームを開始するときに発火する処理を削除する関数
    /// ・曲の再生
    /// ・ノーツの移動開始
    /// ・経過時間のカウントの開始
    /// ・MVの再生
    /// など
    /// 第一引数：移動前のレーン番号
    /// 第二引数：移動後のレーン番号
    /// </summary>
    /// <param name="cb">削除したい処理</param>
    public void RemoveLaneChangeEvent(Action<int, int> cb)
    {
        if (cb == null)
        {
            ErrorLog("レーン移動時時のコールバックが正しくない形で渡されています。");
            return;
        }

        _laneManager.OnLaneChangeEvent -= cb;
    }

    /// <summary>
    /// 一つ右のレーンにレーン番号を移動する
    /// 移動してしまうと範囲外になる場合、数値を変えずfalseを返す
    /// 移動可能な場合、数値を更新してtrueを返す
    /// </summary>
    /// <returns>移動が可能かどうか</returns>
    public bool MoveToRightLane()
    {
        return _laneManager.MoveToRightLane();
    }

    /// <summary>
    /// 一つ左のレーンにレーン番号を移動する
    /// 移動してしまうと範囲外になる場合、数値を変えずfalseを返す
    /// 移動可能な場合、数値を更新してtrueを返す
    /// </summary>
    /// <returns>移動が可能かどうか</returns>
    public bool MoveToLeftLane()
    {
        return _laneManager.MoveToLeftLane();
    }

    /// <summary>
    /// レーン番号を引数で与えられた番号に移動する
    /// 移動してしまうと範囲外になる場合、数値を変えずfalseを返す
    /// 移動可能な場合、数値を更新してtrueを返す
    /// </summary>
    /// <param name="laneIndex">移動先のレーン番号</param>
    /// <returns>移動が可能かどうか</returns>
    public bool MoveToLane(int laneIndex)
    {
        return _laneManager.MoveToLane(laneIndex);
    }

    /// <summary>
    /// レーン番号からレーンの座標を取得する関数
    /// レーンオブジェクトが確認できない、または範囲外の番号が指定された場合はエラーを残して現在位置を返す
    /// </summary>
    /// <param name="laneIndex"></param>
    /// <returns></returns>
    public Vector3 GetTargetLanePosition(int laneIndex)
    {
        return _laneManager.GetTargetLanePosition(laneIndex);
    }

    public async void OnLaneChange(int prevLaneIndex, int newLaneIndex)
    {
        double changedElapsedTime = BeatManager.Instance.elapsed;
        _cts.Cancel();
        _cts = new CancellationTokenSource();

        var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy(), _cts.Token);
        var token = linkedSource.Token;

        try
        {
            while (!token.IsCancellationRequested)
            {
                _laneManager.TimeSecOnMoveLane = BeatManager.Instance.elapsed - changedElapsedTime;
                // Debug.Log($"移動後：{_laneManager.TimeSecOnMoveLane}");
                await UniTask.Yield(cancellationToken: token);
            }
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("レーン移動後の秒数カウントをキャンセル");
            return;
        }
    }
    #endregion

    #region Note
    public void GenerateAppealNotes(SeniorMoveNote[] seniorMoveNotes)
    {
        _notesManager?.GenerateAppealNotes(seniorMoveNotes);
    }
    #endregion

    #region ComboManager
    /// <summary>
    /// プレイ中の曲の現在のコンボ
    /// </summary>
    public int CurrentCombo
    {
        get
        {
            if (_comboManager == null)
            {
                ErrorLog("ComboManagerが存在しませんでした");
                return 0;
            }

            return _comboManager.CurrentCombo;
        }
    }

    /// <summary>
    /// プレイ中の曲の現在のコンボに引数で与えた値を加算する
    /// 負の値を渡された場合、減算されず処理を行わずreturnする
    /// </summary>
    /// <param name="amount">コンボ加算量</param>
    public void AddCombo(int amount)
    {
        if (_comboManager == null)
        {
            ErrorLog("ComboManagerが存在しませんでした");
            return;
        }

        _comboManager.AddCombo(amount);
    }

    /// <summary>
    /// 音ゲー判定に応じたコンボの加算を行う
    /// </summary>
    /// <param name="result">判定結果</param>
    public void UpdateComboByJudge(JudgeResult result)
    {
        if (_comboManager == null)
        {
            ErrorLog("ComboManagerが存在しませんでした");
            return;
        }

        _comboManager.UpdateComboByJudge(result);
    }

    /// <summary>
    /// プレイ中の現在のコンボをリセットする
    /// </summary>
    public void ResetCombo()
    {
        if (_comboManager == null)
        {
            ErrorLog("ComboManagerが存在しませんでした");
            return;
        }

        _comboManager.ResetCombo();
    }

    public bool ShowCombo()
    {
        if (_comboManager == null)
        {
            ErrorLog("ComboManagerが存在しませんでした");
            return false;
        }

        return _comboManager.ShowCombo;
    }

    /// <summary>
    /// コンボの値が更新された際に発火されるコールバック関数に処理を追加する
    /// 引数にintの値を一つ含んだvoid関数が登録可能
    /// </summary>
    /// <param name="cb">追加するAction変数</param>
    public void AddOnComboChanged(Action<int> cb)
    {
        if (_comboManager == null)
        {
            ErrorLog("ComboManagerが存在しませんでした");
            return;
        }
        if (cb == null)
        {
            ErrorLog("[AddOnComboChanged]で渡された値がnullです。");
            return;
        }

        _comboManager.OnComboChanged += cb;
    }

    /// <summary>
    /// コンボの値が更新された際に発火されるコールバック関数から処理を削除する
    /// 引数にintの値を一つ含んだvoid関数が登録可能
    /// </summary>
    /// <param name="cb">追加するAction変数</param>
    public void RemoveOnComboChanged(Action<int> cb)
    {
        if (_comboManager == null)
        {
            ErrorLog("comboManagerが存在しませんでした");
            return;
        }
        if (cb == null)
        {
            ErrorLog("[RemoveOnComboChanged]で渡された値がnullです。");
            return;
        }

        _comboManager.OnComboChanged -= cb;
    }
    #endregion

    #region Judge


    /// <summary>
    /// 判定時コールバックイベントに処理を登録する
    /// 戻り値なし/JudgeResult型の引数/の二条件を満たす処理を入れる必要あり
    /// </summary>
    /// <param name="cb">登録したい処理</param>
    public void AddOnJudgeEvent(Action<JudgeResult> cb)
    {
        if (_judgeManager == null)
        {
            ErrorLog("ScoreManagerが存在しませんでした");
            return;
        }
        _judgeManager.AddOnJudgeEvent(cb);
    }

    /// <summary>
    /// 判定時コールバックイベントから処理を削除する
    /// 戻り値なし/JudgeResult型の引数/の二条件を満たす処理を入れる必要あり
    /// </summary>
    /// <param name="cb">登録したい処理</param>
    public void RemoveOnJudgeEvent(Action<JudgeResult> cb)
    {
        if (_judgeManager == null)
        {
            ErrorLog("ScoreManagerが存在しませんでした");
            return;
        }
        _judgeManager.RemoveOnJudgeEvent(cb);
    }

    /// <summary>
    /// ノーツが判定ラインに乗った際に呼ばれる判定処理
    /// 引数で渡されたノーツのレーン番号と今のプレイヤーのレーン番号が一致していないのであればGreat(今はPerfect)
    /// 一致していればMiss
    ///
    /// その判定をもとに、エフェクトの生成とスコアの更新の関数を呼ぶ
    /// </summary>
    /// <param name="noteIndex">ノーツのCSV番号</param>
    /// <param name="laneIndex">ノーツのレーン番号</param>
    public void JudgeOnNotes(int noteIndex, int laneIndex)
    {
        if (_judgeManager == null)
        {
            ErrorLog("ScoreManagerが存在しませんでした");
            return;
        }
        _judgeManager.JudgeOnNotes(noteIndex, laneIndex);
    }

    /// <summary>
    /// アピールチャンスの判定タイミングを設定する
    /// アピールチャンス突入時に呼ぶ想定
    /// </summary>
    /// <param name="judgeElapsedSec">アピールチャンスの判定タイミングの経過時間</param>
    public void SetAppealJudgeSec(double judgeElapsedSec)
    {
        if (_judgeManager == null)
        {
            ErrorLog("ScoreManagerが存在しませんでした");
            return;
        }
        _judgeManager.SetAppealJudgeSec(judgeElapsedSec);
    }

    /// <summary>
    /// アピールチャンスの判定を行う
    /// プレイヤーがアピールをした際に呼ぶ想定
    /// あらかじめ設定されたタイミングとの誤差が+-で閾値いないなら成功とし、
    /// 成功したか否かをbool値で返す
    /// </summary>
    /// <param name="elapsed">アピールしたときの経過時間</param>
    public bool AppealChanceJudge(double elapsed)
    {
        if (_judgeManager == null)
        {
            ErrorLog("ScoreManagerが存在しませんでした");
            return false;
        }
        return _judgeManager.AppealChanceJudge(elapsed);
    }
    #endregion

    /// <summary>
    /// 引数で与えられた内容を、このクラスのエラーログとして残す関数
    /// 引数の値がnullもしくはemptyの場合出力しない
    /// </summary>
    /// <param name="sentence"></param>
    private void ErrorLog(string sentence)
    {
        if (string.IsNullOrEmpty(sentence)) return;

        Debug.LogError($"{typeof(GameManager).Name}：{sentence}");
    }
}
