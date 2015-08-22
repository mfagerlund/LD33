using System;
using CrowdPleaser.Utilities;

public class FloatField
{
    public FloatField(Vector2i size)
    {
        Size = size;
        Values = new float[Size.x, Size.y];
        Clear();
    }

    public Vector2i Size { get; private set; }
    public float[,] Values { get; private set; }

    public void Clear()
    {
        Array.Clear(Values, 0, Size.x * Size.y);
    }

    public float this[int x, int y]
    {
        get { return Values[x, y]; }
        set { Values[x, y] = value; }
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