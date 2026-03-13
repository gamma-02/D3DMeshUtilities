using System.Numerics;
using Assimp;
using TelltaleToolKit.T3Types.Meshes;
using TelltaleToolKit.T3Types.Meshes.T3Types;

namespace D3DMeshExperimentalExporer;

public static class DecompressorExtensions
{
    
    public static byte[] GetBufferRange(this T3VertexBuffer buffer, int startIndex, int count)
    {
        int startByte = startIndex *  buffer.VertSize;
        int length = count *  buffer.VertSize;

        // Ensure we don't go out of bounds
        if (startByte + length > buffer.Buffer.Length)
            length = buffer.Buffer.Length - startByte;

        var range = new byte[length];
        Array.Copy(buffer.Buffer, startByte, range, 0, length);
        return range;
    }
    
    public static byte[] GetBufferRange(this T3GFXBuffer buffer, uint startIndex, uint count)
    {
        int elementSize = buffer.BufferFormat.GetElementSize();
        var startByte = (int)(startIndex * buffer.Stride);
        var length = (int)(count * buffer.Stride);

        // Ensure we don't go out of bounds
        if (startByte + length > buffer.Buffer.Length)
            length = buffer.Buffer.Length - startByte;

        var range = new byte[length];
        Array.Copy(buffer.Buffer, startByte, range, 0, length);
        return range;
    }

    public static List<Vector3> DecompressToVector3D(byte[] buffer, T3VertexBuffer vertexBuffer, D3DMesh.T3VertexComponentType vertexComponentType)
    {
        List<Vector4> vectors4 = Decompressor.Decompress(buffer, vertexBuffer, vertexComponentType);
        var result = new List<Vector3>(vectors4.Count);

        foreach (Vector4 vec4 in vectors4)
        {
            var vec3 = new Vector3(vec4.X, vec4.Y, vec4.Z);
            result.Add(vec3);
        }

        return result;
    }

    public static List<Vector4> DecompressToColor4D(byte[] buffer, T3VertexBuffer vertexBuffer, D3DMesh.T3VertexComponentType vertexComponentType)
    {
        return Decompressor.Decompress(buffer, vertexBuffer, vertexComponentType);
    }
    
    
    public static List<Vector3> DecompressToVector3D(byte[] buffer, uint stride, GFXPlatformAttributeParams attribute)
    {
        List<Vector4> vectors4 = Decompressor.Decompress(buffer, stride, attribute);
        var result = new List<Vector3>(vectors4.Count);

        foreach (Vector4 vec4 in vectors4)
        {
            var vec3 = new Vector3(vec4.X, vec4.Y, vec4.Z);
            result.Add(vec3);
        }

        return result;
    }

    public static List<Vector4> DecompressToColor4D(byte[] buffer, uint stride, GFXPlatformAttributeParams attribute)
    {
        return Decompressor.Decompress(buffer, stride, attribute);
    }

    public static T3IndexBuffer? GetIndexBuffer(this D3DMesh d3dMesh)
    {
        return d3dMesh.T3IndexBuffer;
    }
    
    public static T3GFXBuffer? GetIndexBuffer(this D3DMesh d3dMesh, bool isShadow = false)
    {
        if (isShadow && d3dMesh.MeshData.VertexStates[0].IndexBuffer.ElementAtOrDefault(1) != null)
        {
            return d3dMesh.MeshData.VertexStates[0].IndexBuffer[1];
        }

        return d3dMesh.MeshData.VertexStates[0].IndexBuffer[0];
    }

    // public static void ProcessBatchFaces(ref Mesh assimpMesh, D3DMesh d3dMesh, T3MeshBatch batch, bool isShadow = false)
    // {
    //     T3GFXBuffer? indexBuffer = d3dMesh.GetIndexBuffer(isShadow);
    //     if (indexBuffer == null)
    //         return;
    //
    //     assimpMesh.Faces.Capacity = (int)batch.NumPrimitives;
    //
    //     for (var faceIndex = 0; faceIndex < batch.NumPrimitives; faceIndex++)
    //     {
    //         var face = new Face();
    //
    //         face.Indices.Add((int)(ReadIndexValueSimple(indexBuffer, (uint)(batch.StartIndex + faceIndex * 3 + 0)) -
    //                                batch.MinVertIndex));
    //         face.Indices.Add((int)(ReadIndexValueSimple(indexBuffer, (uint)(batch.StartIndex + faceIndex * 3 + 1)) -
    //                                batch.MinVertIndex));
    //         face.Indices.Add((int)(ReadIndexValueSimple(indexBuffer, (uint)(batch.StartIndex + faceIndex * 3 + 2)) -
    //                                batch.MinVertIndex));
    //
    //         assimpMesh.Faces.Add(face);
    //     }
    // }

    private static uint ReadIndexValueSimple(T3GFXBuffer indexBuffer, uint elementPosition)
    {
        uint byteOffset = elementPosition * indexBuffer.Stride;

        if (byteOffset + indexBuffer.Stride > (uint)indexBuffer.Buffer.Length)
            return 0;

        return indexBuffer.Stride switch
        {
            // Most common case: 16-bit indices (u16)
            2 => BitConverter.ToUInt16(indexBuffer.Buffer, (int)byteOffset),
            // Maybe : 32-bit indices (u32)
            4 => BitConverter.ToUInt32(indexBuffer.Buffer, (int)byteOffset),
            _ => throw new Exception("Unexpected element position")
        };
    }
}