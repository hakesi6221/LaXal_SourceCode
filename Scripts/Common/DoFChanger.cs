using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.XR.XREAL;
using UnityEngine;

/// <summary>
/// DOFの変更機能を担当する静的クラス
/// </summary>
public static class DoFChanger
{
    /// <summary>
    /// DOFを0DOFに変更
    /// </summary>
    public static void ChangeDoF_0DoF()
    {
        XREALPlugin.SwitchTrackingTypeAsync(TrackingType.MODE_0DOF);
    }

    /// <summary>
    /// DOFを遅延あり0DOFに変更
    /// </summary>

    public static void ChangeDoF_0DoF_Stab()
    {
        XREALPlugin.SwitchTrackingTypeAsync(TrackingType.MODE_0DOF_STAB);
    }

    /// <summary>
    /// DOFを3DOFに変更
    /// </summary>

    public static void ChangeDoF_3DoF()
    {
        XREALPlugin.SwitchTrackingTypeAsync(TrackingType.MODE_3DOF);
    }

    /// <summary>
    /// DOFを6DOFに変更
    /// </summary>

    public static void ChangeDoF_6DoF()
    {
        XREALPlugin.SwitchTrackingTypeAsync(TrackingType.MODE_6DOF);
    }

    /// <summary>
    /// 指定したモードにDofを変更
    /// </summary>
    /// <param name="trackingType">指定のモード</param>
    public static void ChangeDof(TrackingType trackingType)
    {
        XREALPlugin.SwitchTrackingTypeAsync(trackingType);
    }

    public static async UniTask ResetAngleOfView(CancellationToken cancellationToken)
    {
        var currentType = XREALPlugin.GetTrackingType();

        try
        {
            await XREALPlugin.SwitchTrackingTypeAsync(TrackingType.MODE_0DOF)
                            .AsUniTask()
                            .AttachExternalCancellation(cancellationToken);
            // 反映させるため1フレーム待つ
            await UniTask.Yield(cancellationToken);
            await XREALPlugin.SwitchTrackingTypeAsync(currentType)
                            .AsUniTask()
                            .AttachExternalCancellation(cancellationToken);
        }
        catch (System.OperationCanceledException)
        {
            Debug.LogWarning("非同期処理のキャンセル");
            return;
        }
    }
}
