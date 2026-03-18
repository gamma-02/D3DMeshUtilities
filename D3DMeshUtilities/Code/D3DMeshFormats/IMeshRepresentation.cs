using D3DMeshUtilities.Code.MeshHandling;
using SharpGLTF.Geometry;
using SharpGLTF.Materials;
using TelltaleToolKit.T3Types.Meshes;

namespace D3DMeshUtilities.Code.D3DMeshFormats;

public interface IMeshRepresentation
{

    //this should probably be a part of Read()
    //but, todo: getting mesh info
    public MeshInfo GetMeshInfo();
    
    public bool SaveToMeshBuilder(out IMeshBuilder<MaterialBuilder> meshBuilder);

    //todo: saving to D3DMesh
    public bool SaveToD3DMesh(out D3DMesh? mesh) => throw new NotImplementedException();
}