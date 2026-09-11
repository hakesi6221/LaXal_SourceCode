using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Common.SingleTon;
using UnityEngine;
using UnityEngine.Audio;
using System.Threading;

[DefaultExecutionOrder(-90)]
public class SoundManager : SingletonMonoBehaviour<SoundManager>
{
    protected override bool dontDestroyOnLoad => true;

    #region privateオブジェクト
    [SerializeField, Header("BGMリスト")]
    private AudioInfomations _bgmList = null;

    [SerializeField, Header("SEリスト")]
    private AudioInfomations _seList = null;

    [SerializeField, Header("ボイスリスト")]
    private AudioInfomations _voiceList = null;

    [SerializeField, Header("親ミキサー")]
    private AudioMixer _mixer;

    [SerializeField, Header("BGMミキサー")]
    private AudioMixerGroup _bgmMixier;

    [SerializeField, Header("SEミキサー")]
    private AudioMixerGroup _seMixier;

    [SerializeField, Header("Voiceミキサー")]
    private AudioMixerGroup _voiceMixier;

    [SerializeField, Header("デフォルトフェード時間"), Range(0.0f, 5.0f)]
    private float _defaultFadeRate = 0.0f;

    private List<AudioSource> _sources = new List<AudioSource>();

    new void Awake()
    {
        base.Awake();

        // 各音のリストを初期化
            _sources = new List<AudioSource>();
        if (_bgmList != null)
        {
            foreach (AudioInfomation audio in _bgmList.Audios)
            {
                AudioSource source = CreateNewAudioSource();
                source.outputAudioMixerGroup = _bgmMixier;
            }
        }
        if (_seList != null)
        {
            foreach (AudioInfomation audio in _seList.Audios)
            {
                AudioSource source = CreateNewAudioSource();
                source.outputAudioMixerGroup = _seMixier;
            }
        }
        if (_voiceList != null)
        {
            foreach (AudioInfomation audio in _voiceList.Audios)
            {
                AudioSource source = CreateNewAudioSource();
                source.outputAudioMixerGroup = _voiceMixier;
            }
        }
    }

    private AudioSource CreateNewAudioSource()
    {
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        _sources.Add(newSource);

        return newSource;
    }

    /// <summary>
    /// サウンドを実際に鳴らす
    /// </summary>
    /// <param name="source">鳴らすソース</param>
    /// <param name="audio">鳴らす音の情報</param>
    private void OnPlaySound(AudioSource source, AudioInfomation audio, AudioMixerGroup mixer)
    {
        if (source == null || audio == null)
        {
            Debug.LogError($"{this.name}:渡されたAudioSourceもしくはAudioInfomationがnullです");
            return;
        }
        source.clip = audio.Clip;
        source.volume = audio.Volume;
        source.loop = audio.Loop;
        source.time = audio.Ofset;
        source.outputAudioMixerGroup = mixer;

        source.Play();
    }

    /// <summary>
    /// サウンドを実際に鳴らす
    /// </summary>
    /// <param name="source">鳴らすソース</param>
    /// <param name="clip">鳴らす音のクリップ</param>
    private void OnPlaySound(AudioSource source, AudioClip clip, AudioMixerGroup mixer)
    {
        if (source == null || clip == null)
        {
            Debug.LogError($"{this.name}:渡されたAudioSourceもしくはAudioClipがnullです");
            return;
        }
        source.clip = clip;
        source.outputAudioMixerGroup = mixer;

        source.Play();
    }


    /// <summary>
    /// サウンドを実際に鳴らす
    /// フェードインあり
    /// </summary>
    /// <param name="source">鳴らすソース</param>
    /// <param name="audio">鳴らす音の情報</param>
    /// <param name="startVolume">最初のボリューム</param>
    /// <param name="endVolume">最終的なボリューム</param>
    /// <param name="fadeTime">フェードの時間</param>
    private async UniTask OnPlaySoundWithFadeIn(AudioSource source, AudioInfomation audio, AudioMixerGroup mixer, float startVolume, float endVolume, float fadeTime)
    {
        source.clip = audio.Clip;
        source.volume = startVolume;
        source.loop = audio.Loop;
        source.time = audio.Ofset;
        source.outputAudioMixerGroup = mixer;

        source.Play();
        try
        {
            await FadeMoveSound(source, fadeTime, endVolume, source.GetCancellationTokenOnDestroy());
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("サウンド再生での非同期キャンセル");
            return;
        }
    }


