using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TelltaleToolKit;
using TelltaleToolKit.Resource;
using TelltaleToolKit.T3Types;
using TelltaleToolKit.T3Types.Meshes;
using TelltaleToolKit.T3Types.Properties;
using TelltaleToolKit.T3Types.Skeletons;
using TelltaleToolKit.TelltaleArchives;

namespace D3DMeshUtilities.Code.D3DMeshFormats;

public static class AsyncSearchForSkeletonFiles
{
    public const int NumSearchThreads = 2;

    public static Dictionary<ulong, PropertySet> AgentPropertiesByMeshFile = new Dictionary<ulong, PropertySet>();
    public static bool BuiltDictionary { get; private set; }

    public static Lock AgentMeshDictionaryLock = new Lock();

    public static Task? BuildDictionaryTask = null;

    public static HashSet<ulong> FindSkeletons(IEnumerable<ResourceEntry> entries)
    {
        return entries.Where(entry => entry.Name.EndsWith("skl")).Select(fe => fe.NameCrc).ToHashSet();
    }


    public static IEnumerable<ResourceEntry> GetPropFiles(IEnumerable<ResourceEntry> entries)
    {
        return entries.Where(entry => entry.Name.EndsWith("prop"));
    }

    public static IEnumerable<PropertySet> GetPropertySetsFromPropFiles(IEnumerable<ResourceEntry> props, Workspace? w)
    {
        w ??= TttkInit.Workspace;

        if (w == null)
            return [];
        
        return props
            .Where(fe => w.ContainsFile(fe.NameCrc))
            .Select(fe => w.LoadAsset<PropertySet>(fe.NameCrc))
            .Where(ps => ps != null)
            .Select(ps => ps!);
    }
    
    public static IEnumerable<PropertySet> GetPropertySetsReferencingSkeletonFiles(IEnumerable<PropertySet> sets, HashSet<ulong> skeletons)
    {
        return sets.Where(
            ps => ps.Properties.Values
            .Any(pe => (pe.Value is Handle<Skeleton> h) && skeletons.Contains(h.ObjectInfo.ObjectName.Crc64))
        );
    }


    // public static void FillAgentMeshPropertiesFromSkeletons(IEnumerable<PropertySet> sets, HashSet<ulong> skeletons)
    //     => FillAgentMeshProperties(GetPropertySetsReferencingSkeletonFiles(sets, skeletons));


    public static void FillAgentMeshProperties(IEnumerable<PropertySet> sets)
    {

        lock(ResourceLoader.ResourceLock)
        {
            foreach (PropertySet set in sets)
            {

                //resolve all of the property set stuff
                if (TttkInit.Workspace != null)
                {
                    TttkInit.Workspace!.ResolveSymbols(set.Properties.Keys);
                }

                Handle<D3DMesh>? meshHandle = set.GetProperty<Handle<D3DMesh>>("D3D Mesh");

                if (meshHandle != null)
                {
                    AgentPropertiesByMeshFile[meshHandle.ObjectInfo.ObjectName.Crc64] = set;
                    continue;
                }

                List<Handle<D3DMesh>>? meshHandles = set.GetProperty<List<Handle<D3DMesh>>>("D3D Mesh List");

                if (meshHandles != null)
                {
                    foreach (Handle<D3DMesh> handle in meshHandles)
                        AgentPropertiesByMeshFile[handle.ObjectInfo.ObjectName.Crc64] = set;
                }

            }
        }

        // BuiltDictionary = true;
        // bool referencesThisMesh = ps.Properties.Values.Any(pe =>
        //     (pe.Value is Handle<D3DMesh> meshHandle) && meshHandle.ObjectInfo.ObjectName.Crc64 == meshHash);
    }

    public static void BuildAgentMeshDictionary(ResourceLoader archive)
    {

        try
        {
            AgentMeshDictionaryLock.Enter();

            if (BuiltDictionary)
                return;

            Profiler.Instance.BeginFrame("Skeleton search", out Profiler.ProfilerFrame sklSearchFrame);
            IEnumerable<ResourceEntry> skeletonEntries = [];
            IEnumerable<ResourceEntry> otherOne = [];

            foreach (ResourceContext rc in archive.Contexts!)
            {
                skeletonEntries = skeletonEntries.Concat(rc.GetAllEntries());
                otherOne = otherOne.Concat(rc.GetAllEntries());
            }

            HashSet<ulong> skeletons = FindSkeletons(skeletonEntries);

            var propFiles = GetPropFiles(otherOne);

            var propSets =
                GetPropertySetsFromPropFiles(propFiles, TttkInit.Workspace);


            var skeletonProperties =
                GetPropertySetsReferencingSkeletonFiles(propSets, skeletons);

            FillAgentMeshProperties(skeletonProperties);
            Profiler.Instance.EndFrame(sklSearchFrame, out TimeSpan length);

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Out.WriteLine("\tFinished building skeleton dictionary, took: " + length);
            Console.ResetColor();
            
            BuiltDictionary = true;
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            AgentMeshDictionaryLock.Exit();
        }
        
        

    }
    
    
    
    
    
    
    
}