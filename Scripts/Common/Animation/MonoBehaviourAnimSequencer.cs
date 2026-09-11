using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

[Serializable]
public class MonoBehaviourAnimSequencer
{
    [SerializeField]
    private AnimationSequenceElement[] _anims = null;


    /// <summary>
    /// アニメーションの流れをグループごとにQueueとして定義する関数
    /// Join：同じグループに追加する
    /// Append：新しいグループを作成する
    /// </summary>
    /// <returns></returns>
    private Queue<AnimationSeqenceElementBase[]> DefineStartAnimHandles()
    {
        Queue<AnimationSeqenceElementBase[]> handles = new Queue<AnimationSeqenceElementBase[]>();

        List<AnimationSeqenceElementBase> animHandles = new List<AnimationSeqenceElementBase>();
        foreach (var anim in _anims)
        {
            if (anim == null) continue;

            if (!animHandles.Any())
            {
                animHandles.Add(anim.Anim);
                continue;
            }

            if (anim.AnimType == GameStartAnimType.Join)
            {
                animHandles.Add(anim.Anim);
            }
            else if (anim.AnimType == GameStartAnimType.Append)
            {
                handles.Enqueue(animHandles.ToArray());
                animHandles = new List<AnimationSeqenceElementBase>();
                animHandles.Add(anim.Anim);
            }
        }
        handles.Enqueue(animHandles.ToArray());
        return handles;
    }

    /// <summary>
    /// 設定されたアニメーションオブジェクトたちの
    /// 初期化処理を行う関数
    /// </summary>
    public void Initialize()
    {
        if (_anims == null)
        {
            Debug.LogError($"{typeof(MonoBehaviourAnimSequencer).Name}:再生するアニメーションが存在しませんでした");
            return;
        }

        // アニメーションオブジェクトの初期化
        foreach (var anim in _anims)
        {
            if (anim == null)
            {
                Debug.LogError($"{typeof(MonoBehaviourAnimSequencer).Name}：[_startAnims]のいずれかがアタッチされていません。Inspectorを確認してください");
                return;
            }

            anim.Anim.Initialize();
        }
    }

    /// <summary>
    /// 設定されたアニメーションたちを順番に再生する
    /// </summary>
    /// <returns></returns>
    public async UniTask PlayAnimSequence(CancellationToken cancellationToken)
    {
        if (_anims == null)
        {
            Debug.LogError($"{typeof(MonoBehaviourAnimSequencer).Name}:再生するアニメーションが存在しませんでした");
            return;
        }

        // メインゲーム開始時の非同期処理の配列
        var anims = DefineStartAnimHandles();

        // アニメーションをグループごとに非同期で再生
        while (anims.Any())
        {
            var animHandles = anims.Dequeue();

            // グループのアニメーション処理を取り出し
            List<UniTask> handles = new List<UniTask>();
            foreach (var anim in animHandles)
            {
                handles.Add(anim.PlayAnimationAsync(cancellationToken));
            }

            // 実行後グループがすべて終わるまで待機
            try
            {
                await UniTask.WhenAll(handles.ToArray());
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("非同期処理がキャンセルされました。");
                return;
            }
        }
    }
}
