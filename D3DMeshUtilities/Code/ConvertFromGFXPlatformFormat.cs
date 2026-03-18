using System.Numerics;
using TelltaleToolKit.T3Types.Meshes.T3Types;

// ReSharper disable InconsistentNaming

namespace D3DMeshUtilities.Code;


/// <summary>
/// This class is designed to contain implementations for reading any GFXPlatformFormat tuple
/// from a ReadOnlySpan of bytes.
///
/// <see cref="TelltaleToolKit.T3Types.Meshes.T3Types.GFXPlatformFormat"/>
/// </summary>
public static class ConvertFromGfxPlatformFormat
{    
    
    private const uint TEN_BIT_MASK = 0x3FF;
    private const uint NINE_BIT_MASK = 0x1FF;
    private const uint ONE_BYTE_MASK = 0xFF;
    private const uint TWO_BIT_MASK = 0x3;
    private const uint ELEVEN_BIT_MASK = 0x7FF;
    
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

    public static bool IsFormatScalar(GFXPlatformFormat format)
    {
        return format switch
        {
            GFXPlatformFormat.F32 => true,
            GFXPlatformFormat.S32 => true,
            GFXPlatformFormat.U32 => true,
            GFXPlatformFormat.S16 => true,
            GFXPlatformFormat.U16 => true,
            GFXPlatformFormat.SN16 => true,
            GFXPlatformFormat.UN16 => true,
            GFXPlatformFormat.S8 => true,
            GFXPlatformFormat.U8 => true,
            GFXPlatformFormat.SN8 => true,
            GFXPlatformFormat.UN8 => true,
            _ => false
        };
    }

    public static bool IsFormatFloat(GFXPlatformFormat format)
    {
        return format switch
        {
            GFXPlatformFormat.F32 => true,
            GFXPlatformFormat.SN16 => true,
            GFXPlatformFormat.UN16 => true,
            GFXPlatformFormat.SN8 => true,
            GFXPlatformFormat.UN8 => true,
            _ => false
        };
    }

    public static bool IsFormatScalarUnsignedInteger(GFXPlatformFormat format)
    {
        return format switch
        {
            GFXPlatformFormat.U32 => true,
            GFXPlatformFormat.U16 => true,
            GFXPlatformFormat.U8 => true,
            _ => false
        };
    }
    
    public static bool IsFormatInteger(GFXPlatformFormat format)
    {
        return format switch
        {
            GFXPlatformFormat.S32 => true,
            GFXPlatformFormat.U32 => true,
            GFXPlatformFormat.S16 => true,
            GFXPlatformFormat.U16 => true,
            GFXPlatformFormat.S8 => true,
            GFXPlatformFormat.U8 => true,
            _ => false
        };
    }

    public static bool IsTemplatedVector(GFXPlatformFormat format)
    {
        return format switch
        {
            GFXPlatformFormat.S32x2 => true,
            GFXPlatformFormat.U32x2 => true,
            GFXPlatformFormat.S32x3 => true,
            GFXPlatformFormat.U32x3 => true,
            GFXPlatformFormat.S32x4 => true,
            GFXPlatformFormat.U32x4 => true,
            GFXPlatformFormat.S16x2 => true,
            GFXPlatformFormat.U16x2 => true,
            GFXPlatformFormat.S16x4 => true,
            GFXPlatformFormat.U16x4 => true,
            GFXPlatformFormat.S8x2 => true,
            GFXPlatformFormat.U8x2 => true,
            GFXPlatformFormat.S8x4 => true,
            GFXPlatformFormat.U8x4 => true,
            _ => false
        };
    }
    
    public static bool IsFormatVector2(GFXPlatformFormat format)
    {
        return format switch
        {
            GFXPlatformFormat.F32x2 => true,
            GFXPlatformFormat.F16x2 => true,
            GFXPlatformFormat.SN16x2 => true,
            GFXPlatformFormat.UN16x2 => true,
            GFXPlatformFormat.SN8x2 => true,
            GFXPlatformFormat.UN8x2 => true,
            _ => false
        };
    }

    public static bool IsFormatVector3(GFXPlatformFormat format)
    {
        return format switch
        {
            GFXPlatformFormat.F32x3 => true,
            GFXPlatformFormat.SN10_SN11_SN11 => true,
            _ => false
        };
    }

