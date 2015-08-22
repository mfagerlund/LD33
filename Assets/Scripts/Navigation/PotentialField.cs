using System;
using System.Linq;
using CrowdPleaser.Utilities;
using UnityEngine;

public class PotentialField
{
    private readonly MaxHeap<CellPotentialHeapEntry> _heap;

    public PotentialField(
        Vector2i size,
        NavigationField navigationField,
        Action<Action<Vector2i, float>> potentialPrimer)
    {
        Size = size;
        NavigationField = navigationField;
        PotentialPrimer = potentialPrimer;
        Potentials = new FloatField(size);

        _heap =
             new MaxHeap<CellPotentialHeapEntry>
             {
                 RemoveAction = CellPotentialHeapEntry.ReturnCellCostHeapEntry
             };
    }

    public Vector2i Size { get; set; }
    public NavigationField NavigationField { get; set; }
    public FloatField Potentials { get; set; }
    public Action<Action<Vector2i, float>> PotentialPrimer { get; set; }

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
        SetPotentialsFromNavigationField();
        PotentialPrimer(AddTraversal);

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

            TryAddTraversal(position, Left, currentPotential);
            TryAddTraversal(position, Right, currentPotential);
            TryAddTraversal(position, Down, currentPotential);
            TryAddTraversal(position, Up, currentPotential);

            CellPotentialHeapEntry.ReturnCellCostHeapEntry(cellPotentialHeapEntry);
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

        if (newPotential > currentPotential)
        {
            AddTraversal(destination, newPotential);
        }
    }

    private void AddTraversal(Vector2i position, float potential)
    {
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

                if (float.IsNegativeInfinity(cost))
                {
                    Potentials[x, y] = float.NegativeInfinity;
                }
                else
                {
                    Potentials[x, y] = -50;
                }
            }
        }
    }

    private struct Vector2iWithNormal
    {
        public Vector2i vector2i;
        public Vector2 normal;
        public float magnitude;
        public Vector2iWithNormal(Vector2i vector2i)
            : this()
        {
            this.vector2i = vector2i;
            normal = vector2i.ToVector2().normalized;
            magnitude = normal.magnitude;
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