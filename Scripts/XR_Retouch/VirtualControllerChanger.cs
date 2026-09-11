using System;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VirtualControllerChanger : MonoBehaviour
{
    [Serializable]
    private struct SceneNameFeatureText
    {
        public string SceneName;
        public string DisplayText;
    }

    [SerializeField, Label("汎用ボタン")]
    private GameObject _commonButton = null;

    [SerializeField, Label("アピールボタン")]
    private GameObject _appualButton = null;

    [SerializeField, Label("ハンドモードボタン")]
    private GameObject _handModeButton = null;

    [SerializeField, Label("シーン名表示テキスト")]
    private TextMeshProUGUI _sceneNameTMP = null;

    [SerializeField, Label("シーン名と表示文字の紐づけ")]
    private SceneNameFeatureText[] _featureTexts = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SceneManager.activeSceneChanged += OnSceneChange;
    }

    private void OnSceneChange(Scene previousScene, Scene nextScene)
    {
        if (_commonButton == null
        || _handModeButton == null
        || _appualButton == null
        || _sceneNameTMP == null)
        {
            Debug.LogError($"{this.name}:必要な参照が足りていません");
            return;
        }
        string nextSceneName = nextScene.name;
        string featureText = GetFeatureText(nextSceneName);

        _sceneNameTMP.text = featureText;
        switch (nextSceneName)
        {
            case "MusicSelectScene":
                _commonButton.SetActive(true);
                _appualButton.SetActive(false);
                _handModeButton.SetActive(true);
                break;
            case "MainGameScene_4Lane":
                _commonButton.SetActive(false);
                _appualButton.SetActive(true);
                _handModeButton.SetActive(true);
                break;
            default:
                _commonButton.SetActive(true);
                _appualButton.SetActive(false);
                _handModeButton.SetActive(false);
                break;
        }
    }

    private string GetFeatureText(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return string.Empty;
        if (_featureTexts == null) return string.Empty;

        foreach (var texts in _featureTexts)
        {
            if (texts.SceneName == sceneName)
                return texts.DisplayText;
        }

        return sceneName;
    }
}
