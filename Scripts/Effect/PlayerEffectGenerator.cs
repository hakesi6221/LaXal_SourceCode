using System;
using NaughtyAttributes;
using UnityEngine;

[Serializable]
/// <summary>
/// プレイヤー関連のエフェクトの生成処理を担っているクラス
/// Monobehaviourを継承しておらず、エフェクトの生成を管理するマネージャークラス内でインスタンスを作成し、処理を呼ぶ想定
/// </summary>
public class PlayerEffectGenerator
{
    [SerializeField, Label("プレイヤー着地時に出すエフェクト"), BoxGroup("EffectPrefabs")]
    private ParticleSystem _effectGround = null;

    /// <summary>
    /// プレイヤーが着地したときのエフェクトを生成する関数
    /// それぞれのエフェクトは1つのParticleSystemオブジェクトだけ用意し、
    /// 生成位置に移動させてパーティクルを生成する形をとっている
    /// </summary>
    /// <param name="playPos"></param>
    public void PlayGroundEffetct(Vector3 playPos)
    {
        if (playPos == null)
        {
            Debug.LogError($"{typeof(PlayerEffectGenerator).Name}:エフェクト生成時の生成座標が不正な形です。");
            return;
        }

        if (_effectGround == null)
        {
            Debug.LogError($"{typeof(PlayerEffectGenerator).Name}:エフェクト生成時の生成エフェクトが指定されていません");
            return;
        }

        _effectGround.transform.position = playPos;
        _effectGround.Play();
    }
}
