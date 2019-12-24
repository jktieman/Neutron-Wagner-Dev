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
    public partial class FrmChangeQuantity : Form
    {
        public int NewQty { get; set; }
        public int Position { get; set; }
        private readonly PickStop _pickStop;
        private ReplenPickStop _replenPickStop;

        public FrmChangeQuantity( PickStop pickStop)
        {
            InitializeComponent();
            this._pickStop = pickStop;
            var pos = pickStop.PickViews.First().PickPosition;
            TextBoxChangeQuantityPosition.Text = pos.ToString();
            TextBoxNewQuantity.Text = "0";
            TextBoxChangeQuantityPosition.SelectAll();
            TextBoxChangeQuantityPosition.Focus();
        }

        public FrmChangeQuantity(ReplenPickStop pickStop)
        {
            InitializeComponent();
            this._replenPickStop = pickStop;
            var pos = pickStop.PickViews.First().PickPosition;
            TextBoxChangeQuantityPosition.Text = pos.ToString();
            TextBoxNewQuantity.Text = "0";
            TextBoxChangeQuantityPosition.SelectAll();
            TextBoxChangeQuantityPosition.Focus();
        }

        public FrmChangeQuantity()
        {
            InitializeComponent();
            TextBoxChangeQuantityPosition.Visible = false;
            LabelChangeQuantityPosition.Visible = false;
            TextBoxNewQuantity.Text = "0";
            TextBoxChangeQuantityPosition.SelectAll();
            TextBoxChangeQuantityPosition.Focus();
        }

        //private bool ValidateForm()
        //{
        //    //if (CheckForValidPosition(Position.ToString()) && CheckForValidNumber(NewQty.ToString()))
        //    //{
        //    //    return true;
        //    //}
        //    //return false;
        //}

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
            foreach (var pickView in _pickStop.PickViews)
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
