using Unity.XR.XREAL;
using UnityEngine;

/// <summary>
/// XREALHomeMenuプレハブに着けて、追加の機能を持たせるためのクラス
/// </summary>
public class XREALHomeMenuExpandInMainGame : MonoBehaviour
{
    void OnEnable()
    {
        Time.timeScale = 0;
        BeatManager.Instance.Pause();
    }

    void OnDisable()
    {
        // 非アクティブ時楽曲をまた再生
        Time.timeScale = 1;
        BeatManager.Instance.Resume();
    }
}
