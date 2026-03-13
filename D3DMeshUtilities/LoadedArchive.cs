using System.IO;
using System.Text;
using System.Windows.Threading;
using D3DMeshUtilities.Code;
using TelltaleToolKit;
using TelltaleToolKit.Resource;
using TelltaleToolKit.TelltaleArchives;

namespace D3DMeshUtilities;

public class LoadedArchive
{
    public ArchiveBase? CurrentArchive = null;

    public List<ResourceContext>? Contexts
    {
        get;
        private set;
    } = null;

    public static LoadedArchive Instance = new LoadedArchive();

    private string _archiveLocation;

    public static Lock ArchiveLocationLock = new Lock();

    public static Lock ResourceContextLock = new Lock();

    public static bool SetArchive = false;

    public async void LoadArchive(Dispatcher dispatcher, string archiveLocation, string game)
    {

        if (TttkInit.Instance.Workspace == null)
        {
            TttkInit.Instance.Workspace = Toolkit.Instance.CreateWorkspace("D3DMeshUtilsWorkspace",
                game);
            
        }

        _archiveLocation = archiveLocation;
        
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
        
        Instance.CurrentArchive = TttkInit.Instance.Workspace?.LoadArchive(_archiveLocation, debugPrint: true);

        ArchiveLocationLock.Exit();

    }
    
}