using Cysharp.Threading.Tasks;
using LitMotion;
using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// このクラスをアタッチしたオブジェクトに、
/// リズムゲーム中、判定のタイミングでスケールアニメーションを起こさせるためのユーティリティクラス
///
/// メインゲームでのみ使用可能
/// </summary>
public class ScaleAnimOnJudge : MonoBehaviour
{
    [SerializeField, Label("デフォルトのlocalScale"), BoxGroup("CommentWindow")]
    private float _defaultScaleMult = 1.0f;

    [SerializeField, Label("ズームアニメ後のlocalScale"), BoxGroup("CommentWindow")]
    private float _zoomedScaleMult = 1.1f;

    [SerializeField, Label("ズームアニメの長さ：秒"), BoxGroup("CommentWindow")]
    private float _zoomAnimDuration = 0.1f;

    [SerializeField, Label("ズームアニメのEasing"), BoxGroup("CommentWindow")]
    private Ease _zoomAnimEase = Ease.InQuad;

    // コメントウィンドウのアニメコンポーネント
    private ScaleAnimation_Zoom<Transform> _zoomAnim = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // アニメーションクラスインスタンスを作成
        _zoomAnim = new ScaleAnimation_Zoom<Transform>
        (
            transform,
            _defaultScaleMult,
            _zoomedScaleMult,
            _zoomAnimDuration,
            _zoomAnimEase
        );
        // 判定時のコールバックイベントに登録
        GameManager.Instance?.AddOnJudgeEvent(OnJudge);
    }

    /// <summary>
    /// 判定時のコールバックイベントに登録する処理
    /// ・ウィンドウの拡縮アニメーション
    /// ・コメント文の更新
    /// </summary>
    private void OnJudge(JudgeResult result)
    {
        var token = this.GetCancellationTokenOnDestroy();
        _zoomAnim.ZoomAnim_Single(token).Forget();
    }
}
