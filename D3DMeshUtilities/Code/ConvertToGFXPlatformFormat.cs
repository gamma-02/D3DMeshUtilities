using System;
using System.Numerics;
using D3DMeshUtilities.Code.Util;
using TelltaleToolKit.T3Types.Meshes.T3Types;

namespace D3DMeshUtilities.Code;

/// <summary>
/// This class is designed to contain implementations for writing any GFXPlatformFormat tuple
/// to a byte array.
///
/// <see cref="TelltaleToolKit.T3Types.Meshes.T3Types.GFXPlatformFormat"/>
/// </summary>
public static class ConvertToGFXPlatformFormat
{

    public static bool WriteGFXPlatformFormat(Span<byte> span, object value, GFXPlatformFormat format)
    {
        return format switch
        {
            GFXPlatformFormat.None => true, //technically worked :D
            GFXPlatformFormat.F32 => WriteF32(span, (float) value),
            GFXPlatformFormat.F32x2 => WriteF32x2(span, (Vector2) value),
            GFXPlatformFormat.F32x3 => WriteF32x3(span, (Vector3) value),
            GFXPlatformFormat.F32x4 => WriteF32x4(span, (Vector4) value),
            GFXPlatformFormat.F16x2 => WriteF16x2(span, (Vector2) value),
            GFXPlatformFormat.F16x4 => WriteF16x4(span, (Vector4) value),
            GFXPlatformFormat.S32 => WriteS32(span, (int) value),
            GFXPlatformFormat.U32 => WriteU32(span, (uint) value),
            GFXPlatformFormat.S32x2 => WriteS32x2(span, (Vec2<int>) value),
            GFXPlatformFormat.U32x2 => WriteU32x2(span, (Vec2<uint>) value),
            GFXPlatformFormat.S32x3 => WriteS32x3(span, (Vec3<int>) value),
            GFXPlatformFormat.U32x3 => WriteU32x3(span, (Vec3<uint>) value),
            GFXPlatformFormat.S32x4 => WriteS32x4(span, (Vec4<int>) value),
            GFXPlatformFormat.U32x4 => WriteU32x4(span, (Vec4<uint>) value),
            GFXPlatformFormat.S16 => WriteS16(span, (short) value),
            GFXPlatformFormat.U16 => WriteU16(span, (ushort) value),
            GFXPlatformFormat.S16x2 => WriteS16x2(span, (Vec2<short>) value),
            GFXPlatformFormat.U16x2 => WriteU16x2(span, (Vec2<ushort>) value),
            GFXPlatformFormat.S16x4 => WriteS16x4(span, (Vec4<short>) value),
            GFXPlatformFormat.U16x4 => WriteU16x4(span, (Vec4<ushort>) value),
            GFXPlatformFormat.SN16 => WriteSN16(span, (float) value),
            GFXPlatformFormat.UN16 => WriteUN16(span, (float) value),
            GFXPlatformFormat.SN16x2 => WriteSN16x2(span, (Vector2) value),
            GFXPlatformFormat.UN16x2 => WriteUN16x2(span, (Vector2) value),
            GFXPlatformFormat.SN16x4 => WriteSN16x4(span, (Vector4) value),
            GFXPlatformFormat.UN16x4 => WriteUN16x4(span, (Vector4) value),
            GFXPlatformFormat.S8 => WriteS8(span, (sbyte) value),
            GFXPlatformFormat.U8 => WriteU8(span, (byte) value),
            GFXPlatformFormat.S8x2 => WriteS8x2(span, (Vec2<sbyte>) value),
            GFXPlatformFormat.U8x2 => WriteU8x2(span, (Vec2<byte>) value),
            GFXPlatformFormat.S8x4 => WriteS8x4(span, (Vec4<sbyte>) value),
            GFXPlatformFormat.U8x4 => WriteU8x4(span, (Vec4<byte>) value),
            GFXPlatformFormat.SN8 => WriteSN8(span, (float) value),
            GFXPlatformFormat.UN8 => WriteUN8(span, (float) value),
            GFXPlatformFormat.SN8x2 => WriteSN8x2(span, (Vector2) value),
            GFXPlatformFormat.UN8x2 => WriteUN8x2(span, (Vector2) value),
            GFXPlatformFormat.SN8x4 => WriteSN8x4(span, (Vector4) value),
            GFXPlatformFormat.UN8x4 => WriteUN8x4(span, (Vector4) value),
            GFXPlatformFormat.SN10_SN11_SN11 => WriteSN10_SN11_SN11(span, (Vector3) value),
            GFXPlatformFormat.SN10x3_SN2 => WriteSN10x3_SN2(span, (Vector4) value),
            GFXPlatformFormat.UN10x3_UN2 => WriteUN10x3_UN2(span, (Vector4) value),
            GFXPlatformFormat.D3DCOLOR => WriteD3DColor(span, (Vector3) value),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }
    
    /*
    For my reference, all of the GFX formats:
`None,
  F32,
  F32x2,
  F32x3,
  F32x4,
  F16x2,
  F16x4,
  S32,
  U32,
  S32x2,
  U32x2,
  S32x3,
  U32x3,
  S32x4,
  U32x4,
  S16,
  U16,
  S16x2,
  U16x2,
  S16x4,
  U16x4,
  SN16,
  UN16,
  SN16x2,
  UN16x2,
  SN16x4,
  UN16x4,
  S8,
  U8,
  S8x2,
  U8x2,
  S8x4,
  U8x4,
  SN8,
  UN8,
  SN8x2,
  UN8x2,
  SN8x4,
  UN8x4,
  SN10_SN11_SN11,
  SN10x3_SN2,
  UN10x3_UN2,
  D3DCOLOR,
     */

    public static bool WriteF32(Span<byte> span, float value)
    {
        byte[] val = BitConverter.GetBytes(value);

        try
        {
            val.CopyTo(span);
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteF32x2(Span<byte> span, Vector2 value)
    {
        byte[] x = BitConverter.GetBytes(value.X);
        byte[] y = BitConverter.GetBytes(value.Y);

        try
        {
            x.CopyTo(span.Slice(0, 4));
            y.CopyTo(span.Slice(4, 4));
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteF32x3(Span<byte> span, Vector3 value)
    {
        byte[] x = BitConverter.GetBytes(value.X);
        byte[] y = BitConverter.GetBytes(value.Y);
        byte[] z = BitConverter.GetBytes(value.Z);
        try
        {
            x.CopyTo(span.Slice(0, 4));
            y.CopyTo(span.Slice(4, 4));
            z.CopyTo(span.Slice(8, 4));
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteF32x4(Span<byte> span, Vector4 value)
    {
        byte[] x = BitConverter.GetBytes(value.X);
        byte[] y = BitConverter.GetBytes(value.Y);
        byte[] z = BitConverter.GetBytes(value.Z);
        byte[] w = BitConverter.GetBytes(value.W);

        try
        {
            x.CopyTo(span.Slice(0, 4));
            y.CopyTo(span.Slice(4, 4));
            z.CopyTo(span.Slice(8, 4));
            w.CopyTo(span.Slice(12, 4));
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteF16x2(Span<byte> span, Vector2 value)
    {
        byte[] x = BitConverter.GetBytes((Half)value.X);
        byte[] y = BitConverter.GetBytes((Half)value.Y);

        try
        {
            x.CopyTo(span.Slice(0, 2));
            y.CopyTo(span.Slice(2, 2));
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteF16x4(Span<byte> span, Vector4 value)
    {
        byte[] x = BitConverter.GetBytes((Half)value.X);
        byte[] y = BitConverter.GetBytes((Half)value.Y);
        byte[] z = BitConverter.GetBytes((Half)value.Z);
        byte[] w = BitConverter.GetBytes((Half)value.W);

        try
        {
            x.CopyTo(span.Slice(0, 2));
            y.CopyTo(span.Slice(2, 2));
            z.CopyTo(span.Slice(4, 2));
            w.CopyTo(span.Slice(6, 2));
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteS32(Span<byte> span, int value)
    {
        byte[] val = BitConverter.GetBytes(value);

        try
        {
            val.CopyTo(span.Slice(0, 4));
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteU32(Span<byte> span, uint value)
    {
        byte[] val = BitConverter.GetBytes(value);

        try
        {
            val.CopyTo(span.Slice(0, 4));
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteS32x2(Span<byte> span, Vec2<int> value)
    {
        byte[] x = BitConverter.GetBytes(value[0]);
        byte[] y = BitConverter.GetBytes(value[1]);

        try
        {
            x.CopyTo(span.Slice(0, 4));
            y.CopyTo(span.Slice(4, 4));
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteU32x2(Span<byte> span, Vec2<uint> value)
    {
        byte[] x = BitConverter.GetBytes(value[0]);
        byte[] y = BitConverter.GetBytes(value[1]);

        try
        {
            x.CopyTo(span.Slice(0, 4));
            y.CopyTo(span.Slice(4, 4));
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteS32x3(Span<byte> span, Vec3<int> value)
    {
        byte[] x = BitConverter.GetBytes(value[0]);
        byte[] y = BitConverter.GetBytes(value[1]);
        byte[] z = BitConverter.GetBytes(value[2]);

        try
        {
            x.CopyTo(span.Slice(0, 4));
            y.CopyTo(span.Slice(4, 4));
            z.CopyTo(span.Slice(8, 4));
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteU32x3(Span<byte> span, Vec3<uint> value)
    {
        byte[] x = BitConverter.GetBytes(value[0]);
        byte[] y = BitConverter.GetBytes(value[1]);
        byte[] z = BitConverter.GetBytes(value[2]);

        try
        {
            x.CopyTo(span.Slice(0, 4));
            y.CopyTo(span.Slice(4, 4));
            z.CopyTo(span.Slice(8, 4));
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteS32x4(Span<byte> span, Vec4<int> value)
    {
        byte[] x = BitConverter.GetBytes(value[0]);
        byte[] y = BitConverter.GetBytes(value[1]);
        byte[] z = BitConverter.GetBytes(value[2]);
        byte[] w = BitConverter.GetBytes(value[3]);

        try
        {
            x.CopyTo(span.Slice(0, 4));
            y.CopyTo(span.Slice(4, 4));
            z.CopyTo(span.Slice(8, 4));
            w.CopyTo(span.Slice(12, 4));
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteU32x4(Span<byte> span, Vec4<uint> value)
    {
        byte[] x = BitConverter.GetBytes(value[0]);
        byte[] y = BitConverter.GetBytes(value[1]);
        byte[] z = BitConverter.GetBytes(value[2]);
        byte[] w = BitConverter.GetBytes(value[3]);

        try
        {
            x.CopyTo(span.Slice(0, 4));
            y.CopyTo(span.Slice(4, 4));
            z.CopyTo(span.Slice(8, 4));
            w.CopyTo(span.Slice(12, 4));
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteS16(Span<byte> span, short value)
    {
        byte[] val = BitConverter.GetBytes(value);

        try
        {
            val.CopyTo(span.Slice(0, 2));
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteU16(Span<byte> span, ushort value)
    {
        byte[] val = BitConverter.GetBytes(value);

        try
        {
            val.CopyTo(span.Slice(0, 2));
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteS16x2(Span<byte> span, Vec2<short> value)
    {
        byte[] x = BitConverter.GetBytes(value[0]);
        byte[] y = BitConverter.GetBytes(value[1]);

        try
        {
            x.CopyTo(span.Slice(0, 2));
            y.CopyTo(span.Slice(2, 2));
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteU16x2(Span<byte> span, Vec2<ushort> value)
    {
        byte[] x = BitConverter.GetBytes(value[0]);
        byte[] y = BitConverter.GetBytes(value[1]);

        try
        {
            x.CopyTo(span.Slice(0, 2));
            y.CopyTo(span.Slice(2, 2));
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteS16x4(Span<byte> span, Vec4<short> value)
    {
        byte[] x = BitConverter.GetBytes(value[0]);
        byte[] y = BitConverter.GetBytes(value[1]);
        byte[] z = BitConverter.GetBytes(value[2]);
        byte[] w = BitConverter.GetBytes(value[3]);

        try
        {
            x.CopyTo(span.Slice(0, 2));
            y.CopyTo(span.Slice(2, 2));
            z.CopyTo(span.Slice(4, 2));
            w.CopyTo(span.Slice(6, 2));
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteU16x4(Span<byte> span, Vec4<ushort> value)
    {
        byte[] x = BitConverter.GetBytes(value[0]);
        byte[] y = BitConverter.GetBytes(value[1]);
        byte[] z = BitConverter.GetBytes(value[2]);
        byte[] w = BitConverter.GetBytes(value[3]);

        try
        {
            x.CopyTo(span.Slice(0, 2));
            y.CopyTo(span.Slice(2, 2));
            z.CopyTo(span.Slice(4, 2));
            w.CopyTo(span.Slice(6, 2));
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteSN16(Span<byte> span, float value)
    {
        float f = MathF.Round(value * short.MaxValue);
        var val = BitConverter.GetBytes(
            MathUtils.ClampSF2SMax(f)
        );

        try
        {
            val.CopyTo(span.Slice(0, 2));
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteUN16(Span<byte> span, float value)
    {
        float f = MathF.Round(value * ushort.MaxValue);
        var val = BitConverter.GetBytes(
            MathUtils.ClampUF2SMax(f)
        );

        try
        {
            val.CopyTo(span.Slice(0, 2));
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteSN16x2(Span<byte> span, Vector2 value)
    {
        byte[] x = BitConverter.GetBytes(
            MathUtils.ClampSF2SMax(
                MathF.Round(value.X * short.MaxValue)
            )
        );

        byte[] y = BitConverter.GetBytes(
            MathUtils.ClampSF2SMax(
                MathF.Round(value.Y * short.MaxValue)
            )
        );

        try
        {
            x.CopyTo(span.Slice(0, 2));
            y.CopyTo(span.Slice(2, 2));
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteUN16x2(Span<byte> span, Vector2 value)
    {
        byte[] x = BitConverter.GetBytes(
            MathUtils.ClampUF2SMax(
                MathF.Round(value.X * ushort.MaxValue)
            )
        );

        byte[] y = BitConverter.GetBytes(
            MathUtils.ClampUF2SMax(
                MathF.Round(value.Y * ushort.MaxValue)
            )
        );

        try
        {
            x.CopyTo(span.Slice(0, 2));
            y.CopyTo(span.Slice(2, 2));
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteSN16x4(Span<byte> span, Vector4 value)
    {
        byte[] x = BitConverter.GetBytes(
            MathUtils.ClampSF2SMax(
                MathF.Round(value.X * short.MaxValue)
            )
        );

        byte[] y = BitConverter.GetBytes(
            MathUtils.ClampSF2SMax(
                MathF.Round(value.Y * short.MaxValue)
            )
        );

        byte[] z = BitConverter.GetBytes(
            MathUtils.ClampSF2SMax(
                MathF.Round(value.Z * short.MaxValue)
            )
        );

        byte[] w = BitConverter.GetBytes(
            MathUtils.ClampSF2SMax(
                MathF.Round(value.W * short.MaxValue)
            )
        );

        try
        {
            x.CopyTo(span.Slice(0, 2));
            y.CopyTo(span.Slice(2, 2));
            z.CopyTo(span.Slice(4, 2));
            w.CopyTo(span.Slice(6, 2));
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteUN16x4(Span<byte> span, Vector4 value)
    {
        byte[] x = BitConverter.GetBytes(
            MathUtils.ClampUF2SMax(
                MathF.Round(value.X * ushort.MaxValue)
            )
        );

        byte[] y = BitConverter.GetBytes(
            MathUtils.ClampUF2SMax(
                MathF.Round(value.Y * ushort.MaxValue)
            )
        );

        byte[] z = BitConverter.GetBytes(
            MathUtils.ClampUF2SMax(
                MathF.Round(value.Z * ushort.MaxValue)
            )
        );

        byte[] w = BitConverter.GetBytes(
            MathUtils.ClampUF2SMax(
                MathF.Round(value.W * ushort.MaxValue)
            )
        );

        try
        {
            x.CopyTo(span.Slice(0, 2));
            y.CopyTo(span.Slice(2, 2));
            z.CopyTo(span.Slice(4, 2));
            w.CopyTo(span.Slice(6, 2));
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteS8(Span<byte> span, sbyte value)
    {
        try
        {
            span[0] = (byte)value; //...?
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteU8(Span<byte> span, byte value)
    {
        try
        {
            span[0] = value; //...?
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteS8x2(Span<byte> span, Vec2<sbyte> value)
    {
        try
        {
            span[0] = (byte)value[0];
            span[1] = (byte)value[1];
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteU8x2(Span<byte> span, Vec2<byte> value)
    {
        try
        {
            span[0] = value[0];
            span[1] = value[1];
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteS8x4(Span<byte> span, Vec4<sbyte> value)
    {
        try
        {
            span[0] = (byte)value[0];
            span[1] = (byte)value[1];
            span[2] = (byte)value[2];
            span[3] = (byte)value[3];
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteU8x4(Span<byte> span, Vec4<byte> value)
    {
        try
        {
            span[0] = value[0];
            span[1] = value[1];
            span[2] = value[2];
            span[3] = value[3];
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteSN8(Span<byte> span, float value)
    {
        float f = MathF.Round(value * sbyte.MaxValue);
        sbyte val = MathUtils.ClampSF2BMax(f);

        try
        {
            span[0] = (byte)val;
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteUN8(Span<byte> span, float value)
    {
        float f = MathF.Round(value * byte.MaxValue);
        byte val = MathUtils.ClampUF2BMax(f);

        try
        {
            span[0] = val;
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteSN8x2(Span<byte> span, Vector2 value)
    {
        sbyte x = MathUtils.ClampSF2BMax(
            MathF.Round(
                value.X * sbyte.MaxValue
            )
        );

        sbyte y = MathUtils.ClampSF2BMax(
            MathF.Round(
                value.Y * sbyte.MaxValue
            )
        );

        try
        {
            span[0] = (byte)x;
            span[1] = (byte)y;
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteUN8x2(Span<byte> span, Vector2 value)
    {
        byte x = MathUtils.ClampUF2BMax(
            MathF.Round(
                value.X * byte.MaxValue
            )
        );

        byte y = MathUtils.ClampUF2BMax(
            MathF.Round(
                value.Y * byte.MaxValue
            )
        );

        try
        {
            span[0] = x;
            span[1] = y;
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteSN8x4(Span<byte> span, Vector4 value)
    {
        sbyte x = MathUtils.ClampSF2BMax(
            MathF.Round(
                value.X * sbyte.MaxValue
            )
        );

        sbyte y = MathUtils.ClampSF2BMax(
            MathF.Round(
                value.Y * sbyte.MaxValue
            )
        );

        sbyte z = MathUtils.ClampSF2BMax(
            MathF.Round(
                value.Z * sbyte.MaxValue
            )
        );

        sbyte w = MathUtils.ClampSF2BMax(
            MathF.Round(
                value.W * sbyte.MaxValue
            )
        );

        try
        {
            span[0] = (byte)x;
            span[1] = (byte)y;
            span[2] = (byte)z;
            span[3] = (byte)w;
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteUN8x4(Span<byte> span, Vector4 value)
    {
        byte x = MathUtils.ClampUF2BMax(
            MathF.Round(
                value.X * byte.MaxValue
            )
        );

        byte y = MathUtils.ClampUF2BMax(
            MathF.Round(
                value.Y * byte.MaxValue
            )
        );

        byte z = MathUtils.ClampUF2BMax(
            MathF.Round(
                value.Z * byte.MaxValue
            )
        );

        byte w = MathUtils.ClampUF2BMax(
            MathF.Round(
                value.W * byte.MaxValue
            )
        );

        try
        {
            span[0] = x;
            span[1] = y;
            span[2] = z;
            span[3] = w;
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    //HERE BE DRAGONS: it looks like z is actually the 10-bit-wide value, not x.
    // will cross bridge when find bridge troll.
    public static bool WriteSN10_SN11_SN11(Span<byte> span, Vector3 value)
    {
        float x = MathUtils.ClampA1(value.X);
        float y = MathUtils.ClampA1(value.Y);
        float z = MathUtils.ClampA1(value.Z);

        uint cx;
        if (x < 0)
        {
            cx = (~(uint)(int)(x * -1024.0f) + 1);
        }
        else
        {
            cx = ((uint)(int)(x * 1023.0f));
        }

        cx &= 0x7ff;

        uint cy;
        if (y < 0)
        {
            cy = (~(uint)(int)(y * -1024.0f) + 1);
        }
        else
        {
            cy = (uint)(int)(y * 1023.0f);
        }

        cy &= 0x7ff;

        uint cz;
        if (z < 0)
        {
            cz = (~(uint)(int)(z * -512.0f) + 1);
        }
        else
        {
            cz = (uint)(int)(z * 511.0f);
        }

        // x *= x < 0 ? 512 : 511;
        // y *= y < 0 ? 1024 : 1023;
        // z *= z < 0 ? 1024 : 1023;

        uint outValue = ((cz << 11) | cy) << 11 | cx;

        byte[] outBytes = BitConverter.GetBytes(outValue);

        try
        {
            outBytes.CopyTo(span);
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteSN10x3_SN2(Span<byte> span, Vector4 value)
    {
        float x = MathUtils.ClampA1(value.X);
        float y = MathUtils.ClampA1(value.Y);
        float z = MathUtils.ClampA1(value.Z);
        float w = MathUtils.ClampA1(value.W);

        uint xc = ((uint)(int)(x * 511.0f) & 0x3ff);
        uint yc = ((uint)(int)(y * 511.0f) & 0x3ff);
        uint zc = ((uint)(int)(z * 511.0f) & 0x3ff);
        uint wc = ((uint)(int)w);

        uint outValue = xc | yc << 10 | zc << 20 | wc << 30;

        byte[] outBytes = BitConverter.GetBytes(outValue);

        try
        {
            outBytes.CopyTo(span);
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteUN10x3_UN2(Span<byte> span, Vector4 value)
    {
        float x = MathUtils.Clamp01(value.X);
        float y = MathUtils.Clamp01(value.Y);
        float z = MathUtils.Clamp01(value.Z);

        float w = 1.0f;
        if (value.W < 1.0f)
        {
            w = value.W;
        }

        uint cx = (uint)(x * 1023.0f) & 0x3ff;
        uint cy = (uint)(y * 1023.0f) & 0x3ff;
        uint cz = (uint)(z * 1023.0f) & 0x3ff;
        uint cw = (uint)(w * 3) & 0x3ff;

        uint outValue = cx | cy << 10 | cz << 20 | cw << 30;

        byte[] outBytes = BitConverter.GetBytes(outValue);

        try
        {
            outBytes.CopyTo(span);
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    public static bool WriteD3DColor(Span<byte> span, Vector3 value) =>
        throw new NotImplementedException(
            "Writing D3DColor is not yet implemented! Contact me with the file you are trying to write if you see this :3");
    
    
    public static bool WriteTelltaleBoneWeights(Span<byte> span, Vector4 value)
    {
        if (span.Length < 4)
            return false;

        float weight2 = value.Y;
        float weight3 = value.Z;
        float weight4 = value.W;

        uint additiveBits = (uint)Math.Clamp((int)MathF.Floor(weight2 * 8.0f), 0, 3);

        float weight2Base = additiveBits / 8.0f;

        uint weight2Raw = Quantize10Bit((weight2 - weight2Base) * 8.0f);
        uint weight3Raw = Quantize10Bit(weight3 * 3.0f);
        uint weight4Raw = Quantize10Bit(weight4 * 4.0f);

        uint outValue =
            (weight2Raw & 0x3FF) |
            ((weight3Raw & 0x3FF) << 10) |
            ((weight4Raw & 0x3FF) << 20) |
            ((additiveBits & 0x3) << 30);

        byte[] outBytes = BitConverter.GetBytes(outValue);

        try
        {
            outBytes.CopyTo(span);
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }

    private static uint Quantize10Bit(float value)
    {
        if (float.IsNaN(value))
            return 0;

        float rounded = MathF.Round(value * 1023.0f, MidpointRounding.AwayFromZero);
        return (uint)Math.Clamp(rounded, 0.0f, 1023.0f);
    }
}