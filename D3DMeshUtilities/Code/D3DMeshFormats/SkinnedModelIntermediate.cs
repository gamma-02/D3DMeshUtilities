using System;
using System.Collections.Generic;
using System.Numerics;
using D3DMeshUtilities.Code.MeshHandling;
using D3DMeshUtilities.Code.Util;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using TelltaleToolKit.T3Types;
using TelltaleToolKit.T3Types.Meshes;
using TelltaleToolKit.T3Types.Meshes.T3Types;
using TelltaleToolKit.T3Types.Properties;
using TelltaleToolKit.T3Types.Skeletons;

namespace D3DMeshUtilities.Code.D3DMeshFormats;

public class SkinnedModelIntermediate : IMeshRepresentation
{
    
    public MeshInfo Info { get; }
    
    public List<Vector3> VertexPositions;
    public List<Vector3> VertexNormals;
    public List<Vector4>? VertexTangents;

    private int tris = 0;
    
    //unusued/unknown:
    //public List<Vector4>? SecondVertexNormalsList;
    //public List<Vector4>? VertexColorList;

    public List<List<Vector2>?> TextureCoordLists = new(4);
    
    //this is the list of index buffers specified by 
    public List<List<uint>?> IndexBuffers;
    
    public List<MaterialBuilder> Materials;

    public List<T3MeshLOD> LODs;

    //todo: make skeleton actually optional
    public Skeleton? Skeleton;

    public List<Vec4<int>> BlendIndices;
    public List<Vector4> BlendWeights;

