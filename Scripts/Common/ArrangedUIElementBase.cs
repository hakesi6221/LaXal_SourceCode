using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class ArrangedUIElementBase : MonoBehaviour
{
    [SerializeField, Label("ImageのEventTrigger参照")]
    private EventTrigger _trigger;

    // 選択状態か
    private bool _isSelected = false;
    /// <summary>
    /// 選択状態か
    /// </summary>
    public bool IsSelected => _isSelected;

    /// <summary>
    /// 初期化処理
    /// 押下、および離したときの処理を登録する
    /// </summary>
    /// <param name="down">押下時の処理</param>
    /// <param name="up">離したときの処理</param>
    public void Initialize
    (
        Action<ArrangedUIElementBase, CancellationToken> down,
        Action<ArrangedUIElementBase, CancellationToken> up
    )
    {
        if (_trigger == null)
        {
            Debug.LogError($"{this.name}：[_trigger]がアタッチされていません。Inspectorを確認してください");
            return;
        }
        if (down == null)
        {
            Debug.LogError($"{this.name}：EventTrigger用のコールバックが正しくない形で渡されています。");
            return;
        }
        if (up == null)
        {
            Debug.LogError($"{this.name}：EventTrigger用のコールバックが正しくない形で渡されています。");
            return;
        }

        EventTrigger.Entry entryDown = new EventTrigger.Entry();
        entryDown.eventID = EventTriggerType.PointerDown;
        entryDown.callback.AddListener(_ => down.Invoke(this, this.GetCancellationTokenOnDestroy()));

        EventTrigger.Entry entryUp = new EventTrigger.Entry();
        entryUp.eventID = EventTriggerType.PointerUp;
        entryUp.callback.AddListener(_ => up.Invoke(this, this.GetCancellationTokenOnDestroy()));

        _trigger.triggers.Add(entryDown);
        _trigger.triggers.Add(entryUp);
    }

    /// <summary>
    /// 選択状態を更新する関数
    /// 可否変更を行う
    /// </summary>
    /// <param name="isSelected">選択状態かどうか</param>
    public void UpdateState(bool isSelected)
    {
        OnUpdateState(isSelected);
        _isSelected = isSelected;
    }

    /// <summary>
    /// 選択状態が更新されたときの処理
    /// </summary>
    /// <param name="isSelected">選択状態かどうか</param>
    protected abstract void OnUpdateState(bool isSelected);

    /// <summary>
    /// つかまれているかどうかが更新されたときの処理
    /// </summary>
    /// <param name="isGrabed">現在つかまれているか</param>
    public abstract void OnUpdateGrabState(bool isGrabed);
}
