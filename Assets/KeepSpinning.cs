using UnityEngine;

public class KeepSpinning : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    public float rotationSpeed = 240;
    public void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Update()
    {
        _spriteRenderer.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}
