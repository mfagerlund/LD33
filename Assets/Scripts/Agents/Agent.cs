using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Agent : MonoBehaviour
{
    private Rigidbody2D _rigidbody2D;

    public Target Target { get; set; }
    public Vector2 Position { get { return _rigidbody2D.position; } set { _rigidbody2D.position = value; } }
    public bool Selected { get; set; }
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
        _rigidbody2D.velocity = Vector2.Lerp(_rigidbody2D.velocity, _wantedSpeed, Level.Instance.agentMomentum);
        //_rigidbody2D.position += _rigidbody2D.velocity * Time.deltaTime;
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