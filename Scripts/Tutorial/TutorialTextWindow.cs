using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

public class TutorialTextWindow : MonoBehaviour
{
    [SerializeField, Label("チュートリアル説明表示TMP")]
    private TextMeshProUGUI _explainTMP = null;

    [SerializeField, Label("CanvasGroup")]
    private CanvasGroup _canvasGroup = null;

    private FadeAnimationCanvasGroup _fadeAnim = new FadeAnimationCanvasGroup();

    public async void OnStartTutorial(bool tutorial)
    {
        if (!tutorial) return;
        if (_canvasGroup == null) return;

        _fadeAnim.PlayAnim
        (
            _canvasGroup,
            1f,
            0.5f,
            0,
            LitMotion.Ease.InSine,
            this.GetCancellationTokenOnDestroy()
        ).Forget();
    }

    public void SetText(string text)
    {
        if (_explainTMP == null) return;

        _explainTMP.text = text;
    }
}
