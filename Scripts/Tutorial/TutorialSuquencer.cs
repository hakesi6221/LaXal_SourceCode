using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

public class TutorialSuquencer : MonoBehaviour
{
    [SerializeField, Label("チュートリアル動作CSV")]
    private TextAsset _tutorialCSV = null;

    [SerializeField, Label("チュートリアル説明アセット")]
    private TutorialExplainInfos _tutorialExplainInfos = null;

    [SerializeField, Label("チュートリアル説明表示テキストウィンドウ")]
    private TutorialTextWindow _explainText = null;

    private Stack<TutorialCommandNote> _commandStack = new Stack<TutorialCommandNote>();

    private TutorialDataLoader _loader = new TutorialDataLoader();

    private TutorialCommandContext _context;

    void OnEnable()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (!RhythmGameInfomation.Tutorial) return;

        _context = new TutorialCommandContext(_tutorialExplainInfos, _explainText);
        _commandStack = _loader.LoadTutorialData(_tutorialCSV, _context);
        GameManager.Instance.AddGameStartEvent(StartTutorial);
    }

    /// <summary>
    /// 移動処理のカウントの開始
    /// </summary>
    public void StartTutorial()
    {
        if (!_commandStack.Any()) return;

        _explainText.OnStartTutorial(RhythmGameInfomation.Tutorial);
        // アピール用のノーツも生成
        foreach (var command in _commandStack)
        {
            command.MoveOrderWithCount(this.GetCancellationTokenOnDestroy()).Forget();
        }
    }
}
