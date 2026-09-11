using NaughtyAttributes;
using UnityEngine;
using UnityEngine.U2D.Animation;
using LitMotion;
using Cysharp.Threading.Tasks;
using CommonStringValues.SpriteLibrary;
using System.Threading;

/// <summary>
/// プレイヤーのアニメーションなどの見た目の管理を行うクラス
/// </summary>
public class PlayerView : MonoBehaviour
{
    [SerializeField, Label("移動完了時に足元から出るエフェクト生成位置"), BoxGroup("Effects")]
    private Transform _movedParticlePos = null;

    [SerializeField, Label("差分変更用のSpriteResolver"), BoxGroup("Animation")]
    private SpriteResolver _resolver = null;

    [SerializeField, Label("差分が元に戻るまでの秒数"), BoxGroup("Animation")]
    private float _resolvDuration = 0.5f;

    // 差分変更アニメ用のcts
    private CancellationTokenSource _ctsReolve = new CancellationTokenSource();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 判定時のアニメーションを判定時のコールバックイベントに登録
        GameManager.Instance?.AddOnJudgeEvent(OnJudgeAnimation);
    }

    /// <summary>
    /// 判定時の起こすアニメーション
    /// 差分を評価に応じて変え、拡縮アニメーションを起こす
    /// </summary>
    /// <param name="result">評価</param>
    public void OnJudgeAnimation(JudgeResult result)
    {
        var token = this.GetCancellationTokenOnDestroy();
        string spriteLabel = string.Empty;
        switch (result)
        {
            case JudgeResult.None:
                Debug.LogError("評価が正しくない形で渡れています");
                break;
            case JudgeResult.Miss:
                spriteLabel = Player_SpriteLib.Miss;
                break;
            case JudgeResult.Great:
                spriteLabel = Player_SpriteLib.Success;
                break;
            case JudgeResult.Perfect:
                spriteLabel = Player_SpriteLib.Success;
                break;
        }
        ReturnToIdleSprite();
        // _rotAnim.RotAnim_One(token).Forget();
        _resolver.SetCategoryAndLabel(Player_SpriteLib.Nagisa_Standing, spriteLabel);
    }

    /// <summary>
    /// 移動完了時のパーティクルを再生する関数
    /// アタッチされていない場合はエラーログを残す
    /// </summary>
    public void OnMovedEffect()
    {
        if (_movedParticlePos == null)
        {
            Debug.LogError($"{typeof(PlayerView).Name}:[_movedParticle]がアタッチされていません。");
            return;
        }

        if (_movedParticlePos == null)
        {
            Debug.LogError($"{typeof(PlayerView)}:[_movedParticlePos]がアタッチされていません。");
            return;
        }
        EffectGenerator.Instance?.PlayPlayerOnGroundEffect(_movedParticlePos.position);
    }

    private async void ReturnToIdleSprite()
    {
        _ctsReolve.Cancel();
        _ctsReolve = new CancellationTokenSource();
        var token = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy(), _ctsReolve.Token).Token;
        try
        {
            await UniTask.WaitForSeconds(_resolvDuration, cancellationToken: token);
        }
        catch (System.OperationCanceledException)
        {
            Debug.LogWarning("差分変更アニメーションがキャンセルされました");
            return;
        }
        // _zoomAnim.ZoomAnim_Single(token).Forget();
        _resolver.SetCategoryAndLabel(Player_SpriteLib.Nagisa_Standing, Player_SpriteLib.Idle);
    }

}
