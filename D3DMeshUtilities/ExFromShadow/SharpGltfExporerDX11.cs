using System.IO;
using System.Numerics;
using System.Text.Json;
using SharpGLTF.Animations;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Memory;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;
using SharpGLTF.Transforms;
using TelltaleTextureTool;
using TelltaleTextureTool.Graphics;
using TelltaleToolKit;
using TelltaleToolKit.Serialization.Binary;
using TelltaleToolKit.T3Types;
using TelltaleToolKit.T3Types.Animations;
using TelltaleToolKit.T3Types.Chores;
using TelltaleToolKit.T3Types.Dialogs.Dlg;
using TelltaleToolKit.T3Types.Meshes;
using TelltaleToolKit.T3Types.Meshes.T3Types;
using TelltaleToolKit.T3Types.Properties;
using TelltaleToolKit.T3Types.Skeletons;
using TelltaleToolKit.T3Types.Textures;
using TelltaleToolKit.T3Types.Textures.T3Types;
using TelltaleToolKit.TelltaleArchives;
using AlphaMode = SharpGLTF.Materials.AlphaMode;
using Animation = SharpGLTF.Schema2.Animation;
using TextureType = TelltaleTextureTool.Graphics.TextureType;
using Toolkit = TelltaleToolKit.Toolkit;

namespace D3DMeshExperimentalExporer;

public static class SharpGltfExporterDX11
{
    public static Dictionary<ulong, (ArchiveInfo Archive, TelltaleFileEntry Entry)> index;

    // ProcessMeshByName now uses SharpGLTF instead of Assimp

    public static bool ProcessMeshByName(SceneBuilder scene, string agentName, PropertySet propertySet,
        IReadOnlyList<ArchiveInfo> archives,
        string dataFolderPath, Matrix4x4 worldTransform, NodeBuilder? model = null)
    {
        // if (!ArchiveManager.TryFindEntryByName(archives, sceneFile, out ArchiveInfo sceneArchive,
        //         out TelltaleFileEntry _))
        // {
        //     Console.WriteLine($"Mesh '{propFile}' not found in any opened archive.");
        //     return false;
        // }

        // using MemoryStream? choreStream =  ArchiveManager.TryExtractWithIndex(index, new Symbol("sk56_clementineSitSwing_swing.chore").Crc64, out _, out _); 
        // if (choreStream is not null)
        // {
        //     var chore = TTK.Load<Chore>(choreStream, out MetaStreamConfiguration choreConfiguration);
        //     TTKGlobalContext.Instance().ResolveSymbols(choreConfiguration.SerializedSymbols);
        //     Console.WriteLine(chore.Name);
        // }

        // using MemoryStream? dlgStream =  ArchiveManager.TryExtractWithIndex(index, new Symbol("env_dairyExterior_atTheDairy.dlog").Crc64, out _, out _); 
        // if (dlgStream is not null)
        // {
        //     var dlg = TTK.Load<Dlg>(dlgStream, out MetaStreamConfiguration dlgConfiguration);
        //     TTKGlobalContext.Instance().ResolveSymbols(dlgConfiguration.SerializedSymbols);
        //     Console.WriteLine(dlg.Name);
        // }

        // Skeleton
        Skeleton? skeleton = null;
        var handleSkeleton = propertySet.GetProperty<Handle<Skeleton>>("Skeleton File");
        if (handleSkeleton is not null)
        {
            MemoryStream? skeletonStream =
                ArchiveManager.TryExtractWithIndex(index, handleSkeleton.ObjectInfo.ObjectName.Crc64, out _, out _);
            skeleton = Toolkit.Instance.LoadObject<Skeleton>(skeletonStream, out MetaStreamConfiguration sklConfig);
            Toolkit.Instance.ResolveSymbols(sklConfig.SerializedSymbols);
            Console.WriteLine($"Found skeleton {handleSkeleton.ObjectInfo.ObjectName} for {agentName}!");
        }

        // Meshes
        List<D3DMesh> meshes = [];
        List<Handle<D3DMesh>>? handleMeshes = propertySet.GetD3DMeshList();

        if (handleMeshes is not null)
        {
            foreach (Handle<D3DMesh> meshHandle in handleMeshes)
            {
                MemoryStream? meshStream =
                    ArchiveManager.TryExtractWithIndex(index, meshHandle.ObjectInfo.ObjectName.Crc64, out _, out _);
                if (meshStream is null)
                    continue;

                var mesh =Toolkit.Instance.LoadObject<D3DMesh>(meshStream, out MetaStreamConfiguration meshConfig);
                Toolkit.Instance.ResolveSymbols(meshConfig.SerializedSymbols);
                meshes.Add(mesh);
                //     string json1 = JsonSerializer.Serialize(mesh);
                //     if (!Path.Exists(Path.Combine(dataFolderPath, mesh.Name + ".json")))
                //     {
                //         File.WriteAllText(Path.Combine(dataFolderPath, mesh.Name + ".json"), json1);
                //     }
            }
        }

        if (meshes.Count == 0)
        {
            return false;
        }

        if (model == null)
        {
            model = new NodeBuilder(agentName);
            scene.AddNode(model);
        }

        ExportD3DMeshToSharpGltf(scene, model, meshes, skeleton, worldTransform);
        return true;
    }


