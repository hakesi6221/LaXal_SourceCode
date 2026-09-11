using UnityEngine;

/// <summary>
/// ノーツの見た目関係処理を担当するクラス
/// ・距離に応じたαの更新
///
/// などを行う
/// </summary>
public class NoteView : MonoBehaviour
{
    [Header("コンポーネント参照")]
    [SerializeField]
    private SpriteRenderer[] _renderer = null;

    // ノーツのalphaが1になる判定ラインからの距離
    private float _notesVisuableDistance;

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <param name="visuableDistance">ノーツのalphaが1になる判定ラインからの距離</param>
    public void Initialize(float visuableDistance)
    {
        _notesVisuableDistance = visuableDistance;
    }

    /// <summary>
    /// 判定ラインとの距離に応じてSpriteRendererのα値を更新する関数
    /// Move側のUpdatePositionで呼ぶ想定
    /// </summary>
    /// <param name="distance">現在の距離</param>
    public void UpdateSpriteAlpha(float distance)
    {
        float alpha = (distance < _notesVisuableDistance) ? 1.0f : 0.0f;

        foreach (SpriteRenderer renderer in _renderer)
        {
            if (renderer == null)
            {
                continue;
            }

            Color color = renderer.color;
            color.a = alpha;
            renderer.color = color;
        }
    }

    public void SetColor(Color color)
    {
        foreach (SpriteRenderer renderer in _renderer)
        {
            if (renderer == null)
            {
                continue;
            }

            renderer.color = color;
        }
    }

    public void SetColorAlpha(float alpha)
    {
        foreach (SpriteRenderer renderer in _renderer)
        {
            if (renderer == null)
            {
                continue;
            }

            Color color = renderer.color;
            color.a = alpha;
            renderer.color = color;
        }
    }
}
