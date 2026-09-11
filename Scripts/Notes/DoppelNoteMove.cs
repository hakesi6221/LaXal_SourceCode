using Cysharp.Threading.Tasks;
using LitMotion;
using NaughtyAttributes;
using UnityEngine;

public class DoppelNoteMove : NoteMove
{
    [SerializeField, Label("出現するローカルY座標")]
    private float _appearLocalY = 60f;

    [SerializeField, Label("拡縮アニメの所要時間"), BoxGroup("ScaleAnimation")]
    private float _scaleAnimDurSec = 0.2f;

    [SerializeField, Label("拡縮アニメのEase"), BoxGroup("ScaleAnimation")]
    private Ease _scaleAnimEase = Ease.OutBounce;


    private ScaleAnimation_Zoom<Transform> _scaleAnim;

    // 出現済みか
    private bool _hasAppeared = false;

    protected override void UpdatePosition()
    {
        if (_generatePos == null || BeatManager.Instance == null)
        {
            return;
        }

        float songTime = (float)BeatManager.Instance.elapsed;
        float distance = (_hitTime - songTime) * _noteSpeed;
        if (distance <= 0f && !_judged)
        {
            _judged = true;
            GameManager.Instance.JudgeOnNotes(_noteIndex, _laneIndex);
        }
        _view?.UpdateSpriteAlpha(distance);

        if (distance <= _appearLocalY)
        {
            Appear();
            Vector3 localPosition = transform.localPosition;
            localPosition.y = distance;
            transform.localPosition = localPosition;
        }
    }

    private void Appear()
    {
        if (_hasAppeared) return;

        Debug.Log("分身ノーツ：出現");
        _hasAppeared = true;
        _view?.SetColorAlpha(1f);
        _scaleAnim?.ZoomAnim_Single(this.GetCancellationTokenOnDestroy(), 1).Forget();
    }

    public override void Initialize(Transform generatePos, float hitTime, float noteSpeed, Vector3 noteScaleMult, int laneIndex, int noteIndex, float visuableDistance)
    {
        base.Initialize(generatePos, hitTime, noteSpeed, noteScaleMult, laneIndex, noteIndex, visuableDistance);
        base.UpdatePosition();
        _scaleAnim = new ScaleAnimation_Zoom<Transform>
        (
            transform,
            0.5f,
            1f,
            _scaleAnimDurSec,
            _scaleAnimEase
        );
        _view?.SetColorAlpha(0f);
    }
}
