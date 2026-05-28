using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using D3DMeshUtilities.Code.D3DMeshFormats;
using TelltaleToolKit;
using TelltaleToolKit.Resource;
using TelltaleToolKit.TelltaleArchives;

namespace D3DMeshUtilities;

public class ResourceLoader
{
    public ResourceContext? ArchiveContext;
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

    public static bool SetArchive;

    public static object ResourceLock = new();

    public void LoadArchive(Dispatcher dispatcher, string archiveLocation, string game)
    {

        TttkInit.Instance.Workspace ??= Toolkit.Instance.CreateWorkspace("D3DMeshUtilsWorkspace",
            game);

        ArchiveLocation = archiveLocation;
        
        ArchiveLocationLock.Enter();
        
        dispatcher.InvokeAsync(LoadArchive).GetAwaiter().OnCompleted(() => SetArchive = true);

    }

    public async Task<List<string>> LoadResourceContexts(CancellationTokenSource cts, string gameDataDir, string game)
    {
        Profiler.Instance.BeginFrame("Resource Loading");
        if (TttkInit.Instance.Workspace == null || TttkInit.Instance.Workspace.GameName != game)
        {
            TttkInit.Instance.Workspace = Toolkit.Instance.CreateWorkspace("D3DMeshUtilsWorkspace",
                game);
            
        }

        IEnumerable<string> resdescs = Directory.EnumerateFiles(gameDataDir, "*.lua").Where((s) => !s.Contains("_version_"));

        List<string> archives = [];
        Contexts = [];
        
        foreach (string resdesc in resdescs)
        {
            ResourceContext? rc;
            try
            {
                 rc = TttkInit.Instance.Workspace.LoadResourceDescription(resdesc);
            }
            catch (Exception)
            {
                cts.Cancel();
                return [];
            }
                
            if (rc == null)
            {
                Console.WriteLine($"Got nil lua table value for resdesc: {resdesc}");
                continue;
            }
            
            Contexts.Add(rc);
            
            archives.AddRange(rc.Providers.Select((e) => (e is ArchiveProvider p) ? p.Path : "").ToArray());
        }

        Profiler.Instance.EndFrame(out TimeSpan length);
        
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.Out.WriteLine("\tLoading game resources took: " + length);
        Console.ResetColor();
        
        return archives.Where(s => !string.IsNullOrEmpty(s)).ToList();
        
        
    }

    private async void LoadArchive()
    {
        // var startLoadTime = DateTime.Now;
        Profiler.Instance.BeginFrame("Resource load");
        AsyncSearchForSkeletonFiles.BuildDictionaryTask = Task.Run(() => AsyncSearchForSkeletonFiles.BuildAgentMeshDictionary(ResourceLoader.Instance));
        
        lock(ResourceLock)
        {
            if (Contexts == null)
                Contexts = new List<ResourceContext>();
            else if (Contexts.Count != 0)
                return;

            Instance.ArchiveContext = TttkInit.Instance.Workspace?.LoadArchive(ArchiveLocation, "Current Archive");

            if (Instance.ArchiveContext != null)
                Contexts.Add(Instance.ArchiveContext);
            
            ArchiveLocationLock.Exit();

        }
        
        Profiler.Instance.EndFrame(out TimeSpan length);
        
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.Out.WriteLine("\tLoading game resources took: " + length);
        Console.ResetColor();

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
            ArchiveLocationLock.Enter();
            
            ResourceContext? ctx = GetContextWithArchive(archivePath);

            if (ctx == null)
                return [];
            
            ArchiveLocationLock.Exit();

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