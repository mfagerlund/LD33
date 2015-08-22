﻿using System;
using CrowdPleaser.Utilities;
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

    public void Start()
    {
        _halfGrid = new Vector2(gridSize * 0.5f, gridSize * 0.5f);
        Costs = new FloatField(fieldSize);
        Populate();
    }

    public FloatField Costs { get; private set; }

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
        if (Costs == null)
        {
            Start();
        }
        PotentialField potentialField =
            new PotentialField(
                fieldSize,
                this,
                PotentialPrimer);

        potentialField.Populate();

        for (int y = 0; y < fieldSize.y; y++)
        {
            for (int x = 0; x < fieldSize.x; x++)
            {
                Gizmos.color = new Color(1, 0.5f, 0.5f, 0.7f);
                Vector2i p = new Vector2i(x, y);
                if (float.IsPositiveInfinity(Costs[x, y]))
                {
                    Rect rect = GetCellRect(p);
                    Gizmos.DrawCube(rect.center, rect.size * 0.95f);
                }

                float potential = potentialField.Potentials[x, y];
                if (potential > PotentialField.UnreachablePotential)
                {
                    Vector2 center = GetCellCenter(p);
                    Handles.Label(center, potential.ToString("F2"));
                }
            }
        }
    }

    private void PotentialPrimer(Action<Vector2i, float> setPotential)
    {
        setPotential(new Vector2i(5, 5), 100);
        //setPotential(new Vector2i(30, 30), 100);
    }

    private Vector2 GetCellCenter(Vector2i p)
    {
        Vector2 pos = p.ToVector2() * gridSize - _halfGrid;
        return pos;
    }

    private Rect GetCellRect(Vector2i p)
    {
        Vector2 center = GetCellCenter(p);
        return new Rect(center.x - gridSize * 0.5f, center.y - gridSize * 0.5f, gridSize, gridSize);
    }
}