using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Numerics;
using System.Text;
using Accessibility;
using D3DMeshUtilities.Code.D3DMeshFormats;
using D3DMeshUtilities.Code.ImageStuffAUGH;
using D3DMeshUtilities.Code.MeshHandling;
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
using TelltaleToolKit.T3Types.Properties;
using TelltaleToolKit.T3Types.Skeletons;
using TelltaleToolKit.T3Types.Textures;
using TelltaleToolKit.TelltaleArchives;
using AlphaMode = SharpGLTF.Materials.AlphaMode;
using Codecs = D3DMeshUtilities.Code.MeshHandling.Codecs;
using Image = SharpGLTF.Schema2.Image;
using Texture = D3DMeshUtilities.Code.ImageStuffAUGH.Texture;
using Toolkit = SharpGLTF.Schema2.Toolkit;

namespace D3DMeshUtilities.Code;

public class D3DMeshManager(List<string> file, string outputPath)
{
    
    // these were erroring because I was only parsing UN10x3_UN2
    //now I'm parsing all Vector4 formats
    //done: meshes A-E
    //done: birthday banner 
    
    //These were wierd because I hadn't set up texture coordinate transformations
    //done: arcade game meshes (materials?)
    //done: poker booster seat (textures?)
    
    //todo: elevatorshaft, exterior, grotien bar case, poker card (armature/skinning?)
    //todo: obj_cellPhoneHomestar (armature/skinning?)
    //todo: computerSinistar301 (armature/skinning?)
    
    // This was missing per-vertex tangents, which I implemented in the NormalModelIntermediate
    //along with more generalizaitons and abstractions :3
    //done: disguiseGlassesDangeresqueToo tangents?? - This was missing the per-vertex tangents
    
    //done: obj_skyGeneric (allowing for direct encoding of vertex positions)
    
    //todo: make sure normals are actually being correct
    
    //todo: why wasn't heavy mesh as table working?
    
    public List<string> Files = file;

    public bool MeshRead = false;
    
    public readonly List<D3DMesh?> ReadMeshes = [];

    public void LoadMeshes()
    {
        AsyncSerachForSkeletonFiles.BuildDictionaryTask = Task.Run(() => AsyncSerachForSkeletonFiles.BuildAgentMeshDictionary(LoadedArchive.Instance));
        
        ReadMeshes.Clear();

        if (!Codecs.Registered)
        {
            Codecs.RegisterCodecs();
        }


        // List<PropertySet> propertySets = skeletonProperties.ToList();
        
        
        if (!Directory.Exists(outputPath))
            return;

        foreach (var meshFile in Files)
        {
            MemoryStream? stream = LoadedArchive.Instance.CurrentArchive?.ExtractFile(meshFile);

            if (stream == null)
                continue;

            if (stream.Length < 4)
                continue;

            if (stream.Position != 0)
                stream.Position = 0;

            D3DMesh? mesh = TttkInit.Instance.Workspace?.TryOpenObject<D3DMesh>(stream, out var config);

            stream.Close();

            Console.Out.WriteLine($"Attemtping to decode {meshFile}");
            
            if (mesh == null)
                continue;

            ReadMeshes.Add(mesh);

            MeshInfo info = new MeshInfo(mesh, meshFile);

            IMeshCodec? codec = info.GetMeshRepresentation(mesh);

            if (codec == null)
            {
                Console.Out.WriteLine($"Failed to find a mesh codec that can decode {meshFile}, skipping!");
                
                continue;
            }

            bool succeeded = codec.Read(mesh, info, meshFile, out IMeshRepresentation? intermediateMesh);
                // NormalModelIntermediate.Read(mesh, meshFile, out NormalModelIntermediate? intermediateMesh);

            if (!succeeded || intermediateMesh == null)
                continue;


            if (!intermediateMesh .SaveToMeshBuilder(out var meshBuilder))
            {
                Console.Out.WriteLine("Failed to create mesh!");
                
                continue;
            }
            
            
            SceneBuilder scene = new SceneBuilder();

            var rootNode = new NodeBuilder(meshFile.Remove(meshFile.Length - ".d3dmesh".Length));
            
            rootNode.WithLocalScale(new Vector3(10.0f));
            
            scene.AddRigidMesh(meshBuilder, rootNode);

            ModelRoot root = scene.ToGltf2();
            
            string outPath = Path.Combine(outputPath, meshFile.Replace("d3dmesh", "glb"));

            root.SaveGLB(outPath);

            Console.Out.WriteLine($"Succeeded in converting {meshFile}! \n\tSaved at: {outPath}");

        }

    }
    