    // Main conversion function that returns a ModelRoot ready to be saved
    private static void ExportD3DMeshToSharpGltf(SceneBuilder scene, NodeBuilder model, List<D3DMesh> d3dMeshes,
        Skeleton? skeleton, Matrix4x4 worldTransform)
    {
        // Setup Materials
        // === Materials ===
        // For now create simple PBR materials with base color texture if present
        List<MaterialBuilder> materialBuilders = CreateMaterials(d3dMeshes);

        // === Skeleton ===

        List<NodeBuilder> jointNodeBuilders = [];

        int camera = 0;
        if (skeleton != null)
        {
            // First create all nodes
            foreach (Skeleton.Entry bone in skeleton.Entries)
            {
                var nodeBuilder = new NodeBuilder(bone.JointName.ToString());

                // Set local transform
                nodeBuilder.WithLocalTranslation(new Vector3(bone.LocalPosition.X, bone.LocalPosition.Y,
                    bone.LocalPosition.Z));

                var originalRotation =
                    new Quaternion(bone.LocalQuat.X, bone.LocalQuat.Y, bone.LocalQuat.Z, bone.LocalQuat.W);

                nodeBuilder.WithLocalRotation(originalRotation);
                nodeBuilder.WithLocalScale(Vector3.One);

                if (bone.ParentIndex == -1 && bone.JointName.ToString() != "Root")
                {
                    model.AddNode(nodeBuilder);
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
                    model.AddNode(jointNodeBuilders[i]);
                }
            }
        }

        // === Meshes ===
        // We're going to create a MeshBuilder for each submesh (batch). We'll add positions/normals/texcoords and later set JOINTS_0/WEIGHTS_0.

        // We will map per-base-data to arrays and share if needed.
        // For simplicity here we create per-submesh MeshBuilder and fill vertices from your DecompressorExtensions results.
        //   var meshScene = new SceneBuilder(d3dMesh.Name);
        int materialBaseIndex = 0;
        foreach (D3DMesh d3dMesh in d3dMeshes)
        {
            Console.WriteLine($"Extracting {d3dMesh.Name}");
            for (var lodIndex = 0; lodIndex < 1; lodIndex++)
            {
                T3MeshLOD lod = d3dMesh.MeshData.LODs[lodIndex];
                // create a LOD node under meshNode

                var batchNum = 0;

                var meshBuilder =
                    new MeshBuilder<VertexPositionNormalTangent, VertexTexture4, VertexJoints4>(d3dMesh.Name);
                foreach (T3MeshBatch batch in lod.Batches1)
                {
                    // meshBuilder.TransformVertices();
                    // "Submesh_" + batchNum
                    AddBatchAsPrimitive(meshBuilder, jointNodeBuilders, d3dMesh, batch, materialBaseIndex,
                        materialBuilders, false, camera);

                    batchNum++;
                }

                meshBuilder.Validate();
                if (skeleton != null)
                {
                    scene.AddSkinnedMesh(meshBuilder, worldTransform, jointNodeBuilders.ToArray());
                }
                else
                {
                    scene.AddRigidMesh(meshBuilder, model);
                }

                if (skeleton != null)
                {
                    foreach (T3MeshBatch batch in lod.Batches2)
                    {
                        // var meshBuilder =
                      	//     new MeshBuilder<VertexPositionNormalTangent, VertexTexture4, VertexJoints4>("Submesh_Shadow_" +
                        //         batchNum);
                        
                        AddBatchAsPrimitive(meshBuilder, d3dMesh, batch, materialBuilders, true);
                        batchNum++;
                        
                        // Rigid mesh - no skeleton
                        scene.AddRigidMesh(meshBuilder, Matrix4x4.Identity);
                    }
                }

                // The rest of batches1 / batches2 can be handled the same way as above...
                break; // preserved from original (your code returns after first LOD)
            }

            materialBaseIndex += d3dMesh.MeshData.Materials.Count;
        }
    }

