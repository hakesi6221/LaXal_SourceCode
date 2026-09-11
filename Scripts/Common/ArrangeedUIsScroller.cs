using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;


public abstract class ArrangeedUIsScroller<T> : MonoBehaviour where T : ArrangedUIElementBase
{
    [SerializeField, Label("親のTransform"), BoxGroup("UIの配置設定")]
    private Transform _displaysRoot = null;

    [SerializeField, Label("直径"), BoxGroup("UIの配置設定")]
    private float _diameter = 6;

    [SerializeField, Label("角の広さ"), BoxGroup("UIの配置設定")]
    private float _range = 360f;

    [SerializeField, Label("初期正面インデックス"), BoxGroup("UIの配置設定")]
    private int _forwardIndex = 0;

    [SerializeField, Label("X軸の傾き"), BoxGroup("UIの配置設定")]
    private float _slopeX;

    [SerializeField, Label("z軸の傾き"), BoxGroup("UIの配置設定")]
    private float _slopeZ;

    [SerializeField, Label("スクロール速度倍率"), BoxGroup("スクロール設定")]
    private float _scrollSpeedMult = 5.0f;

    [SerializeField, Label("スクロール減速度"), BoxGroup("スクロール設定")]
    private float _scrollDeceleration = 5.0f;

    [SerializeField, Label("正面に戻る時間"), BoxGroup("スクロール設定")]
    private float _lookAtForwardDur = 0.5f;

    [SerializeField, Label("デバイス回転入力プロパティ"), BoxGroup("スクロール設定")]
    private InputActionProperty _action;

    private Vector3 _forwardVector = Vector3.zero;

    // UIを並べるクラス
    private ArrangeObjectToPolygonShape<T> _arrangeObject;

    // いずれかのUIをつかまれているか
    private bool _isGrabed = false;

    // 直前のデバイスの角度
    private Vector3 _prevEulerAngles = Vector3.zero;

    // 現在のスクロール速度(Y回転)
    private float _scrollVelocity = 0.0f;

    // 子にあるUIのオブジェクトの配列
    protected T[] _children = null;

    // ディスプレイから手を離したときの非同期処理のキャンセルトークンソース
    private CancellationTokenSource _scrollCts = new CancellationTokenSource();

    // ディスプレイクリック時の処理用のCTS
    private CancellationTokenSource _onClickCts = new CancellationTokenSource();

    // 動作中かどうか
    private bool _active = false;

    /// <summary>
    /// 動作中かどうか
    /// </summary>
    public bool Active => _active;

    /// <summary>
    /// 子にあるUIのオブジェクトの配列
    /// </summary>
    public IReadOnlyCollection<T> Children => _children;

    /// <summary>
    /// UIの選択状態が更新されたときの処理
    /// </summary>
    /// <param name="ui">選択されたUI</param>
    protected virtual void OnUpdateSelectedUI(T ui)
    {

    }

    /// <summary>
    /// いずれかのUIでクリック判定が起きた時の処理
    /// </summary>
    /// <param name="ui">クリックされたUI</param>
    protected abstract void OnClickUI(T ui);

    /// <summary>
    /// UIの選択状態の更新
    /// </summary>
    /// <param name="ui">選択されたUI</param>
    private void UpdateSelectedUI(T ui)
    {
        foreach (var child in _children)
        {
            if (child == null) continue;

            if (ui == null)
                child.UpdateState(false);
            else
                child.UpdateState(child == ui);
        }

        OnUpdateSelectedUI(ui);
    }

    // Update is called once per frame
    void Update()
    {
        if (!_active) return;
        // DebugDisplaySpeed();
        CalcAngulerVelocity();
        Rotation();
    }

    /// <summary>
    /// 初期化処理
    /// </summary>
    public void Initialize(Vector3 forwardVector)
    {
        _arrangeObject = new ArrangeObjectToPolygonShape<T>
        (
            _displaysRoot,
            _diameter,
            _range,
            _forwardIndex,
            _slopeX,
            _slopeZ
        );
        _forwardVector = forwardVector;
        _children = _arrangeObject.ArrangeObjects();
        SetFuncToDisplays(_children);
    }

    /// <summary>
    /// 動作開始処理
    /// </summary>
    public void Activate(bool resetSelectedUI = true)
    {
        // 先頭のUIを選択状態としておく
        if (resetSelectedUI)
            UpdateSelectedUI(_children[_forwardIndex]);
        // 動作開始
        _active = true;
    }

    /// <summary>
    /// 動作停止処理
    /// </summary>
    public void Deactivate()
    {
        // 動作停止
        _active = false;
    }

    /// <summary>
    /// スクロールのリセットを行う
    /// </summary>
    public void ResetScroll()
    {
        if (_children == null) return;
        if (_children.Length <= _forwardIndex) return;
        if (_children[_forwardIndex] == null) return;

        LookAtTargetDisplay(_children[_forwardIndex]);
    }

