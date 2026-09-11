using Cysharp.Threading.Tasks;
using LitMotion;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

public class AppealChanceEnterAnim : MonoBehaviour
{
    [SerializeField, Label("対象のTMP")]
    private TextMeshProUGUI _tmp = null;

    [SerializeField, Label("所要時間：秒")]
    private float _duration = 0.5f;

    [SerializeField, Label("開始幅値")]
    private float _startSpace = 0.0f;

    [SerializeField, Label("終了幅値")]
    private float _finishSpace = 10.0f;

    [SerializeField, Label("イージング")]
    private Ease _fadeEase = Ease.InQuad;

    // 文字間隔アニメーションクラス
    private CharSpaceAnimTmp _anim = new CharSpaceAnimTmp();

    void Start()
    {
        if (_tmp == null)
        {
            Debug.LogError($"{this.name}:[_tmp]がアタッチされていません。");
            return;
        }
        _tmp.characterSpacing = _startSpace;
        _tmp.gameObject.SetActive(false);
        // 4レーンモードなら処理を登録
        if (RhythmGameInfomation.GameMode == RhythmGameMode.FourLane)
        {
            AppealChanceManager.Instance?.AddEnterAppealChanceEvent(OnEnterAppealChance);
            AppealChanceManager.Instance?.AddExitAppealChanceEvent(OnExitAppealChance);
        }
    }

    private void OnEnterAppealChance(double judgeElapsed)
    {
        if (_tmp == null)
        {
            Debug.LogError($"{this.name}:[_tmp]がアタッチされていません。");
            return;
        }
        _tmp.characterSpacing = _startSpace;
        _tmp.gameObject.SetActive(true);
        _anim.PlayAnim
        (
            _tmp,
            _finishSpace,
            _duration,
            ease: _fadeEase,
            cancellationToken: this.GetCancellationTokenOnDestroy()
        ).Forget();
    }

    private void OnExitAppealChance(bool result)
    {
        if (_tmp == null)
        {
            Debug.LogError($"{this.name}:[_tmp]がアタッチされていません。");
            return;
        }
        _anim.PlayAnim
        (
            _tmp,
            _startSpace,
            _duration,
            ease: _fadeEase,
            cancellationToken: this.GetCancellationTokenOnDestroy()
        ).Forget();
        _tmp.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (RhythmGameInfomation.GameMode == RhythmGameMode.FourLane)
        {
            AppealChanceManager.Instance?.RemoveEnterAppealChanceEvent(OnEnterAppealChance);
            AppealChanceManager.Instance?.RemoveExitAppealChanceEvent(OnExitAppealChance);
        }
    }
}
