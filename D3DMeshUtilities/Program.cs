using Avalonia;
using System;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using D3DMeshUtilities.Code;
using D3DMeshUtilities.Code.ImageStuffAUGH;
using SharpGLTF.Schema2;
using Codecs = D3DMeshUtilities.Code.MeshHandling.Codecs;


namespace D3DMeshUtilities;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        
        App.ProcessArgs(args);
        
        //todo: process a lot of the startup args relating to default folder loading here
        //todo: allow for game switching while program is running
        TttkInit.Init();
        Task.Run(Codecs.RegisterCodecs);
        
        
        
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args, builderer);
    }

    public static Action<IClassicDesktopStyleApplicationLifetime> builderer = lifetime =>
    {
        lifetime.Exit += App.App_OnExit;
    } ;

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithDeveloperTools()
//-:cnd:noEmit
// #if DEBUG
//             .WithDeveloperTools()
// #endif
//+:cnd:noEmit
            .LogToTrace();
    
}