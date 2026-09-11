using UnityEngine;
using NaughtyAttributes;

/// <summary>
/// 3レーンモードでのプレイヤー移動関連クラス
/// 
/// XROriginの移動がプレイヤーの移動となるため、その移動の補正と
/// それに伴うレーン移動処理を行う
/// </summary>
public class PlayerControllerIn3Lane : MonoBehaviour
{
    [SerializeField, Label("XR OriginのTransform")]
    private Transform _xrOrigin = null;

    [SerializeField, Label("MainCameraのTransform")]
    private Transform head;

    [SerializeField, Label("移動の倍率")]
    private float moveScale = 3.0f;

    [SerializeField, Label("レーン移動の判定となる閾値")]
    private float _laneChangeThreshold = 0.75f;

    // HMDの前フレームのローカル座標
    private Vector3 _prevHeadLocalPos;

    // XRのトラッキング後に行いたいので、LateUpdate
    void LateUpdate()
    {
        MultipleXROriginePos();
        ChangeLaneByPosition();
    }

    /// <summary>
    /// XROriginの移動を補正する関数
    /// </summary>
    private void MultipleXROriginePos()
    {
        // 本来のHMDの位置を更新
        Vector3 current = head.localPosition;

        // 通常時は位置が変わらないように補正をかける
        Vector3 delta = current - _prevHeadLocalPos;

        // よい防止のため水平方向の移動のみ適用
        delta.y = 0;

        Vector3 extraMove = delta * (moveScale - 1.0f);

        _xrOrigin.position += _xrOrigin.rotation * extraMove;

        _prevHeadLocalPos = current;
    }

    /// <summary>
    /// XROriginのポジションによって、レーン番号を更新する関数
    /// </summary>
    private void ChangeLaneByPosition()
    {
        float xPos = _xrOrigin.localPosition.x + head.localPosition.x;

        if (xPos > _laneChangeThreshold)
            GameManager.Instance.MoveToLane(2);
        else if (xPos < -_laneChangeThreshold)
            GameManager.Instance.MoveToLane(0);
        else
            GameManager.Instance.MoveToLane(1);
    }
}
