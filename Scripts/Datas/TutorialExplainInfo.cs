using System;
using UnityEngine;

[Serializable]
public class TutorialExplainInfo
{
    [SerializeField]
    private int _idx = 1;

    [SerializeField, TextArea(4, 6)]
    private string _explainText = string.Empty;

    [SerializeField]
    private AudioType[] _explainVoices = null;


    public int Idx => _idx;
    public string ExplainText => _explainText;
    public AudioType[] ExplainVoies => _explainVoices;
}
