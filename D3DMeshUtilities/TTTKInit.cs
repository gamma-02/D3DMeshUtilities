using TelltaleToolKit;
using Path = System.IO.Path;

namespace D3DMeshUtilities;

public class TttkInit
{
    // Set up the context from a folder.

    public static TttkInit Instance = new TttkInit();

    public Workspace? Workspace;
    
    // Toolkit.Initialize("hello!");

    public TttkInit()
    {

        Toolkit.Configuration config = new Toolkit.Configuration();

        string dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
        config.DataFolder = Path.Combine(dataDir);
        Console.Out.WriteLine($"Looking for Telltale Tool Kit data directory at {dataDir}");
        
        
        Toolkit.Initialize(config);
        

    }


}