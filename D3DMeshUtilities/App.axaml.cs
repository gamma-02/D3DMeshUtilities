using System;
using System.IO;
using System.Text;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace D3DMeshUtilities;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static void ProcessArgs(string[] args)
    {
        foreach (string arg in args)
        {
            if (arg.StartsWith("-gameDir="))
            {
                string dir = arg[9..];

                if (Directory.Exists(dir))
                {
                    StartupGameArchivesDirectory = dir;
                }
            }
            else if (arg.Equals("-autoLoad") || arg.Equals("-al"))
            {
                StartupLoadGameDir = false;
            }
            else if (arg.StartsWith("-game="))
            {
                string game = arg.Substring(6);
                
                StartupGameName = game;


                // if (Enum.TryParse<T3BlowfishKey>(game[..], true, out var key))
                // {
                // }
                // else if (int.TryParse(game[..], out var index) && index >= 0 &&
                //          index < Enum.GetValues<T3BlowfishKey>().Length)
                // {
                //     StartupGame = Enum.GetValues<T3BlowfishKey>()[index];
                // }
            }
            else if (arg.StartsWith("-archive="))
            {
                string archive = arg.Substring(9);

                StartupArchive = archive;
            }
            else if (arg.Equals("-autoChooseArchive") || arg.Equals("-aa"))
            {
                StartupChooseArchive = false;
            }
            else if (arg.StartsWith("-model="))
            {
                StartupModels = [arg.Substring(7)];
            }
            else if (arg.StartsWith("-m="))
            {
                StartupModels = [arg.Substring(3)];
            }
            else if (arg.StartsWith("-models="))
            {
                StartupModels = arg.Substring(8).Split(';');
            }
            else if (arg.StartsWith("-ms="))
            {
                StartupModels = arg.Substring(4).Split(';');
            }
            else if (arg.Equals("-autoChooseModels") || arg.Equals("-am"))
            {
                StartupChooseModels = false;
            }
            else if (arg.StartsWith("-out="))
            {
                string outDir = arg.Substring(5);

                if (Directory.Exists(outDir))
                {
                    StartupOutputDir = outDir;
                }
            }
            else if (arg.StartsWith("-o="))
            {
                string outDir = arg.Substring(3);

                if (Directory.Exists(outDir))
                {
                    StartupOutputDir = outDir;
                }
            }
            else if (arg.Equals("-autoConvert") || arg.Equals("-ac"))
            {
                StartupConvertModels = false;
            }
            else if (arg.Equals("-autoQuit") || arg.Equals("-aq"))
            {
                QuitAfterConvert = true;
            }
            else if (arg.StartsWith("-profileOut=") || arg.StartsWith("-po="))
            {
                PrintProfilerOutput = true;
                
                if (arg.Contains("print") && arg.Length is 17 or 9)
                    continue;

                int prefixLen = arg.StartsWith("-profileOut=") ? 12 : 4;

                ProfilerOutputFile = arg.Substring(prefixLen);
            }
            else if (arg.Equals("-dumpProfilerOnConversion") || arg.Equals("-dpoc"))
            {
                DumpOnConversionComplete = true;
            }
        }
    }

    public override void OnFrameworkInitializationCompleted()
    {
        
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            base.OnFrameworkInitializationCompleted();
            return;
        }
        
        desktop.MainWindow = new MainWindow();
    
        base.OnFrameworkInitializationCompleted();
    }
    
    //todo: find platform independent versions of this!
#if BUILT_FOR_WINDOWS
    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool AllocConsole();

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool FreeConsole();
#endif


    public static string? StartupGameArchivesDirectory { get; private set; }
    public static string? StartupGameName { get; private set; }
    public static string? StartupArchive { get; private set; }
    public static string[] StartupModels { get; private set; } = [];
    public static string? StartupOutputDir { get; private set; }

    public static bool StartupLoadGameDir
    {
        get
        {
            bool temp = field;
            field = false;
            return temp;
        }
        set;
    } = true;

    public static bool StartupChooseArchive {
        get
        {
            bool temp = field;
            field = false;
            return temp;
        }
        set; 
    } = true;
    public static bool StartupChooseModels {
        get
        {
            bool temp = field;
            field = false;
            return temp;
        }
        set; 
    } = true;
    public static bool StartupConvertModels { 
        get
        {
            bool temp = field;
            field = false;
            return temp;
        }
        set; 
    } = true;

    public static bool PrintProfilerOutput = false;
    public static string? ProfilerOutputFile = null;
    public static bool DumpOnConversionComplete = false;

    public static bool QuitAfterConvert { get; set; }

    public static void App_OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
#if BUILT_FOR_WINDOWS
        FreeConsole();
#endif
        
        DumpProfiler();
        
    }
    
    public static void DumpProfiler()
    {
        if (!PrintProfilerOutput) return;
        
        string profilerOutput = Profiler.Instance.GetResults();

        if (string.IsNullOrWhiteSpace(ProfilerOutputFile))
        {

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Out.WriteLine(profilerOutput);
            Console.ResetColor();
                
        }
        else
        {
            File.WriteAllBytes(ProfilerOutputFile!, Encoding.UTF8.GetBytes(profilerOutput));
        }

    }
}