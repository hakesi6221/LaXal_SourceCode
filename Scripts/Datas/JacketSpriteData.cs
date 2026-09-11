using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
[CreateAssetMenu(fileName = "JacketSpriteData", menuName = "ScriptableObjects/JacketSpriteData")]
public class JacketSpriteData : ScriptableObject
{
    [SerializeField, Header("ジャケットの素材")]
    private Sprite _jacketSprite = null;

    [SerializeField, Header("ゲーム内配置時の幅")]
    private float _width = 0f;

    [SerializeField, Header("ゲーム内配置時の高さ")]
    private float _height = 0f;

    /// <summary>
    /// ジャケットの素材
    /// </summary>
    public Sprite Sprite => _jacketSprite;

    /// <summary>
    /// ゲーム内配置時の幅
    /// </summary>
    public float Width => _width;

    /// <summary>
    /// ゲーム内配置時の高さ
    /// </summary>
    public float Height => _height;

    /// <summary>
    /// ゲーム内配置時の縦横の大きさ
    /// </summary>
    public Vector2 SizeDelta => new Vector2(_width, _height);

    /// <summary>
    /// 引数のImageコンポーネントの各情報を、このデータを同じものにする関数
    /// </summary>
    /// <param name="image"></param>
    public void SetDataToImageComp(Image image)
    {
        if (image == null)
        {
            Debug.LogError($"{typeof(JacketSpriteData).Name}:ジャケットデータのImageへの登録の際にImageが正しくない形で渡されました");
            return;
        }

        var sizeDelta = new Vector2(_width, _height);
        image.sprite = _jacketSprite;
        image.rectTransform.sizeDelta = sizeDelta;
    }
}
