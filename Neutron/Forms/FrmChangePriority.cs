using System;
using System.Globalization;
using System.Resources;
using System.Threading;
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
            NewPriority = TextBoxNewPriority.Text;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
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
                ButtonCancel.Text = _resourceManager.GetString("Cancel");
                ButtonOk.Text = _resourceManager.GetString("Ok");
                LabelNewPriority.Text = _resourceManager.GetString("NewPriority");
                Text = _resourceManager.GetString("ChangePriority");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading language file.  { ex.Message} { Environment.NewLine} { ex.InnerException} ");
            }
        }
    }
}

