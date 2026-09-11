using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// リズムゲームで必要な情報、結果の情報を保持する静的クラス
/// 曲選択画面後から、リザルト画面まで通してプレイ中の楽曲情報を保持しておくのに使用する
/// 曲選択画面移行時もしくはリザルト画面終了時に必ずリセットする必要あり
/// </summary>
public static class RhythmGameInfomation
{
    // リズムゲームのプレイモード
    private static RhythmGameMode s_gameMode = RhythmGameMode.None;

    private static bool s_tutorial = false;

    // 楽曲名
    private static string s_name = string.Empty;

    // 楽曲種類
    private static MusicType s_type = MusicType.None;

    // 難易度
    private static MusicDifficulty s_difficulty = MusicDifficulty.None;

    // BPM
    private static int s_bpm = 0;

    // 最大スコア
    private static int s_maxScore = 0;

    // 特殊ノーツのプレハブ
    private static NoteMove s_specialNotePrefab = null;

    // 譜面CSV_3レーン
    private static ScoreData3Lane s_scores3Lane = null;

    // 譜面CSV_4レーン
    private static ScoreData4Lane s_scores4Lane = null;

    // ジャケット画像
    private static JacketSpriteData s_jacket = null;

    // 音声素材
    private static AudioClip s_audio = null;

    // 楽曲音声素材ラベル
    private static AudioType s_audioType = AudioType.None;

    // 楽曲音声(インスト)素材ラベル
    private static AudioType s_instAudioType = AudioType.None;

    // 動画素材
    private static VideoClip s_movie = null;

    // 先輩の動作マスターデータ
    private static TextAsset s_seniorMove = null;

    // スーパーチャットのCSV
    private static TextAsset s_superChatCSV = null;

    // 楽曲対応キャラクターアセットデータ
    private static CharacterAssetsData s_featureCharaAssets = null;

    // プレイ後最終的なスコア
    private static int s_finalScore = 0;

    // プレイ後の、判定ごとの判定数
    private static Dictionary<JudgeResult, int> s_judgeNums = new Dictionary<JudgeResult, int>();

    /// <summary>
    /// リズムゲームのプレイモード
    /// </summary>
    public static RhythmGameMode GameMode => s_gameMode;

    /// <summary>
    /// チュートリアルかどうか
    /// </summary>
    public static bool Tutorial => s_tutorial;

    /// <summary>
    /// 楽曲名
    /// </summary>
    public static string Name => s_name;

    /// <summary>
    /// 楽曲種類
    /// </summary>
    public static MusicType Type => s_type;

    /// <summary>
    /// 難易度
    /// </summary>
    public static MusicDifficulty Difficulty => s_difficulty;

    /// <summary>
    /// BPM
    /// </summary>
    public static int BPM => s_bpm;

    /// <summary>
    /// 最大スコア
    /// </summary>
    public static int MaxScore => s_maxScore;

    /// <summary>
    /// 楽曲の特殊ノーツのプレハブ
    /// </summary>
    public static NoteMove SpecialNotePrefab => s_specialNotePrefab;

    /// <summary>
    /// ジャケット画像
    /// </summary>
    public static JacketSpriteData Jacket => s_jacket;

    /// <summary>
    /// 3レーンの楽譜CSVの配列
    /// </summary>
    public static ScoreData3Lane Scores3Lane => s_scores3Lane;

    /// <summary>
    /// 4レーンの楽譜CSVの配列
    /// </summary>
    public static ScoreData4Lane Scores4Lane => s_scores4Lane;

    /// <summary>
    /// 音声素材
    /// </summary>
    public static AudioClip Audio => s_audio;

    /// <summary>
    /// 楽曲音声素材ラベル
    /// </summary>
    public static AudioType AudioType => s_audioType;

    /// <summary>
    /// 楽曲音声(インスト)素材ラベル
    /// </summary>
    public static AudioType InstAudioType => s_instAudioType;

    /// <summary>
    /// 動画素材
    /// </summary>
    public static VideoClip Movie => s_movie;

    /// <summary>
    /// 先輩の動作マスターデータ
    /// </summary>
    public static TextAsset SeniorMove => s_seniorMove;

    /// <summary>
    /// スーパーチャットのCSV
    /// </summary>
    public static TextAsset SuperChatCSV => s_superChatCSV;

    /// <summary>
    /// 楽曲対応キャラクターアセットデータ
    /// </summary>
    public static CharacterAssetsData FeatureCharaAssets => s_featureCharaAssets;

