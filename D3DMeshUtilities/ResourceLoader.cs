using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using D3DMeshUtilities.Code.D3DMeshFormats;
using TelltaleToolKit;
using TelltaleToolKit.IO.Archives;
using TelltaleToolKit.IO.Resources;
using Tmds.DBus.Protocol;


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

    public void LoadArchive(MainWindow window, string archiveLocation, string game)
    {

        TttkInit.Workspace ??= Toolkit.Instance.CreateWorkspace("D3DMeshUtilsWorkspace",
            game);

        ArchiveLocation = archiveLocation;
        
        ArchiveLocationLock.Enter();
        
        window.Dispatcher.InvokeAsync(() => LoadArchive(window)).GetAwaiter().OnCompleted(() => SetArchive = true);

    }

    public async Task<List<string>> LoadResourceContexts(CancellationTokenSource cts, MainWindow window, string gameDataDir, string game)
    {
        Profiler.Instance.BeginFrame("Resource Loading", out Profiler.ProfilerFrame loadFrame);
        
        if (TttkInit.Workspace == null || TttkInit.Workspace.GameName != game)
        {
            TttkInit.Workspace = Toolkit.Instance.CreateWorkspace("D3DMeshUtilsWorkspace",
                game);
            
        }

        IEnumerable<string> resdescs = Directory.EnumerateFiles(gameDataDir, "_res*.lua").Where((s) => !s.Contains("_version_"));

        List<string> archives = [];
        Contexts = [];
        
        foreach (string resdesc in resdescs)
        {
            ResourceContext? rc;
            try
            {
                 rc = await TttkInit.Workspace.LoadResourceDescriptionAsync(resdesc);
            }
            catch (Exception)
            {
                await cts.CancelAsync();
                Profiler.Instance.EndFrame(loadFrame);
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

        Profiler.Instance.EndFrame(loadFrame, out TimeSpan length);
        
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        await Console.Out.WriteLineAsync("\tLoading game resources took: " + length);
        Console.ResetColor();
        
        AsyncSearchForSkeletonFiles.BuildDictionaryTask = Task.Run(() => AsyncSearchForSkeletonFiles.BuildAgentMeshDictionary(ResourceLoader.Instance));
        
        return archives.Where(s => !string.IsNullOrEmpty(s)).ToList();
    }

    //todo: remove logic for loading a single archive without the rest of game data
    // because we're dropping that functionality
    private async void LoadArchive(MainWindow window)
    {
        try
        {
            // var startLoadTime = DateTime.Now;
            Profiler.Instance.BeginFrame("Archive Loading", out Profiler.ProfilerFrame loadFrame);
            
            AsyncSearchForSkeletonFiles.BuildDictionaryTask ??= Task.Run(() =>
                AsyncSearchForSkeletonFiles.BuildAgentMeshDictionary(ResourceLoader.Instance));
        
            lock(ResourceLock)
            {
                if (Contexts == null)
                    Contexts = [];
                else if (Contexts.Count != 0)
                {
                    Profiler.Instance.EndFrame(loadFrame, out TimeSpan length1);
        
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Out.WriteLine("\tLoading game resources took: " + length1);
                    Console.ResetColor();

                    return;
                }

                Instance.ArchiveContext = TttkInit.Workspace?.LoadArchive(ArchiveLocation, "Current Archive");

                if (Instance.ArchiveContext != null)
                    Contexts.Add(Instance.ArchiveContext);
            
                ArchiveLocationLock.Exit();

            }
        
            Profiler.Instance.EndFrame(loadFrame, out TimeSpan length);
        
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            await Console.Out.WriteLineAsync("\tLoading game resources took: " + length);
            Console.ResetColor();
        }
        catch (Exception)
        {
            //ignored
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
    
    public (string Name, IEnumerable<ResourceEntry>) GetEntriesInArchive(string archivePath)
    {
        lock(ResourceLock)
        {
            ArchiveLocationLock.Enter();
            
            ResourceContext? ctx = GetContextWithArchive(archivePath);

            if (ctx == null)
                return ("", []);
            
            ArchiveLocationLock.Exit();

            return (ctx.Name, ctx.GetAllEntries());
        }
    }

    public Stream? ExtractFile(string file)
    {
        lock(ResourceLock)
        {
            return TttkInit.Workspace?.ExtractFile(file);
        }
    }

    public (string Name, IEnumerable<ResourceEntry>) GetEntriesInCurrentArchive()
    {
        lock(ResourceLock)
        {
            return GetEntriesInArchive(ArchiveLocation);
        }
    }

    public Dictionary<string, IEnumerable<ResourceEntry>> GetEntriesInCurrentContexts()
    {
        lock (ResourceLock)
        {
            if (Contexts == null)
            {
                return [];
            }
            

            Dictionary<string, IEnumerable<ResourceEntry>> entryDict = [];

            foreach (ResourceContext context in Contexts)
            {
                entryDict[context.Name] = context.GetAllEntries();
            }

            return entryDict;
        }
    }
}