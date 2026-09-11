using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// リズムゲームに必要な、楽曲の情報をまとめて保持するクラス
/// ・曲名
/// ・BPM
/// ・ジャケット画像
/// ・音声素材
/// ・動画素材
/// </summary>
[Serializable]
[CreateAssetMenu(fileName = "MusicData", menuName = "ScriptableObjects/MusicData")]
public class MusicData : ScriptableObject
{
    [Header("数値")]
    [SerializeField]
    private string _name = string.Empty;

    // 楽曲タイプ
    [SerializeField]
    private MusicType _type = MusicType.None;

    // BPM
    [SerializeField]
    private int _bpm = 0;

    // 最大スコア
    [SerializeField]
    private int _maxScoreEasy = 0;
    [SerializeField]
    private int _maxScoreNormal = 0;
    [SerializeField]
    private int _maxScoreHard = 0;
    [SerializeField]
    private int _maxScore3Lane = 0;

    [Header("楽譜参照")]
    // 譜面CSV_3レーン
    [SerializeField]
    private ScoreData3Lane _score3Lane = null;

    // 譜面CSV_4レーン
    [SerializeField]
    private ScoreData4Lane[] _scores4Lane = null;

    [Header("アセット参照")]
    // 特殊ノーツのプレハブ
    [SerializeField]
    private NoteMove _specialNotePrefab = null;

    // ジャケット画像
    [SerializeField]
    private JacketSpriteData _jacket = null;

    // 先輩アセット情報
    [SerializeField]
    private CharacterAssetsData _charaAsset = null;

    // 音声素材
    [SerializeField]
    private AudioClip _audio = null;

    // 楽曲音声素材ラベル
    [SerializeField]
    private AudioType _audioType = AudioType.None;

    // 楽曲音声(インスト)素材ラベル
    [SerializeField]
    private AudioType _instAudioType = AudioType.None;

    // 動画素材
    [SerializeField]
    private VideoClip _movie = null;

    // 特殊ノーツの説明動画
    [SerializeField]
    private VideoClip _movieExNotes = null;

    // 先輩の動作CSVデータ
    [SerializeField]
    private SeniorMovementDatas _seniorMoveDatas = null;

    // スーパーチャット
    [SerializeField]
    private TextAsset _superChatCSV = null;

    /// <summary>
    /// 楽曲名
    /// </summary>
    public string Name => _name;

    /// <summary>
    /// 楽曲の種類
    /// </summary>
    public MusicType Type => _type;

    /// <summary>
    /// BPM
    /// </summary>
    public int BPM => _bpm;

    /// <summary>
    /// 最大スコア
    /// </summary>
    public int MaxScore(RhythmGameMode gameMode, MusicDifficulty difficulty)
    {
        int result = 1;
        if (gameMode == RhythmGameMode.ThreeLane)
            return _maxScore3Lane;
        else if (gameMode == RhythmGameMode.FourLane)
        {
            switch (difficulty)
            {
                case MusicDifficulty.Easy:
                    result = _maxScoreEasy;
                    break;
                case MusicDifficulty.Normal:
                    result = _maxScoreNormal;
                    break;
                case MusicDifficulty.Hard:
                    result = _maxScoreHard;
                    break;
                default:
                    break;
            }
        }
        return result;
    }

    /// <summary>
    /// ジャケット画像
    /// </summary>
    public JacketSpriteData Jacket => _jacket;

    /// <summary>
    /// 先輩のアセットデータ
    /// </summary>
    public CharacterAssetsData CharaAsset => _charaAsset;

    /// <summary>
    /// 3レーンの楽譜CSV
    /// </summary>
    public ScoreData3Lane GetScore3Lane => _score3Lane;

    /// <summary>
    /// 指定した難易度の4レーンの楽譜CSV
    /// </summary>
    public ScoreData4Lane GetScore4Lane(MusicDifficulty diff)
    {
        var scores = _scores4Lane.Where(score => score.Difficulty == diff)
                                .ToArray();
        if (scores.Length >= 2)
        {
            Debug.LogError($"曲の譜面において、同じ難易度のものが複数存在します。music=>{_type}");
            return null;
        }

        return scores.FirstOrDefault();
    }

    /// <summary>
    /// 音声素材
    /// </summary>
    public AudioClip Audio => _audio;

    /// <summary>
    /// 楽曲音声素材ラベル
    /// </summary>
    public AudioType AudioType => _audioType;

    /// <summary>
    /// 楽曲音声(インスト)素材ラベル
    /// </summary>
    public AudioType InstAudioType => _instAudioType;

    /// <summary>
    /// 動画素材
    /// </summary>
    public VideoClip Movie => _movie;

    /// <summary>
    /// 特殊ノーツの説明動画
    /// </summary>
    public VideoClip MovieExNotes => _movieExNotes;

    /// <summary>
    /// 先輩の動作CSVデータ
    /// </summary>
    public SeniorMovementData GetSeniorMoveDatas(MusicDifficulty diff)
    {
        var moveMentData = _seniorMoveDatas.Datas.Where(score => score.Difficulty == diff)
                                .ToArray();
        if (moveMentData.Length >= 2)
        {
            Debug.LogError($"曲の譜面において、同じ難易度のものが複数存在します。music=>{_type}");
            return null;
        }

        return moveMentData.FirstOrDefault();
    }

    /// <summary>
    /// スーパーチャット
    /// </summary>
    public TextAsset SuperChatCSV => _superChatCSV;

    /// <summary>
    /// 特殊ノーツのプレハブ
    /// </summary>
    public NoteMove SpecialNotePrefab => _specialNotePrefab;
}