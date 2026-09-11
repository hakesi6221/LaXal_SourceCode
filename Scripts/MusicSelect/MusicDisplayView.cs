using UnityEngine;
using NaughtyAttributes;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using System;

/// <summary>
/// 曲選択画面で配置する曲を表示するでディスプレイの見た目を制御するクラス
/// SelectMusicDisplayPresenterで処理を呼ぶ想定
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class MusicDisplayView : MonoBehaviour
{
    [SerializeField, Label("メインサムネイルImage")]
    private Image _thumbNailImage = null;

    [SerializeField, Label("サムネイルハイライト用Image")]
    private Image _thumbNailHighLight = null;

    [SerializeField, Label("非選択状態のサムネイル色")]
    private Color _thumbNailColorUnSelect = Color.white;

    // 自身のRectTransform参照
    private RectTransform _transform = null;

    /// <summary>
    /// 初期化処理
    /// 表示したい曲のサムネイルの投影や、
    /// 選択状態の更新を行う
    /// </summary>
    /// <param name="thumbNail">投影したいサムネイル</param>
    /// <param name="isSelected">選択状態にあるかどうか</param>
    public void Initialize(JacketSpriteData thumbNail, bool isSelected)
    {
        if (_thumbNailImage == null)
        {
            Debug.LogError($"{this.name}：[_thumbNailImage]がアタッチされていません。Inspectorを確認してください");
            return;
        }
        if (_thumbNailHighLight == null)
        {
            Debug.LogError($"{this.name}：[_thumbNailHighLight]がアタッチされていません。Inspectorを確認してください");
            return;
        }
        if (thumbNail == null)
        {
            Debug.LogError($"{this.name}：サムネイルデータが正しくない形で渡されています");
            return;
        }

        _transform = GetComponent<RectTransform>();
        _transform.sizeDelta = thumbNail.SizeDelta;
        _thumbNailImage.sprite = thumbNail.Sprite;

        OnUpdateSelectState(isSelected);
    }

    /// <summary>
    /// 選択状態を更新する
    /// 選択中なら、
    /// ・ハイライトをオン
    /// ・色を通常に
    /// 非選択なら、
    /// ・ハイライトをオフ
    /// ・色を黒く
    /// </summary>
    /// <param name="isSelected"></param>
    public void OnUpdateSelectState(bool isSelected)
    {
        if (_thumbNailImage == null)
        {
            Debug.LogError($"{this.name}：[_thumbNailImage]がアタッチされていません。Inspectorを確認してください");
            return;
        }
        if (_thumbNailHighLight == null)
        {
            Debug.LogError($"{this.name}：[_thumbNailHighLight]がアタッチされていません。Inspectorを確認してください");
            return;
        }

        if (isSelected)
        {
            _thumbNailHighLight.gameObject.SetActive(true);
            _thumbNailImage.color = Color.white;
        }
        else
        {
            _thumbNailHighLight.gameObject.SetActive(false);
            _thumbNailImage.color = _thumbNailColorUnSelect;
        }
    }
    
    /// <summary>
    /// 選択する曲をクリックで確定したときの処理
    /// じぶんじしんを少し実透明にし、
    /// </summary>
    /// <param name="cg"></param>
    /// <param name="fadeDuration"></param>
    /// <returns></returns>
    public async UniTask OnDicisionMusic(CanvasGroup cg, float fadeDuration)
    {
        var token = this.GetCancellationTokenOnDestroy();
        try
        {
            await LMotion.Create(1f, 0f, fadeDuration)
                        .BindToAlpha(cg)
                        .ToUniTask(cancellationToken: token);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("非同期処理をキャンセル");
            return;
        }
    }

    
    public async UniTask OnReStartSelect(CanvasGroup cg, float fadeDuration)
    {
        var token = this.GetCancellationTokenOnDestroy();
        try
        {
            await LMotion.Create(0f, 1f, fadeDuration)
                        .BindToAlpha(cg)
                        .ToUniTask(cancellationToken: token);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("非同期処理をキャンセル");
            return;
        }
    }
}
