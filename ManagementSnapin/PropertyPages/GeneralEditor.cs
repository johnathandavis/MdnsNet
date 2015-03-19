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
    public partial class GeneralEditor : UserControl
    {
        public GeneralEditor()
        {
            InitializeComponent();
        }
    }

    public class GeneralRecordPropertyPage : PropertyPage
    {
        public GeneralRecordPropertyPage()
        {
            this.Title = "General";
            this.Control = new GeneralEditor();
        }
    }
}
