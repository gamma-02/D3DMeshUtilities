using System.Configuration;
using System.Data;
using System.Windows;

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

    protected override void OnExit(ExitEventArgs e)
    {
        FreeConsole();
        base.OnExit(e);
    }
}