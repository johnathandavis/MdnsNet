using Microsoft.ManagementConsole;
using System.ComponentModel;
using System;
using System.Security.Permissions;

using MdnsNet;

namespace ManagementSnapin.Nodes
{
    public class RootNode : ScopeNode
    {
        public RootNode()
        {
            DisplayName = Snapin.SNAPIN_NAME;

            var client = new ManagementClient(Environment.MachineName);
            this.Children.Add(new Nodes.RecordsNode(client));
        }

    }
}
