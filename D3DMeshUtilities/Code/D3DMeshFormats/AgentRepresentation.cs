using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using D3DMeshUtilities.Code.MeshHandling;
using D3DMeshUtilities.Code.Util;
using JetBrains.Annotations;
using SharpGLTF.Geometry;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;
using TelltaleToolKit.T3Types;
using TelltaleToolKit.T3Types.Meshes;
using TelltaleToolKit.T3Types.Meshes.T3Types;
using TelltaleToolKit.T3Types.Properties;
using TelltaleToolKit.T3Types.Skeletons;

namespace D3DMeshUtilities.Code.D3DMeshFormats;

public record MeshVisibility(Handle<D3DMesh> Mesh, bool Visible)
{
    public string? MeshName
    {
        get => Mesh.GetDebugString();
    }
}

//note: "Mesh %s - Visible" is what is used for checking visibility
//
public class AgentRepresentation
{
    public static bool BeganConvertingAgent = false;
    
    public string PropertySetName { get; set; }
    public PropertySet AgentPropertySet;
    
    public List<Handle<D3DMesh>> AgentMeshHandles = [];
    public List<Handle<D3DMesh>> VisibleAgentMeshes = [];

    private ObservableCollection<MeshVisibility>? _meshVisibilities; //build and cache mesh visibility
    public ObservableCollection<MeshVisibility> MeshAndVisibility 
    {
        get
        {
            if (_meshVisibilities != null) return _meshVisibilities;
            List<MeshVisibility> val = [];
            val.AddRange(AgentMeshHandles.Select(handle => new MeshVisibility(handle, VisibleAgentMeshes.Contains(handle))));
            _meshVisibilities = new ObservableCollection<MeshVisibility>(val);
            return _meshVisibilities;
        }
    }

