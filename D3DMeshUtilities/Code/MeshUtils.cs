using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Numerics;
using D3DMeshUtilities.Code.ImageStuffAUGH;
using D3DMeshUtilities.Code.MeshHandling;
using SharpGLTF.Materials;
using SharpGLTF.Memory;
using TelltaleToolKit;
using TelltaleToolKit.Resource;
using TelltaleToolKit.T3Types;
using TelltaleToolKit.T3Types.Meshes;
using TelltaleToolKit.T3Types.Meshes.T3Types;
using TelltaleToolKit.T3Types.Properties;
using TelltaleToolKit.T3Types.Textures;
using AlphaMode = SharpGLTF.Materials.AlphaMode;

namespace D3DMeshUtilities.Code;

public static class MeshUtils
{
    
    
    #region Vertex State Handling
    
    public static List<Vector4>? GetVertices(T3MeshData meshData, int vertexStateIndex)
    {

        var vertexState = meshData.VertexStates[vertexStateIndex];

        GFXPlatformAttributeParams? bufferParam = null;

        T3GFXBuffer? vertexPositionBuffer = null;

        for (uint index = 0; index < vertexState.AttributeCount; index++)
        {
            var vertexAttribute = vertexState.Attributes[(int)index];

            if (vertexAttribute.Attribute == GFXPlatformVertexAttribute.Position)
            {
                vertexPositionBuffer = vertexState.VertexBuffer[(int)index];
                bufferParam = vertexAttribute;
                break;
            }
            
        }

        if (vertexPositionBuffer == null || bufferParam == null)
            return null;

        bool validFormatFromBuffer = ConvertFromGfxPlatformFormat.IsFormatVector4(vertexPositionBuffer.BufferFormat);
        if (!validFormatFromBuffer && !ConvertFromGfxPlatformFormat.IsFormatVector4(bufferParam.Format))
            return null;

        GFXPlatformFormat vertexFormat = vertexPositionBuffer.BufferFormat;
        if (!validFormatFromBuffer)
            vertexFormat = bufferParam.Format;

        ReadOnlySpan<byte> vertexBuffer = vertexPositionBuffer.Buffer;

        List<Vector4> vertices = new List<Vector4>();

        uint readerIndex = 0;

        while (readerIndex < vertexBuffer.Length)
        {
            // vertices.Add(ConvertFromGfxPlatformFormat.ReadUN10x3_UN2(vertexBuffer.Slice((int)readerIndex)));

            vertices.Add(ConvertFromGfxPlatformFormat.ReadVector4FromSpanAndFormat(vertexBuffer.Slice((int)readerIndex),
                vertexFormat)!.Value);
            
            readerIndex += vertexPositionBuffer.Stride;
        }

        return vertices;
    }
    
    public static List<Vector4>? GetNormals(T3MeshData meshData, int vertexStateIndex, int normalsToSkip = 0)
    {
    
        var vertexState = meshData.VertexStates[vertexStateIndex];
    
        T3GFXBuffer? vertexNormalBuffer = null;

        GFXPlatformFormat format = GFXPlatformFormat.None;

        //get just the one normal for now -- don't know what the second does
        for (uint index = 0; index < vertexState.AttributeCount; index++)
        {
            var vertexAttribute = vertexState.Attributes[(int)index];
    
            if (vertexAttribute.Attribute == GFXPlatformVertexAttribute.Normal)
            {
                if (normalsToSkip > 0)
                {
                    normalsToSkip -= 1;
                    continue;
                }
                
                vertexNormalBuffer = vertexState.VertexBuffer[(int)index];
                format = vertexAttribute.Format;
                break;
            }
            
        }
    
        if (vertexNormalBuffer == null)
            return null;

        if (ConvertFromGfxPlatformFormat.IsFormatVector4(vertexNormalBuffer.BufferFormat))
            format = vertexNormalBuffer.BufferFormat;
        else if (!ConvertFromGfxPlatformFormat.IsFormatVector4(format))
            return null;
    
        ReadOnlySpan<byte> vertexBuffer = vertexNormalBuffer.Buffer;
    
        List<Vector4> normals = new List<Vector4>();
    
        uint readerIndex = 0;
    
        while (readerIndex < vertexBuffer.Length)
        {
            normals.Add((Vector4)ConvertFromGfxPlatformFormat.ReadVector4FromSpanAndFormat(vertexBuffer.Slice((int)readerIndex), format)!);
    
            readerIndex += vertexNormalBuffer.Stride;
    
        }
    
        return normals;
    }
    
