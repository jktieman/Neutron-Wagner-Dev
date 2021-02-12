using System;
using System.Globalization;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using NeutronCore;

namespace Neutron.Forms
{
    public partial class FrmLocationCount : Form
    {
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;
        public string NewQty { get; set; }

        public FrmLocationCount()
        {
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            NewQty = TextBoxNewQuantity.Text;
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
                _resourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "FrmLocationCount",
                    resourceDir: languageDirectory, usingResourceSet: null);
                ButtonCancel.Text = _resourceManager.GetString("Cancel");
                ButtonOk.Text = _resourceManager.GetString("Ok");
                LabelNewQuantity.Text = _resourceManager.GetString("NewQuantity");
                Text = _resourceManager.GetString("LocationCount");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading language file.  { ex.Message} { Environment.NewLine} { ex.InnerException} ");
            }
        }

    }
}
