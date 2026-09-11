using System.Linq;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;

public class AppealChanceExitAnim : MonoBehaviour
{
    [SerializeField, Label("カットイン")]
    private CutInObj _cutIn = null;

    [SerializeField, Label("アピールアセット")]
    private AppealAssetsDatas _appealAssets = null;

    void Start()
    {
        if (_cutIn == null)
        {
            Debug.LogError($"{this.name}:[_tmp]がアタッチされていません。");
            return;
        }
        _cutIn.gameObject.SetActive(false);
        // 4レーンモードなら処理を登録
        if (RhythmGameInfomation.GameMode == RhythmGameMode.FourLane)
            AppealChanceManager.Instance?.AddExitAppealChanceEvent(OnExitAppealChance);
    }

    private void OnExitAppealChance(bool result)
    {
        if (!result) return;
        if (_appealAssets == null) return;

        AppealAssetsData[] datas = _appealAssets.AssetDatas.ToArray();
        if (datas == null) return;

        int index = Random.Range(0, datas.Length);
        AppealAssetsData data = datas[index];
        if (data == null) return;

        _cutIn.gameObject.SetActive(true);
        _cutIn.PlayCutIn(data.Standing).Forget();
        SoundManager.Instance.PlayVoice(data.AudioType);
    }

    void OnDestroy()
    {
        if (RhythmGameInfomation.GameMode == RhythmGameMode.FourLane)
            AppealChanceManager.Instance?.RemoveExitAppealChanceEvent(OnExitAppealChance);
    }
}
