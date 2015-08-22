using System.Reflection;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Agent : MonoBehaviour
{
    private Rigidbody2D _rigidbody2D;

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