using Microsoft.ManagementConsole;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ManagementSnapin.PropertyPages
{
    public partial class ARecordEditor : UserControl
    {
        public ARecordEditor()
        {
            InitializeComponent();
        }
    }

    public class ARecordPropertyPage : PropertyPage
    {
        public ARecordPropertyPage()
        {
            this.Title = "Host";
            this.Control = new ARecordEditor();
        }
    }
}
