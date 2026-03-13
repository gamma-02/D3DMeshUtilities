using System.Numerics;
using TelltaleToolKit.T3Types.Meshes;
using TelltaleToolKit.T3Types.Meshes.T3Types;

public static class Decompressor
{
    public static List<Vector4> Decompress(byte[] buffer, T3VertexBuffer vertexBuffer,
        D3DMesh.T3VertexComponentType vertexComponentType)
    {
        if (buffer == null || buffer.Length == 0)
            return [];

        int totalElements = buffer.Length / vertexBuffer.VertSize;
        var result = new List<Vector4>(totalElements);

        T3VertexComponent vertexComponentInfo = vertexBuffer.VertexComponents[(int)vertexComponentType];

        if (vertexComponentInfo.Type == T3VertexComponent.EnumType.VTypeNone)
        {
            return [];
        }

        var elementSize = (int)(vertexComponentInfo.Type.GetTypeSize() * vertexComponentInfo.Count);

        // Use spans for efficient memory access
        ReadOnlySpan<byte> bufferSpan = buffer;

        for (var i = 0; i < totalElements; i++)
        {
            var offset = (int)(i * vertexBuffer.VertSize + vertexComponentInfo.Offset);
            if (offset + elementSize > buffer.Length)
                break;

            ReadOnlySpan<byte> slice = bufferSpan.Slice(offset, elementSize);
            result.Add(ReadVector4(slice, vertexComponentInfo, vertexComponentType));
        }

        return result;
    }

    public static List<Vector4> Decompress(byte[] buffer, uint stride, GFXPlatformAttributeParams attribute)
    {
        if (buffer.Length == 0)
            return [];

        var totalElements = (int)(buffer.Length / stride);
        var result = new List<Vector4>(totalElements);

        int elementSize = attribute.Format.GetElementSize();

        // Use spans for efficient memory access
        ReadOnlySpan<byte> bufferSpan = buffer;

        for (var i = 0; i < totalElements; i++)
        {
            var offset = (int)(i * stride + attribute.BufferOffset);
            if (offset + elementSize > buffer.Length)
                break;

            ReadOnlySpan<byte> slice = bufferSpan.Slice(offset, elementSize);
            result.Add(ReadVector4(slice, attribute.Format,
                attribute.Attribute == GFXPlatformVertexAttribute.BlendWeight));
        }

        return result;
    }

// Even more efficient - return span or array to avoid list overhead
    static Vector4[] DecompressToArray(byte[] buffer, GFXPlatformFormat format)
    {
        if (buffer == null || buffer.Length == 0)
            return [];

        int elementSize = format.GetElementSize();
        int totalElements = buffer.Length / elementSize;

        var result = new Vector4[totalElements];
        ReadOnlySpan<byte> bufferSpan = buffer;

        for (int i = 0; i < totalElements; i++)
        {
            int offset = i * elementSize;
            var slice = bufferSpan.Slice(offset, elementSize);
            result[i] = ReadVector4(slice, format);
        }

        return result;
    }

// Most efficient - process directly without creating new collections
    static void DecompressToSpan(ReadOnlySpan<byte> buffer, GFXPlatformFormat format, Span<Vector4> output)
    {
        int elementSize = GetElementSize(format);
        int totalElements = Math.Min(buffer.Length / elementSize, output.Length);

        for (int i = 0; i < totalElements; i++)
        {
            int offset = i * elementSize;
            var slice = buffer.Slice(offset, elementSize);
            output[i] = ReadVector4(slice, format);
        }
    }

