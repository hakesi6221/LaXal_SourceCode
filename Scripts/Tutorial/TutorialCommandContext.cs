using System.Linq;
using System.Threading;
using TMPro;

public class TutorialCommandContext
{
    private TutorialExplainInfos _explainInfos;
    private TutorialTextWindow _explainText;
    private SequentialVoicePlayer _voicePlayer = new SequentialVoicePlayer();

    public TutorialCommandContext(TutorialExplainInfos infos, TutorialTextWindow explainText)
    {
        _explainInfos = infos;
        _explainText = explainText;
    }

    public void FireCommand(int order, CancellationToken cancellationToken)
    {
        if (_explainText == null) return;
        if (_explainInfos == null) return;
        var explainInfos = _explainInfos.ExplainInfo.ToArray();
        if (explainInfos == null) return;
        if (explainInfos.Length < order) return;

        var info = DecisionInfo(order);
        _explainText.SetText(info.ExplainText);
        _voicePlayer.PlayVoiceSequential(SoundManager.Instance, info.ExplainVoies, cancellationToken);
    }

    private TutorialExplainInfo DecisionInfo(int order)
    {
        if (_explainInfos == null) return null;
        var explainInfos = _explainInfos.ExplainInfo.ToArray();
        if (explainInfos == null) return null;
        foreach (var info in explainInfos)
        {
            if (info == null) continue;

            if (info.Idx == order)
            {
                return info;
            }
        }

        return null;
    }
}