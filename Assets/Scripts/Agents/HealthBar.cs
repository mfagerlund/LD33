using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Gradient gradient;

    private Agent _agent;
    private SpriteRenderer _spriteRenderer;

    public void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _agent = GetComponentInParent<Agent>();
    }

    public void Update()
    {
        float healthPercentage = _agent.health / _agent.maxHealth;
        if (healthPercentage > 0.99)
        {
            _spriteRenderer.enabled = false;
            return;
        }

        _spriteRenderer.enabled = true;
        Color color = gradient.Evaluate(healthPercentage);
        float alpha = MathfStuff.Map(healthPercentage, 0.8f, 1.0f, 0.7f, 0.0f);

        color.a = alpha;
        _spriteRenderer.color = color;
    }
}