    /// <summary>
    /// デバイスの角度を取得し、角速度を計測する関数
    /// Updateで呼ぶ想定
    /// </summary>
    public void CalcAngulerVelocity()
    {

        var rotation = _action.action.ReadValue<Quaternion>();
        var eulerAngles = rotation.eulerAngles;
        if (eulerAngles.y > 180f)
            eulerAngles.y -= 360f;
        if (_isGrabed)
            _scrollVelocity = Mathf.DeltaAngle(_prevEulerAngles.y, eulerAngles.y);
        _prevEulerAngles = eulerAngles;
        Debug.Log(_scrollVelocity);
    }

    /// <summary>
    /// 角速度をもとに、ディスプレイの親オブジェクトを回転させる関数
    /// Updateで呼ぶ関数
    /// </summary>
    private void Rotation()
    {

        if (_displaysRoot == null)
        {
            Debug.LogError($"{typeof(ArrangeedUIsScroller<T>).Name}：[_displaysRoot]がアタッチされていません。Inspectorを確認してください");
            return;
        }
        if (!_isGrabed)
        {
            _scrollVelocity = Mathf.Lerp(_scrollVelocity, 0f, _scrollDeceleration * Time.deltaTime);
            if (_scrollVelocity != 0f && Mathf.Abs(_scrollVelocity) < 0.1f)
                _scrollVelocity = 0f;
        }
        _displaysRoot.transform.localEulerAngles += new Vector3(0f, _scrollSpeedMult * _scrollVelocity, 0f) * Time.deltaTime;
    }

    public async UniTask RotateToNext(bool right, CancellationToken cancellationToken)
    {
        if (!_active) return;
        _scrollCts.Cancel();
        _scrollCts = new CancellationTokenSource();
        var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken,
                                                                            _scrollCts.Token);
        var token = linkedSource.Token;
        float angle = _arrangeObject.CenterPartAngle * (right ? -1.0f : 1.0f);

