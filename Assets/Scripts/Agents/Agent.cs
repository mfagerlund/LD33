using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Agent : MonoBehaviour
{
    private Rigidbody2D _rigidbody2D;

    public Target Target { get; set; }
    public Vector2 Position { get { return _rigidbody2D.position; } set { _rigidbody2D.position = value; } }
    public Vector2 Velocity { get; set; }
    public bool Selected { get; set; }

    public const float AgentRadius = 0.3f;

    private Vector2 _wantedSpeed;

    public void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    public void Update()
    {
        GoToTarget();
    }

    public void FixedUpdate()
    {
        Velocity = Vector2.Lerp(_wantedSpeed, Velocity, Level.Instance.agentMomentum);
        _rigidbody2D.position += Velocity * Time.deltaTime;
    }

    private void GoToTarget()
    {
        if (Target != null)
        {
            Vector2 flow = Target.GetFlowToTarget(Position);
            _wantedSpeed = flow * Level.Instance.agentMaxSpeed;
        }
        else
        {
            _wantedSpeed = Vector2.zero;
        }
    }
}