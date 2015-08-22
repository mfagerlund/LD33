using UnityEngine;

public class UiFollow2D : MonoBehaviour
{
    public Transform follow;
    public float sizeExpansion = 1.05f;

    private RectTransform _rectTransform;
    private Collider2D _collider;

    public void Start()
    {
        _rectTransform = (RectTransform)transform;
    }

    public void FixedUpdate()
    {
        UpdatePosition();
    }

    public void LateUpdate()
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        if (follow != null)
        {
            _collider = _collider ?? follow.GetComponentInChildren<Collider2D>();
            _rectTransform.position = Camera.main.WorldToScreenPoint(follow.position);
            if (_collider != null)
            {
                _rectTransform.sizeDelta = Camera.main.WorldToScreenPoint(_collider.bounds.size)*sizeExpansion - Camera.main.WorldToScreenPoint(Vector2.zero);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
