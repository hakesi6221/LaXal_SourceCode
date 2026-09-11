using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// XR Interaction Toolkitを使用している際、エディタ上で発生するエラーを回避するためのクラス。
/// </summary>
public class XR_EditorErrorsAvoider : MonoBehaviour
{
    [SerializeField, Header("LineVisualオブジェクト")]
    private GameObject[] _lineVisuals = null;

    [SerializeField, Header("NotificationListenerオブジェクト")]
    private GameObject _notificationListener = null;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {

#if UNITY_EDITOR
        if (_notificationListener == null)
        {
            Debug.LogError($"{typeof(XR_EditorErrorsAvoider).Name}：[_notificationListener]がアタッチされていません。");
        }
        else
        {
            _notificationListener.SetActive(false);
        }
#endif
        SetLineVisualActive(false);

        await UniTask.Yield();
        await UniTask.Yield();

        SetLineVisualActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetLineVisualActive(bool isActive)
    {
        if (_lineVisuals == null || _lineVisuals.Length == 0)
        {
            Debug.LogError($"{typeof(XR_EditorErrorsAvoider).Name}：[_lineVisuals]がアタッチされていません。");
            return;
        }
        foreach (var lineVisual in _lineVisuals)
        {
            lineVisual.SetActive(isActive);
        }
    }
}
