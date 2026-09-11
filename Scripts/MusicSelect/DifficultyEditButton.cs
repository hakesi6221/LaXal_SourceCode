using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 楽曲エディット画面の何度選択ボタンのクラス
/// 情報を保持し、他クラスから参照するように使う
/// </summary>
public class DifficultyEditButton : MonoBehaviour
{
    [SerializeField, Label("Image参照")]
    private Image _buttonImage = null;

    [SerializeField, Label("EventTrigger参照")]
    private EventTrigger _buttonTrigger = null;

    [SerializeField, Label("対応する難易度")]
    private MusicDifficulty _difficulty = MusicDifficulty.None;

    [SerializeField, Label("選択中の色：テスト用"), BoxGroup("Difficulty")]
    private Color _selectedColor = Color.white;

    [SerializeField, Label("非選択中の色：テスト用"), BoxGroup("Difficulty")]
    private Color _unSelectedColor = Color.white;
    
    /// <summary>
    /// Image参照
    /// </summary>
    public Image Image => _buttonImage;

    /// <summary>
    /// EventTrigger参照
    /// </summary>
    public EventTrigger Trigger => _buttonTrigger;

    /// <summary>
    /// 対応する難易度
    /// </summary>
    public MusicDifficulty Difficulty => _difficulty;

    /// <summary>
    /// ボタンの選択状態を切り替える関数
    /// テスト的にボタン背景画像の色を切り替える
    /// </summary>
    /// <param name="isSelected"></param>
    public void ToggleSelected(bool isSelected)
    {
        if (_buttonImage == null)
        {
            Debug.LogError($"{this.name}:[_buttonImage]がアタッチされていません。");
            return;
        }

        _buttonImage.color = isSelected
                            ? _selectedColor
                            : _unSelectedColor;
    }
}
