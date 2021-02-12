using NeutronCore.Extensions;
using NeutronData.ModelViews;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using NeutronCore;

namespace Neutron.Forms
{
    public partial class FrmChangeQuantity : Form
    {
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;
        public int NewQty { get; set; }
        public int Position { get; set; }
        private readonly PickStop _pickStop;
        private ReplenPickStop _replenPickStop;
        private List<int> _validPositions = new List<int>();

        public FrmChangeQuantity(PickStop pickStop)
        {
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            _pickStop = pickStop;
            GetValidPositions(_pickStop.PickViews);
            var pos = pickStop.PickViews.First().PickPosition;
            TextBoxChangeQuantityPosition.Text = pos.ToString();
            TextBoxNewQuantity.Text = "0";
            TextBoxChangeQuantityPosition.SelectAll();
            TextBoxChangeQuantityPosition.Focus();
        }

        private void GetValidPositions(List<PickView> pickviews)
        {
            _validPositions.Clear();
            foreach (var view in pickviews)
            {
                _validPositions.Add(view.PickPosition);
            }
        }
        private void GetValidPositions(List<ReplenPickView> pickviews)
        {
            _validPositions.Clear();
            foreach (var view in pickviews)
            {
                _validPositions.Add(view.PickPosition);
            }
        }

        public FrmChangeQuantity(ReplenPickStop pickStop)
        {
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            _replenPickStop = pickStop;
            GetValidPositions(_replenPickStop.PickViews);
            var pos = pickStop.PickViews.First().PickPosition;
            TextBoxChangeQuantityPosition.Text = pos.ToString();
            TextBoxNewQuantity.Text = "0";
            TextBoxChangeQuantityPosition.SelectAll();
            TextBoxChangeQuantityPosition.Focus();
        }

        public FrmChangeQuantity()
        {
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            TextBoxChangeQuantityPosition.Visible = false;
            LabelChangeQuantityPosition.Visible = false;
            TextBoxNewQuantity.Text = "0";
            TextBoxChangeQuantityPosition.SelectAll();
            TextBoxChangeQuantityPosition.Focus();
        }

        private void TextBoxChangeQuantityPosition_TextChanged(object sender, EventArgs e)
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
            return _validPositions.Contains(position.ParseInt());
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
            MessageBox.Show(_resourceManager.GetString("Message1"));
            return false;
        }

        private void MBChangeQuantityCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MBChangeQuantitySave_Click(object sender, EventArgs e)
        {
            if (CheckForValidNumber(TextBoxNewQuantity.Text) &&
                CheckForValidPosition(TextBoxChangeQuantityPosition.Text))
            {
                NewQty = TextBoxNewQuantity.Text.ParseInt();
                Position = TextBoxChangeQuantityPosition.Text.ParseInt();
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
                _resourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "FrmChangeQuantity",
                    resourceDir: languageDirectory, usingResourceSet: null);
                LabelChangeQuantityPosition.Text = _resourceManager.GetString("PickPosition");
                MBChangeQuantityCancel.Text = _resourceManager.GetString("Cancel");
                MBChangeQuantitySave.Text = _resourceManager.GetString("Save");
                LabelNewQuantity.Text = _resourceManager.GetString("NewQuantity");
                Text = _resourceManager.GetString("ChangeQuantity");

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading language file.  { ex.Message} { Environment.NewLine} { ex.InnerException} ");
            }
        }

    }
}

