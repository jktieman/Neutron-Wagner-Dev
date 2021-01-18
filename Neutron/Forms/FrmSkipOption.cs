using System;
using System.Windows.Forms;
using NeutronData.ModelViews;

namespace Neutron.Forms
{
    public partial class FrmSkipOption : Form
    {

        public string Pushed { get; set; }
        public string NewQty { get; set; }

        public FrmSkipOption(PickView pickView)
        {
            InitializeComponent();
            Pushed = string.Empty;

            TextBoxOrd1.Text = pickView.Ord1;
            TextBoxOrd2.Text = pickView.Ord2;
            TextBoxItem.Text = pickView.Item;
            TextBoxDescription.Text = pickView.Description;

            TextBoxSkipOptions.Text = "There are no locations with the item listed above.\r\nThere are 2 choices.\r\n Skip will allow you to continue picking this order and come back later to complete the order.\r\nPick Zero will record the item as picked with zero quantity.";
        }

        private void MBSkip_Click(object sender, EventArgs e)
        {
            NewQty = "Skip";
            Pushed = "Skip";
            DialogResult = DialogResult.OK;
            Close();
        }

        private void MBPickZero_Click(object sender, EventArgs e)
        {
            Pushed = "Pick Zero";
            DialogResult = DialogResult.OK;
            Close();
        }


    }
}
