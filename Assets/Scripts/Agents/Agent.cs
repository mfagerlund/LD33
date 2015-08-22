using System.Reflection;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Agent : MonoBehaviour
{
    private Rigidbody2D _rigidbody2D;

    public Vector2 Position { get { return _rigidbody2D.position; } set { _rigidbody2D.position = value; } }
    public bool Selected { get; set; }

    public void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    public void Update()
    {
    }

    public void FixedUpdate()
    {
        _rigidbody2D.position += Vector2.one * Time.deltaTime * Level.Instance.maxAgentSpeed;
    }
}