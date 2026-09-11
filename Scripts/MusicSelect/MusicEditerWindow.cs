using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using NaughtyAttributes;
using UnityEngine;

public class MusicEditerWindow : ArrangedUIElementBase
{
    [SerializeField, Label("CanvasGroup")]
    private CanvasGroup _canvasGroup = null;

    [SerializeField, Label("左右の移動案内アイコン")]
    private GameObject _moveGuideIcon = null;

    protected override void OnUpdateState(bool isSelected)
    {
        if (_canvasGroup == null)
        {
            Debug.LogError($"{this.name}:[_canvasGroup]がアタッチされていません。");
            return;
        }

        _canvasGroup.interactable = isSelected;
        _moveGuideIcon?.SetActive(isSelected);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fadeDur"></param>
    /// <returns></returns>
    public async UniTask FadeInAnim(float fadeDur)
    {
        if (_canvasGroup == null)
        {
            Debug.LogError($"{this.name}:[_canvasGroup]がアタッチされていません。");
            return;
        }
        try
        {
            await LMotion.Create(0f, 1.0f, fadeDur)
                        .BindToAlpha(_canvasGroup)
                        .ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("非同期処理をキャンセル");
            return;
        }
    }
    public async UniTask FadeOutAnim(float fadeDur)
    {
        if (_canvasGroup == null)
        {
            Debug.LogError($"{this.name}:[_canvasGroup]がアタッチされていません。");
            return;
        }
        try
        {
            await LMotion.Create(1.0f, 0f, fadeDur)
                        .BindToAlpha(_canvasGroup)
                        .ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("非同期処理をキャンセル");
            return;
        }
    }

    public override void OnUpdateGrabState(bool isGrabed)
    {
        // throw new NotImplementedException();
    }
}
