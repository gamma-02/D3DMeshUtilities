using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Numerics;
using System.Text;
using Accessibility;
using D3DMeshUtilities.Code.ImageStuffAUGH;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Memory;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;
using TelltaleToolKit;
using TelltaleToolKit.Resource;
using TelltaleToolKit.Serialization.Binary;
using TelltaleToolKit.T3Types;
using TelltaleToolKit.T3Types.Meshes;
using TelltaleToolKit.T3Types.Meshes.T3Types;
using TelltaleToolKit.T3Types.Textures;
using TelltaleToolKit.TelltaleArchives;
using Image = SharpGLTF.Schema2.Image;
using Texture = D3DMeshUtilities.Code.ImageStuffAUGH.Texture;
using Toolkit = SharpGLTF.Schema2.Toolkit;

namespace D3DMeshUtilities.Code;

public class D3DMeshManager(List<string> file, string outputPath)
{
    public List<string> Files = file;

    public bool MeshRead = false;
    
    public readonly List<D3DMesh?> ReadMeshes = [];

    public void LoadMeshes()
    {
        ReadMeshes.Clear();

        if (!Directory.Exists(outputPath))
            return;

        foreach (var meshFile in Files)
        {
            MemoryStream? stream = LoadedArchive.Instance.CurrentArchive?.ExtractFile(meshFile);

            if (stream == null)
                continue;
            
            if(stream.Length < 4)
                continue;

            if (stream.Position != 0)
                stream.Position = 0;
            
            D3DMesh? mesh = TttkInit.Instance.Workspace?.TryOpenObject<D3DMesh>(stream, out var config);
            
            stream.Close();

            if (mesh == null)
                continue;

            ReadMeshes.Add(mesh);

            var meshData = mesh.MeshData;
            
            //data to look for:
            // map_badge_nm.d3dtx (CRC: EE6008BD7BBF3D27, Offset: 306299554, Size: 1398591)
            // map_badge_spec.d3dtx (CRC: 487C9D9EDEF86051, Offset: 307698145, Size: 175233)
            // obj_badge.d3dtx (CRC: B5FEB46E95390461, Offset: 343975970, Size: 699540)
            // obj_badge.prop (CRC: E16DED721C14BF35, Offset: 344675510, Size: 193)
            

            var verticesPositionList = GetVertices(meshData, 0);
            var verticesNormalsList = GetNormals(meshData, 0);
            var verticesTangentsList = GetTangents(meshData, 0);
            var secondNormalsList = GetNormals(meshData, 0, 1);
            var vertexColorList = GetVertexColors(meshData, 0);
            var vertexTextureCoordList = GetVertexTextureCoords(meshData, 0, 0);
            
            var indexList = GetIndexBuffer(meshData, 0, 0);

            if (verticesPositionList == null || verticesNormalsList == null || verticesTangentsList == null ||
                indexList == null)
            {
                throw new ArgumentNullException(
                    $"One of these is null! {verticesPositionList}, {verticesNormalsList}, {verticesTangentsList}, {indexList}");
            }

            List<MaterialBuilder> materials = [];
            foreach (T3MeshMaterial mat in meshData.Materials)
            {
                var materialName = mat.Material.ObjectInfo.ObjectName.Crc64.ToString();
                var mb = new MaterialBuilder(materialName).WithDoubleSide(false);

                Handle<T3Texture>? materialBaseColor = mesh.GetDiffuseTexture(mat.Material);
                ProcessTexture(materialBaseColor, TttkInit.Instance.Workspace!, mb, KnownChannel.BaseColor);

                Handle<T3Texture>? normalMap = mesh.GetNormalMapTexture(mat.Material);
                ProcessTexture(normalMap, TttkInit.Instance.Workspace!, mb, KnownChannel.Normal);

                Handle<T3Texture>? specularMap = mesh.GetSpecularTexture(mat.Material);
                ProcessTexture(specularMap, TttkInit.Instance.Workspace!, mb, KnownChannel.SpecularColor);
                
                //unknown: what is detail texture and how is it used

                materials.Add(mb);
            }
            
            
            
            //todo: make this dynamic! gonna have to do a bunch of variations for different combos :3
            //todo: add back ColorTexture2!!!!!!
            
            // var material = new MaterialBuilder("material");

            var meshBuilder = new MeshBuilder<VertexPositionNormalTangent, VertexTexture1, VertexEmpty>("mesh1");
            //
            // var fileStream = File.OpenWrite(Path.Combine(outputPath, "archive-dump.txt"));
            // LoadedArchive.Instance.CurrentArchive?.FileEntries.ToList().ForEach(fe =>
            // {
            //     fileStream.Write(Encoding.ASCII.GetBytes(fe.ToString() + "\n"));
            // });
            
            List<VertexBuilder<VertexPositionNormalTangent, VertexTexture1, VertexEmpty>> verts = [];

            for (int vertexIndex = 0; vertexIndex < verticesPositionList.Count; vertexIndex++)
            {
                Vector4 pos = verticesPositionList[vertexIndex];
                Vector4 normal = verticesNormalsList[vertexIndex];
                Vector4 tanget = verticesTangentsList[vertexIndex];
                Vector2 texCoord = vertexTextureCoordList[vertexIndex];
                
                Vector3 scaledPos = new Vector3(
                    pos.X * meshData.PositionScale.X,
                    pos.Y * meshData.PositionScale.Y,
                    pos.Z * meshData.PositionScale.Z
                );

                Vector3 finalPos = new Vector3(
                    scaledPos.X + meshData.PositionOffset.X + pos.W * meshData.PositionWScale.X,
                    scaledPos.Y + meshData.PositionOffset.Y + pos.W * meshData.PositionWScale.Y,
                    scaledPos.Z + meshData.PositionOffset.Z + pos.W * meshData.PositionWScale.Z);
                
                var vertex = new VertexBuilder<VertexPositionNormalTangent, VertexTexture1, VertexEmpty>
                {
                    Geometry = new VertexPositionNormalTangent(
                        finalPos,
                        Vector3.Normalize(new Vector3(normal.X, normal.Y, normal.Z)),
                        Vector4.Normalize(tanget)
                    ),
                    Material = new VertexTexture1(
                        texCoord
                    )
                };
                
                verts.Add(vertex);
                
            }


            //todo: working for now on just one LOD, will change later
            T3MeshLOD lod = meshData.LODs[0];

            List<T3MeshBatch> allBatches = [];
            allBatches.AddRange(lod.Batches);
            allBatches.AddRange(lod.Batches1);
            allBatches.AddRange(lod.Batches2);

            //todo: also working with just one material, will change later 
            MaterialBuilder material = materials[0];

            foreach (T3MeshBatch batch in allBatches)
            {
                Console.Out.WriteLine(batch.AdjacencyStartIndex);

                for (uint indexI = batch.StartIndex + 2; indexI < (batch.StartIndex + batch.NumIndices); indexI += 3)
                {
                    int i = (int)indexI;
                    
                    int vertIOne = (int)indexList[i - 2];
                    int vertITwo = (int)indexList[i - 1];
                    int vertIThree = (int)indexList[i];

                    var vertOne = verts[vertIOne];
                    var vertTwo = verts[vertITwo];
                    var vertThree = verts[vertIThree];
                    
                    meshBuilder.UsePrimitive(material).AddTriangle(vertOne, vertTwo, vertThree);
                }
            }
            
            SceneBuilder scene = new SceneBuilder();

            var rootNode = new NodeBuilder(meshFile.Remove(meshFile.Length - ".d3dmesh".Length));
            
            rootNode.WithLocalScale(new Vector3(10.0f));
            
            scene.AddRigidMesh(meshBuilder, rootNode);

            ModelRoot root = scene.ToGltf2();
            
            string outPath = Path.Combine(outputPath, meshFile.Replace("d3dmesh", "glb"));

            root.SaveGLB(outPath);

            Console.Out.WriteLine($"Saved at: {outPath}");
            
        }
        
        
        MeshRead = true;
    }

    


