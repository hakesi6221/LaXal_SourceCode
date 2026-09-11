using System.Linq;
using UnityEngine;

/// <summary>
/// NoteDataを複数テーブルとして保持するためのデータクラス
/// ScriptableObjectとして生成する想定
/// </summary>
[CreateAssetMenu(fileName = "NoteDatasTable", menuName = "ScriptableObjects/NoteDatasTable")]
public class NotePrefabDatasTable : ScriptableObject
{
    [Header("ノーツデータ")]
    [SerializeField]
    private NotePrefabData[] _noteDatas = null;

    /// <summary>
    /// ノーツデータの配列
    /// </summary>
    public NotePrefabData[] NoteDatas => _noteDatas;

    /// <summary>
    /// ノートデータを取得する
    /// </summary>
    /// <param name="index">CSV内での番号</param>
    /// <returns></returns>
    public NotePrefabData GetNoteData(int index)
    {
        if (_noteDatas == null)
        {
            Debug.LogError("ノーツデータの取得に失敗しました。ノーツデータの配列が存在しません。");
            return null;
        }

        NotePrefabData[] matched = _noteDatas.Where(data => data.CSVIndex == index).ToArray();
        NotePrefabData result = matched.FirstOrDefault();
        if (1 < matched.Length)
        {
            Debug.LogError("ノーツデータの取得に失敗しました。同じインデックスのノーツデータが複数存在しています。");
        }

        return result;
    }
}
