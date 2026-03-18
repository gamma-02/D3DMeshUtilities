using D3DMeshUtilities.Code.D3DMeshFormats;
using TelltaleToolKit.T3Types.Meshes;

namespace D3DMeshUtilities.Code.MeshHandling;

public interface IMeshCodec
{

    public bool Read(D3DMesh mesh, MeshInfo info, string meshFile, out IMeshRepresentation? readMesh);

    public bool CanRead(D3DMesh mesh, MeshInfo info, out string? reason);
}