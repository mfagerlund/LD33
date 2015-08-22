using System.Linq;
using UnityEngine;

public class NavigationTool : Tool
{
    public override void RightClick(Vector2 position)
    {
        base.RightClick(position);
        if (SelectionManager.Instance.SelectedAgents.Any())
        {
            LocationTarget locationTarget =
                (LocationTarget)Object.Instantiate(AgentController.Instance.locationTargetPrefab, Vector2.zero, Quaternion.identity);

            locationTarget.transform.SetParent(SelectionManager.Instance.targetsParent);
            locationTarget.SetLocations(new[] { Vector2i.FromVector2Round(position) });
            SelectionManager.Instance.SelectedAgents.ForEach(a => a.Target = locationTarget);
        }
    }
}