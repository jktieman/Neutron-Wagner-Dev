using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Neutron.Forms
{
    public partial class FrmNewItemAuth : Form
    {
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;
        public string AuthCode = string.Empty;
        public FrmNewItemAuth()
        {
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            // SetCulture(_cultureInfo.Name);
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            this.AuthCode = TextBoxAuthCode.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
