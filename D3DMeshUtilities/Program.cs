using Avalonia;
using System;
using Avalonia.Controls.ApplicationLifetimes;

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
//-:cnd:noEmit
// #if DEBUG
            // .WithDeveloperTools()
// #endif
//+:cnd:noEmit
            .LogToTrace();
    
}