    /// <summary>
    /// プレイ後最終的なスコア
    /// </summary>
    public static int FinalScore => s_finalScore;

    /// <summary>
    /// プレイ後の、判定ごとの判定数
    /// </summary>
    public static Dictionary<JudgeResult, int> JudgeNums => s_judgeNums;

    /// <summary>
    /// リズムゲームのプレイモードを指定する関数
    /// タイトル画面でのモード選択時に必ず呼ぶ想定
    /// </summary>
    /// <param name="gameMode"></param>
    public static void SetGameMode(RhythmGameMode gameMode)
    {
        if (gameMode == RhythmGameMode.None)
        {
            Debug.LogWarning($"{typeof(RhythmGameInfomation).Name}:ゲームモードが[None]で指定されています");
        }
        s_gameMode = gameMode;
    }

    /// <summary>
    /// インゲームに必要な情報を保存する関数
    /// </summary>
    /// <param name="musicType">プレイする曲名</param>
    /// <param name="bpm">プレイする曲のBPM</param>
    /// <param name="maxScore">プレイする曲の最大スコア</param>
    /// <param name="specialNotePrefab">プレイする曲の特殊ノーツのプレハブ</param>
    /// <param name="scoreData3Lane">プレイする曲の3レーンモード楽曲データ</param>
    /// <param name="scoreData4Lane">プレイする曲の4レーンモード楽曲データ</param>
    /// <param name="jacket">プレイする曲のジャケット画像</param>
    /// <param name="audio">プレイする曲の音源</param>
    /// <param name="audioType">楽曲音声素材ラベル</param>
    /// <param name="instAudioType">楽曲音声(インスト)素材ラベル</param>
    /// <param name="movie">プレイする曲の動画</param>
    /// <param name="seniorMovement">先輩の動作マスターデータ</param>
    /// <param name="superChatCSV">プレイする曲の動画</param>
    public static void SetInGameInfomations(
        string musicName,
        MusicType musicType,
        MusicDifficulty difficulty,
        int bpm,
        int maxScore,
        NoteMove specialNotePrefab,
        ScoreData3Lane scoreData3Lane,
        ScoreData4Lane scoreData4Lane,
        JacketSpriteData jacket,
        AudioClip audio,
        AudioType audioType,
        AudioType instAudioType,
        VideoClip movie,
        TextAsset seniorMovement,
        TextAsset superChatCSV,
        CharacterAssetsData featureCharaAssets,
        bool tutorial = false
    )
    {
        s_name = musicName;
        s_type = musicType;
        s_difficulty = difficulty;
        s_bpm = bpm;
        s_maxScore = maxScore;
        s_specialNotePrefab = specialNotePrefab;
        s_scores3Lane = scoreData3Lane;
        s_scores4Lane = scoreData4Lane;
        s_jacket = jacket;
        s_audio = audio;
        s_audioType = audioType;
        s_instAudioType = instAudioType;
        s_movie = movie;
        s_seniorMove = seniorMovement;
        s_superChatCSV = superChatCSV;
        s_featureCharaAssets = featureCharaAssets;
        s_tutorial = tutorial;
    }

    /// <summary>
    /// リザルトに関した情報を保存する関数
    /// </summary>
    /// <param name="finalScore"></param>
    /// <param name="judgeNums"></param>
    public static void SetResultInfomations(
        int finalScore,
        Dictionary<JudgeResult, int> judgeNums
    )
    {
        s_finalScore = finalScore;
        s_judgeNums = judgeNums;
    }

    /// <summary>
    /// 保存している情報のすべてをリセットする関数
    /// 曲選択画面開始時にもしくはリザルト画面終了時に呼ぶ想定
    /// </summary>
    public static void ResetInfomations()
    {
        // s_gameMode = RhythmGameMode.None;
        s_tutorial = false;
        s_name = string.Empty;
        s_type = MusicType.None;
        s_bpm = 0;
        s_maxScore = 0;
        s_specialNotePrefab = null;
        s_scores3Lane = null;
        s_scores4Lane = null;
        s_jacket = null;
        s_audio = null;
        s_audioType = AudioType.None;
        s_instAudioType = AudioType.None;
        s_movie = null;
        s_seniorMove = null;
        s_superChatCSV = null;
        s_featureCharaAssets = null;
        s_finalScore = 0;
        s_judgeNums = new Dictionary<JudgeResult, int>();
    }
}
