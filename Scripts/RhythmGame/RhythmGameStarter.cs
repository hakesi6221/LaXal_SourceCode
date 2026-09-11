using System;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// メインリズムゲームを開始するクラス
/// 生成時、GameManagerの初期化処理を呼び、アニメーションの後にゲーム開始処理を呼ぶ
/// </summary>
public class RhythmGameStarter : MonoBehaviour
{
    [SerializeField, Label("アニメーション設定")]
    private MonoBehaviourAnimSequencer _anims;

    [SerializeField, Label("アニメーション後何秒後にリズムゲームが開始されるか")]
    private float _startRhythmGameDelay = 3.0f;

    [SerializeField, Label("UI")]
    private GameStarterView _starterView = null;

    [SerializeField, Label("開始ボイス")]
    private MainGameVoice _mainGameVoice = new MainGameVoice();

    /// <summary>
    /// ゲーム開始時を宣言する処理
    /// 開始時のUIを表示し、一定時間後にメインゲームのUIを表示する
    /// </summary>
    /// <returns></returns>
    private async void GameStart()
    {
        var token = this.GetCancellationTokenOnDestroy();

        // 楽曲情報の反映
        if (_starterView == null)
            Debug.LogError($"{this.name}:[_starterView]がアタッチされていません。Inspectorを確認してください。");
        else
            _starterView.DisplayMusicInfos
            (
                RhythmGameInfomation.Jacket,
                RhythmGameInfomation.Difficulty,
                RhythmGameInfomation.Name
            );

        // ゲームの初期化
        GameManager.Instance.GameConstractor();

        // アニメーションの初期化
        _anims.Initialize();

        // 全てのアニメーションの再生
        try
        {
            await _anims.PlayAnimSequence(token);
            GameManager.Instance.OnStartRhythmGame(_startRhythmGameDelay);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("非同期処理のキャンセル");
            return;
        }

        //開始ボイスの再生
        _mainGameVoice.StartGameVoice();
    }

    void Start()
    {
        GameStart();
    }
}