    /// <summary>
    /// サウンドを実際に鳴らす
    /// フェードインあり
    /// </summary>
    /// <param name="source">鳴らすソース</param>
    /// <param name="clip">鳴らす音の情報</param>
    /// <param name="startVolume">最初のボリューム</param>
    /// <param name="endVolume">最終的なボリューム</param>
    /// <param name="fadeTime">フェードの時間</param>
    private async UniTask OnPlaySoundWithFadeIn(AudioSource source, AudioClip clip, AudioMixerGroup mixer, float startVolume, float endVolume, float fadeTime)
    {
        source.clip = clip;
        source.outputAudioMixerGroup = mixer;

        source.Play();
        try
        {
            await FadeMoveSound(source, fadeTime, endVolume, source.GetCancellationTokenOnDestroy());
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("サウンド再生での非同期キャンセル");
            return;
        }
    }

    /// <summary>
    /// サウンドを直接止める
    /// </summary>
    /// <param name="source">鳴らすソース</param>
    private void OnStopSound(AudioSource source)
    {
        source.Stop();
    }

    /// <summary>
    /// サウンドを直接止める
    /// フェードアウトあり
    /// </summary>
    /// <param name="source">止めるソース</param>
    /// <param name="endVolume">最終的なボリューム</param>
    /// <param name="fadeTime">フェードの時間</param>
    private async UniTask OnStopSoundWithFadeOut(AudioSource source, float endVolume, float fadeTime)
    {
        try
        {
            await FadeMoveSound(source, fadeTime, endVolume, source.GetCancellationTokenOnDestroy());
            source.Stop();
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("サウンド再生での非同期キャンセル");
            return;
        }
    }

    /// <summary>
    /// 現在再生中ではないAudioSourceをリストの中から検索
    /// すべて埋まっていた場合、新しいものを作る
    /// </summary>
    /// <param name="sources">検索したいAudioSourceのリスト</param>
    /// <returns></returns>
    private AudioSource SearchEmptySource(List<AudioSource> sources)
    {
        foreach (AudioSource source in sources)
        {
            if (!source.isPlaying)
                return source;
        }

        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        sources.Add(newSource);

        return newSource;
    }

    /// <summary>
    /// 指定したAudioClipを再生しているAudioSourceをリストの中から検索
    /// </summary>
    /// <param name="sources">検索したいAudioSourceのリスト</param>
    /// <param name="clip">検索したいAudioClip</param>
    /// <returns></returns>
    private AudioSource SearchSourceByClip(List<AudioSource> sources, AudioClip clip)
    {
        foreach (AudioSource source in sources)
        {
            if (source.clip == clip)
                return source;
        }

        return null;
    }

