using NeutronCore;
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
    public partial class FrmQuantity : Form
    {
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;
        public string Quantity { get; set; }

        public FrmQuantity()
        {
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
        }
        private void ButtonOk_Click(object sender, EventArgs e)
        {
            Quantity = TextBoxQuantity.Text;
            DialogResult = DialogResult.OK;

            Close();
        }
        private void SetCulture(string lang)
        {
            try
            {
                var languageDirectory = LoaderSettings.GetLanguageDirectory();
                _cultureInfo = CultureInfo.CreateSpecificCulture(lang);
                _resourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "FrmChangePriority",
                    resourceDir: languageDirectory, usingResourceSet: null);
               
                ButtonOk.Text = _resourceManager.GetString("Ok");
                LabelQuantity.Text = _resourceManager.GetString("Quantity");
                Text = _resourceManager.GetString("EnterQuantity");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading language file.  {ex.Message} {Environment.NewLine} {ex.InnerException} ");
            }
        }


    }
}
