using UnityEngine;
using LitMotion;
using LitMotion.Extensions;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using Unity.VisualScripting.Antlr3.Runtime;
using NaughtyAttributes;
using Unity.VisualScripting;
using Unity.Collections;
using UnityEngine.UIElements;

/// <summary>
/// Enemyの動きを管理するクラス
/// </summary>
public class EnemyMove : MonoBehaviour
{
    [SerializeField, Label("レーン間移動の所要時間"), BoxGroup("MoveParameters")]
    private float _durationSecMov;

    [SerializeField, Label("デフォルトのY座標"), BoxGroup("MoveParameters")]
    private float _defaultPosY = -4f;

    [SerializeField, Label("デフォルトのZ座標"), BoxGroup("MoveParameters")]
    private float _defaultPosZ = -4f;

    [SerializeField, Label("ジャンプしたときの最高高度と通常時の高さとの差"), BoxGroup("MoveParameters")]
    private float _distanceByJumpMov;

    [SerializeField, Label("アピールの所要時間"), BoxGroup("MoveParameters")]
    private float _durationSecAP;

    [SerializeField, Label("ジャンプしたときの最高高度と通常時の高さとの差"), BoxGroup("MoveParameters")]
    private float _distanceByJumpAP;
    private PlayerMove _move = null;

    void Start()
    {
        Initialize();
        // GameManager.Instance?.AddOnJudgeEvent(JumpEnemy);
    }



    private void Initialize()
    {
        Vector3 initialPos = Vector3.zero;
        if (GameManager.Instance != null)
            initialPos.y = _defaultPosY;
            initialPos.z = _defaultPosZ;
        transform.position = initialPos;
        _move = new PlayerMove(transform, _durationSecMov, _distanceByJumpMov, _durationSecAP, _distanceByJumpAP);
    }

    private void JumpEnemy(JudgeResult result)
    {
        //Debug.Log("JumpEnemyが呼ばれました");
        if (_move.IsMove)
            return;
        // TODO：回転アニメーションを作成&Viewを通して再生
        var token = this.GetCancellationTokenOnDestroy();

        _move.AppealOnLane(token);
    }
}