    private static void ProcessTexture(D3DMesh d3dmesh, MaterialBuilder builder, Handle<T3Texture>? textureHandle,
        KnownChannel knownChannel)
    {
        if (textureHandle == null)
        {
            // Console.WriteLine($"Texture handle is null.");
            return;
        }

        MemoryStream? texStream =
            ArchiveManager.TryExtractWithIndex(index, textureHandle.ObjectInfo.ObjectName.Crc64, out _, out _);

        if (texStream == null)
        {
            // Console.WriteLine($"Failed to extract texture from crc64 '{textureHandle.ObjectInfo.ObjectName.Crc64}'.");
            return;
        }

        var d3dtxTexture =Toolkit.Instance.LoadObject<T3Texture>(texStream, out MetaStreamConfiguration configuration);

        ChannelBuilder? currentChannel = builder.UseChannel(knownChannel);

        currentChannel.UseTexture().WrapS = d3dtxTexture.SamplerStateBlock.WrapU switch
        {
            T3SamplerStateBlock.TextureWrapMode.Clamp => TextureWrapMode.CLAMP_TO_EDGE,
            T3SamplerStateBlock.TextureWrapMode.Wrap => TextureWrapMode.REPEAT,
            _ => TextureWrapMode.CLAMP_TO_EDGE
        };

        currentChannel.UseTexture().WrapT = d3dtxTexture.SamplerStateBlock.WrapV switch
        {
            T3SamplerStateBlock.TextureWrapMode.Clamp => TextureWrapMode.CLAMP_TO_EDGE,
            T3SamplerStateBlock.TextureWrapMode.Wrap => TextureWrapMode.REPEAT,
            _ => TextureWrapMode.CLAMP_TO_EDGE
        };

        currentChannel.UseTexture().MagFilter = d3dtxTexture.SamplerStateBlock.Filtered
            ? TextureInterpolationFilter.LINEAR
            : TextureInterpolationFilter.NEAREST;

        switch (knownChannel)
        {
            case KnownChannel.BaseColor:
                builder.WithAlpha(d3dtxTexture.SurfaceFormat.HasAlpha() ? AlphaMode.BLEND : AlphaMode.OPAQUE);
                if (d3dtxTexture.AlphaModeEnum is TelltaleToolKit.T3Types.Textures.AlphaMode.NoAlpha)
                {
                    builder.WithAlpha();
                }

                if (d3dtxTexture.SurfaceFormat.HasAlpha() &&
                    d3dtxTexture.AlphaModeEnum is TelltaleToolKit.T3Types.Textures.AlphaMode.Unknown)
                {
                    builder.WithAlpha(AlphaMode.MASK);
                }

                builder.Name = Path.GetFileNameWithoutExtension(d3dtxTexture.Name);
                currentChannel.Texture.CoordinateSet = 0;

                if (d3dmesh.MeshData.TexCoordTransform?[0] != null)
                {
                    T3MeshTexCoordTransform tex = d3dmesh.MeshData.TexCoordTransform[0];
                    currentChannel.Texture.WithTransform(new Vector2(tex.Offset.X, tex.Offset.Y),
                        new Vector2(tex.Scale.X, tex.Scale.Y));
                }

                break;
            case KnownChannel.Normal:
                currentChannel.Texture.CoordinateSet = 1;
                if (d3dmesh.MeshData.TexCoordTransform?[1] != null)
                {
                    T3MeshTexCoordTransform tex = d3dmesh.MeshData.TexCoordTransform[1];
                    currentChannel.Texture.WithTransform(new Vector2(tex.Offset.X, tex.Offset.Y),
                        new Vector2(tex.Scale.X, tex.Scale.Y));
                }

                break;
        }

        D3DTXConverter.ConvertD3DTXToImage(d3dtxTexture,
            @"C:\Users\ivani\Downloads\TelltaleLib.NET\samples\D3DMeshExperimentalExporer\bin\Debug\net9.0",
            TextureType.PNG,
            knownChannel == KnownChannel.Normal, knownChannel == KnownChannel.SpecularColor, false);

        builder.WithChannelImage(knownChannel, Path.GetFileNameWithoutExtension(d3dtxTexture.Name) + ".png");
        // Console.WriteLine($"Successfully extracted {d3dtxTexture.Name}");
    }

