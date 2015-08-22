using CrowdPleaser.Utilities;
using UnityEngine;

[ExecuteInEditMode]
public class NavigationField : MonoBehaviour
{
    public Vector2i fieldSize = new Vector2i(50, 50);
    public float gridSize = 1.0f;
    public LayerMask inpassable;

    private FloatField _field;
    private float _sqrt2 = Mathf.Sqrt(2);

    public void Start()
    {
        _field = new FloatField(fieldSize);
        Populate();
    }

    public void Populate()
    {
        _field.Clear();
        for (int y = 0; y < fieldSize.y; y++)
        {
            for (int x = 0; x < fieldSize.x; x++)
            {
                Vector2i p = new Vector2i(x, y);
                Rect rect = GetCellRect(p);
                Collider2D collider = Physics2D.OverlapArea(rect.min, rect.max, inpassable);
                if (collider != null)
                {
                    _field[x, y] = float.PositiveInfinity;
                }
            }
        }
    }

    public void OnDrawGizmos()
    {
        if (_field == null)
        {
            Start();
        }
        Gizmos.color = Color.red;
        for (int y = 0; y < fieldSize.y; y++)
        {
            for (int x = 0; x < fieldSize.x; x++)
            {
                if (float.IsPositiveInfinity(_field[x, y]))
                {
                    Vector2i p = new Vector2i(x, y);
                    Rect rect = GetCellRect(p);
                    Gizmos.DrawCube(rect.center, rect.size * 0.95f);
                }
            }
        }
    }

    private Vector2 GetCellCenter(Vector2i p)
    {
        Vector2 pos = p.ToVector2() * gridSize;
        return pos;
    }


    private Rect GetCellRect(Vector2i p)
    {
        Vector2 center = GetCellCenter(p);
        return new Rect(center, Vector2.one * gridSize);
    }
}