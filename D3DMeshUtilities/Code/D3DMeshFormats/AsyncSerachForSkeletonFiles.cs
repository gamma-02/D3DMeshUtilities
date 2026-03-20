using System.Windows.Documents;
using TelltaleToolKit;
using TelltaleToolKit.Reflection;
using TelltaleToolKit.Resource;
using TelltaleToolKit.T3Types;
using TelltaleToolKit.T3Types.Meshes;
using TelltaleToolKit.T3Types.Properties;
using TelltaleToolKit.T3Types.Skeletons;
using TelltaleToolKit.TelltaleArchives;

namespace D3DMeshUtilities.Code.D3DMeshFormats;

public static class AsyncSerachForSkeletonFiles
{
    public const int NumSearchThreads = 2;

    public static Dictionary<ulong, PropertySet> AgentPropertiesByMeshFile = new Dictionary<ulong, PropertySet>();

    public static Lock AgentMeshDictionaryLock = new Lock();

    public static Task? BuildDictionaryTask = null;

    public static HashSet<ulong> FindSkeletons(IEnumerable<TelltaleFileEntry> entries)
    {
        return entries.Where(entry => entry.Name.EndsWith("skl")).Select(fe => fe.Crc64).ToHashSet();
    }


    public static IEnumerable<TelltaleFileEntry> GetPropFiles(IEnumerable<TelltaleFileEntry> entries)
    {
        return entries.Where(entry => entry.Name.EndsWith("prop"));
    }

    public static IEnumerable<PropertySet> GetPropertySetsFromPropFiles(IEnumerable<TelltaleFileEntry> props, Workspace? w)
    {
        w ??= TttkInit.Instance.Workspace;

        if (w == null)
            return [];
        
        return props
            .Where(fe => w.ContainsFile(fe.Crc64))
            .Select(fe => w.LoadAsset<PropertySet>(fe.Crc64))
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

        foreach (PropertySet set in sets)
        {
            
            //resolve all of the property set stuff
            if (TttkInit.Instance.Workspace != null)
            {
                TttkInit.Instance.Workspace!.ResolveSymbols(set.Properties.Keys);
            }
            
            Handle<D3DMesh>? meshHandle = set.GetProperty<Handle<D3DMesh>>("D3D Mesh");
            
            if(meshHandle != null)
            {
                AgentPropertiesByMeshFile[meshHandle.ObjectInfo.ObjectName.Crc64] = set;
                continue;
            }

            List<Handle<D3DMesh>>? meshHandles = set.GetProperty<List<Handle<D3DMesh>>>("D3D Mesh List");
            
            if(meshHandles != null)
            {
                foreach(Handle<D3DMesh> handle in meshHandles)
                    AgentPropertiesByMeshFile[handle.ObjectInfo.ObjectName.Crc64] = set;
            }

        }
        // bool referencesThisMesh = ps.Properties.Values.Any(pe =>
        //     (pe.Value is Handle<D3DMesh> meshHandle) && meshHandle.ObjectInfo.ObjectName.Crc64 == meshHash);
    }

    public static void BuildAgentMeshDictionary(LoadedArchive archive)
    {

        try
        {
            AgentMeshDictionaryLock.Enter();


            IEnumerable<TelltaleFileEntry> skeletonEntries = [];
            IEnumerable<TelltaleFileEntry> otherOne = [];

            foreach (ResourceContext rc in archive.Contexts!)
            {
                skeletonEntries = skeletonEntries.Concat(rc.GetAllEntries());
                otherOne = otherOne.Concat(rc.GetAllEntries());
            }

            HashSet<ulong> skeletons = AsyncSerachForSkeletonFiles.FindSkeletons(skeletonEntries);

            var propFiles = AsyncSerachForSkeletonFiles.GetPropFiles(otherOne);

            var propSets =
                AsyncSerachForSkeletonFiles.GetPropertySetsFromPropFiles(propFiles, TttkInit.Instance.Workspace);

            var skeletonProperties =
                AsyncSerachForSkeletonFiles.GetPropertySetsReferencingSkeletonFiles(propSets, skeletons);

            AsyncSerachForSkeletonFiles.FillAgentMeshProperties(skeletonProperties);
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