    public static bool IsFormatVector4(GFXPlatformFormat format)
    {
        return format switch
        {
            GFXPlatformFormat.F32x4 => true,
            GFXPlatformFormat.F16x4 => true,
            GFXPlatformFormat.SN16x4 => true,
            GFXPlatformFormat.UN16x4 => true,
            GFXPlatformFormat.SN8x4 => true,
            GFXPlatformFormat.UN8x4 => true,
            GFXPlatformFormat.SN10x3_SN2 => true,
            GFXPlatformFormat.UN10x3_UN2 => true,
            _ => false
        };
    }
    
    //implied "format" of this is UN10x3_UN2, but Telltale does some Interesting Things here...
    public static Vector4 ReadTelltaleBoneWeights(ReadOnlySpan<byte> data)
    {
        uint packed = ReadUInt32(data);

        uint additiveBits = (packed >> 30);
        uint weight2Raw = packed & 0x3FF;
        uint weight3Raw = (packed >> 10) & 0x3FF;
        uint weight4Raw = (packed >> 20) & 0x3FF;

        float weight2 = ((weight2Raw / 1023.0f) / 8.0f) + (additiveBits / 8.0f);
        float weight3 = (weight3Raw / 1023.0f) / 3.0f;
        float weight4 = (weight4Raw / 1023.0f) / 4.0f;
        float weight1 = 1.0f - weight2 - weight3 - weight4;

        return new Vector4(weight1, weight2, weight3, weight4);
    }
    
    /// <summary>
    /// This will safely read the vertex from the first few elements of span,
    /// and will return null if it can't convert
    /// </summary>
    /// <param name="span">Span to read from</param>
    /// <param name="format">Format of the vertices contained within span</param>
    /// <returns>Vector containing the first vertex in the span</returns>
    /// <exception cref="ArgumentOutOfRangeException">if you've got an error in your span!</exception>
    public static object? ReadVertexFromFormat(ReadOnlySpan<byte> span, GFXPlatformFormat format)
    {
        return format switch
        {
            GFXPlatformFormat.None => null,
            GFXPlatformFormat.F32 => ReadF32(span),
            GFXPlatformFormat.F32x2 => ReadF32x2(span),
            GFXPlatformFormat.F32x3 => ReadF32x3(span),
            GFXPlatformFormat.F32x4 => ReadF32x4(span),
            GFXPlatformFormat.F16x2 => ReadF16x2(span),
            GFXPlatformFormat.F16x4 => ReadF16x4(span),
            GFXPlatformFormat.S32 => ReadS32(span),
            GFXPlatformFormat.U32 => ReadU32(span),
            GFXPlatformFormat.S32x2 => ReadS32x2(span),
            GFXPlatformFormat.U32x2 => ReadU32x2(span),
            GFXPlatformFormat.S32x3 => ReadS32x3(span),
            GFXPlatformFormat.U32x3 => ReadU32x3(span),
            GFXPlatformFormat.S32x4 => ReadS32x4(span),
            GFXPlatformFormat.U32x4 => ReadU32x4(span),
            GFXPlatformFormat.S16 => ReadS16(span),
            GFXPlatformFormat.U16 => ReadU16(span),
            GFXPlatformFormat.S16x2 => ReadS16x2(span),
            GFXPlatformFormat.U16x2 => ReadU16x2(span),
            GFXPlatformFormat.S16x4 => ReadS16x4(span),
            GFXPlatformFormat.U16x4 => ReadU16x4(span),
            GFXPlatformFormat.SN16 => ReadSN16(span),
            GFXPlatformFormat.UN16 => ReadUN16(span),
            GFXPlatformFormat.SN16x2 => ReadSN16x2(span),
            GFXPlatformFormat.UN16x2 => ReadUN16x2(span),
            GFXPlatformFormat.SN16x4 => ReadSN16x4(span),
            GFXPlatformFormat.UN16x4 => ReadUN16x4(span),
            GFXPlatformFormat.S8 => ReadS8(span),
            GFXPlatformFormat.U8 => ReadU8(span),
            GFXPlatformFormat.S8x2 => ReadS8x2(span),
            GFXPlatformFormat.U8x2 => ReadU8x2(span),
            GFXPlatformFormat.S8x4 => ReadS8x4(span),
            GFXPlatformFormat.U8x4 => ReadU8x4(span),
            GFXPlatformFormat.SN8 => ReadSN8(span),
            GFXPlatformFormat.UN8 => ReadUN8(span),
            GFXPlatformFormat.SN8x2 => ReadSN8x2(span),
            GFXPlatformFormat.UN8x2 => ReadUN8x2(span),
            GFXPlatformFormat.SN8x4 => ReadSN8x4(span),
            GFXPlatformFormat.UN8x4 => ReadUN8x4(span),
            GFXPlatformFormat.SN10_SN11_SN11 => ReadSN10_SN11_SN11(span),
            GFXPlatformFormat.SN10x3_SN2 => ReadSN10x3_SN2(span),
            GFXPlatformFormat.UN10x3_UN2 => ReadUN10x3_UN2(span),
            GFXPlatformFormat.D3DCOLOR => ReadD3DColor(span),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }


