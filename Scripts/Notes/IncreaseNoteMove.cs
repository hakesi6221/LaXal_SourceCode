using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using NaughtyAttributes;
using UnityEngine;

public class IncreaseNoteMove : NoteMove
{
    [SerializeField, Label("分裂するローカルY座標")]
    private float _increaseLocalY = 60f;

    [SerializeField, Label("拡縮アニメの所要時間"), BoxGroup("ScaleAnimation")]
    private float _scaleAnimDurSec = 0.2f;

    [SerializeField, Label("拡縮アニメのEase"), BoxGroup("ScaleAnimation")]
    private Ease _scaleAnimEase = Ease.OutBounce;

    // 消滅済みか
    private bool _hasDisappeared = false;

    private ScaleAnimation_Zoom<Transform> _scaleAnim;

    protected override void UpdatePosition()
    {
        if (_generatePos == null || BeatManager.Instance == null)
        {
            return;
        }

        float songTime = (float)BeatManager.Instance.elapsed;
        float distance = (_hitTime - songTime) * _noteSpeed;
        _view?.UpdateSpriteAlpha(distance);

        if (distance > _increaseLocalY)
        {
            Vector3 localPosition = transform.localPosition;
            localPosition.y = distance;
            transform.localPosition = localPosition;
        }
        else
        {
            Disappear();
        }
    }

    private async void Disappear()
    {
        if (_hasDisappeared) return;

        _hasDisappeared = true;
        try
        {
            await _scaleAnim.ZoomAnim_Single(this.GetCancellationTokenOnDestroy(), 1);
            gameObject.SetActive(false);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning($"{this.name}:非同期処理のキャンセル");
            return;
        }
    }

    public override void Initialize(Transform generatePos, float hitTime, float noteSpeed, Vector3 noteScaleMult, int laneIndex, int noteIndex, float visuableDistance)
    {
        base.Initialize(generatePos, hitTime, noteSpeed, noteScaleMult, laneIndex, noteIndex, visuableDistance);
        base.UpdatePosition();
        _scaleAnim = new ScaleAnimation_Zoom<Transform>
        (
            transform,
            1.0f,
            0f,
            _scaleAnimDurSec,
            _scaleAnimEase
        );
    }
}
