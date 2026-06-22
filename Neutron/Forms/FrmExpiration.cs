using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NeutronData.Models;
using NeutronData.ModelViews;
using NeutronData.SqlModelViews;

namespace Neutron.Forms
{
    public partial class FrmExpiration : Form
    {
        private readonly SqlInventoryView _inventoryView;

        public string LotNumber => TextBoxLotNumber.Text.Trim();
        public DateTime ExpirationDate => DateTimePickerExpirationDate.Value;

        public FrmExpiration(SqlInventoryView inventoryView)
        {
            InitializeComponent();
            DateTimePickerExpirationDate.MinDate = DateTime.Today;
            _inventoryView = inventoryView ?? throw new ArgumentNullException(nameof(inventoryView));
            LabelItemText.Text = inventoryView.Item ?? string.Empty;
            LabelDescriptionText.Text = inventoryView.Description ?? string.Empty;
            TextBoxLotNumber.Text = inventoryView.LotNumber ?? string.Empty;
            DateTimePickerExpirationDate.Value = inventoryView.ExpirationDate ?? DateTime.Today;

            ButtonSave.Click += ButtonSave_Click;
            ButtonCancel.Click += ButtonCancel_Click;

            TextBoxLotNumber.Focus();
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            _inventoryView.LotNumber = LotNumber;
            _inventoryView.ExpirationDate = ExpirationDate;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
