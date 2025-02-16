using NeutronCore;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.ModelViews;
using NeutronData.Repositories;
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
using NeutronCore.Extensions;

namespace Neutron.Forms
{
    public partial class FrmScanTransId : Form
    {

        public string TransId { get; set; }
        public ReplenOrder ReplenOrder { get; set; }
        private readonly GenericRepository<ReplenOrder> _repoReplenOrder;
        private readonly string _order;
        private readonly Func<NeutronDb> _contextFactory;

        public FrmScanTransId(string order, Func<NeutronDb> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _repoReplenOrder = new GenericRepository<ReplenOrder>(contextFactory);
            _order = order;
            InitializeComponent();
            TextBoxOrder.Text = order;
        }

        private bool CheckForValidOrder()
        {
            var ord = _repoReplenOrder.FindBy(r => r.Ord1 == _order && r.OrderInfo == TransId);
            if( ord != null )
            {
                ReplenOrder = ord.First();
                return true;
            }

            ReplenOrder = null;
            return false;
        }

        private void MBCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MBSave_Click(object sender, EventArgs e)
        {
            TransId = TextBoxTransId.Text;
            if (CheckForValidOrder())
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Invalid TransId");
            }
        }
    }
}
