using UnityEngine;

public class LongNote : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _longNote;

    private float _length;

    public void SetLength(float length)
    {
        _length = length;

        float parentYScale = transform.parent.localScale.y;
        Vector2 size = _longNote.size;
        size.y = length / parentYScale;
        _longNote.size = size;
    }

    public Vector3 GetTailPosition()
    {
        return transform.position + transform.up * _length;
    }
}