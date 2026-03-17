using System.CodeDom.Compiler;
using System.IO;
using System.Text;
using TelltaleToolKit.T3Types.Meshes.T3Types;

namespace D3DMeshUtilities.Code;

public class Utils
{
    public static string MeshBatchToString(T3MeshBatch batch)
    {
        
        var sb = new StringBuilder();

        var sw = new StringWriter(sb);

        var isw = new IndentedTextWriter(sw);
        
        
        isw.WriteLine("Bounding Box: \t\t\t" + batch.BoundingBox);
        isw.WriteLine("Bounding Sphere: \t\t" + batch.BoundingSphere);
        isw.WriteLine("Batch usage: \t\t\t" + batch.BatchUsage);
        isw.WriteLine("Vertex index: ");
        isw.Indent++;
        isw.WriteLine("Min: \t" + batch.MinVertIndex);
        isw.WriteLine("Max: \t" + batch.MaxVertIndex);
        isw.WriteLine("Base: \t" + batch.BaseIndex);
        isw.WriteLine("Start: \t" + batch.StartIndex);
        isw.Indent--;
        isw.WriteLine("Primitives: \t\t\t" + batch.NumPrimitives);
        isw.WriteLine("Indices: \t\t\t" + batch.NumIndices);
        
        // if(batch.TextureIndices != null)
        // {
        //     isw.WriteLine($"Texture Indices: \t\t [{batch.TextureIndices.Index[0]}, {batch.TextureIndices.Index[1]}]");
        // }
        
        isw.WriteLine("Material Index: \t\t" + batch.MaterialIndex);
        isw.WriteLine("Adjacency Start: \t\t" + batch.AdjacencyStartIndex);
        isw.WriteLine("Local Transform Index: \t\t" + batch.LocalTransformIndex);
        isw.WriteLine("Bone Palette Index: \t\t" + batch.BonePaletteIndex);
        
        isw.Flush();
        
        isw.Close();
        
        sw.Flush();
        
        sw.Close();
        


        return sb.ToString();
    }
}