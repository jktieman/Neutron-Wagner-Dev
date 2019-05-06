using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Neutron.Extensions;
using NeutronEvents;

namespace Neutron.Forms
{
    public partial class FrmCommunication : Form
    {
        public FrmCommunication()
        {
            InitializeComponent();
            Mediator.GetInstance().SerialPortWrite += (s, e) => ShowCommand(e.Request);
        }

        private void ShowCommand(string request)
        {
            ListBoxRequests.Invoke(new Action(() => ListBoxRequests.Items.Add(request)));
        }

        private void ButtonClear_Click(object sender, EventArgs e)
        {
            ListBoxRequests.Items.Clear();
        }

        private void ButtonClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
