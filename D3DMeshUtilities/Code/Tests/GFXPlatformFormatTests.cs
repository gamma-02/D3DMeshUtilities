using System;
using System.Numerics;
using System.Runtime.InteropServices.ComTypes;
using D3DMeshUtilities.Code.Util;
using Xunit;

namespace D3DMeshUtilities.Code.Tests;

public static class GFXPlatformFormatTests
{
    
    private static void AssertEqual<T>(Func<Span<Byte>, T, bool> write, Func<ReadOnlySpan<Byte>, T> read, T val, int sizeOfT = 16)
    {
        var buffer = new byte[sizeOfT];
        write.Invoke(buffer, val);
        Assert.Equal(val, read.Invoke(buffer));
    }

    private static void AssertNearlyEqual(Func<Span<Byte>, float, bool> write, Func<ReadOnlySpan<Byte>, float> read,
        float val, int sizeOfT = 16, float epsilon = 0.001f)
    {
        var buffer = new byte[sizeOfT];
        write.Invoke(buffer, val);
        float readVal = read.Invoke(buffer);
        Assert.True(MathF.Abs(readVal - val) <= epsilon, $"Is: {readVal} not close enough to: {val}");
    }

    private static void AssertNearlyEqual(Func<Span<Byte>, Vector2, bool> write, Func<ReadOnlySpan<Byte>, Vector2> read,
        Vector2 val, int sizeOfT = 16, float epsilon = 0.001f)
    {
        var buffer = new byte[sizeOfT];
        write.Invoke(buffer, val);
        Vector2 res = read.Invoke(buffer);
        Assert.True(MathF.Abs(res.X - val.X) <= epsilon, $"X is: {res.X} not close enough to: {val.X}");
        Assert.True(MathF.Abs(res.Y - val.Y) <= epsilon, $"Y is: {res.Y} not close enough to: {val.Y}");
    }
    
    private static void AssertNearlyEqual(Func<Span<Byte>, Vector3, bool> write, Func<ReadOnlySpan<Byte>, Vector3> read,
        Vector3 val, int sizeOfT = 16, float epsilon = 0.001f)
    {
        var buffer = new byte[sizeOfT];
        write.Invoke(buffer, val);
        Vector3 res = read.Invoke(buffer);
        Assert.True(MathF.Abs(res.X - val.X) <= epsilon, $"X is: {res.X} not close enough to: {val.X}");
        Assert.True(MathF.Abs(res.Y - val.Y) <= epsilon, $"Y is: {res.Y} not close enough to: {val.Y}");
        Assert.True(MathF.Abs(res.Z - val.Z) <= epsilon, $"Z is: {res.Z} not close enough to: {val.Z}");
    }
    
    private static void AssertNearlyEqual(Func<Span<Byte>, Vector4, bool> write, Func<ReadOnlySpan<Byte>, Vector4> read,
        Vector4 val, int sizeOfT = 16, float epsilon = 0.001f)
    {
        var buffer = new byte[sizeOfT];
        write.Invoke(buffer, val);
        Vector4 res = read.Invoke(buffer);
        Assert.True(MathF.Abs(res.X - val.X) <= epsilon, $"X is: {res.X} not close enough to: {val.X}");
        Assert.True(MathF.Abs(res.Y - val.Y) <= epsilon, $"Y is: {res.Y} not close enough to: {val.Y}");
        Assert.True(MathF.Abs(res.Z - val.Z) <= epsilon, $"Z is: {res.Z} not close enough to: {val.Z}");
        Assert.True(MathF.Abs(res.W - val.W) <= epsilon, $"W is: {res.W} not close enough to: {val.W}");
    }
    
    private static void AssertWLessNearlyEqual(Func<Span<Byte>, Vector4, bool> write, Func<ReadOnlySpan<Byte>, Vector4> read,
        Vector4 val, int sizeOfT = 16, float epsilon = 0.002f, float wEpsilon = 0.33333334f)
    {
        var buffer = new byte[sizeOfT];
        write.Invoke(buffer, val);
        Vector4 res = read.Invoke(buffer);
        Assert.True(MathF.Abs(res.X - val.X) <= epsilon, $"X is: {res.X} not close enough to: {val.X}");
        Assert.True(MathF.Abs(res.Y - val.Y) <= epsilon, $"Y is: {res.Y} not close enough to: {val.Y}");
        Assert.True(MathF.Abs(res.Z - val.Z) <= epsilon, $"Z is: {res.Z} not close enough to: {val.Z}");
        Assert.True(MathF.Abs(res.W - val.W) <= wEpsilon, $"W is: {res.W} not close enough to: {val.W}");
    }

    // private static Vector<int> Vec2si(int x, int y) => new (new[] { x, y });
    // private static Vector<uint> Vec2ui(uint x, uint y) => new (new[] { x, y });
    private static Vec2<T> Vec2<T>(T x, T y) => new ([x, y]);
    private static Vec3<T> Vec3<T>(T x, T y, T z) => new ([x, y, z]);
    private static Vec4<T> Vec4<T>(T x, T y, T z, T w) => new ([x, y, z, w]);

    #region F32
    