    /// <summary>
    /// Returns a float if the format defines reading a float or normalized int
    /// </summary>
    /// <param name="span"> span </param>
    /// <param name="format"> format </param>
    /// <returns>null if the format does not define reading a float</returns>
    public static float? ReadFloatFromSpanAndFormat(ReadOnlySpan<byte> span, GFXPlatformFormat format)
    {
        object? vert = ReadVertexFromFormat(span, format);

        return vert is float f ? f : null;
    }

    public static Vector2? ReadVector2FromSpanAndFormat(ReadOnlySpan<byte> span, GFXPlatformFormat format)
    {
        object? vert = ReadVertexFromFormat(span, format);

        return vert is Vector2 v ? v : null;
    }

    public static Vector3? ReadVector3FromSpanAndFormat(ReadOnlySpan<byte> span, GFXPlatformFormat format)
    {
        object? vert = ReadVertexFromFormat(span, format);

        return vert is Vector3 v ? v : null;
    }
    
    public static Vector4? ReadVector4FromSpanAndFormat(ReadOnlySpan<byte> span, GFXPlatformFormat format)
    {
        object? vert = ReadVertexFromFormat(span, format);

        return vert is Vector4 v ? v : null;
    }

    public static uint? ReadUIntFromSpanAndFormat(ReadOnlySpan<byte> span, GFXPlatformFormat format)
    {
        object? element = ReadVertexFromFormat(span, format);

        uint ret;
        
        // return element is uint ? (uint)element : null;
        if (element is byte b)
            ret = b;
        else if (element is ushort s)
            ret = s;
        else if (element is uint u)
            ret = u;
        else
            return null;

        return ret;
    }
    
    //todo: check that this will work...
    public static float ReadF32(ReadOnlySpan<byte> span)
    {
        //little endian (?) reading
        return BitConverter.ToSingle(span.Slice(0, 4));
    }
    //
    // public static float ReadF32_BE(ReadOnlySpan<byte> span)
    // {
    //     
    //     //big endian (?) reading
    //     return BitConverter.ToSingle(new []{ span[3], span[2], span[1], span[0] });
    //
    // }

    public static Vector2 ReadF32x2(ReadOnlySpan<byte> span)
    {
        float x = BitConverter.ToSingle(span.Slice(0, 4));
        float y = BitConverter.ToSingle(span.Slice(4, 4));

        return new Vector2(x, y);
    }
    
    // public static Vector2 ReadF32x2_BE(ReadOnlySpan<byte> span)
    // {
    //     float x = ReadF32_BE(span.Slice(0, 4));
    //     float y = ReadF32_BE(span.Slice(4, 4));
    //
    //     return new Vector2(x, y);
    // }

    public static Vector3 ReadF32x3(ReadOnlySpan<byte> span)
    {
        float x = BitConverter.ToSingle(span.Slice(0, 4));
        float y = BitConverter.ToSingle(span.Slice(4, 4));
        float z = BitConverter.ToSingle(span.Slice(8, 4));

        return new Vector3(x, y, z);
    }

    // public static Vector3 ReadF32x3_BE(ReadOnlySpan<byte> span)
    // {
    //     float x = ReadF32_BE(span.Slice(0, 4));
    //     float y = ReadF32_BE(span.Slice(4, 4));
    //     float z = ReadF32_BE(span.Slice(8, 4));
    //
    //     return new Vector3(x, y, z);
    // }

    public static Vector4 ReadF32x4(ReadOnlySpan<byte> span)
    {
        float x = BitConverter.ToSingle(span.Slice(0, 4));
        float y = BitConverter.ToSingle(span.Slice(4, 4));
        float z = BitConverter.ToSingle(span.Slice(8, 4));
        float w = BitConverter.ToSingle(span.Slice(12, 4));

        return new Vector4(x, y, z, w);
    }
    
