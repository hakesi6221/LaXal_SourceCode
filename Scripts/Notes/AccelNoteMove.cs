using UnityEngine;
using NaughtyAttributes;
using LitMotion;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using LitMotion.Extensions;

public class AccelNoteMove : NoteMove
{
    [SerializeField, Label("ノーツが生成されるローカルY座標")]
    private float _spawnLocalY = 60f;

    [SerializeField, Label("変更後の色")]
    private Color _toColor = Color.white;

    [SerializeField, Label("急加速時の判定ラインまでの移動時間")]
    private float _accelMoveTime = 0.1f;

    [SerializeField, Label("急加速時のイージング")]
    private Ease _accelEase = Ease.OutExpo;

    [SerializeField, Label("拡縮アニメの倍率"), BoxGroup("ScaleAnimation")]
    private float _scaleAnimMult = 1.2f;

    [SerializeField, Label("拡縮アニメの所要時間"), BoxGroup("ScaleAnimation")]
    private float _scaleAnimDurSec = 0.2f;
    private float _spawnElapsed = 0f;
    private float _changeColoElapsed = 0f;
    private float _accelElapsed = 0f;

    private ScaleAnimation_Zoom<Transform> _scaleAnim;

    private bool _hasAcceled = false;

    protected override void UpdatePosition()
    {
        if (_hasAcceled)
            base.UpdatePosition();
    }

    private async UniTask WaitForElapsed(float elapsed, CancellationToken token)
    {
        try
        {
            await UniTask.WaitUntil
            (
                () => elapsed <= BeatManager.Instance.elapsed,
                cancellationToken: token
            );
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("非同期処理のキャンセル");
            return;
        }
    }

    private async void AccelAsync()
    {
        try
        {
            await WaitForElapsed(_accelElapsed, this.GetCancellationTokenOnDestroy());
            await LMotion.Create(transform.localPosition.y, 0f, _accelMoveTime)
                        .WithEase(_accelEase)
                        .BindToLocalPositionY(transform)
                        .ToUniTask(this.GetCancellationTokenOnDestroy());
            _hasAcceled = true;
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("非同期処理のキャンセル");
            return;
        }
    }

    private async void ChangeColorAsync()
    {
        try
        {
            await WaitForElapsed(_changeColoElapsed, this.GetCancellationTokenOnDestroy());
            _scaleAnim.ZoomAnim_Single(this.GetCancellationTokenOnDestroy()).Forget();
            _view.SetColor(_toColor);
            AccelAsync();
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("非同期処理のキャンセル");
            return;
        }
    }

    private async void SpawnAsync()
    {
        try
        {
            await WaitForElapsed(_spawnElapsed, this.GetCancellationTokenOnDestroy());
            _scaleAnim.ZoomAnim_Single(this.GetCancellationTokenOnDestroy()).Forget();
            transform.localPosition = new Vector3(transform.localPosition.x, _spawnLocalY, transform.localPosition.z);
            ChangeColorAsync();
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("非同期処理のキャンセル");
            return;
        }
    }

    // ノーツが最終的に到着する点は分かっている。
    // 最初の座標を決定する
    public override void Initialize(Transform generatePos, float hitTime, float noteSpeed, Vector3 moveDirection, int laneIndex, int noteIndex, float visuableDistance)
    {
        base.Initialize(generatePos, hitTime, noteSpeed, moveDirection, laneIndex, noteIndex, visuableDistance);
        base.UpdatePosition();
        _spawnElapsed = hitTime;
        _scaleAnim = new ScaleAnimation_Zoom<Transform>(transform, 1f, _scaleAnimMult, _scaleAnimDurSec, Ease.OutBack);
        SpawnAsync();
    }

    public void SetChangeColorElapsed(float elapsed)
    {
        _changeColoElapsed = elapsed;
    }

    public void SetAccelElapsed(float elapsed)
    {
        _accelElapsed = elapsed;
    }
}
