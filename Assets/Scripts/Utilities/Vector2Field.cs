using System;
using UnityEngine;

public class Vector2Field
{
    public Vector2Field(Vector2i size)
    {
        Size = size;
        Values = new Vector2[Size.x, Size.y];
        Clear();
    }

    public Vector2i Size { get; private set; }
    public Vector2[,] Values { get; private set; }

    public void Clear()
    {
        // 2 floats per flow
        Array.Clear(Values, 0, Size.x * Size.y);
    }

    public Vector2 this[int x, int y]
    {
        get { return Values[x, y]; }
        set { Values[x, y] = value; }
    }
}