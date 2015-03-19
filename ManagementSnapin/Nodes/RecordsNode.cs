using System;
using System.ComponentModel;
using System.Security.Permissions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.ManagementConsole;

namespace ManagementSnapin.Nodes
{
    public class RecordsNode : ScopeNode
    {
        public RecordsNode()
        {
            this.DisplayName = "Records";

            MmcListViewDescription lvd = new MmcListViewDescription();
            lvd.DisplayName = "Records";
            lvd.ViewType = typeof(RecordMmcListView);
            lvd.Options = MmcListViewOptions.ExcludeScopeNodes;

            this.ViewDescriptions.Add(lvd);
            this.ViewDescriptions.DefaultIndex = 0;
        }
    }
}
