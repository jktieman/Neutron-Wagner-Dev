using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NeutronTestForm
{
    public partial class FrmNeutronTest : Form
    {
        public FrmNeutronTest()
        {
            InitializeComponent();
        }

        private void ButtonExcelTesting_Click(object sender, EventArgs e)
        {
            Hide();
            using (var frm = new FrmExcelTesting())
            {
                frm.ShowDialog();
                Show();
            }
        }
    }
}