    #region old mesh loading
    // public void LoadMeshesOld()
    // {
    //     ReadMeshes.Clear();
    //
    //     if (!Directory.Exists(outputPath))
    //         return;
    //
    //     foreach (var meshFile in Files)
    //     {
    //         MemoryStream? stream = LoadedArchive.Instance.CurrentArchive?.ExtractFile(meshFile);
    //
    //         if (stream == null)
    //             continue;
    //         
    //         if(stream.Length < 4)
    //             continue;
    //
    //         if (stream.Position != 0)
    //             stream.Position = 0;
    //         
    //         D3DMesh? mesh = TttkInit.Instance.Workspace?.TryOpenObject<D3DMesh>(stream, out var config);
    //         
    //         stream.Close();
    //         
    //         Console.Out.WriteLine($"Attemtping to decode {meshFile}");
    //
    //         if (mesh == null)
    //             continue;
    //
    //         ReadMeshes.Add(mesh);
    //
    //         var meshData = mesh.MeshData;
    //         
    //         List<Vector4>? verticesPositionList = MeshUtils.GetVertices(meshData, 0);
    //         List<Vector4>? verticesNormalsList = MeshUtils.GetNormals(meshData, 0);
    //         List<Vector4>? verticesTangentsList = MeshUtils.GetTangents(meshData, 0);
    //         List<Vector4>? secondNormalsList = MeshUtils.GetNormals(meshData, 0, 1);
    //         List<Vector4>? vertexColorList = MeshUtils.GetVertexColors(meshData, 0);
    //         List<Vector2>? vertexTextureCoordList = MeshUtils.GetVertexTextureCoords(meshData, 0, 0);
    //         List<Vector2>? vertexTextureCoordList2 = MeshUtils.GetVertexTextureCoords(meshData, 0, 1);
    //         List<Vector2>? vertexTextureCoordList3 = MeshUtils.GetVertexTextureCoords(meshData, 0, 2);
    //         List<Vector2>? vertexTextureCoordList4 = MeshUtils.GetVertexTextureCoords(meshData, 0, 3);
    //
    //         
    //         List<uint>? indexList = MeshUtils.GetIndexBuffer(meshData, 0, 0);
    //
    //         try
    //         {
    //             ArgumentNullException.ThrowIfNull(verticesPositionList);
    //             ArgumentNullException.ThrowIfNull(verticesNormalsList);
    //             ArgumentNullException.ThrowIfNull(verticesTangentsList);
    //             ArgumentNullException.ThrowIfNull(vertexTextureCoordList);
    //         }
    //         catch(ArgumentNullException e)
    //         {
    //             
    //             Console.Out.WriteLine($"Failed reading {meshFile}! See: " + e);
    //             
    //             continue;
    //         }
    //
    //         if (verticesPositionList == null || verticesNormalsList == null || verticesTangentsList == null || vertexTextureCoordList == null ||
    //             indexList == null)
    //         {
    //             throw new ArgumentNullException(
    //                 $"One of these is null! {verticesPositionList}, {verticesNormalsList}, {verticesTangentsList}, {vertexTextureCoordList}, {indexList}");
    //         }
    //         
    //
    //         List<MaterialBuilder> materials = [];
    //         foreach (T3MeshMaterial mat in meshData.Materials)
    //         {
    //             var materialName = mat.Material.ObjectInfo.ObjectName.Crc64.ToString();
    //
    //             bool doubleSided = true;
    //
    //             if (mesh.InternalResources.Find(res => res.ObjectInfo.ObjectName == mat.Material.ObjectInfo.ObjectName)?
    //                     .ObjectInfo.HandleObject is PropertySet p)
    //             {
    //                 TttkInit.Instance.Workspace!.ResolveSymbols(p.Properties.Keys);
    //
    //                 // foreach (Symbol s in p.Properties.Keys)
    //                 // {
    //                 //     Console.Out.WriteLine($"{s} : {p.Properties[s].MetaType!.FullTypeName} ; {p.Properties[s].Value}");
    //                 // }
    //                 
    //                 //the property '5971B48CE79829D9' corresponds to if a model should render double sided
    //                 bool? sided = p.GetProperty<bool>(new Symbol(0x5971B48CE79829D9));
    //
    //                 if (sided != null)
    //                 {
    //                     doubleSided = sided.Value;
    //                 }
    //                 
    //             }
    //             
    //             
    //             var mb = new MaterialBuilder(materialName).WithDoubleSide(doubleSided);
    //
    //             Handle<T3Texture>? materialBaseColor = mesh.GetDiffuseTexture(mat.Material);
    //             MeshUtils.ProcessTexture(materialBaseColor, TttkInit.Instance.Workspace!, mb, KnownChannel.BaseColor);
    //
    //             Handle<T3Texture>? normalMap = mesh.GetNormalMapTexture(mat.Material);
    //             MeshUtils.ProcessTexture(normalMap, TttkInit.Instance.Workspace!, mb, KnownChannel.Normal);
    //
    //             Handle<T3Texture>? specularMap = mesh.GetSpecularTexture(mat.Material);
    //             MeshUtils.ProcessTexture(specularMap, TttkInit.Instance.Workspace!, mb, KnownChannel.SpecularColor);
    //             
    //             //todo: detail texture, used for lines and stuff on models
    //             Handle<T3Texture>? detailMap = mesh.GetDetailTexture(mat.Material);
    //             // ProcessTexture(detailMap, TttkInit.Instance.Workspace!, mb, KnownChannel);
    //
    //             if (detailMap != null)
    //             {
    //                 Console.Out.WriteLine("OOH, detail map! " + detailMap);
    //                 
    //             }
    //             
    //             materials.Add(mb);
    //         }
    //         
    //         
    //         
    //         
    //         //todo: make this dynamic! gonna have to do a bunch of variations for different combos :3
    //         //todo: add back ColorTexture2!!!!!!
    //         
    //         var meshBuilder = new MeshBuilder<VertexPositionNormalTangent, VertexTexture1, VertexEmpty>("mesh1");
    //         
    //         List<VertexBuilder<VertexPositionNormalTangent, VertexTexture1, VertexEmpty>> verts = [];
    //
    //         for (int vertexIndex = 0; vertexIndex < verticesPositionList.Count; vertexIndex++)
    //         {
    //             Vector4 pos = verticesPositionList[vertexIndex];
    //             Vector4 normal = verticesNormalsList[vertexIndex];
    //             Vector4 tanget = verticesTangentsList[vertexIndex];
    //
    //             Vector2 texCoord1 = vertexTextureCoordList[vertexIndex];
    //             
    //             if(meshData.TexCoordTransform[0] != null && meshData.TexCoordTransform[0].Scale != null)
    //             { 
    //                 texCoord1 *= meshData.TexCoordTransform[0].Scale;
    //             }else if (meshData.TexCoordTransform[0] != null && meshData.TexCoordTransform[0].Offset != null)
    //             {
    //                 texCoord1 += meshData.TexCoordTransform[0].Offset;
    //             }
    //             
    //             var scaledPos = new Vector3(
    //                 pos.X * meshData.PositionScale.X,
    //                 pos.Y * meshData.PositionScale.Y,
    //                 pos.Z * meshData.PositionScale.Z
    //             );
    //
    //             var finalPos = new Vector3(
    //                 scaledPos.X + meshData.PositionOffset.X + pos.W * meshData.PositionWScale.X,
    //                 scaledPos.Y + meshData.PositionOffset.Y + pos.W * meshData.PositionWScale.Y,
    //                 scaledPos.Z + meshData.PositionOffset.Z + pos.W * meshData.PositionWScale.Z
    //             );
    //             
    //             var vertex = new VertexBuilder<VertexPositionNormalTangent, VertexTexture1, VertexEmpty>
    //             {
    //                 Geometry = new VertexPositionNormalTangent(
    //                     finalPos, 
    //                     Vector3.Normalize(new Vector3(normal.X, normal.Y, normal.Z)),
    //                     Vector4.Normalize(tanget)
    //                 ),
    //                 Material = new VertexTexture1(
    //                     texCoord1
    //                 )
    //             };
    //             
    //             verts.Add(vertex);
    //             
    //         }
    //
    //
    //         //todo: working for now on just one LOD, will change later
    //         T3MeshLOD lod = meshData.LODs[0];
    //
    //         List<T3MeshBatch> allBatches = [];
    //         allBatches.AddRange(lod.Batches);
    //         allBatches.AddRange(lod.Batches1);
    //         allBatches.AddRange(lod.Batches2);
    //         
    //         foreach (T3MeshBatch batch in allBatches)
    //         {
    //
    //             MaterialBuilder mat = materials[batch.MaterialIndex];
    //
    //             for (uint indexI = batch.StartIndex + 2; indexI < (batch.StartIndex + batch.NumIndices); indexI += 3)
    //             {
    //                 int i = (int)indexI;
    //                 
    //                 int vertIOne = (int)indexList[i - 2];
    //                 int vertITwo = (int)indexList[i - 1];
    //                 int vertIThree = (int)indexList[i];
    //
    //                 var vertOne = verts[vertIOne];
    //                 var vertTwo = verts[vertITwo];
    //                 var vertThree = verts[vertIThree];
    //                 
    //                 meshBuilder.UsePrimitive(mat).AddTriangle(vertOne, vertTwo, vertThree);
    //             }
    //         }
    //         
    //         SceneBuilder scene = new SceneBuilder();
    //
    //         var rootNode = new NodeBuilder(meshFile.Remove(meshFile.Length - ".d3dmesh".Length));
    //         
    //         rootNode.WithLocalScale(new Vector3(10.0f));
    //         
    //         scene.AddRigidMesh(meshBuilder, rootNode);
    //
    //         ModelRoot root = scene.ToGltf2();
    //         
    //         string outPath = Path.Combine(outputPath, meshFile.Replace("d3dmesh", "glb"));
    //
    //         root.SaveGLB(outPath);
    //
    //         Console.Out.WriteLine($"Succeeded in converting {meshFile}! Saved at: {outPath}");
    //         
    //     }
    //     
    //     MeshRead = true;
    //     
    // }
    //
    #endregion
    
    public void SaveTexture(Handle<T3Texture>? textureHandle, Workspace workspace)
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
        
        intermediateTexture.ConvertToRGBA8sRGB();
        
        byte[] pngData = PngCodec.Codec.SaveToMemory(intermediateTexture, new CodecOptions(), true);
        
        File.WriteAllBytes(Path.Combine(outputPath, textureHandle.ObjectInfo.ObjectName + ".png"), pngData);
        
        Console.Out.WriteLine("Saved texture: " + textureHandle.ObjectInfo.ObjectName);
    }
    
    
    
}