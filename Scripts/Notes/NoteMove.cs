using UnityEngine;

/// <summary>
/// ローカル座標でノーツを移動するクラス
/// </summary>
[RequireComponent(typeof(FixOrderInlayerByLaneGap))]
[RequireComponent(typeof(NoteView))]
public class NoteMove : MonoBehaviour
{
    [Header("確認用")]
    [SerializeField] protected float _hitTime;
    [SerializeField] protected float _noteSpeed;

    protected Transform _generatePos;
    protected int _laneIndex = 0;
    protected int _noteIndex = 0;
    protected bool _judged = false;

    // === コンポーネント参照 ===
    protected FixOrderInlayerByLaneGap _fixOrderInlayerByLaneGap;
    protected NoteView _view = null;

    private void Update()
    {
        UpdatePosition();
    }

    /// <summary>
    /// 曲時間から現在位置を更新
    /// </summary>
    protected virtual void UpdatePosition()
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
        _view.UpdateSpriteAlpha(distance);

        Vector3 localPosition = transform.localPosition;
        localPosition.y = distance;
        transform.localPosition = localPosition;
    }

    public virtual void Initialize(Transform generatePos, float hitTime, float noteSpeed, Vector3 noteScaleMult, int laneIndex, int noteIndex, float visuableDistance)
    {
        _fixOrderInlayerByLaneGap = GetComponent<FixOrderInlayerByLaneGap>();
        _view = GetComponent<NoteView>();
        _generatePos = generatePos;
        _hitTime = hitTime;
        _noteSpeed = noteSpeed;
        _laneIndex = laneIndex;
        _noteIndex = noteIndex;
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