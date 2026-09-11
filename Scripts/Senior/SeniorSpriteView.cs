using NaughtyAttributes;
using UnityEngine;

public class SeniorSpriteView : MonoBehaviour
{
    [SerializeField, Label("SpriteRenderer参照")]
    private SpriteRenderer _renderer = null;

    public void Initialize()
    {
        if (_renderer == null)
        {
            Debug.LogError($"{this.name}:[_renderer]がアタッチされていません。");
            return;
        }
        Sprite seniorSprite = RhythmGameInfomation.FeatureCharaAssets.StandingOri;
        _renderer.sprite = seniorSprite;
    }
}
