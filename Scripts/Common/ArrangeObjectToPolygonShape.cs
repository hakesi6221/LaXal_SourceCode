using UnityEngine;

/// <summary>
/// 子オブジェクトをその数の角を持つ多角形の頂点に配置するクラス
/// </summary>
public class ArrangeObjectToPolygonShape<T> where T : MonoBehaviour
{
    // 親のTransform
    private Transform _tranform;

    // 直径
    private float _diameter;

    // 角の広さ
    private float _range;

    // 初期の正面Index
    private int _frontIndex;

    // X軸の傾き
    private float _slopeX;

    // z軸の傾き
    private float _slopeZ;

    // 中心角の一角
    private float _centerPartAngle;

    /// <summary>
    /// 親のTransform
    /// </summary>
    public Transform Root => _tranform;

    /// <summary>
    /// 直径
    /// </summary>
    public float Diameter => _diameter;

    /// <summary>
    /// X軸の傾き
    /// </summary>
    public float SlopeX => _slopeX;

    /// <summary>
    /// z軸の傾き
    /// </summary>
    public float SlopeZ => _slopeZ;

    /// <summary>
    /// 中心角
    /// </summary>
    public float CenterPartAngle => _centerPartAngle;

    /// <summary>
    /// 子オブジェクトをその数の角を持つ多角形の頂点に配置するクラス
    /// </summary>
    /// <param name="root">親のTransform</param>
    /// <param name="diameter">直径</param>
    /// <param name="range">角の広さ</param>
    /// <param name="sloapX">X軸の傾き</param>
    /// <param name="slopeZ">z軸の傾き</param>
    public ArrangeObjectToPolygonShape(Transform root
                                        , float diameter
                                        , float range
                                        , int frontIndex = 0
                                        , float sloapX = 0f
                                        , float slopeZ = 0f)
    {
        _tranform = root;
        _diameter = diameter;
        _range = range;
        _frontIndex = frontIndex;
        _slopeX = sloapX;
        _slopeZ = slopeZ;
    }

    /// <summary>
    /// 初期化関数
    /// Inspectorで指定された角度分このオブジェクトを傾け、
    /// 子オブジェクトたちを多角形の方に並べる
    /// </summary>
    public T[] ArrangeObjects()
    {
        // Quaternion lookRot = Quaternion.LookRotation(Camera.main.transform.position - this.transform.position, Vector3.up);
        Quaternion offsetRotZ = Quaternion.AngleAxis(_slopeZ, Vector3.forward);
        Quaternion offsetRotX = Quaternion.AngleAxis(_slopeX, Vector3.right);
        _tranform.rotation = _tranform.rotation * offsetRotZ * offsetRotX;

        return InitializeObjectsPos();
    }

    private T[] InitializeObjectsPos()
    {
        // 子オブジェクトたち
        var children = _tranform.GetComponentsInChildren<T>();
        // 子オブジェクトの数
        int childCount = children.Length;
        // 内角の大きさ
        _centerPartAngle  = _range / childCount;

        // 一個前に配置したもののローカル座標
        Vector3 prevLocalPos = Vector3.zero;
        // 配置する角度
        float targetAngle = 90f;
        float radius = _diameter / 2;
        for (int r = 0; r < childCount; r++)
        {
            // 今見ているオブジェクト
            Transform obj = children[r].transform;

            // targetAngleをベクトルに変換
            Vector3 targetVec = new Vector3
            (
                Mathf.Cos(targetAngle * Mathf.Deg2Rad),
                0f,
                Mathf.Sin(targetAngle * Mathf.Deg2Rad)
            );
            // 配置場所の確定、配置処理
            var position = targetVec * radius;
            SetChildFirstRotAndPos(obj, position);

            // 配置角度を加算
            targetAngle += _centerPartAngle;
        }

        if (childCount <= _frontIndex) _frontIndex = 0;
        Vector3 forwardVec = children[_frontIndex].transform.localPosition.normalized;
        Quaternion forwardAngle = Quaternion.LookRotation(forwardVec);
        forwardAngle = Quaternion.Inverse(forwardAngle);
        _tranform.localRotation = forwardAngle;

        return children;
    }

    private void SetChildFirstRotAndPos(Transform transform, Vector3 localPosition)
    {
        transform.localPosition = localPosition;
        transform.LookAt(this._tranform);
        Vector3 eulerAngles = transform.localEulerAngles;
        eulerAngles.y -= 180f;
        transform.localEulerAngles = eulerAngles;
    }
}
