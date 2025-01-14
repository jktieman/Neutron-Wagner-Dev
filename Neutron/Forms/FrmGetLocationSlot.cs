using NeutronData.ModelViews;
using SlotNameFactory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Neutron.Forms
{
    public partial class FrmGetLocationSlot : Form
    {
        private readonly BindingSource _newLocationBindingSource;
        public string SlotNumber = string.Empty;
        public LocationView Rec;
        public FrmGetLocationSlot(BindingSource newLocationBindingSource)
        {
            InitializeComponent();
            _newLocationBindingSource = newLocationBindingSource;            
            TextBoxSlotNumber.Focus();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            SlotNumber = TextBoxSlotNumber.Text.Trim().ToUpper();

            // get a record from _newLocationBindingSource that matches the slot
            Rec = _newLocationBindingSource.List.Cast<LocationView>().FirstOrDefault(r => r.Slot.ToUpper() == SlotNumber);
            if (Rec == null)
            {
                MessageBox.Show("No record found for the specified slot.");
                TextBoxSlotNumber.Focus();
                TextBoxSlotNumber.SelectAll();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