        _isGrabed = false;
        GetClosestToFrontDisplay()?.OnUpdateGrabState(false);
        _isGrabed = true;
        GetClosestToFrontDisplay()?.OnUpdateGrabState(true);
        await LMotion.Create(_displaysRoot.localEulerAngles.y, _displaysRoot.localEulerAngles.y + angle, _lookAtForwardDur)
                    .WithEase(Ease.OutExpo)
                    .BindToLocalEulerAnglesY(_displaysRoot.transform)
                    .ToUniTask(cancellationToken:cancellationToken);
        LookAtToFrontDisplay(token);
        _isGrabed = false;
        GetClosestToFrontDisplay()?.OnUpdateGrabState(false);
    }

    /// <summary>
    /// 全ての子ディスプレイオブジェクトを取得し、それらにタップ&離したときの処理を登録する関数
    /// 動作開始時に呼ぶ想定
    /// </summary>
    private void SetFuncToDisplays(T[] displays)
    {
        for (int i = 0; i < displays.Length; i++)
        {
            var child = displays[i];
            child.Initialize(OnPointerDown, OnPointerUp);
        }
    }

    /// <summary>
    /// 最も正面に近いディスプレイのTransformを取得する関数
    /// </summary>
    /// <returns></returns>
    private T GetClosestToFrontDisplay()
    {
        if (_displaysRoot == null)
        {
            Debug.LogError($"{typeof(ArrangeedUIsScroller<T>).Name}：[_displaysRoot]がアタッチされていません。Inspectorを確認してください");
            return null;
        }
        Vector3 forwardPoint = (_arrangeObject.Diameter / 2) * _forwardVector + transform.position;
        T result = null;
        foreach (var child in _children)
        {
            T current = child;
            Transform transform = current.transform;
            float thisDistance = Vector3.Distance(forwardPoint, transform.position);
            if (result == null)
            {
                result = current;
                continue;
            }

            float closestDistance = Vector3.Distance(forwardPoint, result.transform.position);
            if (thisDistance < closestDistance)
                result = current;
        }

        return result;
    }

    /// <summary>
    /// 最も正面に近いディスプレイを真正面に向かせるように親オブジェクトを回転を設定する関数
    /// </summary>
    /// <param name="targetDisplay"></param>
    private void LookAtTargetDisplay(T targetDisplay)
    {
        if (_displaysRoot == null)
        {
            Debug.LogError($"{typeof(ArrangeedUIsScroller<T>).Name}：[_displaysRoot]がアタッチされていません。Inspectorを確認してください");
            return;
        }
        if (targetDisplay == null)
        {
            Debug.LogError($"{typeof(ArrangeedUIsScroller<T>).Name}：[LookAtTargetDisplay(Transform targetDisplay)]引数のTransformが正しく渡されませんでした。");
            return;
        }
        Vector3 forward = targetDisplay.transform.localPosition.normalized;

        var fromAngle = _displaysRoot.transform.localRotation;
        var lookAngle = Quaternion.LookRotation(forward);
        lookAngle = Quaternion.Inverse(lookAngle);

        _displaysRoot.transform.localRotation = lookAngle;
    }

    /// <summary>
    /// 最も正面に近いディスプレイを真正面に向かせるように親オブジェクトを回転させる関数
    /// </summary>
    /// <param name="targetDisplay"></param>
    private async UniTask LookAtTargetDisplayWithAnim(T targetDisplay, CancellationToken cancellationToken)
    {
        if (_displaysRoot == null)
        {
            Debug.LogError($"{typeof(ArrangeedUIsScroller<T>).Name}：[_displaysRoot]がアタッチされていません。Inspectorを確認してください");
            return;
        }
        if (targetDisplay == null)
        {
            Debug.LogError($"{typeof(ArrangeedUIsScroller<T>).Name}：[LookAtTargetDisplay(Transform targetDisplay)]引数のTransformが正しく渡されませんでした。");
            return;
        }
        Vector3 forward = targetDisplay.transform.localPosition.normalized;

        var fromAngle = _displaysRoot.transform.localRotation;
        var lookAngle = Quaternion.LookRotation(forward);
        lookAngle = Quaternion.Inverse(lookAngle);
        await LMotion.Create(fromAngle, lookAngle, _lookAtForwardDur)
                    .WithEase(Ease.OutExpo)
                    .BindToLocalRotation(_displaysRoot.transform)
                    .ToUniTask(cancellationToken:cancellationToken);

        _displaysRoot.transform.localRotation = lookAngle;
    }

    /// <summary>
    /// ディスプレイを離したとき、速度が0になった後に
    /// 最も正面に近いディスプレイが正面に来るように
    /// ルートオブジェクトのローカル角を調整する
    /// </summary>
    /// <param name="cancellationToken"></param>
    private async void LookAtToFrontDisplay(CancellationToken cancellationToken)
    {
        try
        {
            await UniTask.WaitUntil(() => _scrollVelocity == 0
                                    , cancellationToken: cancellationToken);
            var closestDisplay = GetClosestToFrontDisplay();
            UpdateSelectedUI(closestDisplay);
            await LookAtTargetDisplayWithAnim(closestDisplay, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("非同期処理をキャンセル");
            return;
        }
    }
    /// <summary>
    /// ディスプレイをつかまれたときに呼ばれる関数
    /// フラグをオンにし、すでに動いている非同期処理を止める
    /// </summary>
    public async void OnPointerDown(ArrangedUIElementBase ui, CancellationToken cancellationToken)
    {
        var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken
                                                                            , _onClickCts.Token);
        var token = linkedSource.Token;
        _scrollCts.Cancel();
        _scrollCts = new CancellationTokenSource();

        try
        {
            await UniTask.WaitForSeconds(0.2f, cancellationToken: token);
            UpdateSelectedUI(null);
            _isGrabed = true;
            ui?.OnUpdateGrabState(true);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("非同期処理をキャンセル");
            return;
        }
    }

    /// <summary>
    /// ディスプレイをつかんでいる判定の間にディスプレイを離したときの処理
    /// ディスプレイの速度が0になったら、最も正面に近いディスプレイが正面に来るようにする
    /// </summary>
    private void OnLetGoWhileGrab(CancellationToken cancellationToken)
    {
        if (!_isGrabed) return;

        // ディスプレイを離し、自動で正面に向くように調整する
        _isGrabed = false;
        var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken,
                                                                            _scrollCts.Token);
        var token = linkedSource.Token;
        LookAtToFrontDisplay(token);
        foreach (var ui in _children)
        {
            ui?.OnUpdateGrabState(false);
        }
    }

    /// <summary>
    /// ディスプレイをつかんでいる判定になっていないときにディスプレイを離したときの処理
    /// 正面にいるディスプレイの曲のエディット画面に移動する
    /// </summary>
    /// <param name="ui"></param>
    private void OnLetGoNoneGrab(T ui)
    {
        if (_isGrabed) return;
        if (ui == null)
        {
            Debug.LogError($"{typeof(ArrangeedUIsScroller<T>).Name}：楽曲データが正しくない形で渡されています。");
            return;
        }
        OnClickUI(ui);
    }

    /// <summary>
    /// ディスプレイから手を離したときに呼ばれる関数
    /// フラグをオフにし、離したときの止めるための非同期処理を実行
    /// </summary>
    public void OnPointerUp(ArrangedUIElementBase ui, CancellationToken cancellationToken)
    {
        _onClickCts.Cancel();
        _onClickCts = new CancellationTokenSource();
        // ディスプレイをつかむ判定に移行しているなら
        if (_isGrabed)
        {
            // ディスプレイを離し、自動で正面に向くように調整する
            OnLetGoWhileGrab(cancellationToken);
        }
        // 移行していないなら
        else
        {
            // 曲選択後のエディット画面へ移行する
            OnLetGoNoneGrab(ui as T);
        }
    }
}
