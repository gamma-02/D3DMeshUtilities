using System;

namespace D3DMeshUtilities.Code.Util;

public class Vec<T>
{
    public readonly T[] values;

    // public T X => values[0];
    // public T Y => values[1];
    // public T Z => values[2];
    // public T W => values[3];
    

    public Vec(T[] values)
    {
        this.values = values;
    }

    public static Vec<T1> Vec2<T1>(T1 x, T1 y)
    {
        return new Vec<T1>([x, y]);
    }

    public static Vec<T1> Vec3<T1>(T1 x, T1 y, T1 z)
    {
        return new Vec<T1>([x, y, z]);
    }

    public static Vec<T1> Vec4<T1>(T1 x, T1 y, T1 z, T1 w)
    {
        return new Vec<T1>([x, y, z, w]);
    }
    
    public T this[int index]
    {
        get
        {
            return values[index];
        }
        set
        {
            values[index] = value;
        }
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Vec<T> vec)
        {
            return false;
        }

        if (vec.values.Length != values.Length)
        {
            return false;
        }

        for (int i = 0; i < values.Length; i++)
        {
            if (!vec[i]!.Equals(this[i]))
            {
                return false;
            }
        }

        return true;
    }

    protected bool Equals(Vec<T> vec)
    {
        if (vec.values.Length != values.Length)
        {
            return false;
        }

        for (int i = 0; i < values.Length; i++)
        {
            if (!(vec[i]!.Equals(this[i])))
            {
                return false;
            }
        }

        return true;
    }

    public override int GetHashCode()
    {
        return values.GetHashCode();
    }
}