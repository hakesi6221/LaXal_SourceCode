using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// リザルト画面UIへの見た目の反映を行うクラス
/// ジャケット画像やLive2Dキャラ、テキスト内容の反映を行う
/// </summary>
public class ResultUIsView : MonoBehaviour
{

    [System.Serializable]
    private class ResultGradeSprite
    {
        [SerializeField]
        public ResultGrade Grade = ResultGrade.None;

        [SerializeField]
        public Sprite Sprite = null;
    }

    [SerializeField, Label("曲タイトルTMP"), BoxGroup("TMP")]
    private TextMeshProUGUI _musicTitle = null;

    [SerializeField, Label("プレイ難易度TMP"), BoxGroup("TMP")]
    private TextMeshProUGUI _playDifficulty = null;

    [SerializeField, Label("曲ジャケット画像"), BoxGroup("Image")]
    private Image _jacketImage = null;

    [SerializeField, Label("最終スコアTMP"), BoxGroup("TMP")]
    private TextMeshProUGUI _finalScore = null;

    [SerializeField, Label("判定カウント表示UI"), BoxGroup("TMP")]
    private ResultUIJudgeCountView[] _judgeCountUIs = null;

    [SerializeField, Label("リザルト評価"), BoxGroup("Image")]
    private Image _resultGrade = null;

    [SerializeField, Label("操作案内テキスト")]
    private GameObject _controllText = null;

    [SerializeField, Label("リザルトランクスプライト"), BoxGroup("Assets")]
    private ResultGradeSprite[] _resultGradeSprites = null;

    [SerializeField, Label("なぎさのアセットデータ"), BoxGroup("Assets")]
    private CharacterAssetsData _nagisaAssetsData = null;

    [SerializeField, Label("キャラクター立ち絵Image"), BoxGroup("Image")]
    private Image _charaStandImage = null;

    [SerializeField, Label("キャラクターサインImage"), BoxGroup("Image")]
    private Image _charaSignImage = null;

    /// <summary>
    /// リザルト画面UIに対して、結果の情報の表示を行う関数
    /// 各情報を受け取り、各オブジェクトに対して渡す
    /// </summary>
    /// <param name="musicName">楽曲名</param>
    /// <param name="difficulty">難易度</param>
    /// <param name="jacketData">楽曲データ</param>
    /// <param name="finalScore">最終スコア</param>
    /// <param name="judgeCount">判定のカウント</param>
    /// <param name="grade">楽曲名</param>
    public void DisplayInfomations(string musicName
                                , MusicDifficulty difficulty
                                , JacketSpriteData jacketData
                                , int finalScore
                                , Dictionary<JudgeResult, int> judgeCount
                                , ResultGrade grade)
    {
        if (_musicTitle == null)
        {
            Debug.LogError($"{this.name}:[_musicTitle]がアタッチされていません。");
            return;
        }
        if (_playDifficulty == null)
        {
            Debug.LogError($"{this.name}:[_playDifficulty]がアタッチされていません。");
            return;
        }
        if (_jacketImage == null)
        {
            Debug.LogError($"{this.name}:[_jacketImage]がアタッチされていません。");
            return;
        }
        if (_finalScore == null)
        {
            Debug.LogError($"{this.name}:[_finalScore]がアタッチされていません。");
            return;
        }
        if (_judgeCountUIs == null)
        {
            Debug.LogError($"{this.name}:[_judgeCountUIs]がアタッチされていません。");
            return;
        }
        if (_resultGrade == null)
        {
            Debug.LogError($"{this.name}:[_resultGrade]がアタッチされていません。");
            return;
        }
        if (_charaStandImage == null)
        {
            Debug.LogError($"{this.name}:[_charaStandImage]がアタッチされていません。");
            return;
        }
        if (_charaSignImage == null)
        {
            Debug.LogError($"{this.name}:[_charaSignImage]がアタッチされていません。");
            return;
        }

        _musicTitle.text = musicName;
        _playDifficulty.text = difficulty.ToString();
        jacketData.SetDataToImageComp(_jacketImage);
        _finalScore.text = finalScore.ToString();
        foreach (ResultUIJudgeCountView ui in _judgeCountUIs)
        {
            if (!judgeCount.TryGetValue(ui.JudgeResult, out int count))
            {
                Debug.LogError($"{this.name}:判定結果のカウント配列が正しく初期化されていませんでした");
                return;
            }

            // TMPのテキストに反映
            ui.SetCountToText(count);
        }
        SetRandomCharaAssets(RhythmGameInfomation.FeatureCharaAssets);
        AttachResultGradeSprite(grade);
    }

    /// <summary>
    /// リザルト評価に応じたスプライトを適用する関数
    /// </summary>
    /// <param name="resultGrade"></param>
    private void AttachResultGradeSprite(ResultGrade resultGrade)
    {
        if (_resultGradeSprites == null) return;
        if (_resultGrade == null) return;

        foreach (var sprite in _resultGradeSprites)
        {
            if (sprite == null) continue;
            if (sprite.Grade == resultGrade)

            _resultGrade.sprite = sprite.Sprite;
        }
    }

    private void SetRandomCharaAssets(CharacterAssetsData charaData)
    {
        if (charaData == null) return;
        if (_nagisaAssetsData == null) return;

        int which = Random.Range(0, 100);
        CharacterAssetsData asset = which < 50 ? _nagisaAssetsData : charaData;
        _charaStandImage.sprite = asset.StandingPro;
        _charaSignImage.sprite = asset.CharaSign;
    }

    /// <summary>
    /// リザルト画面が終了するときの処理
    /// </summary>
    public void OnFinishResult()
    {
        if (_controllText == null)
        {
            Debug.LogError($"{this.name}:[_controllText]がアタッチされていません。");
            return;
        }

        _controllText.SetActive(false);
    }
}