    // public static Vector4 ReadF32x4_BE(ReadOnlySpan<byte> span)
    // {
    //     float x = ReadF32_BE(span.Slice(0, 4));
    //     float y = ReadF32_BE(span.Slice(4, 4));
    //     float z = ReadF32_BE(span.Slice(8, 4));
    //     float w = ReadF32_BE(span.Slice(12, 4));
    //
    //     return new Vector4(x, y, z, w);
    // }

    public static Vector2 ReadF16x2(ReadOnlySpan<byte> span)
    {
        Half x = BitConverter.ToHalf(span.Slice(0, 2));
        Half y = BitConverter.ToHalf(span.Slice(2, 2));

        return new Vector2((float)x, (float)y);
    }
    
    public static Vector4 ReadF16x4(ReadOnlySpan<byte> span)
    {
        Half x = BitConverter.ToHalf(span.Slice(0, 2));
        Half y = BitConverter.ToHalf(span.Slice(2, 2));
        Half z = BitConverter.ToHalf(span.Slice(4, 2));
        Half w = BitConverter.ToHalf(span.Slice(6, 2));

        return new Vector4((float)x, (float)y, (float)z, (float)w);
    }

    public static int ReadS32(ReadOnlySpan<byte> span)
    {
        int i = BitConverter.ToInt32(span.Slice(0, 4));

        return i;
    }

    public static uint ReadU32(ReadOnlySpan<byte> span)
    {
        uint i = BitConverter.ToUInt32(span.Slice(0, 4));

        return i;
    }

    public static Vector<int> ReadS32x2(ReadOnlySpan<byte> span)
    {
        return new Vector<int>(new[]
        {
            ReadS32(span.Slice(0, 4)), 
            ReadS32(span.Slice(4, 4))
        });
    }
    
    public static Vector<uint> ReadU32x2(ReadOnlySpan<byte> span)
    {
        return new Vector<uint>(new[]
        {
            ReadU32(span.Slice(0, 4)), 
            ReadU32(span.Slice(4, 4))
        });
    }


    public static Vector<int> ReadS32x3(ReadOnlySpan<byte> span)
    {
        return new Vector<int>(new[]
        {
            ReadS32(span.Slice(0, 4)), 
            ReadS32(span.Slice(4, 4)),
            ReadS32(span.Slice(8, 4))
        });

    }
    
    public static Vector<uint> ReadU32x3(ReadOnlySpan<byte> span)
    {
        return new Vector<uint>(new[]
        {
            ReadU32(span.Slice(0, 4)), 
            ReadU32(span.Slice(4, 4)),
            ReadU32(span.Slice(8, 4))
        });

    }
    
    public static Vector<int> ReadS32x4(ReadOnlySpan<byte> span)
    {
        return new Vector<int>(new[]
        {
            ReadS32(span.Slice(0, 4)), 
            ReadS32(span.Slice(4, 4)),
            ReadS32(span.Slice(8, 4)),
            ReadS32(span.Slice(12, 4))
        });

    }
    
    public static Vector<uint> ReadU32x4(ReadOnlySpan<byte> span)
    {
        return new Vector<uint>(new[]
        {
            ReadU32(span.Slice(0, 4)), 
            ReadU32(span.Slice(4, 4)),
            ReadU32(span.Slice(8, 4)),
            ReadU32(span.Slice(12, 4))
        });

    }

    public static short ReadS16(ReadOnlySpan<byte> span)
    {
        return BitConverter.ToInt16(span.Slice(0, 2));
    }

    public static ushort ReadU16(ReadOnlySpan<byte> span)
    {
        return BitConverter.ToUInt16(span.Slice(0, 2));
    }

    public static Vector<short> ReadS16x2(ReadOnlySpan<byte> span)
    {
        return new Vector<short>(new[]
        {
            ReadS16(span.Slice(0, 2)),
            ReadS16(span.Slice(2, 2))
        });
    }
    
    public static Vector<ushort> ReadU16x2(ReadOnlySpan<byte> span)
    {
        return new Vector<ushort>(new[]
        {
            ReadU16(span.Slice(0, 2)),
            ReadU16(span.Slice(2, 2))
        });
    }
    
    public static Vector<short> ReadS16x4(ReadOnlySpan<byte> span)
    {
        return new Vector<short>(new[]
        {
            ReadS16(span.Slice(0, 2)),
            ReadS16(span.Slice(2, 2)),
            ReadS16(span.Slice(4, 2)),
            ReadS16(span.Slice(6, 2))
        });
    }

