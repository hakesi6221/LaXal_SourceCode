using UnityEngine;
using System;

/// <summary>
/// コメントのマスターデータアセットとメイン進行状況Enumを紐づけるためのデータクラス
/// </summary>
[Serializable]
public class CommentMasterData
{
    [SerializeField]
    private ProgressStatus _status = ProgressStatus.None;

    [SerializeField]
    private TextAsset _commentMaster = null;

    /// <summary>
    /// 対応するメイン進行状況
    /// </summary>
    public ProgressStatus Status => _status;

    /// <summary>
    /// 対応しているコメントのマスターデータ
    /// </summary>
    public TextAsset CommentMaster => _commentMaster;
}