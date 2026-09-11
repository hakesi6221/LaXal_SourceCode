using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// MonoBehaviourAnimSequencerでのアニメーション取得を整理するためのラッパークラス
/// AnimationSeqenceElementBaseの継承クラスをInspector上で取得し、再生方式を設定する
/// </summary>
[System.Serializable]
public class AnimationSequenceElement
{
    [SerializeField, Label("アニメーション")]
    private AnimationSeqenceElementBase _anim = null;

    [SerializeField, Label("再生方式")]
    private GameStartAnimType _animType = GameStartAnimType.None;

    /// <summary>
    /// アニメーション
    /// </summary>
    public AnimationSeqenceElementBase Anim => _anim;

    /// <summary>
    /// 再生方式
    /// </summary>
    public GameStartAnimType AnimType => _animType;
}
