using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AgentTypeTarget : Target
{
    public HashSet<Vector2i> Locations { get; set; }
    public AgentType agentType;

    public override void SeedPotentials()
    {
        foreach (Agent agent in Level.Instance.GetAgents().Where(a => a.agentType == agentType))
        {
            SetPotential(Vector2i.FromVector2Round(agent.Position));
        }
    }

    public override string ToString()
    {
        return "AgentType: " + agentType.ToString();
    }

    public override bool IsAtTarget(Vector2 position, out Vector2 actualTarget)
    {
        // Can we every really arrive at our enemies?
        actualTarget = Vector2.zero;
        return false;
        //Vector2i rounded = Vector2i.FromVector2Round(position);
        //if (Locations.Contains(rounded))
        //{
        //    actualTarget = rounded.ToVector2();
        //    return true;
        //}
        //else
        //{
        //    actualTarget = Vector2.zero;
        //    return false;
        //}
    }
}