    private static List<MaterialBuilder> CreateMaterials(List<D3DMesh> d3dMeshes)
    {
        var materialBuilders = new List<MaterialBuilder>();

        foreach (D3DMesh d3dMesh in d3dMeshes)
        {
            materialBuilders.AddRange(CreateMaterials(d3dMesh));
        }

        return materialBuilders;
    }

    private static List<MaterialBuilder> CreateMaterials(D3DMesh d3dMesh)
    {
        var materialBuilders = new List<MaterialBuilder>();

        foreach (T3MeshMaterial mat in d3dMesh.MeshData.Materials)
        {
            var materialName = mat.Material.ObjectInfo.ObjectName.Crc64.ToString();
            MaterialBuilder? mb = new MaterialBuilder(materialName)
                .WithDoubleSide(false);

            // Set up material properties
            ProcessTexture(d3dMesh, mb, d3dMesh.GetDiffuseTexture(mat.Material), KnownChannel.BaseColor);
            ProcessTexture(d3dMesh, mb, d3dMesh.GetNormalMapTexture(mat.Material), KnownChannel.Normal);
            ProcessTexture(d3dMesh, mb, d3dMesh.GetSpecularTexture(mat.Material), KnownChannel.SpecularColor);

            // Set default metallic roughness if no texture
            if (mb.GetChannel(KnownChannel.MetallicRoughness)?.Texture == null)
            {
                // mb.WithChannelParam(KnownChannel.MetallicRoughness, KnownProperty.RoughnessFactor, 0.5f);
            }

            materialBuilders.Add(mb);
        }

        // Add a default material if no materials were created
        if (materialBuilders.Count == 0)
        {
            materialBuilders.Add(new MaterialBuilder("Default")
                .WithDoubleSide(false)
                .WithMetallicRoughnessShader()
                .WithChannelParam(KnownChannel.BaseColor, KnownProperty.RGBA, new Vector4(0.8f, 0.8f, 0.8f, 1.0f)));
        }

        return materialBuilders;
    }

    private static int RemapBoneIndex(D3DMesh d3dmesh, T3MeshBoneEntry meshBone,
        Dictionary<string, int> boneNameToJointIndex)
    {
        var boneName = meshBone.BoneName.ToString(); // Use whatever property has the bone name

        if (string.IsNullOrEmpty(boneName))
        {
            Console.WriteLine(d3dmesh.Name);
            throw new Exception($"Invalid {meshBone.BoneName.Crc64} bone name");
        }

        if (boneNameToJointIndex.TryGetValue(boneName, out int jointIndex))
        {
            return jointIndex;
        }

        throw new Exception($"Bone {boneName} not found!");
    }

