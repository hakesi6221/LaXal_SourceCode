using System;
using UnityEngine;

[Serializable]
public class AudioInfomation
{
    [SerializeField, Header("サウンドの種類")]
    private AudioType _type = AudioType.None;

    [SerializeField, Header("サウンドのクリップ")]
    private AudioClip _clip = null;

    [SerializeField, Header("サウンドのボリューム"), Range(0.0f, 1.0f)]
    private float _volume = 1.0f;

    [SerializeField, Header("ループするか")]
    private bool _loop = false;

    [SerializeField, Header("再生のオフセット"), Range(0.0f, 1.0f)]
    private float _ofset = 0f;
    /// <summary>
    /// 再生のオフセット
    /// </summary>
    public float Ofset => _ofset;
    /// <summary>
    /// サウンドの種類
    /// </summary>
    public AudioType Type => _type;
    /// <summary>
    /// サウンドのクリップ
    /// </summary>
    public AudioClip Clip => _clip;
    /// <summary>
    /// サウンドのボリューム
    /// </summary>
    public float Volume => _volume;
    /// <summary>
    /// ループするか
    /// </summary>
    public bool Loop => _loop;
}
