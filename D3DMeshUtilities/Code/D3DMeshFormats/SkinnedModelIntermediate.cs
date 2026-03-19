using System.IO;
using System.Numerics;
using D3DMeshUtilities.Code.MeshHandling;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using TelltaleToolKit.T3Types;
using TelltaleToolKit.T3Types.Meshes;
using TelltaleToolKit.T3Types.Meshes.T3Types;
using TelltaleToolKit.T3Types.Properties;
using TelltaleToolKit.T3Types.Skeletons;

namespace D3DMeshUtilities.Code.D3DMeshFormats;

public class SkinnedModelIntermediate : IMeshRepresentation
{
    
    public MeshInfo Info { get; private set; }
    
    public List<Vector3> VertexPositions;
    public List<Vector3> VertexNormals;
    public List<Vector4>? VertexTangents;
    
    //unusued/unknown:
    //public List<Vector4>? SecondVertexNormalsList;
    //public List<Vector4>? VertexColorList;

    public List<List<Vector2>?> TextureCoordLists = new(4);
    
    //this is the list of index buffers specified by 
    public List<List<uint>?> IndexBuffers;
    
    public List<MaterialBuilder> Materials;

    public List<T3MeshLOD> LODs;

    public Skeleton Skeleton;

    public List<Vector<int>> BlendIndices;
    public List<Vector4> BlendWeights;
    
    
    // public List<IVertexBuilder> vertices

    
    public SkinnedModelIntermediate(
        MeshInfo info, 
        List<Vector3> vertexPositionList, 
        List<Vector3> vertexNormalList, 
        List<Vector4>? vertexTangentsList, 
        List<List<Vector2>?> textureCoords, 
        List<List<uint>?> indexBuffers, 
        List<MaterialBuilder> materials, 
        List<T3MeshLOD> loDs, 
        Skeleton skeleton, 
        List<Vector<int>> blendIndices, 
        List<Vector4> blendWeights)
    {
        Info = info;
        
        VertexPositions = vertexPositionList;
        VertexNormals = vertexNormalList;
        VertexTangents = vertexTangentsList;

        for (int i = 0; i < textureCoords.Count; i++)
        {
            if (textureCoords[i] != null)
            {
                TextureCoordLists.Add(textureCoords[i]);
            }
        }
        
        IndexBuffers = indexBuffers;

        Materials = materials;
        LODs = loDs;
        Skeleton = skeleton;
        BlendIndices = blendIndices;
        BlendWeights = blendWeights;
    }


    public static bool Read(D3DMesh mesh, MeshInfo info, string meshFile, out SkinnedModelIntermediate? readMesh)
    {
        var meshData = mesh.MeshData;
        
        AsyncSerachForSkeletonFiles.AgentMeshDictionaryLock.Enter();
        
        PropertySet? agentProp = null;

        if (info.Crc64.HasValue)
        {
            agentProp = AsyncSerachForSkeletonFiles.AgentPropertiesByMeshFile[info.Crc64.Value];
        }
        
        AsyncSerachForSkeletonFiles.AgentMeshDictionaryLock.Exit();

        Handle<Skeleton>? skeletonReference = agentProp?.GetProperty<Handle<Skeleton>>("Skeleton File");

        if (skeletonReference == null)
        {
            readMesh = null;
            return false;
        }

        Skeleton? skeleton = skeletonReference.ObjectInfo.HandleObject as Skeleton;

        if (skeleton == null)
        {
            skeleton = TttkInit.Instance.Workspace!.LoadAsset<Skeleton>(skeletonReference.ObjectInfo.ObjectName.Crc64);

            if (skeleton == null)
            {
                readMesh = null;
                return false;
            }
        }
        
        List<Vector4>? rawPositionList = MeshUtils.GetVertices(meshData, info, 0);
        List<Vector4>? rawNormalsList = MeshUtils.GetNormals(meshData, 0);
        List<Vector4>? tangentsList = MeshUtils.GetTangents(meshData, 0);
        
        //unusued, I think, but keeping them here because they're on everything
        // List<Vector4>? secondNormalsList = MeshUtils.GetNormals(meshData, 0, 1);
        // List<Vector4>? vertexColorList = MeshUtils.GetVertexColors(meshData, 0);
        
        
        List<Vector<int>>? vertexBlendIndices = MeshUtils.GetVertexBlendIndices(meshData, 0);
        List<Vector4>? vertexBlendWeights = MeshUtils.GetVertexBlendWeights(meshData, 0);
        
        
        List<Vector2>? vertexTextureCoordList1 = MeshUtils.GetVertexTextureCoords(meshData, 0);
        List<Vector2>? vertexTextureCoordList2 = MeshUtils.GetVertexTextureCoords(meshData, 0, 1);
        List<Vector2>? vertexTextureCoordList3 = MeshUtils.GetVertexTextureCoords(meshData, 0, 2);
        List<Vector2>? vertexTextureCoordList4 = MeshUtils.GetVertexTextureCoords(meshData, 0, 3);

        

        List<Vector2>?[] textureCoordArray = [vertexTextureCoordList1, vertexTextureCoordList2, vertexTextureCoordList3, vertexTextureCoordList4];
        
        List<List<Vector2>?> textureCoords =
            [vertexTextureCoordList1, vertexTextureCoordList2, vertexTextureCoordList3, vertexTextureCoordList4];
        
        //todo: how are multiple index buffers used?
        List<uint>? indexList = MeshUtils.GetIndexBuffer(meshData, 0, 0);

        try
        {
            ArgumentNullException.ThrowIfNull(rawPositionList);
            ArgumentNullException.ThrowIfNull(rawNormalsList);
            
            if(info.HasVertexTangents)
                ArgumentNullException.ThrowIfNull(tangentsList);
            
            //the other texture coord lists are optional, but we need the first one
            ArgumentNullException.ThrowIfNull(vertexTextureCoordList1);

            //and we need the index list
            ArgumentNullException.ThrowIfNull(indexList);
        }
        catch(ArgumentNullException e)
        {
                
            Console.Out.WriteLine($"Failed reading {meshFile}! See: " + e);

            readMesh = null;
                
            return false;
        }
        
        if (rawPositionList == null || rawNormalsList == null || (tangentsList == null && info.HasVertexTangents) || vertexTextureCoordList1 == null ||
            indexList == null)
        {
            throw new ArgumentNullException(
                $"One of these is null! {rawPositionList}, {rawNormalsList}, {tangentsList}, {vertexTextureCoordList1}, {indexList}");
        }

        List<Vector3> vertexPositions = new List<Vector3>(rawPositionList.Count);
        List<Vector3> normals = new List<Vector3>(rawNormalsList.Count);
        
        for (int vertexIndex = 0; vertexIndex < rawPositionList.Count; vertexIndex++)
        {
            MeshUtils.ApplyTransforms(meshData, vertexIndex, rawPositionList[vertexIndex], vertexPositions, textureCoordArray);

            //normalize, because the interpreted 
            var normal = rawNormalsList[vertexIndex].AsVector3();
            
            normals.Add(Vector3.Normalize(normal));
            if(info.HasVertexTangents)
                tangentsList![vertexIndex] = Vector4.Normalize(tangentsList[vertexIndex]);

        }
        
        // List<MaterialBuilder> materials = [];
        MeshUtils.GetMaterials(mesh, info, out List<MaterialBuilder> materials);

        //todo: null check blend stuff
        readMesh = new SkinnedModelIntermediate(info, vertexPositions, normals, tangentsList,
            textureCoords, [indexList], materials, meshData.LODs, skeleton, vertexBlendIndices!, vertexBlendWeights!);

        return true;

    }

