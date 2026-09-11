using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public class Button_RestartSceneOnPush : MonoBehaviour
{
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
        SceneTransitionManager.Instance.ChangeSceneAsync(SceneManager.GetActiveScene().name).Forget();
    }
}
