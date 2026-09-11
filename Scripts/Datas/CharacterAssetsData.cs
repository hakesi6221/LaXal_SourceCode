using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// キャラクター名と、それに付随するグラフィックや音声のデータをまとめたデータクラス
/// 選択した楽曲に応じた描画や音声の再生に使用する
/// </summary>
[Serializable]
[CreateAssetMenu(fileName = "CharacterAssetsData", menuName = "ScriptableObjects/CharacterAssetsData")]
public class CharacterAssetsData : ScriptableObject
{
    [SerializeField, Header("キャラクター名")]
    private string _charName = string.Empty;

    [SerializeField, Header("キャラクター立ち絵(提供)")]
    private Sprite _charStandingPro = null;

    [SerializeField, Header("キャラクター立ち絵(オリジナル)")]
    private Sprite _charStandingOri = null;

    [SerializeField, Header("Live2D立ち絵")]
    private GameObject _charLive2D = null;

    [SerializeField, Header("配信ロゴ")]
    private Sprite _streamLogo;

    [SerializeField, Header("キャラのサイン")]
    private Sprite _charaSign;

    /// <summary>
    /// キャラクター名
    /// </summary>
    public string Name => _charName;

    /// <summary>
    /// キャラクター立ち絵：提供
    /// </summary>
    public Sprite StandingPro => _charStandingPro;

    /// <summary>
    /// キャラクター立ち絵：オリジナル
    /// </summary>
    public Sprite StandingOri => _charStandingOri;

    /// <summary>
    /// Live2D立ち絵
    /// </summary>
    public GameObject Live2D => _charLive2D;

    /// <summary>
    /// 配信ロゴ
    /// </summary>
    public Sprite StreamLogo => _streamLogo;

    /// <summary>
    /// キャラのサイン
    /// </summary>
    public Sprite CharaSign => _charaSign;
}