    public Skeleton? Skeleton;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public AgentRepresentation(List<string> fakeMeshes, List<string> fakeVisibleMeshes, string fakeName)
    {
        AgentMeshHandles = fakeMeshes.Select((name) =>
        {
            var h = new Handle<D3DMesh>();
            h.ObjectInfo.ObjectName = Symbol.FromName(name);
            return h;
        }).ToList();
        
        VisibleAgentMeshes = fakeVisibleMeshes.Select((name) =>
        {
            return AgentMeshHandles.First(h => h.GetDebugString()?.Equals(name) ?? false);
        }).ToList();

        PropertySetName = fakeName;
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    
    public AgentRepresentation(PropertySet agentPropertySet, string propertySetName)
    {
        BeganConvertingAgent = true;
        
        AgentPropertySet = agentPropertySet;
        PropertySetName = propertySetName;
        
        var nullableMeshList = agentPropertySet.GetProperty<List<Handle<D3DMesh>>>("D3D Mesh List");
        
        if(nullableMeshList != null)
            AgentMeshHandles = nullableMeshList;

        foreach (Handle<D3DMesh> handle in AgentMeshHandles)
        {
            bool? meshVisible = agentPropertySet.GetProperty<bool>(
                $"Mesh {handle.GetObjectName().DebugString?.Replace(".d3dmesh", "")} - Visible");

            if (meshVisible.HasValue && meshVisible.Value)
            {
                VisibleAgentMeshes.Add(handle);
            }
            
        }

        Skeleton = agentPropertySet.GetProperty<Handle<Skeleton>>("Skeleton File")?
            .GetObject<Skeleton>(TttkInit.Workspace!);


    }

    public void SaveMeshes(string outputPath, Converting? convertingWindow, Profiler.ProfilerFrame? taskFrame = null, List<Handle<D3DMesh>>? meshesToSave = null)
    {
        Profiler.Instance.BeginFrame($"Converting agent {PropertySetName}", taskFrame, out Profiler.ProfilerFrame agentConversionFrame);
        if (meshesToSave == null)
        {
            meshesToSave = VisibleAgentMeshes;
        }

        List<(D3DMesh mesh, Symbol meshSymbol)> meshes = ResolveD3DMeshHandles(meshesToSave);



        List<IMeshRepresentation?> convertedMeshes = [];

        foreach ((D3DMesh mesh, Symbol name) in meshes)
        {
            var representation = ConvertMesh(convertingWindow, mesh, name.ToString(), agentConversionFrame);
            
            convertedMeshes.Add(representation);
        }

        // List<Task<IMeshRepresentation?>> conversionTasks = [];
        // foreach ((D3DMesh mesh, Symbol meshSymbol) in meshes)
        // {
        //     conversionTasks.Add(Task.Run(() => ConvertMesh(convertingWindow, mesh, meshSymbol.ToString(), agentConversionFrame)));
        // }
        //
        // List<IMeshRepresentation?> convertedMeshes = [];
        // await foreach (IMeshRepresentation? representation in ProcessTasksAsync(conversionTasks))
        // {
        //     convertedMeshes.Add(representation);
        // }
        //
        // Console.Out.WriteLine("WOO YEAH WE DID A THING LETS GOOOOOOOOO (I don't know if it works this is for Debugging Purposes)");
        
        //todo: converting window logging all through this, more extensive console logging
        
        Profiler.Instance.BeginFrame($"Saving {PropertySetName}", agentConversionFrame, out Profiler.ProfilerFrame savingFrame);
        SceneBuilder scene = new SceneBuilder();

        var rootNode = new NodeBuilder($"{PropertySetName}");
        
        bool builtSkeleton = 
            SkinnedModelIntermediate.BuildSkeletonStructure(
                Skeleton, 
                rootNode, 
                out List<NodeBuilder> jointNodeBuilders, 
                out Dictionary<string, int> boneNameToJointIndex
                );

        if (!builtSkeleton)
        {
            Profiler.Instance.EndFrame(savingFrame);
            Profiler.Instance.EndFrame(agentConversionFrame);

            Console.Out.WriteLine($"Failed to load skeleton structure for agent prop {PropertySetName}");
            return;
        }

        bool success = true;
        
        foreach (IMeshRepresentation? representation in convertedMeshes)
        {
            if(representation is not SkinnedModelIntermediate skinnedMesh) continue;

            if (!skinnedMesh.SaveSkinnedModelToScene(scene, rootNode, jointNodeBuilders, boneNameToJointIndex))
            {
                success = false;
                break;
            }
        }

        if (!success)
        {
            Profiler.Instance.EndFrame(savingFrame);
            Profiler.Instance.EndFrame(agentConversionFrame);
            
            Console.Out.WriteLine("Failed to create mesh!");

            return;
        }
        
        ModelRoot root = scene.ToGltf2();
        
        string outPath = Path.Combine(outputPath, PropertySetName.Replace("prop", "glb"));

        root.SaveGLB(outPath);
        
        Profiler.Instance.EndFrame(savingFrame);
        Profiler.Instance.EndFrame(agentConversionFrame);
        
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.Out.WriteLine($"Saved agent {AgentPropertySet} to {outPath}, took: {savingFrame.Length}");
        Console.ResetColor();
        
        
    }
    
    private IMeshRepresentation? ConvertMesh(Converting? convertingWindow, D3DMesh mesh, string meshFile, Profiler.ProfilerFrame convertTask)
    {
        string meshName = meshFile.Remove(meshFile.Length - ".d3dmesh".Length);
            
        Profiler.Instance.BeginFrame($"{meshName} conversion", convertTask, out Profiler.ProfilerFrame conversionFrame);

        Console.Out.WriteLine($"Attempting to decode {meshFile}");
            
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
            
            return null;
        }

        bool succeeded = codec.Read(mesh, info, meshFile, conversionFrame, out IMeshRepresentation? intermediateMesh);

        Profiler.Instance.EndFrame(conversionFrame, out TimeSpan convertDuration); //end convert frame

        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine($"Converting {meshName} took: {convertDuration}"); 
        Console.ResetColor();

        if (!succeeded || intermediateMesh == null)
        {
            return null;
        }

        return intermediateMesh;
    }
    
    
    
    /*
     * =[
        string outPath = Path.Combine(outputPath, meshFile.Replace("d3dmesh", "glb"));
            
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine($"Converting {meshName} took: " + convertDuration); 
        Console.ResetColor();

        convertingWindow?.Dispatcher.Invoke(() => convertingWindow.AddMessageToBox($"Converted {meshName}"));

        Profiler.ProfilerFrame frame = conversionFrame;
        t = Task.Run(() =>
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
     */

    private static List<(D3DMesh, Symbol)> ResolveD3DMeshHandles(List<Handle<D3DMesh>> meshHandleList)
    {
        List<(D3DMesh mesh, Symbol meshReference)> meshes = [];
        foreach (Handle<D3DMesh> handle in meshHandleList)
        {
            D3DMesh? mesh = handle.GetObject<D3DMesh>(TttkInit.Workspace!);

            if (mesh == null)
            {
                Console.Out.WriteLine($"Mesh refereced by {handle.GetObjectName()} is null! Skipping...");
                continue;
            }
            
            meshes.Add((mesh, handle.GetObjectName()));
        }

        return meshes;
    }
    
    
    public static async IAsyncEnumerable<T> ProcessTasksAsync<T>(List<Task<T>> tasks)
    {
        var taskList = tasks.ToList();
    
        while (taskList.Count > 0)
        {
            // Wait for the first task to complete
            Task<T> finishedTask = await Task.WhenAny(taskList);
            taskList.Remove(finishedTask);

            // Yield the result as soon as it's available
            yield return await finishedTask; 
        }
    }
    
    
}

