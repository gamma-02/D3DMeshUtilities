using System;

namespace D3DMeshUtilities.Code.Util;

public class Vec3<T> : Vec<T>
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

    public T Z
    {
        get => values[2];
        set => values[2] = value;
    }
    
    public Vec3(T x, T y, T z) : base([x, y, z])
    {
    }
    
    public Vec3(T[] vals) : base(vals)
    {
        if (vals.Length != 3)
        {
            throw new ArgumentOutOfRangeException(nameof(vals), "Must be exactly three");
        }
    }
}