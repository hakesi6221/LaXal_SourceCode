using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class OnButtonClickAdministrator : MonoBehaviour
{
    [SerializeField, Label("戻るボタン")]
    private Button _returnButton = null;

    public void SetReturnProcess(Action returnProcess)
    {
        if (_returnButton == null)
        {
            Debug.LogError($"{this.name}:[_returnButton]がアタッチされていません。");
            return;
        }

        _returnButton.onClick.RemoveAllListeners();
        _returnButton.onClick.AddListener(() => returnProcess?.Invoke());
    }
}