    //todo: look into exporting LODs as seprate meshBuilders
    public bool SaveToGLTF(out IMeshBuilder<MaterialBuilder> meshBuilder)
    {
        if (!Info.HasVertexTangents)
            meshBuilder = new MeshBuilder<VertexPositionNormal, VertexTexture1, VertexEmpty>("mainMesh");
        else
            meshBuilder = new MeshBuilder<VertexPositionNormalTangent, VertexTexture1, VertexEmpty>("mainMesh");
        
        //build list of vertices
        List<IVertexBuilder> verts = new(VertexPositions.Count);

        for (int i = 0; i < VertexPositions.Count; i++)
        {
            // IVertexBuilder vertex;
            //no skinning data :D
            if (Info.HasVertexTangents)
            {
                var vertex = new VertexBuilder<VertexPositionNormalTangent, VertexTexture1, VertexEmpty>
                {
                    Geometry = new VertexPositionNormalTangent(
                        VertexPositions[i],
                        VertexNormals[i],
                        VertexTangents![i]
                    ),
                    Material = new VertexTexture1(
                        TextureCoordLists[0]![i]
                    )
                };
                
                verts.Add(vertex);
            }
            else
            {
                var vertex = new VertexBuilder<VertexPositionNormal, VertexTexture1, VertexJoints4>
                {
                    Geometry = new VertexPositionNormal(
                        VertexPositions[i],
                        VertexNormals[i]
                    ),
                    Material = new VertexTexture1(
                        TextureCoordLists[0]![i]
                    )
                };
                verts.Add(vertex);
            }
        }
        
        
        T3MeshLOD lod = LODs[0];

        List<T3MeshBatch> allBatches = [];
        allBatches.AddRange(lod.Batches);
        allBatches.AddRange(lod.Batches1);
        allBatches.AddRange(lod.Batches2);
            
        foreach (T3MeshBatch batch in allBatches)
        {

            List<uint>? indexList = IndexBuffers[0];

            if (indexList == null)
                return false;

            MaterialBuilder mat = Materials[batch.MaterialIndex];

            //limit validation -- we need to decide between two limits:
            //      StartIndex + NumIndices and MaxVertIndex
            uint limit;
            uint numIndicesLimit = batch.StartIndex + batch.NumIndices;
            uint MaxVertIndexLimit = batch.MaxVertIndex;
            
            //first make the choice of numIndicesLimit (it worked until obj_skyGeneric.d3dmesh) (have i talked about obj_skyGeneric.d3dmesh?) (it's evil. I am this world's number one obj_skyGeneric hater) (it was the only thing preventing me from saying that i can load every model) (and then I though adding support for it would be easy but NOOOO it had to be fucked up didn't it)
            limit = numIndicesLimit;

            if (limit % 3 != 0 || limit >= indexList.Count)
            {
                limit = MaxVertIndexLimit;
            }

            for (uint indexI = batch.StartIndex + 2; indexI < limit; indexI += 3)
            {
                int i = (int)indexI;
                    
                int vertIOne = (int)indexList[i - 2];
                int vertITwo = (int)indexList[i - 1];
                int vertIThree = (int)indexList[i];

                var vertOne = verts[vertIOne];
                var vertTwo = verts[vertITwo];
                var vertThree = verts[vertIThree];
                    
                meshBuilder.UsePrimitive(mat).AddTriangle(vertOne, vertTwo, vertThree);
            }
        }

        return true;

    }

    public MeshInfo GetMeshInfo()
    {
        return Info;
    }

    public bool SaveToMeshBuilder(out IMeshBuilder<MaterialBuilder> meshBuilder)
    {
        bool succeeded = SaveToGLTF(out var mb);

        meshBuilder = mb;

        return succeeded;
    }

    
}