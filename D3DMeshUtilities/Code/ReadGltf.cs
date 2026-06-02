using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using D3DMeshUtilities.Code.D3DMeshFormats;
using SharpGLTF.Memory;
using SharpGLTF.Runtime;
using SharpGLTF.Schema2;

namespace D3DMeshUtilities.Code;

public static class ReadGltf
{

    public static IMeshRepresentation? ImportGltf(ModelRoot model)
    {
        // model.LogicalBufferViews
        // foreach (Mesh mesh in model.LogicalMeshes)
        // {
        //     mesh.
        //     // IMeshDecoder<SharpGLTF.Schema2.Material>? decoder = mesh.Decode();
        //     //
        //     // foreach (IMeshPrimitiveDecoder<SharpGLTF.Schema2.Material> primitive in decoder.Primitives )
        //     // {
        //     //     if (primitive.VertexCount != 3)
        //     //     {
        //     //         Console.Out.WriteLine("Please make sure that your mesh is triangulated!");
        //     //         return null;
        //     //     }
        //     //     
        //     //     primitive.TriangleIndices
        //     // }
        //     //
        // }
        
        Console.Out.WriteLine($"Got model: {model}");
        
        MeshPrimitive primitive = model.LogicalMeshes[0].Primitives[0];
        
        
        Profiler.Instance.BeginFrame("Load mesh data", out Profiler.ProfilerFrame loadFrame);
        List<Vector3> positions = GetPositions(primitive);
        List<Vector3> normals = GetNormals(primitive);
        List<Vector4> tangents = GetTangents(primitive);
        List<Vector2> texcoords = GetTextureCoordinates(primitive);
        
        primitive.Material.ToMaterialBuilder();
        
        Profiler.Instance.EndFrame(loadFrame, out TimeSpan loadLength);
        
        Console.Out.WriteLine($"Positions: {positions}");
        
        Console.Out.WriteLine($"{primitive.VertexAccessors["POSITION"].SourceBufferView}");
        
        Console.Out.WriteLine($"Model buffer views: {model.LogicalBufferViews}");
        Console.Out.WriteLine($"Model meshes: {model.LogicalMeshes}");
        
        Console.Out.WriteLine($"Loading took: {loadLength}");
        
        return null;
    }

    private static List<Vector2> GetTextureCoordinates(MeshPrimitive primitive)
    {
        IAccessorArray<Vector2> array = primitive.VertexAccessors["TEXCOORD_0"].AsVector2Array();
        var texcoords = new Vector2[array.Count];
        
        array.CopyTo(texcoords, 0);

        return texcoords.ToList();
    }

    private static List<Vector4> GetTangents(MeshPrimitive primitive)
    {
        IAccessorArray<Vector4> array = primitive.VertexAccessors["TANGENT"].AsVector4Array();
        var normals = new Vector4[array.Count];
        
        array.CopyTo(normals, 0);

        return normals.ToList();
    }

    private static List<Vector3> GetNormals(MeshPrimitive primitive)
    {
        IAccessorArray<Vector3> array = primitive.VertexAccessors["NORMAL"].AsVector3Array();
        var normals = new Vector3[array.Count];
        
        array.CopyTo(normals, 0);

        return normals.ToList();
    }

    private static List<Vector3> GetPositions(MeshPrimitive primitive)
    {
        IAccessorArray<Vector3>? array = primitive.VertexAccessors["POSITION"].AsVector3Array();

        var positionArray = new Vector3[array.Count];
        
        array.CopyTo(positionArray, 0);
        
        return positionArray.ToList();
    }
}