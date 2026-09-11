using System;
using UnityEngine;

/// <summary>
/// ノーツのプレハブと、CSV内での番号を紐づけるためのデータクラス
/// </summary>
[Serializable]
public class NotePrefabData
{
    // CSV内での番号
    [SerializeField]
    private int _csvIndex = 0;

    // 紐づけるノーツのプレハブ
    [SerializeField]
    private NoteMove _notePrefab = null;

    /// <summary>
    /// CSV内での番号
    /// </summary>
    public int CSVIndex => _csvIndex;

    /// <summary>
    /// ノーツのプレハブ
    /// </summary>
    public NoteMove NotePrefab => _notePrefab;
}
