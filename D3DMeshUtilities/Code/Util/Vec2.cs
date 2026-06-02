using System;

namespace D3DMeshUtilities.Code.Util;

public class Vec2<T> : Vec<T>
{

    public T X
    {
        get => values[0];
        set => values[0] = value;
    }
    
    public T Y
    {
        get => values[1];
        set => values[1] = value;
    }
    
    public Vec2(T x, T y) : base([x, y])
    {
    }

    public Vec2(T[] vals) : base(vals)
    {
        if (vals.Length != 2)
        {
            throw new ArgumentOutOfRangeException(nameof(vals), "Must be exactly two");
        }
    }
}