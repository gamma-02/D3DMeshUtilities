using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using TelltaleToolKit;
using TelltaleToolKit.Utility.Blowfish;

namespace D3DMeshUtilities;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool AllocConsole();
    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool FreeConsole();

    public static string? StartupGameArchivesDirectory { get; private set; }
    public static T3BlowfishKey? StartupGame { get; private set; }
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

    protected override void OnExit(ExitEventArgs e)
    {
        FreeConsole();
        base.OnExit(e);
    }

    private void App_OnStartup(object sender, StartupEventArgs e)
    {
        foreach (string arg in e.Args)
        {
            if (arg.StartsWith("-gameDir="))
            {
                string dir = arg.Substring(9);

                if (Directory.Exists(dir))
                {
                    StartupGameArchivesDirectory = dir;
                }
            }
            else if (arg.Equals("-autoLoad=false") || arg.Equals("-al=false"))
            {
                StartupLoadGameDir = false;
            }
            else if (arg.StartsWith("-game="))
            {
                string game = arg.Substring(6);

                if (Enum.TryParse<T3BlowfishKey>(game[..], true, out var key))
                {
                    StartupGame = key;
                }
                else if (int.TryParse(game[..], out var index) && index >= 0 && index < Enum.GetValues<T3BlowfishKey>().Length)
                {
                    StartupGame = Enum.GetValues<T3BlowfishKey>()[index];
                }
            }
            else if (arg.StartsWith("-archive="))
            {
                string archive = arg.Substring(9);

                StartupArchive = archive;
            }
            else if (arg.Equals("-autoChooseArchive=false") || arg.Equals("-ac=false"))
            {
                StartupChooseArchive = false;
            }
            else if (arg.StartsWith("-model="))
            {
                StartupModels = [arg.Substring(7)];
            }
            else if (arg.StartsWith("-models="))
            {
                StartupModels = arg.Substring(8).Split(';');
            }
            else if (arg.Equals("-autoChooseModels=false") || arg.Equals("-am=false"))
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
            else if (arg.Equals("-autoConvert=false") || arg.Equals("-ao=false"))
            {
                StartupConvertModels = false;
            }
        }
    }
}