    private static List<Vector4>? GetVertices(T3MeshData meshData, int vertexStateIndex)
    {

        var vertexState = meshData.VertexStates[vertexStateIndex];

        T3GFXBuffer? vertexPositionBuffer = null;

        for (uint index = 0; index < vertexState.AttributeCount; index++)
        {
            var vertexAttribute = vertexState.Attributes[(int)index];

            if (vertexAttribute.Attribute == GFXPlatformVertexAttribute.Position)
            {
                vertexPositionBuffer = vertexState.VertexBuffer[(int)index];
                break;
            }
            
        }

        if (vertexPositionBuffer == null)
            return null;


        if (vertexPositionBuffer.BufferFormat != GFXPlatformFormat.UN10x3_UN2)
            return null;

        ReadOnlySpan<byte> vertexBuffer = vertexPositionBuffer.Buffer;

        List<Vector4> vertices = new List<Vector4>();

        uint readerIndex = 0;

        while (readerIndex < vertexBuffer.Length)
        {
            vertices.Add(ConvertFromGfxPlatformFormat.ReadUN10x3_UN2(vertexBuffer.Slice((int)readerIndex)));

            readerIndex += vertexPositionBuffer.Stride;

        }

        return vertices;
    }
    
    private static List<Vector4>? GetNormals(T3MeshData meshData, int vertexStateIndex, int normalsToSkip = 0)
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
    
