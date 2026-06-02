using System;
using System.Runtime.CompilerServices;

namespace D3DMeshUtilities.Code.Util;

public static class MathUtils
{
    public static float Clamp01(float i)
    {
        return Math.Clamp(i, 0.0f, 1.0f);
    }

    public static float ClampA1(float i)
    {
        return Math.Clamp(i, -1.0f, 1.0f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ClampSF2SMax(float f) =>
        (short) Math.Clamp(f, -32768.0f, 32767.0);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ClampUF2SMax(float f) => 
        (ushort) Math.Clamp(f, 0.0f, 65535.0f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte ClampSF2BMax(float f) =>
        (sbyte) Math.Clamp(f, -128.0f, 127.0f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ClampUF2BMax(float f) =>
        (byte) Math.Clamp(f, 0.0f, 255.0f);

}