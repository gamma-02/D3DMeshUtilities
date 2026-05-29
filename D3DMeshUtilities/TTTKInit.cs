using System;
using System.IO;
using System.Runtime.CompilerServices;
using TelltaleToolKit;
using Path = System.IO.Path;

namespace D3DMeshUtilities;

public static class TttkInit
{
    // Set up the context from a folder.

    // public static TttkInit Instance = new ();

    public static Workspace? Workspace;
    
    public static string DataDir = "";
    
    // Toolkit.Initialize("hello!");

    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    private static void InternalInit()
    {
        DataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ttk-data");

        Toolkit.Configuration config = new Toolkit.Configuration();

        
        config.DataFolder = DataDir;
        Console.Out.WriteLine($"Looking for Telltale Tool Kit data directory at {DataDir}");

        if (!Directory.Exists(DataDir))
        {
            Console.Out.WriteLine("Could not find data directory!");
        }
        
        Toolkit.Initialize(config);

    }


    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    public static void Init() { InternalInit(); }


}