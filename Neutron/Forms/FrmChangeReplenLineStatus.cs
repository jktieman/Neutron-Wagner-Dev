using System;
using System.Linq;
using System.Windows.Forms;
using Neutron.Global;
using NeutronCore.Enums;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Models.Lookups;
using NeutronData.Repositories;
using OrderStatus = NeutronCore.Enums.OrderStatus;

namespace Neutron.Forms
{
    public partial class FrmChangeReplenLineStatus : Form
    {
        private readonly ReplenOrderDetail _orderDetail;
        private readonly GenericRepository<Inventory> _repoInventory = new GenericRepository<Inventory>(new NeutronDb());
        private readonly GenericRepository<LineStatusLookup> _repoStatus = new GenericRepository<LineStatusLookup>(new NeutronDb());
        private readonly GenericRepository<ReplenOrderDetail> _repoOrderDetails = new GenericRepository<ReplenOrderDetail>(new NeutronDb());
        private readonly GenericRepository<ReplenOrder> _repoOrders = new GenericRepository<ReplenOrder>(new NeutronDb());
        // private readonly List<int> _statusNumbers = new List<int> { 1, 6, 9 };
        private readonly int _currentStatus;

        public FrmChangeReplenLineStatus(ReplenOrderDetail orderDetail)
        {
            _orderDetail = orderDetail;
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
                SetOrderAvailable(_orderDetail.ReplenOrder);

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
                        GlobalVar.HistoryManager.SaveHistory(ActionCode.PickOrder, inv, _orderDetail.Quantity, _orderDetail);
                    }

                    CheckForOrderComplete(_orderDetail.ReplenOrder);
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
                        GlobalVar.HistoryManager.SaveHistory(ActionCode.PickOrder, inv, _orderDetail.Quantity, _orderDetail);
                    }

                    CheckForOrderComplete(_orderDetail.ReplenOrder);
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

                    CheckForOrderComplete(_orderDetail.ReplenOrder);
                }
                else if (newStatus == (int)LineStatus.Available)
                {
                    _orderDetail.LineStatusId = newStatus;
                    _repoOrderDetails.Update(_orderDetail);
                    SetOrderAvailable(_orderDetail.ReplenOrder);
                }
            }
            else
            {
                _orderDetail.LineStatusId = newStatus;
                _repoOrderDetails.Update(_orderDetail);
                SetOrderAvailable(_orderDetail.ReplenOrder);
            }

            GlobalVar.HistoryManager.SaveHistory(ActionCode.ChangeLineStatus, _orderDetail);
            Close();
        }

        private void CheckForOrderComplete(ReplenOrder order)
        {
            var linesNotComplete = _repoOrderDetails.FindBy(r => r.ReplenOrderId == order.Id).Where(r => r.LineStatusId != (int)LineStatus.Complete)
                .ToList();
            if (linesNotComplete.Any()) return;

            order.OrderStatusId = (int)OrderStatus.Complete;
            GlobalVar.HistoryManager.SaveHistory(ActionCode.OrderComplete, order, _orderDetail.AreaId);
            _repoOrders.Update(order);
        }

        private void SetOrderAvailable(ReplenOrder order)
        {
            order.OrderStatusId = (int)OrderStatus.Available;
            _repoOrders.Update(order);

        }
    }
}