using System;
using UnityEngine;

/// <summary>
/// 楽曲選択ディスプレイたちの回転処理を担当するクラス
/// </summary>
public class MusicDisplaysScroller : ArrangeedUIsScroller<MusicDisplayObj>
{
    // ディスプレイたちの状態を更新するコールバック
    // 初期配置時や角度の調整後に発火する
    public event Action<MusicDisplayObj> OnDisplaysUpdateCallBack;

    // 曲選択時に発火するコールバック
    public event Action<MusicDisplayObj> OnDecisionedMusicCallBack;

    protected override void OnClickUI(MusicDisplayObj ui)
    {
        OnDecisionedMusicCallBack?.Invoke(ui);
    }

    protected override void OnUpdateSelectedUI(MusicDisplayObj ui)
    {
        OnDisplaysUpdateCallBack?.Invoke(ui);
    }
}
