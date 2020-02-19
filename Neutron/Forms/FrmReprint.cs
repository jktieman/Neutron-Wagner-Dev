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
using NeutronCore;

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
            SetCulture(_cultureInfo.Name);
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

        private void SetCulture(string lang)
        {
            try
            {
                var languageDirectory = LoaderSettings.GetLanguageDirectory();
                _cultureInfo = CultureInfo.CreateSpecificCulture(lang);
                _resourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "FrmReprint",
                    resourceDir: languageDirectory, usingResourceSet: null);
                CheckBoxToteLabel.Text = _resourceManager.GetString("ToteLabel");
                CheckBoxDocument.Text = _resourceManager.GetString("PackingList");
                LabelChangeQuantityPosition.Text = _resourceManager.GetString("PickPosition");
                MBReprintCancel.Text = _resourceManager.GetString("Cancel");
                MBReprintPrint.Text = _resourceManager.GetString("Print");
                this.Text = _resourceManager.GetString("FrmReprint");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading language file.  { ex.Message} { Environment.NewLine} { ex.InnerException} ");
            }
        }

    }
}
