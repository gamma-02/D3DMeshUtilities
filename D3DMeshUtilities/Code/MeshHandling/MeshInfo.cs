using D3DMeshUtilities.Code.D3DMeshFormats;
using TelltaleToolKit.T3Types;
using TelltaleToolKit.T3Types.Meshes;
using TelltaleToolKit.T3Types.Meshes.T3Types;

namespace D3DMeshUtilities.Code.MeshHandling;

/// <summary>
/// This is designed to be something like a "header" -- holding information about a mesh
/// 
/// </summary>
public class MeshInfo
{

    public ulong? Crc64
    {
        get;

        private set;
    }
    
    
    public bool IsSkinnedModel
    {
        get;

        private set;
    }

    public bool HasVertexTangents
    {
        get;

        private set;
    }

    public bool Has3DVertexBuffer
    {
        get;

        private set;
    }

    public int NumberOfTextureCoords
    {
        get;

        private set;
    }
    
    
    
    

    public MeshInfo(D3DMesh mesh)
    {
        
        bool vertexTangents = false;

        int numberOfTexCoords = 0;
        foreach (GFXPlatformAttributeParams attribute in mesh.MeshData.VertexStates[0].Attributes)
        {
            switch (attribute.Attribute)
            {
                case GFXPlatformVertexAttribute.Tangent:
                    vertexTangents = true;
                    break;
                case GFXPlatformVertexAttribute.TexCoord:
                    numberOfTexCoords += 1;
                    break;
                case GFXPlatformVertexAttribute.Position:
                    if (ConvertFromGfxPlatformFormat.IsFormatVector3(attribute.Format))
                        Has3DVertexBuffer = true;
                    break;
                case GFXPlatformVertexAttribute.Normal:
                case GFXPlatformVertexAttribute.BlendWeight:
                case GFXPlatformVertexAttribute.BlendIndex:
                case GFXPlatformVertexAttribute.Color:
                default:
                    break;
            }
        }

        HasVertexTangents = vertexTangents;

        NumberOfTextureCoords = numberOfTexCoords;
        
        IsSkinnedModel = mesh.MeshData.Bones.Count > 0;


    }

    public MeshInfo(D3DMesh mesh, string meshName) : this(mesh)
    {

        Crc64 = Symbol.GetCrc64(meshName);
    }
    
    

    public IMeshCodec? GetMeshRepresentation(D3DMesh mesh)
    {
        //doing: skinned model parsing lol
        // if (IsSkinnedModel)
        //     return null;
        
        
        //properly detect these kinds of things
        
        // detect the number of texture coords

        //wait, i could also just. do this...
        return Codecs.RegisteredCodecs.Find(c => c.CanRead(mesh, this, out _));

    }
}