    public static Vector<ushort> ReadU16x4(ReadOnlySpan<byte> span)
    {
        return new Vector<ushort>(new[]
        {
            ReadU16(span.Slice(0, 2)),
            ReadU16(span.Slice(2, 2)),
            ReadU16(span.Slice(4, 2)),
            ReadU16(span.Slice(6, 2))
        });
    }
    
    public static float ReadSN16(ReadOnlySpan<byte> span)
    {
        short s = BitConverter.ToInt16(span.Slice(0, 2));
        
        return s < 0 ? (s / 32768.0f) : (s / 32767.0f);
    }

    public static float ReadUN16(ReadOnlySpan<byte> span)
    {
        return BitConverter.ToUInt16(span.Slice(0, 2)) / 65535.0f;
    }

    public static Vector2 ReadSN16x2(ReadOnlySpan<byte> span)
    {
        return new Vector2(
            ReadSN16(span.Slice(0, 2)),
            ReadSN16(span.Slice(2, 2))
        );
    }
    
    public static Vector2 ReadUN16x2(ReadOnlySpan<byte> span)
    {
        return new Vector2(
            ReadUN16(span.Slice(0, 2)),
            ReadUN16(span.Slice(2, 2))
        );
    }
    
    public static Vector4 ReadSN16x4(ReadOnlySpan<byte> span)
    {
        return new Vector4(
            ReadSN16(span.Slice(0, 2)),
            ReadSN16(span.Slice(2, 2)),
            ReadSN16(span.Slice(4, 2)),
            ReadSN16(span.Slice(6, 2))
        );
    }

    public static Vector4 ReadUN16x4(ReadOnlySpan<byte> span)
    {
        return new Vector4(
            ReadUN16(span.Slice(0, 2)),
            ReadUN16(span.Slice(2, 2)),
            ReadUN16(span.Slice(4, 2)),
            ReadUN16(span.Slice(6, 2))
        );
    }

    public static sbyte ReadS8(ReadOnlySpan<byte> data)
    {
        unchecked
        {
            return (sbyte)data[0];
        }
    }

    public static byte ReadU8(ReadOnlySpan<byte> data)
    {
        return data[0];
    }

    public static Vector<sbyte> ReadS8x2(ReadOnlySpan<byte> data)
    {
        unchecked
        {
            return new Vector<sbyte>(new[]
            {
                (sbyte)data[0],
                (sbyte)data[1]
            });
        }
    }

    public static Vector<byte> ReadU8x2(ReadOnlySpan<byte> data)
    {
        return new Vector<byte>(data[..2]);
    }

    public static Vector<sbyte> ReadS8x4(ReadOnlySpan<byte> data)
    {
        unchecked
        {
            return new Vector<sbyte>(new[]
            {
                (sbyte)data[0],
                (sbyte)data[1],
                (sbyte)data[2],
                (sbyte)data[3]
            });
        }
    }

    public static Vector<byte> ReadU8x4(ReadOnlySpan<byte> data)
    {
        return new Vector<byte>(data[..4]);
    }

    public static float ReadSN8(ReadOnlySpan<byte> data)
    {
        unchecked
        {
            return ((sbyte)data[0]) / ((data[0] & 0x80) == 0 ? 127.0f : 128.0f);
        }
    }

    public static float ReadUN8(ReadOnlySpan<byte> data)
    {
        return data[0] / 255.0f;
    }

    public static Vector2 ReadSN8x2(ReadOnlySpan<byte> data)
    {
        unchecked
        {
            return new Vector2(
                ((sbyte)data[0]) / ((data[0] & 0x80) == 0 ? 127.0f : 128.0f),
                ((sbyte)data[1]) / ((data[1] & 0x80) == 0 ? 127.0f : 128.0f)
            );
        }
    }

    public static Vector2 ReadUN8x2(ReadOnlySpan<byte> data)
    {
        return new Vector2(
            data[0] / 255.0f,
            data[1] / 255.0f
        );
    }
    
    public static Vector4 ReadSN8x4(ReadOnlySpan<byte> data)
    {
        unchecked {
            sbyte x = (sbyte)data[0];
            sbyte y = (sbyte)data[1];
            sbyte z = (sbyte)data[2];
            sbyte w = (sbyte)data[3];

            float sxn = x / ((x & 0x80) == 0 ? 127.0f : 128.0f);
            float syn = y / ((y & 0x80) == 0 ? 127.0f : 128.0f);
            float szn = z / ((z & 0x80) == 0 ? 127.0f : 128.0f);
            float swn = w / ((w & 0x80) == 0 ? 127.0f : 128.0f);

            return new Vector4(sxn, syn, szn, swn);
        }
    }

