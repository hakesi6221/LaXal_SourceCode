using UnityEngine;
using System;

[Serializable]
public class AppealAssetsData
{
    [SerializeField]
    private Sprite _standing = null;

    [SerializeField]
    private AudioType _audioType = AudioType.None;

    public Sprite Standing => _standing;

    public AudioType AudioType => _audioType;
}
