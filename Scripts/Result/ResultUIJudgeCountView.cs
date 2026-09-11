using System;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

/// <summary>
/// リザルト画面の判定回数表示UIの見た目担当クラス
/// </summary>
[Serializable]
public class ResultUIJudgeCountView
{
    [SerializeField, Label("判定の種類")]
    private JudgeResult _judgeResult = JudgeResult.None;

    [SerializeField, Label("カウント表示TMP")]
    private TextMeshProUGUI _countDisplayTMP = null;

    /// <summary>
    /// 判定の種類
    /// </summary>
    public JudgeResult JudgeResult => _judgeResult;

    /// <summary>
    /// カウント表示TMPのテキストに判定の回数を適用
    /// </summary>
    /// <param name="count"></param>
    public void SetCountToText(int count)
    {
        if (_countDisplayTMP == null)
        {
            Debug.LogError($"{typeof(ResultUIJudgeCountView)}:[_countDisplayTMP]がアタッチされていません。");
            return;
        }

        _countDisplayTMP.text = count.ToString();
    }
}
