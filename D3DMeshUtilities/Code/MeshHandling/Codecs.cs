using D3DMeshUtilities.Code.D3DMeshFormats;
using TelltaleToolKit.T3Types.Meshes;
using TelltaleToolKit.T3Types.Meshes.T3Types;
using TelltaleToolKit.T3Types.Miscellaneous;

namespace D3DMeshUtilities.Code.MeshHandling;

public static class Codecs
{
    public static readonly List<IMeshCodec> RegisteredCodecs = new ();

    public static bool Registered
    {
        get;

        private set;
    }


    public static void RegisterCodecs()
    {
        
        RegisteredCodecs.Add(new NormalIntermediateCodec());
        
        RegisteredCodecs.Add(new SkinnedIntermediateCodec());

        Registered = true;

    }
}

public class NormalIntermediateCodec : IMeshCodec
{
    public bool Read(D3DMesh mesh, MeshInfo info, string meshFile, out IMeshRepresentation? readMesh)
    {
        bool succeeded = NormalModelIntermediate.Read(mesh, info, meshFile, out NormalModelIntermediate? intermediate);

        readMesh = intermediate;

        return succeeded;
    }

    public bool CanRead(D3DMesh mesh, MeshInfo info, out string? reason)
    {
        if (info.IsSkinnedModel)
        {
            reason = "Mesh is a skinned model, this is not the skinned model codec";
            return false;
        }
        
        for (var index = 0; index < mesh.MeshData.VertexStates.Count; index++)
        {
            var state = mesh.MeshData.VertexStates[index];

            if (state.IndexBufferCount <= 0)
            {
                //will not read :3
                reason = $"Vertex state {index} does not have any index buffers!";
                return false;
            }
            
            bool vertexPositions = false;
            bool vertexNormals = false;
            bool vertexTangents = false;
            bool texCoordList1 = false;

            foreach (GFXPlatformAttributeParams attribute in state.Attributes)
            {
                switch (attribute.Attribute)
                {
                    case GFXPlatformVertexAttribute.Position:
                        vertexPositions = true;
                        break;
                    case GFXPlatformVertexAttribute.Normal:
                        vertexNormals = true;
                        break;
                    case GFXPlatformVertexAttribute.Tangent:
                        vertexTangents = true;
                        break;
                    case GFXPlatformVertexAttribute.TexCoord:
                        texCoordList1 = true;
                        break;
                    case GFXPlatformVertexAttribute.BlendWeight:
                    case GFXPlatformVertexAttribute.BlendIndex:
                    case GFXPlatformVertexAttribute.Color:
                    default:
                        break;
                }
            }

            if (!vertexPositions)
            {
                reason = $"Vertex state {index} does not have any vertex positions!";
                return false;
            }

            if (!vertexNormals)
            {
                reason = $"Vertex state {index} does not have any vertex normals!";
                return false;
            }

            if (!vertexTangents && info.HasVertexTangents)
            {
                reason = $"Vertex state {index} does not have any vertex tangents!";
                return false;
            }

            if (!texCoordList1)
            {
                reason = $"Vertex state {index} does not have any texture coordinates!";
                return false;
            }
            
            
        }

        reason = null;
        return true;
    }
}

public class SkinnedIntermediateCodec : IMeshCodec
{
    public bool Read(D3DMesh mesh, MeshInfo info, string meshFile, out IMeshRepresentation? readMesh)
    {
        bool succeeded = SkinnedModelIntermediate.Read(mesh, info, meshFile, out SkinnedModelIntermediate? intermediate);

        readMesh = intermediate;

        return succeeded;
    }

    public bool CanRead(D3DMesh mesh, MeshInfo info, out string? reason)
    {
        for (var index = 0; index < mesh.MeshData.VertexStates.Count; index++)
        {
            var state = mesh.MeshData.VertexStates[index];
            
            bool vertexPositions = false;
            bool vertexNormals = false;
            bool vertexTangents = false;
            bool texCoordList1 = false;

            foreach (GFXPlatformAttributeParams attribute in state.Attributes)
            {
                switch (attribute.Attribute)
                {
                    case GFXPlatformVertexAttribute.Position:
                        vertexPositions = true;
                        break;
                    case GFXPlatformVertexAttribute.Normal:
                        vertexNormals = true;
                        break;
                    case GFXPlatformVertexAttribute.Tangent:
                        vertexTangents = true;
                        break;
                    case GFXPlatformVertexAttribute.TexCoord:
                        texCoordList1 = true;
                        break;
                    case GFXPlatformVertexAttribute.BlendWeight:
                    case GFXPlatformVertexAttribute.BlendIndex:
                    case GFXPlatformVertexAttribute.Color:
                    default:
                        break;
                }
            }

            if (!vertexPositions)
            {
                reason = $"Vertex state {index} does not have any vertex positions!";
                return false;
            }

            if (!vertexNormals)
            {
                reason = $"Vertex state {index} does not have any vertex normals!";
                return false;
            }

            if (!vertexTangents && info.HasVertexTangents)
            {
                reason = $"Vertex state {index} does not have any vertex tangents!";
                return false;
            }

            if (!texCoordList1)
            {
                reason = $"Vertex state {index} does not have any texture coordinates!";
                return false;
            }
            
            
        }

        reason = null;
        return true;
    }
}