    public static List<Vector4>? GetTangents(T3MeshData meshData, int vertexStateIndex)
    {
    
        var vertexState = meshData.VertexStates[vertexStateIndex];
    
        T3GFXBuffer? vertexTangentBuffer = null;

        GFXPlatformFormat format = GFXPlatformFormat.None;

        //get just the one normal for now -- don't know what the second does
        for (uint index = 0; index < vertexState.AttributeCount; index++)
        {
            var vertexAttribute = vertexState.Attributes[(int)index];
    
            if (vertexAttribute.Attribute == GFXPlatformVertexAttribute.Tangent)
            {
                vertexTangentBuffer = vertexState.VertexBuffer[(int)index];
                format = vertexAttribute.Format;
                break;
            }
            
        }
    
        if (vertexTangentBuffer == null)
            return null;

        if (ConvertFromGfxPlatformFormat.IsFormatVector4(vertexTangentBuffer.BufferFormat))
            format = vertexTangentBuffer.BufferFormat;
        else if (!ConvertFromGfxPlatformFormat.IsFormatVector4(format))
            return null;
    
        ReadOnlySpan<byte> vertexBuffer = vertexTangentBuffer.Buffer;
    
        List<Vector4> tangents = new List<Vector4>();
    
        uint readerIndex = 0;
    
        while (readerIndex < vertexBuffer.Length)
        {
            tangents.Add((Vector4)ConvertFromGfxPlatformFormat.ReadVector4FromSpanAndFormat(vertexBuffer.Slice((int)readerIndex), format)!);
    
            readerIndex += vertexTangentBuffer.Stride;
    
        }
    
        return tangents;
    }
    
    public static List<Vector4>? GetVertexColors(T3MeshData meshData, int vertexStateIndex)
    {
        var vertexState = meshData.VertexStates[vertexStateIndex];
    
        T3GFXBuffer? vertexColorBuffer = null;

        GFXPlatformFormat format = GFXPlatformFormat.None;

        //get just the one normal for now -- don't know what the second does
        for (uint index = 0; index < vertexState.AttributeCount; index++)
        {
            var vertexAttribute = vertexState.Attributes[(int)index];
    
            if (vertexAttribute.Attribute == GFXPlatformVertexAttribute.Color)
            {
                vertexColorBuffer = vertexState.VertexBuffer[(int)index];
                format = vertexAttribute.Format;
                break;
            }
            
        }
    
        if (vertexColorBuffer == null)
            return null;

        if (ConvertFromGfxPlatformFormat.IsFormatVector4(vertexColorBuffer.BufferFormat))
            format = vertexColorBuffer.BufferFormat;
        else if (!ConvertFromGfxPlatformFormat.IsFormatVector4(format))
            return null;
    
        ReadOnlySpan<byte> colorBuffer = vertexColorBuffer.Buffer;
    
        var colors = new List<Vector4>();
    
        uint readerIndex = 0;
    
        while (readerIndex < colorBuffer.Length)
        {
            colors.Add((Vector4)ConvertFromGfxPlatformFormat.ReadVector4FromSpanAndFormat(colorBuffer.Slice((int)readerIndex), format)!);
    
            readerIndex += vertexColorBuffer.Stride;
    
        }
    
        return colors;
    }
    
