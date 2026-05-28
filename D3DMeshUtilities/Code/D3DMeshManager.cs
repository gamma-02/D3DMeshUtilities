using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using D3DMeshUtilities.Code.D3DMeshFormats;
using D3DMeshUtilities.Code.ImageStuffAUGH;
using D3DMeshUtilities.Code.MeshHandling;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;
using TelltaleToolKit;
using TelltaleToolKit.Serialization.Binary;
using TelltaleToolKit.T3Types;
using TelltaleToolKit.T3Types.Meshes;
using TelltaleToolKit.T3Types.Textures;
using Codecs = D3DMeshUtilities.Code.MeshHandling.Codecs;
using Texture = D3DMeshUtilities.Code.ImageStuffAUGH.Texture;

namespace D3DMeshUtilities.Code;

public class D3DMeshManager(List<string> file, string outputPath)
{
    
    // these were erroring because I was only parsing UN10x3_UN2
    //now I'm parsing all Vector4 formats
    //done: meshes A-E
    //done: birthday banner 
    
    //These were wierd because I hadn't set up texture coordinate transformations
    //done: arcade game meshes (materials?)
    //done: poker booster seat (textures?)
    
    //All skinning!
    //done: elevatorshaft, exterior, grotien bar case, poker card (armature/skinning?)
    //done: obj_cellPhoneHomestar (armature/skinning?)
    //done: computerSinistar301 (armature/skinning?)
    
    // This was missing per-vertex tangents, which I implemented in the NormalModelIntermediate
    //along with more generalizaitons and abstractions :3
    //done: disguiseGlassesDangeresqueToo tangents?? - This was missing the per-vertex tangents
    
    //done: obj_skyGeneric (allowing for direct encoding of vertex positions)
    
    //done: make sure normals are actually being correct
    
    //todo: why wasn't heavy mesh as table working?

    // public static MeshBuilder<VertexPositionNormal, VertexTexture1, VertexEmpty>? BoneMesh = null;
    
    public List<string> Files = file;
    
    [Discardable]
    public Task? LoadMeshes(Converting? convertingWindow = null)
    {

        Profiler.Instance.BeginFrame("Convert Task", out Profiler.ProfilerFrame convertTask);

        if (!Codecs.Registered)
        {
            Codecs.RegisterCodecs();
        }
        
        if (!Directory.Exists(outputPath))
            return null;

        Profiler.ProfilerFrame? conversionFrame = null;

        foreach (string meshFile in Files)
        {
            string meshName = meshFile.Remove(meshFile.Length - ".d3dmesh".Length);
            
            Profiler.Instance.BeginFrame($"{meshName} conversion", convertTask, out conversionFrame);
            D3DMesh? mesh = TttkInit.Workspace?.LoadAsset<D3DMesh>(meshFile);


            Console.Out.WriteLine($"Attempting to decode {meshFile}");
           
            
            if (mesh == null)
            {
                Profiler.Instance.EndFrame(conversionFrame);
                continue;
            }

            
            Task.Run(() =>
            {
                convertingWindow?.Dispatcher.Invoke(() =>
                    convertingWindow.AddMessageToBox($"Beginning {meshName}"));
            });
            
            MeshInfo info = new MeshInfo(mesh, meshFile);

            IMeshCodec? codec = info.GetMeshRepresentation(mesh);

            if (codec == null)
            {
                Profiler.Instance.EndFrame(conversionFrame);
                Console.Out.WriteLine($"Failed to find a mesh codec that can decode {meshFile}, skipping!");
                
                continue;
            }

            bool succeeded = codec.Read(mesh, info, meshFile, conversionFrame, out IMeshRepresentation? intermediateMesh);

            Profiler.Instance.EndFrame(conversionFrame, out TimeSpan convertDuration); //end convert frame

            if (!succeeded || intermediateMesh == null)
            {
                continue;
            }
            
            string outPath = Path.Combine(outputPath, meshFile.Replace("d3dmesh", "glb"));
            
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"Converting {meshName} took: " + convertDuration); 
            Console.ResetColor();

            convertingWindow?.Dispatcher.Invoke(() => convertingWindow.AddMessageToBox($"Converted {meshName}"));

            Profiler.ProfilerFrame frame = conversionFrame;
            Task t = Task.Run(() =>
            { 
                Profiler.Instance.BeginFrame($"Saving {meshName}", frame, out Profiler.ProfilerFrame savingFrame);
                SceneBuilder scene = new SceneBuilder();

                var rootNode = new NodeBuilder(meshName);

                rootNode.WithLocalScale(new Vector3(10.0f));

                if (!intermediateMesh.SaveToScene(scene, rootNode))
                {
                    Console.Out.WriteLine("Failed to create mesh!");

                    return;
                }

                ModelRoot root = scene.ToGltf2();

                root.SaveGLB(outPath);
                
                Profiler.Instance.EndFrame(savingFrame, out TimeSpan saveDuration);
                
                Console.Out.WriteLine($"Succeeded in converting {meshFile}! \r\n\tSaved at: {outPath}");

                //todo: reduced debug info or something?
                convertingWindow?.Dispatcher.Invoke(() => convertingWindow.AddMessageToBox($"Saved {meshName}.glb to disk"));
                
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Out.WriteLine($"\tSaving {meshName} took: " + saveDuration);
                Console.ResetColor();

            });

            if (Files[^1] == meshFile)
            {
                Profiler.Instance.EndFrame(convertTask, out TimeSpan length);
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("Converting task took: " + length);
                Console.ResetColor();
                
                return t;
            }
            
        }
        
        
        Profiler.Instance.EndFrame(convertTask, out TimeSpan length1);
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine("Converting task took: " + length1);
        Console.ResetColor();
        
        return null;

    }
    
    
    public void SaveTexture(Handle<T3Texture>? textureHandle, Workspace workspace)
    {
        //Make sure the texture exists :D
        if (textureHandle == null || !workspace.ResolveSymbol(textureHandle.ObjectInfo.ObjectName)) return;
        
        // Console.Out.WriteLine("Loading texture: " + textureHandle.ObjectInfo.ObjectName);

        // IFileProvider? textureEntry = workspace.GetFileProviderForResource(textureHandle.ObjectInfo.ObjectName.Crc64);
        //
        // Stream? file = textureEntry?.ExtractFile(textureHandle.ObjectInfo.ObjectName.Crc64);
        
        // Stream? file = workspace.ExtractFile(textureHandle.ObjectInfo.ObjectName.Crc64);
        //             
        // if(file == null || file.Length < 4)
        //     return;
        //
        // if (file.Position != 0)
        //     file.Position = 0;

        T3Texture? texture = workspace.LoadAsset<T3Texture>(textureHandle.ObjectInfo.ObjectName.Crc64);

        if (texture == null)
            return;

        Texture intermediateTexture = D3dtxCodec.Codec.LoadFromMemory(texture, new CodecOptions());
        
        intermediateTexture.ConvertToRGBA8sRGB();
        
        byte[] pngData = PngCodec.Codec.SaveToMemory(intermediateTexture, new CodecOptions(), true);
        
        File.WriteAllBytes(Path.Combine(outputPath, textureHandle.ObjectInfo.ObjectName + ".png"), pngData);
        
        Console.Out.WriteLine("Saved texture: " + textureHandle.ObjectInfo.ObjectName);
    }
    
    
    
}