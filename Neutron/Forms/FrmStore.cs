using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MetroFramework.Forms;
using Neutron.Global;
using Neutron.Models;
using NeutronData.DataContexts;
using NeutronData.ModelViews;
using NeutronData.Models;
using NeutronData.Repositories;
using NeutronCore.Extensions;
using System.Threading.Tasks;
using Neutron.Controllers;
using Neutron.Interfaces;
using JsonManager;
using Neutron.Classes;
using Neutron.Enums;
using EnumsNET;
using System.IO;
using NeutronData.SqlModelViews;
using NeutronLoader;
using NeutronCore.Global;
using NeutronData.Interfaces;

namespace Neutron.Forms
{
    public partial class FrmStore : MetroForm
    {
        private GenericRepository<ReplenOrder> repoOrders = new GenericRepository<ReplenOrder>(new NeutronDb());
        private GenericRepository<ReplenOrderDetail> repoOrderDetails = new GenericRepository<ReplenOrderDetail>(new NeutronDb());
        private GenericRepository<Inventory> repoInventory = new GenericRepository<Inventory>(new NeutronDb());
        private InventoryRepository repoInv = new InventoryRepository();
        private GenericRepository<LocationCount> repoLocationCount = new GenericRepository<LocationCount>(new NeutronDb());
        private GenericRepository<ItemDefinition> repoItemDefinition = new GenericRepository<ItemDefinition>(new NeutronDb());
        private ReplenOrdersRepository repo = new ReplenOrdersRepository();
        private ReplenOrderDetailsRepository replenOrderDetailsRepository = new ReplenOrderDetailsRepository();
        private GenericRepository<HardwareDevice> repoHardwareDevices = new GenericRepository<HardwareDevice>(new NeutronDb());
        //private GenericRepository<Station> repoStation = new GenericRepository<Station>(new NeutronDb());
        private StationRepository repoStation = new StationRepository();

        private BindingSource bindingSource = new BindingSource();
        private BindingSource bindingSourceCompleted = new BindingSource();
        private BindingSource bindingSourceAvailableOrders = new BindingSource();
        private BindingSource bindingSourceViewPick = new BindingSource();
        private BindingSource hotBindingSource = new BindingSource();
        private BindingSource bindingSourceOrderDetailsView = new BindingSource();
        private BindingSource bindingSourcePickStops = new BindingSource();
        //New Order
        private BindingSource bindingSourceItems = new BindingSource();
        private BindingSource bindingSourceNewItems = new BindingSource();
        //----
        ISecurityProcessor securityProcessor;
        private Global.HistoryManager historyManager = new Global.HistoryManager();
        public bool CloseButtonPressed { get; set; }
        public ReplenOrderView CurrentItem;
        public TextBox CurrentTextBoxPos;
        public bool CloseForm = false;
        private List<BatchPosition> ordersToPick = new List<BatchPosition>();
        ReplenPickView currentPickView = new ReplenPickView();
        ReplenPickStop currentPickStop = new ReplenPickStop();
        private InventoryView currentInventoryView = new InventoryView();
        private ItemDefinition currentItem;
        //private int pickedSoFar = 0;
        private INomenclature nomenclature;
        private IJsonData jsonData;
        private InterfaceProcessor interfaceProcessor;
        NeutronVariables neutronVariables;
        private StationView station;
        IAkaRepository akaRepository;

        public FrmStore(IJsonData jsonData, StationView station, IAkaRepository akaRepository)
        {
            InitializeComponent();
            this.station = station;
            this.jsonData = jsonData;
            neutronVariables = jsonData.LoadFile<NeutronVariables>();
            securityProcessor = new SecurityProcessor();
            securityProcessor.ReprocessSecuritySet(Variables.user.Pin);
            this.akaRepository = akaRepository;
            UpdateNomenclature();
            SetupGrids();
            HideTabControlTabs();
            mlUserInfo.Text = Variables.user?.UserInfo;
            CloseButtonPressed = false;
            CurrentTextBoxPos = TextBoxPos1;
            InitOrdersToPick();
            InitListView();
           // SetupShuttle();
        }

        //private void SetupShuttle()
        //{
        //    if (neutronVariables.DisableShuttle == false)
        //    {
        //        StationView station = repoStation.GetStationView(neutronVariables.StationNumber);
        //        if (station.HardwareDevices.Count > 0)
        //        {
        //            if (neutronVariables.DeviceDriver == "C3000" && GlobalVar.Shuttle == null)
        //            {
        //                GlobalVar.Shuttle = new C3000(this, station);
        //            }
        //            if (neutronVariables.DeviceDriver == "C2000" && GlobalVar.Shuttle == null)
        //            {
        //                GlobalVar.Shuttle = new C2000(this, station);
        //            }
        //            if (neutronVariables.DeviceDriver == "RCC2" && GlobalVar.Shuttle == null)
        //            {
        //                GlobalVar.Shuttle = new C2000(this, station);
        //            }
        //        }
        //    }
        //}

        private void UpdateNomenclature()
        {
            nomenclature = new Nomenclature();
            nomenclature.GetValues();
            MBStoreAccept.Text = nomenclature.MBStoreAccept;
            LabelTray.Text = nomenclature.LabelTray;
            LabelOver.Text = nomenclature.LabelOver;
            LabelBack.Text = nomenclature.LabelBack;
            LabelDevice.Text = nomenclature.LabelDevice;
        }

        private void FrmStore_Load(object sender, EventArgs e)
        {
            int id = ShowListing();
            if (Variables.LoaderRunning)
            {
                MBMainLoadOrders.Text = "Stop Loader";
            }
        }

        // Set the focus to the passed in recId if it's passed in
        private int ShowListing(int recId = 0)
        {
            int idx = 0;
            bindingSource.DataSource = repo.GetOrderView();
            DataGridView1.DataSource = bindingSource;

            if (GetRecordCount(bindingSource) > 0)
            {
                if (recId != 0)
                {
                    idx = IndexOf(bindingSource, recId);
                    DataGridView1.FirstDisplayedScrollingRowIndex = DataGridView1.Rows[idx].Index;
                }
                else
                {
                    DataGridView1.ClearSelection();
                }

                DataGridView1.Refresh();
                DataGridView1.ClearSelection();
                CurrentItem = ((ReplenOrderView) bindingSource.Current);
            }
            return idx;
        }

        private int ShowAvailableOrders(int recId = 0)
        {
            int idx = 0;
            bindingSourceAvailableOrders.DataSource = repo.GetAvailableOrders(string.Empty );
            DataGridViewAvailableOrders.DataSource = bindingSourceAvailableOrders;

            if (GetRecordCount(bindingSourceAvailableOrders) > 0)
            {
                if (recId != 0)
                {
                    idx = IndexOf(bindingSourceAvailableOrders, recId);
                    DataGridViewAvailableOrders.FirstDisplayedScrollingRowIndex = DataGridViewAvailableOrders.Rows[idx].Index;
                    DataGridViewAvailableOrders.CurrentCell = DataGridViewAvailableOrders.Rows[idx].Cells[1];
                    DataGridViewAvailableOrders.Rows[idx].Selected = true;
                }
                else
                {
                    DataGridViewAvailableOrders.ClearSelection();
                }
                CurrentItem = ((ReplenOrderView) bindingSourceAvailableOrders.Current);
            }
            return idx;
        }

        public int IndexOf(BindingSource bs, int value)
        {
            int count = bs.Count;
            int itemIndex = -1;
            for (int i = 0; i < count; i++)
            {
                int rec = ((ReplenOrderView) bs[i]).Id;
                if (rec == value)
                {
                    itemIndex = i;
                    break;
                }
            }
            return itemIndex;
        }

        private int GetRecordCount(BindingSource bs)
        {
            int count = bs.Count;
            LabelRecordCount.Text = string.Format("Records: {0}", count.ToString());
            return count;
        }

        private void MButtonFind_Click(object sender, EventArgs e)
        {
            FindRecord(TextBoxFind.Text.Trim());
        }

        private void FindRecord(string s)
        {
            try
            {
                if (string.IsNullOrEmpty(s))
                {
                    ShowListing();
                }
                else
                {
                    IEnumerable<ReplenOrderView> projection = repo.GetOrderView();
                    bindingSource.DataSource = projection.Where(d => d.Ord1.Contains(s) || d.Ord2.Contains(s)).ToList();
                    GetRecordCount(bindingSource);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Find Error: " + ex.Message);
            }
        }

        private void TextBoxFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                FindRecord(TextBoxFind.Text.Trim());
            }
            if (e.KeyCode == Keys.Escape)
            {
                TextBoxFind.Text = "";
            }
        }

        #region Button Clicks
        private void ButtonClear_Click(object sender, EventArgs e)
        {
            TextBoxFind.Text = string.Empty;
            ShowListing();
            TextBoxFind.Focus();
        }