    static Vector4 ReadTelltaleBoneWeights(ReadOnlySpan<byte> data)
    {
        uint packed = ReadUInt32(data);

        // Telltale's custom bone weight decoding (NOT standard UN10x3_UN2)
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

    static Vector4 ReadTelltaleBoneIndex(ReadOnlySpan<byte> data, T3VertexComponent component)
    {
        return component.Type switch
        {
            T3VertexComponent.EnumType.VTypeS8N => new Vector4(ReadSByte(data, 0) / 4.0f,
                ReadSByte(data, 1) / 4.0f, ReadSByte(data, 2) / 4.0f,
                ReadSByte(data, 3) / 4.0f),
            T3VertexComponent.EnumType.VTypeS8NBones => new Vector4(ReadSByte(data, 0) / 3.0f,
                ReadSByte(data, 1) / 3.0f, ReadSByte(data, 2) / 3.0f,
                ReadSByte(data, 3) / 3.0f),
            _ => Vector4.Zero
        };
    }

    static Vector4 ReadVector4(ReadOnlySpan<byte> data, T3VertexComponent component,
        D3DMesh.T3VertexComponentType vertexComponentType)
    {
        if (vertexComponentType is D3DMesh.T3VertexComponentType.BlendIndex &&
            component.Type is T3VertexComponent.EnumType.VTypeS8N or T3VertexComponent.EnumType.VTypeS8NBones)
        {
            return ReadTelltaleBoneIndex(data, component);
        }

        static Vector4 FromXYZ(float x, float y, float z) => new Vector4(x, y, z, 1);
        static Vector4 FromXY(float x, float y) => new Vector4(x, y, 0f, 1);
        static Vector4 FromX(float x) => new Vector4(x, 0f, 0f, 1);
        static Vector4 FromXYZW(float x, float y, float z, float w) => new Vector4(x, y, z, w);

        switch (component.Type)
        {
            case T3VertexComponent.EnumType.VTypeFloat:
                // float = 4 bytes each
                return component.Count switch
                {
                    1 => FromX(ReadFloat(data, 0)),
                    2 => FromXY(ReadFloat(data, 0), ReadFloat(data, 4)),
                    3 => FromXYZ(ReadFloat(data, 0), ReadFloat(data, 4), ReadFloat(data, 8)),
                    4 => FromXYZW(ReadFloat(data, 0), ReadFloat(data, 4), ReadFloat(data, 8), ReadFloat(data, 12)),
                    _ => throw new ArgumentOutOfRangeException(nameof(component.Count),
                        $"Invalid float component count: {component.Count}")
                };

            case T3VertexComponent.EnumType.VTypeS8N:
                // signed 8-bit normalized -> 1 byte each
                return component.Count switch
                {
                    1 => FromX(NormalizeSigned8(ReadSByte(data, 0))),
                    2 => FromXY(NormalizeSigned8(ReadSByte(data, 0)), NormalizeSigned8(ReadSByte(data, 1))),
                    3 => FromXYZ(NormalizeSigned8(ReadSByte(data, 0)), NormalizeSigned8(ReadSByte(data, 1)),
                        NormalizeSigned8(ReadSByte(data, 2))),
                    4 => FromXYZW(NormalizeSigned8(ReadSByte(data, 0)), NormalizeSigned8(ReadSByte(data, 1)),
                        NormalizeSigned8(ReadSByte(data, 2)), NormalizeSigned8(ReadSByte(data, 3))),
                    _ => throw new ArgumentOutOfRangeException(nameof(component.Count),
                        $"Invalid S8N component count: {component.Count}")
                };

            case T3VertexComponent.EnumType.VTypeU8N:
                // unsigned 8-bit normalized -> 1 byte each
                return component.Count switch
                {
                    1 => FromX(NormalizeUnsigned8(ReadByte(data, 0))),
                    2 => FromXY(NormalizeUnsigned8(ReadByte(data, 0)), NormalizeUnsigned8(ReadByte(data, 1))),
                    3 => FromXYZ(NormalizeUnsigned8(ReadByte(data, 0)), NormalizeUnsigned8(ReadByte(data, 1)),
                        NormalizeUnsigned8(ReadByte(data, 2))),
                    4 => FromXYZW(NormalizeUnsigned8(ReadByte(data, 0)), NormalizeUnsigned8(ReadByte(data, 1)),
                        NormalizeUnsigned8(ReadByte(data, 2)), NormalizeUnsigned8(ReadByte(data, 3))),
                    _ => throw new ArgumentOutOfRangeException(nameof(component.Count),
                        $"Invalid U8N component count: {component.Count}")
                };

            case T3VertexComponent.EnumType.VTypeS16N:
                // signed 16-bit normalized -> 2 bytes each
                return component.Count switch
                {
                    1 => FromX(NormalizeSigned16(ReadInt16(data, 0))),
                    2 => FromXY(NormalizeSigned16(ReadInt16(data, 0)), NormalizeSigned16(ReadInt16(data, 2))),
                    3 => FromXYZ(NormalizeSigned16(ReadInt16(data, 0)), NormalizeSigned16(ReadInt16(data, 2)),
                        NormalizeSigned16(ReadInt16(data, 4))),
                    4 => FromXYZW(NormalizeSigned16(ReadInt16(data, 0)), NormalizeSigned16(ReadInt16(data, 2)),
                        NormalizeSigned16(ReadInt16(data, 4)), NormalizeSigned16(ReadInt16(data, 6))),
                    _ => throw new ArgumentOutOfRangeException(nameof(component.Count),
                        $"Invalid S16N component count: {component.Count}")
                };

            case T3VertexComponent.EnumType.VTypeU16N:
                // unsigned 16-bit normalized -> 2 bytes each
                return component.Count switch
                {
                    1 => FromX(NormalizeUnsigned16(ReadUInt16(data, 0))),
                    2 => FromXY(NormalizeUnsigned16(ReadUInt16(data, 0)), NormalizeUnsigned16(ReadUInt16(data, 2))),
                    3 => FromXYZ(NormalizeUnsigned16(ReadUInt16(data, 0)), NormalizeUnsigned16(ReadUInt16(data, 2)),
                        NormalizeUnsigned16(ReadUInt16(data, 4))),
                    4 => FromXYZW(NormalizeUnsigned16(ReadUInt16(data, 0)), NormalizeUnsigned16(ReadUInt16(data, 2)),
                        NormalizeUnsigned16(ReadUInt16(data, 4)), NormalizeUnsigned16(ReadUInt16(data, 6))),
                    _ => throw new ArgumentOutOfRangeException(nameof(component.Count),
                        $"Invalid U16N component count: {component.Count}")
                };

            case T3VertexComponent.EnumType.VTypeSF16:
                // signed float16 (half) -> 2 bytes each
                return component.Count switch
                {
                    1 => FromX(ReadHalf(data, 0)),
                    2 => FromXY(ReadHalf(data, 0), ReadHalf(data, 2)),
                    3 => FromXYZ(ReadHalf(data, 0), ReadHalf(data, 2), ReadHalf(data, 4)),
                    4 => FromXYZW(ReadHalf(data, 0), ReadHalf(data, 2), ReadHalf(data, 4), ReadHalf(data, 6)),
                    _ => throw new ArgumentOutOfRangeException(nameof(component.Count),
                        $"Invalid SF16 component count: {component.Count}")
                };

            default:
                throw new NotSupportedException($"Unsupported vertex component type: {component.Type}");
        }
    }

    public static int GetTypeSize(this T3VertexComponent.EnumType type)
    {
        return type switch
        {
            T3VertexComponent.EnumType.VTypeNone => 0,
            T3VertexComponent.EnumType.VTypeFloat => 4,
            T3VertexComponent.EnumType.VTypeS8N => 1,
            T3VertexComponent.EnumType.VTypeU8N => 1,
            T3VertexComponent.EnumType.VTypeS16N => 2,
            T3VertexComponent.EnumType.VTypeU16N => 2,
            T3VertexComponent.EnumType.VTypeSF16 => 2,
            T3VertexComponent.EnumType.VTypeS8NBones => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    static Vector4 ReadVector4(ReadOnlySpan<byte> data, GFXPlatformFormat format, bool isWeights = false)
    {
        if (isWeights && format == GFXPlatformFormat.UN10x3_UN2)
        {
            return ReadTelltaleBoneWeights(data);
        }

        return format switch
        {
            GFXPlatformFormat.F32 => new Vector4(ReadFloat(data), 0, 0, 1),
            GFXPlatformFormat.F32x2 => new Vector4(ReadFloat(data, 0), ReadFloat(data, 4), 0, 0),
            GFXPlatformFormat.F32x3 => new Vector4(ReadFloat(data, 0), ReadFloat(data, 4), ReadFloat(data, 8), 1),
            GFXPlatformFormat.F32x4 => new Vector4(ReadFloat(data, 0), ReadFloat(data, 4), ReadFloat(data, 8),
                ReadFloat(data, 12)),

            GFXPlatformFormat.F16x2 => new Vector4(ReadHalf(data, 0), ReadHalf(data, 2), 0, 1),
            GFXPlatformFormat.F16x4 => new Vector4(ReadHalf(data, 0), ReadHalf(data, 2), ReadHalf(data, 4),
                ReadHalf(data, 6)),

            GFXPlatformFormat.S32 => new Vector4(ReadInt32(data), 0, 0, 1),
            GFXPlatformFormat.U32 => new Vector4(ReadUInt32(data), 0, 0, 1),
            GFXPlatformFormat.S32x2 => new Vector4(ReadInt32(data, 0), ReadInt32(data, 4), 0, 1),
            GFXPlatformFormat.U32x2 => new Vector4(ReadUInt32(data, 0), ReadUInt32(data, 4), 0, 1),
            GFXPlatformFormat.S32x3 => new Vector4(ReadInt32(data, 0), ReadInt32(data, 4), ReadInt32(data, 8), 1),
            GFXPlatformFormat.U32x3 => new Vector4(ReadUInt32(data, 0), ReadUInt32(data, 4), ReadUInt32(data, 8), 1),
            GFXPlatformFormat.S32x4 => new Vector4(ReadInt32(data, 0), ReadInt32(data, 4), ReadInt32(data, 8),
                ReadInt32(data, 12)),
            GFXPlatformFormat.U32x4 => new Vector4(ReadUInt32(data, 0), ReadUInt32(data, 4), ReadUInt32(data, 8),
                ReadUInt32(data, 12)),

            GFXPlatformFormat.S16 => new Vector4(ReadInt16(data), 0, 0, 1),
            GFXPlatformFormat.U16 => new Vector4(ReadUInt16(data), 0, 0, 1),
            GFXPlatformFormat.S16x2 => new Vector4(ReadInt16(data, 0), ReadInt16(data, 2), 0, 1),
            GFXPlatformFormat.U16x2 => new Vector4(ReadUInt16(data, 0), ReadUInt16(data, 2), 0, 1),
            GFXPlatformFormat.S16x4 => new Vector4(ReadInt16(data, 0), ReadInt16(data, 2), ReadInt16(data, 4),
                ReadInt16(data, 6)),
            GFXPlatformFormat.U16x4 => new Vector4(ReadUInt16(data, 0), ReadUInt16(data, 2), ReadUInt16(data, 4),
                ReadUInt16(data, 6)),

            GFXPlatformFormat.SN16 => new Vector4(NormalizeSigned16(ReadInt16(data)), 0, 0, 1),
            GFXPlatformFormat.UN16 => new Vector4(NormalizeUnsigned16(ReadUInt16(data)), 0, 0, 1),
            GFXPlatformFormat.SN16x2 => new Vector4(NormalizeSigned16(ReadInt16(data, 0)),
                NormalizeSigned16(ReadInt16(data, 2)), 0, 1),
            GFXPlatformFormat.UN16x2 => new Vector4(NormalizeUnsigned16(ReadUInt16(data, 0)),
                NormalizeUnsigned16(ReadUInt16(data, 2)), 0, 1),
            GFXPlatformFormat.SN16x4 => new Vector4(NormalizeSigned16(ReadInt16(data, 0)),
                NormalizeSigned16(ReadInt16(data, 2)), NormalizeSigned16(ReadInt16(data, 4)),
                NormalizeSigned16(ReadInt16(data, 6))),
            GFXPlatformFormat.UN16x4 => new Vector4(NormalizeUnsigned16(ReadUInt16(data, 0)),
                NormalizeUnsigned16(ReadUInt16(data, 2)), NormalizeUnsigned16(ReadUInt16(data, 4)),
                NormalizeUnsigned16(ReadUInt16(data, 6))),

            GFXPlatformFormat.S8 => new Vector4(ReadSByte(data), 0, 0, 1),
            GFXPlatformFormat.U8 => new Vector4(ReadByte(data), 0, 0, 1),
            GFXPlatformFormat.S8x2 => new Vector4(ReadSByte(data, 0), ReadSByte(data, 1), 0, 1),
            GFXPlatformFormat.U8x2 => new Vector4(ReadByte(data, 0), ReadByte(data, 1), 0, 1),
            GFXPlatformFormat.S8x4 => new Vector4(ReadSByte(data, 0), ReadSByte(data, 1), ReadSByte(data, 2),
                ReadSByte(data, 3)),
            GFXPlatformFormat.U8x4 => new Vector4(ReadByte(data, 0), ReadByte(data, 1), ReadByte(data, 2),
                ReadByte(data, 3)),

            GFXPlatformFormat.SN8 => new Vector4(NormalizeSigned8(ReadSByte(data)), 0, 0, 1),
            GFXPlatformFormat.UN8 => new Vector4(NormalizeUnsigned8(ReadByte(data)), 0, 0, 1),
            GFXPlatformFormat.SN8x2 => new Vector4(NormalizeSigned8(ReadSByte(data, 0)),
                NormalizeSigned8(ReadSByte(data, 1)), 0, 1),
            GFXPlatformFormat.UN8x2 => new Vector4(NormalizeUnsigned8(ReadByte(data, 0)),
                NormalizeUnsigned8(ReadByte(data, 1)), 0, 1),
            GFXPlatformFormat.SN8x4 => new Vector4(NormalizeSigned8(ReadSByte(data, 0)),
                NormalizeSigned8(ReadSByte(data, 1)), NormalizeSigned8(ReadSByte(data, 2)),
                NormalizeSigned8(ReadSByte(data, 3))),
            GFXPlatformFormat.UN8x4 => new Vector4(NormalizeUnsigned8(ReadByte(data, 0)),
                NormalizeUnsigned8(ReadByte(data, 1)), NormalizeUnsigned8(ReadByte(data, 2)),
                NormalizeUnsigned8(ReadByte(data, 3))),

            GFXPlatformFormat.D3DCOLOR => ReadD3DColor(data),
            GFXPlatformFormat.SN10_SN11_SN11 => ReadSN10_SN11_SN11(data),
            GFXPlatformFormat.SN10x3_SN2 => ReadSN10x3_SN2(data),
            GFXPlatformFormat.UN10x3_UN2 => ReadUN10x3_UN2(data),
            GFXPlatformFormat.None => throw new ArgumentOutOfRangeException(nameof(format), format, null),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

// Helper readers with offset support
    static float ReadFloat(ReadOnlySpan<byte> data, int offset = 0)
        => BitConverter.ToSingle(data.Slice(offset, 4));

    static float ReadHalf(ReadOnlySpan<byte> data, int offset = 0)
    {
        var halfValue = BitConverter.ToUInt16(data.Slice(offset, 2));
        return (float)BitConverter.UInt16BitsToHalf(halfValue);
    }

    static int ReadInt32(ReadOnlySpan<byte> data, int offset = 0)
        => BitConverter.ToInt32(data.Slice(offset, 4));

    static uint ReadUInt32(ReadOnlySpan<byte> data, int offset = 0)
        => BitConverter.ToUInt32(data.Slice(offset, 4));

    static short ReadInt16(ReadOnlySpan<byte> data, int offset = 0)
        => BitConverter.ToInt16(data.Slice(offset, 2));

    static ushort ReadUInt16(ReadOnlySpan<byte> data, int offset = 0)
        => BitConverter.ToUInt16(data.Slice(offset, 2));

    static sbyte ReadSByte(ReadOnlySpan<byte> data, int offset = 0)
        => (sbyte)data[offset];

    static byte ReadByte(ReadOnlySpan<byte> data, int offset = 0)
        => data[offset];

// Normalization helpers
    static float NormalizeSigned16(short value) => Math.Clamp(value / 32767f, -1f, 1f);
    static float NormalizeUnsigned16(ushort value) => value / 65535.0f;
    static float NormalizeSigned8(sbyte value) => Math.Clamp(value / 127f, -1f, 1f);
    static float NormalizeUnsigned8(byte value) => value / 255.0f;

    static Vector4 ReadD3DColor(ReadOnlySpan<byte> data)
    {
        // D3DCOLOR is typically BGRA
        return new Vector4(data[2] / 255f, data[1] / 255f, data[0] / 255f, data[3] / 255f);
    }

    public static int GetComponentCount(this GFXPlatformFormat format) => format switch
    {
        GFXPlatformFormat.F32 or GFXPlatformFormat.S32 or GFXPlatformFormat.U32 or
            GFXPlatformFormat.S16 or GFXPlatformFormat.U16 or GFXPlatformFormat.SN16 or
            GFXPlatformFormat.UN16 or GFXPlatformFormat.S8 or GFXPlatformFormat.U8 or
            GFXPlatformFormat.SN8 or GFXPlatformFormat.UN8 => 1,

        GFXPlatformFormat.F32x2 or GFXPlatformFormat.F16x2 or GFXPlatformFormat.S32x2 or
            GFXPlatformFormat.U32x2 or GFXPlatformFormat.S16x2 or GFXPlatformFormat.U16x2 or
            GFXPlatformFormat.SN16x2 or GFXPlatformFormat.UN16x2 or GFXPlatformFormat.S8x2 or
            GFXPlatformFormat.U8x2 or GFXPlatformFormat.SN8x2 or GFXPlatformFormat.UN8x2 => 2,

        GFXPlatformFormat.F32x3 or GFXPlatformFormat.S32x3 or GFXPlatformFormat.U32x3 or
            GFXPlatformFormat.SN10_SN11_SN11 => 3,

        GFXPlatformFormat.F32x4 or GFXPlatformFormat.F16x4 or GFXPlatformFormat.S32x4 or
            GFXPlatformFormat.U32x4 or GFXPlatformFormat.S16x4 or GFXPlatformFormat.U16x4 or
            GFXPlatformFormat.SN16x4 or GFXPlatformFormat.UN16x4 or GFXPlatformFormat.S8x4 or
            GFXPlatformFormat.U8x4 or GFXPlatformFormat.SN8x4 or GFXPlatformFormat.UN8x4 or
            GFXPlatformFormat.D3DCOLOR => 4,

        _ => 0
    };

    public static int GetElementSize(this GFXPlatformFormat format) => format switch
    {
        GFXPlatformFormat.F32 => 4,
        GFXPlatformFormat.F32x2 => 8,
        GFXPlatformFormat.F32x3 => 12,
        GFXPlatformFormat.F32x4 => 16,
        GFXPlatformFormat.F16x2 => 4,
        GFXPlatformFormat.F16x4 => 8,
        GFXPlatformFormat.S32 or GFXPlatformFormat.U32 => 4,
        GFXPlatformFormat.S32x2 or GFXPlatformFormat.U32x2 => 8,
        GFXPlatformFormat.S32x3 or GFXPlatformFormat.U32x3 => 12,
        GFXPlatformFormat.S32x4 or GFXPlatformFormat.U32x4 => 16,
        GFXPlatformFormat.S16 or GFXPlatformFormat.U16 or GFXPlatformFormat.SN16 or GFXPlatformFormat.UN16 => 2,
        GFXPlatformFormat.S16x2 or GFXPlatformFormat.U16x2 or GFXPlatformFormat.SN16x2 or GFXPlatformFormat.UN16x2 => 4,
        GFXPlatformFormat.S16x4 or GFXPlatformFormat.U16x4 or GFXPlatformFormat.SN16x4 or GFXPlatformFormat.UN16x4 => 8,
        GFXPlatformFormat.S8 or GFXPlatformFormat.U8 or GFXPlatformFormat.SN8 or GFXPlatformFormat.UN8 => 1,
        GFXPlatformFormat.S8x2 or GFXPlatformFormat.U8x2 or GFXPlatformFormat.SN8x2 or GFXPlatformFormat.UN8x2 => 2,
        GFXPlatformFormat.S8x4 or GFXPlatformFormat.U8x4 or GFXPlatformFormat.SN8x4 or GFXPlatformFormat.UN8x4 => 4,
        GFXPlatformFormat.SN10_SN11_SN11 or GFXPlatformFormat.UN10x3_UN2 or GFXPlatformFormat.SN10x3_SN2 => 4,
        GFXPlatformFormat.D3DCOLOR => 4,
        _ => 16
    };

    // [4 BYTES] SN10_SN11_SN11 - 10+11+11 bits = 32 bits
    static Vector4 ReadSN10_SN11_SN11(ReadOnlySpan<byte> data)
    {
        uint packed = ReadUInt32(data);

        // Assuming layout: 10 bits X, 11 bits Y, 11 bits Z
        uint x = packed & 0x3FF; // Bits 0-9: X (10 bits)
        uint y = (packed >> 10) & 0x7FF; // Bits 10-20: Y (11 bits)
        uint z = (packed >> 21) & 0x7FF; // Bits 21-31: Z (11 bits)

        // Convert from normalized integers to floats
        float xf = (x / 511.0f) * 2.0f - 1.0f; // 10-bit signed: -1 to 1
        float yf = (y / 1023.0f) * 2.0f - 1.0f; // 11-bit signed: -1 to 1  
        float zf = (z / 1023.0f) * 2.0f - 1.0f; // 11-bit signed: -1 to 1

        // Clamp to [-1, 1] range
        xf = Math.Clamp(xf, -1.0f, 1.0f);
        yf = Math.Clamp(yf, -1.0f, 1.0f);
        zf = Math.Clamp(zf, -1.0f, 1.0f);

        return new Vector4(xf, yf, zf, 1.0f); // W defaults to 1
    }

// [4 BYTES] UN10x3_UN2 - 10+10+10+2 bits = 32 bits
    static Vector4 ReadUN10x3_UN2(ReadOnlySpan<byte> data)
    {
        uint packed = ReadUInt32(data);

        // Unpack the components (little-endian)
        uint x = packed & 0x3FF; // Bits 0-9: X
        uint y = (packed >> 10) & 0x3FF; // Bits 10-19: Y
        uint z = (packed >> 20) & 0x3FF; // Bits 20-29: Z  
        uint w = (packed >> 30) & 0x3; // Bits 30-31: W

        // Convert from normalized integers to floats
        float xf = x / 1023.0f; // 10-bit unsigned: 0 to 1
        float yf = y / 1023.0f;
        float zf = z / 1023.0f;
        float wf = w / 3.0f; // 2-bit unsigned: 0 to 1

        // Clamp to [0, 1] range (matching the original encoding)
        // xf = Math.Clamp(xf, 0.0f, 1.0f);
        // yf = Math.Clamp(yf, 0.0f, 1.0f);
        // zf = Math.Clamp(zf, 0.0f, 1.0f);
        // wf = Math.Clamp(wf, 0.0f, 1.0f);

        return new Vector4(xf, yf, zf, wf);
    }

// [4 BYTES] SN10x3_SN2 - 10+10+10+2 bits = 32 bits  
    static Vector4 ReadSN10x3_SN2(ReadOnlySpan<byte> data)
    {
        uint packed = ReadUInt32(data);

        // Unpack the components (little-endian)
        uint x = packed & 0x3FF; // Bits 0-9: X
        uint y = (packed >> 10) & 0x3FF; // Bits 10-19: Y  
        uint z = (packed >> 20) & 0x3FF; // Bits 20-29: Z
        uint w = (packed >> 30) & 0x3; // Bits 30-31: W

        // Convert from normalized integers to floats
        float xf = (x / 511.0f) * 2.0f - 1.0f; // 10-bit signed: -1 to 1
        float yf = (y / 511.0f) * 2.0f - 1.0f;
        float zf = (z / 511.0f) * 2.0f - 1.0f;
        float wf = (w / 3.0f) * 2.0f - 1.0f; // 2-bit signed: -1 to 1

        // Clamp to [-1, 1] range (matching the original encoding)
        xf = Math.Clamp(xf, -1.0f, 1.0f);
        yf = Math.Clamp(yf, -1.0f, 1.0f);
        zf = Math.Clamp(zf, -1.0f, 1.0f);
        wf = Math.Clamp(wf, -1.0f, 1.0f);

        return new Vector4(xf, yf, zf, wf);
    }
}