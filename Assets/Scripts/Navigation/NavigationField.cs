using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class NavigationField : MonoBehaviour
{
    public Vector2i fieldSize = new Vector2i(50, 50);
    public float gridSize = 1.0f;
    public LayerMask inpassable;

    private float _sqrt2 = Mathf.Sqrt(2);
    private Vector2 _halfGrid;
    private readonly UpdateTimer _updateTimer = new UpdateTimer(2);

    public static NavigationField Instance { get; private set; }

    public void Start()
    {
        Instance = this;
        _halfGrid = new Vector2(gridSize * 0.5f, gridSize * 0.5f);
        Costs = new FloatField(fieldSize);
        Populate();
    }

    public void FixedUpdate()
    {
        if (_updateTimer.ShouldUpdateNow())
        {
            Populate();
        }
    }

    public FloatField Costs { get; private set; }

    public float GetCost(Vector2i pos)
    {
        if (fieldSize.ContainsAsSize(pos))
        {
            return Costs[pos.x, pos.y];
        }
        else
        {
            return float.PositiveInfinity;
        }
    }

    public void Populate()
    {
        Costs.Clear();
        for (int y = 0; y < fieldSize.y; y++)
        {
            for (int x = 0; x < fieldSize.x; x++)
            {
                Vector2i p = new Vector2i(x, y);
                Rect rect = GetCellRect(p);
                Collider2D collider = Physics2D.OverlapArea(rect.min, rect.max, inpassable);
                if (collider != null)
                {
                    Costs[x, y] = float.PositiveInfinity;
                }
            }
        }
    }

    public void OnDrawGizmos()
    {
        //if (Costs == null)
        //{
        //    Start();
        //}

        //for (int y = 0; y < fieldSize.y; y++)
        //{
        //    for (int x = 0; x < fieldSize.x; x++)
        //    {
        //        Gizmos.color = new Color(1, 0.5f, 0.5f, 0.7f);

        //        Vector2i p = new Vector2i(x, y);
        //        Vector2 center = GetCellCenter(p);
        //        if (float.IsPositiveInfinity(Costs[x, y]))
        //        {
        //            Rect rect = GetCellRect(p);
        //            Gizmos.DrawCube(rect.center, rect.size * 0.95f);
        //        }
        //        else
        //        {
        //            Rect rect = GetCellRect(p);
        //            Gizmos.color = new Color(1, 1, 1, 0.3f);
        //            Gizmos.DrawCube(rect.center, rect.size * 0.95f);
        //        }

        //        if (PotentialField.DebugInstance != null)
        //        {
        //            float potential = PotentialField.DebugInstance.Potentials[x, y];
        //            if (potential > PotentialField.UnreachablePotential)
        //            {
        //                Handles.Label(center, potential.ToString("F2"));
        //            }

        //            Vector2 flow = PotentialField.DebugInstance.Flows[x, y];
        //            Gizmos.color = Color.black;
        //            Gizmos.DrawRay(center, flow * 0.5f);
        //        }
        //    }
        //}
    }

    private Vector2 GetCellCenter(Vector2i p)
    {
        Vector2 pos = p.ToVector2() * gridSize;
        return pos;
    }

    private Rect GetCellRect(Vector2i p)
    {
        Vector2 center = GetCellCenter(p);
        return new Rect(center.x - gridSize * 0.5f, center.y - gridSize * 0.5f, gridSize, gridSize);
    }
}