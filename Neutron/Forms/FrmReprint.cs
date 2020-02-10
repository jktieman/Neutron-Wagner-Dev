using NeutronCore.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Neutron.Forms
{
    public partial class FrmReprint : Form
    {
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;
        public PrintData printData;

        public FrmReprint()
        {
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            // SetCulture(_cultureInfo.Name);
        }

        private void MBReprintPrint_Click(object sender, EventArgs e)
        {
            printData = new Forms.FrmReprint.PrintData();
            printData.Position = TextBoxReprintPosition.Text.ParseInt();
            printData.PrintDocument = CheckBoxDocument.Checked;
            printData.PrintToteLabel = CheckBoxToteLabel.Checked;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void MBReprintCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public class PrintData
        {
            public int Position { get; set; }
            public bool PrintDocument { get; set; }
            public bool PrintToteLabel { get; set; }
        }
    }
}