    private static void AddBatchAsPrimitive(
        MeshBuilder<VertexPositionNormalTangent, VertexTexture4, VertexJoints4> meshBuilder,
        List<NodeBuilder> jointBuilders,
        D3DMesh d3dMesh,
        T3MeshBatch batch,
        int materialBaseIndex,
        List<MaterialBuilder> materialBuilders, bool isShadow = false, int skipCamera = 0)
    {
        uint batchVertexCount = batch.MaxVertIndex - batch.MinVertIndex + 1;

        // Get vertex data
        List<Vector3> positions = GetVertexPositions(d3dMesh, batch, batchVertexCount);
        List<Vector3> normals = GetVertexNormals(d3dMesh, batch, batchVertexCount);
        List<Vector4> tangents = GetVertexTangents(d3dMesh, batch, batchVertexCount);
        List<Vector2> uvs1 = GetVertexUVs(d3dMesh, batch, batchVertexCount, 0);
        List<Vector2> uvs2 = GetVertexUVs(d3dMesh, batch, batchVertexCount, 1);
        List<Vector2> uvs3 = GetVertexUVs(d3dMesh, batch, batchVertexCount, 2);
        List<Vector2> uvs4 = GetVertexUVs(d3dMesh, batch, batchVertexCount, 3);

        (List<Vector4> blendIndices, List<Vector4> blendWeights) = GetVertexWeights(d3dMesh, batch, batchVertexCount);
        bool hasSkinning = blendIndices.Count > 0 && blendWeights.Count > 0;

        if (hasSkinning)
        {
            Dictionary<string, int> boneNameToJointIndex = [];
            for (int i = 0; i < jointBuilders.Count; i++)
            {
                // Assuming jointBuilders[i].Name is the bone name (or you can extract it)
                string boneName = jointBuilders[i].Name;
                boneNameToJointIndex[boneName] = i;
            }

            List<T3MeshBoneEntry> d3dMeshBones = d3dMesh.MeshData.Bones;
            for (var i = 0; i < blendIndices.Count; i++)
            {
                var index1 = (int)blendIndices[i].X;
                var index2 = (int)blendIndices[i].Y;
                var index3 = (int)blendIndices[i].Z;
                var index4 = (int)blendIndices[i].W;

                T3MeshBoneEntry bone1 = d3dMeshBones[index1];
                T3MeshBoneEntry bone2 = d3dMeshBones[index2];
                T3MeshBoneEntry bone3 = d3dMeshBones[index3];
                T3MeshBoneEntry bone4 = d3dMeshBones[index4];

                int remappedIndex1 = RemapBoneIndex(d3dMesh, bone1, boneNameToJointIndex);
                int remappedIndex2 = RemapBoneIndex(d3dMesh, bone2, boneNameToJointIndex);
                int remappedIndex3 = RemapBoneIndex(d3dMesh, bone3, boneNameToJointIndex);
                int remappedIndex4 = RemapBoneIndex(d3dMesh, bone4, boneNameToJointIndex);

                blendIndices[i] = new Vector4(remappedIndex1, remappedIndex2, remappedIndex3, remappedIndex4);
            }
        }

        // Select material for this batch
        int materialIndex = Math.Clamp(materialBaseIndex + batch.MaterialIndex, 0, materialBuilders.Count - 1);
        MaterialBuilder material = materialBuilders[materialIndex];

        // Create primitive for this batch
        PrimitiveBuilder<MaterialBuilder, VertexPositionNormalTangent, VertexTexture4, VertexJoints4>? prim =
            meshBuilder.UsePrimitive(material);

        T3GFXBuffer? indexBuffer = d3dMesh.GetIndexBuffer(isShadow);
        if (indexBuffer == null)
            return;

        var indices = new List<uint>((int)batch.NumPrimitives);

        for (var faceIndex = 0; faceIndex < batch.NumPrimitives; faceIndex++)
        {
            indices.Add(ReadIndexValueSimple(indexBuffer, (uint)(batch.StartIndex + faceIndex * 3 + 0)));
            indices.Add(ReadIndexValueSimple(indexBuffer, (uint)(batch.StartIndex + faceIndex * 3 + 1)));
            indices.Add(ReadIndexValueSimple(indexBuffer, (uint)(batch.StartIndex + faceIndex * 3 + 2)));
        }

        // Add triangles using the indices
        for (var i = 0; i < indices.Count; i += 3)
        {
            if (i + 2 >= indices.Count)
                continue;
            // Get the original indices from the batch
            uint idx0 = indices[i];
            uint idx1 = indices[i + 1];
            uint idx2 = indices[i + 2];

            // FIX: Reverse winding order for glTF coordinate system
            // GltfCoordinateConverter.FixTriangleWinding(ref idx0, ref idx1, ref idx2);

            // Check if indices are within our batch range
            if (idx0 >= batch.MinVertIndex && idx0 <= batch.MaxVertIndex &&
                idx1 >= batch.MinVertIndex && idx1 <= batch.MaxVertIndex &&
                idx2 >= batch.MinVertIndex && idx2 <= batch.MaxVertIndex)
            {
                // Convert to local vertex indices within this batch
                var localIdx0 = (int)(idx0 - batch.MinVertIndex);
                var localIdx1 = (int)(idx1 - batch.MinVertIndex);
                var localIdx2 = (int)(idx2 - batch.MinVertIndex);

                // Get vertex data for each index
                (VertexPositionNormalTangent geometry, VertexTexture4 texture, VertexJoints4 skinning) vertex0 =
                    CreateVertex(localIdx0, positions, normals, tangents, uvs1, uvs2, uvs3, uvs4,
                        blendIndices, blendWeights, hasSkinning);
                (VertexPositionNormalTangent geometry, VertexTexture4 texture, VertexJoints4 skinning) vertex1 =
                    CreateVertex(localIdx1, positions, normals, tangents, uvs1, uvs2, uvs3, uvs4,
                        blendIndices, blendWeights, hasSkinning);
                (VertexPositionNormalTangent geometry, VertexTexture4 texture, VertexJoints4 skinning) vertex2 =
                    CreateVertex(localIdx2, positions, normals, tangents, uvs1, uvs2, uvs3, uvs4,
                        blendIndices, blendWeights, hasSkinning);

                // Add the triangle with full vertex data
                prim.AddTriangle(vertex0, vertex1, vertex2);
            }
        }
    }

