using System;
using System.Runtime.CompilerServices;
using TelltaleToolKit;
using Path = System.IO.Path;

namespace D3DMeshUtilities;

public class TttkInit
{
    // Set up the context from a folder.

    public static TttkInit Instance = new ();

    public Workspace? Workspace;
    
    public static string DataDir = "";
    
    // Toolkit.Initialize("hello!");

    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    public void InternalInit() { }

    public TttkInit()
    {
        DataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");

        Toolkit.Configuration config = new Toolkit.Configuration();

        
        config.DataFolder = DataDir;
        Console.Out.WriteLine($"Looking for Telltale Tool Kit data directory at {DataDir}");
        
        Toolkit.Initialize(config);
        
    }

    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    public static void Init() { Instance.InternalInit(); }


}