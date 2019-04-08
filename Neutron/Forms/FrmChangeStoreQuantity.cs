using NeutronCore.Extensions;
using NeutronData.ModelViews;
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
    public partial class FrmChangeStoreQuantity : Form
    {
        public int NewQty { get; set; }
        public int Position { get; set; }
        private int InitialQuantityToBePicked { get; set; }
        private ReplenPickStop pickStop;
        public FrmChangeStoreQuantity( ReplenPickStop pickStop)
        {
            InitializeComponent();
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
    }
}