    private static List<Vector4>? GetTangents(T3MeshData meshData, int vertexStateIndex)
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
    
    private List<Vector4>? GetVertexColors(T3MeshData meshData, int vertexStateIndex)
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
    
        List<Vector4> colors = new List<Vector4>();
    
        uint readerIndex = 0;
    
        while (readerIndex < colorBuffer.Length)
        {
            colors.Add((Vector4)ConvertFromGfxPlatformFormat.ReadVector4FromSpanAndFormat(colorBuffer.Slice((int)readerIndex), format)!);
    
            readerIndex += vertexColorBuffer.Stride;
    
        }
    
        return colors;
    }
    
    private List<Vector2>? GetVertexTextureCoords(T3MeshData meshData, int vertexStateIndex, int texture = 0)
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


    private static List<uint>? GetIndexBuffer(T3MeshData meshData, int vertexStateIndex, int vertexPositionBufferIndex)
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


    public void ProcessTexture(Handle<T3Texture>? textureHandle, Workspace workspace, MaterialBuilder mb, KnownChannel channel)
    {
        //Make sure the texture exists :D
        if (textureHandle == null || !workspace.ResolveSymbol(textureHandle.ObjectInfo.ObjectName)) return;
        
        // Console.Out.WriteLine("Loading texture: " + textureHandle.ObjectInfo.ObjectName);

        IFileProvider? textureEntry = workspace.GetFileProviderForResource(textureHandle.ObjectInfo.ObjectName.Crc64);

        Stream? file = textureEntry?.ExtractFile(textureHandle.ObjectInfo.ObjectName.Crc64);
                    
        if(file == null || file.Length < 4)
            return;

        if (file.Position != 0)
            file.Position = 0;

        T3Texture? texture = workspace.TryOpenObject<T3Texture>(file, out _);

        if (texture == null)
            return;

        Texture intermediateTexture = D3dtxCodec.Codec.LoadFromMemory(texture, new CodecOptions());
        
        intermediateTexture.ConvertToRGBA8sRGB();
        
        byte[] pngData = PngCodec.Codec.SaveToMemory(intermediateTexture, new CodecOptions(), true);
        
        // File.WriteAllBytes(Path.Combine(outputPath, textureHandle.ObjectInfo.ObjectName + ".dds"), ddsData);
        
        MemoryImage memImage = new MemoryImage(pngData);
        
        // memImage.SaveToFile(Path.Combine(outputPath, textureHandle.ObjectInfo.ObjectName + "-memImage.dds"));
        
        
        ImageBuilder image = ImageBuilder.From(memImage, textureHandle.ObjectInfo.ObjectName.ToString());

        mb.WithChannelImage(channel, image);
        
        Console.Out.WriteLine("Loaded texture: " + textureHandle.ObjectInfo.ObjectName);
    }
    
    
    
}