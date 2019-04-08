using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetroFramework.Controls;
using Neutron.Models;

namespace Neutron.UserControls
{
    public partial class ucDashboard : MetroUserControl
    {
        private List<RectangleControl> controls;

        private Size containerOrigSize;

        private bool initialLoad = true;

        public ucDashboard()
        {
            InitializeComponent();
            controls = new List<RectangleControl>();
        }

        private void ucDashboard_Load(object sender, EventArgs e)
        {
            containerOrigSize = this.Size;

            foreach (Control control in this.Controls)
            {
                var rectangle = new Rectangle(control.Location.X, control.Location.Y, control.Width, control.Height);
                var rc = new RectangleControl(rectangle, control);
                controls.Add(rc);
            }
            initialLoad = false;
        }

        private void mtNDefDb_Click(object sender, EventArgs e)
        {
            //if (!FrmMain.Instance.MetroContainer.Controls.ContainsKey("UcItemDefinition")) 
            //{
            //    var uc = new UcItemDefinition();
            //    uc.Dock = DockStyle.Fill;
            //    FrmMain.Instance.MetroContainer.Controls.Add(uc);
            //}
            //FrmMain.Instance.MetroContainer.Controls["UcItemDefinition"].BringToFront();
        }

        private void mtNLocDb_Click(object sender, EventArgs e)
        {
            //if (!FrmMain.Instance.MetroContainer.Controls.ContainsKey("ucNLocDb"))
            //{
            //    var uc = new ucNLocDb();
            //    uc.Dock = DockStyle.Fill;
            //    FrmMain.Instance.MetroContainer.Controls.Add(uc);
            //}
            //FrmMain.Instance.MetroContainer.Controls["ucNLocDb"].BringToFront();
        }

        //private void mtNova_Click(object sender, EventArgs e)
        //{
        //    if (!FrmMain.Instance.MetroContainer.Controls.ContainsKey("ucNova"))
        //    {
        //        var uc = new ucNova();
        //        uc.Dock = DockStyle.Fill;
        //        FrmMain.Instance.MetroContainer.Controls.Add(uc);
        //    }
        //    FrmMain.Instance.MetroContainer.Controls["ucNova"].BringToFront();
        //}

        private void ResizeChildenControls()
        {
            foreach (var item in controls)
            {
                ResizeControl(item.Rectangle, item.Control);
            }
        }

        private void ResizeControl(Rectangle origRect, Control control)
        {
            float xRatio = (float) (this.Width) / (float) (containerOrigSize.Width);
            float yRatio = (float) (this.Height) / (float) (containerOrigSize.Height);
            var newX = (int) (origRect.X * xRatio);
            var newY = (int) (origRect.Y * yRatio);
            var newWidth = (int) (origRect.Width * xRatio);
            var newHeight = (int) (origRect.Height * yRatio);
            control.Location = new Point(newX, newY);
            control.Size = new Size(newWidth, newHeight);
        }

        private void ucDashboard_Resize(object sender, EventArgs e)
        {
            if (!initialLoad)
            {
                ResizeChildenControls();
            }

        }
    }
}
