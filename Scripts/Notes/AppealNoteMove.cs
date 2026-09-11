using UnityEngine;

/// <summary>
/// ローカル座標でノーツを移動するクラス
/// </summary>
[RequireComponent(typeof(NoteView))]
public class AppealNoteMove : NoteMove
{
    private void Update()
    {
        UpdatePosition();
    }

    /// <summary>
    /// 曲時間から現在位置を更新
    /// </summary>
    protected override void UpdatePosition()
    {
        if (_generatePos == null || BeatManager.Instance == null)
        {
            return;
        }

        float songTime = (float)BeatManager.Instance.elapsed;

        float distance = (_hitTime - songTime) * _noteSpeed;
        _view.UpdateSpriteAlpha(distance);

        Vector3 localPosition = transform.localPosition;
        localPosition.y = distance;
        transform.localPosition = localPosition;
    }

    public override void Initialize(Transform generatePos, float hitTime, float noteSpeed, Vector3 noteScaleMult, int laneIndex, int noteIndex, float visuableDistance)
    {
        _fixOrderInlayerByLaneGap = GetComponent<FixOrderInlayerByLaneGap>();
        _view = GetComponent<NoteView>();
        _generatePos = generatePos;
        _hitTime = hitTime;
        _noteSpeed = noteSpeed;
        _view.Initialize(visuableDistance);

        Vector3 localScale = transform.localScale;
        localScale.x *= noteScaleMult.x;
        localScale.y *= noteScaleMult.y;
        localScale.z *= noteScaleMult.z;
        transform.localScale = localScale;
        _fixOrderInlayerByLaneGap.SetLaneIndexToObjIndex(_laneIndex);
        UpdatePosition();
    }
}