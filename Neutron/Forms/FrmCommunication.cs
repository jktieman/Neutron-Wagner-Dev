using System;
using System.Globalization;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using NeutronCore;
using NeutronEvents;

namespace Neutron.Forms
{
    public partial class FrmCommunication : Form
    {
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;

        public FrmCommunication()
        {
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            Mediator.GetInstance().SerialPortWrite += (s, e) => ShowCommand(e.Request);
        }

        private void ShowCommand(string request)
        {
            ListBoxRequests.Invoke(new Action(() => ListBoxRequests.Items.Add(request)));
        }

        private void ButtonClear_Click(object sender, EventArgs e)
        {
            ListBoxRequests.Items.Clear();
        }

        private void ButtonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void SetCulture(string lang)
        {
            try
            {
                var languageDirectory = LoaderSettings.GetLanguageDirectory();
                _cultureInfo = CultureInfo.CreateSpecificCulture(lang);
                _resourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "FrmCommunication",
                    resourceDir: languageDirectory, usingResourceSet: null);
                ButtonClear.Text = _resourceManager.GetString("Clear");
                ButtonClose.Text = _resourceManager.GetString("Close");
                Text = _resourceManager.GetString("SerialCommunication");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading language file.  { ex.Message} { Environment.NewLine} { ex.InnerException} ");
            }
        }
    }
}
