using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class Button_LoadSceneOnPush : MonoBehaviour
{
    [SerializeField, Label("遷移先のシーン名")]
    private string _targetSceneName = string.Empty;
    private Button _button = null;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _button = GetComponent<Button>();

        if (_button == null)
        {
            Debug.LogError($"{this.name}: Button component is not found.");
            return;
        }
        _button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        SceneTransitionManager.Instance.ChangeSceneAsync(_targetSceneName).Forget();
    }
}
