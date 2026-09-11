using UnityEngine;
using Unity.XR.XREAL;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// XREALホームメニューを管理する独自クラス
/// SDK側のではカバーできない処理があるため、作成
///
/// メインゲームとアウトゲームでのメニューのだし分けを行う
/// </summary>
public class HomeMenuObj : XREALHomeMenu
{
    [SerializeField]
    private GameObject _mainSceneMenu = null;
    [SerializeField]
    private GameObject _outGameMenu = null;

    [SerializeField]
    private List<string> _mainSceneNames = new List<string>();

    private void OnEnable()
    {
        // アクティブ時、現在のシーンに合わせたメニューを表示させる
        ShowMenuElement(true);
    }

    private void OnDisable()
    {
        // 非アクティブ時、現在のシーンに合わせたメニューを非表示に
        ShowMenuElement(false);
    }

    /// <summary>
    /// シーンの応じたメニュー画面の表示非表示を切り替える
    /// アクティブが切り替わるときに呼ぶ想定
    /// </summary>
    /// <param name="show">表示するか</param>
    public void ShowMenuElement(bool show)
    {
        if (_mainSceneNames.Contains(SceneManager.GetActiveScene().name))
        {
            _mainSceneMenu?.SetActive(show);
        }
        else
        {
            _outGameMenu?.SetActive(show);
        }
    }

    /// <summary>
    /// 基底クラスのShowを外部で呼ぶためのラッパー関数
    /// メニュー画面の開閉はここを通して行う想定
    /// </summary>
    /// <param name="show">表示するか</param>
    public new void Show(bool show)
    {
        base.Show(show);
    }
}
