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
        private readonly IHistoryManager _historyManager;
        private readonly GenericRepository<OrderStatus> _repoStatus;
        private readonly GenericRepository<Order> _repoOrders;
        private readonly Func<NeutronDb> _contextFactory;

        public FrmChangeOrderStatus(Order order, IHistoryManager historyManager, Func<NeutronDb> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _order = order;
            _historyManager = historyManager;
            _repoStatus = new GenericRepository<OrderStatus>(contextFactory);
            _repoOrders = new GenericRepository<Order>(contextFactory);
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
