using NeutronCore.Extensions;
using NeutronData.ModelViews;
using System;
using System.Globalization;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using NeutronCore;

namespace Neutron.Forms
{
    public partial class FrmChangeQuantityOnly : Form
    {
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;
        public int NewQty { get; set; }


        public FrmChangeQuantityOnly(PickStop pickStop)
        {
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            TextBoxNewQuantity.Text = "0";
        }

        public FrmChangeQuantityOnly(ReplenPickStop pickStop)
        {
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            TextBoxNewQuantity.Text = "0";

        }

        public FrmChangeQuantityOnly(SkipView currentSkip)
        {
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            TextBoxOrder.Text = currentSkip.Ord1;
            TextBoxReservation.Text = currentSkip.Ord2;
            TextBoxItem.Text = currentSkip.Item;
            TextBoxQuantity.Text = currentSkip.Quantity.ToString();
            TextBoxNewQuantity.Text = "0";
        }

      private void TextBoxNewQuantity_TextChanged(object sender, EventArgs e)
        {
            if (CheckForValidNumber(TextBoxNewQuantity.Text))
            {
                MBChangeQuantitySave.Focus();
            }
            else
            {
                TextBoxNewQuantity.SelectAll();
                TextBoxNewQuantity.Focus();
            }
        }

        private bool CheckForValidNumber(string qty)
        {
            int value;
            var valid = int.TryParse(qty, out value);
            if (valid)
            {
                NewQty = value;
                return true;
            }
            MessageBox.Show(_resourceManager.GetString("Message1"), "Valid Number", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            return false;
        }

        private void MBChangeQuantityCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void MBChangeQuantitySave_Click(object sender, EventArgs e)
        {
            if (CheckForValidNumber(TextBoxNewQuantity.Text))
            {
                NewQty = TextBoxNewQuantity.Text.ParseInt();
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void SetCulture(string lang)
        {
            try
            {
                var languageDirectory = LoaderSettings.GetLanguageDirectory();
                _cultureInfo = CultureInfo.CreateSpecificCulture(lang);
                _resourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "FrmChangeQuantityOnly",
                    resourceDir: languageDirectory, usingResourceSet: null);
                MBChangeQuantityCancel.Text = _resourceManager.GetString("Cancel");
                MBChangeQuantitySave.Text = _resourceManager.GetString("Save");
                LabelNewQuantity.Text = _resourceManager.GetString("NewQuantity");
                Text = _resourceManager.GetString("ChangeQuantity");

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading language file.  { ex.Message} { Environment.NewLine} { ex.InnerException} ", "Set Culture Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }

    }
}

