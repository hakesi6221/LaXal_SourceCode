using System;
using UnityEngine;

/// <summary>
/// 先輩の動作CSVと楽曲難易度を紐づけるクラス
/// </summary>
[Serializable]
public class SeniorMovementData
{
    [SerializeField]
    private MusicDifficulty _difficulty = MusicDifficulty.None;

    [SerializeField]
    private TextAsset _movementData = null;

    /// <summary>
    /// 対応難易度
    /// </summary>
    public MusicDifficulty Difficulty => _difficulty;
    /// <summary>
    /// マスターデータ
    /// </summary>
    public TextAsset MovementData => _movementData;
}