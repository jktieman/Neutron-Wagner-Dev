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
using NeutronData.Models.Lookups;
using NeutronData.Repositories;
//using LineStatusLookup = NeutronData.Models.Lookups.LineStatusLookup;

namespace Neutron.Forms
{
    public partial class FrmChangeLineStatus : Form
    {
        private readonly OrderDetail _orderDetail;
        private readonly GenericRepository<Inventory> _repoInventory = new GenericRepository<Inventory>(new NeutronDb());
        private readonly GenericRepository<LineStatusLookup> _repoStatus = new GenericRepository<LineStatusLookup>(new NeutronDb());
        private readonly GenericRepository<OrderDetail> _repoOrderDetails = new GenericRepository<OrderDetail>(new NeutronDb());

        public FrmChangeLineStatus(OrderDetail orderDetail)
        {
            _orderDetail = orderDetail;
            InitializeComponent();
            SetupStatusComboBox();
            if (_orderDetail == null) return;
            LabelInfo.Text = orderDetail.PartNum;
            ComboBoxStatus.SelectedValue = _orderDetail.LineStatusId;

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
            _orderDetail.LineStatusId = ((LineStatusLookup)ComboBoxStatus.SelectedItem).Id;
            _repoOrderDetails.Update(_orderDetail);
            GlobalVar.HistoryManager.SaveHistory(ActionCode.ChangeLineStatus, _orderDetail);
            if (_orderDetail.LineStatusId == (int) LineStatus.Complete)
            {
                var inv = _repoInventory
                    .AllInclude(r => r.ItemDefinition).FirstOrDefault(s => s.ItemDefinition.Item == _orderDetail.PartNum);
                if (inv != null)
                {
                    GlobalVar.HistoryManager.SaveHistory(ActionCode.PickOrder, inv, _orderDetail.Quantity, _orderDetail);
                }
            }
            
            Close();
        }
    }
}