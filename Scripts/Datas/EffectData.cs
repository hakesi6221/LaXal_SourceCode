using System;
using UnityEngine;

/// <summary>
/// エフェクトのデータを定義するデータクラス
/// </summary>
[Serializable]
public class EffectData
{
    // エフェクトの種類
    [SerializeField]
    private EffectType _type = EffectType.None;

    // エフェクトのオブジェクト
    [SerializeField]
    private ParticleSystem _particle = null;

    /// <summary>
    /// エフェクトの種類
    /// </summary>
    public EffectType Type => _type;

    /// <summary>
    /// エフェクトのオブジェクト
    /// </summary>
    public ParticleSystem ParticleSystem => _particle;
}