    public static Vector4 ReadUN8x4(ReadOnlySpan<byte> data)
    {
        return new Vector4(
                data[0] / 255.0f,
                data[1] / 255.0f,
                data[2] / 255.0f,
                data[3] / 255.0f
            );
    }

    public static Vector3 ReadSN10_SN11_SN11(ReadOnlySpan<byte> data)
    {
        //read data
        uint packed = ReadUInt32(data);
        
        uint x = packed & TEN_BIT_MASK;
        uint y = (packed >> 10) & ELEVEN_BIT_MASK;
        uint z = (packed >> 21) & ELEVEN_BIT_MASK;

        
        //sign and normalize
        
        float snx;
        
        //x is negative
        if ((x & (1 << 9)) != 0)
        {
            snx = -((ToPositive(x) & NINE_BIT_MASK) / 512.0f);
        }
        //x is positive
        else
        {
            snx = x / 511.0f;
        }

        float sny;
        
        //y is negative
        if ((y & (1 << 10)) != 0)
        {
            sny = -((ToPositive(y) & TEN_BIT_MASK) / 1024.0f);
        }
        //y is positive
        else
        {
            sny = y / 1023.0f;
        }

        float snz;

        //z is negative
        if ((z & (1 << 10)) != 0)
        {
            snz = -((ToPositive(z) & TEN_BIT_MASK) / 1024.0f);
        }
        //z is positive
        else
        {
            snz = z / 1023.0f;
        }

        //return
        return new Vector3(
            snx,
            sny,
            snz
        );
    }
    
    public static Vector4 ReadSN10x3_SN2(ReadOnlySpan<byte> data)
    {
        //read data
        uint packed = ReadUInt32(data);

        uint x = packed & TEN_BIT_MASK;
        uint y = (packed >> 10) & TEN_BIT_MASK;
        uint z = (packed >> 10) & TEN_BIT_MASK;
        uint w = (packed >> 30) & TWO_BIT_MASK;
        
        //sign and normalize
        
        float snx;
        
        //x is negative
        if ((x & (1 << 9)) != 0)
        {
            snx = -((ToPositive(x) & NINE_BIT_MASK) / 512.0f);
        }
        //x is positive
        else
        {
            snx = x / 511.0f;
        }

        float sny;
        
        //y is negative
        if ((y & (1 << 9)) != 0)
        {
            sny = -(ToPositive(y, NINE_BIT_MASK) / 512.0f);
        }
        //y is positive
        else
        {
            sny = y / 1023.0f;
        }

        float snz;

        //z is negative
        if ((z & (1 << 9)) != 0)
        {
            snz = -((ToPositive(z, NINE_BIT_MASK))  / 512.0f);
        }
        //z is positive
        else
        {
            snz = z / 511.0f;
        }

        float snw;
        
        //w is negative
        if ((w & 0b10) != 0)
        {
            snw = -(ToPositive(w, 0b1) / 2.0f);
        }
        else
        {
            snw = w / 1.0f;
        }

        return new Vector4(snx, sny, snz, snw);

    }
    

    
    public static Vector4 ReadUN10x3_UN2(ReadOnlySpan<byte> data)
    {
        uint packed = ReadUInt32(data);

        uint x = packed & TEN_BIT_MASK;
        uint y = (packed >> 10) & TEN_BIT_MASK;
        uint z = (packed >> 20) & TEN_BIT_MASK;
        uint w = (packed >> 30) & TWO_BIT_MASK;

        float xn = x / 1023.0f;
        float yn = y / 1023.0f;
        float zn = z / 1023.0f;
        float wn = w / 3.0f;

        return new Vector4(xn, yn, zn, wn);

    }

    public static Vector3? ReadD3DColor(ReadOnlySpan<byte> span)
    {
        throw new NotImplementedException("Reading D3DColor is not yet implemented! Contact me with the file you read if you see this :3");
    }

    public static uint ToPositive(uint x)
    {
        return ~(x) + 1;
    }

    public static uint ToPositive(uint x, uint mask)
    {
        return ToPositive(x) & mask;
    }

    private static uint ReadUInt32(ReadOnlySpan<byte> data)
    {
        return (((uint)(data[3]) << 24) + ((uint)data[2] << 16) + ((uint)data[1] << 8) + (data[0]));
    }
    
}