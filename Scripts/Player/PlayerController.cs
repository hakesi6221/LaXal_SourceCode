using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// プレイヤーの操作による入力を受けて、プレイヤーキャラクターの動作を起こす部分を担当するクラス
/// タップかフリック化の判定をし、それに応じた処理をPresenterを通して呼ぶ処理
/// </summary>
[RequireComponent(typeof(PlayerObj))]
public class PlayerController : MonoBehaviour
{
    [SerializeField, Label("フリック入力に移行するタップ時間の閾値"), BoxGroup("Input Parameters")]
    private float _inputDurThresholdToFlic = 0.3f;

    [SerializeField, Label("フリック入力に移行するタップ距離の閾値"), BoxGroup("Input Parameters")]
    private float _inputDisThresholdToFlic = 0.2f;

    private PlayerObj _presenter = null;

    [SerializeField, Foldout("Debug")]
    private Vector2 _startedTapPos = Vector3.zero;

    [SerializeField, Foldout("Debug")]
    private Vector2 _performedTapPos = Vector3.zero;

    [SerializeField, Foldout("Debug")]
    private bool _isDrag = false;

    [SerializeField, Foldout("Debug")]
    private float _tapCountSecToFlic = 0.0f;

    private CancellationTokenSource _ctsToFlic = new CancellationTokenSource();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _presenter = GetComponent<PlayerObj>();
    }

    // Update is called once per frame
    void Update()
    {
        // InputDevice
    }

    public void OnTouchPad(InputAction.CallbackContext context)
    {
        Vector2 input = Vector2.zero;
        if (context.action.type == InputActionType.Value)
        {
            input = context.ReadValue<Vector2>();
        }
        // Debug.Log($"x={input.x}, y={input.y}");
        if (context.started)
        {
            if ( context.action.type == InputActionType.Button) return;
            // Debug.Log("フリック開始");
            _isDrag = true;
            _startedTapPos = input;

            CancellationTokenSource flicNewCTS = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy(), _ctsToFlic.Token);
            CountOnTapToFlic(flicNewCTS.Token);
        }
        else if (context.performed)
        {
            if ( context.action.type == InputActionType.Button) return;

            _performedTapPos = input;
        }
        else if (context.canceled)
        {
            if (context.action.type == InputActionType.Value)
            {
                _ctsToFlic.Cancel();
                _ctsToFlic = new CancellationTokenSource();
                float inputGap = _performedTapPos.x - _startedTapPos.x;
                if (_tapCountSecToFlic <= _inputDurThresholdToFlic)
                {
                    if (_inputDisThresholdToFlic <= Mathf.Abs(inputGap))
                    {
                        if (inputGap < 0f)
                        {
                            // Debug.Log($"フリック判定：左 value={inputGap}");
                            _presenter?.MoveToLeftLane();
                        }
                        else if (0f < inputGap)
                        {
                            // Debug.Log($"フリック判定：右 value={inputGap}");
                            _presenter?.MoveToRightLane();
                        }
                    }
                    else
                    {
                        // Debug.Log("タップ判定：Move");
                        _presenter.AppealOnLane();
                    }
                }
                _isDrag = false;
            }
            else if (context.action.type == InputActionType.Button)
            {
                if (_isDrag) return;

                // Debug.Log("タップ判定：Tap");
                _presenter.AppealOnLane();
            }
        }
    }

    private async void CountOnTapToFlic(CancellationToken cancellationToken)
    {
        _tapCountSecToFlic = 0.0f;

        while (!cancellationToken.IsCancellationRequested)
        {
            _tapCountSecToFlic += Time.deltaTime;
            try
            {
                await UniTask.Yield(cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Debug.Log("フリック入力用の秒数カウントがキャンセルされました");
                break;
            }
        }
    }

    /// <summary>
    /// PCテスト用
    /// </summary>
    /// <param name="context"></param>
    public void OnMoveRight_PCTest(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        _presenter?.MoveToRightLane();
    }

    /// <summary>
    /// PCテスト用
    /// </summary>
    /// <param name="context"></param>
    public void OnMoveLeft_PCTest(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        _presenter?.MoveToLeftLane();
    }

    public void OnAppeal_PCTest(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        _presenter?.AppealOnLane();
    }
}
