﻿using System;
using System.Linq;
using UnityEngine;

public class PotentialField
{
    private readonly MaxHeap<CellPotentialHeapEntry> _heap;

    public PotentialField(
        NavigationField navigationField,
        Target target)
    {
        Size = navigationField.fieldSize;
        NavigationField = navigationField;
        Target = target;
        Potentials = new FloatField(navigationField.fieldSize);
        Flows = new Vector2Field(navigationField.fieldSize);

        _heap =
             new MaxHeap<CellPotentialHeapEntry>
             {
                 RemoveAction = CellPotentialHeapEntry.ReturnCellCostHeapEntry
             };
    }

    public Vector2i Size { get; set; }
    public NavigationField NavigationField { get; set; }
    public FloatField Potentials { get; set; }
    public Vector2Field Flows { get; set; }
    public Action<Action<Vector2i, float>> PotentialSeeder { get; set; }
    public Target Target { get; set; }

    public float LastRequested { get; set; }
    public float LastUpdated { get; set; }
    public static PotentialField DebugInstance { get; set; }

    public const float UnreachablePotential = -5000;

    private static readonly Vector2iWithNormal DownLeft = new Vector2iWithNormal(new Vector2i(-1, -1));
    private static readonly Vector2iWithNormal DownRight = new Vector2iWithNormal(new Vector2i(1, -1));
    private static readonly Vector2iWithNormal UpLeft = new Vector2iWithNormal(new Vector2i(-1, 1));
    private static readonly Vector2iWithNormal UpRight = new Vector2iWithNormal(new Vector2i(1, 1));

    private static readonly Vector2iWithNormal Left = new Vector2iWithNormal(new Vector2i(-1, 0));
    private static readonly Vector2iWithNormal Right = new Vector2iWithNormal(new Vector2i(+1, 0));
    private static readonly Vector2iWithNormal Up = new Vector2iWithNormal(new Vector2i(0, 1));
    private static readonly Vector2iWithNormal Down = new Vector2iWithNormal(new Vector2i(0, -1));

    public void Populate()
    {
        LastUpdated = Time.time;
        Flows.Clear();
        SetPotentialsFromNavigationField();
        Target.SeedPotentials(AddTraversal);
        DebugInstance = this;
        while (_heap.Any())
        {
            CellPotentialHeapEntry cellPotentialHeapEntry = _heap.ExtractDominating();
            Vector2i position = cellPotentialHeapEntry.Position;

            float currentPotential = Potentials[position.x, position.y];

            if (currentPotential < cellPotentialHeapEntry.Potential)
            {
                continue;
            }

            Potentials[position.x, position.y] = cellPotentialHeapEntry.Potential;

            TryAddTraversal(position, DownLeft, currentPotential);
            TryAddTraversal(position, DownRight, currentPotential);
            TryAddTraversal(position, UpLeft, currentPotential);
            TryAddTraversal(position, UpRight, currentPotential);

            TryAddTraversal(position, Left, currentPotential);
            TryAddTraversal(position, Right, currentPotential);
            TryAddTraversal(position, Down, currentPotential);
            TryAddTraversal(position, Up, currentPotential);

            CellPotentialHeapEntry.ReturnCellCostHeapEntry(cellPotentialHeapEntry);
        }
    }

    public Vector2 GetSmoothFlow(Vector2 point)
    {
        //Vector2i position = Vector2i.FromVector2Trunc(rawPosition);
        //return Flows[position.x, position.y];
        Vector2i ipos = Vector2i.FromVector2Trunc(point);
        Vector2 fracs = new Vector2(point.x - ipos.x, point.y - ipos.y);

        Vector2 f00 = GetFlow(ipos);
        Vector2 f01 = GetFlow(ipos.Move(0, 1));
        Vector2 f10 = GetFlow(ipos.Move(1, 0));
        Vector2 f11 = GetFlow(ipos.Move(1, 1));
        if (f00 == Vector2.zero)
        {
            f00 = f01;
        }
        else if (f01 == Vector2.zero)
        {
            f01 = f00;
        }

        if (f10 == Vector2.zero)
        {
            f10 = f11;
        }
        else if (f11 == Vector2.zero)
        {
            f11 = f10;
        }

        Vector2 flow =
            Vector2.Lerp(
                Vector2.Lerp(f00, f01, fracs.y),
                Vector2.Lerp(f10, f11, fracs.y),
                fracs.x);

        return flow;
    }

