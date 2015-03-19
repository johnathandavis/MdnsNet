using Microsoft.ManagementConsole;
using System.ComponentModel;
using System;
using System.Security.Permissions;

namespace ManagementSnapin.Nodes
{
    public class RootNode : ScopeNode
    {
        public RootNode()
        {
            DisplayName = Snapin.SNAPIN_NAME;

            
            this.Children.Add(new Nodes.RecordsNode());
        }

    }
}
