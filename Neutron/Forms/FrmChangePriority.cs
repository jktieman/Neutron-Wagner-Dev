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
    public partial class FrmChangePriority : Form
    {
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;

        public FrmChangePriority()
        {
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
        }

        public string NewPriority { get; set; }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            this.NewPriority = TextBoxNewPriority.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SetCulture(string lang)
        {
            try
            {
                var languageDirectory = LoaderSettings.GetLanguageDirectory();
                _cultureInfo = CultureInfo.CreateSpecificCulture(lang);
                _resourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "FrmChangePriority",
                    resourceDir: languageDirectory, usingResourceSet: null);
                ButtonCancel.Text = _resourceManager.GetString("Cancel");
                ButtonOk.Text = _resourceManager.GetString("Ok");
                LabelNewPriority.Text = _resourceManager.GetString("NewPriority");
                this.Text = _resourceManager.GetString("ChangePriority");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading language file.  { ex.Message} { Environment.NewLine} { ex.InnerException} ");
            }
        }
    }
}

