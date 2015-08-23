using System;
using UnityEngine;

public abstract class Target : MonoBehaviour
{
    public const float DefaultPotential = 100;
    private Action<Vector2i, float> _setPotential;

    public bool destroyOnInactive = true;

    public void SeedPotentials(Action<Vector2i, float> setPotential)
    {
        _setPotential = setPotential;
        SeedPotentials();
    }

    public bool HasArrived { get; set; }

    public void SetPotential(Vector2i position, float potential = DefaultPotential)
    {
        _setPotential(position, potential);
    }

    public virtual void Update()
    {
        if (destroyOnInactive && !NavigationHandler.Instance.GetIsTargetActive(this))
        {
            Destroy(gameObject);
        }
    }

    public abstract void SeedPotentials();

    public Vector2 GetFlowToTarget(Vector2 position)
    {
        Vector2 actualTarget;
        if (IsAtTarget(position, out actualTarget))
        {
            Vector2 delta = actualTarget - position;
            return delta * 0.1f;
        }
        PotentialField potentialField = NavigationHandler.Instance.GetPotentialField(this);
        return potentialField.GetSmoothFlow(position);
    }

    public float GetWalkingDistanceFrom(Vector2 position)
    {
        PotentialField potentialField = NavigationHandler.Instance.GetPotentialField(this);
        return potentialField.GetDistanceFrom(position);
    }


    public abstract bool IsAtTarget(Vector2 position, out Vector2 actualTarget);
}