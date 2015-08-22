using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NavigationHandler : MonoBehaviour
{
    private readonly List<PotentialField> _potentialFields = new List<PotentialField>();
    public float updateInterval = 2;
    public float cullDelay = 0.1f;

    public static NavigationHandler Instance { get; private set; }
    public void Start()
    {
        Instance = this;
    }

    public void Update()
    {
        Instance = this;

        foreach (PotentialField potentialField in _potentialFields.ToArray())
        {
            if (Time.time - potentialField.LastRequested > cullDelay)
            {
                //Debug.LogFormat("Dropping potentialField for {0}", potentialField.Target);
                _potentialFields.Remove(potentialField);
            }
        }
    }

    public PotentialField GetPotentialField(Target target)
    {
        PotentialField potentialField = _potentialFields.SingleOrDefault(p => p.Target == target);

        if (potentialField == null)
        {
            potentialField = new PotentialField(NavigationField.Instance, target);
            _potentialFields.Add(potentialField);
        }

        potentialField.LastRequested = Time.time;

        if (Time.time - potentialField.LastUpdated >= updateInterval)
        {
            //Debug.LogFormat("potentialField.Populate for {0}", potentialField.Target);
            potentialField.Populate();
        }

        return potentialField;
    }

    public bool GetIsTargetActive(Target target)
    {
        return _potentialFields.Any(pf => pf.Target == target);
    }
}