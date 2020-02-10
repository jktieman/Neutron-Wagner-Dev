using NeutronCore.Extensions;
using NeutronData.ModelViews;
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
    public partial class FrmChangeStoreQuantity : Form
    {
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;
        public int NewQty { get; set; }
        public int Position { get; set; }
        private int InitialQuantityToBePicked { get; set; }
        private ReplenPickStop pickStop;
        public FrmChangeStoreQuantity( ReplenPickStop pickStop)
        {
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            this.pickStop = pickStop;
            foreach (var pickView in pickStop.PickViews)
            {
                if (pickView.GetQuantityToBePicked() > 0)
                {
                    Position = pickView.PickPosition;
                    TextBoxNewQuantity.Text = pickView.GetQuantityToBePicked().ToString();
                    TextBoxNewQuantity.SelectAll();
                    TextBoxNewQuantity.Focus();
                }
            }
            TextBoxChangeQuantityPosition.Text = Position.ToString();
        }

        private void TextBoxChangeQuantityPosition_Leave(object sender, EventArgs e)
        {
            if (CheckForValidPosition(TextBoxChangeQuantityPosition.Text))
            {
                TextBoxNewQuantity.SelectAll();
                TextBoxNewQuantity.Focus();
            }
            else
            {
                TextBoxChangeQuantityPosition.SelectAll();
                TextBoxChangeQuantityPosition.Focus();
            }
        }

        private bool CheckForValidPosition(string position)
        {

            int pos = position.ParseInt();
            foreach (var pickView in pickStop.PickViews)
            {
                if (pickView.PickPosition == pos)
                {
                    Position = pos;
                    return true;
                }
            }
            MessageBox.Show("Invalid Position Number.");
            return false;
        }

        private void TextBoxNewQuantity_Leave(object sender, EventArgs e)
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
            bool valid = int.TryParse(qty, out value);
            if (valid)
            {
                NewQty = value;
                return true;
            }
            return false;
        }

        private void MBChangeQuantityCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MBChangeQuantitySave_Click(object sender, EventArgs e)
        {
            NewQty = TextBoxNewQuantity.Text.ParseInt();
            Position = TextBoxChangeQuantityPosition.Text.ParseInt();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void SetCulture(string lang)
        {
            try
            {
                var languageDirectory = LoaderSettings.GetLanguageDirectory();
                _cultureInfo = CultureInfo.CreateSpecificCulture(lang);
                _resourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "FrmChangeStoreQuantity",
                    resourceDir: languageDirectory, usingResourceSet: null);
                LabelChangeQuantityPosition.Text = _resourceManager.GetString("PickPosition");
                MBChangeQuantityCancel.Text = _resourceManager.GetString("Cancel");
                MBChangeQuantitySave.Text = _resourceManager.GetString("Save");
                LabelNewQuantity.Text = _resourceManager.GetString("NewQuantity");
                this.Text = _resourceManager.GetString("ChangeQuantity");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading language file.  {ex.Message} {Environment.NewLine} {ex.InnerException} ");
            }
        }
    }
}
