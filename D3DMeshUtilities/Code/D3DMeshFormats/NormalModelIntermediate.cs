using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using TelltaleToolKit.T3Types;
using TelltaleToolKit.T3Types.Meshes;
using TelltaleToolKit.T3Types.Meshes.T3Types;
using TelltaleToolKit.T3Types.Properties;
using TelltaleToolKit.T3Types.Textures;

namespace D3DMeshUtilities.Code.D3DMeshFormats;

public class NormalModelIntermediate
{
    public List<Vector3> VertexPositions;
    public List<Vector3> VertexNormals;
    public List<Vector4> VertexTangents;
    
    //unusued/unknown:
    //public List<Vector4>? SecondVertexNormalsList;
    //public List<Vector4>? VertexColorList;

    public List<List<Vector2>?> TextureCoordLists = new(4);
    
    //this is the list of index buffers specified by 
    public List<List<uint>?> IndexBuffers;
    
    public List<MaterialBuilder> Materials;

    public List<T3MeshLOD> LODs;
    
    // public List<IVertexBuilder> vertices

    
    public NormalModelIntermediate(List<Vector3> vertexPositionList, List<Vector3> vertexNormalList, List<Vector4> vertexTangentsList, List<List<Vector2>?> textureCoords, List<List<uint>?> indexBuffers, List<MaterialBuilder> materials, List<T3MeshLOD> loDs)
    {
        
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
    }


    public static bool Read(D3DMesh mesh, string meshFile, out NormalModelIntermediate? readMesh)
    {
        var meshData = mesh.MeshData;
            
        List<Vector4>? rawPositionList = MeshUtils.GetVertices(meshData, 0);
        List<Vector4>? rawNormalsList = MeshUtils.GetNormals(meshData, 0);
        List<Vector4>? tangentsList = MeshUtils.GetTangents(meshData, 0);
        
        //unusued, I think, but keeping them here because they're on everything
        // List<Vector4>? secondNormalsList = MeshUtils.GetNormals(meshData, 0, 1);
        // List<Vector4>? vertexColorList = MeshUtils.GetVertexColors(meshData, 0);
        
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
        
        if (rawPositionList == null || rawNormalsList == null || tangentsList == null || vertexTextureCoordList1 == null ||
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
            tangentsList[vertexIndex] = Vector4.Normalize(tangentsList[vertexIndex]);

        }

        
        // List<MaterialBuilder> materials = [];
        MeshUtils.GetMaterials(mesh, meshData, out List<MaterialBuilder> materials);

        readMesh = new NormalModelIntermediate(vertexPositions, normals, tangentsList,
            textureCoords, [indexList], materials, meshData.LODs);

        return true;

    }

    //todo: look into exporting LODs as seprate meshBuilders
    public bool SaveToGLTF(out MeshBuilder<VertexPositionNormalTangent, VertexTexture1, VertexEmpty> meshBuilder)
    {
        meshBuilder = new MeshBuilder<VertexPositionNormalTangent, VertexTexture1, VertexEmpty>("mainMesh");
        
        //build list of vertices
        List<VertexBuilder<VertexPositionNormalTangent, VertexTexture1, VertexEmpty>> verts = new(VertexPositions.Count);

        for (int i = 0; i < VertexPositions.Count; i++)
        {
            //no skinning data :D
            var vertex = new VertexBuilder<VertexPositionNormalTangent, VertexTexture1, VertexEmpty>
            {
                Geometry = new VertexPositionNormalTangent(
                    VertexPositions[i], 
                    VertexNormals[i],
                    VertexTangents[i]
                ),
                Material = new VertexTexture1(
                    TextureCoordLists[0]![i]
                )
            };

            verts.Add(vertex);
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

            for (uint indexI = batch.StartIndex + 2; indexI < (batch.StartIndex + batch.NumIndices); indexI += 3)
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

    public bool SaveToMeshBuilder(out IMeshBuilder<MaterialBuilder> meshBuilder)
    {
        bool succeeded = SaveToGLTF(out var mb);

        meshBuilder = mb;

        return succeeded;
    }
    
}