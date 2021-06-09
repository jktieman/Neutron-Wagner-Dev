using System;
using System.Windows.Forms;
using NeutronCore.Enums;
using NeutronData.ModelViews;

namespace Neutron.Forms
{
    public partial class FrmPickViewAdjustment : Form
    {
        private readonly PickView _pickView;
        public bool Success = false;
        public string ButtonPressed = string.Empty;

        public FrmPickViewAdjustment(PickView pickView)
        {

            InitializeComponent();
            KeyPreview = true;

            _pickView = pickView;
            LabelOrder.Text = pickView.Ord1;
            LabelItem.Text = pickView.Item;
            LabelDescription.Text = pickView.Description;
            TextBoxPosition.Text = pickView.PickPosition.ToString();
            TextBoxNewQuantity.Text = pickView.QuantityToBePicked.ToString();
        }

        private void ButtonHighlight_Click(object sender, EventArgs e)
        {
            Highlight();
        }

        private void Highlight()
        {
            ButtonPressed = "Highlight";
            _pickView.OrderDetail.LineStatusId = (int)LineStatus.Skipped;
            _pickView.OrderDetail.PickedQuantity = 0;
            _pickView.PickedQty = 0;
            _pickView.QuantityToBePicked = _pickView.Quantity;
            Success = true;
            Close();
        }

        private void ButtonBackorder_Click(object sender, EventArgs e)
        {
            Backorder();
        }

        private void Backorder()
        {
            ButtonPressed = "Backorder";
            _pickView.OrderDetail.LineStatusId = (int)LineStatus.Complete;


            _pickView.OrderDetail.PickedQuantity = _pickView.PickedQty;
            //_pickView.PickedQty = 0;
           // _pickView.QuantityToBePicked = _pickView.Quantity;
            Success = true;
            Close();
        }

        private void ButtonAccept_Click(object sender, EventArgs e)
        {
            Accept();
        }

        private void Accept()
        {
            ButtonPressed = "Accept";
            _pickView.OrderDetail.LineStatusId = (int)LineStatus.Complete;
            _pickView.OrderDetail.PickedQuantity = _pickView.QuantityToBePicked;
            _pickView.PickedQty = _pickView.QuantityToBePicked;
            Success = true;
            Close();
        }

        private void FrmPickViewAdjustment_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    {
                        Highlight();
                        break;
                    }
                case Keys.Down:
                    {
                        Backorder();
                        break;
                    }
                case Keys.Right:
                    {
                        Accept();
                        break;
                    }
            }
        }
    }
}
