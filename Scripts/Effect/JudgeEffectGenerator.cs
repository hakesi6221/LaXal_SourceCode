using System;
using NaughtyAttributes;
using UnityEngine;

[Serializable]
/// <summary>
/// リズムゲームの成否判定時のエフェクトの生成処理を担っているクラス
/// Monobehaviourを継承しておらず、エフェクトの生成を管理するマネージャークラス内でインスタンスを作成し、処理を呼ぶ想定
/// </summary>
public class JudgeEffectGenerator
{
    [SerializeField, Label("Perfect判定のエフェクト"), BoxGroup("EffectPrefabs")]
    private ParticleSystem _effectPerfect = null;

    [SerializeField, Label("Great判定のエフェクト"), BoxGroup("EffectPrefabs")]
    private ParticleSystem _effectGreat = null;

    [SerializeField, Label("Miss判定のエフェクト"), BoxGroup("EffectPrefabs")]
    private ParticleSystem _effectMiss = null;

    [SerializeField, Label("エフェクト生成座標"), BoxGroup("EffectParams")]
    private Vector3 _effectGeneratePos = Vector3.zero;

    /// <summary>
    /// リズムゲームの成否判定時のエフェクトを生成する関数
    /// それぞれのエフェクトは1つのParticleSystemオブジェクトだけ用意し、
    /// 生成位置に移動させてパーティクルを生成する形をとっている
    /// 引数で渡された判定結果に応じたエフェクトを生成する
    /// </summary>
    /// <param name="result">判定結果</param>
    /// <param name="playPos">生成するワールド座標</param>
    public void PlayJudgeEffect(JudgeResult result, Vector3 playPos)
    {
        if (playPos == null)
        {
            Debug.LogError($"{typeof(JudgeEffectGenerator).Name}:判定エフェクト生成時の生成座標が不正な形です。");
            return;
        }

        ParticleSystem particle = null;
        switch (result)
        {
            case JudgeResult.None:
                Debug.LogError($"{typeof(JudgeEffectGenerator).Name}:判定エフェクト生成時の成否判定が不正な形です。");
                return;
            case JudgeResult.Perfect:
                if (_effectPerfect == null)
                    Debug.LogError($"{typeof(JudgeEffectGenerator).Name}:[_effectPerfect]がアタッチされていません。");
                else
                    particle = _effectPerfect;
                break;
            case JudgeResult.Great:
                if (_effectGreat == null)
                    Debug.LogError($"{typeof(JudgeEffectGenerator).Name}:[_effectGreat]がアタッチされていません。");
                else
                    particle = _effectGreat;
                break;
            case JudgeResult.Miss:
                if (_effectMiss == null)
                    Debug.LogError($"{typeof(JudgeEffectGenerator).Name}:[_effectMiss]がアタッチされていません。");
                else
                    particle = _effectMiss;
                break;
        }

        if (particle == null)
        {
            Debug.LogError($"{typeof(JudgeEffectGenerator).Name}:判定エフェクト生成時の生成エフェクトが指定されていません");
            return;
        }

        particle.transform.position = playPos;
        particle.Play();
    }

    /// <summary>
    /// リズムゲームの成否判定時のエフェクトを生成する関数
    /// それぞれのエフェクトは1つのParticleSystemオブジェクトだけ用意し、
    /// 生成位置に移動させてパーティクルを生成する形をとっている
    /// 引数で渡された判定結果に応じたエフェクトを生成する
    /// </summary>
    /// <param name="result">判定結果</param>
    public void PlayJudgeEffect(JudgeResult result)
    {
        PlayJudgeEffect(result, _effectGeneratePos);
    }
}
