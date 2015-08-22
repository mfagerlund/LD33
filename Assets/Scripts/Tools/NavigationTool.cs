using System.Linq;
using UnityEngine;

public class NavigationTool : Tool
{
    public override void RightClick(Vector2 position)
    {
        base.RightClick(position);
        if (SelectionManager.Instance.SelectedControlledAgents.Any())
        {
            Vector2i? pos = GetActualPosition(position);

            if (!pos.HasValue)
            {
                // Insantiate bad selection!
                return;
            }

            // Instantiate good selection!
            LocationTarget locationTarget = (LocationTarget)Object.Instantiate(AgentController.Instance.locationTargetPrefab, Vector2.one * -100, Quaternion.identity);
            locationTarget.transform.SetParent(SelectionManager.Instance.targetsParent);
            locationTarget.SetLocations(new[] { pos.Value });
            SelectionManager.Instance.SelectedAgents.ForEach(a => a.Target = locationTarget);
        }
    }

    private static Vector2i? GetActualPosition(Vector2 position)
    {
        NavigationField navigationField = NavigationField.Instance;
        Vector2i pos = Vector2i.FromVector2Round(position);
        if (!navigationField.fieldSize.ContainsAsSize(pos))
        {
            return null;
        }

        if (!float.IsPositiveInfinity(navigationField.Costs[pos.x, pos.y]))
        {
            return pos;
        }

        // Try to find a nice neighbour...
        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                Vector2i test = new Vector2i(pos.x + x, pos.y + y);
                if (!float.IsPositiveInfinity(navigationField.GetCost(test)))
                {
                    return test;
                }
            }
        }

        return null;
    }
}