using Microsoft.ManagementConsole;
using System.ComponentModel;
using System;
using System.Security.Permissions;

namespace ManagementSnapin
{
    [SnapInSettings("{B6E774B5-47B9-409C-9A59-4BFE0B6FF1C7}",
                    DisplayName = Snapin.SNAPIN_NAME,
                    Description = Snapin.SNAPIN_DESCRIPTION)]
    public class Snapin : SnapIn
    {
        public const string SNAPIN_NAME = "MDNS Server";
        public const string SNAPIN_DESCRIPTION = "Manage the installed MDNS Server service on your local computer.";


        public Snapin()
        {
            this.RootNode = new Nodes.RootNode();            
        }
    }
}
