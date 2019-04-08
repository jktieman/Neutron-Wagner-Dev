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
    public partial class FrmChangePriority : Form
    {
        public FrmChangePriority()
        {
            InitializeComponent();
        }

        public string NewPriority { get; set; }


        private void ButtonOk_Click(object sender, EventArgs e)
        {
            this.NewPriority = TextBoxNewPriority.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
