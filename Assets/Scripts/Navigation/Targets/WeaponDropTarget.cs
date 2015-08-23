using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WeaponDropTarget : Target
{
    public List<Vector2i> Locations { get; set; }
    public override void SeedPotentials()
    {
        Locations =
            Level
                .Instance
                .garbageHome
                .GetComponentsInChildren<WeaponDrop>()
                .Select(wd => Vector2i.FromVector2Round(wd.transform.position))
                .ToList();
        foreach (Vector2i location in Locations)
        {
            SetPotential(location);
        }
    }

    public override string ToString()
    {
        return "WeaponDrop";
    }

    public override bool IsAtTarget(Vector2 position, out Vector2 actualTarget)
    {
        Vector2i rounded = Vector2i.FromVector2Round(position);
        if (Locations != null && Locations.Contains(rounded))
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