    public Vector2 GetFlow(Vector2i point)
    {
        if (Size.ContainsAsSize(point))
        {
            return Flows[point.x, point.y];
        }
        else
        {
            return Vector2.zero;
        }
    }

    private void TryAddTraversal(Vector2i position, Vector2iWithNormal delta, float incomingPotential)
    {
        Vector2i destination = position + delta.vector2i;
        if (!Size.ContainsAsSize(destination))
        {
            return;
        }
        float currentPotential = Potentials[destination.x, destination.y];
        if (float.IsNegativeInfinity(currentPotential))
        {
            return;
        }

        float newPotential = incomingPotential - delta.magnitude;

        if (newPotential <= currentPotential)
        {
            return;
        }

        if (delta.isDiagonal)
        {
            // Only flow diagonally if it can step there cardinally in two steps. If both 
            // cardinal two-steps are blocked, there is no route.
            if (float.IsNegativeInfinity(Potentials[position.x + delta.vector2i.x, position.y])
                && float.IsNegativeInfinity(Potentials[position.x, position.y + delta.vector2i.y]))
            {
                return;
            }
        }

        //Gizmos.color = Color.black;
        //Gizmos.DrawRay(position.ToVector2() - Vector2.one * 0.5f, delta.normal * 0.8f);
        // This is the best flow we've found so far
        Flows[destination.x, destination.y] = -delta.normal;
        AddTraversal(destination, newPotential);
    }

    private void AddTraversal(Vector2i position, float potential)
    {
        float oldPotential = Potentials[position.x, position.y];

        if (float.IsNegativeInfinity(oldPotential))
        {
            // The darn cell is inpassible. Unpassable?
            return;
        }
        Potentials[position.x, position.y] = potential;
        _heap.Add(CellPotentialHeapEntry.GetPooled(position, potential));
    }

    private void SetPotentialsFromNavigationField()
    {
        FloatField costs = NavigationField.Costs;
        for (int y = 0; y < Size.y; y++)
        {
            for (int x = 0; x < Size.x; x++)
            {
                float cost = costs[x, y];

                if (float.IsPositiveInfinity(cost))
                {
                    Potentials[x, y] = float.NegativeInfinity;
                }
                else
                {
                    Potentials[x, y] = UnreachablePotential;
                }
            }
        }
    }

    private struct Vector2iWithNormal
    {
        public Vector2i vector2i;
        public Vector2 normal;
        public float magnitude;
        public bool isDiagonal;
        public Vector2iWithNormal(Vector2i vector2i)
            : this()
        {
            this.vector2i = vector2i;
            normal = vector2i.ToVector2().normalized;
            magnitude = vector2i.ToVector2().magnitude;
            isDiagonal = magnitude > 1;
            if (isDiagonal)
            {
                // Prefer diagonals...
                magnitude = magnitude * 0.96f;
            }
        }
    }
}

//public class NavigableField
//{
//    public NavigableField(Vector2i size)
//    {
//        Size = size;
//        Clear();
//    }

//    public Vector2i Size { get; private set; }
//    public float[,] Cost { get; private set; }

//    public void Populate()
//    {

//    }

//    private void Clear()
//    {
//        Cost = new float[Size.x, Size.y];
//    }
//}

//public class CostField
//{
//    public CostField(Vector2i size)
//    {
//        Size = size;
//        Clear();
//    }

//    public Vector2i Size { get; private set; }
//    public float[,] Cost { get; private set; }

//    public void Populate()
//    {

//    }

//    private void Clear()
//    {
//        Cost = new float[Size.x, Size.y];
//    }
//}