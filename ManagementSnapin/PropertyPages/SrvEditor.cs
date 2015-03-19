using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.ManagementConsole;

namespace ManagementSnapin.PropertyPages
{
    public partial class SrvEditor : UserControl
    {
        public SrvEditor()
        {
            InitializeComponent();
        }
    }

    public class SrvRecordPropertyPage : PropertyPage
    {
        public SrvRecordPropertyPage()
        {
            this.Title = "Service";
            this.Control = new SrvEditor();
        }
    }
}
