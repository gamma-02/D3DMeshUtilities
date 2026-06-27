using System.Collections.Generic;
using TelltaleToolKit;
using TelltaleToolKit.T3Types;
using TelltaleToolKit.T3Types.Properties;

namespace D3DMeshUtilities.Code;

public static class PropertySetHelpers
{

    public static List<PropertySet> GetParents(this PropertySet set, Workspace workspace)
    {
        List<PropertySet> parents = [];

        foreach (Handle<PropertySet> parentHandle in set.ParentList)
        {
            PropertySet? parent = parentHandle.GetObject<PropertySet>(workspace);
            
            if(parent == null) continue;
            
            parents.Add(parent);
        }

        return parents;
    }
}