    public static List<Vector2>? GetVertexTextureCoords(T3MeshData meshData, int vertexStateIndex, int texture = 0)
    {
        var vertexState = meshData.VertexStates[vertexStateIndex];
    
        T3GFXBuffer? texCoordBuffer = null;

        GFXPlatformFormat format = GFXPlatformFormat.None;

        //get just the one normal for now -- don't know what the second does
        for (uint index = 0; index < vertexState.AttributeCount; index++)
        {
            var vertexAttribute = vertexState.Attributes[(int)index];
    
            if (vertexAttribute.Attribute == GFXPlatformVertexAttribute.TexCoord)
            {
                if (texture > 0)
                {
                    texture -= 1;
                    continue;
                }
                
                texCoordBuffer = vertexState.VertexBuffer[(int)index];
                format = vertexAttribute.Format;
                break;
            }
            
        }
    
        if (texCoordBuffer == null)
            return null;

        if (ConvertFromGfxPlatformFormat.IsFormatVector2(texCoordBuffer.BufferFormat))
            format = texCoordBuffer.BufferFormat;
        else if (!ConvertFromGfxPlatformFormat.IsFormatVector2(format))
            return null;
    
        ReadOnlySpan<byte> colorBuffer = texCoordBuffer.Buffer;
    
        List<Vector2> textureCoords = new List<Vector2>();
    
        uint readerIndex = 0;
    
        while (readerIndex < colorBuffer.Length)
        {
            textureCoords.Add((Vector2)ConvertFromGfxPlatformFormat.ReadVector2FromSpanAndFormat(colorBuffer.Slice((int)readerIndex), format)!);
    
            readerIndex += texCoordBuffer.Stride;
    
        }
    
        return textureCoords;
    }


    public static List<uint>? GetIndexBuffer(T3MeshData meshData, int vertexStateIndex, int vertexPositionBufferIndex)
    {
        var vertexState = meshData.VertexStates[vertexStateIndex];

        T3GFXBuffer indexBuffer = vertexState.IndexBuffer[vertexPositionBufferIndex];

        GFXPlatformFormat format = indexBuffer.BufferFormat;

        if (!ConvertFromGfxPlatformFormat.IsFormatScalarUnsignedInteger(format))
            return null;

        ReadOnlySpan<byte> vertexBuffer = indexBuffer.Buffer;

        List<uint> vertices = new List<uint>();

        uint readerIndex = 0;

        while (readerIndex < vertexBuffer.Length)
        {
            vertices.Add((uint)ConvertFromGfxPlatformFormat.ReadUIntFromSpanAndFormat(vertexBuffer.Slice((int)readerIndex), format)!);

            readerIndex += indexBuffer.Stride;

        }

        return vertices;
    }
    
    #endregion
    
    
    #region Mesh Parsing
    
    
    public static void ApplyTransforms(
        T3MeshData meshData,
        int vertexIndex,
        Vector4 pos, 
        List<Vector3> vertexPositions, 
        List<Vector2>?[] textureCoordArray
        )
    {
        //todo: document these transformations lol
        
        var scaledPos = pos.AsVector3() * meshData.PositionScale;
        Vector3 wScaleOffset = meshData.PositionWScale * pos.W;
        
        Vector3 finalPos = scaledPos + meshData.PositionOffset + wScaleOffset;
        
        vertexPositions.Add(finalPos);

        //finally, transform all of the texture coordinates
        //so that they like. make sense, even if we can't use them in gLTF files
        for (int texCoordIndex = 0; texCoordIndex < 4; texCoordIndex++)
        {
            if(textureCoordArray[texCoordIndex] == null)
                break;
            
            Vector2 texCoord = textureCoordArray[texCoordIndex]![vertexIndex];
            
            textureCoordArray[texCoordIndex]![vertexIndex] = ApplyTexCoordTransform(meshData, 0, texCoord);
        
        }
        

    }

    //todo: document these too
    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract")]
    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
    public static Vector2 ApplyTexCoordTransform(T3MeshData meshData, int texCoordIndex, Vector2 texCoord)
    {
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

        if (meshData.TexCoordTransform[texCoordIndex] != null && meshData.TexCoordTransform[texCoordIndex].Scale != null)
        {
            texCoord *= meshData.TexCoordTransform[texCoordIndex].Scale;
        }
        if (meshData.TexCoordTransform[texCoordIndex] != null && meshData.TexCoordTransform[texCoordIndex].Offset != null)
        {
            texCoord += meshData.TexCoordTransform[texCoordIndex].Offset;
        }
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

        return texCoord;
    }


