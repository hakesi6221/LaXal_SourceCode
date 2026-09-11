using System;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// メインリズムゲームを終了し、リザルト画面に移行するクラス
/// GameManager側で取得し、呼んでもらう想定
/// アニメーションを再生し、リザルト画面に移行
/// </summary>
public class RhythmGameFinisher : MonoBehaviour
{
    [SerializeField, Label("アニメーション設定")]
    private MonoBehaviourAnimSequencer _anims;

    [SerializeField, Label("メインメニューシーン名")]
    private string _mainMenuSceneName = string.Empty;

    [SerializeField, Label("リザルト画面シーン名")]
    private string _resultSceneName = string.Empty;

    [SerializeField, Label("アニメーションが終了してから何秒後にリザルト画面に移行するか")]
    private float _finishRhythmGameDelay = 3.0f;
    [SerializeField, Label("終了ボイス")]
    private MainGameVoice _mainGameVoice = new MainGameVoice();

    private void Start()
    {
        // アニメーションの状態初期化
        _anims.Initialize();
        GameManager.Instance?.AddGameFinishEvent(GameFinish);
    }

    /// <summary>
    /// ゲームを終了し、リザルト画面に移行する処理
    /// 非同期
    ///
    /// アニメーション再生後、指定秒待ってリザルト画面に移行する
    /// </summary>
    /// <returns></returns>
    private async void GameFinish()
    {
        var token = this.GetCancellationTokenOnDestroy();
        string sceneName = RhythmGameInfomation.Tutorial
                        ? _mainMenuSceneName
                        : _resultSceneName;

        _mainGameVoice.FinishGameVoice();
        try
        {
            await _anims.PlayAnimSequence(token);
            await UniTask.WaitForSeconds(_finishRhythmGameDelay
                                        , cancellationToken: token);
            SceneTransitionManager.Instance.ChangeSceneAsync(sceneName).Forget();
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("非同期処理のキャンセル");
            return;
        }
    }
}
