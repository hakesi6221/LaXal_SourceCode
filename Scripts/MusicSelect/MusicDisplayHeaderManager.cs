using System;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// 楽曲ディスプレイの先頭を管理するクラス
/// 枚数は8枚程度から膨らまない想定なのでUpdateで確認
/// 更新が行われた場合、更新時のイベントを発火する
/// </summary>
public class MusicDisplayHeaderManager : MonoBehaviour
{
    [SerializeField, Label("先頭ポジション")]
    private Transform _headerPosition = null;

    // 楽曲ディスプレイの配列（仕様では8）
    private MusicDisplayObj[] _displays = null;

    // 現在の先頭
    private MusicDisplayObj _currentHeaderDisplay = null;

    // 先頭判定を行う閾値
    private const float HEADER_DISTANCE_THRESHOLD = 0.2f;

    // 更新時の処理
    public event Action<MusicDisplayObj> OnUpdateEvent;

    /// <summary>
    /// 初期化処理
    /// 全ての楽曲ディスプレイの参照を配列で取得
    /// </summary>
    /// <param name="displays"></param>
    public void Initialize(MusicDisplayObj[] displays)
    {
        _displays = displays;
        _currentHeaderDisplay = _displays.FirstOrDefault();
    }

    /// <summary>
    /// それぞれのディスプレイの距離を確認する
    /// 条件を満たすものがあれば先頭のものの更新を行う
    /// </summary>
    private void FindHeaderDisplay()
    {
        if (_displays == null)
        {
            Debug.LogError($"{this.name}:ディスプレイの配列が存在しません");
            return;
        }
        if (_headerPosition == null)
        {
            Debug.LogError($"{this.name}:先頭の目安が存在しません");
            return;
        }

        // 最も先頭目安に近いディスプレイと距離の箱
        MusicDisplayObj closestDisplay = null;
        float closestDistance = float.MaxValue;
        // 全てのディスプレイで回す
        foreach (MusicDisplayObj display in _displays)
        {
            if (display == null) continue;

            // 最も先頭目安に近いディスプレイとその距離を保持
            float distance = Vector3.Distance
            (
                display.transform.position,
                _headerPosition.position
            );

            if (distance < closestDistance)
            {
                closestDisplay = display;
                closestDistance = distance;
            }
        }

        // 閾値の範囲内であれば、先頭のディスプレイを更新
        if (closestDistance <= HEADER_DISTANCE_THRESHOLD)
            UpdateHeaderDisplay(closestDisplay);
    }

    /// <summary>
    /// 先頭ディスプレイの更新処理
    /// 直前のものと異なるものに変わるのなら、それ用の処理を発火
    /// </summary>
    /// <param name="musicDisplay">該当した楽曲ディスプレイ</param>
    private void UpdateHeaderDisplay(MusicDisplayObj musicDisplay)
    {
        if (musicDisplay == null) return;

        // まだ先頭のディスプレイが登録されていないか、直前までのものと違うなら更新時の処理を呼ぶ
        if (_currentHeaderDisplay != null
            && _currentHeaderDisplay != musicDisplay)
            OnUpdateEvent?.Invoke(musicDisplay);
        _currentHeaderDisplay = musicDisplay;
    }

    private void Update()
    {
        FindHeaderDisplay();
    }
}
