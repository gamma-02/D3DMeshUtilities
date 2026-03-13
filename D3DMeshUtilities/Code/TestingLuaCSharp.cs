using Lua;

namespace D3DMeshUtilities.Code;

public class TestingLuaCSharp
{
    private static List<LuaTable> _sets = new List<LuaTable>();
    
    private const string TEST_LUA = """ 
                                           local set = {}
                                           set.name = "Common"
                                           set.setName = "Common"
                                           set.descriptionFilenameOverride = ""
                                           set.logicalName = "<Common>"
                                           set.logicalDestination = "<>"
                                           set.priority = 100
                                           set.localDir = _currentDirectory
                                           set.enableMode = "constant"
                                           set.version = "trunk"
                                           set.descriptionPriority = 0
                                           set.gameDataName = "Common Game Data"
                                           set.gameDataPriority = 0
                                           set.gameDataEnableMode = "constant"
                                           set.localDirIncludeBase = true
                                           set.localDirRecurse = false
                                           set.localDirIncludeOnly = nil
                                           set.localDirExclude = 
                                           {
                                               "Packaging/",
                                               "_dev/"
                                           }
                                           set.gameDataArchives = 
                                           {
                                               _currentDirectory .. "CP_pc_Common_compressed.ttarch2",
                                               _currentDirectory .. "CP_pc_Common_uncompressed.ttarch2"
                                           }
                                           RegisterSetDescription(set)
                                           """;
    
    
    private static readonly LuaState state = LuaState.Create();

    public static async void TestLuaStuff()
    {
        try
        {

            state.Environment["RegisterSetDescription"] = new LuaFunction((context, ct) =>
            {
                var set = context.GetArgument<LuaTable>(0);
            
                _sets.Add(set);

                context.Return();
            
                return new (0);
            });

            state.Environment["_currentDirectory"] =
                ".\\";
        
            await state.DoStringAsync(TEST_LUA);
        
            LuaTable t = _sets[0];

            PrintLuaTable(t);
        }
        catch (Exception e)
        {
            Console.Write(e);
        }
    }
    
    private static void PrintLuaTable(LuaTable t, int padding = 0, bool recurse = true)
    {
        foreach (KeyValuePair<LuaValue, LuaValue> pair in t.ToArray())
        {
            
            string valRep = pair.ToString();
            Console.Out.WriteLine(valRep.PadLeft(valRep.Length + padding * 4));
            
            if (pair.Value.TryRead<LuaTable>(out var table) && recurse)
            {
                PrintLuaTable(table, padding + 1);
            }
            
        }
    }
}