using System.Text;
using System.ComponentModel;
using System.Windows.Forms;
using System.Security.Permissions;

using Microsoft.ManagementConsole;
using MdnsNet;

namespace ManagementSnapin
{
    public class RecordMmcListView : MmcListView
    {
        public RecordMmcListView()
        {
            
        }

        public ManagementClient Client
        {
            get
            {
                return (ManagementClient)this.ViewDescriptionTag;
            }
        }

        protected override void OnInitialize(AsyncStatus status)
        {
            // do default handling
            base.OnInitialize(status);

            // Create a set of columns for use in the list view
            // Define the default column title
            this.Columns[0].Title = "Domain";
            this.Columns[0].SetWidth(200);

            this.Columns.Add(new MmcListViewColumn("Name", 350));
            this.Columns.Add(new MmcListViewColumn("IP", 200));
            this.Columns.Add(new MmcListViewColumn("Port", 50));
            this.Columns.Add(new MmcListViewColumn("TXT Count", 75));
            this.Mode = MmcListViewMode.Report;  // default (set for clarity)

            // Set to show refresh as an option
            this.SelectionData.EnabledStandardVerbs = StandardVerbs.Refresh | StandardVerbs.Properties;
            
            // Load the list with values
            Refresh();
        }

        protected override void OnSelectionChanged(SyncStatus status)
        {
            if (this.SelectedNodes.Count == 0)
            {
                this.SelectionData.Clear();
            }
            else
            {
                this.SelectionData.Update(GetSelectedUsers(), this.SelectedNodes.Count > 1, null, null);
                this.SelectionData.ActionsPaneItems.Clear();
                this.SelectionData.ActionsPaneItems.Add(new Action("Delete Record", "Remove this record from the current MDNS server.", -1, "DeleteRecord"));
            }
        }

        protected override void OnSelectionAction(Action action, AsyncStatus status)
        {
            switch ((string)action.Tag)
            {
                case "DeleteRecord":
                    DeleteRecord();
                    break;
            }
        }



        /// <summary>
        /// Shows selected items.
        /// </summary>
        private void DeleteRecord()
        {
            var result = MessageBox.Show("Are you sure you want to delete the record " + this.SelectedNodes[0].DisplayName + "?", "Confirm Record Delete", MessageBoxButtons.YesNoCancel);
        }

        private string GetSelectedUsers()
        {
            StringBuilder selectedUsers = new StringBuilder();

            foreach (ResultNode resultNode in this.SelectedNodes)
            {

                selectedUsers.Append(resultNode.DisplayName + "\n");
            }

            return selectedUsers.ToString();
        }

        public void Refresh()
        {
            // Clear existing information.
            this.ResultNodes.Clear();

            // Use fictitious data to populate the lists.
            var records = Client.GetAllRecords();

            // Populate the list.
            foreach (var record in records)
            {
                ResultNode node = new ResultNode();
                node.DisplayName = record.Domain;
                node.SubItemDisplayNames.Add(record.Name);
                node.SubItemDisplayNames.Add(record.IP.ToString());
                node.SubItemDisplayNames.Add(record.Port.ToString());
                node.SubItemDisplayNames.Add(record.TxtRecords.Count.ToString());

                this.ResultNodes.Add(node);
            }
        }

        protected override void OnAddPropertyPages(PropertyPageCollection propertyPageCollection)
        {
            if (this.SelectedNodes.Count == 0)
            {
                // Nothing
            }
            else
            {
                propertyPageCollection.Add(new PropertyPages.GeneralRecordPropertyPage());
                propertyPageCollection.Add(new PropertyPages.ARecordPropertyPage());
                propertyPageCollection.Add(new PropertyPages.SrvRecordPropertyPage());
                propertyPageCollection.Add(new PropertyPages.TxtRecordPropertyPage());
            }
        }
    }
}
