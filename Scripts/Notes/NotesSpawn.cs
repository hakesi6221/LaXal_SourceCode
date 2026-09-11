using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

/// <summary>
/// CSVの時間に合わせてノーツを生成するクラス
/// </summary>
public class NotesSpawn : MonoBehaviour
{
    [Header("ノーツの生成位置")]
    [SerializeField] private Transform _generatePos;

    // ノーツの速度
    private float _noteSpeed;

    // ノーツの流れる向き
    private Vector3 _noteScaleMult = Vector3.back;

    // ノーツ回転オフセット
    private Vector3 _rotationOffset;

    [SerializeField, Range(0, 3)] private int _laneIndex = 0;

    private readonly List<NoteData> _noteDatas = new List<NoteData>();

    // ノーツの管理クラス
    private NotesManager _noteManager = null;
    private int _nextNoteIndex;

    // ノーツのalphaが1になる判定ラインからの距離
    private float _notesVisuableDistance;
    private NoteData _longNoteStart;

    // あなたのいちばんちかくで の直前生成特殊ノーツインスタンス参照
    // 行動タイミングを渡すために保持
    private AccelNoteMove _lastAccelNote = null;

    // ---各ノーツのプレハブ---
    private NoteMove _normalNotePrefab = null;
    private NoteMove _longNoteStartPrefab = null;
    private NoteMove _longNoteFinishPrefab = null;
    private NoteMove _specialNotePrefab = null;

    /// <summary>
    /// 更新：原
    /// 
    /// ノーツ生成者に担当の譜面csvをアタッチする初期化関数
    /// </summary>
    /// <param name="noteCSV"></param>
    public void Initialize
    (
        NotesManager manager,
        TextAsset noteCSV,
        float noteSpeed,
        Vector3 noteScaleMult,
        Vector3 rotationOffset,
        float notesVisuableDis,
        NoteMove normalNotePrefab,
        NoteMove longNoteStartPrefab,
        NoteMove longNoteFinishPrefab,
        NoteMove specialNotePrefab
    )
    {
        _noteManager = manager;
        _noteSpeed = noteSpeed;
        _noteScaleMult = noteScaleMult;
        _rotationOffset = rotationOffset;
        _notesVisuableDistance = notesVisuableDis;
        _normalNotePrefab = normalNotePrefab;
        _longNoteStartPrefab = longNoteStartPrefab;
        _longNoteFinishPrefab = longNoteFinishPrefab;
        _specialNotePrefab = specialNotePrefab;
        LoadCsv(noteCSV);
    }

    /// <summary>
    /// CSVを読み込んでノーツデータに変換する
    /// </summary>
    private void LoadCsv(TextAsset noteCSV)
    {
        if (noteCSV == null)
        {
            Debug.LogError("CSVファイルが設定されていません。");
            return;
        }

        using StringReader reader = new StringReader(noteCSV.text);

        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine();

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            string[] cells = line.Split(',');

            if (cells.Length < 3)
            {
                Debug.LogWarning($"CSVの形式が不正です: {line}");
                continue;
            }

            float spawnTime = float.Parse(cells[0], CultureInfo.InvariantCulture);
            int noteType = int.Parse(cells[1]);
            // float longNoteTime = float.Parse(cells[2], CultureInfo.InvariantCulture);

            _noteDatas.Add(new NoteData(spawnTime, noteType));
        }

