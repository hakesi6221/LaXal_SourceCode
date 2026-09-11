using UnityEngine;

/// <summary>
/// コメントのマスターデータを一覧としてアセット内で保持するためのデータクラス
/// </summary>
[CreateAssetMenu(fileName = "CommentMasterDatas", menuName = "ScriptableObjects/CommentMasterDatas")]
public class CommentMasterDatas : ScriptableObject
{
    [SerializeField]
    private CommentMasterData[] _commentDatas = null;

    /// <summary>
    /// コメントのマスターデータ一覧
    /// </summary>
    public CommentMasterData[] CommentDatas => _commentDatas;

    /// <summary>
    /// マスターデータの数
    /// </summary>
    public int Length => _commentDatas.Length;

    /// <summary>
    /// 指定した進行状況に対応するコメントのマスターデータを取得する
    /// </summary>
    /// <param name="status">取得したいマスターデータの進行状態</param>
    /// <returns></returns>
    public CommentMasterData GetCommentMasterData(ProgressStatus status)
    {
        CommentMasterData result = null;
        foreach (CommentMasterData data in _commentDatas)
        {
            if (data.Status == status)
            {
                if (result != null)
                    Debug.LogWarning($"{typeof(CommentMasterDatas).Name}:[GetCommentMasterData(ProgressStatus status)]指定された曲名の楽曲データが複数存在しています。");
                result = data;
            }

        }

        if (result == null)
            Debug.LogError($"{typeof(CommentMasterDatas).Name}:[GetCommentMasterData(ProgressStatus status)]指定された曲名の楽曲データが存在しません。");

        return result;
    }
}
