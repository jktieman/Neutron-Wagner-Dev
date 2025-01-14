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
    public partial class FrmTaskNumber : Form
    {
        public string TaskNumber { get; set; }
        public FrmTaskNumber(List<string> taskNumbers)
        {
            InitializeComponent();
            // set the combobox data source
            this.ComboBoxTaskNumbers.DataSource = taskNumbers;

        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            TaskNumber = ComboBoxTaskNumbers.SelectedItem.ToString();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
