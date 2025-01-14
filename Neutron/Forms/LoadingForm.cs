using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Neutron.Forms
{
    public class LoadingForm : Form
    {
        public LoadingForm()
        {
            this.Text = "Please Wait";
            this.Size = new System.Drawing.Size(200, 100);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.ControlBox = false; // Disable close button
            this.TopMost = true;
            var loadingLabel = new Label
            {
                Text = "Loading...",
                AutoSize = true,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            this.Controls.Add(loadingLabel);
        }
    }
}

