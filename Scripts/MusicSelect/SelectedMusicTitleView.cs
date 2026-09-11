using NaughtyAttributes;
using TMPro;
using UnityEngine;

public class SelectedMusicTitleView : MonoBehaviour
{
    [SerializeField, Label("楽曲ディスプレイスクロール管理")]
    private MusicDisplaysScroller _displaysScroller = null;

    [SerializeField, Label("楽曲タイトル表示UI")]
    private TextMeshProUGUI _musicTitleTMP = null;

    public void UpdateMusicTitleUI(MusicDisplayObj musicDisplay)
    {
        if (_musicTitleTMP == null)
        {
            Debug.LogError($"{this.name}:[_musicTitleTMP]がアタッチされていません。");
            return;
        }

        string musicTitle = string.Empty;
        if (musicDisplay != null)
        {
            MusicData musicData = musicDisplay.MusicData;
            musicTitle = musicData.Name;
        }
        _musicTitleTMP.text = musicTitle;
    }
}