    public static void GetMaterials(D3DMesh mesh, MeshInfo info, out List<MaterialBuilder> materials)
    {
        T3MeshData meshData = mesh.MeshData;
        materials = new List<MaterialBuilder>();
        
        foreach (T3MeshMaterial mat in meshData.Materials)
        {
            var materialName = mat.Material.ObjectInfo.ObjectName.Crc64.ToString();

            bool doubleSided = true;

            if (mesh.InternalResources.Find(res => res.ObjectInfo.ObjectName == mat.Material.ObjectInfo.ObjectName)?
                    .ObjectInfo.HandleObject is PropertySet p)
            {
                TttkInit.Instance.Workspace!.ResolveSymbols(p.Properties.Keys);
                
                //the property '5971B48CE79829D9' corresponds to if a model should render double sided
                bool? sided = p.GetProperty<bool>(new Symbol(0x5971B48CE79829D9));

                if (sided != null)
                {
                    doubleSided = sided.Value;
                }
                
            }
            
            var mb = new MaterialBuilder(materialName).WithDoubleSide(doubleSided);

            Handle<T3Texture>? materialBaseColor = mesh.GetDiffuseTexture(mat.Material);
            MeshUtils.ProcessTexture(materialBaseColor, TttkInit.Instance.Workspace!, mb, KnownChannel.BaseColor);

            Handle<T3Texture>? normalMap = mesh.GetNormalMapTexture(mat.Material);
            MeshUtils.ProcessTexture(normalMap, TttkInit.Instance.Workspace!, mb, KnownChannel.Normal);

            Handle<T3Texture>? specularMap = mesh.GetSpecularTexture(mat.Material);
            MeshUtils.ProcessTexture(specularMap, TttkInit.Instance.Workspace!, mb, KnownChannel.SpecularColor);
            
            //todo: detail texture, used for lines and stuff on models
            Handle<T3Texture>? detailMap = mesh.GetDetailTexture(mat.Material);
            // ProcessTexture(detailMap, TttkInit.Instance.Workspace!, mb, KnownChannel);

            if (detailMap != null)
            {
                Console.Out.WriteLine("OOH, detail map! " + detailMap);
                
            }
            
            materials.Add(mb);
        }
    }
    
    #endregion
    
    
    #region Textures


    public static void ProcessTexture(Handle<T3Texture>? textureHandle, Workspace workspace, MaterialBuilder mb, KnownChannel channel)
    {
        //Make sure the texture exists :D
        if (textureHandle == null || !workspace.ResolveSymbol(textureHandle.ObjectInfo.ObjectName)) return;
        
        // Console.Out.WriteLine("Loading texture: " + textureHandle.ObjectInfo.ObjectName);

        // IFileProvider? textureEntry = workspace.GetFileProviderForResource(textureHandle.ObjectInfo.ObjectName.Crc64);
        //
        // Stream? file = textureEntry?.ExtractFile(textureHandle.ObjectInfo.ObjectName.Crc64);

        Stream? file = workspace.ExtractFile(textureHandle.ObjectInfo.ObjectName.Crc64);
                    
        if(file == null || file.Length < 4)
            return;

        if (file.Position != 0)
            file.Position = 0;

        T3Texture? texture = workspace.TryOpenObject<T3Texture>(file, out _);

        if (texture == null)
            return;

        Texture intermediateTexture = D3dtxCodec.Codec.LoadFromMemory(texture, new CodecOptions());
        
        // intermediateTexture.ConvertToRGBA8sRGB();
        
        if (channel == KnownChannel.Normal)
        {
            //regenerate Z channel
            intermediateTexture.RestoreBC5ZChannel();
        }
        else
        {
            intermediateTexture.ConvertToRGBA8sRGB();
        }
        
        byte[] pngData = PngCodec.Codec.SaveToMemory(intermediateTexture, new CodecOptions(), true);
        
        // File.WriteAllBytes(Path.Combine(outputPath, textureHandle.ObjectInfo.ObjectName + ".dds"), ddsData);

        MemoryImage memImage = new MemoryImage(pngData);
        
        // memImage.SaveToFile(Path.Combine(outputPath, textureHandle.ObjectInfo.ObjectName + "-memImage.dds"));
        
        ImageBuilder image = ImageBuilder.From(memImage, textureHandle.ObjectInfo.ObjectName.ToString());

        mb.WithChannelImage(channel, image);
        mb.AlphaMode = AlphaMode.MASK;
        
        // mb.WithChannelImage(KnownChannel)
        
        Console.Out.WriteLine("Loaded texture: " + textureHandle.ObjectInfo.ObjectName);
    }
    #endregion
    
}