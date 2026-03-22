using TelltaleToolKit;
using Path = System.IO.Path;

namespace D3DMeshUtilities;

public class TttkInit
{
    // Set up the context from a folder.

    public static TttkInit Instance = new TttkInit();

    public Workspace? Workspace;
    
    public static string DataDir;
    
    // Toolkit.Initialize("hello!");

    public TttkInit()
    {

        DataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");

        Toolkit.Configuration config = new Toolkit.Configuration();

        
        config.DataFolder = DataDir;
        Console.Out.WriteLine($"Looking for Telltale Tool Kit data directory at {DataDir}");
        
        
        Toolkit.Initialize(config);
        

    }


}