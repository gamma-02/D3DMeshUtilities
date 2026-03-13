using System.Windows;
using TelltaleToolKit;
using TelltaleToolKit.GamesDatabase;
using TelltaleToolKit.Serialization.Binary;
using TelltaleToolKit.T3Types.Textures;
using TelltaleToolKit.T3Types.Textures.T3Types;
using TelltaleToolKit.Utility;

namespace D3DMeshUtilities.Code;

public class TttkInit
{
    // Set up the context from a folder.

    public static TttkInit Instance = new TttkInit();

    public Workspace? Workspace;
    
    // Toolkit.Initialize("hello!");

    public TttkInit()
    {

        Toolkit.Configuration config = new Toolkit.Configuration();
        config.DataFolder = "./data";
        
        
        Toolkit.Initialize(config);
        

    }


}