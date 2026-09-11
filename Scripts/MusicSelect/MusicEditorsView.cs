using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// 曲選択時のエディットウィンドウの見た目への反映を担当するクラス
/// </summary>
public class MusicEditorsView : MonoBehaviour
{
    [SerializeField, Label("曲のジャケット表示")]
    private Image _jacketImage = null;

    [SerializeField, Label("先輩キャラの立ち絵Image")]
    private Image _seniorCharaImage = null;

    [SerializeField, Label("先輩キャラの名前Image")]
    private Image _seniorCharaNameImage = null;

    [SerializeField, Label("特殊ノーツの動画ウィンドウ")]
    private VideoPlayer _specialNoteVideoPlayer = null;

    [SerializeField, Label("ウィンドウのフェードイン時間")]
    private float _fadeDuration = 0.5f;

    /// <summary>
    /// 操作が開始されたときの処理
    /// 楽曲データを受け取り、その内容をUIに表示させる
    /// </summary>
    /// <param name="musicData">楽曲データ</param>
    public async UniTask OnStartEdit(MusicEditerWindow[] windows, MusicData musicData)
    {
        if (musicData == null)
        {
            Debug.LogError($"{this.name}：楽曲データが正しくない形で渡されています。");
            return;
        }
        if (windows == null)
        {
            Debug.LogError($"{this.name}：オブジェクトが正しくない形で渡されています。");
            return;
        }

        // 各UIの内容を更新
        ApplyInfoUIs(musicData);

        try
        {
            List<UniTask> handles = new List<UniTask>();
            foreach (var window in windows)
            {
                if (window == null) continue;

                var handle = window.FadeInAnim(_fadeDuration);
                handles.Add(handle);
            }
            await UniTask.WhenAll(handles);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("非同期処理をキャンセル");
            return;
        }
    }

    /// <summary>
    /// 各情報表示のUIの内容更新を行う
    /// 開かれるたびに呼ぶ想定
    /// </summary>
    /// <param name="musicData">楽曲データ</param>
    private void ApplyInfoUIs(MusicData musicData)
    {
        if (_jacketImage == null
            || _seniorCharaImage == null
            || _seniorCharaNameImage == null
            || _specialNoteVideoPlayer == null)
        {
            Debug.LogError($"{this.name}：情報表示のUIのいずれかの参照がありません");
            return;
        }

        musicData.Jacket.SetDataToImageComp(_jacketImage);
        var charaAsset = musicData.CharaAsset;
        if (charaAsset != null)
        {
            _seniorCharaImage.sprite = musicData.CharaAsset.StandingPro;
            _seniorCharaNameImage.sprite = musicData.CharaAsset.StreamLogo;
        }

        _specialNoteVideoPlayer.clip = musicData.MovieExNotes;
        if (musicData.MovieExNotes != null)
        {
            _specialNoteVideoPlayer.prepareCompleted += _ => _specialNoteVideoPlayer.Play();
            _specialNoteVideoPlayer.Prepare();
        }
    }

    public async UniTask OnClose(MusicEditerWindow[] windows)
    {
        if (windows == null)
        {
            Debug.LogError($"{this.name}：オブジェクトが正しくない形で渡されています。");
            return;
        }

        try
        {
            List<UniTask> handles = new List<UniTask>();
            foreach (var window in windows)
            {
                if (window == null) continue;

                var handle = window.FadeOutAnim(_fadeDuration);
                handles.Add(handle);
            }
            await UniTask.WhenAll(handles);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("非同期処理をキャンセル");
            return;
        }
    }

    /// <summary>
    /// 難易度が変更されたときに呼ばれる処理
    /// 各ボタンの見た目の状態を、選択状態化に応じて更新する
    /// </summary>
    /// <param name="buttons">難易度ボタン</param>
    /// <param name="targetDifficulty">選択されたボタン</param>
    public void OnSelectedDifficulty(DifficultyEditButton[] buttons, MusicDifficulty targetDifficulty)
    {
        foreach (DifficultyEditButton button in buttons)
        {
            button.ToggleSelected(button.Difficulty == targetDifficulty);
        }
    }
}
