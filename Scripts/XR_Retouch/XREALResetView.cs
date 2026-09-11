using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class XREALResetView : MonoBehaviour
{
    [SerializeField, Label("対応する入力")]
    private InputActionProperty _action;

    private bool _processing = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _action.action.performed += ResetViewOfAngle;
    }

    private async void ResetViewOfAngle(InputAction.CallbackContext context)
    {
        if (!(context.interaction is PressInteraction)) return;
        // if (!context.performed) return;
        if (_processing) return;

        _processing = true;
        await DoFChanger.ResetAngleOfView(this.GetCancellationTokenOnDestroy());
        _processing = false;
    }

    void OnDestroy()
    {
        _action.action.performed -= ResetViewOfAngle;
    }
}
