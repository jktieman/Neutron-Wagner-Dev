using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Neutron.Global;
using NeutronCore.Enums;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Repositories;
using OrderStatus = NeutronData.Models.Lookups.OrderStatus;

namespace Neutron.Forms
{
    public partial class FrmChangeOrderStatus : Form
    {
        private readonly Order _order;
        private readonly GenericRepository<OrderStatus> _repoStatus = new GenericRepository<OrderStatus>(new NeutronDb());

        private readonly GenericRepository<Order> _repoOrders = new GenericRepository<Order>(new NeutronDb());

        public FrmChangeOrderStatus(Order order)
        {
            _order = order;
            InitializeComponent();
            SetupStatusComboBox();
            if (order == null) return;
            LabelInfo.Text = order.Ord1;
            ComboBoxStatus.SelectedValue = order.OrderStatusId;

        }

        private void SetupStatusComboBox()
        {
            var statusTypes = _repoStatus.All();

            ComboBoxStatus.DataSource = statusTypes;
            ComboBoxStatus.DisplayMember = "Name";
            ComboBoxStatus.ValueMember = "Id";
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            _order.OrderStatusId = ((OrderStatus) ComboBoxStatus.SelectedItem).Id;
            _repoOrders.Update(_order);
            GlobalVar.HistoryManager.SaveHistory(ActionCode.ChangePriority, _order);
            Close();
        }
    }
}