    [Fact]
    public static void F32_1() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteF32,
        ConvertFromGfxPlatformFormat.ReadF32,
        1.0f,
        8
        );
    
    [Fact]
    public static void F32_0() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteF32,
        ConvertFromGfxPlatformFormat.ReadF32,
        0.0f,
        8
    );

    [Fact]
    public static void F32_PosInf() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteF32,
        ConvertFromGfxPlatformFormat.ReadF32, 
        float.PositiveInfinity, 
        8
        );

    [Fact]
    public static void F32_NegInf() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteF32,
        ConvertFromGfxPlatformFormat.ReadF32,
        float.NegativeInfinity,
        8
        );

    [Fact]
    public static void F32_Rand10()
    {
        for (int i = 0; i < 10; i++)
        {
            float rand = Random.Shared.NextSingle();
            rand *= Random.Shared.Next(1000);
            
            AssertEqual(
                ConvertToGFXPlatformFormat.WriteF32,
                ConvertFromGfxPlatformFormat.ReadF32,
                rand,
                8
            );
        }
    }
    
    #endregion

    #region F32x2

    
    [Fact]
    public static void F32x2_11() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteF32x2,
        ConvertFromGfxPlatformFormat.ReadF32x2,
        new Vector2(1.0f, 1.0f),
        8
    );
    
    [Fact]
    public static void F32x2_00() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteF32x2,
        ConvertFromGfxPlatformFormat.ReadF32x2,
        new Vector2(0.0f, 0.0f),
        8
    );

    [Fact]
    public static void F32x2_pInfnInf() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteF32x2,
        ConvertFromGfxPlatformFormat.ReadF32x2, 
        new Vector2(float.PositiveInfinity, float.NegativeInfinity), 
        8
    );
    
    [Fact]
    public static void F32x2_nInfpInf() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteF32x2,
        ConvertFromGfxPlatformFormat.ReadF32x2, 
        new Vector2(float.NegativeInfinity, float.PositiveInfinity), 
        8
    );
    

    [Fact]
    public static void F32x2_RandxRand10()
    {
        for (int i = 0; i < 10; i++)
        {
            float randX = Random.Shared.NextSingle();
            randX *= Random.Shared.Next(1000);
            
            float randY = Random.Shared.NextSingle();
            randY *= Random.Shared.Next(1000);
            
            AssertEqual(
                ConvertToGFXPlatformFormat.WriteF32x2,
                ConvertFromGfxPlatformFormat.ReadF32x2,
                new Vector2(randX, randY),
                8
            );
        }
    }

    #endregion

    #region F32x3

    [Fact]
    public static void F32x3_111() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteF32x3,
        ConvertFromGfxPlatformFormat.ReadF32x3,
        new Vector3(1.0f, 1.0f, 1.0f)
    );
    
    [Fact]
    public static void F32x3_000() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteF32x3,
        ConvertFromGfxPlatformFormat.ReadF32x3,
        new Vector3(0.0f, 0.0f, 0.0f)
    );

    [Fact]
    public static void F32x3_pInfnInfpInf() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteF32x3,
        ConvertFromGfxPlatformFormat.ReadF32x3, 
        new Vector3(float.PositiveInfinity, float.NegativeInfinity, float.PositiveInfinity)
    );
    
    [Fact]
    public static void F32x3_nInfpInfnInf() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteF32x3,
        ConvertFromGfxPlatformFormat.ReadF32x3, 
        new Vector3(float.NegativeInfinity, float.PositiveInfinity, float.NegativeInfinity)
    );
    

    [Fact]
    public static void F32x3_RandxRandxRand10()
    {
        for (int i = 0; i < 10; i++)
        {
            float randX = Random.Shared.NextSingle();
            randX *= Random.Shared.Next(1000);
            
            float randY = Random.Shared.NextSingle();
            randY *= Random.Shared.Next(1000);
            
            float randZ = Random.Shared.NextSingle();
            randZ *= Random.Shared.Next(1000);
            
            AssertEqual(
                ConvertToGFXPlatformFormat.WriteF32x3,
                ConvertFromGfxPlatformFormat.ReadF32x3,
                new Vector3(randX, randY, randZ)
            );
        }
    }

    #endregion

    #region F32x4

    [Fact]
    public static void F32x4_1111() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteF32x4,
        ConvertFromGfxPlatformFormat.ReadF32x4,
        new Vector4(1.0f, 1.0f, 1.0f, 1.0f)
    );
    
    [Fact]
    public static void F32x4_0000() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteF32x4,
        ConvertFromGfxPlatformFormat.ReadF32x4,
        new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
    );

    [Fact]
    public static void F32x4_pInfnInfpInfnInf() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteF32x4,
        ConvertFromGfxPlatformFormat.ReadF32x4, 
        new Vector4(float.PositiveInfinity, float.NegativeInfinity, float.PositiveInfinity, float.NegativeInfinity)
    );
    
    [Fact]
    public static void F32x4_nInfpInfnInfpInf() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteF32x4,
        ConvertFromGfxPlatformFormat.ReadF32x4, 
        new Vector4(float.NegativeInfinity, float.PositiveInfinity, float.NegativeInfinity, float.PositiveInfinity)
    );
    

    [Fact]
    public static void F32x4_RandxRandxRandxRand10()
    {
        for (int i = 0; i < 10; i++)
        {
            float randX = Random.Shared.NextSingle();
            randX *= Random.Shared.Next(1000);
            
            float randY = Random.Shared.NextSingle();
            randY *= Random.Shared.Next(1000);
            
            float randZ = Random.Shared.NextSingle();
            randZ *= Random.Shared.Next(1000);
            
            float randW = Random.Shared.NextSingle();
            randW *= Random.Shared.Next(1000);
            
            AssertEqual(
                ConvertToGFXPlatformFormat.WriteF32x4,
                ConvertFromGfxPlatformFormat.ReadF32x4,
                new Vector4(randX, randY, randZ, randW)
            );
        }
    }

    #endregion

    #region F16x2

    [Fact]
    public static void F16x2_11() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteF16x2,
        ConvertFromGfxPlatformFormat.ReadF16x2,
        new Vector2(1.0f, 1.0f),
        4
    );
    
    [Fact]
    public static void F16x2_00() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteF16x2,
        ConvertFromGfxPlatformFormat.ReadF16x2,
        new Vector2(0.0f, 0.0f),
        4
    );

    [Fact]
    public static void F16x2_pInfnInf() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteF16x2,
        ConvertFromGfxPlatformFormat.ReadF16x2, 
        new Vector2(float.PositiveInfinity, float.NegativeInfinity), 
        4
    );
    
    [Fact]
    public static void F16x2_nInfpInf() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteF16x2,
        ConvertFromGfxPlatformFormat.ReadF16x2, 
        new Vector2(float.NegativeInfinity, float.PositiveInfinity), 
        4
    );
    

    [Fact]
    public static void F16x2_RandxRand10()
    {
        for (int i = 0; i < 10; i++)
        {
            float randX = Random.Shared.NextSingle();
            randX *= Random.Shared.Next(100);
            
            float randY = Random.Shared.NextSingle();
            randY *= Random.Shared.Next(100);

            Half castX = (Half)randX;
            Half castY = (Half)randY;
            
            
            
            AssertEqual(
                ConvertToGFXPlatformFormat.WriteF16x2,
                ConvertFromGfxPlatformFormat.ReadF16x2,
                new Vector2((float)castX, (float)castY),
                4
            );
        }
    }

    #endregion

    #region F16x4

    [Fact]
    public static void F16x4_1111() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteF16x4,
        ConvertFromGfxPlatformFormat.ReadF16x4,
        new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
        8
    );
    
    [Fact]
    public static void F16x4_0000() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteF16x4,
        ConvertFromGfxPlatformFormat.ReadF16x4,
        new Vector4(0.0f, 0.0f, 0.0f, 0.0f),
        8
    );

    [Fact]
    public static void F16x4_pInfnInfpInfnInf() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteF16x4,
        ConvertFromGfxPlatformFormat.ReadF16x4, 
        new Vector4(float.PositiveInfinity, float.NegativeInfinity, float.PositiveInfinity, float.NegativeInfinity), 
        8
    );
    
    [Fact]
    public static void F16x4_nInfpInfnInfpInf() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteF16x4,
        ConvertFromGfxPlatformFormat.ReadF16x4, 
        new Vector4(float.NegativeInfinity, float.PositiveInfinity, float.NegativeInfinity, float.PositiveInfinity), 
        8
    );
    

    [Fact]
    public static void F16x4_RandxRandxRandxRand10()
    {
        for (int i = 0; i < 10; i++)
        {
            float randX = Random.Shared.NextSingle();
            randX *= Random.Shared.Next(100);
            
            float randY = Random.Shared.NextSingle();
            randY *= Random.Shared.Next(100);
            
            float randZ = Random.Shared.NextSingle();
            randZ *= Random.Shared.Next(100);
            
            float randW = Random.Shared.NextSingle();
            randW *= Random.Shared.Next(100);

            Half castX = (Half)randX;
            Half castY = (Half)randY;
            Half castZ = (Half)randZ;
            Half castW = (Half)randW;
            
            
            
            AssertEqual(
                ConvertToGFXPlatformFormat.WriteF16x4,
                ConvertFromGfxPlatformFormat.ReadF16x4,
                new Vector4((float)castX, (float)castY, (float)castZ, (float)castW),
                8
            );
        }
    }

    #endregion

    #region S/U32

    [Fact]
    public static void S32_1() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS32,
        ConvertFromGfxPlatformFormat.ReadS32,
        1,
        4
    );
    
    [Fact]
    public static void U32_1() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteU32,
        ConvertFromGfxPlatformFormat.ReadU32,
        1u,
        4
    );
    
    [Fact]
    public static void S32_0() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS32,
        ConvertFromGfxPlatformFormat.ReadS32,
        0,
        4
    );
    
    [Fact]
    public static void U32_0() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteU32,
        ConvertFromGfxPlatformFormat.ReadU32,
        0u,
        4
    );

    [Fact]
    public static void S32_Max() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS32,
        ConvertFromGfxPlatformFormat.ReadS32, 
        int.MaxValue, 
        4
    );
    
    [Fact]
    public static void U32_Max() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteU32,
        ConvertFromGfxPlatformFormat.ReadU32, 
        uint.MaxValue, 
        4
    );

    [Fact]
    public static void S32_Min() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS32,
        ConvertFromGfxPlatformFormat.ReadS32,
        int.MinValue,
        4
    );

    [Fact]
    public static void S32_Rand10()
    {
        for (int i = 0; i < 10; i++)
        {
            int rand = Random.Shared.Next();
            
            AssertEqual(
                ConvertToGFXPlatformFormat.WriteS32,
                ConvertFromGfxPlatformFormat.ReadS32,
                rand,
                4
            );
        }
    }
    
    [Fact]
    public static void U32_Rand16()
    {
        for (int i = 0; i < 16; i++)
        {
            uint rand = (uint)Random.Shared.Next();
            
            AssertEqual(
                ConvertToGFXPlatformFormat.WriteU32,
                ConvertFromGfxPlatformFormat.ReadU32,
                rand,
                4
            );
        }
    }

    #endregion

    #region S/U32x2

    [Fact]
    public static void S32x2_11() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS32x2,
        ConvertFromGfxPlatformFormat.ReadS32x2,
        Vec2(1, 1),
        8
    );
    
    [Fact]
    public static void U32x2_11() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteU32x2,
        ConvertFromGfxPlatformFormat.ReadU32x2,
        Vec2(1u, 1u),
        8
    );
    
    [Fact]
    public static void S32x2_00() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS32x2,
        ConvertFromGfxPlatformFormat.ReadS32x2,
        Vec2(0, 0),
        8
    );
    
    [Fact]
    public static void U32x2_00() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteU32x2,
        ConvertFromGfxPlatformFormat.ReadU32x2,
        Vec2(0u, 0u),
        8
    );

    [Fact]
    public static void S32x2_MaxMin() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS32x2,
        ConvertFromGfxPlatformFormat.ReadS32x2, 
        Vec2(int.MaxValue, int.MinValue), 
        8
    );
    
    [Fact]
    public static void S32x2_MinMax() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS32x2,
        ConvertFromGfxPlatformFormat.ReadS32x2, 
        Vec2(int.MinValue, int.MaxValue), 
        8
    );
    
    [Fact]
    public static void U32x2_MaxMax() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteU32x2,
        ConvertFromGfxPlatformFormat.ReadU32x2, 
        Vec2(uint.MaxValue, uint.MaxValue), 
        8
    );

    [Fact]
    public static void S32x2_RandxRand10()
    {
        for (int i = 0; i < 10; i++)
        {
            int randX = Random.Shared.Next();
            int randY = Random.Shared.Next();
            
            AssertEqual(
                ConvertToGFXPlatformFormat.WriteS32x2,
                ConvertFromGfxPlatformFormat.ReadS32x2,
                Vec2(randX, randY),
                8
            );
        }
    }
    
    [Fact]
    public static void U32x2_RandxRand16()
    {
        for (int i = 0; i < 15; i++)
        {
            uint randX = (uint) Random.Shared.Next();
            uint randY = (uint) Random.Shared.Next();
            
            AssertEqual(
                ConvertToGFXPlatformFormat.WriteU32x2,
                ConvertFromGfxPlatformFormat.ReadU32x2,
                Vec2(randX, randY),
                8
            );
        }
    }

    #endregion

    #region S/U32x3

    [Fact]
    public static void S32x3_111() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS32x3,
        ConvertFromGfxPlatformFormat.ReadS32x3,
        Vec3(1, 1, 1),
        12
    );
    
    [Fact]
    public static void U32x3_111() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteU32x3,
        ConvertFromGfxPlatformFormat.ReadU32x3,
        Vec3(1u, 1u, 1u),
        12
    );
    
    [Fact]
    public static void S32x3_000() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS32x3,
        ConvertFromGfxPlatformFormat.ReadS32x3,
        Vec3(0, 0, 0),
        12
    );
    
    [Fact]
    public static void U32x3_000() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteU32x3,
        ConvertFromGfxPlatformFormat.ReadU32x3,
        Vec3(0u, 0u, 0u),
        12
    );

    [Fact]
    public static void S32x3_MaxMinMax() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS32x3,
        ConvertFromGfxPlatformFormat.ReadS32x3, 
        Vec3(int.MaxValue, int.MinValue, int.MaxValue), 
        12
    );
    
    [Fact]
    public static void S32x3_MinMaxMin() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS32x3,
        ConvertFromGfxPlatformFormat.ReadS32x3, 
        Vec3(int.MinValue, int.MaxValue, int.MinValue), 
        12
    );
    
    [Fact]
    public static void U32x3_MaxMaxMax() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteU32x3,
        ConvertFromGfxPlatformFormat.ReadU32x3, 
        Vec3(uint.MaxValue, uint.MaxValue, uint.MaxValue), 
        12
    );

    [Fact]
    public static void S32x3_RandxRandxRand10()
    {
        for (int i = 0; i < 10; i++)
        {
            int randX = Random.Shared.Next();
            int randY = Random.Shared.Next();
            int randZ = Random.Shared.Next();
            
            AssertEqual(
                ConvertToGFXPlatformFormat.WriteS32x3,
                ConvertFromGfxPlatformFormat.ReadS32x3,
                Vec3(randX, randY, randZ),
                12
            );
        }
    }
    
    [Fact]
    public static void U32x3_RandxRandxRand16()
    {
        for (int i = 0; i < 15; i++)
        {
            uint randX = (uint) Random.Shared.Next();
            uint randY = (uint) Random.Shared.Next();
            uint randZ = (uint) Random.Shared.Next();
            
            AssertEqual(
                ConvertToGFXPlatformFormat.WriteU32x3,
                ConvertFromGfxPlatformFormat.ReadU32x3,
                Vec3(randX, randY, randZ),
                12
            );
        }
    }

    #endregion

    #region S/U32x4

    [Fact]
    public static void S32x4_111() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS32x4,
        ConvertFromGfxPlatformFormat.ReadS32x4,
        Vec4(1, 1, 1, 1),
        16
    );
    
    [Fact]
    public static void U32x4_111() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteU32x4,
        ConvertFromGfxPlatformFormat.ReadU32x4,
        Vec4(1u, 1u, 1u, 1u),
        16
    );
    
    [Fact]
    public static void S32x4_000() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS32x4,
        ConvertFromGfxPlatformFormat.ReadS32x4,
        Vec4(0, 0, 0, 0),
        16
    );
    
    [Fact]
    public static void U32x4_000() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteU32x4,
        ConvertFromGfxPlatformFormat.ReadU32x4,
        Vec4(0u, 0u, 0u, 0u),
        16
    );

    [Fact]
    public static void S32x4_MaxMinMaxMin() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS32x4,
        ConvertFromGfxPlatformFormat.ReadS32x4, 
        Vec4(int.MaxValue, int.MinValue, int.MaxValue, int.MinValue), 
        16
    );
    
    [Fact]
    public static void S32x4_MinMaxMinMax() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS32x4,
        ConvertFromGfxPlatformFormat.ReadS32x4, 
        Vec4(int.MinValue, int.MaxValue, int.MinValue, int.MaxValue), 
        16
    );
    
    [Fact]
    public static void U32x4_MaxMaxMaxMax() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteU32x4,
        ConvertFromGfxPlatformFormat.ReadU32x4, 
        Vec4(uint.MaxValue, uint.MaxValue, uint.MaxValue, uint.MaxValue), 
        16
    );

    [Fact]
    public static void S32x4_RandxRandxRand10()
    {
        for (int i = 0; i < 10; i++)
        {
            int randX = Random.Shared.Next();
            int randY = Random.Shared.Next();
            int randZ = Random.Shared.Next();
            int randW = Random.Shared.Next();
            
            AssertEqual(
                ConvertToGFXPlatformFormat.WriteS32x4,
                ConvertFromGfxPlatformFormat.ReadS32x4,
                Vec4(randX, randY, randZ, randW),
                16
            );
        }
    }
    
    [Fact]
    public static void U32x4_RandxRandxRand16()
    {
        for (int i = 0; i < 15; i++)
        {
            uint randX = (uint) Random.Shared.Next();
            uint randY = (uint) Random.Shared.Next();
            uint randZ = (uint) Random.Shared.Next();
            uint randW = (uint) Random.Shared.Next();
            
            AssertEqual(
                ConvertToGFXPlatformFormat.WriteU32x4,
                ConvertFromGfxPlatformFormat.ReadU32x4,
                Vec4(randX, randY, randZ, randW),
                16
            );
        }
    }

    #endregion

    #region S/U16

    [Fact]
    public static void S16_1() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS16,
        ConvertFromGfxPlatformFormat.ReadS16,
        (short)1,
        2
    );
    
    [Fact]
    public static void U16_1() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteU16,
        ConvertFromGfxPlatformFormat.ReadU16,
        (ushort)1,
        2
    );
    
    [Fact]
    public static void S16_0() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS16,
        ConvertFromGfxPlatformFormat.ReadS16,
        (short)0,
        2
    );
    
    [Fact]
    public static void U16_0() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteU16,
        ConvertFromGfxPlatformFormat.ReadU16,
        (ushort)0,
        4
    );

    [Fact]
    public static void S16_Max() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS16,
        ConvertFromGfxPlatformFormat.ReadS16, 
        short.MaxValue, 
        4
    );
    
    [Fact]
    public static void U16_Max() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteU16,
        ConvertFromGfxPlatformFormat.ReadU16, 
        ushort.MaxValue, 
        4
    );

    [Fact]
    public static void S16_Min() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS16,
        ConvertFromGfxPlatformFormat.ReadS16,
        short.MinValue,
        4
    );

    [Fact]
    public static void S16_Rand10()
    {
        for (int i = 0; i < 10; i++)
        {
            short rand = (short)Random.Shared.Next(short.MinValue, short.MaxValue);
            
            AssertEqual(
                ConvertToGFXPlatformFormat.WriteS16,
                ConvertFromGfxPlatformFormat.ReadS16,
                rand,
                4
            );
        }
    }
    
    [Fact]
    public static void U16_Rand16()
    {
        for (int i = 0; i < 16; i++)
        {
            ushort rand = (ushort)Random.Shared.Next(ushort.MaxValue);
            
            AssertEqual(
                ConvertToGFXPlatformFormat.WriteU16,
                ConvertFromGfxPlatformFormat.ReadU16,
                rand,
                4
            );
        }
    }

    #endregion

    #region S/U16x2

    
    [Fact]
    public static void S16x2_11() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS16x2,
        ConvertFromGfxPlatformFormat.ReadS16x2,
        Vec2((short)1, (short)1),
        4
    );
    
    [Fact]
    public static void U16x2_11() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteU16x2,
        ConvertFromGfxPlatformFormat.ReadU16x2,
        Vec2((ushort)1u, (ushort)1u),
        4
    );
    
    [Fact]
    public static void S16x2_00() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS16x2,
        ConvertFromGfxPlatformFormat.ReadS16x2,
        Vec2((short)0, (short)0),
        4
    );
    
    [Fact]
    public static void U16x2_00() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteU16x2,
        ConvertFromGfxPlatformFormat.ReadU16x2,
        Vec2((ushort)0u, (ushort)0u),
        4
    );

    [Fact]
    public static void S16x2_MaxMin() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS16x2,
        ConvertFromGfxPlatformFormat.ReadS16x2, 
        Vec2(short.MaxValue, short.MinValue), 
        4
    );
    
    [Fact]
    public static void S16x2_MinMax() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS16x2,
        ConvertFromGfxPlatformFormat.ReadS16x2, 
        Vec2(short.MinValue, short.MaxValue), 
        4
    );
    
    [Fact]
    public static void U16x2_MaxMax() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteU16x2,
        ConvertFromGfxPlatformFormat.ReadU16x2, 
        Vec2(ushort.MaxValue, ushort.MaxValue), 
        4
    );

    [Fact]
    public static void S16x2_RandxRand10()
    {
        for (int i = 0; i < 10; i++)
        {
            short randX = (short)Random.Shared.Next(short.MinValue, short.MaxValue);
            short randY = (short)Random.Shared.Next(short.MinValue, short.MaxValue);
            
            AssertEqual(
                ConvertToGFXPlatformFormat.WriteS16x2,
                ConvertFromGfxPlatformFormat.ReadS16x2,
                Vec2(randX, randY),
                4
            );
        }
    }
    
    [Fact]
    public static void U16x2_RandxRand16()
    {
        for (int i = 0; i < 15; i++)
        {
            ushort randX = (ushort) Random.Shared.Next(ushort.MaxValue);
            ushort randY = (ushort) Random.Shared.Next(ushort.MaxValue);
            
            AssertEqual(
                ConvertToGFXPlatformFormat.WriteU16x2,
                ConvertFromGfxPlatformFormat.ReadU16x2,
                Vec2(randX, randY),
                4
            );
        }
    }

    #endregion
    
    #region S/U16x4
    
    [Fact]
    public static void S16x4_1111() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS16x4,
        ConvertFromGfxPlatformFormat.ReadS16x4,
        Vec4((short)1, (short)1, (short)1, (short)1),
        8
    );
    
    [Fact]
    public static void U16x4_1111() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteU16x4,
        ConvertFromGfxPlatformFormat.ReadU16x4,
        Vec4((ushort)1u, (ushort)1u, (ushort)1u, (ushort)1u),
        8
    );
    
    [Fact]
    public static void S16x4_0000() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS16x4,
        ConvertFromGfxPlatformFormat.ReadS16x4,
        Vec4((short)0, (short)0, (short)0, (short)0),
        8
    );
    
    [Fact]
    public static void U16x4_0000() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteU16x4,
        ConvertFromGfxPlatformFormat.ReadU16x4,
        Vec4((ushort)0u, (ushort)0u, (ushort)0u, (ushort)0u),
        8
    );

    [Fact]
    public static void S16x4_MaxMinMaxMin() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS16x4,
        ConvertFromGfxPlatformFormat.ReadS16x4, 
        Vec4(short.MaxValue, short.MinValue, short.MaxValue, short.MinValue), 
        8
    );
    
    [Fact]
    public static void S16x4_MinMaxMinMax() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS16x4,
        ConvertFromGfxPlatformFormat.ReadS16x4, 
        Vec4(short.MinValue, short.MaxValue, short.MinValue, short.MaxValue), 
        8
    );
    
    [Fact]
    public static void U16x4_MaxMaxMaxMax() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteU16x4,
        ConvertFromGfxPlatformFormat.ReadU16x4, 
        Vec4(ushort.MaxValue, ushort.MaxValue, ushort.MaxValue, ushort.MaxValue), 
        8
    );

    [Fact]
    public static void S16x4_RandxRandxRand10()
    {
        for (int i = 0; i < 10; i++)
        {
            short randX = (short) Random.Shared.Next(short.MinValue, short.MaxValue);
            short randY = (short) Random.Shared.Next(short.MinValue, short.MaxValue);
            short randZ = (short) Random.Shared.Next(short.MinValue, short.MaxValue);
            short randW = (short) Random.Shared.Next(short.MinValue, short.MaxValue);
            
            AssertEqual(
                ConvertToGFXPlatformFormat.WriteS16x4,
                ConvertFromGfxPlatformFormat.ReadS16x4,
                Vec4(randX, randY, randZ, randW),
                8
            );
        }
    }
    
    [Fact]
    public static void U16x4_RandxRandxRand16()
    {
        for (int i = 0; i < 15; i++)
        {
            ushort randX = (ushort) Random.Shared.Next(ushort.MaxValue);
            ushort randY = (ushort) Random.Shared.Next(ushort.MaxValue);
            ushort randZ = (ushort) Random.Shared.Next(ushort.MaxValue);
            ushort randW = (ushort) Random.Shared.Next(ushort.MaxValue);
            
            AssertEqual(
                ConvertToGFXPlatformFormat.WriteU16x4,
                ConvertFromGfxPlatformFormat.ReadU16x4,
                Vec4(randX, randY, randZ, randW),
                8
            );
        }
    }
    
    #endregion

    #region S/UN16
    
    [Fact] //Max
    public static void SN16_Half() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN16,
        ConvertFromGfxPlatformFormat.ReadSN16,
        0.5f,
        2
    );
    
    [Fact] //Max
    public static void SN16_NegHalf() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN16,
        ConvertFromGfxPlatformFormat.ReadSN16,
        -0.5f,
        2
    );
    
    [Fact] //Max
    public static void UN16_One() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN16,
        ConvertFromGfxPlatformFormat.ReadSN16,
        1.0f,
        2
    );
    
    [Fact] //exactly in half lol
    public static void SN16_0() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN16,
        ConvertFromGfxPlatformFormat.ReadSN16,
        0.0f,
        2
    );
    
    [Fact] //Min
    public static void UN16_0() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteSN16,
        ConvertFromGfxPlatformFormat.ReadSN16,
        0.0f,
        2
    );

    [Fact] //Max
    public static void SN16_1() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN16,
        ConvertFromGfxPlatformFormat.ReadSN16,
        1.0f,
        2
    );
    
    [Fact] //Max
    public static void UN16_1() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN16,
        ConvertFromGfxPlatformFormat.ReadSN16,
        1.0f,
        2
    );

    [Fact] //Min
    public static void SN16_Neg1() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN16,
        ConvertFromGfxPlatformFormat.ReadSN16,
        -1.0f,
        2
    );

    [Fact]
    public static void SN16_Rand16()
    {
        for (int i = 0; i < 16; i++)
        {
            float rand = Random.Shared.NextSingle();

            short s = (short)MathF.Round(rand * short.MaxValue);
            float convertedBack = s < 0 ? (s / 32768.0f) : (s / 32767.0f);
            
            AssertNearlyEqual(
                ConvertToGFXPlatformFormat.WriteSN16,
                ConvertFromGfxPlatformFormat.ReadSN16,
                convertedBack,
                2
            );
        }
    }
    
    [Fact]
    public static void UN16_Rand32()
    {
        for (int i = 0; i < 32; i++)
        {
            float rand = Random.Shared.NextSingle();
            rand *= (Random.Shared.Next() % 2 == 0) ? -1 : 1;

            float convertedBack = ((ushort)MathF.Round(rand * short.MaxValue)) / 65535.0f ;
            
            AssertNearlyEqual(
                ConvertToGFXPlatformFormat.WriteUN16,
                ConvertFromGfxPlatformFormat.ReadUN16,
                convertedBack,
                2
            );
        }
    }
    
    #endregion

    #region S/UN16x2
    
    [Fact]
    public static void SN16x2_phph() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN16x2,
        ConvertFromGfxPlatformFormat.ReadSN16x2,
        new Vector2(0.5f, 0.5f),
        4
    );
    
    [Fact]
    public static void UN16x2_hh() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteUN16x2,
        ConvertFromGfxPlatformFormat.ReadUN16x2,
        new Vector2(0.5f, 0.5f),
        4
    );
    
    [Fact]
    public static void SN16x2_nhnh() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN16x2,
        ConvertFromGfxPlatformFormat.ReadSN16x2,
        new Vector2(-0.5f, -0.5f),
        4
    );
    
    [Fact]
    public static void SN16x2_00() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN16x2,
        ConvertFromGfxPlatformFormat.ReadSN16x2,
        new Vector2(0.0f, 0.0f),
        4
    );
    
    [Fact]
    public static void UN16x2_00() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteUN16x2,
        ConvertFromGfxPlatformFormat.ReadUN16x2,
        new Vector2(0.0f, 0.0f),
        4
    );

    [Fact]
    public static void SN16x2_1n1() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN16x2,
        ConvertFromGfxPlatformFormat.ReadSN16x2,
        new Vector2(1.0f, -1.0f),
        4
    );
    
    [Fact]
    public static void SN16x2_n11() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN16x2,
        ConvertFromGfxPlatformFormat.ReadSN16x2,
        new Vector2(-1.0f, 1.0f),
        4
    );

    [Fact]
    public static void UN16x2_11() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN16x2,
        ConvertFromGfxPlatformFormat.ReadSN16x2,
        new Vector2(1.0f, 1.0f),
        4
    );
    

    [Fact]
    public static void SN16x2_RandxRand16()
    {
        for (int i = 0; i < 16; i++)
        {
            float randX = Random.Shared.NextSingle();
            randX *= (Random.Shared.Next() % 2 == 0) ? -1 : 1;
            
            short sx = (short)MathF.Round(randX * short.MaxValue);
            float cx = sx < 0 ? (sx / 32768.0f) : (sx / 32767.0f);

            float randY = Random.Shared.NextSingle();
            randY *= (Random.Shared.Next() % 2 == 0) ? -1 : 1;
            
            short sy = (short)MathF.Round(randY * short.MaxValue);
            float cy = sy < 0 ? (sy / 32768.0f) : (sy / 32767.0f);

            
            AssertNearlyEqual(
                ConvertToGFXPlatformFormat.WriteSN16x2,
                ConvertFromGfxPlatformFormat.ReadSN16x2,
                new Vector2(cx, cy),
                4
            );
        }
    }
    
    [Fact]
    public static void UN16x2_RandxRand32()
    {
        for (int i = 0; i < 32; i++)
        {
            float randX = Random.Shared.NextSingle();
            float randY = Random.Shared.NextSingle();
            
            AssertNearlyEqual(
                ConvertToGFXPlatformFormat.WriteSN16x2,
                ConvertFromGfxPlatformFormat.ReadSN16x2,
                new Vector2(randX, randY),
                4
            );
        }
    }

    #endregion

    #region S/UN16x4

    [Fact]
    public static void SN16x4_phphphph() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN16x4,
        ConvertFromGfxPlatformFormat.ReadSN16x4,
        new Vector4(0.5f, 0.5f, 0.5f, 0.5f),
        8
    );
    
    [Fact]
    public static void UN16x4_hhhh() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteUN16x4,
        ConvertFromGfxPlatformFormat.ReadUN16x4,
        new Vector4(0.5f, 0.5f, 0.5f, 0.5f),
        8
    );
    
    [Fact]
    public static void SN16x4_nhnhnhnh() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN16x4,
        ConvertFromGfxPlatformFormat.ReadSN16x4,
        new Vector4(-0.5f, -0.5f, -0.5f, -0.5f),
        8
    );
    
    [Fact]
    public static void SN16x4_0000() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN16x4,
        ConvertFromGfxPlatformFormat.ReadSN16x4,
        new Vector4(0.0f, 0.0f, 0.0f, 0.0f),
        8
    );
    
    [Fact]
    public static void UN16x4_0000() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteUN16x4,
        ConvertFromGfxPlatformFormat.ReadUN16x4,
        new Vector4(0.0f, 0.0f, 0.0f, 0.0f),
        8
    );

    [Fact]
    public static void SN16x4_1n11n1() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN16x4,
        ConvertFromGfxPlatformFormat.ReadSN16x4,
        new Vector4(1.0f, -1.0f, 1.0f, -1.0f),
        8
    );
    
    [Fact]
    public static void SN16x4_n11n11() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN16x4,
        ConvertFromGfxPlatformFormat.ReadSN16x4,
        new Vector4(-1.0f, 1.0f, -1.0f, 1.0f),
        8
    );

    [Fact]
    public static void UN16x4_1111() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN16x4,
        ConvertFromGfxPlatformFormat.ReadSN16x4,
        new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
        8
    );
    

    [Fact]
    public static void SN16x4_RandxRand16()
    {
        for (int i = 0; i < 16; i++)
        {
            float randX = Random.Shared.NextSingle();
            randX *= (Random.Shared.Next() % 2 == 0) ? -1 : 1;
            
            short sx = (short)MathF.Round(randX * short.MaxValue);
            float cx = sx < 0 ? (sx / 32768.0f) : (sx / 32767.0f);

            float randY = Random.Shared.NextSingle();
            randY *= (Random.Shared.Next() % 2 == 0) ? -1 : 1;
            
            short sy = (short)MathF.Round(randY * short.MaxValue);
            float cy = sy < 0 ? (sy / 32768.0f) : (sy / 32767.0f);
            
            float randZ = Random.Shared.NextSingle();
            randZ *= (Random.Shared.Next() % 2 == 0) ? -1 : 1;
            
            short sz = (short)MathF.Round(randZ * short.MaxValue);
            float cz = sz < 0 ? (sz / 32768.0f) : (sz / 32767.0f);

            float randW = Random.Shared.NextSingle();
            randW *= (Random.Shared.Next() % 2 == 0) ? -1 : 1;
            
            short sw = (short)MathF.Round(randW * short.MaxValue);
            float cw = sw < 0 ? (sw / 32768.0f) : (sw / 32767.0f);
            

            
            AssertNearlyEqual(
                ConvertToGFXPlatformFormat.WriteSN16x4,
                ConvertFromGfxPlatformFormat.ReadSN16x4,
                new Vector4(cx, cy, cz, cw),
                8
            );
        }
    }
    
    [Fact]
    public static void UN16x4_RandxRand32()
    {
        for (int i = 0; i < 32; i++)
        {
            float randX = Random.Shared.NextSingle();
            float randY = Random.Shared.NextSingle();
            float randZ = Random.Shared.NextSingle();
            float randW = Random.Shared.NextSingle();
            
            AssertNearlyEqual(
                ConvertToGFXPlatformFormat.WriteSN16x4,
                ConvertFromGfxPlatformFormat.ReadSN16x4,
                new Vector4(randX, randY, randZ, randW),
                8
            );
        }
    }

    #endregion
    
    #region S/U8

    [Fact]
    public static void S8_1() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS8,
        ConvertFromGfxPlatformFormat.ReadS8,
        (sbyte)1,
        1
    );
    
    [Fact]
    public static void U8_1() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteU8,
        ConvertFromGfxPlatformFormat.ReadU8,
        (byte)1,
        1
    );
    
    [Fact]
    public static void S8_0() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS8,
        ConvertFromGfxPlatformFormat.ReadS8,
        (sbyte)0,
        2
    );
    
    [Fact]
    public static void U8_0() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteU8,
        ConvertFromGfxPlatformFormat.ReadU8,
        (byte)0,
        1
    );

    [Fact]
    public static void S8_Max() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS8,
        ConvertFromGfxPlatformFormat.ReadS8, 
        sbyte.MaxValue, 
        1
    );
    
    [Fact]
    public static void U8_Max() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteU8,
        ConvertFromGfxPlatformFormat.ReadU8, 
        byte.MaxValue, 
        1
    );

    [Fact]
    public static void S8_Min() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS8,
        ConvertFromGfxPlatformFormat.ReadS8,
        sbyte.MinValue,
        1
    );

    [Fact]
    public static void S8_Rand10()
    {
        for (int i = 0; i < 10; i++)
        {
            sbyte rand = (sbyte)Random.Shared.Next(sbyte.MinValue, sbyte.MaxValue);
            
            AssertEqual(
                ConvertToGFXPlatformFormat.WriteS8,
                ConvertFromGfxPlatformFormat.ReadS8,
                rand,
                1
            );
        }
    }
    
    [Fact]
    public static void U8_Rand16()
    {
        for (int i = 0; i < 16; i++)
        {
            byte rand = (byte)Random.Shared.Next(byte.MaxValue);
            
            AssertEqual(
                ConvertToGFXPlatformFormat.WriteU8,
                ConvertFromGfxPlatformFormat.ReadU8,
                rand,
                1
            );
        }
    }

    #endregion

    #region S/U8x2

    
    [Fact]
    public static void S8x2_11() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS8x2,
        ConvertFromGfxPlatformFormat.ReadS8x2,
        Vec2((sbyte)1, (sbyte)1),
        2
    );
    
    [Fact]
    public static void U8x2_11() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteU8x2,
        ConvertFromGfxPlatformFormat.ReadU8x2,
        Vec2((byte)1u, (byte)1u),
        2
    );
    
    [Fact]
    public static void S8x2_00() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS8x2,
        ConvertFromGfxPlatformFormat.ReadS8x2,
        Vec2((sbyte)0, (sbyte)0),
        2
    );
    
    [Fact]
    public static void U8x2_00() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteU8x2,
        ConvertFromGfxPlatformFormat.ReadU8x2,
        Vec2((byte)0u, (byte)0u),
        2
    );

    [Fact]
    public static void S8x2_MaxMin() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS8x2,
        ConvertFromGfxPlatformFormat.ReadS8x2, 
        Vec2(sbyte.MaxValue, sbyte.MinValue), 
        2
    );
    
    [Fact]
    public static void S8x2_MinMax() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS8x2,
        ConvertFromGfxPlatformFormat.ReadS8x2, 
        Vec2(sbyte.MinValue, sbyte.MaxValue), 
        2
    );
    
    [Fact]
    public static void U8x2_MaxMax() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteU8x2,
        ConvertFromGfxPlatformFormat.ReadU8x2, 
        Vec2(byte.MaxValue, byte.MaxValue), 
        2
    );

    [Fact]
    public static void S8x2_RandxRand10()
    {
        for (int i = 0; i < 10; i++)
        {
            sbyte randX = (sbyte)Random.Shared.Next(sbyte.MinValue, sbyte.MaxValue);
            sbyte randY = (sbyte)Random.Shared.Next(sbyte.MinValue, sbyte.MaxValue);
            
            AssertEqual(
                ConvertToGFXPlatformFormat.WriteS8x2,
                ConvertFromGfxPlatformFormat.ReadS8x2,
                Vec2(randX, randY),
                2
            );
        }
    }
    
    [Fact]
    public static void U8x2_RandxRand16()
    {
        for (int i = 0; i < 15; i++)
        {
            byte randX = (byte) Random.Shared.Next(byte.MaxValue);
            byte randY = (byte) Random.Shared.Next(byte.MaxValue);
            
            AssertEqual(
                ConvertToGFXPlatformFormat.WriteU8x2,
                ConvertFromGfxPlatformFormat.ReadU8x2,
                Vec2(randX, randY),
                2
            );
        }
    }

    #endregion
    
    #region S/U8x4
    
    [Fact]
    public static void S8x4_1111() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS8x4,
        ConvertFromGfxPlatformFormat.ReadS8x4,
        Vec4((sbyte)1, (sbyte)1, (sbyte)1, (sbyte)1),
        4
    );
    
    [Fact]
    public static void U8x4_1111() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteU8x4,
        ConvertFromGfxPlatformFormat.ReadU8x4,
        Vec4((byte)1u, (byte)1u, (byte)1u, (byte)1u),
        4
    );
    
    [Fact]
    public static void S8x4_0000() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS8x4,
        ConvertFromGfxPlatformFormat.ReadS8x4,
        Vec4((sbyte)0, (sbyte)0, (sbyte)0, (sbyte)0),
        4
    );
    
    [Fact]
    public static void U8x4_0000() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteU8x4,
        ConvertFromGfxPlatformFormat.ReadU8x4,
        Vec4((byte)0u, (byte)0u, (byte)0u, (byte)0u),
        4
    );

    [Fact]
    public static void S8x4_MaxMinMaxMin() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS8x4,
        ConvertFromGfxPlatformFormat.ReadS8x4, 
        Vec4(sbyte.MaxValue, sbyte.MinValue, sbyte.MaxValue, sbyte.MinValue), 
        4
    );
    
    [Fact]
    public static void S8x4_MinMaxMinMax() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteS8x4,
        ConvertFromGfxPlatformFormat.ReadS8x4, 
        Vec4(sbyte.MinValue, sbyte.MaxValue, sbyte.MinValue, sbyte.MaxValue), 
        4
    );
    
    [Fact]
    public static void U8x4_MaxMaxMaxMax() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteU8x4,
        ConvertFromGfxPlatformFormat.ReadU8x4, 
        Vec4(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), 
        4
    );

    [Fact]
    public static void S8x4_RandxRandxRand10()
    {
        for (int i = 0; i < 10; i++)
        {
            sbyte randX = (sbyte) Random.Shared.Next(sbyte.MinValue, sbyte.MaxValue);
            sbyte randY = (sbyte) Random.Shared.Next(sbyte.MinValue, sbyte.MaxValue);
            sbyte randZ = (sbyte) Random.Shared.Next(sbyte.MinValue, sbyte.MaxValue);
            sbyte randW = (sbyte) Random.Shared.Next(sbyte.MinValue, sbyte.MaxValue);
            
            AssertEqual(
                ConvertToGFXPlatformFormat.WriteS8x4,
                ConvertFromGfxPlatformFormat.ReadS8x4,
                Vec4(randX, randY, randZ, randW),
                4
            );
        }
    }
    
    [Fact]
    public static void U8x4_RandxRandxRand16()
    {
        for (int i = 0; i < 15; i++)
        {
            byte randX = (byte) Random.Shared.Next(byte.MaxValue);
            byte randY = (byte) Random.Shared.Next(byte.MaxValue);
            byte randZ = (byte) Random.Shared.Next(byte.MaxValue);
            byte randW = (byte) Random.Shared.Next(byte.MaxValue);
            
            AssertEqual(
                ConvertToGFXPlatformFormat.WriteU8x4,
                ConvertFromGfxPlatformFormat.ReadU8x4,
                Vec4(randX, randY, randZ, randW),
                4
            );
        }
    }
    
    #endregion
    
    #region S/UN8
    
    [Fact] //Max
    public static void SN8_Half() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN8,
        ConvertFromGfxPlatformFormat.ReadSN8,
        0.5f,
        1,
        0.01f
    );
    
    [Fact] //Max
    public static void SN8_NegHalf() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN8,
        ConvertFromGfxPlatformFormat.ReadSN8,
        -0.5f,
        1,
        0.01f
    );
    
    [Fact] //Max
    public static void UN8_One() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN8,
        ConvertFromGfxPlatformFormat.ReadSN8,
        1.0f,
        1,
        0.01f
    );
    
    [Fact] //exactly in half lol
    public static void SN8_0() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN8,
        ConvertFromGfxPlatformFormat.ReadSN8,
        0.0f,
        1,
        0.01f
    );
    
    [Fact] //Min
    public static void UN8_0() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN8,
        ConvertFromGfxPlatformFormat.ReadSN8,
        0.0f,
        1,
        0.01f
    );

    [Fact] //Max
    public static void SN8_1() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN8,
        ConvertFromGfxPlatformFormat.ReadSN8,
        1.0f,
        1,
        0.01f
    );
    
    [Fact] //Max
    public static void UN8_1() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN8,
        ConvertFromGfxPlatformFormat.ReadSN8,
        1.0f,
        1,
        0.01f
    );

    [Fact] //Min
    public static void SN8_Neg1() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN8,
        ConvertFromGfxPlatformFormat.ReadSN8,
        -1.0f,
        1,
        0.01f
    );

    [Fact]
    public static void SN8_Rand16()
    {
        for (int i = 0; i < 16; i++)
        {
            float rand = Random.Shared.NextSingle();

            sbyte s = (sbyte)MathF.Round(rand * sbyte.MaxValue);
            float convertedBack = s / 127.0f;
            
            AssertNearlyEqual(
                ConvertToGFXPlatformFormat.WriteSN8,
                ConvertFromGfxPlatformFormat.ReadSN8,
                convertedBack,
                1,
                0.01f
            );
        }
    }
    
    [Fact]
    public static void UN8_Rand32()
    {
        for (int i = 0; i < 32; i++)
        {
            float rand = Random.Shared.NextSingle();
            rand *= (Random.Shared.Next() % 2 == 0) ? -1 : 1;

            float convertedBack = ((byte)MathF.Round(rand * byte.MaxValue)) / 255.0f;
            
            AssertNearlyEqual(
                ConvertToGFXPlatformFormat.WriteUN8,
                ConvertFromGfxPlatformFormat.ReadUN8,
                convertedBack,
                1,
                0.01f
            );
        }
    }
    
    #endregion

    #region S/UN8x2
    
    [Fact]
    public static void SN8x2_phph() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN8x2,
        ConvertFromGfxPlatformFormat.ReadSN8x2,
        new Vector2(0.5f, 0.5f),
        2,
        0.01f
    );
    
    [Fact]
    public static void UN8x2_hh() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteUN8x2,
        ConvertFromGfxPlatformFormat.ReadUN8x2,
        new Vector2(0.5f, 0.5f),
        2,
        0.01f
    );
    
    [Fact]
    public static void SN8x2_nhnh() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN8x2,
        ConvertFromGfxPlatformFormat.ReadSN8x2,
        new Vector2(-0.5f, -0.5f),
        2,
        0.01f
    );
    
    [Fact]
    public static void SN8x2_00() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN8x2,
        ConvertFromGfxPlatformFormat.ReadSN8x2,
        new Vector2(0.0f, 0.0f),
        2,
        0.01f
    );
    
    [Fact]
    public static void UN8x2_00() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteUN8x2,
        ConvertFromGfxPlatformFormat.ReadUN8x2,
        new Vector2(0.0f, 0.0f),
        2,
        0.01f
    );

    [Fact]
    public static void SN8x2_1n1() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN8x2,
        ConvertFromGfxPlatformFormat.ReadSN8x2,
        new Vector2(1.0f, -1.0f),
        2,
        0.01f
    );
    
    [Fact]
    public static void SN8x2_n11() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN8x2,
        ConvertFromGfxPlatformFormat.ReadSN8x2,
        new Vector2(-1.0f, 1.0f),
        2,
        0.01f
    );

    [Fact]
    public static void UN8x2_11() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN8x2,
        ConvertFromGfxPlatformFormat.ReadSN8x2,
        new Vector2(1.0f, 1.0f),
        2,
        0.01f
    );
    

    [Fact]
    public static void SN8x2_RandxRand16()
    {
        for (int i = 0; i < 16; i++)
        {
            float randX = Random.Shared.NextSingle();
            randX *= (Random.Shared.Next() % 2 == 0) ? -1 : 1;
            
            sbyte sx = (sbyte)MathF.Round(randX * sbyte.MaxValue);
            float cx = (sx / 127.0f);

            float randY = Random.Shared.NextSingle();
            randY *= (Random.Shared.Next() % 2 == 0) ? -1 : 1;
            
            sbyte sy = (sbyte)MathF.Round(randY * sbyte.MaxValue);
            float cy = (sy / 127.0f);

            
            AssertNearlyEqual(
                ConvertToGFXPlatformFormat.WriteSN8x2,
                ConvertFromGfxPlatformFormat.ReadSN8x2,
                new Vector2(cx, cy),
                2,
                0.01f
            );
        }
    }
    
    [Fact]
    public static void UN8x2_RandxRand32()
    {
        for (int i = 0; i < 32; i++)
        {
            float randX = Random.Shared.NextSingle();
            
            byte sx = (byte)MathF.Round(randX * byte.MaxValue);
            float cx = (sx / 255.0f);

            float randY = Random.Shared.NextSingle();
            
            byte sy = (byte)MathF.Round(randY * byte.MaxValue);
            float cy = (sy / 255.0f);
            
            AssertNearlyEqual(
                ConvertToGFXPlatformFormat.WriteSN8x2,
                ConvertFromGfxPlatformFormat.ReadSN8x2,
                new Vector2(cx, cy),
                4,
                0.01f
            );
        }
    }

    #endregion

    #region S/UN8x4

    [Fact]
    public static void SN8x4_phphphph() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN8x4,
        ConvertFromGfxPlatformFormat.ReadSN8x4,
        new Vector4(0.5f, 0.5f, 0.5f, 0.5f),
        4,
        0.01f
    );
    
    [Fact]
    public static void UN8x4_hhhh() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteUN8x4,
        ConvertFromGfxPlatformFormat.ReadUN8x4,
        new Vector4(0.5f, 0.5f, 0.5f, 0.5f),
        4,
        0.01f
    );
    
    [Fact]
    public static void SN8x4_nhnhnhnh() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN8x4,
        ConvertFromGfxPlatformFormat.ReadSN8x4,
        new Vector4(-0.5f, -0.5f, -0.5f, -0.5f),
        4,
        0.01f
    );
    
    [Fact]
    public static void SN8x4_0000() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN8x4,
        ConvertFromGfxPlatformFormat.ReadSN8x4,
        new Vector4(0.0f, 0.0f, 0.0f, 0.0f),
        4,
        0.01f
    );
    
    [Fact]
    public static void UN8x4_0000() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteUN8x4,
        ConvertFromGfxPlatformFormat.ReadUN8x4,
        new Vector4(0.0f, 0.0f, 0.0f, 0.0f),
        4,
        0.01f
    );

    [Fact]
    public static void SN8x4_1n11n1() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN8x4,
        ConvertFromGfxPlatformFormat.ReadSN8x4,
        new Vector4(1.0f, -1.0f, 1.0f, -1.0f),
        4,
        0.01f
    );
    
    [Fact]
    public static void SN8x4_n11n11() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN8x4,
        ConvertFromGfxPlatformFormat.ReadSN8x4,
        new Vector4(-1.0f, 1.0f, -1.0f, 1.0f),
        4,
        0.01f
    );

    [Fact]
    public static void UN8x4_1111() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteSN8x4,
        ConvertFromGfxPlatformFormat.ReadSN8x4,
        new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
        4,
        0.01f
    );
    

    [Fact]
    public static void SN8x4_RandxRand16()
    {
        for (int i = 0; i < 16; i++)
        {
            float randX = Random.Shared.NextSingle();
            randX *= (Random.Shared.Next() % 2 == 0) ? -1 : 1;
            
            sbyte sx = (sbyte)MathF.Round(randX * sbyte.MaxValue);
            float cx = (sx / 127.0f);

            float randY = Random.Shared.NextSingle();
            randY *= (Random.Shared.Next() % 2 == 0) ? -1 : 1;
            
            sbyte sy = (sbyte)MathF.Round(randY * sbyte.MaxValue);
            float cy = (sy / 127.0f);
            
            float randZ = Random.Shared.NextSingle();
            randZ *= (Random.Shared.Next() % 2 == 0) ? -1 : 1;
            
            sbyte sz = (sbyte)MathF.Round(randZ * sbyte.MaxValue);
            float cz = sz / 127.0f;

            float randW = Random.Shared.NextSingle();
            randW *= (Random.Shared.Next() % 2 == 0) ? -1 : 1;
            
            sbyte sw = (sbyte)MathF.Round(randW * sbyte.MaxValue);
            float cw = sw / 127.0f;
            

            
            AssertNearlyEqual(
                ConvertToGFXPlatformFormat.WriteSN8x4,
                ConvertFromGfxPlatformFormat.ReadSN8x4,
                new Vector4(cx, cy, cz, cw),
                4,
        0.01f
            );
        }
    }
    
    [Fact]
    public static void UN8x4_RandxRand32()
    {
        for (int i = 0; i < 32; i++)
        {
            float randX = Random.Shared.NextSingle();
            
            byte sx = (byte)MathF.Round(randX * byte.MaxValue);
            float cx = (sx / 255.0f);

            float randY = Random.Shared.NextSingle();
            
            byte sy = (byte)MathF.Round(randY * byte.MaxValue);
            float cy = (sy / 255.0f);
            
            float randZ = Random.Shared.NextSingle();
            
            byte sz = (byte)MathF.Round(randZ * byte.MaxValue);
            float cz = sz / 255.0f;

            float randW = Random.Shared.NextSingle();
            
            byte sw = (byte)MathF.Round(randW * byte.MaxValue);
            float cw = sw / 255.0f;
            
            AssertNearlyEqual(
                ConvertToGFXPlatformFormat.WriteSN8x4,
                ConvertFromGfxPlatformFormat.ReadSN8x4,
                new Vector4(cx, cy, cz, cw),
                4,
        0.01f
            );
        }
    }

    #endregion

    #region SN10_SN11_SN11

    [Fact]
    public static void SN10_SN11_SN11_0() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteSN10_SN11_SN11,
        ConvertFromGfxPlatformFormat.ReadSN10_SN11_SN11,
        new Vector3(0.0f, 0.0f, 0.0f),
        4
    );
    
    [Fact]
    public static void SN10_SN11_SN11_1() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteSN10_SN11_SN11,
        ConvertFromGfxPlatformFormat.ReadSN10_SN11_SN11,
        new Vector3(1.0f, 1.0f, 1.0f),
        4
    );

    [Fact] //sometimes you can't avoid the damn indents -_-
    public static void SN10_SN11_SN11_01S()
    {
        for (int i = -1; i < 2; i++)
            for (int j = -1; j < 2; j++)
                for (int k = -1; k < 2; k++)
                    AssertEqual(
                        ConvertToGFXPlatformFormat.WriteSN10_SN11_SN11,
                        ConvertFromGfxPlatformFormat.ReadSN10_SN11_SN11,
                        new Vector3(i, j, k),
                        4
                    );
    }

    [Fact]
    public static void SN10_SN11_SN11_Rand64()
    {
        for (int i = 0; i < 64; i++)
        {
            float x = Random.Shared.NextSingle();
            float y = Random.Shared.NextSingle();
            float z = Random.Shared.NextSingle();
            
            AssertNearlyEqual(
                ConvertToGFXPlatformFormat.WriteSN10_SN11_SN11,
                ConvertFromGfxPlatformFormat.ReadSN10_SN11_SN11,
                new Vector3(x, y, z),
                4,
                0.002f //close to 2^(-9), the uncertanty of a 10 bit fixed point number
            );
        }
    }

    #endregion

    #region SN10x3_SN2

    [Fact]
    public static void SN10x3_SN2_0() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteSN10x3_SN2,
        ConvertFromGfxPlatformFormat.ReadSN10x3_SN2,
        new Vector4(0.0f, 0.0f, 0.0f, 0.0f),
        4
    );
    
    [Fact]
    public static void SN10x3_SN2_1() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteSN10x3_SN2,
        ConvertFromGfxPlatformFormat.ReadSN10x3_SN2,
        new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
        4
    );
    
    [Fact]
    public static void SN10x3_SN2_N1() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteSN10x3_SN2,
        ConvertFromGfxPlatformFormat.ReadSN10x3_SN2,
        new Vector4(-1.0f, -1.0f, -1.0f, -1.0f),
        4
    );

    [Fact] //sometimes you can't avoid the damn indents -_-
    public static void SN10x3_SN2_01S()
    {
        for (int i = -1; i < 2; i++)
            for (int j = -1; j < 2; j++)
                for (int k = -1; k < 2; k++)
                    for (int l = -1; l < 2; l++)
                        AssertEqual(
                            ConvertToGFXPlatformFormat.WriteSN10x3_SN2,
                            ConvertFromGfxPlatformFormat.ReadSN10x3_SN2,
                            new Vector4(i, j, k, l),
                            4
                        );
    }

    [Fact]
    public static void SN10x3_SN2_Rand64()
    {
        for (int i = 0; i < 64; i++)
        {
            float x = Random.Shared.NextSingle();
            float y = Random.Shared.NextSingle();
            float z = Random.Shared.NextSingle();
            float w = MathF.Round(Random.Shared.NextSingle());
            
            AssertNearlyEqual(
                ConvertToGFXPlatformFormat.WriteSN10x3_SN2,
                ConvertFromGfxPlatformFormat.ReadSN10x3_SN2,
                new Vector4(x, y, z, w),
                4,
                0.002f //close to 2^(-9), the uncertanty of a 10 bit fixed point number
            );
        }
    }
    
    #endregion
    
    #region UN10x3_UN2

    [Fact]
    public static void UN10x3_UN2_0() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteUN10x3_UN2,
        ConvertFromGfxPlatformFormat.ReadUN10x3_UN2,
        new Vector4(0.0f, 0.0f, 0.0f, 0.0f),
        4
    );
    
    [Fact]
    public static void UN10x3_UN2_1() => AssertEqual(
        ConvertToGFXPlatformFormat.WriteUN10x3_UN2,
        ConvertFromGfxPlatformFormat.ReadUN10x3_UN2,
        new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
        4
    );
    
    [Fact]
    public static void UN10x3_UN2_Half() => AssertNearlyEqual(
        ConvertToGFXPlatformFormat.WriteUN10x3_UN2,
        ConvertFromGfxPlatformFormat.ReadUN10x3_UN2,
        new Vector4(0.5f, 0.5f, 0.5f, 0.5f),
        4,
        0.25f
    );

    [Fact] //sometimes you can't avoid the damn indents -_-
    public static void UN10x3_UN2_01S()
    {
        for (int i = 0; i < 2; i++)
            for (int j = 0; j < 2; j++)
                for (int k = 0; k < 2; k++)
                    for (int l = 0; l < 2; l++)
                        AssertEqual(
                            ConvertToGFXPlatformFormat.WriteUN10x3_UN2,
                            ConvertFromGfxPlatformFormat.ReadUN10x3_UN2,
                            new Vector4(i, j, k, l),
                            4
                        );
    }

    [Fact]
    public static void UN10x3_UN2_Rand64()
    {
        for (int i = 0; i < 64; i++)
        {
            float x = Random.Shared.NextSingle();
            float y = Random.Shared.NextSingle();
            float z = Random.Shared.NextSingle();
            float w = MathF.Round(Random.Shared.NextSingle());
            
            AssertWLessNearlyEqual(
                ConvertToGFXPlatformFormat.WriteUN10x3_UN2,
                ConvertFromGfxPlatformFormat.ReadUN10x3_UN2,
                new Vector4(x, y, z, w),
                4
            );
        }
    }
    
    #endregion
    
}
