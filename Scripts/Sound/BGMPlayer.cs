using UnityEngine;

public class BGMPlayer : MonoBehaviour
{
    [SerializeField]
    private AudioType _titleBGM;

    private void Start()
    {
        SoundManager.Instance?.PlayBGMWithFadeIn(_titleBGM);
    }

    private void OnDestroy()
    {
        SoundManager.Instance?.StopBGMWithFadeOut(_titleBGM);
    }
}