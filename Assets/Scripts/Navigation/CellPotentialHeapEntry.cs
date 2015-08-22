using System;
using CrowdPleaser.Utilities;

public class CellPotentialHeapEntry : IComparable<CellPotentialHeapEntry>
{
    private static readonly ObjectPool<CellPotentialHeapEntry> Pool = new ObjectPool<CellPotentialHeapEntry>(() => new CellPotentialHeapEntry());
    private CellPotentialHeapEntry()
    {
    }

    public float Potential { get; set; }
    public Vector2i Position { get; set; }

    public static void ReturnCellCostHeapEntry(CellPotentialHeapEntry cellCostHeapEntry)
    {
        Pool.ReturnItem(cellCostHeapEntry);
    }

    public static CellPotentialHeapEntry GetPooled(Vector2i position, float potential)
    {
        return
            Pool
                .GetFromObjectPoolOrCreateItem(
                    item =>
                    {
                        item.Potential = potential;
                        item.Position = position;
                    });
    }

    public override string ToString()
    {
        return string.Format("{0} ({1})", Potential, Position);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }
        if (ReferenceEquals(this, obj))
        {
            return true;
        }
        if (obj.GetType() != this.GetType())
        {
            return false;
        }
        return Equals((CellPotentialHeapEntry)obj);
    }

    public override int GetHashCode()
    {
        return Position.GetHashCode();
    }

    public int CompareTo(CellPotentialHeapEntry other)
    {
        if (other.Potential > Potential)
        {
            return -1;
        }
        else if (other.Potential < Potential)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    protected bool Equals(CellPotentialHeapEntry other)
    {
        return Equals(Position, other.Position);
    }
}