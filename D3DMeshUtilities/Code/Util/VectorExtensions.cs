using System.Numerics;

namespace D3DMeshUtilities.Code.Util;

public static class VectorExtensions
{
    // public static Vector<int> Create(int x)
    // {
    //     Vector<int> vec = new Vector<int>(0);
    //     
    // }
    //
    public static T X<T>(this Vector<T> vector)
    {
        return vector[0];
    }

    public static T Y<T>(this Vector<T> vector)
    {
        return vector[1];
    }
    
    public static T Z<T>(this Vector<T> vector)
    {
        return vector[2];
    }

    public static T W<T>(this Vector<T> vector)
    {
        return vector[3];
    }
}