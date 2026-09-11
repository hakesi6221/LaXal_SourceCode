using NaughtyAttributes;
using UnityEngine;using Unity.XR.XREAL;

/// <summary>
/// Awake時に、Inspectorで設定されたDOFに変更するクラス
/// </summary>
public class SetDofOnAwake : MonoBehaviour
{
    [SerializeField, Label("変更先のdOfモード")]
    private TrackingType _targetType = TrackingType.MODE_3DOF;

    private void Awake()
    {
        if (XREALPlugin.GetTrackingType() == _targetType) return;
        DoFChanger.ChangeDof(_targetType);
    }
}
