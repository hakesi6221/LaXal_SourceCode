using UnityEngine;
using NaughtyAttributes;
using Common.SingleTon;

public class EffectGenerator : SingletonMonoBehaviour<EffectGenerator>
{
    protected override bool dontDestroyOnLoad => true;

    // 判定時エフェクトの生成を管理するクラスのインスタンス
    [SerializeField, Label("判定エフェクト生成関連")]
    private JudgeEffectGenerator _judgeEffectGenerator = null;

    [SerializeField, Label("プレイヤーエフェクト生成関連")]
    private PlayerEffectGenerator _playerEffectGenerator = null;

    [SerializeField, Label("画面遷移演出")]
    private ParticleSystem _titleTransition = null;

    /// <summary>
    /// プレイヤー着地時のエフェクトを再生
    /// </summary>
    /// <param name="playPos">再生場所</param>
    public void PlayPlayerOnGroundEffect(Vector3 playPos)
    {
        if (playPos == null)
        {
            Debug.LogError($"{typeof(EffectGenerator).Name}:エフェクト生成時の生成座標が不正な形です。");
            return;
        }

        _playerEffectGenerator.PlayGroundEffetct(playPos);
    }

    /// <summary>
    /// 判定時のエフェクトを再生
    /// </summary>
    /// <param name="judge">判定結果</param>
    public void PlayJudgeEffect(JudgeResult judge)
    {
        _judgeEffectGenerator.PlayJudgeEffect(judge);
    }

    /// <summary>
    /// タイトルの遷移エフェクトを再生する
    /// </summary>
    public void PlayTitleTransitionEffect()
    {
        if (_titleTransition == null)
        {
            Debug.LogError($"{this.name}:タイトルフェード演出参照が存在しません");
            return;
        }

        _titleTransition.Play();
    }
}
