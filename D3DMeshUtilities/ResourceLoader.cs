using System.IO;
using System.Text;
using Avalonia.Threading;
using D3DMeshUtilities.Code;
using D3DMeshUtilities.Code.D3DMeshFormats;
using TelltaleToolKit;
using TelltaleToolKit.Resource;
using TelltaleToolKit.TelltaleArchives;

namespace D3DMeshUtilities;

public class ResourceLoader
{
    public ResourceContext? ArchiveContext = null;
    // public string CurrentArchivePath = "";

    public List<ResourceContext>? Contexts
    {
        get;
        private set;
    } = new List<ResourceContext>();

    public static readonly ResourceLoader Instance = new ResourceLoader();

    public string ArchiveLocation = "";

    public static Lock ArchiveLocationLock = new Lock();

    public static Lock ResourceContextLock = new Lock();

    public static bool SetArchive = false;

    public static object ResourceLock = new();

    public void LoadArchive(Dispatcher dispatcher, string archiveLocation, string game)
    {

        TttkInit.Instance.Workspace ??= Toolkit.Instance.CreateWorkspace("D3DMeshUtilsWorkspace",
            game);

        ArchiveLocation = archiveLocation;
        
        ArchiveLocationLock.Enter();
        
        dispatcher.InvokeAsync(LoadArchive).GetAwaiter().OnCompleted(() => SetArchive = true);

    }

    public async Task<List<string>> LoadResourceContexts(Dispatcher dispatcher, string gameDataDir, string game)
    {
        
        if (TttkInit.Instance.Workspace == null)
        {
            TttkInit.Instance.Workspace = Toolkit.Instance.CreateWorkspace("D3DMeshUtilsWorkspace",
                game);
            
        }

        var resdescs = Directory.EnumerateFiles(gameDataDir, "*.lua").Where((s) => !s.Contains("_version_"));

        List<string> archives = [];
        Contexts = new List<ResourceContext>();
        
        foreach (var resdesc in resdescs)
        {
            var rc = TttkInit.Instance.Workspace.LoadResourceDescription(resdesc);
            
            Contexts.Add(rc);
            
            archives.AddRange(rc.Providers.Select((e) => (e is ArchiveProvider p) ? p.Path : "").ToArray());
        }

        return archives.Where((s => !string.IsNullOrEmpty(s))).ToList();
        
        
    }

    private async void LoadArchive()
    {
        AsyncSearchForSkeletonFiles.BuildDictionaryTask = Task.Run(() => AsyncSearchForSkeletonFiles.BuildAgentMeshDictionary(ResourceLoader.Instance));
        
        lock(ResourceLock)
        {
            // Instance.CurrentArchive = TttkInit.Instance.Workspace?.LoadArchive(_archiveLocation, debugPrint: true);
            // Instance.ArchiveContext = TttkInit.Instance.Workspace?.LoadArchive(_archiveLocation, "Current Archive");

            if (Contexts == null)
                Contexts = new List<ResourceContext>();
            else
                return;

            Instance.ArchiveContext = TttkInit.Instance.Workspace?.LoadArchive(ArchiveLocation, "Current Archive");

            if (Instance.ArchiveContext != null)
                Contexts.Add(Instance.ArchiveContext);

            ArchiveLocationLock.Exit();
        }
        

    }

    public ResourceContext? GetContextWithArchive(string archivePath)
    {
        // int seperator = archivePath.LastIndexOfAny(['\\', '/']);
        //
        // string archiveName = archivePath.Substring(seperator + 1);

        return Contexts?
            .Where(rc => rc.Providers.Any(
                    fp => (fp is ArchiveProvider ap && ap.Path.Equals(archivePath))
                )
            ).First();
    }
    
    public IEnumerable<TelltaleFileEntry> GetEntriesInArchive(string archivePath)
    {
        lock(ResourceLock)
        {
            ResourceContext? ctx = GetContextWithArchive(archivePath);

            if (ctx == null)
                return [];

            return ctx.GetAllEntries();
        }
    }

    public Stream? ExtractFile(string file)
    {
        lock(ResourceLock)
        {
            return TttkInit.Instance.Workspace?.ExtractFile(file);
        }
    }

    public IEnumerable<TelltaleFileEntry> GetEntriesInCurrentArchive()
    {
        lock(ResourceLock)
        {
            return GetEntriesInArchive(ArchiveLocation);
        }
    }
}