        _noteDatas.Sort((a, b) => a.SpawnTime.CompareTo(b.SpawnTime));
    }

    /// <summary>
    /// ノーツ生成を開始する
    /// </summary>
    public void StartSpawn()
    {
        _nextNoteIndex = 0;

        while (_nextNoteIndex < _noteDatas.Count)
        {
            NoteData noteData = _noteDatas[_nextNoteIndex];

            SpawnNote(noteData);
            _nextNoteIndex++;
        }
    }

    public void CreateNote(NoteData noteData, NoteMove notePrefab, int noteIndex)
    {
        if (notePrefab == null)
        {
            Debug.LogWarning($"存在しないノーツタイプです:{notePrefab}");
            return;
        }

        Quaternion noteRotation = _generatePos.rotation * Quaternion.Euler(_rotationOffset);

        NoteMove note = Instantiate(
            notePrefab,
            _generatePos.position,
            noteRotation,
            _generatePos);

        note.Initialize(
            _generatePos,
            noteData.SpawnTime,
            _noteSpeed,
            _noteScaleMult,
            _laneIndex,
            noteIndex,
            _notesVisuableDistance);

        // あなたのいちばんちかくで の特殊ノーツだった場合、この後も情報を渡す必要があるためインスタンスを保持する
        if (note is AccelNoteMove)
            _lastAccelNote = note as AccelNoteMove;
        // kawaiiNo1の特殊ノーツの場合、分裂後ノーツを生成しておく
        if (note is IncreaseNoteMove)
            _noteManager?.OnGenerateIncreaseNotes(noteData, _laneIndex);
    }

    private void CreateLongNote(NoteData start, NoteData end)
    {
        const int LONG_HEAD_PREFAB = 2;
        const int LONG_TAIL_PREFAB = 3;

        Quaternion noteRotation = _generatePos.rotation * Quaternion.Euler(_rotationOffset);

        // 頭生成
        NoteMove head = Instantiate(
            _longNoteStartPrefab,
            _generatePos.position,
            noteRotation,
            _generatePos);

        head.Initialize(
            _generatePos,
            start.SpawnTime,
            _noteSpeed,
            _noteScaleMult,
            _laneIndex,
            LONG_HEAD_PREFAB,
            _notesVisuableDistance);

        LongNote longNote = head.GetComponentInChildren<LongNote>();

        float length = (end.SpawnTime - start.SpawnTime) * _noteSpeed;
        longNote.SetLength(length);

        NoteMove tail = Instantiate(
            _longNoteFinishPrefab,
            longNote.GetTailPosition(),
            noteRotation,
            _generatePos);

        tail.Initialize(
            _generatePos,
            end.SpawnTime,
            _noteSpeed,
            _noteScaleMult,
            _laneIndex,
            LONG_TAIL_PREFAB,
            _notesVisuableDistance);
    }

    public void CreateDoppelNote(NoteData noteData, NoteMove notePrefab)
    {
        if (notePrefab == null)
        {
            Debug.LogError($"{this.name}:分裂ノーツの生成に失敗しました。分裂後ノーツのプレハブ参照が存在しません。");
            return;
        }

        CreateNote(noteData, notePrefab, (int)NoteType.Normal);
    }

    /// <summary>
    /// ノーツを生成する
    /// </summary>
    private void SpawnNote(NoteData noteData)
    {
        switch (noteData.NoteType)
        {
            // 通常ノーツ
            case 1:
                CreateNote(noteData, _normalNotePrefab,  noteData.NoteType);
                break;
            // ロングノーツ開始
            case 2:
                _longNoteStart = noteData;
                break;

            // ロングノーツ終了
            case 3:
                if (_longNoteStart == null)
                {
                    Debug.LogWarning("ロングノーツ開始がありません");
                    return;
                }

                CreateLongNote(_longNoteStart, noteData);

                _longNoteStart = null;
                break;
            // 特殊ノーツ
            case 4:
                CreateNote(noteData, _specialNotePrefab,  noteData.NoteType);
                break;
            // 特殊コマンド
            case 5:
                _lastAccelNote?.SetChangeColorElapsed(noteData.SpawnTime);
                break;
            case 6:
                _lastAccelNote?.SetAccelElapsed(noteData.SpawnTime);
                // 情報の入力を終えたら参照を切る
                _lastAccelNote = null;
                break;
            default:
                // Debug.LogError($"存在しない種類のノーツ生成指示が出されています lane={_laneIndex}");
                break;
        }
    }

    public class NoteData
    {
        public float SpawnTime { get; }
        public int NoteType { get; }

        public NoteData(float spawnTime, int noteType)
        {
            SpawnTime = spawnTime;
            NoteType = noteType;
        }
    }
}