using System;

namespace D3DMeshUtilities.Code.Util;

public class Vec4<T> : Vec<T>
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
    
    public T W
    {
        get => values[3];
        set => values[3] = value;
    }

    public Vec4(T x, T y, T z, T w) : base([x, y, z, w])
    {
        
    }
    
    public Vec4(T[] vals) : base(vals)
    {
        if (vals.Length != 4)
        {
            throw new ArgumentOutOfRangeException(nameof(vals), "Must be exactly four");
        }
    }
    
}