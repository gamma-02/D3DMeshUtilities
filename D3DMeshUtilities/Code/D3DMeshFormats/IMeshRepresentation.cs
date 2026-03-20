using D3DMeshUtilities.Code.MeshHandling;
using SharpGLTF.Geometry;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using TelltaleToolKit.T3Types.Meshes;

namespace D3DMeshUtilities.Code.D3DMeshFormats;

public interface IMeshRepresentation
{

    //this should probably be a part of Read()
    public MeshInfo GetMeshInfo();
    
    public bool SaveToMeshBuilder(out IMeshBuilder<MaterialBuilder> meshBuilder);

    public bool SaveToScene(SceneBuilder scene, NodeBuilder modelRoot);

    //todo: saving to D3DMesh
    public bool SaveToD3DMesh(out D3DMesh? mesh) => throw new NotImplementedException();
}