        private void MButtonClose_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Store";
            LabelFormTitle.BackColor = Color.Green;
            tabControl1.SelectedTab = Main;
        }

        private void MButtonViewEdit_Click(object sender, EventArgs e)
        {
            //CurrentItem = ((ReplenOrderView) bindingSource.Current);
            //TextBoxViewEditId.Text = CurrentItem.Id.ToString();
            //TextBoxViewEditItem.Text = CurrentItem.Item;
            //TextBoxViewEditDescription.Text = CurrentItem.Description;

            //  IEnumerable<Inventory> recs = repoInventory.AllInclude(h => h.ItemDefinition, h => h.Location
            //, h => h.Location.Station, h => h.Location.SizeCode, h => h.Location.VelocityCode, h => h.Location.HeightCode)
            //.Where(r => r.ItemDefinitionId == CurrentItem.Id);

            // locationBindingSource.DataSource = GetInventoryViewListByItem(CurrentItem.Id);
            // DataGridViewInventoryLocations.DataSource = locationBindingSource;
            //DataGridViewInventoryNewLocations.DataSource = newLocationBindingSource;

            this.tabControl1.SelectedTab = AvailableOrders;
        }

        private void MButtonNew_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Store Item";
            LabelFormTitle.BackColor = Color.Green;
            this.tabControl1.SelectedTab = PickScreen;
        }

        private void MbViewEditListing_Click(object sender, EventArgs e)
        {
            ShowListing();
            LabelFormTitle.Text = "Job Listing";
            LabelFormTitle.BackColor = Color.Green;
            this.tabControl1.SelectedTab = OrderListing;
        }

        private void MbViewEditNew_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Store Item";
            LabelFormTitle.BackColor = Color.Green;
            this.tabControl1.SelectedTab = PickScreen;
        }

        private void MbViewEditClose_Click(object sender, EventArgs e)
        {
            CloseButtonPressed = true;
            this.Close();
        }

        //private void MbViewEditEdit_Click(object sender, EventArgs e)
        //{
        //    InventoryView inventoryView = ((InventoryView) locationBindingSource.Current);
        //    TextBoxAddDetailInventoryId.Text = inventoryView.Id.ToString();
        //    Location location = inventoryView.Location;
        //    TextBoxAddDetailLocationId.Text = location.Id.ToString();
        //    TextBoxAddDetailQuantity.Text = inventoryView.Quantity.ToString();
        //    TextBoxAddDetailItemDefinitionId.Text = CurrentItem.Id.ToString();
        //    ComboBoxAddDetailStation.SelectedValue = location.StationId;
        //    TextBoxAddDetailLoc1.Text = location.Loc1.ToString();
        //    TextBoxAddDetailLoc2.Text = location.Loc2.ToString();
        //    TextBoxAddDetailLoc3.Text = location.Loc3.ToString();
        //    TextBoxAddDetailLoc4.Text = location.Loc4.ToString();
        //    TextBoxAddDetailLoc5.Text = location.Loc5.ToString();
        //    TextBoxAddDetailSlot.Text = location.Slot.ToString();
        //    ComboBoxAddDetailStorageType.SelectedValue = inventoryView.StorageTypeId;
        //    ComboBoxAddDetailSizeCode.SelectedValue = location.SizeCodeId;
        //    ComboBoxAddDetailVelocityCode.SelectedValue = location.VelocityCodeId;
        //    ComboBoxAddDetailHeightCode.SelectedValue = location.HeightCodeId;
        //    LabelActionAddDetail.Text = "Edit Detail";
        //    TextBoxAddDetailQuantity.Focus();
        //    tabControl1.SelectedTab = tabPage5;

        //}

        private void MbNewListing_Click(object sender, EventArgs e)
        {
            ShowListing();
            tabControl1.SelectedTab = OrderListing;
        }

        private void MbNewViewEdit_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = AvailableOrders;
        }

        private void MbNewClose_Click(object sender, EventArgs e)
        {
            CloseButtonPressed = true;
            this.Close();
        }
        #endregion

        private void SetupGrids()
        {
            ListBoxSelectedOrders.DisplayMember = "Ord1";
            ListBoxSelectedOrders.ValueMember = "Id";

            DataGridView1.AutoGenerateColumns = false;
            DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridView1.DefaultCellStyle.ForeColor = Color.Black;
            DataGridView1.DefaultCellStyle.BackColor = Color.White;

            int w = 50;

            var colx = new DataGridViewCheckBoxColumn();
            colx.HeaderText = "X";
            colx.Width = Convert.ToInt32(w * .5);
            colx.Name = "IsChecked";
            colx.TrueValue = true;
            colx.FalseValue = false;
            DataGridView1.Columns.Add(colx);

            var col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Ord1";
            col.HeaderText = "Job";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Ord1";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Ord2";
            col.HeaderText = "Invoice";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Ord2";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Priority";
            col.HeaderText = "Priority";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Priority";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "ShipMethodName";
            col.HeaderText = "Ship Method";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "ShipMethodName";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "OrderStatusName";
            col.HeaderText = "Job Status";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "OrderStatusName";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Lines";
            col.HeaderText = "Lines";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Lines";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Pieces";
            col.HeaderText = "Pieces";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Pieces";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "LoadDate";
            col.HeaderText = "LoadDate";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "LoadDate";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Id";
            col.HeaderText = "Id";
            col.Visible = false;
            col.Name = "Id";
            DataGridView1.Columns.Add(col);

            //*****************************************************************************
            //DataGridViewPick

            DataGridViewPick.AutoGenerateColumns = false;
            DataGridViewPick.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewPick.DefaultCellStyle.ForeColor = Color.Black;
            DataGridViewPick.DefaultCellStyle.BackColor = Color.White;

            w = 50;

            //var bCol = new DataGridViewButtonColumn();
            //bCol.HeaderText = "";
            //bCol.Visible = true;
            //bCol.Name = "Position";
            //bCol.Text = "Position";
            //bCol.UseColumnTextForButtonValue = true;
            //DataGridViewPick.Columns.Add(bCol);
            //var colx = new DataGridViewCheckBoxColumn();
            //colx.HeaderText = "X";
            //colx.Width = Convert.ToInt32(w * .5);
            //colx.Name = "IsChecked";
            //colx.TrueValue = true;
            //colx.FalseValue = false;
            //DataGridViewPick.Columns.Add(colx);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Sequence";
            col.HeaderText = "Seq";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Visible = false;
            col.Name = "Sequence";
            DataGridViewPick.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "PickPosition";
            col.HeaderText = "Pos";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            colx.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.Name = "PickPosition";
            DataGridViewPick.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Ord1";
            col.HeaderText = "Job";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Ord1";
            DataGridViewPick.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Ord2";
            col.HeaderText = "Invoice";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Ord2";
            DataGridViewPick.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Item";
            col.HeaderText = "Item";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Item";
            DataGridViewPick.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Description";
            col.HeaderText = "Description";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Description";
            DataGridViewPick.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Quantity";
            col.HeaderText = "Quantity";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Quantity";
            DataGridViewPick.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Slot";
            col.HeaderText = "Slot";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            colx.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.Name = "Slot";
            DataGridViewPick.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "SlotQty";
            col.HeaderText = "Slot Qty";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "SlotQty";
            DataGridViewPick.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "OrderId";
            col.HeaderText = "Job Id";
            col.Visible = false;
            col.Name = "OrderId";
            DataGridViewPick.Columns.Add(col);

            //*****************************************************************************
            ///DataGridViewAvailableOrders

            DataGridViewAvailableOrders.AutoGenerateColumns = false;
            DataGridViewAvailableOrders.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewAvailableOrders.DefaultCellStyle.ForeColor = Color.Black;
            DataGridViewAvailableOrders.DefaultCellStyle.BackColor = Color.White;
            DataGridViewAvailableOrders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            w = 50;

            //var bCol = new DataGridViewButtonColumn();
            //bCol.HeaderText = "";
            //bCol.Visible = true;
            //bCol.Name = "Position";
            //bCol.Text = "Position";
            //bCol.UseColumnTextForButtonValue = true;
            //DataGridViewAvailableOrders.Columns.Add(bCol);
            colx = new DataGridViewCheckBoxColumn();
            colx.HeaderText = "X";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colx.Name = "IsChecked";
            colx.TrueValue = true;
            colx.FalseValue = false;
            DataGridViewAvailableOrders.Columns.Add(colx);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Ord1";
            col.HeaderText = "Job";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Ord1";
            DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Ord2";
            col.HeaderText = "Invoice";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Ord2";
            DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Priority";
            col.HeaderText = "Priority";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Priority";
            DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "ShipMethodName";
            col.HeaderText = "Ship Method";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "ShipMethodName";
            DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "OrderStatusName";
            col.HeaderText = "Job Status";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "OrderStatusName";
            DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Lines";
            col.HeaderText = "Lines";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.Name = "Lines";
            DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Pieces";
            col.HeaderText = "Pieces";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.Name = "Pieces";
            DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "LoadDate";
            col.HeaderText = "Load Date";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.Name = "LoadDate";
            DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Id";
            col.HeaderText = "Id";
            col.Visible = false;
            col.Name = "Id";
            DataGridViewAvailableOrders.Columns.Add(col);

            //**********************************************************************************************           
            //DataGridViewInventory

            DataGridViewInventory.AutoGenerateColumns = false;
            DataGridViewInventory.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewInventory.DefaultCellStyle.ForeColor = Color.Black;
            DataGridViewInventory.DefaultCellStyle.BackColor = Color.White;

            w = 50;

            var bCol = new DataGridViewButtonColumn();
            bCol.HeaderText = "";
            bCol.Visible = true;
            bCol.Name = "HotPick";
            bCol.Text = "Hot Store";
            bCol.FlatStyle = FlatStyle.Popup;
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            bCol.UseColumnTextForButtonValue = true;
            DataGridViewInventory.Columns.Add(bCol);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Item";
            col.HeaderText = "Item";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Item";
            DataGridViewInventory.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Description";
            col.HeaderText = "Description";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Description";
            DataGridViewInventory.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Quantity";
            col.HeaderText = "Quantity";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.Name = "Quantity";
            DataGridViewInventory.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Slot";
            col.HeaderText = "Slot";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.Name = "Slot";
            DataGridViewInventory.Columns.Add(col);

            var xcol = new DataGridViewCheckBoxColumn();
            xcol.DataPropertyName = "PrimeBin";
            xcol.HeaderText = "Prime Bin";
            xcol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            xcol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            xcol.Name = "PrimeBin";
            DataGridViewInventory.Columns.Add(xcol);


            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Id";
            col.HeaderText = "Id";
            col.Visible = false;
            col.Name = "Id";
            DataGridViewInventory.Columns.Add(col);

            //********************************************************************
            // DataGridViewNewOrder

            DataGridViewNewOrder.AutoGenerateColumns = false;
            DataGridViewNewOrder.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewNewOrder.DefaultCellStyle.ForeColor = Color.Black;
            DataGridViewNewOrder.DefaultCellStyle.BackColor = Color.White;

            w = 50;

            bCol = new DataGridViewButtonColumn();
            bCol.HeaderText = "";
            bCol.Visible = true;
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            bCol.Name = "AddItem";
            bCol.Text = "Add Item";
            bCol.FlatStyle = FlatStyle.Popup;
            bCol.UseColumnTextForButtonValue = true;
            DataGridViewNewOrder.Columns.Add(bCol);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Item";
            col.HeaderText = "Item";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Item";
            DataGridViewNewOrder.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Description";
            col.HeaderText = "Description";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Description";
            DataGridViewNewOrder.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Id";
            col.HeaderText = "Id";
            col.Visible = false;
            col.Name = "Id";
            DataGridViewNewOrder.Columns.Add(col);

            //********************************************************************
            //DataGridViewOrderDetails

            DataGridViewOrderDetails.AutoGenerateColumns = false;
            DataGridViewOrderDetails.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewOrderDetails.DefaultCellStyle.ForeColor = Color.Black;
            DataGridViewOrderDetails.DefaultCellStyle.BackColor = Color.White;

            w = 50;
            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Ord1";
            col.HeaderText = "Job";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Ord1";
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Ord2";
            col.HeaderText = "Invoice";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Ord2";
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Item";
            col.HeaderText = "Item";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Item";
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Description";
            col.HeaderText = "Description";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Description";
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Quantity";
            col.HeaderText = "Quantity";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.Name = "Quantity";
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Id";
            col.HeaderText = "Id";
            col.Visible = false;
            col.Name = "Id";
            DataGridViewOrderDetails.Columns.Add(col);

        }

        //private bool ValidateFields(Inventory rec)
        //{
        //    //if (!IntegerValidator(rec.Loc1))
        //    //{
        //    //    return false;
        //    //}
        //    //if (!IntegerValidator(rec.Loc2))
        //    //{
        //    //    return false;
        //    //}
        //    //if (!IntegerValidator(rec.Loc3))
        //    //{
        //    //    return false;
        //    //}
        //    //if (!IntegerValidator(rec.Loc4))
        //    //{
        //    //    return false;
        //    //}
        //    //if (!IntegerValidator(rec.Loc5))
        //    //{
        //    //    return false;
        //    //}
        //    return true;
        //}

        private bool StringValidator(string input)
        {
            string pattern = "[^a-zA-Z]";
            if (Regex.IsMatch(input, pattern))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //validate integer 
        private bool IntegerValidator(int input)
        {
            string pattern = "^[0-9]+$";
            if (Regex.IsMatch(input.ToString(), pattern))
            {
                if (input <= 0)
                {
                    MessageBox.Show("Entry must be greater than zero.");
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private void FrmStore_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !CloseButtonPressed;
        }

        private void HideTabControlTabs()
        {
            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(0, 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;
            foreach (TabPage tab in tabControl1.TabPages)
            {
                tab.Text = string.Empty;
            }
        }

        private void MButtonSearch_Click(object sender, EventArgs e)
        {
            string search = TextBoxFind.Text.Trim();
            bindingSource.DataSource = repo.GetOrderView(search);
        }



        private void MButtonClearSelection_Click(object sender, EventArgs e)
        {
            DataGridView1.ClearSelection();
            foreach (DataGridViewRow row in DataGridView1.Rows)
            {
                var chk = (DataGridViewCheckBoxCell) row.Cells[0];
                chk.Value = chk.FalseValue;
                RemoveItemFromListBox(Convert.ToInt32(row.Cells["Id"].Value));
            }
        }

        private void MBSelectAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in DataGridView1.Rows)
            {
                var chk = (DataGridViewCheckBoxCell) row.Cells[0];
                chk.Value = chk.TrueValue;
                if (row.IsNewRow)
                {
                    chk.Value = chk.FalseValue;
                }
                else
                {
                    AddItemToListBox(Convert.ToInt32(row.Cells["Id"].Value));
                }
            }
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                CurrentItem = ((ReplenOrderView) bindingSource.Current);

                var chk = (DataGridViewCheckBoxCell) DataGridView1.Rows[e.RowIndex].Cells[0];

                if (chk.Value == chk.TrueValue)
                {
                    DataGridView1.Rows[e.RowIndex].Cells[0].Value = chk.FalseValue;
                    int id = Convert.ToInt32(DataGridView1.Rows[e.RowIndex].Cells["Id"].Value);
                    RemoveItemFromListBox(id);
                }
                else
                {
                    DataGridView1.Rows[e.RowIndex].Cells[0].Value = chk.TrueValue;
                    int id = Convert.ToInt32(DataGridView1.Rows[e.RowIndex].Cells["Id"].Value);
                    AddItemToListBox(id);
                }
            }
        }

        private void RemoveItemFromListBox(int id)
        {
            var ordersToRemove = new List<ReplenOrder>();
            foreach (ReplenOrder ord in ListBoxSelectedOrders.Items)
            {
                if (ord.Id == id)
                {
                    ordersToRemove.Add(ord);
                }
            }
            foreach (ReplenOrder item in ordersToRemove)
            {
                ListBoxSelectedOrders.Items.Remove(item);
            }
        }

        private void AddItemToListBox(int id)
        {
            ReplenOrder order = repoOrders.FindByKey(id);
            if (!ListBoxSelectedOrders.Items.Contains(order))
            {
                ListBoxSelectedOrders.Items.Add(order);
            }
        }

        private void ListBoxSelectedOrders_Click(object sender, EventArgs e)
        {
            var ord = (ReplenOrder) ListBoxSelectedOrders.SelectedItem;
            if (ord != null)
            {
                foreach (DataGridViewRow row in DataGridView1.Rows)
                {
                    int id = Convert.ToInt32(row.Cells["Id"].Value);
                    if (ord.Id == id)
                    {
                        int rowIndex = row.Index;
                        DataGridView1.FirstDisplayedScrollingRowIndex = rowIndex;
                        DataGridView1.Refresh();
                        DataGridView1.Rows[rowIndex].Selected = true;
                    }
                }
            }
        }

        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var dgv = sender as DataGridView;

            if (dgv.Columns[e.ColumnIndex].Name.Equals("OrderStatusName"))
            {
                if (e.Value != null)
                {
                    switch (e.Value.ToString().Trim())
                    {
                        case "Available":
                            dgv.Rows[e.RowIndex].Cells["IsChecked"].Style.BackColor = Color.White;
                            break;
                        case "Hold":
                            dgv.Rows[e.RowIndex].Cells["IsChecked"].Style.BackColor = Color.Yellow;
                            break;
                        case "Picking":
                            dgv.Rows[e.RowIndex].Cells["IsChecked"].Style.BackColor = Color.Blue;
                            break;
                        case "Partial":
                            dgv.Rows[e.RowIndex].Cells["IsChecked"].Style.BackColor = Color.Orange;
                            break;
                        case "Deleted":
                            dgv.Rows[e.RowIndex].Cells["IsChecked"].Style.BackColor = Color.Red;
                            break;
                        case "Complete":
                            dgv.Rows[e.RowIndex].Cells["IsChecked"].Style.BackColor = Color.ForestGreen;
                            break;
                        default:
                            dgv.Rows[e.RowIndex].Cells["IsChecked"].Style.BackColor = Color.White;
                            break;
                    }
                }
            }
        }

        private void DataGridView1_KeyUp(object sender, KeyEventArgs e)
        {
            //if (vm.mySelectedItem != null && e.KeyCode == Key.Space)
            //{
            //    vm.MySelectedItem.IsChecked = !vm.MySelectedItem.IsChecked;
            //    e.Handled = true; //this is necessary because otherwise when the checkbox cell is selected, it will apply this keyup and also apply the default behavior for the checkbox
            // }
        }

        private void MBHold_Click(object sender, EventArgs e)
        {
            List<int> recs = GetCheckedOrders();
            if (recs.Count() > 0)
            {
                foreach (int id in recs)
                {
                    ReplenOrder ord = repoOrders.FindByKey(id);
                    ord.OrderStatusId = 2;
                    repoOrders.Update(ord);
                }
            }
            ShowListing();
            ListBoxSelectedOrders.Items.Clear();
        }

        private void MBRelease_Click(object sender, EventArgs e)
        {
            List<int> recs = GetCheckedOrders();
            if (recs.Count() > 0)
            {
                foreach (int id in recs)
                {
                    ReplenOrder ord = repoOrders.FindByKey(id);
                    ord.OrderStatusId = 1;
                    repoOrders.Update(ord);
                }
            }
            ShowListing();
            ListBoxSelectedOrders.Items.Clear();
        }

        private List<int> GetCheckedOrders()
        {
            var orderIds = new List<int>();
            foreach (DataGridViewRow row in DataGridView1.Rows)
            {
                if (row.Cells["IsChecked"].Value != null && (bool) row.Cells["IsChecked"].Value == true)
                {
                    orderIds.Add((int) row.Cells["Id"].Value);
                }
            }
            if (orderIds.Count() == 0)
            {
                MessageBox.Show(text: "No Jobs Selected.");
            }
            return orderIds;
        }



        private void MBPickListBack_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Available Jobs";
            LabelFormTitle.BackColor = Color.Green;
            tabControl1.SelectedTab = AvailableOrders;
        }



        private void MBBack_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = OrderListing;
        }

        private void MbNewClose_Click_1(object sender, EventArgs e)
        {

        }

        private void MBAvailableOrdersBack_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Store";
            LabelFormTitle.BackColor = Color.Green;
            tabControl1.SelectedTab = Main;
        }

        private void MBGo_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Store List";
            LabelFormTitle.BackColor = Color.Green;
            tabControl1.SelectedTab = PickList;
            this.PickListLoad();
        }

        private void PickListLoad()
        {
            bool inventoryReady = true;
            string errorMsg = string.Empty;
            IEnumerable<ReplenPickView> pickViews = repo.GetOrderLines(ordersToPick);
            foreach (var item in pickViews)
            {
                List<Inventory> recs = GetInventory(item.ItemId);

                if (recs.Count > 0)
                {
                    var exactInventorySequence = new List<Inventory>();
                    if (Variables.UsePrimeBin)
                    {
                        //if there is a prime bin make it first, remove it from the list of inventory locations
                        Inventory prime = recs.Where(r => r.Location.Slot == item.OrderDetail.PrimeBin).FirstOrDefault();
                        if (prime != null)
                        {
                            exactInventorySequence.Add(prime);
                            recs.Remove(prime);
                        }
                    }
                    //sequence the inventory Recs by Received Date Descending 
                    recs.OrderByDescending(o => o.ReceivedDate).ToList();
                    foreach (var inv in recs)
                    {
                        exactInventorySequence.Add(inv);
                    }

                    item.CurrentInventoryLocation = exactInventorySequence.First();
                    item.Inventory = exactInventorySequence;
                    item.TotalQuantityInInventory = exactInventorySequence.Sum(r => r.Quantity);
                    item.Slot = item.CurrentInventoryLocation.Location.Slot;
                    item.SlotQty = item.SlotQty;
                    item.TotalQuantityInInventory = item.TotalQuantityInInventory;
                    item.InventoryIndex = 0;
                    item.ReceivedDate = item.CurrentInventoryLocation.ReceivedDate?.ToShortDateString() ?? "";
                }
                else
                {
                    inventoryReady = false;
                    errorMsg += "No Inventory Location for Item: " + item.Item + "\r\n";

                }
            }
            if (inventoryReady)
            {
                MBStart.Enabled = true;
                bindingSourceViewPick.DataSource = pickViews;
                DataGridViewPick.DataSource = bindingSourceViewPick;
                GetRecordCount(bindingSourceViewPick);
                DataGridViewPick.ClearSelection();
            }
            else
            {
                MessageBox.Show("You have to set these items up in Inventory before you can put them away.\r\n\r\n" + errorMsg, "No Inventory Locations", MessageBoxButtons.OK);
            }

        }

        private List<Inventory> GetInventory(int itemId)
        {
            var pickableLocations = new int[] { 1, 2 };
            var recs = new List<Inventory>();

            recs = repoInventory.AllInclude(l => l.Location, l => l.ItemDefinition)
                .Where(f => f.ItemDefinitionId == itemId && pickableLocations.Contains(f.StorageTypeId)).ToList();
            return recs;
        }

        //private void FrmStore_FormClosing(object sender, FormClosingEventArgs e)
        //{
        //    if (!CloseButtonPressed)
        //    {
        //        e.Cancel = true;
        //        return;
        //    }
        //}

        //select Available orders to Batch Positions
        private void DataGridViewAvailableOrders_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var chk = (DataGridViewCheckBoxCell) DataGridViewAvailableOrders.Rows[e.RowIndex].Cells[0];

                if (chk.Value == chk.TrueValue)
                {
                    DataGridViewAvailableOrders.Rows[e.RowIndex].Cells[0].Value = chk.FalseValue;
                    int id = Convert.ToInt32(DataGridViewAvailableOrders.Rows[e.RowIndex].Cells["Id"].Value);
                    RemoveItemFromBatch(id);
                }
                else
                {
                    DataGridViewAvailableOrders.Rows[e.RowIndex].Cells[0].Value = chk.TrueValue;
                    int id = Convert.ToInt32(DataGridViewAvailableOrders.Rows[e.RowIndex].Cells["Id"].Value);
                    if (id > 0)
                    {
                        string ord1 = DataGridViewAvailableOrders.Rows[e.RowIndex].Cells["Ord1"].Value.ToString();
                        AddItemToBatch(id, ord1);
                    }
                }
            }
        }

        private void RemoveItemFromBatch(int orderId)
        {
            foreach (BatchPosition bp in ordersToPick)
            {
                if (bp.OrderId == orderId)
                {
                    bp.OrderId = null;
                    bp.Ord1 = string.Empty;
                    UpdateTextBoxPosition(bp);
                }
            }
        }

        private void AddItemToBatch(int orderId, string ord1)
        {
            for (int i = 0; i < 12; i++)
            {
                if (ordersToPick[i].OrderId == null)
                {
                    ordersToPick[i].OrderId = orderId;
                    ordersToPick[i].Ord1 = ord1;
                    UpdateTextBoxPosition(ordersToPick[i]);
                    break;
                }
            }
        }

        private void ClearBatchPostions()
        {
            foreach (BatchPosition bp in ordersToPick)
            {
                bp.OrderId = null;
                bp.Ord1 = string.Empty;
                UpdateTextBoxPosition(bp);

            }
        }

        private void UpdateTextBoxPosition(BatchPosition bp)
        {
            string orderNumber = string.Empty;
            int pos = bp.PositionNumber;
            if (bp.OrderId != null)
            {
                var ordId = (int) bp.OrderId;
                orderNumber = repoOrders.FindByKey(ordId).Ord1;
                bp.Ord1 = orderNumber;
            }

            switch (pos)
            {
                case 1:
                    TextBoxPos1.Text = orderNumber;
                    CurrentTextBoxPos = TextBoxPos2;
                    break;
                case 2:
                    TextBoxPos2.Text = orderNumber;
                    CurrentTextBoxPos = TextBoxPos3;
                    break;
                case 3:
                    TextBoxPos3.Text = orderNumber;
                    CurrentTextBoxPos = TextBoxPos4;
                    break;
                case 4:
                    TextBoxPos4.Text = orderNumber;
                    CurrentTextBoxPos = TextBoxPos5;
                    break;
                case 5:
                    TextBoxPos5.Text = orderNumber;
                    CurrentTextBoxPos = TextBoxPos6;
                    break;
                case 6:
                    TextBoxPos6.Text = orderNumber;
                    CurrentTextBoxPos = TextBoxPos7;
                    break;
                case 7:
                    TextBoxPos7.Text = orderNumber;
                    CurrentTextBoxPos = TextBoxPos8;
                    break;
                case 8:
                    TextBoxPos8.Text = orderNumber;
                    CurrentTextBoxPos = TextBoxPos9;
                    break;
                case 9:
                    TextBoxPos9.Text = orderNumber;
                    CurrentTextBoxPos = TextBoxPos10;
                    break;
                case 10:
                    TextBoxPos10.Text = orderNumber;
                    CurrentTextBoxPos = TextBoxPos11;
                    break;
                case 11:
                    TextBoxPos11.Text = orderNumber;
                    CurrentTextBoxPos = TextBoxPos12;
                    break;
                case 12:
                    TextBoxPos12.Text = orderNumber;
                    CurrentTextBoxPos = TextBoxPos1;
                    break;
                default:
                    break;
            }

        }
        private void InitOrdersToPick()
        {
            for (int i = 0; i < 12; i++)
            {
                var bp = new BatchPosition() { PositionNumber = i + 1, OrderId = null, Ord1 = string.Empty };
                ordersToPick.Add(bp);
            }
        }

        private void MBShowOrderOrQuantityToggle_Click(object sender, EventArgs e)
        {
            if (MBShowOrderOrQuantityToggle.Text == "Show Jobs")
            {
                TextBoxPickPos1.Text = ordersToPick[0].Ord1;
                TextBoxPickPos2.Text = ordersToPick[1].Ord1;
                TextBoxPickPos3.Text = ordersToPick[2].Ord1;
                TextBoxPickPos4.Text = ordersToPick[3].Ord1;
                TextBoxPickPos5.Text = ordersToPick[4].Ord1;
                TextBoxPickPos6.Text = ordersToPick[5].Ord1;
                TextBoxPickPos7.Text = ordersToPick[6].Ord1;
                TextBoxPickPos8.Text = ordersToPick[7].Ord1;
                TextBoxPickPos9.Text = ordersToPick[8].Ord1;
                TextBoxPickPos10.Text = ordersToPick[9].Ord1;
                TextBoxPickPos11.Text = ordersToPick[10].Ord1;
                TextBoxPickPos12.Text = ordersToPick[11].Ord1;
                MBShowOrderOrQuantityToggle.Text = "Show Qty";
            }
            else
            {
                ClearPickPositions();
                MBShowOrderOrQuantityToggle.Text = "Show Jobs";
                UpdatePickScreen();
            }
        }

        private bool GetFirstStop(bool moveDevice = true)
        {
            int numberOfStops = bindingSourcePickStops.Count;
            if (numberOfStops > 0)
            {
                bindingSourcePickStops.MoveFirst();
                currentPickStop = (ReplenPickStop) bindingSourcePickStops.Current;
                UpdatePickScreen();
                if (neutronVariables.DisableShuttle == false)
                {
                    if (moveDevice)
                    {
                        int loc1 = currentPickStop.CurrentInventoryLocation.Location.Loc1;
                        int loc2 = currentPickStop.CurrentInventoryLocation.Location.Loc2;

                        DeviceResponse response = GlobalVar.Shuttle.PositionDevice(loc1, loc2);
                        if (response != DeviceResponse.Success)
                        {
                            MessageBox.Show(response.AsString(EnumFormat.Description), caption: "Device Information"
                                , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                        }
                    }
                }
                return true;
            }
            return false;
        }

        private bool GetNextStop(bool moveDevice = true)
        {
            int numberOfStops = bindingSourcePickStops.Count;
            if (currentPickStop.Sequence < numberOfStops)
            {
                bindingSourcePickStops.MoveNext();
                currentPickStop = (ReplenPickStop) bindingSourcePickStops.Current;
                UpdatePickScreen();
                if (neutronVariables.DisableShuttle == false)
                {
                    if (moveDevice)
                    {
                        int loc1 = currentPickStop.CurrentInventoryLocation.Location.Loc1;
                        int loc2 = currentPickStop.CurrentInventoryLocation.Location.Loc2;

                        DeviceResponse response = GlobalVar.Shuttle.PositionDevice(loc1, loc2);
                        if (response != DeviceResponse.Success)
                        {
                            MessageBox.Show(response.AsString(EnumFormat.Description), caption: "Device Information"
                                , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                        }
                    }
                }
                return true;
            }
            return false;
        }

        private bool GetPreviousStop(bool moveDevice = true)
        {
            int numberOfStops = bindingSourcePickStops.Count;
            if (currentPickStop.Sequence > 0)
            {
                bindingSourcePickStops.MovePrevious();
                currentPickStop = (ReplenPickStop) bindingSourcePickStops.Current;
                UpdatePickScreen();
                if (neutronVariables.DisableShuttle == false)
                {
                    if (moveDevice)
                    {
                        int loc1 = currentPickStop.CurrentInventoryLocation.Location.Loc1;
                        int loc2 = currentPickStop.CurrentInventoryLocation.Location.Loc2;

                        DeviceResponse response = GlobalVar.Shuttle.PositionDevice(loc1, loc2);
                        if (response != DeviceResponse.Success)
                        {
                            MessageBox.Show(response.AsString(EnumFormat.Description), caption: "Device Information"
                                , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                        }
                    }
                }
                return true;
            }
            return false;
        }

        private bool GetLastStop(bool moveDevice = true)
        {
            int numberOfStops = bindingSourcePickStops.Count;
            if (numberOfStops > 0)
            {
                bindingSourcePickStops.MoveLast();
                currentPickStop = (ReplenPickStop) bindingSourcePickStops.Current;
                UpdatePickScreen();
                if (neutronVariables.DisableShuttle == false)
                {
                    if (moveDevice)
                    {
                        int loc1 = currentPickStop.CurrentInventoryLocation.Location.Loc1;
                        int loc2 = currentPickStop.CurrentInventoryLocation.Location.Loc2;

                        DeviceResponse response = GlobalVar.Shuttle.PositionDevice(loc1, loc2);
                        if (response != DeviceResponse.Success)
                        {
                            MessageBox.Show(response.AsString(EnumFormat.Description), caption: "Device Information"
                                , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                        }
                    }
                }
                return true;
            }
            return false;
        }

        private void MBPickBack_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Store List";
            LabelFormTitle.BackColor = Color.Green;
            tabControl1.SelectedTab = PickList;
        }

        private void MBStart_Click(object sender, EventArgs e)
        {
            //bindingSourceViewPick.MoveFirst();
            //currentPickView = (ReplenPickView) bindingSourceViewPick.Current;
            //int loc1 = currentPickStop.CurrentInventoryLocation.Location.Loc1;
            //int loc2 = currentPickStop.CurrentInventoryLocation.Location.Loc2;

            // DeviceResponse response = GlobalVar.Shuttle.PositionDevice(loc1, loc2);
            //if (response != DeviceResponse.Success)
            //{
            //    MessageBox.Show(response.AsString(EnumFormat.Description), caption: "Device Information"
            //        , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
            //}
            //UpdatePickScreen();
            //tabControl1.SelectedTab = PickScreen;
            var recs = (IList<ReplenPickView>) bindingSourceViewPick.DataSource;

            //SetOrderStatusToPartial(recs);

            var pickStops = new List<ReplenPickStop>();
            IEnumerable<IGrouping<string, ReplenPickView>> pickViewGroups = recs.GroupBy(r => r.Item).ToList();
            int sequence = 0;
            foreach (var pickViewGroup in pickViewGroups)
            {
                int total = 0;

                var pickStop = new ReplenPickStop();
                ReplenPickView firstPickView = pickViewGroup.First();
                pickStop.Sequence = sequence += 1;
                pickStop.OrderId = firstPickView.ReplenOrderId;
                pickStop.Ord1 = firstPickView.Ord1;
                pickStop.Ord2 = firstPickView.Ord2;
                pickStop.ItemId = firstPickView.ItemId;
                pickStop.Item = firstPickView.Item;
                pickStop.Description = firstPickView.Description;
                pickStop.PickedQty = 0;
                pickStop.Images = firstPickView.Images;
                pickStop.Inventory = firstPickView.Inventory;
                pickStop.InventoryIndex = firstPickView.InventoryIndex;
                pickStop.Slot = firstPickView.Slot;
                pickStop.SlotQty = firstPickView.SlotQty;
                pickStop.CurrentInventoryLocation = firstPickView.CurrentInventoryLocation;
                pickStop.TotalQuantityInInventory = firstPickView.TotalQuantityInInventory;
                foreach (var pickView in pickViewGroup)
                {
                    pickStop.PickViews.Add(pickView);
                    total += pickView.Quantity;
                }
                pickStop.Quantity = total;
                pickStop.QuantityToBePicked = total;

                pickStops.Add(pickStop);
            }
            bindingSourcePickStops.DataSource = pickStops;
            GetFirstStop();

            tabControl1.SelectedTab = PickScreen;
        }

        private bool GetFirstStop()
        {
            int numberOfStops = bindingSourcePickStops.Count;
            if (numberOfStops > 0)
            {
                bindingSourcePickStops.MoveFirst();
                currentPickStop = (ReplenPickStop) bindingSourcePickStops.Current;
                UpdatePickScreen();
                if (neutronVariables.DisableShuttle == false)
                {
                    int loc1 = currentPickStop.CurrentInventoryLocation.Location.Loc1;
                    int loc2 = currentPickStop.CurrentInventoryLocation.Location.Loc2;
                    DeviceResponse response = GlobalVar.Shuttle.PositionDevice(loc1, loc2);
                    if (response != DeviceResponse.Success)
                    {
                        MessageBox.Show(response.AsString(EnumFormat.Description), caption: "Device Information"
                            , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                    }
                }
                return true;
            }
            return false;
        }

        private bool GetNextStop()
        {
            int numberOfStops = bindingSourcePickStops.Count;
            if (currentPickStop.Sequence < numberOfStops)
            {
                bindingSourcePickStops.MoveNext();
                currentPickStop = (ReplenPickStop) bindingSourcePickStops.Current;
                UpdatePickScreen();
                if (neutronVariables.DisableShuttle == false)
                {
                    int loc1 = currentPickStop.CurrentInventoryLocation.Location.Loc1;
                    int loc2 = currentPickStop.CurrentInventoryLocation.Location.Loc2;

                    DeviceResponse response = GlobalVar.Shuttle.PositionDevice(loc1, loc2);
                    if (response != DeviceResponse.Success)
                    {
                        MessageBox.Show(response.AsString(EnumFormat.Description), caption: "Device Information"
                            , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                    }
                }
                return true;
            }
            return false;
        }

        private bool GetPreviousStop()
        {
            int numberOfStops = bindingSourcePickStops.Count;
            if (currentPickStop.Sequence > 0)
            {
                bindingSourcePickStops.MovePrevious();
                currentPickStop = (ReplenPickStop) bindingSourcePickStops.Current;
                UpdatePickScreen();
                if (neutronVariables.DisableShuttle == false)
                {
                    int loc1 = currentPickStop.CurrentInventoryLocation.Location.Loc1;
                    int loc2 = currentPickStop.CurrentInventoryLocation.Location.Loc2;

                    DeviceResponse response = GlobalVar.Shuttle.PositionDevice(loc1, loc2);
                    if (response != DeviceResponse.Success)
                    {
                        MessageBox.Show(response.AsString(EnumFormat.Description), caption: "Device Information"
                            , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                    }
                }
                return true;
            }
            return false;
        }

        private bool GetLastStop()
        {
            int numberOfStops = bindingSourcePickStops.Count;
            if (numberOfStops > 0)
            {
                bindingSourcePickStops.MoveLast();
                currentPickStop = (ReplenPickStop) bindingSourcePickStops.Current;
                UpdatePickScreen();
                if (neutronVariables.DisableShuttle == false)
                {
                    int loc1 = currentPickStop.CurrentInventoryLocation.Location.Loc1;
                    int loc2 = currentPickStop.CurrentInventoryLocation.Location.Loc2;

                    DeviceResponse response = GlobalVar.Shuttle.PositionDevice(loc1, loc2);
                    if (response != DeviceResponse.Success)
                    {
                        MessageBox.Show(response.AsString(EnumFormat.Description), caption: "Device Information"
                            , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                    }
                }
                return true;
            }
            return false;
        }

        private void UpdatePickScreen()
        {
            UpdatePickPosition();
            UpdateInventoryLocation();
            UpdateImages();

            LabelPickDescription.Text = currentPickStop.Description;
            LabelPickItemNumber.Text = currentPickStop.Item;
            LabelLineOfLines.Text = string.Format("{0} of {1}"
                , currentPickStop.Sequence.ToString(), bindingSourcePickStops.Count);
            TextBoxRequestedQty.Text = currentPickStop.Quantity.ToString();

            int pickedSoFar = GetPickedSoFar(currentPickStop.PickViews);
            TextBoxPickedSoFar.Text = pickedSoFar.ToString();

            LabelPickQty.Text = (currentPickStop.QuantityToBePicked).ToString();
        }

        private void UpdateImages()
        {
            string path = Environment.ExpandEnvironmentVariables(name: @"%SystemDrive%\Neutron\Images\" + currentPickStop.Item + ".jpg");
            if (File.Exists(path))
            {
                PictureBoxItemImage.Load(path);
            }
        }

        private void UpdateInventoryLocation()
        {
            TextBoxPickLoc1.Text = currentPickStop.CurrentInventoryLocation.Location.Loc1.ToString();
            TextBoxPickLoc2.Text = currentPickStop.CurrentInventoryLocation.Location.Loc2.ToString();
            TextBoxPickLoc3.Text = currentPickStop.CurrentInventoryLocation.Location.Loc3.ToString();
            TextBoxPickLoc4.Text = currentPickStop.CurrentInventoryLocation.Location.Loc4.ToString();
            TextBoxPickLoc5.Text = currentPickStop.CurrentInventoryLocation.Location.Loc5.ToString();
            LabelLocationNumber.Text = string.Format(format: "{0} of {1}"
                , arg0: currentPickStop.InventoryIndex + 1, arg1: currentPickStop.Inventory.Count);
            TextBoxLocationQuantity.Text = currentPickStop.CurrentInventoryLocation.Quantity.ToString();
            TextBoxTotalQuantity.Text = currentPickStop.TotalQuantityInInventory.ToString();
            TextBoxReceivedDate.Text = currentPickStop.CurrentInventoryLocation.ReceivedDate?.ToShortDateString() ?? string.Empty;
            LabelPrimeBin.Visible = currentPickStop.CurrentInventoryLocation.PrimeBin;
        }

        public int GetTotalQuantityToBePicked(IList<ReplenPickView> pickViews)
        {
            int total = 0;
            foreach (var pickview in pickViews)
            {
                total += pickview.QuantityToBePicked;
            }
            return total;
        }

        private int GetPickedSoFar(IList<ReplenPickView> pickViews)
        {
            int total = 0;
            foreach (var pickview in pickViews)
            {
                total += GetPickViewTotal(pickview);
            }
            return total;
        }

        private int GetPickViewTotal(ReplenPickView pickview)
        {
            int total = 0;
            foreach (var pickLocation in pickview.PickLocations)
            {
                total += pickLocation.Quantity;
            }
            return total;
        }

        private void UpdatePickPosition()
        {
            ClearPickPositions();
            ClearPickDisplays();
            foreach (var pickView in currentPickStop.PickViews)
            {
                int pos = pickView.PickPosition;


                switch (pos)
                {
                    case 1:
                        TextBoxPickPos1.Text = pickView.QuantityToBePicked.ToString();
                        LabelPickPos1.BackColor = GetBackColor(pickView.QuantityToBePicked);
                        Pos1Display.BackColor = Color.Yellow;
                        break;
                    case 2:
                        TextBoxPickPos2.Text = pickView.QuantityToBePicked.ToString();
                        LabelPickPos2.BackColor = GetBackColor(pickView.QuantityToBePicked);
                        Pos2Display.BackColor = Color.Yellow;
                        break;
                    case 3:
                        TextBoxPickPos3.Text = pickView.QuantityToBePicked.ToString();
                        LabelPickPos3.BackColor = GetBackColor(pickView.QuantityToBePicked);
                        Pos3Display.BackColor = Color.Yellow;
                        break;
                    case 4:
                        TextBoxPickPos4.Text = pickView.QuantityToBePicked.ToString();
                        LabelPickPos4.BackColor = GetBackColor(pickView.QuantityToBePicked);
                        Pos4Display.BackColor = Color.Yellow;
                        break;
                    case 5:
                        TextBoxPickPos5.Text = pickView.QuantityToBePicked.ToString();
                        LabelPickPos5.BackColor = GetBackColor(pickView.QuantityToBePicked);
                        Pos5Display.BackColor = Color.Yellow;
                        break;
                    case 6:
                        TextBoxPickPos6.Text = pickView.QuantityToBePicked.ToString();
                        LabelPickPos6.BackColor = GetBackColor(pickView.QuantityToBePicked);
                        Pos6Display.BackColor = Color.Yellow;
                        break;
                    case 7:
                        TextBoxPickPos7.Text = pickView.QuantityToBePicked.ToString();
                        LabelPickPos7.BackColor = GetBackColor(pickView.QuantityToBePicked);
                        Pos7Display.BackColor = Color.Yellow;
                        break;
                    case 8:
                        TextBoxPickPos8.Text = pickView.QuantityToBePicked.ToString();
                        LabelPickPos8.BackColor = GetBackColor(pickView.QuantityToBePicked);
                        Pos8Display.BackColor = Color.Yellow;
                        break;
                    case 9:
                        TextBoxPickPos9.Text = pickView.QuantityToBePicked.ToString();
                        LabelPickPos9.BackColor = GetBackColor(pickView.QuantityToBePicked);
                        Pos9Display.BackColor = Color.Yellow;
                        break;
                    case 10:
                        TextBoxPickPos10.Text = pickView.QuantityToBePicked.ToString();
                        LabelPickPos10.BackColor = GetBackColor(pickView.QuantityToBePicked);
                        Pos10Display.BackColor = Color.Yellow;
                        break;
                    case 11:
                        TextBoxPickPos11.Text = pickView.QuantityToBePicked.ToString();
                        LabelPickPos11.BackColor = GetBackColor(pickView.QuantityToBePicked);
                        Pos11Display.BackColor = Color.Yellow;
                        break;
                    case 12:
                        TextBoxPickPos12.Text = pickView.QuantityToBePicked.ToString();
                        LabelPickPos12.BackColor = GetBackColor(pickView.QuantityToBePicked);
                        Pos12Display.BackColor = Color.Yellow;
                        break;
                    default:
                        break;
                }
            }
        }

        private Color GetBackColor(int quantityToBePicked)
        {
            if (quantityToBePicked == 0)
            {
                return Color.ForestGreen;
            }
            else
            {
                return Color.Green;
            }
        }

        private void ClearOrderPositions()
        {
            TextBoxPos1.Text = string.Empty;
            TextBoxPos2.Text = string.Empty;
            TextBoxPos3.Text = string.Empty;
            TextBoxPos4.Text = string.Empty;
            TextBoxPos5.Text = string.Empty;
            TextBoxPos6.Text = string.Empty;
            TextBoxPos7.Text = string.Empty;
            TextBoxPos8.Text = string.Empty;
            TextBoxPos9.Text = string.Empty;
            TextBoxPos10.Text = string.Empty;
            TextBoxPos11.Text = string.Empty;
            TextBoxPos12.Text = string.Empty;
        }


        private void ClearPickPositions()
        {
            TextBoxPickPos1.Text = string.Empty;
            TextBoxPickPos2.Text = string.Empty;
            TextBoxPickPos3.Text = string.Empty;
            TextBoxPickPos4.Text = string.Empty;
            TextBoxPickPos5.Text = string.Empty;
            TextBoxPickPos6.Text = string.Empty;
            TextBoxPickPos7.Text = string.Empty;
            TextBoxPickPos8.Text = string.Empty;
            TextBoxPickPos9.Text = string.Empty;
            TextBoxPickPos10.Text = string.Empty;
            TextBoxPickPos11.Text = string.Empty;
            TextBoxPickPos12.Text = string.Empty;
        }

        private void ClearPickDisplays()
        {
            Pos1Display.BackColor = Color.Transparent;
            Pos2Display.BackColor = Color.Transparent;
            Pos3Display.BackColor = Color.Transparent;
            Pos4Display.BackColor = Color.Transparent;
            Pos5Display.BackColor = Color.Transparent;
            Pos6Display.BackColor = Color.Transparent;
            Pos7Display.BackColor = Color.Transparent;
            Pos8Display.BackColor = Color.Transparent;
            Pos9Display.BackColor = Color.Transparent;
            Pos10Display.BackColor = Color.Transparent;
            Pos11Display.BackColor = Color.Transparent;
            Pos12Display.BackColor = Color.Transparent;
        }


        private void MBPickAccept_Click(object sender, EventArgs e)
        {
            currentPickStop.UpdatePickViews(Variables.user);  //good
            currentPickStop.PickedQty = GetPickedSoFar(currentPickStop.PickViews);
            currentPickStop.QuantityToBePicked = GetTotalQuantityToBePicked(currentPickStop.PickViews);  // QuantityToBePicked on ALL PickViews

            if (StopComplete())
            {
                UpdateInventoryQuantity();

                historyManager.SaveHistory(2, currentPickStop);

                CreateHostFile(currentPickStop);

                currentPickStop.SetPickViewsComplete(Variables.user);

                if (GetNextStop() == false)  //move to next stop or close the batch
                {
                    CloseBatch();
                }
                else
                {

                }
            }
            else  //PickStop is NOT complete, why?
            {

                //MessageBox.Show("Stop is Not complete.");
                UpdatePickScreen();
                //do you have another inventory location
                //yes
                //update the pick positions with the quantities
                //go to next invernoty locaiton
            }
            //UpdatePickScreen();



            //******
            //int pickedThisTime = (LabelPickQty.Text).ParseInt();
            //currentPickView.OrderDetail.NewBin = currentPickView.CurrentInventoryLocation.Location.Slot;
            //if (PickComplete(pickedThisTime)) 
            //{
            //    if (bindingSourceViewPick.Position + 1 < bindingSourceViewPick.Count)
            //    {
            //        bindingSourceViewPick.MoveNext();
            //        currentPickView = (ReplenPickView) bindingSourceViewPick.Current;

            //        int loc1 = currentPickStop.CurrentInventoryLocation.Location.Loc1;
            //        int loc2 = currentPickStop.CurrentInventoryLocation.Location.Loc2;

            //         DeviceResponse response = GlobalVar.Shuttle.PositionDevice(loc1, loc2);
            //if (response != DeviceResponse.Success)
            //{
            //    MessageBox.Show(response.AsString(EnumFormat.Description), caption: "Device Information"
            //        , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
            //}

            //        TextBoxPickedSoFar.Text = string.Empty;
            //        currentPickView.PickedQty = 0;
            //        LabelPickQty.Text = string.Empty;
            //        pickedSoFar = 0;
            //        UpdatePickScreen();
            //    }
            //    else
            //    {
            //        CloseBatch();
            //    }
            //}

            //UpdatePickScreen();
        }

        private void CreateHostFile(ReplenPickStop pickStop)
        {
            if (Variables.SendAllPicksToHost)
            {

            }

            foreach (ReplenPickView pickView in pickStop.PickViews)
            {
                var ord = new HostOrder();
                foreach (PickLocation pickLocation in pickView.PickLocations)
                {
                    // Didn't store in PrimeBin 
                    if (pickLocation.Inventory.Location.Slot != pickView.OrderDetail.PrimeBin)
                    {
                        //PickLocation pickLocation = pickView.PickLocations.FirstOrDefault();
                        if (pickLocation != null)
                        {
                            string newBin = pickLocation.Inventory.Location.Slot;
                            //Wasn't stored in Prime Bin 
                            ord = new HostOrder()
                            {
                                TypeCode = "0",
                                PartNum = pickView.OrderDetail.PartNum,
                                PartDesc = pickView.OrderDetail.PartDesc,
                                JobNum = pickView.OrderDetail.JobNum,
                                PrimeBin = pickView.OrderDetail.PrimeBin,
                                NewBin = newBin,
                                Qty = pickLocation.Quantity.ToString(),
                                TroubleBit = pickView.OrderDetail.TroubleBit,
                                DateTime = pickView.OrderDetail.DateTime,
                                EmpId = ($"EmpId:{Variables.user.EmpId} Note: Stored in Different Location")
                            };
                        }
                        var hostFile = new HostFile(ord);
                    }
                }
            }

        }

        private void UpdateInventoryQuantity()
        {
            currentPickStop.CurrentInventoryLocation.Quantity += currentPickStop.PickedQty;
            currentPickStop.CurrentInventoryLocation.ReceivedDate = DateTime.Now;
            repoInventory.Update(currentPickStop.CurrentInventoryLocation);

        }

        private bool StopComplete()
        {
            if (currentPickStop.QuantityToBePicked == 0)
            {
                return true;
            }
            else if (currentPickStop.QuantityToBePicked < 0)
            {
                DialogResult result = MessageBox.Show("Do you want to store more than the order indicates?", "Excess Inventory Question"
                    , MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    return true;
                }
                return false;
            }
            else if (currentPickStop.QuantityToBePicked > 0)
            {
                //does the stop have more than one inventory lcoation
                if (GetNextInventoryLocation())
                {
                    return false;
                }
                else  // no more locations
                {
                    MessageBox.Show("No more locations");
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        //private bool PickComplete(int pickedThisTime)
        //{
        //    pickedSoFar += pickedThisTime;
        //    if (pickedSoFar == currentPickView.Quantity)
        //    {
        //        UpdateInventoryQuantity(pickedThisTime);
        //        CreateHostFile(currentPickView);
        //        return true;
        //    }
        //    else if (pickedSoFar > currentPickView.Quantity)
        //    {
        //        DialogResult result = MessageBox.Show("You are storing more than is on the order.  Continue?", "Override Question"
        //            , MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
        //        if (result == DialogResult.Yes)
        //        {
        //            UpdateInventoryQuantity(pickedThisTime);
        //            return true;
        //        }
        //        return false;
        //    }
        //    else if (pickedSoFar < currentPickView.Quantity)
        //    {
        //        DialogResult result = MessageBox.Show("Do you want to do a partial store to this location?", "Partial Store Question"
        //           , MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
        //        if (result == DialogResult.Yes)
        //        {
        //            UpdateInventoryQuantity(pickedThisTime);
        //            return true;
        //        }
        //        else
        //        {
        //            //Do nothing and return to Store Screen
        //            return false;
        //        }
        //    }

        //    return false;
        //}

        private void CreateHostFile(ReplenPickView currentPickView)
        {
            //Always create the HostOrder record use it for History
            // and if something needs to go back to the Host, Send It

            //Wasn't picked out of Prime Bin so Return to Stock
            var ord = new HostOrder()
            {
                TypeCode = "0",
                PartNum = currentPickView.OrderDetail.PartNum,
                PartDesc = currentPickView.OrderDetail.PartDesc,
                JobNum = currentPickView.OrderDetail.JobNum,
                PrimeBin = currentPickView.OrderDetail.PrimeBin,
                NewBin = currentPickView.OrderDetail.NewBin,
                Qty = currentPickView.OrderDetail.Qty,
                TroubleBit = currentPickView.OrderDetail.TroubleBit,
                DateTime = currentPickView.OrderDetail.DateTime,
                EmpId = ($"EmpId:{Variables.user.EmpId} Note: Store Item is different location")
            };

            if (currentPickView.Item.Substring(0, 1) != "8")
            {
                if (ord.PrimeBin != ord.NewBin)
                {
                    var hostFile = new HostFile(ord);
                }
            }

        }

        private void UpdateInventoryQuantity(int pickedThisTime)
        {
            currentPickView.CurrentInventoryLocation.Quantity += pickedThisTime;
            repoInventory.Update(currentPickView.CurrentInventoryLocation);

        }

        private void CloseBatch()
        {
            //pickedSoFar = 0;

            // write results to history
            // wirte results to upload
            MarkCompleted(ordersToPick);
            ClearOrderPositions();
            ClearBatchPostions();
            ShowAvailableOrders();
            tabControl1.SelectedTab = AvailableOrders;
        }



        private void MarkCompleted(List<BatchPosition> ordersToPick)
        {
            foreach (BatchPosition ord in ordersToPick)
            {
                if (ord.OrderId != null)
                {
                    int id = ord.OrderId.Value;
                    ReplenOrder rec = repoOrders.FindByKey(id);
                    rec.OrderStatusId = 6;
                    repoOrders.Update(rec);
                    //ShowListing();
                }
            }
        }

        private void ButtonPreviousInventoryLocation_Click(object sender, EventArgs e)
        {
            GetPreviousInventoryLocation();
        }

        private void GetPreviousInventoryLocation()
        {
            if (currentPickStop.InventoryIndex - 1 < 0)
            {
                currentPickStop.InventoryIndex = 0;
            }
            else
            {
                currentPickStop.InventoryIndex -= 1;
            }
            currentPickStop.CurrentInventoryLocation = currentPickStop.Inventory[currentPickStop.InventoryIndex];
            UpdateInventoryLocation();
        }


        private void ButtonNextInventoryLocation_Click(object sender, EventArgs e)
        {
            bool result = GetNextInventoryLocation();
        }

        private bool GetNextInventoryLocation()
        {
            bool result = false;
            if (currentPickStop.InventoryIndex + 1 < currentPickStop.Inventory.Count)
            {
                currentPickStop.InventoryIndex += 1;
                currentPickStop.CurrentInventoryLocation = currentPickStop.Inventory[currentPickStop.InventoryIndex];
                UpdateInventoryLocation();
                result = true;
            }
            currentPickStop.CurrentInventoryLocation = currentPickStop.Inventory[currentPickStop.InventoryIndex];
            UpdateInventoryLocation();
            return result;
        }

        private void MBPickChangeQuantity_Click(object sender, EventArgs e)
        {
            int newQty = 0;
            int pos = 0;
            using (FrmChangeStoreQuantity form = new FrmChangeStoreQuantity(currentPickStop))
            {
                DialogResult result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    newQty = form.NewQty;
                    pos = form.Position;
                    UpdateCurrentPickStopQuantities(pos, newQty);
                }
            }
        }

        private void UpdateCurrentPickStopQuantities(int pos, int newQty)
        {
            foreach (var pickView in currentPickStop.PickViews)
            {
                if (pickView.PickPosition == pos)
                {
                    pickView.QuantityToBePicked = newQty;
                }
            }
            currentPickStop.QuantityToBePicked = currentPickStop.GetTotalQuantityToBePicked();
            UpdatePickScreen();
        }

        private void ButtonMove_Click(object sender, EventArgs e)
        {
            if (neutronVariables.DisableShuttle == false)
            {
                int loc1 = currentPickStop.CurrentInventoryLocation.Location.Loc1;
                int loc2 = currentPickStop.CurrentInventoryLocation.Location.Loc2;

                DeviceResponse response = GlobalVar.Shuttle.PositionDevice(loc1, loc2);
                if (response != DeviceResponse.Success)
                {
                    MessageBox.Show(response.AsString(EnumFormat.Description), caption: "Device Information"
                        , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                }
            }
        }

        private void MBPickHotPick_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Hot Store";
            LabelFormTitle.BackColor = Color.Green;

            tabControl1.SelectedTab = HotPick;
        }

        private void MBPickHotStore_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Hot Store";
            LabelFormTitle.BackColor = Color.Green;

            tabControl1.SelectedTab = HotPick;
        }
        private void MBPriority_Click(object sender, EventArgs e)
        {
            int priority = 0;
            using (FrmChangePriority form = new FrmChangePriority())
            {

                DialogResult result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    priority = (form.NewPriority).ParseInt();
                }
            }
            List<int> recs = GetCheckedOrders();
            if (recs.Count() > 0)
            {
                foreach (int id in recs)
                {
                    ReplenOrder ord = repoOrders.FindByKey(id);
                    ord.Priority = priority;
                    repoOrders.Update(ord);
                }
            }
            ShowListing();
            ListBoxSelectedOrders.Items.Clear();
        }

        private void MBDelete_Click(object sender, EventArgs e)
        {
            List<int> recs = GetCheckedOrders();
            if (recs.Count() > 0)
            {
                foreach (int id in recs)
                {
                    ReplenOrder ord = repoOrders.AllInclude(s => s.ReplenOrderDetails).Where(r => r.Id == id).FirstOrDefault();
                    if (ord != null)
                    {
                        foreach (ReplenOrderDetail orderDetail in ord.ReplenOrderDetails)
                        {
                            orderDetail.LineStatusId = 5;
                            repoOrderDetails.Delete(orderDetail.Id);
                            historyManager.SaveHistory(37, orderDetail);
                        }
                        
                            ord.OrderStatusId = 5;
                            repoOrders.Delete(ord.Id);
                            historyManager.SaveHistory(36, ord);
                        
                    }

                    //repoOrders.Delete(id);
                }
               // bindingSourceCompleted.DataSource = ordersRepository.GetCompletedOrders();
               // DataGridView1.DataSource = bindingSourceCompleted;
               // DataGridView1.ClearSelection();
            }
            ListBoxSelectedOrders.Items.Clear();
            ShowListing();

        }

        private void MBOrderDetails_Click(object sender, EventArgs e)
        {
            var details = new List<ReplenOrderDetailsView>();
            List<int> recs = GetCheckedOrderIds();
            if (recs.Count() > 0)
            {
                details = replenOrderDetailsRepository.GetOrderDetailsViewByOrder(recs[0]);

                bindingSourceOrderDetailsView.DataSource = details;
                DataGridViewOrderDetails.DataSource = bindingSourceOrderDetailsView;
                LabelFormTitle.Text = "Job Details";
                tabControl1.SelectedTab = OrderDetails;

                //var details = new List<OrderDetailsView>();

                //if (CurrentItem.ReplenOrder.ReplenOrderDetails.Count > 0)
                //{
                //    foreach (var item in CurrentItem.ReplenOrder.ReplenOrderDetails)
                //    {
                //        var detail = new OrderDetailsView();
                //        detail.OrderId = CurrentItem.Id;
                //        detail.Ord1 = CurrentItem.Ord1;
                //        detail.Ord2 = CurrentItem.Ord2;
                //        detail.OrderDetailId = item.Id;
                //        detail.ItemId = item.ItemDefinitionId;
                //        detail.Item = item.ItemDefinition.Item;
                //        detail.Description = item.ItemDefinition.Description;
                //        detail.Quantity = item.Quantity;
                //        details.Add(detail);
                //    }
                //    bindingSourceOrderDetailsView.DataSource = details;
                //    DataGridViewOrderDetails.DataSource = bindingSourceOrderDetailsView;
                //}
                //LabelFormTitle.Text = "Job Details";
                //tabControl1.SelectedTab = OrderDetails;
            }
        }

        private List<int> GetCheckedOrderIds()
        {
            //List<OrderView> ordView = DataGridView1.SelectedRows.ToList<OrderView>();
            var orderIds = new List<int>();
            foreach (DataGridViewRow row in DataGridView1.Rows)
            {
                if (row.Cells["IsChecked"].Value != null && (bool) row.Cells["IsChecked"].Value == true)
                {
                    orderIds.Add((int) row.Cells["Id"].Value);
                }
            }
            if (orderIds.Count() == 0)
            {
                MessageBox.Show(text: "No Jobs Selected.");
            }
            return orderIds;
        }

        private void MBCreateOrder_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "New Job";
            tabControl1.SelectedTab = NewOrder;
        }

        private void MBFindItem_Click(object sender, EventArgs e)
        {
            FindHotRecord(TextBoxFindItem.Text.Trim().ToLower());
        }

        private void FindHotRecord(string s)
        {
            ItemDefinition itemDefinition;
            IEnumerable<SqlInventoryView> views = repoInv.FindInventoryViewsByStation(s, station.StationId);
            var hotList = new List<HotStoreListView>();
            string findWhat = s.ToLower().Trim();
            if (!string.IsNullOrEmpty(findWhat))
            {
                try
                {
                    itemDefinition = repoItemDefinition.FindBy(r => r.Item == findWhat).FirstOrDefault();
                    if (itemDefinition != null)
                    {
                        currentItem = itemDefinition;
                        List<SqlInventoryView> viewsByItem = views.Where(d => d.ItemDefinitionId == itemDefinition.Id).ToList();
                        if (viewsByItem.Count() > 0)
                        {
                            hotList = viewsByItem.Select(r => new HotStoreListView
                            {
                                Id = r.Id,
                                Item = r.Item,
                                Description = r.Description,
                                Quantity = r.Quantity,
                                Slot = r.Slot,
                                PrimeBin = r.PrimeBin

                            }).OrderBy(o => o.Item).ThenBy(p => p.Slot).ToList();
                            hotBindingSource.DataSource = hotList;
                            DataGridViewInventory.DataSource = hotBindingSource;
                            DataGridViewInventory.ClearSelection();

                        }
                        else
                        {
                            hotBindingSource.DataSource = null;
                            DataGridViewInventory.DataSource = hotBindingSource;
                            DialogResult result = MessageBox.Show(text: "Item is defined, but it is not set up in Inventory.\r\nDo you want to set it up now? "
, caption: "New Inventory Item", buttons: MessageBoxButtons.YesNo, icon: MessageBoxIcon.Question);
                            if (result == DialogResult.Yes)
                            {

                                if (securityProcessor.SecurityProfile[(int) NeutronSecurity.ManageInventory])
                                {
                                    this.Hide();
                                    using (MetroForm frm = new FrmInventory(jsonData, station, akaRepository))
                                    {
                                        DialogResult result1 = frm.ShowDialog();
                                        this.Show();
                                    }
                                }
                            }

                        }
                    }
                    else
                    {
                        hotBindingSource.DataSource = null;
                        DataGridViewInventory.DataSource = hotBindingSource;
                        MessageBox.Show("Item Definition not found.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hot Find Error: " + ex.Message + "\r\n" + ex.InnerException.Message);
                }
            }
            else
            {
                if (views.Count() > 0)
                {
                    hotList = views.Select(r => new HotStoreListView
                    {
                        Id = r.Id,
                        Item = r.Item,
                        Description = r.Description,
                        Quantity = r.Quantity,
                        Slot = r.Slot,
                        PrimeBin = r.PrimeBin

                    }).OrderBy(o => o.Item).ThenBy(p => p.Slot).ToList();
                    hotBindingSource.DataSource = hotList;
                    DataGridViewInventory.DataSource = hotBindingSource;
                }
                else
                {
                    hotBindingSource.DataSource = null;
                    DataGridViewInventory.DataSource = hotBindingSource;
                }
            }
        }

        private void TextBoxFindItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                FindHotRecord(TextBoxFindItem.Text.Trim().ToLower());
            }
            if (e.KeyCode == Keys.Escape)
            {
                TextBoxFindItem.Text = "";
            }
        }

        private void ButtonHotPickClear_Click(object sender, EventArgs e)
        {
            TextBoxFindItem.Text = string.Empty;
            FindHotRecord(string.Empty);
            TextBoxFindItem.Focus();
        }

        private void MBMainOrderManager_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Job Listing";
            LabelFormTitle.BackColor = Color.Green;
            tabControl1.SelectedTab = OrderListing;
        }

        private void MBMainAvailableOrders_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Available Jobs";
            LabelFormTitle.BackColor = Color.Green;
            ClearOrderPositions();
            ShowAvailableOrders();
            tabControl1.SelectedTab = AvailableOrders;
        }

        private void MBMainHotPick_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Hot Store";
            LabelFormTitle.BackColor = Color.Green;
            hotBindingSource.DataSource = repoInv.GetHotStoreList(string.Empty);
            DataGridViewInventory.DataSource = hotBindingSource;
            tabControl1.SelectedTab = HotPick;
        }
        //New Order
        private void MBMainNewOrder_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "New Job";
            LabelFormTitle.BackColor = Color.Green;
            bindingSourceItems.DataSource = GetItemsList();
            DataGridViewNewOrder.DataSource = bindingSourceItems;
            tabControl1.SelectedTab = NewOrder;
            DataGridViewNewOrder.ClearSelection();
        }
        //New Order
        private List<NewItemView> GetItemsList()
        {
            var recs = new List<NewItemView>();
            recs = repoItemDefinition.All().Select(d => new NewItemView() { Id = d.Id, Description = d.Description, Item = d.Item }).ToList();
            return recs;
        }

        private void MBMainLoadOrders_Click(object sender, EventArgs e)
        {
            if (neutronVariables.LoaderRunning)
            {
                DialogResult result = MessageBox.Show("Do you want to stop the loader?", "Loader", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    MBMainLoadOrders.Text = "Start Loader";
                    if (interfaceProcessor != null)
                    {
                        interfaceProcessor.StopProcessingInterfaceFiles();

                    }


                    neutronVariables.LoaderRunning = false;
                    jsonData.SaveFile<NeutronVariables>(neutronVariables);
                }
            }
            else
            {
                interfaceProcessor = new InterfaceProcessor(jsonData);
                interfaceProcessor.StartProcessingInterfaceFiles();
                MBMainLoadOrders.Text = "Stop Loader";
                neutronVariables.LoaderRunning = true;
                jsonData.SaveFile<NeutronVariables>(neutronVariables);
            }
        }

        private void MBMainClose_Click(object sender, EventArgs e)
        {
            CloseButtonPressed = true;
        }

        private void MBBackHotPick_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Store";
            LabelFormTitle.BackColor = Color.Green;
            tabControl1.SelectedTab = Main;
        }

        private void MBHotPickPickBack_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Hot Store";
            LabelFormTitle.BackColor = Color.Green;
            tabControl1.SelectedTab = HotPick;
        }

        private void MBNewOrderClose_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Job Listing";
            LabelFormTitle.BackColor = Color.Green;
            tabControl1.SelectedTab = OrderListing;
        }

        private void MBOrderDetailsBack_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Job Listing";
            LabelFormTitle.BackColor = Color.Green;
            tabControl1.SelectedTab = OrderListing;
        }

        private void DataGridViewInventory_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                int id = ((HotStoreListView) hotBindingSource.Current).Id;
                currentInventoryView = repoInv.GetInventoryViewById(id);
                if (neutronVariables.DisableShuttle == false)
                {
                    int loc1 = currentInventoryView.Loc1;
                    int loc2 = currentInventoryView.Loc2;
                    
                    DeviceResponse response = GlobalVar.Shuttle.PositionDevice(loc1, loc2);
                    if (response != DeviceResponse.Success)
                    {
                        MessageBox.Show(response.AsString(EnumFormat.Description), caption: "Device Information"
                            , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                    }
                }
                UpdateHotPickScreen(currentInventoryView);
                tabControl1.SelectedTab = HotPickPick;
            }
        }

        private void UpdateHotPickScreen(InventoryView invItem)
        {
            LabelHotPickDescription.Text = invItem.Description;
            LabelHotPickItem.Text = invItem.Item;
            TextBoxHotPickLoc1.Text = invItem.Loc1.ToString();
            TextBoxHotPickLoc2.Text = invItem.Loc2.ToString();
            TextBoxHotPickLoc3.Text = invItem.Loc3.ToString();
            TextBoxHotPickLoc4.Text = invItem.Loc4.ToString();
            TextBoxHotPickLoc5.Text = invItem.Loc5.ToString();
            TextBoxHotPickLocationQuantity.Text = invItem.Quantity.ToString();
            TextBoxHotPickQuantity.Text = "1";
            TextBoxHotPickQuantity.Focus();
        }

        private void ButtonLocationCount_Click(object sender, EventArgs e)
        {
            int inventoryId = currentInventoryView.Id;
            int qty = OpenLocationCountForm(inventoryId);
            //refresh the datasource using the Search function
            FindHotRecord(TextBoxFindItem.Text.Trim().ToLower());
            SetCurrentInventoryView(inventoryId);

            UpdateHotPickScreen(currentInventoryView);
        }

        private int OpenLocationCountForm(int inventoryId)
        {
            int qty = -1;
            using (FrmLocationCount form = new FrmLocationCount())
            {
                DialogResult result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    qty = (form.NewQty).ParseInt();
                    LocationCount(inventoryId, qty);
                }
            }
            return qty;
        }

        private void LocationCount(int inventoryId, int qty)
        {
            Inventory inv = repoInventory.FindByKey(inventoryId);
            int prevQty = inv.Quantity;
            inv.Quantity = qty;
            repoInventory.Update(inv);
            var cnt = new LocationCount()
            {
                InventoryId = inv.Id,
                ItemDefinitionId = inv.ItemDefinitionId,
                LocationId = inv.LocationId,
                UserId = Variables.user.Id,
                PreviousQty = prevQty,
                NewQty = qty,
                CountDate = System.DateTime.Now,
            };
            repoLocationCount.Insert(cnt);
        }

        private void SetCurrentInventoryView(int inventoryId)
        {
            InventoryView rec = hotBindingSource.List.OfType<InventoryView>().ToList().Find(f => f.Id == inventoryId);
            int pos = hotBindingSource.IndexOf(rec);
            hotBindingSource.Position = pos;
            currentInventoryView = (InventoryView) hotBindingSource.Current;
        }

        private void MBHotPickAccept_Click(object sender, EventArgs e)
        {
            int pickQty = (TextBoxHotPickQuantity.Text).ParseInt();
            Inventory inv = repoInventory.FindByKey(currentInventoryView.Id);
            inv.Quantity += pickQty;
            repoInventory.Update(inv);
            FindHotRecord(TextBoxFindItem.Text.Trim().ToLower());
            tabControl1.SelectedTab = HotPick;
        }

        //private void MButtonPick_Click(object sender, EventArgs e)
        //{
        //    //IEnumerable<ReplenPickView> picks = ordersRepository.GetOrderLines();
        //    //DataGridViewPickList.DataSource = picks;

        //    bindingSourceAvailableOrders.DataSource = ordersRepository.GetAvailableOrders();
        //    DataGridViewAvailableOrders.DataSource = bindingSourceAvailableOrders;

        //    //tabControl1.SelectedTab = AvailableOrders;
        //    tabControl1.SelectedIndex = 1;
        //    //var orderIdArray = new List<int>();
        //    //if (ListBoxSelectedOrders.Items.Count > 0)
        //    //{
        //    //    foreach (Order ord in ListBoxSelectedOrders.Items)
        //    //    {
        //    //        orderIdArray.Add(ord.Id);
        //    //    }
        //    //    IEnumerable<ReplenPickView> picks = ordersRepository.GetOrderLines();
        //    //    DataGridViewPick.DataSource = picks;
        //    //    tabControl1.SelectedTab = tabPage2;
        //    //}
        //}

        private void MButtonCompleted_Click(object sender, EventArgs e)
        {
            if (MButtonCompleted.Text == "Completed")
            {
                bindingSourceCompleted.DataSource = repo.GetCompletedOrders();
                DataGridView1.DataSource = bindingSourceCompleted;
                MButtonCompleted.Text = "Jobs";
            }
            else if (MButtonCompleted.Text == "Jobs")
            {
                ShowListing();
                MButtonCompleted.Text = "Completed";
            }
            DataGridView1.ClearSelection();
        }

        private void MBLocationCount_Click(object sender, EventArgs e)
        {
            int inventoryId = currentPickStop.CurrentInventoryLocation.Id;
            int qty = OpenLocationCountForm(inventoryId);

            TextBoxLocationQuantity.Text = qty.ToString();
            currentPickStop.CurrentInventoryLocation.Quantity = qty;
            int total = currentPickStop.Inventory.Sum(r => r.Quantity);
            TextBoxTotalQuantity.Text = total.ToString();
        }
        //New Order
        private void MBNewOrderSearch_Click(object sender, EventArgs e)
        {
            FindItemRecord(TextBoxNewOrderFind.Text.Trim().ToLower());

        }

        private void FindItemRecord(string s)
        {
            try
            {
                if (string.IsNullOrEmpty(s))
                {
                    bindingSourceItems.DataSource = GetItemsList();
                    DataGridViewNewOrder.DataSource = bindingSourceItems;
                }
                else
                {
                    IEnumerable<NewItemView> projection = GetItemsList();
                    bindingSourceItems.DataSource = projection
                        .Where(d => d.Item.ToLower().Contains(s) || d.Description.ToLower().Contains(s)).ToList();
                    DataGridViewNewOrder.DataSource = bindingSourceItems;
                }
                DataGridViewNewOrder.ClearSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Find Error: " + ex.Message);
            }
        }

        private void TextBoxNewOrderFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                FindItemRecord(TextBoxNewOrderFind.Text.Trim().ToLower());
            }
            if (e.KeyCode == Keys.Escape)
            {
                TextBoxNewOrderFind.Text = "";
            }
        }

        private void ButtonNewOrderClear_Click(object sender, EventArgs e)
        {
            TextBoxNewOrderFind.Text = string.Empty;
            TextBoxNewOrderFind.Focus();
        }

        private void DataGridViewNewOrder_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                var currentItem = (NewItemView) bindingSourceItems.Current;
                LabelNewOrderItemId.Text = currentItem.Id.ToString();
                LabelNewOrderItem.Text = currentItem.Item;
                LabelNewOrderDescription.Text = currentItem.Description;
                TextBoxNewOrderQuantity.Focus();
            }
        }

        private void ButtonAddDetail_Click(object sender, EventArgs e)
        {
            var rec = new NewItemView()
            {
                Id = (LabelNewOrderItemId.Text).ParseInt()
                ,
                Item = LabelNewOrderItem.Text,
                Description = LabelNewOrderDescription.Text
                ,
                Quantity = (TextBoxNewOrderQuantity.Text).ParseInt()
            };
            var item = new ListViewItem(new[] { rec.Id.ToString(), rec.Item, rec.Description, rec.Quantity.ToString() });

            ListViewNewItems.Items.Add(item);
        }

        private void InitListView()
        {
            ListViewNewItems.Columns.Add("Id", 80, HorizontalAlignment.Left);
            ListViewNewItems.Columns.Add("Item", 80, HorizontalAlignment.Left);
            ListViewNewItems.Columns.Add("Description", 200, HorizontalAlignment.Left);
            ListViewNewItems.Columns.Add("Quantity", 80, HorizontalAlignment.Left);
        }

        private void MBLoadOrders_Click(object sender, EventArgs e)
        {

        }

        private void MBNewOrderSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(TextBoxNewOrderOrd1.Text) && ListViewNewItems.Items.Count > 0)
                {
                    DateTime dateTime = DateTime.Now;

                    var orderDetails = new List<ReplenOrderDetail>();

                    var order = new ReplenOrder()
                    {
                        Ord1 = TextBoxNewOrderOrd1.Text,
                        Ord2 = TextBoxNewOrderOrd2.Text,
                        Priority = (TextBoxNewOrderPriority.Text).ParseInt(),
                        LoadDate = dateTime,
                        ShipperId = 1,
                        ShipMethodId = 1,
                        OrderStatusId = 1
                    };
                    repoOrders.Insert(order);

                    for (int i = 0; i < ListViewNewItems.Items.Count; i++)
                    {
                        int qty = (ListViewNewItems.Items[i].SubItems[3].Text).ParseInt();
                        int itemDefinitionId = (ListViewNewItems.Items[i].SubItems[0].Text).ParseInt();
                        ItemDefinition itemDef = repoItemDefinition.FindByKey(itemDefinitionId);
                        string primeBin = repoInv.GetPrimeBin(itemDefinitionId);
                        // List<Inventory> inventory = repoInventory.FindBy(r => r.ItemDefinitionId == itemDefinitionId).ToList();

                        var rec = new ReplenOrderDetail();
                        rec.ReplenOrderId = order.Id;
                        rec.ItemDefinitionId = itemDefinitionId;
                        rec.Quantity = qty;
                        rec.DateTime = dateTime.ToString(format: "yyyyMMdd");
                        rec.EmpId = Variables.user.EmpId;
                        rec.JobNum = TextBoxNewOrderOrd1.Text;
                        rec.NewBin = string.Empty;
                        rec.PartDesc = itemDef.Description;
                        rec.PartNum = itemDef.Item;
                        rec.PrimeBin = primeBin;
                        rec.Qty = qty.ToString();
                        rec.TroubleBit = string.Empty;
                        rec.TypeCode = "3";
                        rec.LineStatusId = 1;
                        rec.PickedQuantity = 0;
                        rec.StationNumber = repoInv.GetStationNumber(itemDefinitionId);
                        repoOrderDetails.Insert(rec);
                    }
                    TextBoxNewOrderOrd1.Text = string.Empty;
                    TextBoxNewOrderOrd2.Text = string.Empty;
                    ListViewNewItems.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save Replenishment Order Failure.  " + ex.Message + " " + ex.InnerException, "Replenishment Orders", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MBStoreRefresh_Click(object sender, EventArgs e)
        {
            ShowListing();
        }

        private void ButtonImagePrevious_Click(object sender, EventArgs e)
        {

        }

        private void ButtonImageNext_Click(object sender, EventArgs e)
        {

        }

        private void ButtonStopMoveFirst_Click(object sender, EventArgs e)
        {
            GetFirstStop(moveDevice: false);
        }

        private void ButtonStopMovePrevious_Click(object sender, EventArgs e)
        {
            GetPreviousStop(moveDevice: false);
        }

        private void ButtonStopMoveNext_Click(object sender, EventArgs e)
        {
            GetNextStop(moveDevice: false);
        }

        private void ButtonStopMoveLast_Click(object sender, EventArgs e)
        {
            GetLastStop(moveDevice: false);
        }

        private void TextBoxFindAvailableOrders_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void MBPrintOrderListing_Click(object sender, EventArgs e)
        {
            CsvUtility.SaveToCsv(DataGridView1);
        }

        private void MbPrintAvailableOrders_Click(object sender, EventArgs e)
        {
            CsvUtility.SaveToCsv(DataGridViewAvailableOrders);
        }

        private void MBPrintPickList_Click(object sender, EventArgs e)
        {
            CsvUtility.SaveToCsv(DataGridViewPick);
        }

        private void MBPrintOrderDetails_Click(object sender, EventArgs e)
        {
            CsvUtility.SaveToCsv(DataGridViewOrderDetails);
        }
    }
}