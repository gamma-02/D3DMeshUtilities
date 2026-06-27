using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TelltaleToolKit;
using TelltaleToolKit.IO.Archives;
using TelltaleToolKit.IO.Resources;
using TelltaleToolKit.T3Types;
using TelltaleToolKit.T3Types.Meshes;
using TelltaleToolKit.T3Types.Properties;
using TelltaleToolKit.T3Types.Skeletons;


namespace D3DMeshUtilities.Code.D3DMeshFormats;

public static class AsyncSearchForSkeletonFiles
{
    public const int NumSearchThreads = 2;

    public static Dictionary<ulong, (PropertySet set, Symbol name)> AgentPropertiesByMeshFile = [];
    public static Dictionary<string, List<(PropertySet set, Symbol name)>> AgentPropertiesByContextName = [];
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

    public static IEnumerable<(PropertySet set, Symbol name)> GetPropertySetsFromPropFiles(IEnumerable<ResourceEntry> props, Workspace? w)
    {
        w ??= TttkInit.Workspace;

        if (w == null)
            return [];
        
        return props
            .Where(fe => w.ContainsFile(fe.NameCrc))
            .Select(fe => (w.LoadAsset<PropertySet>(fe.NameCrc), Symbol.FromExplicit(fe.Name, fe.NameCrc)))
            .Where(ps => ps.Item1 != null)
            .Select(ps => (ps.Item1!, ps.Item2));
    }
    
    public static IEnumerable<(PropertySet set, Symbol name)> GetPropertySetsReferencingSkeletonFiles(IEnumerable<(PropertySet set, Symbol name)> sets, HashSet<ulong> skeletons)
    {
        return sets.Where(
            ps => ps.set.Properties.Values
            .Any(pe => (pe.Value is Handle<Skeleton> h) && skeletons.Contains(h.ObjectInfo.ObjectName.Crc64))
        );
    }


    // public static void FillAgentMeshPropertiesFromSkeletons(IEnumerable<PropertySet> sets, HashSet<ulong> skeletons)
    //     => FillAgentMeshProperties(GetPropertySetsReferencingSkeletonFiles(sets, skeletons));


    public static void FillAgentMeshProperties(IEnumerable<(PropertySet set, Symbol name)> sets)
    {

        lock(ResourceLoader.ResourceLock)
        {
            foreach ((PropertySet set, Symbol name) set in sets)
            {

                //resolve all of the property set stuff
                if (TttkInit.Workspace != null)
                {
                    TttkInit.Workspace!.ResolveSymbols(set.set.Properties.Keys);
                }

                Handle<D3DMesh>? meshHandle = set.set.GetProperty<Handle<D3DMesh>>("D3D Mesh");

                if (meshHandle != null)
                {
                    AgentPropertiesByMeshFile[meshHandle.ObjectInfo.ObjectName.Crc64] = set;
                    continue;
                }

                List<Handle<D3DMesh>>? meshHandles = set.set.GetProperty<List<Handle<D3DMesh>>>("D3D Mesh List");

                if (meshHandles != null)
                {
                    foreach (Handle<D3DMesh> handle in meshHandles)
                        AgentPropertiesByMeshFile[handle.ObjectInfo.ObjectName.Crc64] = set;
                }

                ResourceContext? context = ResourceLoader.Instance.GetContextFromSymbol(set.name);
                
                if (context == null) continue;

                if (!AgentPropertiesByContextName.ContainsKey(context.Name))
                {
                    AgentPropertiesByContextName[context.Name] = [];
                }
                
                AgentPropertiesByContextName[context.Name].Add(set);
                
                

                //this is a moot point :P
                //I use the context name everywhere not the archive name. WHOOPS.
                
                
                
                // if (context.Providers.Count <= 0) continue;
                //
                // foreach (IFileProvider fileProvider in context.Providers)
                // {
                //     if(fileProvider is not ArchiveProvider provider) continue;
                //     
                //     //G.I.L.R
                //     //(god i love reflection)
                //     
                //     Type type = typeof(ArchiveProvider);
                //
                //     // 2. Fetch the private field metadata using specific binding flags
                //     FieldInfo? fieldInfo = type.GetField("_archive", BindingFlags.NonPublic | BindingFlags.Instance);
                //
                //     // 3. Extract the value from the specific instance
                //     Archive? archive = fieldInfo?.GetValue(fileProvider) as Archive;
                //     
                //     if(archive is null) continue;
                //     
                //     
                //
                //     
                // }

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
            IEnumerable<ResourceEntry> propEntries = [];

            foreach (ResourceContext rc in archive.Contexts!)
            {
                skeletonEntries = skeletonEntries.Concat(rc.GetAllEntries());
                propEntries = propEntries.Concat(rc.GetAllEntries());
            }

            HashSet<ulong> skeletons = FindSkeletons(skeletonEntries);

            var propFiles = GetPropFiles(propEntries);

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
            
            Task.Run(ArchiveAgentList.BuildArchiveAgentDictionary);
            
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