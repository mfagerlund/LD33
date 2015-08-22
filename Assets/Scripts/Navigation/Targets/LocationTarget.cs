using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LocationTarget : Target
{
    private RectTransform _rectTransform;
    public HashSet<Vector2i> Locations { get; set; }

    public void SetLocations(IEnumerable<Vector2i> locations)
    {
        Locations = new HashSet<Vector2i>(locations);
    }

    public void Start()
    {
        _rectTransform = (RectTransform)transform;
    }

    public override void Update()
    {
        base.Update();
        _rectTransform.position = Camera.main.WorldToScreenPoint(Locations.First().ToVector2());
    }

    public override void SeedPotentials()
    {
        foreach (Vector2i location in Locations)
        {
            SetPotential(location);
        }
    }

    public override string ToString()
    {
        return "LocationTarget: " + string.Join(", ", Locations.Select(c => c.ToString()).ToArray());
    }

    protected override bool IsAtTarget(Vector2 position, out Vector2 actualTarget)
    {
        Vector2i rounded = Vector2i.FromVector2Round(position);
        if (Locations.Contains(rounded))
        {
            actualTarget = rounded.ToVector2();
            return true;
        }
        else
        {
            actualTarget = Vector2.zero; 
            return false;
        }
    }
}