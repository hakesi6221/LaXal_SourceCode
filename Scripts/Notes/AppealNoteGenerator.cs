using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class AppealNoteGenerator : MonoBehaviour
{
    [SerializeField, Label("アピール判定のノーツ")]
    private AppealNoteMove _appealNote = null;

    [SerializeField, Label("生成点")]
    private Transform _generatePos = null;

    private float _noteSpeed;

    private Vector3 _moveDirection = Vector3.back;

    private Vector3 _rotationOffset;

    // ノーツのalphaが1になる判定ラインからの距離
    private float _notesVisuableDistance;

    /// <summary>
    /// 更新：原
    /// 
    /// ノーツ生成者に担当の譜面csvをアタッチする初期化関数
    /// </summary>
    /// <param name="noteCSV"></param>
    public void Initialize
    (
        float notesSpeed,
        Vector3 noteMoveDirection,
        Vector3 noteRotationOffset,
        float notesVisuableDis
    )
    {
        _noteSpeed = notesSpeed;
        _moveDirection = noteMoveDirection;
        _rotationOffset = noteRotationOffset;
        _notesVisuableDistance = notesVisuableDis;
    }

    private float[] ExtractionGenerateElapsed(SeniorMoveNote[] seniorMoveNotes)
    {
        List<float> result = new List<float>();
        foreach (var note in seniorMoveNotes)
        {
            if (note == null) continue;

            if (!(note is SeniorMoveNoteEnterAppeal)) continue;

            SeniorMoveNoteEnterAppeal enterAppeal = note as SeniorMoveNoteEnterAppeal;
            result.Add((float)enterAppeal.AppealChanceJudgeSec);
        }

        return result.ToArray();
    }

    public void GenerateAppealNotes(SeniorMoveNote[] seniorMoveNotes)
    {
        if (seniorMoveNotes == null)
        {
            return;
        }
        if (_appealNote == null)
        {
            return;
        }

        float[] generateElapsed = ExtractionGenerateElapsed(seniorMoveNotes);
        foreach (float elapsed in generateElapsed)
        {

            Quaternion noteRotation = _generatePos.rotation * Quaternion.Euler(_rotationOffset);

            AppealNoteMove instance = Instantiate
            (
                _appealNote,
                _generatePos.position,
                noteRotation,
                _generatePos
            );
            instance.Initialize
            (
                _generatePos,
                elapsed,
                _noteSpeed,
                _moveDirection,
                -1, 
                -1,
                _notesVisuableDistance
            );
        }
    }
}