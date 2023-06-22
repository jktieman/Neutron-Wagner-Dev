using System;
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
        private readonly HistoryManager _historyManager;
        private readonly GenericRepository<OrderStatus> _repoStatus = new GenericRepository<OrderStatus>(new NeutronDb());
        private readonly GenericRepository<Order> _repoOrders = new GenericRepository<Order>(new NeutronDb());

        public FrmChangeOrderStatus(Order order, HistoryManager historyManager)
        {
            _order = order;
            _historyManager = historyManager;
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
            _historyManager.SaveHistory(ActionCode.ChangePriority, _order);
            Close();
        }
    }
}