    public List<T3MeshBoneEntry> MeshDataBones;
    
    
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
        List<Vec4<int>> blendIndices, 
        List<Vector4> blendWeights, List<T3MeshBoneEntry> bones)
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
        MeshDataBones = bones;
    }


    public static bool Read(D3DMesh mesh, MeshInfo info, string meshFile, Profiler.ProfilerFrame parent, out SkinnedModelIntermediate? readMesh)
    {
        readMesh = null;
        var meshData = mesh.MeshData;
    
        if(AsyncSearchForSkeletonFiles.BuildDictionaryTask != null && !AsyncSearchForSkeletonFiles.BuildDictionaryTask.IsCompleted)
            AsyncSearchForSkeletonFiles.BuildDictionaryTask.GetAwaiter().GetResult(); //todo: what does this do exactly? am I just waiting twice?
    
        AsyncSearchForSkeletonFiles.AgentMeshDictionaryLock.Enter();
        
        PropertySet? agentProp = null;
        
        lock(AsyncSearchForSkeletonFiles.AgentPropertiesByMeshFile)
        {
            
            if (info.Name != null)
            {
                AsyncSearchForSkeletonFiles.AgentPropertiesByMeshFile.TryGetValue(info.Name.Crc64,
                    out (PropertySet set, Symbol name) set);
                agentProp = set.set;

            }

        }
        
        AsyncSearchForSkeletonFiles.AgentMeshDictionaryLock.Exit();


        Handle<Skeleton>? skeletonReference = agentProp?.GetProperty<Handle<Skeleton>>("Skeleton File");

        if (skeletonReference == null)
        {
            return false;
        }

        Skeleton? skeleton = skeletonReference.ObjectInfo.HandleObject as Skeleton;

        if (skeleton == null)
        {
            lock(ResourceLoader.ResourceLock)
            {
                skeleton = TttkInit.Workspace!.LoadAsset<Skeleton>(skeletonReference.ObjectInfo.ObjectName
                    .Crc64);
            }

            if (skeleton == null)
            {
                return false;
            }
        }
    
        List<Vector4>? rawPositionList = MeshUtils.GetVertices(meshData, info, 0);
        List<Vector4>? rawNormalsList = MeshUtils.GetNormals(meshData, 0);
        List<Vector4>? tangentsList = MeshUtils.GetTangents(meshData, 0);
    
        //unusued, I think, but keeping them here because they're on everything
        // List<Vector4>? secondNormalsList = MeshUtils.GetNormals(meshData, 0, 1);
        // List<Vector4>? vertexColorList = MeshUtils.GetVertexColors(meshData, 0);
    
    
        List<Vec4<int>>? vertexBlendIndices = MeshUtils.GetVertexBlendIndices(meshData, 0);
        List<Vector4>? vertexBlendWeights = MeshUtils.GetVertexBlendWeights(meshData, 0);
    
    
        List<Vector2>? vertexTextureCoordList1 = MeshUtils.GetVertexTextureCoords(meshData, 0);
        List<Vector2>? vertexTextureCoordList2 = MeshUtils.GetVertexTextureCoords(meshData, 0, 1);
        List<Vector2>? vertexTextureCoordList3 = MeshUtils.GetVertexTextureCoords(meshData, 0, 2);
        List<Vector2>? vertexTextureCoordList4 = MeshUtils.GetVertexTextureCoords(meshData, 0, 3);

        
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
        
            //we need both bone indices and blend weights for skinned models
            ArgumentNullException.ThrowIfNull(vertexBlendIndices);
            ArgumentNullException.ThrowIfNull(vertexBlendWeights);
        }
        catch(ArgumentNullException e)
        {
            
            Console.Out.WriteLine($"Failed reading {meshFile}! See: " + e);

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
            MeshUtils.ApplyTransforms(meshData, vertexIndex, rawPositionList[vertexIndex], vertexPositions, textureCoords);

            //normalize, because the interpreted 
            var normal = rawNormalsList[vertexIndex].AsVector3();
        
            normals.Add(Vector3.Normalize(normal));
            if(info.HasVertexTangents)
                tangentsList![vertexIndex] = Vector4.Normalize(tangentsList[vertexIndex]);

        }
    
        // List<MaterialBuilder> materials = [];
        Profiler.Instance.BeginFrame("Material processing", parent, out Profiler.ProfilerFrame mProcessingFrame);
        MeshUtils.GetMaterials(mesh, info, mProcessingFrame, out List<MaterialBuilder> materials);
        Profiler.Instance.EndFrame(mProcessingFrame, out TimeSpan materialDuration);
        
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.Out.WriteLine("\tMesh material processing took: " + materialDuration);
        Console.ResetColor();


        foreach (T3MeshBoneEntry bone in meshData.Bones)
        {
            TttkInit.Workspace!.ResolveSymbol(bone.BoneName);
        }
        
        readMesh = new SkinnedModelIntermediate(info, vertexPositions, normals, tangentsList,
            textureCoords, [indexList], materials, meshData.LODs, skeleton, vertexBlendIndices, vertexBlendWeights, meshData.Bones);

        return true;
    }

    //todo: look into exporting LODs as seprate meshBuilders
    public bool SaveToGLTF(out IMeshBuilder<MaterialBuilder> meshBuilder, List<Vec4<int>>? blendIndices = null)
    {
        if(blendIndices == null)
        {
            if (!Info.HasVertexTangents)
                meshBuilder = new MeshBuilder<VertexPositionNormal, VertexTexture1, VertexEmpty>("mainMesh");
            else
                meshBuilder = new MeshBuilder<VertexPositionNormalTangent, VertexTexture1, VertexEmpty>("mainMesh");
        }
        else
        {
            if (!Info.HasVertexTangents)
                meshBuilder = new MeshBuilder<VertexPositionNormal, VertexTexture1, VertexJoints4>(Info.Name?.ToString() + "_main");
            else
                meshBuilder = new MeshBuilder<VertexPositionNormalTangent, VertexTexture1, VertexJoints4>(Info.Name?.ToString() + "_main");
        }
        
        //build list of vertices
        List<IVertexBuilder> verts = new(VertexPositions.Count);

        for (int i = 0; i < VertexPositions.Count; i++)
        {
            if( blendIndices == null)
            {
                AddVertexNoSkinning(i, verts);
                continue;
            }
            
            AddVertexWithSkinning(i, verts, blendIndices);
            
        }
        
        T3MeshLOD lod = LODs[0];

        List<T3MeshBatch> allBatches = [];
        allBatches.AddRange(lod.Batches);
        allBatches.AddRange(lod.Batches1);
        // allBatches.AddRange(lod.Batches2);
            
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

            limit = numIndicesLimit;
            
            for (uint indexI = batch.StartIndex + 2; indexI < limit; indexI += 3)
            {
                int i = (int)indexI;
                    
                int vertIOne = (int)indexList[i - 2];
                int vertITwo = (int)indexList[i - 1];
                int vertIThree = (int)indexList[i];

                var vertOne = verts[vertIOne];
                var vertTwo = verts[vertITwo];
                var vertThree = verts[vertIThree];

                tris += 1;
                    
                meshBuilder.UsePrimitive(mat).AddTriangle(vertOne, vertTwo, vertThree);
            }
        }

        return true;

    }

    private void AddVertexNoSkinning(int vertexIndex, List<IVertexBuilder> vertexList)
    {
        // IVertexBuilder vertex;
        //no skinning data :D
        if (Info.HasVertexTangents)
        {
            var vertex = new VertexBuilder<VertexPositionNormalTangent, VertexTexture1, VertexEmpty>
            {
                Geometry = new VertexPositionNormalTangent(
                    VertexPositions[vertexIndex],
                    VertexNormals[vertexIndex],
                    VertexTangents![vertexIndex]
                ),
                Material = new VertexTexture1(
                    TextureCoordLists[0]![vertexIndex]
                )
            };
                
            vertexList.Add(vertex);
        }
        else
        {
            var vertex = new VertexBuilder<VertexPositionNormal, VertexTexture1, VertexEmpty>
            {
                Geometry = new VertexPositionNormal(
                    VertexPositions[vertexIndex],
                    VertexNormals[vertexIndex]
                ),
                Material = new VertexTexture1(
                    TextureCoordLists[0]![vertexIndex]
                )
            };
            vertexList.Add(vertex);
        }
    }
    
    private void AddVertexWithSkinning(int vertexIndex, List<IVertexBuilder> vertexList, List<Vec4<int>> blendIndices)
    {
        
        // Create skinning data
        VertexJoints4 skinning = default;
        
        Vec4<int> indices = blendIndices[vertexIndex];
        Vector4 weights = BlendWeights[vertexIndex];
        // Normalize weights first
        float sum = weights.X + weights.Y + weights.Z + weights.W;

        if (sum <= 0f || (indices[0] == 0 && indices[1] == 0 && indices[2] == 0 && indices[3] == 0))
        {
            // Case 1: No valid skinning data - bind to root bone (bone 0) with weight 1
            skinning.SetBindings((1, 1.0f), (0, 0), (0, 0), (0, 0));
        }
        else
        {
            // Case 2: Valid skinning data - normalize and use as-is
            if (Math.Abs(sum - 1.0f) > 0.0001f)
            {
                weights = new Vector4(
                    weights.X / sum,
                    weights.Y / sum,
                    weights.Z / sum,
                    weights.W / sum
                );
            }
            
            skinning.SetBindings(
                (indices[0], weights.X),
                (indices[1], weights.Y),
                (indices[2], weights.Z),
                (indices[3], weights.W)
            );
        }
        
        // sum = weights.X + weights.Y + weights.Z + weights.W;
        //     
        // Console.WriteLine($"Sum: {sum}");
        //
        // Console.WriteLine($"VtxSum: {skinning.Weights.X + skinning.Weights.Y + skinning.Weights.Z + skinning.Weights.W}");
        //
        
        if (Info.HasVertexTangents)
        {
            var vertex = new VertexBuilder<VertexPositionNormalTangent, VertexTexture1, VertexJoints4>
            {
                Geometry = new VertexPositionNormalTangent(
                    VertexPositions[vertexIndex],
                    VertexNormals[vertexIndex],
                    VertexTangents![vertexIndex]
                ),
                Material = new VertexTexture1(
                    TextureCoordLists[0]![vertexIndex]
                ),
                Skinning = skinning
            };
                
            vertexList.Add(vertex);
        }
        else
        {
            var vertex = new VertexBuilder<VertexPositionNormal, VertexTexture1, VertexJoints4>
            {
                Geometry = new VertexPositionNormal(
                    VertexPositions[vertexIndex],
                    VertexNormals[vertexIndex]
                ),
                Material = new VertexTexture1(
                    TextureCoordLists[0]![vertexIndex]
                ),
                Skinning = skinning
            };
            vertexList.Add(vertex);
        }
    }

    public MeshInfo GetMeshInfo()
    {
        return Info;
    }

    public bool SaveToMeshBuilder(out IMeshBuilder<MaterialBuilder> meshBuilder)
    {
        bool succeeded = SaveToGLTF(out meshBuilder);

        return succeeded;
    }
    
    public bool SaveToScene(
        SceneBuilder scene, 
        NodeBuilder modelRoot
        )
    {
        bool builtSkeleton = 
            BuildSkeletonStructure(Skeleton, modelRoot, out List<NodeBuilder> jointNodeBuilders, out Dictionary<string, int> boneNameToJointIndex);


        if (builtSkeleton && jointNodeBuilders.Count > 0 && boneNameToJointIndex.Count > 0)
        {
            return SaveSkinnedModelToScene(scene, modelRoot, jointNodeBuilders, boneNameToJointIndex);
        }
        
        if (!SaveToGLTF(out IMeshBuilder<MaterialBuilder> nonSkinnedMesh))
        {
            return false;
        }
        
        scene.AddRigidMesh(nonSkinnedMesh, modelRoot);
        return true;
        
    }

    public bool SaveSkinnedModelToScene(SceneBuilder scene, NodeBuilder modelRoot, List<NodeBuilder> jointNodeBuilders,
        Dictionary<string, int> boneNameToJointIndex)
    {
        for (int i = 0; i < jointNodeBuilders.Count; i++)
        {
            // Assuming jointNodeBuilders[i].Name is the bone name (or you can extract it)
            string boneName = jointNodeBuilders[i].Name;
            boneNameToJointIndex[boneName] = i;
        }

        List<Vec4<int>> blendIndices = BlendIndices;
        
        List<T3MeshBoneEntry> d3dMeshBones = MeshDataBones;
        for (var i = 0; i < blendIndices.Count; i++)
        {
            var index1 = blendIndices[i][0];
            var index2 = blendIndices[i][1];
            var index3 = blendIndices[i][2];
            var index4 = blendIndices[i][3];

            T3MeshBoneEntry bone1 = d3dMeshBones[index1];
            T3MeshBoneEntry bone2 = d3dMeshBones[index2];
            T3MeshBoneEntry bone3 = d3dMeshBones[index3];
            T3MeshBoneEntry bone4 = d3dMeshBones[index4];

            int remappedIndex1 = RemapBoneIndex(bone1, boneNameToJointIndex);
            int remappedIndex2 = RemapBoneIndex(bone2, boneNameToJointIndex);
            int remappedIndex3 = RemapBoneIndex(bone3, boneNameToJointIndex);
            int remappedIndex4 = RemapBoneIndex(bone4, boneNameToJointIndex);

            blendIndices[i] = new Vec4<int>([remappedIndex1, remappedIndex2, remappedIndex3, remappedIndex4]);
        }

        if (!SaveToGLTF(out IMeshBuilder<MaterialBuilder> skinnedMesh, blendIndices))
        {
            return false;
        }

        // scene.AddRigidMesh(skinnedMesh, modelRoot);
        scene.AddSkinnedMesh(skinnedMesh, modelRoot.WorldMatrix, jointNodeBuilders.ToArray());
        return true;
    }

    public static bool BuildSkeletonStructure(
        Skeleton? skeleton, 
        NodeBuilder modelRoot,
        out List<NodeBuilder> jointNodeBuilders,
        out Dictionary<string, int> boneNameToJointIndex)
    {
        jointNodeBuilders = [];
        boneNameToJointIndex = [];
        
        int camera = 0;
        if (skeleton == null) return false;
        
        // First create all nodes
        foreach (Skeleton.Entry bone in skeleton.Entries)
        {
            TttkInit.Workspace!.ResolveSymbol(bone.JointName);
                
            var nodeBuilder = new NodeBuilder(bone.JointName.ToString());

            // Set local transform
            nodeBuilder.WithLocalTranslation(new Vector3(bone.LocalPosition.X, bone.LocalPosition.Y,
                bone.LocalPosition.Z));

            var originalRotation =
                new Quaternion(bone.LocalQuat.X, bone.LocalQuat.Y, bone.LocalQuat.Z, bone.LocalQuat.W);

            nodeBuilder.WithLocalRotation(originalRotation);
            nodeBuilder.WithLocalScale(Vector3.One);
                
            // scene.AddRigidMesh(D3DMeshManager.BoneMesh, nodeBuilder);

            if (bone.ParentIndex == -1 && bone.JointName.ToString() != "Root")
            {
                modelRoot.AddNode(nodeBuilder);
                camera++;
            }
            else
            {
                jointNodeBuilders.Add(nodeBuilder);
            }
        }

        // Then build hierarchy
        for (var i = 0; i < jointNodeBuilders.Count; i++)
        {
            Skeleton.Entry bone = skeleton.Entries[i + camera];
            if (bone.ParentIndex >= 0 && bone.ParentIndex < jointNodeBuilders.Count)
            {
                jointNodeBuilders[bone.ParentIndex - camera].AddNode(jointNodeBuilders[i]);
            }
            else if (bone.JointName.ToString() is "Root")
            {
                modelRoot.AddNode(jointNodeBuilders[i]);
            }
        }

        return true;
    }

    public bool OldSaveToScene(SceneBuilder scene, NodeBuilder modelRoot)
    {
        List<NodeBuilder> jointNodeBuilders = [];
        
        Skeleton? skeleton = Skeleton;

        
        int camera = 0;
        if (skeleton != null)
        {
            // First create all nodes
            foreach (Skeleton.Entry bone in skeleton.Entries)
            {
                TttkInit.Workspace!.ResolveSymbol(bone.JointName);
                
                var nodeBuilder = new NodeBuilder(bone.JointName.ToString());

                // Set local transform
                nodeBuilder.WithLocalTranslation(new Vector3(bone.LocalPosition.X, bone.LocalPosition.Y,
                    bone.LocalPosition.Z));

                var originalRotation =
                    new Quaternion(bone.LocalQuat.X, bone.LocalQuat.Y, bone.LocalQuat.Z, bone.LocalQuat.W);

                nodeBuilder.WithLocalRotation(originalRotation);
                nodeBuilder.WithLocalScale(Vector3.One);
                
                // scene.AddRigidMesh(D3DMeshManager.BoneMesh, nodeBuilder);

                if (bone.ParentIndex == -1 && bone.JointName.ToString() != "Root")
                {
                    modelRoot.AddNode(nodeBuilder);
                    camera++;
                }
                else
                {
                    jointNodeBuilders.Add(nodeBuilder);
                }
            }

            // Then build hierarchy
            for (var i = 0; i < jointNodeBuilders.Count; i++)
            {
                Skeleton.Entry bone = skeleton.Entries[i + camera];
                if (bone.ParentIndex >= 0 && bone.ParentIndex < jointNodeBuilders.Count)
                {
                    jointNodeBuilders[bone.ParentIndex - camera].AddNode(jointNodeBuilders[i]);
                }
                else if (bone.JointName.ToString() is "Root")
                {
                    modelRoot.AddNode(jointNodeBuilders[i]);
                }
            }
            
            Dictionary<string, int> boneNameToJointIndex = [];
            for (int i = 0; i < jointNodeBuilders.Count; i++)
            {
                // Assuming jointNodeBuilders[i].Name is the bone name (or you can extract it)
                string boneName = jointNodeBuilders[i].Name;
                boneNameToJointIndex[boneName] = i;
            }

            List<Vec4<int>> blendIndices = BlendIndices;
        
            List<T3MeshBoneEntry> d3dMeshBones = MeshDataBones;
            for (var i = 0; i < blendIndices.Count; i++)
            {
                var index1 = blendIndices[i][0];
                var index2 = blendIndices[i][1];
                var index3 = blendIndices[i][2];
                var index4 = blendIndices[i][3];

                T3MeshBoneEntry bone1 = d3dMeshBones[index1];
                T3MeshBoneEntry bone2 = d3dMeshBones[index2];
                T3MeshBoneEntry bone3 = d3dMeshBones[index3];
                T3MeshBoneEntry bone4 = d3dMeshBones[index4];

                int remappedIndex1 = RemapBoneIndex(bone1, boneNameToJointIndex);
                int remappedIndex2 = RemapBoneIndex(bone2, boneNameToJointIndex);
                int remappedIndex3 = RemapBoneIndex(bone3, boneNameToJointIndex);
                int remappedIndex4 = RemapBoneIndex(bone4, boneNameToJointIndex);

                blendIndices[i] = new Vec4<int>([remappedIndex1, remappedIndex2, remappedIndex3, remappedIndex4]);
            }

            if (!SaveToGLTF(out IMeshBuilder<MaterialBuilder> skinnedMesh, blendIndices))
            {
                return false;
            }

            // scene.AddRigidMesh(skinnedMesh, modelRoot);
            scene.AddSkinnedMesh(skinnedMesh, modelRoot.WorldMatrix, jointNodeBuilders.ToArray());
            return true;

        }
        
        if (!SaveToGLTF(out IMeshBuilder<MaterialBuilder> nonSkinnedMesh))
        {
            return false;
        }
        
        scene.AddRigidMesh(nonSkinnedMesh, modelRoot);
        return true;
        
    }
    
    
    public static int RemapBoneIndex(T3MeshBoneEntry meshBone,
        Dictionary<string, int> boneNameToJointIndex)
    {
        var boneName = meshBone.BoneName.ToString(); // Use whatever property has the bone name

        if (string.IsNullOrEmpty(boneName))
        {
            // Console.WriteLine(d3dmesh.Name);
            throw new Exception($"Invalid {meshBone.BoneName.Crc64} bone name");
        }

        if (boneNameToJointIndex.TryGetValue(boneName, out int jointIndex))
        {
            return jointIndex;
        }

        throw new Exception($"Bone {boneName} not found!");
    }

    public bool SaveToD3DMesh(out D3DMesh? mesh) => throw new NotImplementedException();
}