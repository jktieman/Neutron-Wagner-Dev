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

namespace Neutron.UserControls
{
    public partial class ucNova : MetroUserControl
    {
        public ucNova()
        {
            InitializeComponent();
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

        private void metroLinkBack_Click(object sender, EventArgs e)
        {
           FrmMain.Instance.MetroContainer.Controls["ucNova"].BringToFront();
            // metroLinkBack.Visible = false;
        }

        private void metroLinkBack_Click_1(object sender, EventArgs e)
        {

        }

        private void mtSpecialFunctions_Click(object sender, EventArgs e)
        {
            if (!FrmMain.Instance.MetroContainer.Controls.ContainsKey("ucSpecialFunctions"))
            {
                var uc = new ucSpecialFunctions();
                uc.Dock = DockStyle.Fill;
                FrmMain.Instance.MetroContainer.Controls.Add(uc);
            }
            FrmMain.Instance.MetroContainer.Controls["ucSpecialFunctions"].BringToFront();
        }

        private void mtNDefDb_Click(object sender, EventArgs e)
        {
            //if (!FrmMain.Instance.MetroContainer.Controls.ContainsKey("ucNDefDb"))
            //{
            //    var uc = new UcItemDefinition();
            //    uc.Dock = DockStyle.Fill;
            //    FrmMain.Instance.MetroContainer.Controls.Add(uc);
            //}
            //FrmMain.Instance.MetroContainer.Controls["ucNDefDb"].BringToFront();
        }

        private void mtNovaRec_Click(object sender, EventArgs e)
        {
            if (!FrmMain.Instance.MetroContainer.Controls.ContainsKey("ucNova"))
            {
                var uc = new ucNova();
                uc.Dock = DockStyle.Fill;
                FrmMain.Instance.MetroContainer.Controls.Add(uc);
            }
            FrmMain.Instance.MetroContainer.Controls["ucNova"].BringToFront();
        }
    }
}