    /// <summary>
    /// 指定したサウンドをフェードして音量を移動
    /// </summary>
    /// <param name="audio">フェードしたい音</param>
    /// <param name="fadeOutSec">フェード時間</param>
    /// <param name="volume">フェード後の音量</param>
    /// <returns></returns>
    private async UniTask FadeMoveSound(AudioInfomation audio, float fadeOutSec, float volume, CancellationToken cancellationToken = default)
    {
        if (audio == null) return;

        // 属しているAudioSourceを検索
        AudioSource source = SearchSourceByClip(_sources, audio.Clip);
        if (source == null) return;

        // 変化後の音量がもとと同じ、必要ないので
        if (volume == source.volume) return;

        if (fadeOutSec > 0f)
        {
            float timeCnt = 0;
            float startVolume = source.volume;
            float endVolume = volume;
            try
            {
                while (timeCnt <= fadeOutSec)
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    timeCnt += Time.deltaTime;
                    source.volume = Mathf.Lerp(startVolume, endVolume, timeCnt / fadeOutSec);
                    await UniTask.Yield(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("サウンド再生での非同期キャンセル");
                return;
            }
        }

        source.volume = volume;
    }

    /// <summary>
    /// 指定したサウンドをフェードして音量を移動
    /// </summary>
    /// <param name="source">フェードしたいソース</param>
    /// <param name="fadeOutSec">フェード時間</param>
    /// <param name="volume">フェード後の音量</param>
    /// <returns></returns>
    private async UniTask FadeMoveSound(AudioSource source, float fadeOutSec, float volume, CancellationToken cancellationToken = default)
    {
        if (source == null) return;

        // 変化後の音量がもとと同じ、必要ないので
        if (volume == source.volume) return;

        if (fadeOutSec > 0f)
        {
            float timeCnt = 0;
            float startVolume = source.volume;
            float endVolume = volume;
            try
            {
                while (timeCnt <= fadeOutSec)
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    timeCnt += Time.deltaTime;
                    source.volume = Mathf.Lerp(startVolume, endVolume, timeCnt / fadeOutSec);
                    await UniTask.Yield(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("サウンド再生での非同期キャンセル");
                return;
            }
        }

        source.volume = volume;
    }
    #endregion

    /// <summary>
    /// BGMのボリュームを設定
    /// </summary>
    /// <param name="volume">新しいボリューム</param>
    public void SetBGMVolume(float volume)
    {
        _mixer.SetFloat("BGM", volume);
    }
    /// <summary>
    /// BGMのボリューム
    /// </summary>
    public float GetBGMVolume
    {
        get
        {
            float volume = 0f;
            if (_mixer.GetFloat("BGM", out volume))
                return volume;
            else
                return -1f;
        }
    }

    /// <summary>
    /// SEのボリュームを設定
    /// </summary>
    /// <param name="volume">新しいボリューム</param>
    public void SetSEVolume(float volume)
    {
        _mixer.SetFloat("SE", volume);
    }
    /// <summary>
    /// SEのボリューム
    /// </summary>
    public float GetSEVolume
    {
        get
        {
            float volume = 0f;
            if (_mixer.GetFloat("SE", out volume))
                return volume;
            else
                return -1f;
        }
    }

    /// <summary>
    /// ボイスのボリュームを設定
    /// </summary>
    /// <param name="volume">新しいボリューム</param>
    public void SetVoiceVolume(float volume)
    {
        _mixer.SetFloat("Voice", volume);
    }
    /// <summary>
    /// ボイスのボリューム
    /// </summary>
    public float GetVoiceVolume
    {
        get
        {
            float volume = 0f;
            if (_mixer.GetFloat("Voice", out volume))
                return volume;
            else
                return -1f;
        }
    }

    #region サウンド再生
    /// <summary>
    /// 指定したインデックスのBGMを鳴らす
    /// </summary>
    /// <param name="index">BGMのインデックス</param>
    private AudioSource PlaySound(AudioInfomations infos, int index, AudioMixerGroup mixer)   // 効果音を鳴らす(単発)
    {
        if (index < 0 || infos.Audios.Count <= index) return null;
        AudioInfomation audio = infos.Audios[index];
        if (audio == null) return null;
        AudioSource source = SearchEmptySource(_sources);
        if (source == null) return null;

        OnPlaySound(source, audio, mixer);
        return source;
    }

    /// <summary>
    /// 指定したインデックスのBGMを鳴らす
    /// フェードインあり
    /// </summary>
    /// <param name="index">BGMのインデックス</param>
    private async UniTask<AudioSource> PlaySoundWithFadeIn(AudioInfomations infos, int index, AudioMixerGroup mixer)
    {
        if (index < 0 || infos.Audios.Count <= index) return null;
        AudioInfomation audio = infos.Audios[index];
        if (audio == null) return null;
        AudioSource source = SearchEmptySource(_sources);
        if (source == null) return null;

        try
        {
            await OnPlaySoundWithFadeIn(source, audio, mixer, 0f, audio.Volume, _defaultFadeRate);
            return source;
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("サウンド再生での非同期キャンセル");
            return null;
        }
    }

    /// <summary>
    /// 指定したインデックスのBGMを鳴らす
    /// フェードインあり
    /// </summary>
    /// <param name="index">BGMのインデックス</param>
    private async UniTask<AudioSource> PlaySoundWithFadeIn(AudioInfomations infos, int index,AudioMixerGroup mixer, float startVolume, float endVolume, float fadeInSec)
    {
        if (index < 0 || infos.Audios.Count <= index) return null;
        AudioInfomation audio = infos.Audios[index];
        if (audio == null) return null;
        AudioSource source = SearchEmptySource(_sources);
        if (source == null) return null;

        try
        {
            await OnPlaySoundWithFadeIn(source, audio, mixer, startVolume, endVolume, fadeInSec);
            return source;
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("サウンド再生での非同期キャンセル");
            return null;
        }
    }

    /// <summary>
    /// 指定したインデックスBGMを止める
    /// </summary>
    /// <param name="index">BGMのインデックス</param>
    private void StopSound(AudioInfomations infos, int index)
    {
        if (index < 0 || infos.Audios.Count <= index) return;
        AudioInfomation audio = infos.Audios[index];
        if (audio == null) return;
        AudioSource source = SearchSourceByClip(_sources, audio.Clip);
        if (source == null) return;

        OnStopSound(source);
    }

    /// <summary>
    /// 指定したインデックスBGMを止める
    /// フェードアウトあり
    /// </summary>
    /// <param name="index">BGMのインデックス</param>
    private async UniTask StopSoundWithFadeOut(AudioInfomations infos, int index)
    {
        if (index < 0 || infos.Audios.Count <= index) return;
        AudioInfomation audio = infos.Audios[index];
        if (audio == null) return;
        AudioSource source = SearchSourceByClip(_sources, audio.Clip);
        if (source == null) return;

        try
        {
            await OnStopSoundWithFadeOut(source, 0f, _defaultFadeRate);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("サウンド再生での非同期キャンセル");
            return;
        }
    }

    /// <summary>
    /// 指定したインデックスBGMを止める
    /// フェードアウトあり
    /// </summary>
    /// <param name="index">BGMのインデックス</param>
    private async UniTask StopSoundWithFadeOut(AudioInfomations infos, int index, float endVolume, float fadeOutSec)
    {
        if (index < 0 || infos.Audios.Count <= index) return;
        AudioInfomation audio = infos.Audios[index];
        if (audio == null) return;
        AudioSource source = SearchSourceByClip(_sources, audio.Clip);
        if (source == null) return;

        try
        {
            await OnStopSoundWithFadeOut(source, endVolume, fadeOutSec);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("サウンド再生での非同期キャンセル");
            return;
        }
    }

    /// <summary>
    /// 指定したインデックスのBGMを鳴らす
    /// </summary>
    /// <param name="type">BGMのインデックス</param>
    private AudioSource PlaySound(AudioInfomations infos, AudioType type, AudioMixerGroup mixer)   // 効果音を鳴らす(単発)
    {
        AudioInfomation audio = infos.GetAudioInfomation(type);
        if (audio == null) return null;
        AudioSource source = SearchEmptySource(_sources);
        if (source == null) return null;

        OnPlaySound(source, audio, mixer);
        return source;
    }

    /// <summary>
    /// 指定したインデックスのBGMを鳴らす
    /// フェードインあり
    /// </summary>
    /// <param name="type">BGMのインデックス</param>
    private async UniTask<AudioSource> PlaySoundWithFadeIn(AudioInfomations infos, AudioType type, AudioMixerGroup mixer)
    {
        AudioInfomation audio = infos.GetAudioInfomation(type);
        if (audio == null) return null;
        AudioSource source = SearchEmptySource(_sources);
        if (source == null) return null;

        try
        {
            await OnPlaySoundWithFadeIn(source, audio, mixer, 0f, audio.Volume, _defaultFadeRate);
            return source;
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("サウンド再生での非同期キャンセル");
            return null;
        }
    }

    /// <summary>
    /// 指定したインデックスのBGMを鳴らす
    /// フェードインあり
    /// </summary>
    /// <param name="type">BGMのインデックス</param>
    private async UniTask<AudioSource> PlaySoundWithFadeIn(AudioInfomations infos, AudioType type, AudioMixerGroup mixer, float startVolume, float endVolume, float fadeInSec)
    {
        AudioInfomation audio = infos.GetAudioInfomation(type);
        if (audio == null) return null;
        AudioSource source = SearchEmptySource(_sources);
        if (source == null) return null;

        try
        {
            await OnPlaySoundWithFadeIn(source, audio, mixer, startVolume, endVolume, fadeInSec);
            return source;
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("サウンド再生での非同期キャンセル");
            return null;
        }
    }

    /// <summary>
    /// 指定したインデックスBGMを止める
    /// </summary>
    /// <param name="type">BGMのインデックス</param>
    private void StopSound(AudioInfomations infos, AudioType type)
    {
        AudioInfomation audio = infos.GetAudioInfomation(type);
        if (audio == null) return;
        AudioSource source = SearchSourceByClip(_sources, audio.Clip);
        if (source == null) return;

        OnStopSound(source);
    }

    /// <summary>
    /// 指定したインデックスBGMを止める
    /// フェードアウトあり
    /// </summary>
    /// <param name="type">BGMのインデックス</param>
    private async UniTask StopSoundWithFadeOut(AudioInfomations infos, AudioType type)
    {
        AudioInfomation audio = infos.GetAudioInfomation(type);
        if (audio == null) return;
        AudioSource source = SearchSourceByClip(_sources, audio.Clip);
        if (source == null) return;

        try
        {
            await OnStopSoundWithFadeOut(source, 0f, _defaultFadeRate);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("サウンド再生での非同期キャンセル");
            return;
        }
    }

    /// <summary>
    /// 指定したインデックスBGMを止める
    /// フェードアウトあり
    /// </summary>
    /// <param name="type">BGMのインデックス</param>
    private async UniTask StopSoundWithFadeOut(AudioInfomations infos, AudioType type, float endVolume, float fadeOutSec)
    {
        AudioInfomation audio = infos.GetAudioInfomation(type);
        if (audio == null) return;
        AudioSource source = SearchSourceByClip(_sources, audio.Clip);
        if (source == null) return;

        try
        {
            await OnStopSoundWithFadeOut(source, endVolume, fadeOutSec);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("サウンド再生での非同期キャンセル");
            return;
        }
    }
    #endregion

    #region BGM操作
    /// <summary>
    /// 指定したインデックスのBGMを鳴らす
    /// </summary>
    /// <param name="index">BGMのインデックス</param>
    public AudioSource PlayBGM(int index)   // 効果音を鳴らす(単発)
    {
        return PlaySound(_bgmList, index, _bgmMixier);
    }

    /// <summary>
    /// 指定したインデックスのBGMを鳴らす
    /// フェードインあり
    /// </summary>
    /// <param name="index">BGMのインデックス</param>
    public async UniTask<AudioSource> PlayBGMWithFadeIn(int index)
    {
        return await PlaySoundWithFadeIn(_bgmList, index, _bgmMixier);
    }

    /// <summary>
    /// 指定したインデックスのBGMを鳴らす
    /// フェードインあり
    /// </summary>
    /// <param name="index">BGMのインデックス</param>
    public async UniTask<AudioSource> PlayBGMWithFadeIn(int index, float startVolume, float endVolume, float fadeInSec)
    {
        return await PlaySoundWithFadeIn(_bgmList, index, _bgmMixier, startVolume, endVolume, fadeInSec);
    }

    /// <summary>
    /// 指定したインデックスBGMを止める
    /// </summary>
    /// <param name="index">BGMのインデックス</param>
    public void StopBGM(int index)
    {
        StopSound(_bgmList, index);
    }

    /// <summary>
    /// 指定したインデックスBGMを止める
    /// フェードアウトあり
    /// </summary>
    /// <param name="index">BGMのインデックス</param>
    public async UniTask StopBGMWithFadeOut(int index)
    {
        await StopSoundWithFadeOut(_bgmList, index);
    }

    /// <summary>
    /// 指定したインデックスBGMを止める
    /// フェードアウトあり
    /// </summary>
    /// <param name="index">BGMのインデックス</param>
    public async UniTask StopBGMWithFadeOut(int index, float endVolume, float fadeOutSec)
    {
        await StopSoundWithFadeOut(_bgmList, index, endVolume, fadeOutSec);
    }

    /// <summary>
    /// 指定したインデックスのBGMを鳴らす
    /// </summary>
    /// <param name="type">BGMの種類</param>
    public AudioSource PlayBGM(AudioType type)
    {
        return PlaySound(_bgmList, type, _bgmMixier);
    }

    /// <summary>
    /// 指定したインデックスのBGMを鳴らす
    /// フェードインあり
    /// </summary>
    /// <param name="type">BGMの種類</param>
    public async UniTask<AudioSource> PlayBGMWithFadeIn(AudioType type)
    {
        return await PlaySoundWithFadeIn(_bgmList, type, _bgmMixier);
    }

    /// <summary>
    /// 指定したインデックスのBGMを鳴らす
    /// フェードインあり
    /// </summary>
    /// <param name="type">BGMの種類</param>
    public async UniTask<AudioSource> PlayBGMWithFadeIn(AudioType type, float startVolume, float endVolume, float fadeInSec)
    {
        return await PlaySoundWithFadeIn(_bgmList, type, _bgmMixier, startVolume, endVolume, fadeInSec);
    }

    /// <summary>
    /// 指定したインデックスBGMを止める
    /// </summary>
    /// <param name="type">BGMの種類</param>
    public void StopBGM(AudioType type)
    {
        StopSound(_bgmList, type);
    }

    /// <summary>
    /// 指定したインデックスBGMを止める
    /// フェードアウトあり
    /// </summary>
    /// <param name="type">BGMの種類</param>
    public async UniTask StopBGMWithFadeOut(AudioType type)
    {
        await StopSoundWithFadeOut(_bgmList, type);
    }

    /// <summary>
    /// 指定したインデックスBGMを止める
    /// フェードアウトあり
    /// </summary>
    /// <param name="type">BGMの種類</param>
    public async UniTask StopBGMWithFadeOut(AudioType type, float endVolume, float fadeOutSec)
    {
        await StopSoundWithFadeOut(_bgmList, type, endVolume, fadeOutSec);
    }
    #endregion

    #region SE操作
    /// <summary>
    /// 指定したインデックスのSEを鳴らす
    /// </summary>
    /// <param name="index">SEのインデックス</param>
    public AudioSource PlaySE(int index)   // 効果音を鳴らす(単発)
    {
        return PlaySound(_seList, index, _seMixier);
    }

    /// <summary>
    /// 指定したインデックスのSEを鳴らす
    /// フェードインあり
    /// </summary>
    /// <param name="index">SEのインデックス</param>
    public async UniTask<AudioSource> PlaySEWithFadeIn(int index)
    {
        return await PlaySoundWithFadeIn(_seList, index, _seMixier);
    }

    /// <summary>
    /// 指定したインデックスのSEを鳴らす
    /// フェードインあり
    /// </summary>
    /// <param name="index">SEのインデックス</param>
    public async UniTask<AudioSource> PlaySEWithFadeIn(int index, float startVolume, float endVolume, float fadeInSec)
    {
        return await PlaySoundWithFadeIn(_seList, index, _seMixier, startVolume, endVolume, fadeInSec);
    }

    /// <summary>
    /// 指定したインデックスSEを止める
    /// </summary>
    /// <param name="index">SEのインデックス</param>
    public void StopSE(int index)
    {
        StopSound(_seList, index);
    }

    /// <summary>
    /// 指定したインデックスSEを止める
    /// フェードアウトあり
    /// </summary>
    /// <param name="index">SEのインデックス</param>
    public async UniTask StopSEWithFadeOut(int index)
    {
        await StopSoundWithFadeOut(_seList, index);
    }

    /// <summary>
    /// 指定したインデックスSEを止める
    /// フェードアウトあり
    /// </summary>
    /// <param name="index">SEのインデックス</param>
    public async UniTask StopSEWithFadeOut(int index, float endVolume, float fadeOutSec)
    {
        await StopSoundWithFadeOut(_seList, index, endVolume, fadeOutSec);
    }

    /// <summary>
    /// 指定したインデックスのSEを鳴らす
    /// </summary>
    /// <param name="type">SEの種類</param>
    public AudioSource PlaySE(AudioType type)   // 効果音を鳴らす(単発)
    {
        return PlaySound(_seList, type, _seMixier);
    }

    /// <summary>
    /// 指定したインデックスのSEを鳴らす
    /// フェードインあり
    /// </summary>
    /// <param name="type">SEの種類</param>
    public async UniTask<AudioSource> PlaySEWithFadeIn(AudioType type)
    {
        return await PlaySoundWithFadeIn(_seList, type, _seMixier);
    }

    /// <summary>
    /// 指定したインデックスのSEを鳴らす
    /// フェードインあり
    /// </summary>
    /// <param name="type">SEの種類</param>
    public async UniTask<AudioSource> PlaySEWithFadeIn(AudioType type, float startVolume, float endVolume, float fadeInSec)
    {
        return await PlaySoundWithFadeIn(_seList, type, _seMixier, startVolume, endVolume, fadeInSec);
    }

    /// <summary>
    /// 指定したインデックスSEを止める
    /// </summary>
    /// <param name="type">SEの種類</param>
    public void StopSE(AudioType type)
    {
        StopSound(_seList, type);
    }

    /// <summary>
    /// 指定したインデックスSEを止める
    /// フェードアウトあり
    /// </summary>
    /// <param name="type">SEの種類</param>
    public async UniTask StopSEWithFadeOut(AudioType type)
    {
        await StopSoundWithFadeOut(_seList, type);
    }

    /// <summary>
    /// 指定したインデックスSEを止める
    /// フェードアウトあり
    /// </summary>
    /// <param name="type">SEの種類</param>
    public async UniTask StopSEWithFadeOut(AudioType type, float endVolume, float fadeOutSec)
    {
        await StopSoundWithFadeOut(_seList, type, endVolume, fadeOutSec);
    }
    #endregion

    #region VOICE操作
    /// <summary>
    /// 指定したインデックスのVoiceを鳴らす
    /// </summary>
    /// <param name="index">ボイスのインデックス</param>
    public AudioSource PlayVoice(int index)   // 効果音を鳴らす(単発)
    {
        return PlaySound(_voiceList, index, _voiceMixier);
    }

    /// <summary>
    /// 指定したインデックスのVoiceを鳴らす
    /// フェードインあり
    /// </summary>
    /// <param name="index">ボイスのインデックス</param>
    public async UniTask<AudioSource> PlayVoiceWithFadeIn(int index)
    {
        return await PlaySoundWithFadeIn(_voiceList, index, _voiceMixier);
    }

    /// <summary>
    /// 指定したインデックスのVoiceを鳴らす
    /// フェードインあり
    /// </summary>
    /// <param name="index">ボイスのインデックス</param>
    public async UniTask<AudioSource> PlayVoiceWithFadeIn(int index, float startVolume, float endVolume, float fadeInSec)
    {
        return await PlaySoundWithFadeIn(_voiceList, index, _voiceMixier, startVolume, endVolume, fadeInSec);
    }

    /// <summary>
    /// 指定したインデックスVoiceを止める
    /// </summary>
    /// <param name="index">ボイスのインデックス</param>
    public void StopVoice(int index)
    {
        StopSound(_voiceList, index);
    }

    /// <summary>
    /// 指定したインデックスVoiceを止める
    /// フェードアウトあり
    /// </summary>
    /// <param name="index">ボイスのインデックス</param>
    public async UniTask StopVoiceWithFadeOut(int index)
    {
        await StopSoundWithFadeOut(_voiceList, index);
    }

    /// <summary>
    /// 指定したインデックスVoiceを止める
    /// フェードアウトあり
    /// </summary>
    /// <param name="index">ボイスのインデックス</param>
    public async UniTask StopVoiceWithFadeOut(int index, float endVolume, float fadeOutSec)
    {
        await StopSoundWithFadeOut(_voiceList, index, endVolume, fadeOutSec);
    }

    /// <summary>
    /// 指定したインデックスのボイスを鳴らす
    /// </summary>
    /// <param name="type">ボイスの種類</param>
    public AudioSource PlayVoice(AudioType type)   // 効果音を鳴らす(単発)
    {
        return PlaySound(_voiceList, type, _voiceMixier);
    }

    /// <summary>
    /// 指定したインデックスのボイスを鳴らす
    /// フェードインあり
    /// </summary>
    /// <param name="type">ボイスの種類</param>
    public async UniTask<AudioSource> PlayVoiceWithFadeIn(AudioType type)
    {
        return await PlaySoundWithFadeIn(_voiceList, type, _voiceMixier);
    }

    /// <summary>
    /// 指定したインデックスのボイスを鳴らす
    /// フェードインあり
    /// </summary>
    /// <param name="type">ボイスの種類</param>
    public async UniTask<AudioSource> PlayVoiceWithFadeIn(AudioType type, float startVolume, float endVolume, float fadeInSec)
    {
        return await PlaySoundWithFadeIn(_voiceList, type, _voiceMixier, startVolume, endVolume, fadeInSec);
    }

    /// <summary>
    /// 指定したインデックスボイスを止める
    /// </summary>
    /// <param name="type">ボイスの種類</param>
    public void StopVoice(AudioType type)
    {
        StopSound(_voiceList, type);
    }

    /// <summary>
    /// 指定したインデックスボイスを止める
    /// フェードアウトあり
    /// </summary>
    /// <param name="type">ボイスの種類</param>
    public async UniTask StopVoiceWithFadeOut(AudioType type)
    {
        await StopSoundWithFadeOut(_voiceList, type);
    }

    /// <summary>
    /// 指定したインデックスボイスを止める
    /// フェードアウトあり
    /// </summary>
    /// <param name="type">ボイスの種類</param>
    public async UniTask StopVoiceWithFadeOut(AudioType type, float endVolume, float fadeOutSec)
    {
        await StopSoundWithFadeOut(_voiceList, type, endVolume, fadeOutSec);
    }
    #endregion

    #region 外部ソース
    /// <summary>
    /// 指定したインデックスのサウンドを鳴らす
    /// </summary>
    /// <param name="type">BGMのインデックス</param>
    public void PlaySound(AudioSource source, AudioClip clip)   // 効果音を鳴らす(単発)
    {
        OnPlaySound(source, clip, source.outputAudioMixerGroup);
    }

    /// <summary>
    /// 指定したインデックスのBGMを鳴らす
    /// フェードインあり
    /// </summary>
    /// <param name="type">BGMのインデックス</param>
    public async UniTask PlaySoundWithFadeIn(AudioSource source, AudioClip clip, float startVolume, float endVolume, float fadeInSec)
    {
        try
        {
            await OnPlaySoundWithFadeIn(source, clip, source.outputAudioMixerGroup, startVolume, endVolume, fadeInSec);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("サウンド再生での非同期キャンセル");
            return;
        }
    }

    /// <summary>
    /// 指定したインデックスBGMを止める
    /// </summary>
    /// <param name="type">BGMのインデックス</param>
    public void StopSound(AudioSource source)
    {
        OnStopSound(source);
    }

    /// <summary>
    /// 指定したインデックスBGMを止める
    /// フェードアウトあり
    /// </summary>
    /// <param name="type">BGMのインデックス</param>
    public async UniTask StopSoundWithFadeOut(AudioSource source, float endVolume, float fadeOutSec)
    {
        try
        {
            await OnStopSoundWithFadeOut(source, endVolume, fadeOutSec);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("サウンド再生での非同期キャンセル");
            return;
        }
    }
    #endregion
}