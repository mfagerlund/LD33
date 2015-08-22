using System;
using UnityEngine;

public abstract class Target : MonoBehaviour
{
    public const float DefaultPotential = 100;
    private Action<Vector2i, float> _setPotential;

    public void SeedPotentials(Action<Vector2i, float> setPotential)
    {
        _setPotential = setPotential;
        SeedPotentials();
    }

    public void SetPotential(Vector2i position, float potential = DefaultPotential)
    {
        _setPotential(position, potential);
    }

    public virtual void Update()
    {
        if (!NavigationHandler.Instance.GetIsTargetActive(this))
        {
            Destroy(gameObject);
        }
    }

    public abstract void SeedPotentials();

    public Vector2 GetFlowToTarget(Vector2 position)
    {
        PotentialField potentialField = NavigationHandler.Instance.GetPotentialField(this);
        return potentialField.GetSmoothFlow(position);
    }
}