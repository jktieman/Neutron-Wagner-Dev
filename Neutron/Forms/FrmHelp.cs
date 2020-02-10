using MetroFramework.Forms;
using Neutron.Global;
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
    public partial class FrmHelp : MetroForm
    {
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;
        public bool CloseButtonPressed { get; set; }

        public FrmHelp()
        {
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            mlUserInfo.Text = GlobalVar.User?.UserInfo;
            CloseButtonPressed = false;
        }

        private void MBMainClose_Click(object sender, EventArgs e)
        {
            CloseButtonPressed = true;
        }

        private void FrmHelp_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !CloseButtonPressed;
        }

        private void SetCulture(string lang)
        {
            try
            {
                var languageDirectory = LoaderSettings.GetLanguageDirectory();
                _cultureInfo = CultureInfo.CreateSpecificCulture(lang);
                _resourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "FrmHelp",
                    resourceDir: languageDirectory, usingResourceSet: null);
                LabelFormTitle.Text = _resourceManager.GetString("Help");
                mlUserInfo.Text = _resourceManager.GetString("Login");
                LabelFormHeaderText.Text = _resourceManager.GetString("NeutronWarehouseManagement");
                MBMainClose.Text = _resourceManager.GetString("Home");
                this.Text = _resourceManager.GetString("Help");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading language file.  { ex.Message} { Environment.NewLine} { ex.InnerException} ");
            }
        }

    }
}
