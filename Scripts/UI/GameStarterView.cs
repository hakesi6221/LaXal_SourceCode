using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class GameStarterView : MonoBehaviour
{
    [SerializeField, Label("ジャケット表示Image"), BoxGroup("UIElements")]
    private Image _jacketImage = null;

    [SerializeField, Label("難易度表示TMP"), BoxGroup("UIElements")]
    private TextMeshProUGUI _diffucultyText = null;

    [SerializeField, Label("曲名表示TMP"), BoxGroup("UIElements")]
    private TextMeshProUGUI _musicNameText = null;

    /// <summary>
    /// 開始時表示UIに選択された楽曲の情報を反映させる関数
    /// </summary>
    /// <param name="jacket">ジャケットデーター</param>
    /// <param name="diff">難易度</param>
    /// <param name="musicName">楽曲名</param>
    public void DisplayMusicInfos(JacketSpriteData jacket, MusicDifficulty diff, string musicName)
    {
        if (_jacketImage == null || _diffucultyText == null || _musicNameText == null)
        {
            Debug.LogError($"{this.name}：ジャケット画像、難易度表示、曲名表示のいずれかがアタッチされていません。Inspectorを確認してください");
            return;
        }

        if (jacket == null)
        {
            Debug.LogError($"{this.name}：ジャケット画像が正しく設定されていません。");
            return;
        }
        if (diff == MusicDifficulty.None)
        {
            Debug.LogError($"{this.name}：難易度が正しく設定されていません。");
            return;
        }
        if (string.IsNullOrEmpty(musicName))
        {
            Debug.LogError($"{this.name}：曲名が正しく設定されていません。");
            return;
        }

        _jacketImage.sprite = jacket.Sprite;
        _jacketImage.rectTransform.sizeDelta = jacket.SizeDelta;
        _diffucultyText.text = diff.ToString();
        _musicNameText.text = musicName;
    }
}
