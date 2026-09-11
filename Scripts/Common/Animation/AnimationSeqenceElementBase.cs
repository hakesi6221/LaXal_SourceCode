using System.Threading;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// MonoBehaviourAnimSequencerの要素となるアニメーションを作るための基底クラス
/// これを継承したクラスを上記のクラスにアニメーションに設定できる
/// </summary>
public abstract class AnimationSeqenceElementBase : MonoBehaviour
{
    /// <summary>
    /// アニメーション再生前の初期化処理
    /// </summary>
    public abstract void Initialize();

    /// <summary>
    /// アニメーションの再生
    /// 非同期にする必要あり：待機可能
    /// </summary>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns></returns>
    public abstract UniTask PlayAnimationAsync(CancellationToken cancellationToken = default);

    [Button("アニメーションテスト")]
    private void AnimationTest()
    {
        PlayAnimationAsync().Forget();
    }
}
