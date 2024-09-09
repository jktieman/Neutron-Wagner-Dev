using NeutronCore.Extensions;
using System;
using System.Globalization;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using NeutronCore;
using NeutronCore.Global;

namespace Neutron.Forms
{
    public partial class FrmReprint : Form
    {
        private readonly int _position;
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;
        public PrintData printData;

        public FrmReprint(NeutronVariables neutronVariables, int position)
        {
            _position = position;
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            CheckBoxDocument.Visible = neutronVariables.EnableDocumentPrinter;
            TextBoxReprintPosition.Text = position.ToString();
        }

        private void MBReprintPrint_Click(object sender, EventArgs e)
        {
            printData = new PrintData
            {
                Position = TextBoxReprintPosition.Text.ParseInt(),
                PrintDocument = CheckBoxDocument.Checked,
                PrintToteLabel = CheckBoxToteLabel.Checked
            };
            DialogResult = DialogResult.OK;
            Close();
        }

        private void MBReprintCancel_Click(object sender, EventArgs e)
        {
            Close();
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
                Text = _resourceManager.GetString("FrmReprint");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading language file.  { ex.Message} { Environment.NewLine} { ex.InnerException} ");
            }
        }

    }
}
