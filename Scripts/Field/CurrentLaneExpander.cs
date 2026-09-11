using NaughtyAttributes;
using UnityEngine;
using LitMotion;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

/// <summary>
/// 現在いるレーンを示すオブジェクトの拡大縮小アニメーションを担当するクラス
/// レーンが変更されたときのコールバック関数としてgameManagerに登録し、自動でアニメーションを実行する
/// </summary>
public class CurrentLaneExpander : MonoBehaviour, ILaneChangeEvent
{
    [SerializeField, Label("現在のレーンを示すオブジェクト")]
    private SpriteRenderer _currentLaneRenderer = null;

    [SerializeField, Label("デフォルトのheight")]
    private float _defaultHeight = 1.0f;

    [SerializeField, Label("拡大後のheight")]
    private float _expandedHeight = 60.0f;

    [SerializeField, Label("拡大/縮小にかかる時間")]
    private float _expandDuration = 0.2f;

    [SerializeField, Label("拡大アニメーションのイージング")]
    private Ease _easingTypeExpand = Ease.InQuad;

    [SerializeField, Label("縮小アニメーションのイージング")]
    private Ease _easingTypeContract = Ease.InQuad;

    // 直前のレーン番号
    private int _prevLaneIndex = -1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ExpandCurrentLane(this.GetCancellationTokenOnDestroy(), GameManager.Instance.CurrentLaneIndex).Forget();
        GameManager.Instance?.AddLaneChangeEvent(OnLaneChange);
    }

    /// <summary>
    /// 現在のレーンを示すオブジェクトを拡大するアニメーションを実行する関数
    /// </summary>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <param name="laneIndex">レーンのインデックス</param>
    /// <returns></returns>
    private async UniTask ExpandCurrentLane(CancellationToken cancellationToken, int laneIndex = -1)
    {
        if (_currentLaneRenderer == null)
        {
            Debug.LogError("CurrentLaneExpander：_currentLaneRendererがアタッチされていません。");
            return;
        }
        if (laneIndex != -1)
        {
            // レーンの位置にオブジェクトを移動
            var lanePosition = GameManager.Instance.GetTargetLanePosition(laneIndex);
            _currentLaneRenderer.transform.position = new Vector3(lanePosition.x, _currentLaneRenderer.transform.position.y, _currentLaneRenderer.transform.position.z);
        }

        // 縮小アニメーション
        try
        {
            await LMotion.Create(_defaultHeight, _expandedHeight, _expandDuration)
                        .WithEase(_easingTypeExpand)
                        .Bind(value => _currentLaneRenderer.size = new Vector2(_currentLaneRenderer.size.x, value))
                        .ToUniTask(cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("CurrentLaneExpander：アニメーションがキャンセルされました。");
            return;
        }
        _currentLaneRenderer.size = new Vector2(_currentLaneRenderer.size.x, _expandedHeight);
    }

    /// <summary>
    /// 現在のレーンを示すオブジェクトを縮小するアニメーションを実行する関数
    /// </summary>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <param name="laneIndex">レーンのインデックス</param>
    /// <returns></returns>
    private async UniTask ContractCurrentLane(CancellationToken cancellationToken, int laneIndex = -1)
    {
        if (_currentLaneRenderer == null)
        {
            Debug.LogError("CurrentLaneExpander：_currentLaneRendererがアタッチされていません。");
            return;
        }
        if (laneIndex != -1)
        {
            // レーンの位置にオブジェクトを移動
            var lanePosition = GameManager.Instance.GetTargetLanePosition(laneIndex);
            _currentLaneRenderer.transform.position = new Vector3(lanePosition.x, _currentLaneRenderer.transform.position.y, _currentLaneRenderer.transform.position.z);
        }

        try
        {
            await LMotion.Create(_expandedHeight, _defaultHeight, _expandDuration)
                        .WithEase(_easingTypeContract)
                        .Bind(value => _currentLaneRenderer.size = new Vector2(_currentLaneRenderer.size.x, value))
                        .ToUniTask(cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("CurrentLaneExpander：アニメーションがキャンセルされました。");
            return;
        }
        _currentLaneRenderer.size = new Vector2(_currentLaneRenderer.size.x, _defaultHeight);
    }

    /// <summary>
    /// レーンが変更されたときに呼んでほしいコールバック関数
    /// </summary>
    /// <param name="prevLaneIndex">レーン番号</param>
    public async void OnLaneChange(int prevLaneIndex, int nextLaneIndex)
    {
        if (_currentLaneRenderer == null)
        {
            Debug.LogError("CurrentLaneExpander：_currentLaneRendererがアタッチされていません。");
            return;
        }
        if (_prevLaneIndex == nextLaneIndex)
            return;

        _prevLaneIndex = nextLaneIndex;
        var token = this.GetCancellationTokenOnDestroy();

        // レーンの位置にオブジェクトを移動
        var lanePosition = GameManager.Instance.GetTargetLanePosition(nextLaneIndex);
        _currentLaneRenderer.transform.position = new Vector3(lanePosition.x, _currentLaneRenderer.transform.position.y, _currentLaneRenderer.transform.position.z);
        // await ContractCurrentLane(token);
        await ExpandCurrentLane(token);
    }
}
