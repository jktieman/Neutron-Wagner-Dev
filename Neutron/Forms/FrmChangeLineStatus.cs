using System;
using System.Linq;
using System.Windows.Forms;
using Neutron.Global;
using NeutronCore.Enums;
using NeutronData.DataContexts;
using NeutronData.Interfaces;
using NeutronData.Models;
using NeutronData.Models.Lookups;
using NeutronData.Repositories;
using OrderStatus = NeutronCore.Enums.OrderStatus;

namespace Neutron.Forms
{
    public partial class FrmChangeLineStatus : Form
    {
        private readonly OrderDetail _orderDetail;
        private readonly IHistoryManager _historyManager;
        private readonly GenericRepository<Inventory> _repoInventory;
        private readonly GenericRepository<LineStatusLookup> _repoStatus;
        private readonly GenericRepository<OrderDetail> _repoOrderDetails;
        private readonly GenericRepository<Order> _repoOrders;
       // private readonly List<int> _statusNumbers = new List<int> { 1, 6, 9 };
        private readonly int _currentStatus;
        private readonly Func<NeutronDb> _contextFactory;

        public FrmChangeLineStatus(OrderDetail orderDetail, IHistoryManager historyManager, Func<NeutronDb> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _orderDetail = orderDetail;
            _historyManager = historyManager;

            _repoInventory = new GenericRepository<Inventory>(contextFactory);
            _repoStatus = new GenericRepository<LineStatusLookup>(contextFactory);
            _repoOrderDetails = new GenericRepository<OrderDetail>(contextFactory);
            _repoOrders = new GenericRepository<Order>(contextFactory);

            _currentStatus = orderDetail.LineStatusId;
            InitializeComponent();
            SetupStatusComboBox();
            if (_orderDetail == null) return;
            LabelInfo.Text = orderDetail.PartNum;
            ComboBoxStatus.SelectedValue = _orderDetail.LineStatusId;

        }

        private void SetupStatusComboBox()
        {
            //var statusTypes = _repoStatus.All().Where(r => _statusNumbers.Contains(r.Id)).ToList();
            var statusTypes = _repoStatus.All().OrderBy(r => r.Name).ToList();

            ComboBoxStatus.DataSource = statusTypes;
            ComboBoxStatus.DisplayMember = "Name";
            ComboBoxStatus.ValueMember = "Id";
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            var newStatus = ((LineStatusLookup)ComboBoxStatus.SelectedItem).Id;
            if (_currentStatus == newStatus) return;
            if (_currentStatus == (int)LineStatus.Complete)
            {
                if ((newStatus == (int)LineStatus.Available) || (newStatus == (int)LineStatus.Skipped))
                {
                    _orderDetail.PickedQuantity = 0;

                }
                _orderDetail.LineStatusId = newStatus;
                _repoOrderDetails.Update(_orderDetail);
                SetOrderAvailable(_orderDetail.Order);

            }
            else if (_currentStatus == (int)LineStatus.Available)
            {
                if (newStatus == (int)LineStatus.Complete)
                {
                    _orderDetail.PickedQuantity = _orderDetail.Quantity;
                    _orderDetail.LineStatusId = newStatus;
                    _repoOrderDetails.Update(_orderDetail);
                    var inv = _repoInventory.All().FirstOrDefault(r => r.ItemDefinitionId == _orderDetail.ItemDefinitionId);
                    if (inv != null)
                    {
                        _historyManager.SaveHistory(ActionCode.PickOrder, inv, _orderDetail.Quantity, _orderDetail);
                    }

                    CheckForOrderComplete(_orderDetail.Order);
                }
                else if (newStatus == (int)LineStatus.Skipped)
                {
                    _orderDetail.LineStatusId = newStatus;
                    _repoOrderDetails.Update(_orderDetail);
                }
            }
            else if (_currentStatus == (int)LineStatus.Skipped)
            {
                if (newStatus == (int)LineStatus.Complete)
                {
                    _orderDetail.PickedQuantity = _orderDetail.Quantity;
                    _orderDetail.LineStatusId = newStatus;
                    _repoOrderDetails.Update(_orderDetail);
                    var inv = _repoInventory.All().FirstOrDefault(r => r.ItemDefinitionId == _orderDetail.ItemDefinitionId);
                    if (inv != null)
                    {
                        _historyManager.SaveHistory(ActionCode.PickOrder, inv, _orderDetail.Quantity, _orderDetail);
                    }

                    CheckForOrderComplete(_orderDetail.Order);
                }
                else if (newStatus == (int)LineStatus.Available)
                {
                    _orderDetail.LineStatusId = newStatus;
                    _repoOrderDetails.Update(_orderDetail);
                }
            }
            else if (_currentStatus == (int)LineStatus.Picking)
            {
                if (newStatus == (int)LineStatus.Complete)
                {
                    _orderDetail.PickedQuantity = _orderDetail.Quantity;
                    _orderDetail.LineStatusId = newStatus;
                    _repoOrderDetails.Update(_orderDetail);
                    var inv = _repoInventory.All().FirstOrDefault(r => r.ItemDefinitionId == _orderDetail.ItemDefinitionId);
                    if (inv != null)
                    {
                        GlobalVar.HistoryManager.SaveHistory(ActionCode.PickOrder, inv, _orderDetail.Quantity, _orderDetail);
                    }

                    CheckForOrderComplete(_orderDetail.Order);
                }
                else if (newStatus == (int)LineStatus.Available)
                {
                    _orderDetail.LineStatusId = newStatus;
                    _repoOrderDetails.Update(_orderDetail);
                    SetOrderAvailable(_orderDetail.Order);
                }
            }
            else
            {
                _orderDetail.LineStatusId = newStatus;
                _repoOrderDetails.Update(_orderDetail);
                SetOrderAvailable(_orderDetail.Order);
            }

            _historyManager.SaveHistory(ActionCode.ChangeLineStatus, _orderDetail);
            Close();
        }

        private void CheckForOrderComplete(Order order)
        {
            var linesNotComplete = _repoOrderDetails.FindBy(r => r.OrderId == order.Id).Where(r => r.LineStatusId != (int)LineStatus.Complete)
                .ToList();
            if (linesNotComplete.Any()) return;

            order.OrderStatusId = (int)OrderStatus.Complete;
            _historyManager.SaveHistory(ActionCode.OrderComplete, order, _orderDetail.AreaId);
            _repoOrders.Update(order);
        }

        private void SetOrderAvailable(Order order)
        {
            order.OrderStatusId = (int)OrderStatus.Available;
            _repoOrders.Update(order);

        }
    }
}