using System;
using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// ノーツの生成及び管理を担当するクラス
/// </summary>
[Serializable]
public class NotesManager
{
    [SerializeField, Label("各レーンノーツ生成者")]
    private NotesSpawn[] _laneObjects = null;

    [SerializeField, Label("アピール判定のノーツ")]
    private AppealNoteGenerator _appealNoteGen = null;

    [SerializeField, Label("ノーツの速さ")]
    private float _notesSpeed = 1.0f;

    [SerializeField, Label("ノーツの大きさの倍率")]
    private Vector3 _noteScaleMult = Vector3.back;

    [SerializeField, Label("ノーツ回転オフセット")]
    private Vector3 _notesRotationOffset;

    [SerializeField, Label("ノーツのalphaが1になる判定ラインからの距離")]
    private float _notesVisuableDistance = 20.0f;

    [SerializeField, Label("通常ノーツプレハブ"), BoxGroup("Notes Prefabs")]
    private NoteMove _normalNotePrefab = null;

    [SerializeField, Label("ロングノーツ開始プレハブ"), BoxGroup("Notes Prefabs")]
    private NoteMove _longNoteStartPrefab = null;

    [SerializeField, Label("ロングノーツ終了プレハブ"), BoxGroup("Notes Prefabs")]
    private NoteMove _longNoteFinishPrefab = null;

    [SerializeField, Label("分裂後ノーツプレハブ"), BoxGroup("Notes Prefabs")]
    private NoteMove _doppelNotePrefab = null;

    public void Initialize()
    {
        var gameMode = RhythmGameInfomation.GameMode;
        if ((int)gameMode != _laneObjects.Length)
        {
            Debug.LogError($"{typeof(LaneManager).Name}:現在のゲームモードとレーンの数が一致しません");
            return;
        }
        NoteMove specialNotePrefab = RhythmGameInfomation.SpecialNotePrefab;

        switch (gameMode)
        {
            case RhythmGameMode.ThreeLane:
                SetScoresToLanes(RhythmGameInfomation.Scores3Lane, specialNotePrefab);
                break;
            case RhythmGameMode.FourLane:
                SetScoresToLanes(RhythmGameInfomation.Scores4Lane, specialNotePrefab);
                break;
            default:
                Debug.LogError($"{typeof(LaneManager).Name}:例外的なゲームモードになっています");
                return;
        }

        // ノーツを実際に生成
        foreach (var noteSpawn in _laneObjects)
        {
            if (noteSpawn == null) continue;

            noteSpawn.StartSpawn();
        }

        if (RhythmGameInfomation.GameMode == RhythmGameMode.FourLane)
            _appealNoteGen?.Initialize
            (
                _notesSpeed,
                _noteScaleMult,
                _notesRotationOffset,
                _notesVisuableDistance
            );
    }

    /// <summary>
    /// レーンたちに楽曲の譜面を渡す関数
    /// </summary>
    /// <param name="scoreData">楽曲の譜面データ</param>
    /// <param name="specialNotePrefab">特殊ノーツのプレハブ</param>
    private void SetScoresToLanes(ScoreData3Lane scoreData, NoteMove specialNotePrefab)
    {
        for (int i = 0; i < _laneObjects.Length; i++)
        {
            _laneObjects[i].Initialize
            (
                this,
                scoreData.GetScoreData(i + 1),
                _notesSpeed,
                _noteScaleMult,
                _notesRotationOffset,
                _notesVisuableDistance,
                _normalNotePrefab,
                _longNoteStartPrefab,
                _longNoteFinishPrefab,
                specialNotePrefab
            );
        }
    }

    /// <summary>
    /// レーンたちに楽曲の譜面を渡す関数
    /// </summary>
    /// <param name="scoreData">楽曲の譜面データ</param>
    /// <param name="specialNotePrefab">特殊ノーツのプレハブ</param>
    private void SetScoresToLanes(ScoreData4Lane scoreData, NoteMove specialNotePrefab)
    {
        for (int i = 0; i < _laneObjects.Length; i++)
        {
            _laneObjects[i].Initialize
            (
                this,
                scoreData.GetScoreData(i + 1),
                _notesSpeed,
                _noteScaleMult,
                _notesRotationOffset,
                _notesVisuableDistance,
                _normalNotePrefab,
                _longNoteStartPrefab,
                _longNoteFinishPrefab,
                specialNotePrefab
            );
        }
    }

    public void GenerateAppealNotes(SeniorMoveNote[] seniorMoveNotes)
    {
        if (_appealNoteGen == null) return;

        _appealNoteGen.GenerateAppealNotes(seniorMoveNotes);
    }

    /// <summary>
    /// 分裂する特殊ノーツを生成したときに呼ぶ処理
    /// 全ノーツ生成者にアクセスし、分裂後のノーツを生成させる
    /// </summary>
    /// <param name="noteData">ノーツの生成情報</param>
    /// <param name="laneIndex">分裂ノーツが生成されたレーン番号</param>
    public void OnGenerateIncreaseNotes(NotesSpawn.NoteData noteData, int laneIndex)
    {
        if (_laneObjects == null) return;
        for (int i = 0; i < _laneObjects.Length; i++)
        {
            var noteSpawn = _laneObjects[i];
            if (noteSpawn == null) continue;
            if (i == laneIndex) continue;

            noteSpawn.CreateDoppelNote(noteData, _doppelNotePrefab);
        }
    }
}
