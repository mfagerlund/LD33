using UnityEngine;

public class UpdateTimer
{
    private readonly float _updateInterval;
    private float _lastUpdatedAt;

    public UpdateTimer(float updateInterval)
    {
        _updateInterval = updateInterval;
        _lastUpdatedAt = 0;
    }

    public bool ShouldUpdateNow()
    {
        bool shouldUpdate = (Time.time - _lastUpdatedAt) > _updateInterval;
        if (shouldUpdate)
        {
            _lastUpdatedAt = Time.time;
            return true;
        }
        return false;
    }
}