    private static (VertexPositionNormalTangent geometry, VertexTexture4 texture, VertexJoints4 skinning)
        CreateVertex(int index, List<Vector3> positions, List<Vector3> normals, List<Vector4> tangents,
            List<Vector2> uvs1, List<Vector2> uvs2, List<Vector2> uvs3, List<Vector2> uvs4, List<Vector4> blendIndices,
            List<Vector4> blendWeights, bool hasSkinning = true)
    {
        Vector3 pos = index < positions.Count ? positions[index] : Vector3.Zero;
        Vector3 nor = index < normals.Count ? normals[index] : Vector3.UnitZ;
        Vector4 tan = index < tangents.Count ? tangents[index] : new Vector4(1, 0, 0, 1);
        Vector2 uv1 = index < uvs1.Count ? uvs1[index] : Vector2.Zero;
        Vector2 uv2 = index < uvs2.Count ? uvs2[index] : Vector2.Zero;
        Vector2 uv3 = index < uvs3.Count ? uvs3[index] : Vector2.Zero;
        Vector2 uv4 = index < uvs4.Count ? uvs4[index] : Vector2.Zero;

        // Create skinning data
        VertexJoints4 skinning = default;
        if (!hasSkinning)
        {
            // Non-skinned mesh: bind all vertices rigidly to bone 0
            skinning.SetBindings((0, 1.0f), (0, 0), (0, 0), (0, 0));
        }
        else
        {
            Vector4 indices = blendIndices[index];
            Vector4 weights = blendWeights[index];
            // Normalize weights first
            float sum = weights.X + weights.Y + weights.Z + weights.W;

            if (sum <= 0f || indices is { X: 0, Y: 0, Z: 0, W: 0 })
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
                    ((int)indices.X, weights.X),
                    ((int)indices.Y, weights.Y),
                    ((int)indices.Z, weights.Z),
                    ((int)indices.W, weights.W)
                );
            }
        }

        var geometry = new VertexPositionNormalTangent(pos, nor, tan);
        var texture = new VertexTexture4(uv1, uv2, uv3, uv4);

        return (geometry, texture, skinning);
    }

    private static List<Vector3> GetVertexPositions(D3DMesh d3dMesh, T3MeshBatch batch, uint batchVertexCount)
    {
        (T3GFXBuffer? buffer, GFXPlatformAttributeParams? accessor) vertexBuffer =
            d3dMesh.GetBuffer(GFXPlatformVertexAttribute.Position, 0);

        if (vertexBuffer.buffer == null) return [];

        byte[] vertexRange = vertexBuffer.buffer.GetBufferRange(batch.MinVertIndex + batch.BaseIndex, batchVertexCount);
        List<Vector4> positions =
            DecompressorExtensions.DecompressToColor4D(vertexRange, vertexBuffer.buffer.Stride, vertexBuffer.accessor);

        if (d3dMesh.MeshData.PositionScale.X != 0 && d3dMesh.MeshData.PositionScale.Y != 0 &&
            d3dMesh.MeshData.PositionScale.Z != 0)
        {
            positions = positions.Select(v => new Vector4(
                v.X * d3dMesh.MeshData.PositionScale.X,
                v.Y * d3dMesh.MeshData.PositionScale.Y,
                v.Z * d3dMesh.MeshData.PositionScale.Z,
                v.W)).ToList();
        }

        List<Vector3> finalPositions = positions.Select(v => new Vector3(
            v.X + d3dMesh.MeshData.PositionOffset.X + v.W * d3dMesh.MeshData.PositionWScale.X,
            v.Y + d3dMesh.MeshData.PositionOffset.Y + v.W * d3dMesh.MeshData.PositionWScale.Y,
            v.Z + d3dMesh.MeshData.PositionOffset.Z + v.W * d3dMesh.MeshData.PositionWScale.Z)).ToList();

        // FIX: Convert positions to glTF coordinate system
        finalPositions = finalPositions.Select(GltfCoordinateConverter.ConvertPosition).ToList();

        return finalPositions;
    }

    private static List<Vector3> GetVertexNormals(D3DMesh d3dMesh, T3MeshBatch batch, uint batchVertexCount)
    {
        (T3GFXBuffer? buffer, GFXPlatformAttributeParams? accessor) normalBuffer =
            d3dMesh.GetBuffer(GFXPlatformVertexAttribute.Normal, 0);

        if (normalBuffer.buffer == null) return [];

        byte[] normalRange = normalBuffer.buffer.GetBufferRange(batch.MinVertIndex + batch.BaseIndex, batchVertexCount);

        List<Vector3> normals = DecompressorExtensions.DecompressToVector3D(normalRange, normalBuffer.buffer.Stride,
            normalBuffer.accessor);

        // Normalize and validate each normal
        for (int i = 0; i < normals.Count; i++)
        {
            Vector3 normal = normals[i];

            // Check for invalid values
            if (float.IsNaN(normal.X) || float.IsNaN(normal.Y) || float.IsNaN(normal.Z) ||
                float.IsInfinity(normal.X) || float.IsInfinity(normal.Y) || float.IsInfinity(normal.Z))
            {
                normals[i] = Vector3.UnitZ;
                continue;
            }

            // Normalize the vector
            float length = normal.Length();
            if (length < 0.0001f)
            {
                normals[i] = Vector3.UnitZ;
            }
            else
            {
                normals[i] = Vector3.Normalize(normal);
            }

            // Ensure each component is within valid range
            Vector3 validated = normals[i];
            validated.X = Math.Clamp(validated.X, -1.0f, 1.0f);
            validated.Y = Math.Clamp(validated.Y, -1.0f, 1.0f);
            validated.Z = Math.Clamp(validated.Z, -1.0f, 1.0f);
            normals[i] = validated;

            // FIX: Convert normals to glTF coordinate system
            normals[i] = GltfCoordinateConverter.ConvertDirection(normals[i]);
        }

        return normals;
    }

    private static List<Vector4> GetVertexTangents(D3DMesh d3dMesh, T3MeshBatch batch, uint batchVertexCount)
    {
        (T3GFXBuffer? buffer, GFXPlatformAttributeParams? accessor) tangentBuffer =
            d3dMesh.GetBuffer(GFXPlatformVertexAttribute.Tangent, 0);

        if (tangentBuffer.buffer == null) return new List<Vector4>();

        byte[] tangentRange =
            tangentBuffer.buffer.GetBufferRange(batch.MinVertIndex + batch.BaseIndex, batchVertexCount);

        List<Vector4> tangents = DecompressorExtensions.DecompressToColor4D(tangentRange, tangentBuffer.buffer.Stride,
            tangentBuffer.accessor);

        // Normalize and validate each normal
        for (int i = 0; i < tangents.Count; i++)
        {
            Vector4 tangent = tangents[i];

            // Check for invalid values
            if (float.IsNaN(tangent.X) || float.IsNaN(tangent.Y) || float.IsNaN(tangent.Z) ||
                float.IsInfinity(tangent.X) || float.IsInfinity(tangent.Y) || float.IsInfinity(tangent.Z))
            {
                tangents[i] = Vector4.UnitW;
                continue;
            }

            // Normalize the vector
            float length = tangent.Length();
            if (length < 0.0001f)
            {
                tangents[i] = Vector4.UnitW;
            }
            else
            {
                tangents[i] = Vector4.Normalize(tangent);
            }

            // Ensure each component is within valid range
            Vector4 validated = tangents[i];
            validated.X = Math.Clamp(validated.X, -1.0f, 1.0f);
            validated.Y = Math.Clamp(validated.Y, -1.0f, 1.0f);
            validated.Z = Math.Clamp(validated.Z, -1.0f, 1.0f);
            validated.W = Math.Clamp(validated.W, -1.0f, 1.0f);
            tangents[i] = validated;

            // FIX: Convert tangents to glTF coordinate system
            tangents[i] = GltfCoordinateConverter.ConvertTangent(tangents[i]);
        }

        return tangents;
    }

    private static List<Vector2> GetVertexUVs(D3DMesh d3dMesh, T3MeshBatch batch, uint batchVertexCount,
        int channel = 0)
    {
        (T3GFXBuffer? buffer, GFXPlatformAttributeParams? accessor) uvBuffer =
            d3dMesh.GetBuffer(GFXPlatformVertexAttribute.TexCoord, channel);

        if (uvBuffer.buffer == null) return [];

        byte[] uvRange = uvBuffer.buffer.GetBufferRange(batch.MinVertIndex + batch.BaseIndex, batchVertexCount);
        List<Vector3> uv3 =
            DecompressorExtensions.DecompressToVector3D(uvRange, uvBuffer.buffer.Stride, uvBuffer.accessor);
        return uv3.Select(v => new Vector2(v.X, v.Y)).ToList();
    }

    private static (List<Vector4> indices, List<Vector4> weights) GetVertexWeights(D3DMesh d3dMesh, T3MeshBatch batch,
        uint batchVertexCount)
    {
        (T3GFXBuffer? buffer, GFXPlatformAttributeParams? accessor) blendIndexBuffer =
            d3dMesh.GetBuffer(GFXPlatformVertexAttribute.BlendIndex, 0);
        (T3GFXBuffer? buffer, GFXPlatformAttributeParams? accessor) blendWeightBuffer =
            d3dMesh.GetBuffer(GFXPlatformVertexAttribute.BlendWeight, 0);

        if (blendIndexBuffer.buffer == null || blendWeightBuffer.buffer == null)
            return ([], []);

        List<Vector4> blendIndices = DecompressorExtensions.DecompressToColor4D(
            blendIndexBuffer.buffer.GetBufferRange(batch.MinVertIndex + batch.BaseIndex, batchVertexCount),
            blendIndexBuffer.buffer.Stride, blendIndexBuffer.accessor);

        List<Vector4> blendWeights = DecompressorExtensions.DecompressToColor4D(
            blendWeightBuffer.buffer.GetBufferRange(batch.MinVertIndex + batch.BaseIndex, batchVertexCount),
            blendWeightBuffer.buffer.Stride, blendWeightBuffer.accessor);

        return (blendIndices, blendWeights);
    }

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