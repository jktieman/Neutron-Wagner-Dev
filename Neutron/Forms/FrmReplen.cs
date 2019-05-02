using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
using System.Threading;
using Neutron.Controllers;
using Neutron.Interfaces;
using System.Diagnostics;
using System.Globalization;
using JsonManager;
using Neutron.Enums;
using Neutron.Classes;
using Equin.ApplicationFramework;
using EnumsNET;
using System.IO;
using System.Resources;
using NeutronLoader;
using NeutronCore.Global;
using NeutronCore;
using AlliedLogger;
using System.Text;
using PrintRequest;
using NeutronData.SqlModelViews;
using NeutronData.Interfaces;
using NeutronCore.Models;
using NeutronCore.Enums;

namespace Neutron.Forms
{
    public partial class FrmReplen : MetroForm
    {
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;

        private GenericRepository<Order> repoOrders = new GenericRepository<Order>(new NeutronDb());
        private GenericRepository<OrderDetail> repoOrderDetails = new GenericRepository<OrderDetail>(new NeutronDb());
        private GenericRepository<Inventory> repoInventory = new GenericRepository<Inventory>(new NeutronDb());
        private InventoryRepository repoInv = new InventoryRepository();
        private GenericRepository<LocationCount> repoLocationCount = new GenericRepository<LocationCount>(new NeutronDb());
        private GenericRepository<ItemDefinition> repoItemDefinition = new GenericRepository<ItemDefinition>(new NeutronDb());
        //private GenericRepository<History> repoHistory = new GenericRepository<History>(new NeutronDb());
        private readonly GenericRepository<ReplenOrder> repoReplenOrder = new GenericRepository<ReplenOrder>(new NeutronDb());
        private readonly GenericRepository<ReplenOrderDetail> repoReplenOrderDetail = new GenericRepository<ReplenOrderDetail>(new NeutronDb());
        private ReplenOrdersRepository ordersRepository = new ReplenOrdersRepository();
        private ReplenOrderDetailsRepository orderDetailsRepository = new ReplenOrderDetailsRepository();
        private GenericRepository<HardwareDevice> repoHardwareDevices = new GenericRepository<HardwareDevice>(new NeutronDb());
        private StationRepository repoStation = new StationRepository();
        private GenericRepository<PrintJob> repoPrintJob = new GenericRepository<PrintJob>(new NeutronDb());

        private BindingListView<ReplenOrderView> bindingSourceOrderViewEquin;
        private BindingListView<ReplenOrderView> bindingSourceAvailableOrdersEquin;

        private BindingSource bindingSourceOrderView = new BindingSource();
        private BindingSource bindingSourceCompleted = new BindingSource();
        private BindingSource bindingSourceAvailableOrders = new BindingSource();
        private BindingSource bindingSourcePickViews = new BindingSource();
        private BindingSource bindingSourcePickStops = new BindingSource();
        private BindingSource bindingSourceHot = new BindingSource();
        private BindingSource bindingSourceOrderDetailsView = new BindingSource();
        //New ReplenOrder
        private BindingSource bindingSourceItems = new BindingSource();
        private BindingSource bindingSourceNewItems = new BindingSource();
        //----
        public bool CloseButtonPressed { get; set; }
        public ReplenOrderView CurrentItem;
        public TextBox CurrentTextBoxPos;
        bool ManualOverrideCurrentTextBoxPos = false;
        public bool CloseForm = false;
        private List<BatchPosition> _ordersToPick = new List<BatchPosition>();
        ReplenPickStop currentPickStop = new ReplenPickStop();
        private SqlInventoryView currentInventoryView = new SqlInventoryView();

        private bool gridClickedAvailableOrders = false;
        private string textToFind = string.Empty;
        private bool openHotPickFromPickScreen = false;
        private bool openHotStoreFromPickScreen = false;

        private InterfaceProcessor interfaceProcessor;
        private readonly NeutronVariables _neutronVariables;
        NeutronLicense neutronLicense;
        DynamicLogger _logger;
        readonly string _imagesDirectory = string.Empty;

        ReplenDeviceManager _deviceManager;
        DocumentPrinterPreferences _documentPrinter;
        LabelPrinterPreferences _labelPrinter;

        private readonly IJsonData _jsonData;
        private readonly StationView _station;
        private readonly IAkaRepository _akaRepository;
        private readonly INomenclature _nomenclature;
        private readonly ISecurityProcessor _securityProcessor;
        private readonly ILacProcessor _lacProcessor;


        public FrmReplen(IJsonData jsonData, StationView station
            , IAkaRepository akaRepository, NeutronVariables neutronVariables, INomenclature nomenclature
            , ISecurityProcessor securityProcessor, ILacProcessor lacProcessor)
        {
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);

            SetupLogger();
            KeyPreview = true;
            _station = station;
            _jsonData = jsonData;
            _neutronVariables = neutronVariables;
            neutronLicense = jsonData.LoadFile<NeutronLicense>();
            _akaRepository = akaRepository;
            _nomenclature = nomenclature;
            _securityProcessor = securityProcessor;
            _lacProcessor = lacProcessor;

            UpdateNomenclature();
            SetupPrinters();
            SetupGrids();
            SetupListBoxes();
            ShowButtons();
            HideTabControlTabs();
            mlUserInfo.Text = GlobalVar.User?.UserInfo;
            CloseButtonPressed = false;
            CurrentTextBoxPos = TextBoxPos1;
            InitOrdersToPick(neutronVariables.StoreBatchSize);
            InitListView();
            _imagesDirectory = LoaderSettings.GetImagesDirectory();
        }

        private void SetupLogger()
        {
            string logFileDir = LoaderSettings.GetLogFileDirectory();
            string folderName = @"ReplenModule";
            string logActivity = LoaderSettings.EnableLogging;
            _logger = new DynamicLogger(logFileDir, folderName, logActivity);
        }

        private void SetupPrinters()
        {
            _documentPrinter = _jsonData.LoadFile<DocumentPrinterPreferences>();
            _labelPrinter = _jsonData.LoadFile<LabelPrinterPreferences>();
        }

        private void UpdateNomenclature()
        {
            MBStoreAccept.Text = _nomenclature.MBStoreAccept;
            LabelTray.Text = _nomenclature.LabelTray;
            LabelOver.Text = _nomenclature.LabelOver;
            LabelBack.Text = _nomenclature.LabelBack;
            LabelDevice.Text = _nomenclature.LabelDevice;
        }

        private void FrmReplen_Load(object sender, EventArgs e)
        {
            Task.Run(() => _logger.Log($"Not Loading ShowAllOrders on INIT FrmReplen"));
            if (GlobalVar.LoaderRunning)
            {
                MBMainLoadOrders.Text = "Stop Loader";
            }
        }

        // Set the focus to the passed in recId if it's passed in
        private int ShowAllOrders(int recId = 0)
        {
            Task.Run(() => _logger.Log($"ShowAllOrders Start: [{System.DateTime.Now.ToString()}]"));
            int idx = 0;
            string findWhat = TextBoxFind.Text.Trim().ToLower();
            // string find = _akaRepository.Get(findWhat);
            // TextBoxFind.Text = find;

            if (!string.IsNullOrEmpty(findWhat))
            {
                IEnumerable<ReplenOrderView> views = ordersRepository.GetOrderView(findWhat);
                bindingSourceOrderViewEquin = new BindingListView<ReplenOrderView>(views.ToList());
                //bindingSourceOrderView.DataSource = bindingSourceOrderViewEquin;
            }
            else
            {
                IEnumerable<ReplenOrderView> views = ordersRepository.GetOrderView();
                bindingSourceOrderViewEquin = new BindingListView<ReplenOrderView>(views.ToList());
                //bindingSourceOrderView.DataSource = bindingSourceOrderViewEquin;

            }
            //DataGridView1.DataSource = bindingSourceOrderView;
            DataGridView1.DataSource = bindingSourceOrderViewEquin;
            //if (GetRecordCount(bindingSourceOrderView) > 0)
            if (GetRecordCount(bindingSourceOrderViewEquin) > 0)
            {
                if (recId != 0)
                {
                    //idx = IndexOf(bindingSourceOrderView, recId);
                    idx = IndexOf(bindingSourceOrderViewEquin, recId);
                    DataGridView1.FirstDisplayedScrollingRowIndex = DataGridView1.Rows[idx].Index;
                }
                else
                {
                    DataGridView1.ClearSelection();
                }
                SyncCurrentlySelectedWithDataGrid();
                DataGridView1.Refresh();

                //CurrentItem = ((ObjectView<ReplenOrderView>) bindingSourceOrderView.Current).Object;
                CurrentItem = ((ObjectView<ReplenOrderView>)bindingSourceOrderViewEquin[recId]).Object;
                //PictureBoxItemImage.Load(@"C:\Images\1121.jpg");
            }
            Task.Run(() => _logger.Log($"ShowAllOrders End: [{System.DateTime.Now.ToLongTimeString()}]"));
            return idx;
        }

        private int ShowAvailableOrders(int recId = 0)
        {
            Task.Run(() => _logger.Log($"ShowAvailableOrders: [{System.DateTime.Now.ToLongTimeString()}]"));
            int idx = 0;

            string findWhat = TextBoxFindAvailableOrders.Text.Trim().ToLower();

            try
            {
                //if (!string.IsNullOrEmpty(search))
                //{
                //    //bindingSourceAvailableOrdersEquin.ApplyFilter(delegate (ReplenOrderView orderView) { return orderView.SearchField.Contains(search); });
                //    bindingSourceAvailableOrdersEquin.ApplyFilter(r => r.SearchField.Contains(search));
                //    //IEnumerable<ReplenOrderView> views = ordersRepository.GetAvailableOrders(search);
                //    //bindingSourceAvailableOrdersEquin = new BindingListView<ReplenOrderView>(views.ToList());
                //    //bindingSourceAvailableOrders.DataSource = bindingSourceAvailableOrdersEquin;
                //    //bindingSourceAvailableOrders .Filter = $"SearchField = '{search}'";
                //}
                //else
                //{

                IEnumerable<ReplenOrderView> views = ordersRepository.GetAvailableOrders(_station, findWhat);

                bindingSourceAvailableOrdersEquin = new BindingListView<ReplenOrderView>(views.ToList());

                bindingSourceAvailableOrders.DataSource = bindingSourceAvailableOrdersEquin;
                //}
            }
            catch (Exception ex)
            {

                Task.Run(() => _logger.Log($"ShowAvailableOrders Error: {ex.Message} \r\n {ex.InnerException} [{System.DateTime.Now.ToLongTimeString()}]"));
            }

            DataGridViewAvailableOrders.DataSource = bindingSourceAvailableOrders;
            DataGridViewAvailableOrdersRack.DataSource = bindingSourceAvailableOrders;
            if (GetRecordCount(bindingSourceAvailableOrders) > 0)
            {
                if (recId != 0)
                {
                    idx = IndexOf(bindingSourceAvailableOrders, recId);
                    DataGridViewAvailableOrders.FirstDisplayedScrollingRowIndex = DataGridViewAvailableOrders.Rows[idx].Index;
                    DataGridViewAvailableOrders.CurrentCell = DataGridViewAvailableOrders.Rows[idx].Cells[1];
                    DataGridViewAvailableOrders.Rows[idx].Selected = true;
                    DataGridViewAvailableOrdersRack.FirstDisplayedScrollingRowIndex = DataGridViewAvailableOrders.Rows[idx].Index;
                    DataGridViewAvailableOrdersRack.CurrentCell = DataGridViewAvailableOrders.Rows[idx].Cells[1];
                    DataGridViewAvailableOrdersRack.Rows[idx].Selected = true;
                }
                else
                {
                    DataGridViewAvailableOrders.ClearSelection();
                    DataGridViewAvailableOrdersRack.ClearSelection();
                }
                CheckMarkSelectedAvailableOrders();
                SetBatchPositionToFirstEmpty();
                DataGridViewAvailableOrders.Refresh();
                DataGridViewAvailableOrdersRack.Refresh();

                CurrentItem = ((ObjectView<ReplenOrderView>)bindingSourceAvailableOrders.Current).Object;

            }

            Task.Run(() => _logger.Log($"ShowAvailableOrders End: [{System.DateTime.Now.ToLongTimeString()}]"));
            return idx;
        }

        private void CheckMarkSelectedAvailableOrders()
        {
            foreach (var bp in _ordersToPick)
            {
                if (bp.OrderId == null) continue;
                foreach (DataGridViewRow row in DataGridViewAvailableOrders.Rows)
                {
                    var id = Convert.ToInt32(row.Cells["Id"].Value);
                    if (bp.OrderId != id) continue;
                    var chk = (DataGridViewCheckBoxCell)row.Cells[0];
                    chk.Value = chk.TrueValue;
                    break;
                }
            }
        }

        private int SetBatchPositionToFirstEmpty()
        {
            var result = -1;

            for (var i = 0; i < _ordersToPick.Count; i++)
            {
                var bp = _ordersToPick[i];
                if (bp.OrderId != null) continue;
                SetCurrentTextBoxPos(bp.PositionNumber);
                result = i;
                break;
            }
            return result;
        }

        public int IndexOf(BindingSource bs, int value)
        {
            int count = bs.Count;
            int itemIndex = -1;
            for (int i = 0; i < count; i++)
            {
                int rec = ((ReplenOrderView)bs[i]).Id;
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

        public int IndexOf(BindingListView<ReplenOrderView> bs, int value)
        {
            int count = bs.Count;
            int itemIndex = -1;
            for (int i = 0; i < count; i++)
            {
                int rec = ((ReplenOrderView)bs[i]).Id;
                if (rec == value)
                {
                    itemIndex = i;
                    break;
                }
            }
            return itemIndex;
        }

        private int GetRecordCount(BindingListView<ReplenOrderView> bs)
        {
            int count = bs.Count;
            LabelRecordCount.Text = string.Format("Records: {0}", count.ToString());
            return count;
        }


        private void SyncCurrentlySelectedWithDataGrid()
        {
            //if (ListBoxSelectedOrders.Items.Count > 0)
            //{
            //    foreach (ReplenOrder rec in ListBoxSelectedOrders.Items)
            //    {
            //        foreach (DataGridViewRow row in DataGridView1.Rows)
            //        {
            //            int id = Convert.ToInt32(row.Cells["Id"].Value);
            //            if (rec.Id == id)
            //            {
            //                var chk = (DataGridViewCheckBoxCell) row.Cells[0];
            //                chk.Value = chk.TrueValue;
            //            }
            //        }
            //    }
            //    DataGridView1.Refresh();
            //}
        }

        #region Button Clicks


        private void MButtonClose_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Jobs";
            LabelFormTitle.BackColor = Color.Green;
            tabControl1.SelectedTab = Main;
        }

        private void MButtonViewEdit_Click(object sender, EventArgs e)
        {
            this.tabControl1.SelectedTab = AvailableOrders;
        }

        //private void MButtonNew_Click(object sender, EventArgs e)
        //{
        //    this.tabControl1.SelectedTab = Replen     Screen;
        //}

        //private void MbNewListing_Click(object sender, EventArgs e)
        //{
        //    ShowAllOrders();
        //    tabControl1.SelectedTab = OrderListing;
        //}

        //private void MbNewViewEdit_Click(object sender, EventArgs e)
        //{
        //    tabControl1.SelectedTab = AvailableOrders;
        //}

        #endregion

        private void SetupListBoxes()
        {
            //ListBoxSelectedOrders.DisplayMember = "Ord1";
            //ListBoxSelectedOrders.ValueMember = "Id";
        }

        private void SetupGrids()
        {
            int w = 90;

            DataGridView1.AutoGenerateColumns = false;
            DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridView1.DefaultCellStyle.ForeColor = Color.Black;
            DataGridView1.DefaultCellStyle.BackColor = Color.White;


            var colx = new DataGridViewCheckBoxColumn();
            colx.HeaderText = "   ";
            colx.Width = w;
            colx.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colx.Name = "IsChecked";
            colx.TrueValue = true;
            colx.FalseValue = false;
            colx.Visible = true;
            DataGridView1.Columns.Add(colx);

            var col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Ord1";
            col.HeaderText = "Job";
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Width = w;
            col.Name = "Ord1";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Ord2";
            col.HeaderText = "Invoice";
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Width = w * 3;
            col.Name = "Ord2";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Priority";
            col.HeaderText = "Tray";
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.Width = w;
            col.Name = "Priority";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "OrderStatusName";
            col.HeaderText = "Job Status";
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            col.Name = "OrderStatusName";
            col.Visible = true;
            col.Width = w;
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Station_1_HasPicks";
            col.HeaderText = "1";
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.Name = "Station_1_HasPicks";
            col.Width = w;
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Station_2_HasPicks";
            col.HeaderText = "2";
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.Name = "Station_2_HasPicks";
            col.Width = w;
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Station_3_HasPicks";
            col.HeaderText = "3";
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.Name = "Station_3_HasPicks";
            col.Width = w;
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Station_4_HasPicks";
            col.HeaderText = "4";
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.Name = "Station_4_HasPicks";
            col.Width = w;
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Station_5_HasPicks";
            col.HeaderText = "5";
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.Name = "Station_5_HasPicks";
            col.Width = w;
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Station_8_HasPicks";
            col.HeaderText = "8";
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.Name = "Station_8_HasPicks";
            col.Width = w;
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Lines";
            col.HeaderText = "Lines";
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Lines";
            col.Width = w;
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Pieces";
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.HeaderText = "Pieces";
            col.Name = "Pieces";
            col.Width = w;
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "LoadDate";
            col.HeaderText = "LoadDate";
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.Name = "LoadDate";
            col.Width = w;
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "ShipMethodName";
            col.HeaderText = "Ship Method";
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            col.Width = w;
            col.Name = "ShipMethodName";
            col.Visible = false;
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Id";
            col.HeaderText = "Id";
            col.Visible = false;
            col.Name = "Id";
            DataGridView1.Columns.Add(col);

            //*****************************************************************************
            //DataGridPickView

            DataGridPickView.AutoGenerateColumns = false;
            DataGridPickView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridPickView.DefaultCellStyle.ForeColor = Color.Black;
            DataGridPickView.DefaultCellStyle.BackColor = Color.White;
            // DataGridPickView.Columns["ReceivedDate"].DefaultCellStyle.Format = "{0:dd.MM.yyyy}";

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Sequence";
            col.HeaderText = "Seq";
            col.Width = w;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Visible = false;
            col.Name = "Sequence";
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "PickPosition";
            col.HeaderText = "Pos";
            col.Width = w;
            colx.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.Name = "PickPosition";
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Ord1";
            col.HeaderText = "Job";
            col.Width = w;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Ord1";
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Ord2";
            col.HeaderText = "Invoice";
            col.Width = w * 3;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Ord2";
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Item";
            col.HeaderText = "Item";
            col.Width = w;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Item";
            DataGridPickView.Columns.Add(col);


            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Quantity";
            col.HeaderText = "Qty";
            col.Width = w;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Quantity";
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Slot";
            col.HeaderText = "Slot";
            col.Width = w;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.Name = "Slot";
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "TotalQuantityInInventory";
            col.HeaderText = "Inv Qty";
            col.Name = "TotalQuantityInInventory";
            col.Width = w;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Description";
            col.HeaderText = "Description";
            col.Width = w;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            col.Name = "Description";
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "ReceivedDate";
            col.HeaderText = "Received Date";
            col.Width = w; ;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            col.Name = "ReceivedDate";
            col.DefaultCellStyle.Format = "{0:dd.MM.yyyy}";
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "OrderId";
            col.HeaderText = "Job Id";
            col.Visible = false;
            col.Name = "OrderId";
            DataGridPickView.Columns.Add(col);



            //*****************************************************************************
            ///DataGridViewAvailableOrders

            DataGridViewAvailableOrders.AutoGenerateColumns = false;
            DataGridViewAvailableOrders.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewAvailableOrders.DefaultCellStyle.ForeColor = Color.Black;
            DataGridViewAvailableOrders.DefaultCellStyle.BackColor = Color.White;

            colx = new DataGridViewCheckBoxColumn();
            colx.HeaderText = "   ";
            colx.Width = w;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colx.Name = "IsChecked";
            colx.TrueValue = true;
            colx.FalseValue = false;
            colx.Visible = true;
            DataGridViewAvailableOrders.Columns.Add(colx);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Ord1";
            col.HeaderText = "Job";
            col.Width = w * 3;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Ord1";
            DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Ord2";
            col.HeaderText = "Invoice";
            col.Width = w * 2;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Ord2";
            DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Starter";
            col.HeaderText = "Starter";
            col.Width = w;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.Name = "Starter";
            col.Visible = true;
            DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Priority";
            col.HeaderText = "Tray";
            col.Width = w;
            colx.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.Name = "Priority";
            DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "OrderStatusName";
            col.HeaderText = "Job Status";
            col.Width = w;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            col.Name = "OrderStatusName";
            col.Visible = false;
            DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Lines";
            col.HeaderText = "Lines";
            col.Width = w;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Lines";
            DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Pieces";
            col.HeaderText = "Pieces";
            col.Width = w;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Pieces";
            DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "LoadDate";
            col.HeaderText = "LoadDate";
            col.Name = "LoadDate";
            col.Width = w * 2;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "ShipMethodName";
            col.HeaderText = "Ship Method";
            col.Width = w; ;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            col.Visible = false;
            col.Name = "ShipMethodName";
            DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Id";
            col.HeaderText = "Id";
            col.Visible = false;
            col.Name = "Id";
            DataGridViewAvailableOrders.Columns.Add(col);


            //*****************************************************************************
            ///DataGridViewAvailableOrdersRack

            DataGridViewAvailableOrdersRack.AutoGenerateColumns = false;
            DataGridViewAvailableOrdersRack.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewAvailableOrdersRack.DefaultCellStyle.ForeColor = Color.Black;
            DataGridViewAvailableOrdersRack.DefaultCellStyle.BackColor = Color.White;

            colx = new DataGridViewCheckBoxColumn();
            colx.HeaderText = "   ";
            colx.Width = w;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colx.Name = "IsChecked";
            colx.TrueValue = true;
            colx.FalseValue = false;
            colx.Visible = true;
            DataGridViewAvailableOrdersRack.Columns.Add(colx);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Ord1";
            col.HeaderText = "Job";
            col.Width = w * 3;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Ord1";
            DataGridViewAvailableOrdersRack.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Ord2";
            col.HeaderText = "Invoice";
            col.Width = w * 2;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Ord2";
            DataGridViewAvailableOrdersRack.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Starter";
            col.HeaderText = "Starter";
            col.Width = w;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.Name = "Starter";
            col.Visible = true;
            DataGridViewAvailableOrdersRack.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Priority";
            col.HeaderText = "Tray";
            col.Width = w;
            colx.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.Name = "Priority";
            DataGridViewAvailableOrdersRack.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "OrderStatusName";
            col.HeaderText = "Job Status";
            col.Width = w;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            col.Name = "OrderStatusName";
            col.Visible = false;
            DataGridViewAvailableOrdersRack.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Lines";
            col.HeaderText = "Lines";
            col.Width = w;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Lines";
            DataGridViewAvailableOrdersRack.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Pieces";
            col.HeaderText = "Pieces";
            col.Width = w;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Pieces";
            DataGridViewAvailableOrdersRack.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "LoadDate";
            col.HeaderText = "LoadDate";
            col.Name = "LoadDate";
            col.Width = w * 2;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            DataGridViewAvailableOrdersRack.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "ShipMethodName";
            col.HeaderText = "Ship Method";
            col.Width = w; ;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            col.Visible = false;
            col.Name = "ShipMethodName";
            DataGridViewAvailableOrdersRack.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Id";
            col.HeaderText = "Id";
            col.Visible = false;
            col.Name = "Id";
            DataGridViewAvailableOrdersRack.Columns.Add(col);

            
            //********************************************************************
            // DataGridViewNewOrder

            DataGridViewNewOrder.AutoGenerateColumns = false;
            DataGridViewNewOrder.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewNewOrder.DefaultCellStyle.ForeColor = Color.Black;
            DataGridViewNewOrder.DefaultCellStyle.BackColor = Color.White;

            var bCol = new DataGridViewButtonColumn();
            bCol.HeaderText = "   ";
            bCol.Visible = true;
            bCol.Name = "AddItem";
            bCol.Text = "Add Item";
            bCol.FlatStyle = FlatStyle.Popup;
            bCol.UseColumnTextForButtonValue = true;
            bCol.Width = w;
            DataGridViewNewOrder.Columns.Add(bCol);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Item";
            col.HeaderText = "Item";
            col.Width = w;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Item";
            DataGridViewNewOrder.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Quantity";
            col.HeaderText = "Quantity";
            col.Name = "Quantity";
            col.Width = w;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            DataGridViewNewOrder.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Description";
            col.HeaderText = "Description";
            col.Name = "Description";
            col.Width = w; ;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
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

            colx = new DataGridViewCheckBoxColumn();
            colx.HeaderText = "   ";
            colx.Width = w;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colx.Name = "IsChecked";
            colx.TrueValue = true;
            colx.FalseValue = false;
            DataGridViewOrderDetails.Columns.Add(colx);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "StationNumber";
            col.HeaderText = "Station";
            col.Width = w;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.Name = "StationNumber";
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Ord1";
            col.HeaderText = "Job";
            col.Width = w;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Ord1";
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Ord2";
            col.HeaderText = "Task";
            col.Width = w;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Ord2";
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Item";
            col.HeaderText = "Item";
            col.Width = w;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Item";
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Quantity";
            col.HeaderText = "Quantity";
            col.Name = "Quantity";
            col.Width = w;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "PickedQuantity";
            col.HeaderText = "Picked Quantity";
            col.Name = "PickedQuantity";
            col.Width = w;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Description";
            col.HeaderText = "Description";
            col.Width = w;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            col.Name = "Description";
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "LineStatusName";
            col.HeaderText = "Line Status";
            col.Width = w; ;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            col.Name = "LineStatusName";
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "OrderId";
            col.HeaderText = "OrderId";
            col.Visible = false;
            col.Name = "OrderId";
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "OrderDetailId";
            col.HeaderText = "OrderDetailId";
            col.Visible = false;
            col.Name = "OrderDetailId";
            DataGridViewOrderDetails.Columns.Add(col);

        }

        private void FrmReplen_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !CloseButtonPressed;
        }

        private void HideTabControlTabs()
        {
            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(0, 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;
        }



        private void MButtonClearSelection_Click(object sender, EventArgs e)
        {
            ClearSelection();
        }

        private void ClearSelection()
        {
            DataGridView1.ClearSelection();
            try
            {
                foreach (DataGridViewRow row in DataGridView1.Rows)
                {
                    var cell = (DataGridViewCheckBoxCell)row.Cells["IsChecked"];

                    if (cell.Value != null)
                    {
                        if (cell.Value.Equals(cell.TrueValue))
                        {
                            cell.Value = cell.FalseValue;
                            //RemoveItemFromListBox(Convert.ToInt32(row.Cells["Id"].Value));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Task.Run(() => _logger.Log($"ClearSelection Error: {ex.Message} \r\n {ex.InnerException} [{System.DateTime.Now.ToLongTimeString()}]"));
            }
        }

        private void MBSelectAll_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            SelectAll();
            Cursor.Current = Cursors.Default;

            //foreach (DataGridViewRow row in DataGridView1.Rows)
            //{
            //    var chk = (DataGridViewCheckBoxCell)row.Cells[0];
            //    chk.Value = chk.TrueValue;
            //    if (row.IsNewRow)
            //    {
            //        chk.Value = chk.FalseValue;
            //    }
            //}
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var dgv = sender as DataGridView;
            if (e.RowIndex >= 0)
            {
                //CurrentItem = ((ObjectView<ReplenOrderView>) bindingSourceOrderView.Current).Object;
                // ReplenOrderView orderView = ((ObjectView<ReplenOrderView>) bindingSourceOrderViewEquin[e.RowIndex]).Object;

                var chk = (DataGridViewCheckBoxCell)dgv.Rows[e.RowIndex].Cells[0];

                if (chk.Value == chk.TrueValue)
                {
                    dgv.Rows[e.RowIndex].Cells[0].Value = chk.FalseValue;
                    int id = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["Id"].Value);
                }
                else
                {
                    dgv.Rows[e.RowIndex].Cells[0].Value = chk.TrueValue;
                    int id = Convert.ToInt32(DataGridView1.Rows[e.RowIndex].Cells["Id"].Value);
                }
            }
        }

        //private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        //{
        //    //var watch = new Stopwatch();
        //    //watch.Start();

        //    //var dgv = sender as DataGridView;

        //    //if (dgv.Columns[e.ColumnIndex].Name.Equals("OrderStatusName"))
        //    //{
        //    //    if (e.Value != null)
        //    //    {
        //    //        switch (e.Value.ToString().Trim())
        //    //        {
        //    //            case "Available":
        //    //                dgv.Rows[e.RowIndex].Cells["IsChecked"].Style.BackColor = Color.White;
        //    //                break;
        //    //            case "Hold":
        //    //                dgv.Rows[e.RowIndex].Cells["IsChecked"].Style.BackColor = Color.Yellow;
        //    //                break;
        //    //            case "Picking":
        //    //                dgv.Rows[e.RowIndex].Cells["IsChecked"].Style.BackColor = Color.Blue;
        //    //                break;
        //    //            case "Partial":
        //    //                dgv.Rows[e.RowIndex].Cells["IsChecked"].Style.BackColor = Color.Orange;
        //    //                break;
        //    //            case "Deleted":
        //    //                dgv.Rows[e.RowIndex].Cells["IsChecked"].Style.BackColor = Color.Red;
        //    //                break;
        //    //            case "Complete":
        //    //                dgv.Rows[e.RowIndex].Cells["IsChecked"].Style.BackColor = Color.ForestGreen;
        //    //                break;
        //    //            default:
        //    //                dgv.Rows[e.RowIndex].Cells["IsChecked"].Style.BackColor = Color.White;
        //    //                break;
        //    //        }
        //    //    }
        //    //}
        //    //watch.Stop();
        //    //Task.Run(() => logger.Log($"DataGridView1_CellFormatting Elasped MSec:  {watch.ElapsedMilliseconds}ms"));
        //}

        private void MBHold_Click(object sender, EventArgs e)
        {
            List<int> recs = GetCheckedOrderIds();
            if (recs.Count() > 0)
            {
                foreach (int id in recs)
                {
                    ReplenOrder ord = repoReplenOrder.FindByKey(id);
                    if (ord.OrderStatusId == 1)
                    {
                        ord.OrderStatusId = 2;
                        repoReplenOrder.Update(ord);
                        GlobalVar.HistoryManager.SaveHistory(ActionCode.HoldOrder, ord);
                    }
                }
            }
            ShowAllOrders();
        }

        private void MBRelease_Click(object sender, EventArgs e)
        {
            List<int> recs = GetCheckedOrderIds();
            if (recs.Any())
            {
                foreach (int id in recs)
                {
                    ReplenOrder ord = repoReplenOrder.FindByKey(id);
                    if (ord.OrderStatusId == 2)
                    {
                        ord.OrderStatusId = 1;
                        repoReplenOrder.Update(ord);
                        GlobalVar.HistoryManager.SaveHistory(ActionCode.ReleaseOrder, ord);
                    }
                }
            }
            ShowAllOrders();
            //ListBoxSelectedOrders.Items.Clear();
        }

        private List<int> GetCheckedOrderIds()
        {
            //List<ReplenOrderView> ordView = DataGridView1.SelectedRows.ToList<ReplenOrderView>();
            var orderIds = new List<int>();

            foreach (DataGridViewRow row in DataGridView1.Rows)
            {
                if (row.Cells["IsChecked"].Value != null && (bool)row.Cells["IsChecked"].Value == true)
                {
                    orderIds.Add((int)row.Cells["Id"].Value);
                }
            }
            if (!orderIds.Any())
            {
                MessageBox.Show(text: "No Jobs Selected.");
            }
            return orderIds;
        }

        private List<ReplenOrderView> GetCheckedOrders()
        {
            var ordViews = new List<ReplenOrderView>();
            var orderIds = new List<int>();
            foreach (DataGridViewRow row in DataGridView1.Rows)
            {
                if (row.Cells["IsChecked"].Value != null && (bool)row.Cells["IsChecked"].Value == true)
                {
                    var ordId = (int)row.Cells["Id"].Value;
                    ReplenOrderView view = ordersRepository.GetOrderView().Where(r => r.Id == ordId).FirstOrDefault();
                    if (view != null)
                    {
                        ordViews.Add(view);
                    }
                }
            }
            if (ordViews.Count() == 0)
            {
                MessageBox.Show(text: "No Jobs Selected.");
            }
            return ordViews;
        }

        private List<int> GetCheckedAvailableOrderIds()
        {
            var orderIds = new List<int>();

            foreach (DataGridViewRow row in DataGridViewAvailableOrders.Rows)
            {
                if (row.Cells["IsChecked"].Value != null && (bool)row.Cells["IsChecked"].Value == true)
                {
                    orderIds.Add((int)row.Cells["Id"].Value);
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
            ClearBatchPositions();
            LabelFormTitle.Text = "Jobs";
            LabelFormTitle.BackColor = Color.Green;
            tabControl1.SelectedTab = Main;
        }

        private void MBGo_Click(object sender, EventArgs e)
        {
            if (GetCheckedAvailableOrderIds().Count > 0)
            {


                Task.Run(() => _logger.Log($"Batch Start: [{System.DateTime.Now.ToLongTimeString()}]"));

                LabelFormTitle.Text = "Pick List";
                LabelFormTitle.BackColor = Color.Green;
                this.PickListLoad();
                DataGridPickView.ClearSelection();

                Task.Run(() => _logger.Log($"Call Printing Start: [{System.DateTime.Now.ToLongTimeString()}]"));
                //PrintAllToteLabels();

                //PrintAllDocuments();
                Task.Run(() => _logger.Log($"Call Printing End: [{System.DateTime.Now.ToLongTimeString()}]"));

                tabControl1.SelectedTab = PickList;
                Task.Run(() => _logger.Log($"Batch Start End: [{System.DateTime.Now.ToLongTimeString()}]"));

                Start();
            }
            else
            {
                MessageBox.Show("Nothing Selected.");
            }
        }

        private void PickListLoad()
        {
            Task.Run(() => _logger.Log($"PickListLoad Start: [{System.DateTime.Now.ToLongTimeString()}]"));
            TextBoxFindAvailableOrders.Text = string.Empty;
            ShowAvailableOrders();
            //int[] orderIds = GetOrderIdArray(ordersToPick);
            // pickviews from bindingSourceAvailableOrders?
            List<ReplenPickView> pickViews = GetPickViews();
            //List<ReplenPickView> pickViews = ordersRepository.GetOrderLines(ordersToPick);
            //Add Pick Location based on Inventory
            Task.Run(() => _logger.Log($"Pickviews Count: [{System.DateTime.Now.ToLongTimeString()}] {pickViews.Count.ToString()}"));

            var pickableViews = new List<ReplenPickView>();

            foreach (var item in pickViews)
            {
                List<Inventory> recs = GetInventory(item.ItemId);
                Task.Run(() => _logger.Log($"Inventory Recs [{System.DateTime.Now.ToLongTimeString()}] {recs.Count.ToString()}"));
                if (recs.Count > 0)
                {
                    var exactInventorySequence = new List<Inventory>();
                    if (_neutronVariables.UsePrimeBin)
                    {
                        //if there is a prime bin make it first, remove it from the list of inventory locations
                        Inventory prime = recs.Where(r => r.Location.Slot == item.OrderDetail.PrimeBin).FirstOrDefault();
                        if (prime != null)
                        {
                            Task.Run(() => _logger.Log($"recs Add Prime [{System.DateTime.Now.ToLongTimeString()}] "));
                            exactInventorySequence.Add(prime);
                            recs.Remove(prime);
                        }
                    }
                    //sequence the inventory Recs by Received Date Descending 
                    recs.OrderBy(o => o.ReceivedDate).ToList();
                    foreach (var inv in recs)
                    {
                        exactInventorySequence.Add(inv);
                    }

                    item.CurrentInventoryLocation = exactInventorySequence.First();
                    item.Inventory = exactInventorySequence;
                    item.TotalQuantityInInventory = exactInventorySequence.Sum(r => r.Quantity);
                    item.Slot = item.CurrentInventoryLocation.Location.Slot;
                    item.SlotQty = item.TotalQuantityInInventory;
                    item.InventoryIndex = 0;
                    item.ReceivedDate = item.CurrentInventoryLocation.ReceivedDate;

                    pickableViews.Add(item);
                }
                else
                {
                    MessageBox.Show("No Inventory Locations Set Up for Item: " + item.Item);
                }
            }

            if (pickableViews.Count > 0)
            {
                bindingSourcePickViews.DataSource = pickableViews;
                DataGridPickView.DataSource = bindingSourcePickViews;
                Task.Run(() => _logger.Log($"bindingSourcePickViews [{System.DateTime.Now.ToLongTimeString()}] \r\nCount:{bindingSourcePickViews.Count.ToString()}"));
                GetRecordCount(bindingSourcePickViews);
                DataGridPickView.ClearSelection();
                DataGridPickView.ScrollBars = ScrollBars.Both;
            }
            else
            {
                MessageBox.Show("Nothing to Pick.");
                ClearAllSelectOrdersToPick();
                LabelFormTitle.Text = "Available Jobs";
                LabelFormTitle.BackColor = Color.Green;
                tabControl1.SelectedTab = AvailableOrders;
            }
            Task.Run(() => _logger.Log($"PickListLoad End: [{System.DateTime.Now.ToLongTimeString()}]"));
        }

        private List<ReplenPickView> GetPickViews()
        {
            Task.Run(() => _logger.Log($"GetPickViews Start: [{System.DateTime.Now.ToLongTimeString()}]"));
            var pickViews = new List<ReplenPickView>();
            foreach (var bp in _ordersToPick)
            {
                if (bp.OrderId == null) continue;
                var itemFound = bindingSourceAvailableOrders.Find("Id", bp.OrderId);
                bindingSourceAvailableOrders.Position = itemFound;
                var currentItem = ((ObjectView<ReplenOrderView>)bindingSourceAvailableOrders.Current).Object;
                foreach (var detail in currentItem.ReplenOrder.ReplenOrderDetails)
                {
                    var pickView = new ReplenPickView()
                    {
                        PickPosition = bp.PositionNumber,
                        ReplenOrderId = detail.ReplenOrder.Id,
                        Ord1 = detail.ReplenOrder.Ord1,
                        Ord2 = detail.ReplenOrder.Ord2,
                        ItemId = detail.ItemDefinitionId,
                        Item = string.Empty,
                        Description = string.Empty,
                        UnitOfIssue = detail.ItemDefinition.UnitOfIssue.Name,
                        Quantity = detail.Quantity,
                        QuantityToBePicked = detail.Quantity,
                        PickedQty = 0,
                        Slot = string.Empty,
                        SlotQty = 0,
                        OrderDetail = detail,
                        StationNumber = detail.StationNumber
                    };
                    pickViews.Add(pickView);
                }
            }

            //Add Item definition
            foreach (var item in pickViews)
            {
                var def = repoItemDefinition.FindBy(f => f.Id == item.ItemId).FirstOrDefault();
                if (def == null) continue;
                item.Item = def.Item;
                item.Description = def.Description;
            }
            Task.Run(() => _logger.Log($"GetPickViews End: [{System.DateTime.Now.ToLongTimeString()}]"));
            return pickViews;
        }

        private int GetBatchPosition(int orderId)
        {
            var result = -1;
            foreach (var bp in _ordersToPick)
            {
                if (bp.OrderId != orderId) continue;
                result = bp.PositionNumber;
                break;
            }
            return result;
        }

        private int[] GetOrderIdArray(List<BatchPosition> ordersToPick)
        {
            var orderIds = new List<int>();
            foreach (BatchPosition bp in ordersToPick)
            {
                if (bp.OrderId != null)
                {
                    orderIds.Add(Convert.ToInt32(bp.OrderId));
                }
            }
            return orderIds.ToArray();
        }

        private ReplenPickView CreatePickView(int pos, ReplenOrderDetail detail)
        {
            Task.Run(() => _logger.Log($"CreatePickView Start: [{System.DateTime.Now.ToLongTimeString()}]"));
            var pickView = new ReplenPickView()
            {
                PickPosition = pos,
                ReplenOrderId = detail.ReplenOrder.Id,
                Ord1 = detail.ReplenOrder.Ord1,
                Ord2 = detail.ReplenOrder.Ord2,
                ItemId = detail.ItemDefinitionId,
                Item = string.Empty,
                Description = string.Empty,
                Quantity = detail.Quantity,
                QuantityToBePicked = detail.Quantity,
                PickedQty = 0,
                Slot = string.Empty,
                SlotQty = 0,
                OrderDetail = detail
            };

            ItemDefinition def = repoItemDefinition.FindBy(f => f.Id == pickView.ItemId).FirstOrDefault();
            if (def != null)
            {
                pickView.Item = def.Item;
                pickView.Description = def.Description;
            }

            List<Inventory> recs = GetInventory(pickView.ItemId);

            if (recs.Count > 0)
            {
                var exactInventorySequence = new List<Inventory>();
                if (_neutronVariables.UsePrimeBin)
                {
                    //if there is a prime bin make it first, remove it from the list of inventory locations
                    Inventory prime = recs.Where(r => r.Location.Slot == pickView.OrderDetail.PrimeBin).FirstOrDefault();
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

                pickView.CurrentInventoryLocation = exactInventorySequence.First();
                pickView.Inventory = exactInventorySequence;
                pickView.TotalQuantityInInventory = exactInventorySequence.Sum(r => r.Quantity);
                pickView.Slot = pickView.CurrentInventoryLocation.Location.Slot;
                pickView.SlotQty = pickView.CurrentInventoryLocation.Quantity;
                pickView.InventoryIndex = 0;
                pickView.ReceivedDate = pickView.CurrentInventoryLocation.ReceivedDate;
            }
            else
            {
                MessageBox.Show("No Inventory for Item: " + pickView.Item);
            }
            Task.Run(() => _logger.Log($"CreatePickView End: [{System.DateTime.Now.ToLongTimeString()}]"));
            return pickView;
        }

        private List<Inventory> GetInventory(int itemId)
        {
            Task.Run(() => _logger.Log($"GetInventory Start: [{System.DateTime.Now.ToLongTimeString()}]"));
            var pickableLocations = new int[] { 1, 2 };
            var recs = new List<Inventory>();

            recs = repoInventory.AllInclude(l => l.Location, l => l.ItemDefinition)
                .Where(f => f.ItemDefinitionId == itemId && pickableLocations.Contains(f.StorageTypeId)).ToList();
            Task.Run(() => _logger.Log($"GetInventory End: [{System.DateTime.Now.ToLongTimeString()}]"));
            return recs;
        }

        //private void FrmReplen_FormClosing(object sender, FormClosingEventArgs e)
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
            gridClickedAvailableOrders = true;
            if (e.RowIndex >= 0)
            {
                var chk = (DataGridViewCheckBoxCell)DataGridViewAvailableOrders.Rows[e.RowIndex].Cells[0];

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
                        string ord2 = DataGridViewAvailableOrders.Rows[e.RowIndex].Cells["Ord2"].Value.ToString();
                        AddItemToBatch(id, ord1, ord2);
                    }
                }
            }
        }

        //select Available Rack Orders
        private void DataGridViewAvailableOrdersRack_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var chk = (DataGridViewCheckBoxCell)DataGridViewAvailableOrdersRack.Rows[e.RowIndex].Cells[0];

                if (chk.Value == chk.TrueValue)
                {
                    DataGridViewAvailableOrdersRack.Rows[e.RowIndex].Cells[0].Value = chk.FalseValue;
                    //int id = Convert.ToInt32(DataGridViewAvailableOrdersRack.Rows[e.RowIndex].Cells["Id"].Value);
                }
                else
                {
                    DataGridViewAvailableOrdersRack.Rows[e.RowIndex].Cells[0].Value = chk.TrueValue;
                    //int id = Convert.ToInt32(DataGridViewAvailableOrdersRack.Rows[e.RowIndex].Cells["Id"].Value);
                    //if (id > 0)
                    //{
                    //    string ord1 = DataGridViewAvailableOrdersRack.Rows[e.RowIndex].Cells["Ord1"].Value.ToString();
                    //}
                }
            }
        }

        private void RemoveItemFromBatch(int orderId)
        {
            foreach (var bp in _ordersToPick)
            {
                if (bp.OrderId != orderId) continue;
                bp.OrderId = null;
                bp.Ord1 = string.Empty;
                bp.Ord2 = string.Empty;
                UpdateTextBoxPosition(bp);
            }
        }

        private int AddItemToBatch(int orderId, string ord1, string ord2)
        {
            var idx = ManualOverrideCurrentTextBoxPos ? SetBatchPositionToManualOverride() : SetBatchPositionToFirstEmpty();

            if (idx >= 0 && idx <= 7)
            {
                _ordersToPick[idx].OrderId = orderId;
                _ordersToPick[idx].Ord1 = ord1;
                _ordersToPick[idx].Ord2 = ord2;
                CurrentTextBoxPos.Text = ord1;
            }
            ManualOverrideCurrentTextBoxPos = false;
            ClearTextBoxPosBackColor();
            return idx;
        }

        private int SetBatchPositionToManualOverride()
        {
            int result = -1;
            result = int.Parse(CurrentTextBoxPos.Tag.ToString());

            return result;
        }

        private void ClearAllSelectOrdersToPick()
        {
            foreach (DataGridViewRow row in DataGridViewAvailableOrders.Rows)
            {
                bool checkedBox = Convert.ToBoolean(row.Cells[0].Value);

                if (checkedBox)
                {
                    int orderId = Convert.ToInt32(row.Cells["Id"].Value);
                    row.Cells[0].Value = false;
                    RemoveItemFromBatch(orderId);
                }
            }
        }

        private void ClearBatchPositions()
        {
            foreach (var bp in _ordersToPick)
            {
                bp.OrderId = null;
                bp.Ord1 = string.Empty;
                bp.Ord2 = string.Empty;
                UpdateTextBoxPosition(bp);

            }
        }

        private void SetCurrentTextBoxPos(int batchPositionNumber)
        {
            switch (batchPositionNumber)
            {
                case 1:
                    CurrentTextBoxPos = TextBoxPos1;
                    break;
                case 2:
                    CurrentTextBoxPos = TextBoxPos2;
                    break;
                case 3:
                    CurrentTextBoxPos = TextBoxPos3;
                    break;
                case 4:
                    CurrentTextBoxPos = TextBoxPos4;
                    break;
                case 5:
                    CurrentTextBoxPos = TextBoxPos5;
                    break;
                case 6:
                    CurrentTextBoxPos = TextBoxPos6;
                    break;
                case 7:
                    CurrentTextBoxPos = TextBoxPos7;
                    break;
                case 8:
                    CurrentTextBoxPos = TextBoxPos8;
                    break;
                default:
                    CurrentTextBoxPos = TextBoxPos1;
                    break;
            }
            CurrentTextBoxPos.BackColor = Color.Yellow;
        }

        private void UpdateTextBoxPosition(BatchPosition bp)
        {
            Task.Run(() => _logger.Log($"UpdateTextBoxPosition Start: [{System.DateTime.Now.ToLongTimeString()}]"));
            string orderNumber = bp.Ord1;
            int pos = bp.PositionNumber;

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
                    CurrentTextBoxPos = TextBoxPos1;
                    break;
                default:
                    break;
            }
            Task.Run(() => _logger.Log($"UpdateTextBoxPosition End: [{System.DateTime.Now.ToLongTimeString()}]"));
        }


        private void InitOrdersToPick(int batchSize)
        {
            _ordersToPick = new List<BatchPosition>();
            for (var i = 0; i < batchSize; i++)
            {
                var bp = new BatchPosition() { PositionNumber = i + 1, OrderId = null, Ord1 = string.Empty, Ord2 = string.Empty };
                _ordersToPick.Add(bp);
                ShowPosition(i + 1);
            }
        }

        private void ShowPosition(int position)
        {
            var font = new Font("Microsoft Sans Serif", 24);
            var pos = position.ToString();

            Control c = this.Controls.Find("LabelPickPos" + pos, true).Single() as Label;
            if (c != null) c.Visible = true;

            c = this.Controls.Find("LabelPos" + pos, true).Single() as Label;
            if (c != null) c.Visible = true;

            c = this.Controls.Find("TextBoxPickPos" + pos, true).Single() as TextBox;
            if (c != null)
            {
                c.Visible = true;
                c.Font = font;
            }

            c = this.Controls.Find("TextBoxPos" + pos, true).Single() as TextBox;
            if (c != null) c.Visible = true;

            c = this.Controls.Find("Pos" + pos + "Display", true).Single();
            ((Panel)c).Visible = true;

            c = this.Controls.Find("AvailablePos" + pos + "Display", true).Single();
            ((Panel)c).Visible = true;
        }

        private void ShowOrdersToPick()
        {
            Task.Run(() => _logger.Log($"ShowOrdersToPick Start: [{System.DateTime.Now.ToLongTimeString()}]"));
            var font = new Font("Microsoft Sans Serif", 10);
            for (var i = 0; i < _ordersToPick.Count; i++)
            {
                var pos = (i + 1).ToString();
                Control c = this.Controls.Find("TextBoxPickPos" + pos, true).Single() as TextBox;
                if (c == null) continue;
                c.Font = font;
                c.Text = _ordersToPick[i].Ord1 + Environment.NewLine + _ordersToPick[i].Ord2;
            }
            if (_neutronVariables.DisplaysEnabled)
            {
                if (GlobalVar.Displays != null)
                {
                    if (_neutronVariables.BliEnabled)
                    {
                        Task.Run(() => _logger.Log($"Show Orders To Pick On Displays Clear ALL Bli"));
                        //GlobalVar.Displays.ClearAllBli();
                        foreach (var item in _ordersToPick)
                        {
                            if (string.IsNullOrEmpty(item.Ord1)) continue;
                            Task.Run(() => _logger.Log($"Show Each Order To Pick {item.Ord1}"));
                            GlobalVar.Displays.ShowBli(item.PositionNumber, beacon: 2, text: item.Ord1);
                        }
                    }
                }
            }
            Task.Run(() => _logger.Log($"ShowOrdersToPick End: [{System.DateTime.Now.ToLongTimeString()}]"));
        }
        private void MBShowOrderOrQuantityToggle_Click(object sender, EventArgs e)
        {
            if (MBShowOrderOrQuantityToggle.Text == "Show Jobs")
            {
                ShowOrdersToPick();
                MBShowOrderOrQuantityToggle.Text = "Show Qty";
            }
            else
            {
                if (_neutronVariables.DisplaysEnabled)
                {
                    if (GlobalVar.Displays != null)
                    {
                        if (_neutronVariables.BliEnabled)
                        {
                            Task.Run(() => _logger.Log($"MBShowOrderOrQuantityToggle ClearAllBli"));
                            GlobalVar.Displays.ClearAllBli();
                        }
                    }
                }

                ClearPickPositions();
                ClearPickDisplays();
                MBShowOrderOrQuantityToggle.Text = "Show Jobs";
                UpdatePickScreen();
            }
        }

        private void MBPickBack_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Pick List";
            LabelFormTitle.BackColor = Color.Green;
            tabControl1.SelectedTab = PickList;
        }

        private void MBStart_Click(object sender, EventArgs e)
        {
            Start();
        }
        private void Start()
        {
            Task.Run(() => _logger.Log($"Start_Click Start: [{System.DateTime.Now.ToLongTimeString()}]"));
            var pickViews = (IList<ReplenPickView>)bindingSourcePickViews.DataSource;
            pickViews.OrderBy(p => p.CurrentInventoryLocation.Location.Loc1)
                .ThenBy(p => p.CurrentInventoryLocation.Location.Loc2)
                .ThenBy(p => p.CurrentInventoryLocation.Location.Loc3)
                .ThenBy(p => p.CurrentInventoryLocation.Location.Loc4).ToList();
            //TODO SetOrderStatusToPicking(pickViews);
            Task.Run(() => _logger.Log($"Start_Click 1: [{System.DateTime.Now.ToLongTimeString()}]"));
            var pickStops = new List<ReplenPickStop>();
            IEnumerable<IGrouping<string, ReplenPickView>> pickViewGroups = pickViews.GroupBy(r => r.Item).ToList();
            int sequence = 0;
            foreach (var pickViewGroup in pickViewGroups)  //for each Item in the group of Items
            {
                int total = 0;
                // a ReplenPickStop is of One Item that may be on One to All Pick Positions
                // a ReplenPickView is an individual pick at a single Pick Position
                // so a ReplenPickStop is has One or Multiple PickViews that are concerned 
                // with picking One Item.
                // a ReplenPickStop is a summary of all the PickViews 
                // and some of the information in a ReplenPickStop is the same as in a ReplenPickView
                // that is why the First ReplenPickView is used to provide most of the data to the ReplenPickStop
                ReplenPickView firstPickView = pickViewGroup.First();

                var pickStop = new ReplenPickStop();

                pickStop.Sequence = sequence += 1;
                pickStop.OrderId = firstPickView.ReplenOrderId;
                pickStop.Ord1 = firstPickView.Ord1;
                pickStop.Ord2 = firstPickView.Ord2;
                pickStop.ItemId = firstPickView.ItemId;
                pickStop.Item = firstPickView.Item;
                pickStop.Description = firstPickView.Description;
                pickStop.UnitOfIssue = firstPickView.UnitOfIssue;
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
            Task.Run(() => _logger.Log($"Start_Click 2: [{System.DateTime.Now.ToLongTimeString()}]"));
            List<ReplenPickStop> finalPickSequence = FinalPickSequence(pickStops);
            bindingSourcePickStops.DataSource = finalPickSequence;
            Task.Run(() => _logger.Log($"Start_Click 3 Run GetFirstStop?: [{System.DateTime.Now.ToLongTimeString()}]"));
            // GetFirstStop();
            bindingSourcePickStops.MoveFirst();
            currentPickStop = (ReplenPickStop)bindingSourcePickStops.Current;
            UpdatePickScreen();
            Task.Run(() => _logger.Log($"Start_Click 4  Run GetFirstStop?: [{System.DateTime.Now.ToLongTimeString()}]"));
            // PrintAllDocuments();
            // PrintAllToteLabels();

            tabControl1.SelectedTab = PickScreen;
            //feels good to here
            Task.Run(() => _logger.Log($"Start_Click End: [{System.DateTime.Now.ToLongTimeString()}]"));
        }

        private List<ReplenPickStop> FinalPickSequence(List<ReplenPickStop> pickStops)
        {
            Task.Run(() => _logger.Log($"FinalPickSequence Start: [{System.DateTime.Now.ToLongTimeString()}]"));
            var newList = new List<ReplenPickStop>();
            List<ReplenPickStop> car1List = pickStops.Where(p => p.CurrentInventoryLocation.Location.Loc1 == 1)
                .OrderBy(p => p.CurrentInventoryLocation.Location.Loc2)
                .ThenBy(p => p.CurrentInventoryLocation.Location.Loc3)
                .ThenBy(p => p.CurrentInventoryLocation.Location.Loc4)
                .ToList();
            List<ReplenPickStop> car2List = pickStops.Where(p => p.CurrentInventoryLocation.Location.Loc1 == 2)
               .OrderBy(p => p.CurrentInventoryLocation.Location.Loc2)
               .ThenBy(p => p.CurrentInventoryLocation.Location.Loc3)
               .ThenBy(p => p.CurrentInventoryLocation.Location.Loc4)
               .ToList();
            List<ReplenPickStop> car3List = pickStops.Where(p => p.CurrentInventoryLocation.Location.Loc1 == 3)
               .OrderBy(p => p.CurrentInventoryLocation.Location.Loc2)
               .ThenBy(p => p.CurrentInventoryLocation.Location.Loc3)
               .ThenBy(p => p.CurrentInventoryLocation.Location.Loc4)
               .ToList();
            List<ReplenPickStop> car4List = pickStops.Where(p => p.CurrentInventoryLocation.Location.Loc1 == 4)
               .OrderBy(p => p.CurrentInventoryLocation.Location.Loc2)
               .ThenBy(p => p.CurrentInventoryLocation.Location.Loc3)
               .ThenBy(p => p.CurrentInventoryLocation.Location.Loc4)
               .ToList();
            int seq = 1;
            for (int i = 0; i < 100; i++)
            {
                int done = 0;
                if (car1List.Count >= i + 1)
                {
                    car1List[i].Sequence = seq;
                    seq += 1;
                    newList.Add(car1List[i]);
                }
                else
                {
                    done = 1;
                }
                if (car2List.Count >= i + 1)
                {
                    car2List[i].Sequence = seq;
                    seq += 1;
                    newList.Add(car2List[i]);
                }
                else
                {
                    done += 1;
                }
                if (car3List.Count >= i + 1)
                {
                    car3List[i].Sequence = seq;
                    seq += 1;
                    newList.Add(car3List[i]);
                }
                else
                {
                    done += 1;
                }
                if (car4List.Count >= i + 1)
                {
                    car4List[i].Sequence = seq;
                    seq += 1;
                    newList.Add(car4List[i]);
                }
                else
                {
                    done += 1;
                }
                if (done == 4)
                {
                    break;
                }

            }
            Task.Run(() => _logger.Log($"FinalPickSequence Start Carousel Move: [{System.DateTime.Now.ToLongTimeString()}]"));

            _deviceManager = new ReplenDeviceManager(car1List, car2List, car3List
                , car4List, _neutronVariables.ShuttleEnabled);

            Task.Run(() => _deviceManager.MoveNext(1));
            Task.Run(() => _deviceManager.MoveNext(2));
            Task.Run(() => _deviceManager.MoveNext(3));
            Task.Run(() => _deviceManager.MoveNext(4));

            Task.Run(() => _logger.Log($"FinalPickSequence End Carousel Move: [{System.DateTime.Now.ToLongTimeString()}]"));
            //var sb = new StringBuilder();
            //foreach (var item in newList)
            //{
            //    sb.AppendLine(item.CurrentInventoryLocation.Location.Slot + "  Sequence: " + item.Sequence);
            //}
            //MessageBox.Show(sb.ToString());
            Task.Run(() => _logger.Log($"FinalPickSequence End: [{System.DateTime.Now.ToLongTimeString()}]"));
            return newList;
        }

        private void SetOrderStatusToPartial(IList<ReplenPickView> recs)
        {
            List<int> ids = recs.Select(r => r.ReplenOrderId).Distinct().ToList();
            foreach (var item in ids)
            {
                try
                {
                    ReplenOrder ord = repoReplenOrder.FindByKey(item);
                    ord.OrderStatusId = 4;
                    repoReplenOrder.Update(ord);
                    GlobalVar.HistoryManager.SaveHistory(ActionCode.PartialOrder, ord);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error Saving ReplenOrder Status to Partial. " + ex.Message);
                }
            }
        }

        private void PositionDevice(int loc1, int loc2, int loc3, int loc4, bool moveDevice)
        {
            if (_neutronVariables.ShuttleEnabled)
            {
                if (GlobalVar.Shuttle != null)
                {
                    if (moveDevice)
                    {
                        Task<DeviceResponse> response = Task.Run(() => GlobalVar.Shuttle.PositionDevice(loc1, loc2, loc3, loc4));
                        if (response.Result != DeviceResponse.Success)
                        {
                            MessageBox.Show(response.Result.AsString(EnumFormat.Description), caption: "Device Information"
                                , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void GetFirstStop(bool moveDevice = true)
        {
            Task.Run(() => _logger.Log($"GetFirstStop: [{System.DateTime.Now.ToLongTimeString()}]"));
            int numberOfStops = bindingSourcePickStops.Count;
            if (numberOfStops > 0)
            {
                bindingSourcePickStops.MoveFirst();
                currentPickStop = (ReplenPickStop)bindingSourcePickStops.Current;
                UpdatePickScreen();

                int loc1 = currentPickStop.CurrentInventoryLocation.Location.Loc1;
                int loc2 = currentPickStop.CurrentInventoryLocation.Location.Loc2;
                int loc3 = currentPickStop.CurrentInventoryLocation.Location.Loc3;
                int loc4 = currentPickStop.CurrentInventoryLocation.Location.Loc4;
                PositionDevice(loc1, loc2, loc3, loc4, moveDevice);
            }
        }

        private void GetNextStop(bool moveDevice = true)
        {
            Task.Run(() => _logger.Log($"GetNextStop: [{System.DateTime.Now.ToLongTimeString()}]"));
            int numberOfStops = bindingSourcePickStops.Count;
            if (currentPickStop.Sequence < numberOfStops)
            {
                bindingSourcePickStops.MoveNext();
                currentPickStop = (ReplenPickStop)bindingSourcePickStops.Current;
                UpdatePickScreen();

                int loc1 = currentPickStop.CurrentInventoryLocation.Location.Loc1;
                int loc2 = currentPickStop.CurrentInventoryLocation.Location.Loc2;
                int loc3 = currentPickStop.CurrentInventoryLocation.Location.Loc3;
                int loc4 = currentPickStop.CurrentInventoryLocation.Location.Loc4;
                PositionDevice(loc1, loc2, loc3, loc4, moveDevice);
            }
            Task.Run(() => _logger.Log($"GetNextStop Return: [{System.DateTime.Now.ToLongTimeString()}]"));
        }

        private void GetPreviousStop(bool moveDevice = true)
        {
            int numberOfStops = bindingSourcePickStops.Count;
            if (currentPickStop.Sequence > 0)
            {
                bindingSourcePickStops.MovePrevious();
                currentPickStop = (ReplenPickStop)bindingSourcePickStops.Current;
                UpdatePickScreen();

                int loc1 = currentPickStop.CurrentInventoryLocation.Location.Loc1;
                int loc2 = currentPickStop.CurrentInventoryLocation.Location.Loc2;
                int loc3 = currentPickStop.CurrentInventoryLocation.Location.Loc3;
                int loc4 = currentPickStop.CurrentInventoryLocation.Location.Loc4;
                PositionDevice(loc1, loc2, loc3, loc4, moveDevice);
            }
        }

        private void GetLastStop(bool moveDevice = true)
        {
            int numberOfStops = bindingSourcePickStops.Count;
            if (numberOfStops > 0)
            {
                bindingSourcePickStops.MoveLast();
                currentPickStop = (ReplenPickStop)bindingSourcePickStops.Current;
                UpdatePickScreen();
                int loc1 = currentPickStop.CurrentInventoryLocation.Location.Loc1;
                int loc2 = currentPickStop.CurrentInventoryLocation.Location.Loc2;
                int loc3 = currentPickStop.CurrentInventoryLocation.Location.Loc3;
                int loc4 = currentPickStop.CurrentInventoryLocation.Location.Loc4;
                PositionDevice(loc1, loc2, loc3, loc4, moveDevice);
            }
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


        private void UpdatePickScreen()
        {
           Task.Run(() => _logger.Log($"UpdatePickScreen Start: [{System.DateTime.Now.ToLongTimeString()}]"));
            UpdatePickPosition();
            UpdateInventoryLocation();
            UpdateImages();

            LabelPickDescription.Text = currentPickStop.Description;
            LabelPickItemNumber.Text = currentPickStop.Item;
            LabelLineOfLines.Text = string.Format("{0} of {1}"
                , (currentPickStop.Sequence).ToString(), bindingSourcePickStops.Count);
            TextBoxRequestedQty.Text = currentPickStop.Quantity.ToString();
            LabelPickUOI.Text = currentPickStop.UnitOfIssue;
            var pickedSoFar = GetPickedSoFar(currentPickStop.PickViews);
            TextBoxPickedSoFar.Text = pickedSoFar.ToString();

            LabelPickQty.Text = (currentPickStop.QuantityToBePicked).ToString();
            Task.Run(() => _logger.Log($"UpdatePickScreen End: [{System.DateTime.Now.ToLongTimeString()}]"));
        }

        private void UpdateImages()
        {

            Task.Run(() => _logger.Log($"UpdateImages Start : [{System.DateTime.Now.ToLongTimeString()}]"));
            if (!string.IsNullOrEmpty(_imagesDirectory))
            {
                try
                {
                    string path = string.Concat(_imagesDirectory, currentPickStop.Item, str2: @".jpg");
                    if (File.Exists(path))
                    {
                        PictureBoxItemImage.Load(path);
                    }
                    else
                    {
                        path = string.Concat(_imagesDirectory, str1: @"Unknown.jpg");
                        if (File.Exists(path))
                        {
                            PictureBoxItemImage.Load(path);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error getting Image.  {ex.Message} \r\n {ex.InnerException}");
                }
            }
            Task.Run(() => _logger.Log($"UpdateImages End : [{System.DateTime.Now.ToLongTimeString()}]"));
        }

       private void UpdateInventoryLocation()
        {
            Task.Run(() => _logger.Log($"UpdateInventoryLocation Start : [{System.DateTime.Now.ToLongTimeString()}]"));

            string loc1 = TextBoxPickLoc1.Text = currentPickStop.CurrentInventoryLocation.Location.Loc1.ToString();
            string loc2 = TextBoxPickLoc2.Text = currentPickStop.CurrentInventoryLocation.Location.Loc2.ToString();
            string loc3 = TextBoxPickLoc3.Text = currentPickStop.CurrentInventoryLocation.Location.Loc3.ToString();
            string loc4 = TextBoxPickLoc4.Text = currentPickStop.CurrentInventoryLocation.Location.Loc4.ToString();
            string loc5 = TextBoxPickLoc5.Text = currentPickStop.CurrentInventoryLocation.Location.Loc5.ToString();

            ShowShi(loc1.ParseInt(), loc2.ParseInt(), loc3.ParseInt(), loc4, currentPickStop.QuantityToBePicked.ToString());

            LabelLocationNumber.Text = string.Format(format: "{0} of {1}"
                , arg0: currentPickStop.InventoryIndex + 1, arg1: currentPickStop.Inventory.Count);
            TextBoxLocationQuantity.Text = currentPickStop.CurrentInventoryLocation.Quantity.ToString();
            TextBoxTotalQuantity.Text = currentPickStop.TotalQuantityInInventory.ToString();
            TextBoxReceivedDate.Text = currentPickStop.CurrentInventoryLocation.ReceivedDate.ToString("G");
            LabelPrimeBin.Visible = currentPickStop.CurrentInventoryLocation.PrimeBin;
            Task.Run(() => _logger.Log($"UpdateInventoryLocation End : [{System.DateTime.Now.ToLongTimeString()}]"));
        }

        private void ShowShi(int loc1, int loc2, int loc3, string loc4, string text)
        {
            if (_neutronVariables.DisplaysEnabled)
            {
                if (GlobalVar.Displays != null)
                {
                    if (_neutronVariables.ShiEnabled)
                    {
                        ClearAllShi();
                        Task.Run(() => _logger.Log($"Frm Replen Show SHI {loc1}-{loc2}-{loc3}-{loc4}\r\n-{text}"));
                        GlobalVar.Displays.ShowShi(loc1, loc2, loc3, loc4, text);
                    }
                }
            }

        }

        private void ClearAllShi()
        {
            if (_neutronVariables.DisplaysEnabled)
            {
                if (GlobalVar.Displays != null)
                {
                    if (_neutronVariables.ShiEnabled)
                    {
                        Task.Run(() => _logger.Log($"Frm Replen ClearAllShi"));
                        GlobalVar.Displays.ClearAllShi();
                    }
                }
            }
        }

        private void ClearAllBli()
        {
            if (_neutronVariables.DisplaysEnabled)
            {
                if (GlobalVar.Displays != null)
                {
                    if (_neutronVariables.BliEnabled)
                    {
                        Task.Run(() => _logger.Log($"Frm Replen ClearAllBli"));
                        GlobalVar.Displays.ClearAllBli();
                    }
                }
            }
        }

        private void UpdatePickPosition()
        {
            Task.Run(() => _logger.Log($"UpdatePickPosition Start : [{System.DateTime.Now.ToLongTimeString()}]"));
            ClearPickPositions();
            ClearPickDisplays();
            ClearAllBli();

            foreach (var pickView in currentPickStop.PickViews)
            {
                int pos = pickView.PickPosition;
                var font = new Font("Microsoft San Seriff", 24);
                switch (pos)
                {
                    case 1:
                        TextBoxPickPos1.Font = font;
                        TextBoxPickPos1.Text = pickView.QuantityToBePicked.ToString();
                        LabelPickPos1.BackColor = GetBackColor(pickView.QuantityToBePicked);
                        Pos1Display.BackColor = Color.Red;
                        TurnOnBatchPositionDisplay(position: 1, beacon: 2, text: pickView.QuantityToBePicked.ToString());
                        break;
                    case 2:
                        TextBoxPickPos2.Font = font;
                        TextBoxPickPos2.Text = pickView.QuantityToBePicked.ToString();
                        LabelPickPos2.BackColor = GetBackColor(pickView.QuantityToBePicked);
                        Pos2Display.BackColor = Color.Red;
                        TurnOnBatchPositionDisplay(position: 2, beacon: 2, text: pickView.QuantityToBePicked.ToString());
                        break;
                    case 3:
                        TextBoxPickPos3.Font = font;
                        TextBoxPickPos3.Text = pickView.QuantityToBePicked.ToString();
                        LabelPickPos3.BackColor = GetBackColor(pickView.QuantityToBePicked);
                        Pos3Display.BackColor = Color.Red;
                        TurnOnBatchPositionDisplay(position: 3, beacon: 2, text: pickView.QuantityToBePicked.ToString());
                        break;
                    case 4:
                        TextBoxPickPos4.Font = font;
                        TextBoxPickPos4.Text = pickView.QuantityToBePicked.ToString();
                        LabelPickPos4.BackColor = GetBackColor(pickView.QuantityToBePicked);
                        Pos4Display.BackColor = Color.Red;
                        TurnOnBatchPositionDisplay(position: 4, beacon: 2, text: pickView.QuantityToBePicked.ToString());
                        break;
                    case 5:
                        TextBoxPickPos5.Font = font;
                        TextBoxPickPos5.Text = pickView.QuantityToBePicked.ToString();
                        LabelPickPos5.BackColor = GetBackColor(pickView.QuantityToBePicked);
                        Pos5Display.BackColor = Color.Red;
                        TurnOnBatchPositionDisplay(position: 5, beacon: 2, text: pickView.QuantityToBePicked.ToString());
                        break;
                    case 6:
                        TextBoxPickPos6.Font = font;
                        TextBoxPickPos6.Text = pickView.QuantityToBePicked.ToString();
                        LabelPickPos6.BackColor = GetBackColor(pickView.QuantityToBePicked);
                        Pos6Display.BackColor = Color.Red;
                        TurnOnBatchPositionDisplay(position: 6, beacon: 2, text: pickView.QuantityToBePicked.ToString());
                        break;
                    case 7:
                        TextBoxPickPos7.Font = font;
                        TextBoxPickPos7.Text = pickView.QuantityToBePicked.ToString();
                        LabelPickPos7.BackColor = GetBackColor(pickView.QuantityToBePicked);
                        Pos7Display.BackColor = Color.Red;
                        TurnOnBatchPositionDisplay(position: 7, beacon: 2, text: pickView.QuantityToBePicked.ToString());
                        break;
                    case 8:
                        TextBoxPickPos8.Font = font;
                        TextBoxPickPos8.Text = pickView.QuantityToBePicked.ToString();
                        LabelPickPos8.BackColor = GetBackColor(pickView.QuantityToBePicked);
                        Pos8Display.BackColor = Color.Red;
                        TurnOnBatchPositionDisplay(position: 8, beacon: 2, text: pickView.QuantityToBePicked.ToString());
                        break;
                    default:
                        break;
                }
            }
            Task.Run(() => _logger.Log($"UpdatePickPosition End : [{System.DateTime.Now.ToLongTimeString()}]"));
        }

        private void TurnOnBatchPositionDisplay(int position, int beacon, string text)
        {
            if (_neutronVariables.DisplaysEnabled)
            {
                if (GlobalVar.Displays != null)
                {
                    if (_neutronVariables.BliEnabled)
                    {
                        Task.Run(() => _logger.Log($"Frm Replen TurnOnBatchPositionDisplay"));
                        GlobalVar.Displays.ShowBli(position, beacon, text);
                    }
                }
            }
        }

        private Color GetBackColor(int quantityToBePicked)
        {
            if (quantityToBePicked == 0)
            {
                return Color.Green;
            }
            else
            {
                return Color.Gray;
            }
        }

        private string GetRemainingToPick(int totalQty, int pickedSoFar)
        {
            int result = totalQty - pickedSoFar;

            if (result == 0)
            {
                return string.Empty;
            }
            else
            {
                return result.ToString();
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
        }

        private int GetTotalRequiredThisStop(ReplenPickStop currentPickStop)
        {
            int total = TextBoxPickPos1.Text.ParseInt();
            total += TextBoxPickPos2.Text.ParseInt();
            total += TextBoxPickPos3.Text.ParseInt();
            total += TextBoxPickPos4.Text.ParseInt();
            total += TextBoxPickPos5.Text.ParseInt();
            total += TextBoxPickPos6.Text.ParseInt();
            total += TextBoxPickPos7.Text.ParseInt();
            total += TextBoxPickPos8.Text.ParseInt();
            return total;
        }

        private void MBPickAccept_Click(object sender, EventArgs e)
        {

            Cursor.Current = Cursors.WaitCursor;
            Task.Run(() => _logger.Log($"PickAccept_Click Start : [{System.DateTime.Now.ToLongTimeString()}]"));
            bool pick = false;
            pick = currentPickStop.CurrentInventoryLocation.Quantity < currentPickStop.QuantityToBePicked ? false : true;

            if (pick)
            {
                currentPickStop.UpdatePickViews(GlobalVar.User);  //good
                currentPickStop.PickedQty = GetPickedSoFar(currentPickStop.PickViews);
                currentPickStop.QuantityToBePicked = GetTotalQuantityToBePicked(currentPickStop.PickViews);  // QuantityToBePicked on ALL PickViews
                Task.Run(() => _logger.Log($"PickAccept_Click 1 : [{System.DateTime.Now.ToLongTimeString()}]"));
                if (StopComplete())
                {
                    //Getting next location on the current device/ the one that was just picked from.
                    Task.Run(() => _deviceManager.MoveNext(currentPickStop.CurrentInventoryLocation.Location.Loc1));
                    Task.Run(() => _logger.Log($"PickAccept_Click 2 Stop Complete Start : [{System.DateTime.Now.ToLongTimeString()}]"));

                    UpdateInventoryQuantity(currentPickStop);

                    GlobalVar.HistoryManager.SaveHistory(ActionCode.StoreOrder, currentPickStop);

                    currentPickStop.SetPickViewsComplete(GlobalVar.User);

                    Task.Run(() => _logger.Log($"PickAccept_Click Stop Complete End : [{System.DateTime.Now.ToLongTimeString()}]"));

                    int numberOfStops = bindingSourcePickStops.Count;
                    if (currentPickStop.Sequence < numberOfStops)
                    {
                        bindingSourcePickStops.MoveNext();
                        currentPickStop = (ReplenPickStop)bindingSourcePickStops.Current;
                        UpdatePickScreen();
                    }
                    else
                    {
                        CloseBatch();
                    }

                }
                else  //ReplenPickStop is NOT complete, why?
                {
                    UpdatePickScreen();
                }
            }
            else
            {
                MessageBox.Show("Pick Exceeds Inventory at this location.  Add Inventory or Change Quantity before continuing.", "Inventory", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            Task.Run(() => _logger.Log($"PickAccept_Click End : [{System.DateTime.Now.ToLongTimeString()}]"));
            Cursor.Current = Cursors.Default;
        }

        private void MBStoreAccept_Click(object sender, EventArgs e)
        {

            Cursor.Current = Cursors.WaitCursor;
            Task.Run(() => _logger.Log($"StoreAccept_Click Start : [{System.DateTime.Now.ToLongTimeString()}]"));
            //bool pick = false;
            //pick = currentPickStop.CurrentInventoryLocation.Quantity < currentPickStop.QuantityToBePicked ? false : true;

            // if (pick)
            // {
            currentPickStop.UpdatePickViews(GlobalVar.User);  //good
            currentPickStop.PickedQty = GetPickedSoFar(currentPickStop.PickViews);
            currentPickStop.QuantityToBePicked = GetTotalQuantityToBePicked(currentPickStop.PickViews);  // QuantityToBePicked on ALL PickViews
            Task.Run(() => _logger.Log($"PickAccept_Click 1 : [{System.DateTime.Now.ToLongTimeString()}]"));
            if (StopComplete())
            {
                //Getting next location on the current device/ the one that was just picked from.
                Task.Run(() => _deviceManager.MoveNext(currentPickStop.CurrentInventoryLocation.Location.Loc1));
                Task.Run(() => _logger.Log($"PickAccept_Click 2 Stop Complete Start : [{System.DateTime.Now.ToLongTimeString()}]"));

                UpdateInventoryQuantity(currentPickStop);

                GlobalVar.HistoryManager.SaveHistory(ActionCode.StoreOrder, currentPickStop);

                currentPickStop.SetPickViewsComplete(GlobalVar.User);

                Task.Run(() => _logger.Log($"PickAccept_Click Stop Complete End : [{System.DateTime.Now.ToLongTimeString()}]"));

                int numberOfStops = bindingSourcePickStops.Count;
                if (currentPickStop.Sequence < numberOfStops)
                {
                    bindingSourcePickStops.MoveNext();
                    currentPickStop = (ReplenPickStop)bindingSourcePickStops.Current;
                    UpdatePickScreen();
                }
                else
                {
                    CloseBatch();
                }

            }
            else  //ReplenPickStop is NOT complete, why?
            {
                UpdatePickScreen();
            }
            //}
            //else
            //{
            //    MessageBox.Show("Pick Exceeds Inventory at this location.  Add Inventory or Change Quantity before continuing.", "Inventory", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            //}
            Task.Run(() => _logger.Log($"StoreAccept_Click End : [{System.DateTime.Now.ToLongTimeString()}]"));
            Cursor.Current = Cursors.Default;
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

        private bool StopComplete()
        {
            if (currentPickStop.QuantityToBePicked == 0)
            {
                return true;
            }
            else if (currentPickStop.QuantityToBePicked < 0)
            {
                DialogResult result = MessageBox.Show("Do you want to overpick this item?", "Overpick Question"
                    , MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
                    DialogResult result = MessageBox.Show("No more locations, Pick Short?", "Short Pick", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        private void UpdateInventoryQuantity(ReplenPickStop pickStop)
        {

            foreach (ReplenPickView pickView in pickStop.PickViews)
            {
                foreach (PickLocation pickLocation in pickView.PickLocations)
                {
                    pickLocation.Inventory.Quantity += pickLocation.Quantity;

                    Inventory inv = repoInventory.FindByKey(pickLocation.Inventory.Id);
                    inv.Quantity = pickLocation.Inventory.Quantity;

                    repoInventory.Update(inv);

                }
            }
        }
        //TODO
        private void CloseBatch()
        {
            if (_neutronVariables.DisplaysEnabled)
            {
                if (GlobalVar.Displays != null)
                {
                    if (_neutronVariables.BliEnabled)
                    {
                        Task.Run(() => _logger.Log($"Frm Replen  CloseBatch"));
                        GlobalVar.Displays.ClearAllBli();
                    }

                    if (_neutronVariables.ShiEnabled)
                    {
                        Task.Run(() => _logger.Log($"Frm Replen CloseBatch"));
                        GlobalVar.Displays.ClearAllShi();
                    }
                }
            }

            ClearOrderPositions();
            ClearBatchPositions();

            _logger.Log($"Start Upload Processor: {neutronLicense.CompanyCode}");
            switch (neutronLicense.CompanyCode)
            {
                case "TMG":
                    var uploadProcessor = new UploadProcessor(neutronLicense, _neutronVariables, _logger);
                    uploadProcessor.CreateHostFile(bindingSourcePickStops);
                    break;
                case "SFH":
                    _logger.Log("Choosing the Nova SFH case.");
                    uploadProcessor = new UploadProcessor(neutronLicense, _neutronVariables, _logger);
                    uploadProcessor.CreateReplenHostFile(bindingSourcePickStops);
                    break;
                case "AES":
                    uploadProcessor = new UploadProcessor(neutronLicense, _neutronVariables, _logger);
                    uploadProcessor.CreateHostFile(bindingSourcePickStops);
                    break;
                case "TOP":
                    var topUploadProcessor = new TopUploadProcessor(_neutronVariables, neutronLicense);
                    topUploadProcessor.CreateReplenHostFile(bindingSourcePickStops);
                    break;

                default:
                    break;
            }



            //--------------
            //var uploadProcessor = new UploadProcessor(neutronLicense, neutronVariables, logger);
            //Task.Run(() => uploadProcessor.CreateReplenHostFile(bindingSourcePickStops));

            ShowAllOrders();
            ShowAvailableOrders();
            tabControl1.SelectedTab = AvailableOrders;

        }

        private void PrintAllDocuments()
        {
            foreach (var bp in _ordersToPick)
            {
                if (bp.OrderId == null) continue;
                var id = bp.OrderId.Value;
                var order = repoReplenOrder.FindByKey(id);
                var printJob = repoPrintJob.FindBy(r => r.OrderId == order.Id && r.PickDocument == true).FirstOrDefault();
                if (printJob != null) continue;
                PrintDoc(bp.PositionNumber, order);
                printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, PickDocument = true };
                repoPrintJob.Insert(printJob);
            }
        }

        private void PrintDocument(int batchPosition)
        {
            foreach (var bp in _ordersToPick)
            {
                if (bp.PositionNumber != batchPosition) continue;
                if (bp.OrderId == null) continue;
                var id = bp.OrderId.Value;
                var order = repoReplenOrder.FindByKey(id);
                var printJob = repoPrintJob.FindBy(r => r.OrderId == order.Id && r.PickDocument == true).FirstOrDefault();
                if (printJob != null) continue;
                PrintDoc(bp.PositionNumber, order);
                printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, PickDocument = true };
                repoPrintJob.Insert(printJob);
            }
        }

        private void PrintDoc(int positionNumber, ReplenOrder order)
        {
            Task.Run(() => _logger.Log($"Printing Document. {order.Ord1}"));
            if (_neutronVariables.EnableDocumentPrinter)
            {
                Task.Run(() => DocumentToPrint.Print(positionNumber, order.Ord1, _documentPrinter, order.Ord2));
            }
        }

        private void PrintAllToteLabels()
        {
            foreach (var bp in _ordersToPick)
            {
                if (bp.OrderId == null) continue;
                var id = bp.OrderId.Value;
                var order = repoReplenOrder.FindByKey(id);
                var printJob = repoPrintJob.FindBy(r => r.OrderId == order.Id && r.ToteLabel == true).FirstOrDefault();
                if (printJob != null) continue;
                PrintTote(bp.PositionNumber, order);
                printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, ToteLabel = true };
                repoPrintJob.Insert(printJob);
            }
        }

        private void PrintToteLabel(int batchPosition)
        {
            foreach (var bp in _ordersToPick)
            {
                if (bp.PositionNumber != batchPosition) continue;
                if (bp.OrderId == null) continue;
                var id = bp.OrderId.Value;
                var order = repoReplenOrder.FindByKey(id);
                var printJob = repoPrintJob.FindBy(r => r.OrderId == order.Id && r.ToteLabel == true).FirstOrDefault();
                if (printJob != null) continue;
                PrintTote(bp.PositionNumber, order);
                printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, ToteLabel = true };
                repoPrintJob.Insert(printJob);
            }
        }

        private void PrintTote(int positionNumber, ReplenOrder order)
        {
            Task.Run(() => _logger.Log($"Printing Tote Label. {order.Ord1}"));
            if (_neutronVariables.EnableLabelPrinter)
            {
                Task.Run(() => ToteToPrint.Print(positionNumber, order, _labelPrinter));
            }
        }



        private void MarkCompleted(List<BatchPosition> ordersToPick)
        {
            foreach (BatchPosition bp in ordersToPick)
            {
                if (bp.OrderId != null)
                {
                    int id = bp.OrderId.Value;
                    ReplenOrder rec = repoReplenOrder.FindByKey(id);
                    rec.OrderStatusId = 6;
                    repoReplenOrder.Update(rec);
                    GlobalVar.HistoryManager.SaveHistory(ActionCode.OrderComplete, rec);
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
            using (FrmChangeQuantity form = new FrmChangeQuantity(currentPickStop))
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
            int loc1 = currentPickStop.CurrentInventoryLocation.Location.Loc1;
            int loc2 = currentPickStop.CurrentInventoryLocation.Location.Loc2;
            int loc3 = currentPickStop.CurrentInventoryLocation.Location.Loc3;
            int loc4 = currentPickStop.CurrentInventoryLocation.Location.Loc4;
            PositionDevice(loc1, loc2, loc3, loc4, moveDevice: true);
        }

        private void MBPriority_Click(object sender, EventArgs e)
        {
            int priority = 0;
            List<int> recs = GetCheckedOrderIds();
            if (recs.Count() > 0)
            {
                using (FrmChangePriority form = new FrmChangePriority())
                {
                    DialogResult result = form.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        priority = (form.NewPriority).ParseInt();
                    }
                }

                foreach (int id in recs)
                {
                    ReplenOrder ord = repoReplenOrder.FindByKey(id);
                    ord.Priority = priority;
                    repoReplenOrder.Update(ord);
                    GlobalVar.HistoryManager.SaveHistory(ActionCode.ChangePriority, ord);
                }
            }
            ShowAllOrders();
        }

        private void MBReturnToStock_Click(object sender, EventArgs e)
        {
            var uploadProcessor = new UploadProcessor(neutronLicense, _neutronVariables, _logger);
            List<ReplenOrderView> recs = GetCheckedOrders();
            if (recs.Count() > 0)
            {
                foreach (var ov in recs)
                {
                    if (ov != null)
                    {
                        if (_neutronVariables.UseReturnToStock)
                        {
                            //int rtsCode = (int)OrderStatus.Returned;

                            foreach (ReplenOrderDetail detail in ov.ReplenOrder.ReplenOrderDetails)
                            {
                                SetOrderDetailLineStatus(detail, (int)OrderStatus.Returned, ActionCode.OrderDetailRts);
                            }

                            uploadProcessor.ReturnOrderToStock(ov.ReplenOrder);  //sets the RTS code to each OrderDetail line
                            ov.ReplenOrder.OrderStatusId = (int)OrderStatus.Returned;  //Returned
                            repoReplenOrder.Update(ov.ReplenOrder);
                            GlobalVar.HistoryManager.SaveHistory(ActionCode.OrderRts, ov.ReplenOrder);
                        }
                        //else
                        //{
                        //    ov.ReplenOrder.OrderStatusId = (int) OrderStatus.Deleted;  //Deleted
                        //    repoReplenOrder.Update(ov.ReplenOrder);
                        //    GlobalVar.HistoryManager.SaveHistory(ActionCode.Order
                        // Archived, ov.ReplenOrder);
                        //}
                        if (_neutronVariables.CreateStoreOrderWithRts)
                        {
                            CreateStoreOrderFromOrderDetailComplete(ov.ReplenOrder);
                        }
                    }
                }
            }
            // ListBoxSelectedOrders.Items.Clear();
            ShowAllOrders();
        }

        private void SetOrderStatus(ReplenOrder order, int status, ActionCode actionCode)
        {
            try
            {
                order.OrderStatusId = status;
                repoReplenOrder.Update(order);
                GlobalVar.HistoryManager.SaveHistory(actionCode, order);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error setting ReplenOrder Detail Status Code. " + ex.Message);
            }
        }

        private void SetOrderDetailLineStatus(ReplenOrderDetail detail, int code, ActionCode actionCode)
        {
            try
            {
                detail.LineStatusId = code;
                repoReplenOrderDetail.Update(detail);
                GlobalVar.HistoryManager.SaveHistory(actionCode, detail);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error setting ReplenOrder Detail Status Code. " + ex.Message);
            }
        }

        //private void SetOrderDetailPickedQuantity(OrderDetail detail, int qty)
        //{
        //    try
        //    {
        //        detail.PickedQuantity = qty;
        //        repoReplenOrderDetail.Update(detail);
        //        GlobalVar.HistoryManager.SaveHistory((int)ActionCode.PickOrder, detail);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Error setting ReplenOrder Detail Status Code. " + ex.Message);
        //    }
        //}

        private void MBJobDetails_Click(object sender, EventArgs e)
        {
            var row = DataGridView1.CurrentRow;
            if (row == null || row.Index < 0) return;
            var id = Convert.ToInt32(row.Cells["Id"].Value);
            if (id > 0)
            {
                ShowOrderDetailsByOrder(id);
            }
        }

        private void ShowOrderDetailsByOrder(int orderId)
        {
            //List<int> recs = GetCheckedOrderIds();
            //if (recs.Count() > 0)
            // {
            var details = orderDetailsRepository.GetOrderDetailsViewByOrder(orderId);
            bindingSourceOrderDetailsView.DataSource = details;
            DataGridViewOrderDetails.DataSource = bindingSourceOrderDetailsView;
            LabelFormTitle.Text = "Job Details";
            tabControl1.SelectedTab = OrderDetails;
            // }

        }

        private void MBCreateOrder_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "New Job";
            tabControl1.SelectedTab = NewOrder;
        }


        private void MBMainOrderManager_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Task.Run(() => _logger.Log($"Job Manager Main Screen Start"));
            var watch = new Stopwatch();
            watch.Start();
            LabelFormTitle.Text = "Job Listing";
            LabelFormTitle.BackColor = Color.Green;
            ShowAllOrders();
            var twatch = new Stopwatch();
            twatch.Start();
            tabControl1.SelectedTab = OrderListing;
            twatch.Stop();
            Task.Run(() => _logger.Log($"Load Tab Control Elasped MSec:  {twatch.ElapsedMilliseconds}ms"));

            watch.Stop();
            Task.Run(() => _logger.Log($"Job Manager Main screen Elasped MSec:  {watch.ElapsedMilliseconds}ms"));
            Cursor.Current = Cursors.Default;
        }

        private void MBMainAvailableOrders_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            LabelFormTitle.Text = "Available Jobs";
            LabelFormTitle.BackColor = Color.Green;
            ClearSelection();
            ClearOrderPositions();
            InitOrdersToPick(_neutronVariables.StoreBatchSize);
            ShowAvailableOrders();
            if (_station.StationNumber == 8)
            {
                tabControl1.SelectedTab = AvailableRack;
            }
            else
            {
                tabControl1.SelectedTab = AvailableOrders;
            }
            Cursor.Current = Cursors.Default;
        }

    //New ReplenOrder
        private void MBMainNewOrder_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "New Job";
            LabelFormTitle.BackColor = Color.Green;
            bindingSourceItems.DataSource = GetItemsList();
            DataGridViewNewOrder.DataSource = bindingSourceItems;
            tabControl1.SelectedTab = NewOrder;
            DataGridViewNewOrder.ClearSelection();
        }
        //New ReplenOrder
        private List<NewItemView> GetItemsList()
        {
            var recs = new List<NewItemView>();
            recs = repoInventory.AllInclude(r => r.ItemDefinition).Select(d => new NewItemView()
            {
                //InventoryId = d.Id
                //,
                ItemDefinitionId = d.ItemDefinition.Id,
                Description = d.ItemDefinition.Description
                ,
                Item = d.ItemDefinition.Item,
                Quantity = d.Quantity
            }).ToList();
            return recs;
        }

        private void MBMainLoadOrders_Click(object sender, EventArgs e)
        {
            MBMainLoadOrders.Enabled = false;
            if (GlobalVar.LoaderRunning)
            {
                DialogResult result = MessageBox.Show("Do you want to stop the loader?", "Loader", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    MBMainLoadOrders.Text = "Start Loader";
                    if (interfaceProcessor != null)
                    {
                        interfaceProcessor.StopProcessingInterfaceFiles();

                    }


                    GlobalVar.LoaderRunning = false;
                }
            }
            else
            {
                interfaceProcessor = new InterfaceProcessor(_neutronVariables, neutronLicense, _jsonData);
                interfaceProcessor.StartProcessingInterfaceFiles();
                MBMainLoadOrders.Text = "Stop Loader";
                GlobalVar.LoaderRunning = true;
            }
            MBMainLoadOrders.Enabled = true;
        }

        private void MBMainClose_Click(object sender, EventArgs e)
        {
            CloseButtonPressed = true;
        }

        private void MBBackHotPick_Click(object sender, EventArgs e)
        {
            if (openHotPickFromPickScreen)
            {
                LabelFormTitle.Text = "Selection";
                LabelFormTitle.BackColor = Color.Green;
                tabControl1.SelectedTab = PickScreen;
                openHotPickFromPickScreen = false;
            }
            else
            {
                LabelFormTitle.Text = "Jobs";
                LabelFormTitle.BackColor = Color.Green;
                tabControl1.SelectedTab = Main;
            }
        }

        private void MBNewOrderClose_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Job Listing";
            LabelFormTitle.BackColor = Color.Green;
            tabControl1.SelectedTab = OrderListing;
        }

        private void MBOrderDetailsBack_Click(object sender, EventArgs e)
        {
            DataGridViewAvailableOrders.Refresh();
            LabelFormTitle.Text = "Job Listing";
            LabelFormTitle.BackColor = Color.Green;
            tabControl1.SelectedTab = OrderListing;
        }

        private void MBLocationCount_Click(object sender, EventArgs e)
        {
            int inventoryId = currentPickStop.CurrentInventoryLocation.Id;
            int qty = OpenLocationCountForm(inventoryId);

            if (qty >= 0)
            {
                TextBoxLocationQuantity.Text = qty.ToString();
                currentPickStop.CurrentInventoryLocation.Quantity = qty;
                int total = currentPickStop.Inventory.Sum(r => r.Quantity);
                TextBoxTotalQuantity.Text = total.ToString();
            }
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
                else
                {

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
            GlobalVar.HistoryManager.SaveHistory(ActionCode.InventoryModify, inv);

            var cnt = new LocationCount()
            {
                InventoryId = inv.Id,
                ItemDefinitionId = inv.ItemDefinitionId,
                LocationId = inv.LocationId,
                UserId = GlobalVar.User.Id,
                PreviousQty = prevQty,
                NewQty = qty,
                CountDate = System.DateTime.Now,
            };
            repoLocationCount.Insert(cnt);
            GlobalVar.HistoryManager.SaveHistory(ActionCode.LocationCount, cnt);
        }

        private void SetCurrentInventoryView(int inventoryId)
        {
            InventoryView rec = bindingSourceHot.List.OfType<InventoryView>().ToList().Find(f => f.Id == inventoryId);
            int pos = bindingSourceHot.IndexOf(rec);
            bindingSourceHot.Position = pos;
            currentInventoryView = (SqlInventoryView)bindingSourceHot.Current;
        }

     
        private void MBCompleted_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (MBCompleted.Text == "Completed")
            {
                ShowCompleted();
                SelectAll();
                MBCompleted.Text = "Compress";
            }
            else
            {
                MBCompleted.Text = "Completed";
                CompressOrders();
            }
            Cursor.Current = Cursors.Default;


            //if (MBCompleted.Text == "Completed")
            //{
            //    bindingSourceCompleted.DataSource = ordersRepository.GetCompletedOrders();
            //    DataGridView1.DataSource = bindingSourceCompleted;
            //    DataGridView1.ClearSelection();
            //    MBCompleted.Text = "Jobs";
            //    HideButtons();
            //}
            //else if (MBCompleted.Text == "Jobs")
            //{
            //    ShowAllOrders();
            //    MBCompleted.Text = "Completed";
            //    ShowButtons();
            //}
            //SyncCurrentlySelectedWithDataGrid();
        }

        private void CompressOrders()
        {
            var ids = GetCheckedOrderIds();
            const string orderType = "REPLEN";
            var sb = new StringBuilder();
            foreach (var i in ids)
            {
                sb.Append(i + ",");
            }
            var orderIds = sb.ToString().TrimEnd(',');

            using (var context = new NeutronDb())
            {
                var paramOrderIds = new SqlParameter("@ORDERIDS", SqlDbType.VarChar) { Value = orderIds };
                var paramOrderType = new SqlParameter("@ORDERTYPE", SqlDbType.VarChar) { Value = orderType };

                var parameters = new object[] { paramOrderIds, paramOrderType };

                context.Database.ExecuteSqlCommand("usp_CompressOrders @ORDERIDS, @ORDERTYPE", paramOrderIds,
                    paramOrderType);
            }

            ShowAllOrders();
        }

        private void SelectAll()
        {
            try
            {
                foreach (DataGridViewRow row in DataGridView1.Rows)
                {
                    var chk = (DataGridViewCheckBoxCell)row.Cells[0];
                    chk.Value = chk.TrueValue;
                    if (row.IsNewRow)
                    {
                        chk.Value = chk.FalseValue;
                    }
                }
            }
            catch (Exception ex)
            {
                Task.Run(() => _logger.Log($"Select All Error: {ex.Message} {Environment.NewLine} {ex.InnerException} [{DateTime.Now.ToLongTimeString()}]"));
            }
        }


        private void ShowCompleted()
        {
            bindingSourceCompleted.DataSource = ordersRepository.GetCompletedOrders();
            DataGridView1.DataSource = bindingSourceCompleted;
            DataGridView1.ClearSelection();
        }

        private void ShowButtons()
        {
            MBRefresh.Visible = true;
            MBPriority.Visible = true;
            MBHold.Visible = true;
            MBRelease.Visible = true;
            MBReturnToStock.Visible = _neutronVariables.UseReturnToStock;
            MBDeleteOrder.Visible = true;
        }

        private void HideButtons()
        {
            MBRefresh.Visible = false;
            MBPriority.Visible = false;
            MBHold.Visible = false;
            MBRelease.Visible = false;
            MBReturnToStock.Visible = false;
            MBDeleteOrder.Visible = false;
        }

        //New ReplenOrder
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
                var currentItem = (NewItemView)bindingSourceItems.Current;
                LabelNewOrderItemId.Text = currentItem.ItemDefinitionId.ToString();
                LabelNewOrderItem.Text = currentItem.Item;
                LabelNewOrderDescription.Text = currentItem.Description;
                TextBoxNewOrderQuantity.Focus();
            }
        }

        private void ButtonAddDetail_Click(object sender, EventArgs e)
        {
            var rec = new NewItemView()
            {
                ItemDefinitionId = (LabelNewOrderItemId.Text).ParseInt()
                ,
                Item = LabelNewOrderItem.Text,
                Description = LabelNewOrderDescription.Text
                ,
                Quantity = (TextBoxNewOrderQuantity.Text).ParseInt()
            };
            var item = new ListViewItem(new[] { rec.ItemDefinitionId.ToString(), rec.Item, rec.Description, rec.Quantity.ToString() });

            ListViewNewItems.Items.Add(item);
            ClearNewOrderDetail();
        }

        private void ClearNewOrderDetail()
        {
            LabelNewOrderItem.Text = "";
            LabelNewOrderDescription.Text = "";
            TextBoxNewOrderQuantity.Text = "";
        }

        private void InitListView()
        {
            ListViewNewItems.Columns.Add("Id", 80, HorizontalAlignment.Left);
            ListViewNewItems.Columns.Add("Item", 80, HorizontalAlignment.Left);
            ListViewNewItems.Columns.Add("Description", 200, HorizontalAlignment.Left);
            ListViewNewItems.Columns.Add("Quantity", 80, HorizontalAlignment.Left);
        }

        private void MBNewOrderSave_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(TextBoxNewOrderOrd1.Text) && ListViewNewItems.Items.Count > 0)
            {
                var orderDetails = new List<ReplenOrderDetail>();

                var order = new ReplenOrder()
                {
                    Ord1 = TextBoxNewOrderOrd1.Text,
                    Ord2 = TextBoxNewOrderOrd2.Text,
                    Priority = (TextBoxNewOrderPriority.Text).ParseInt(),
                    LoadDate = System.DateTime.Now,
                    ShipperId = 1,
                    ShipMethodId = 1,
                    OrderStatusId = 1
                };
                repoReplenOrder.Insert(order);

                for (var i = 0; i < ListViewNewItems.Items.Count; i++)
                {
                    var itemDefinitionId = (ListViewNewItems.Items[i].SubItems[0].Text).ParseInt();

                    var rec = new ReplenOrderDetail()
                    {
                        ItemDefinitionId = itemDefinitionId,
                        ReplenOrderId = order.Id,
                        JobNum = order.Ord1,
                        PartNum = ListViewNewItems.Items[i].SubItems[1].Text,
                        PartDesc = ListViewNewItems.Items[i].SubItems[2].Text,
                        Quantity = (ListViewNewItems.Items[i].SubItems[3].Text).ParseInt(),
                        LineStatusId = 1,
                        StationNumber = repoInv.GetStationNumber(itemDefinitionId),
                        ReplenOrder = order,
                        DateTime = DateTime.Now.ToString("g"),
                        EmpId = GlobalVar.User.EmpId
                    };

                    repoReplenOrderDetail.Insert(rec);
                }
                TextBoxNewOrderOrd1.Text = string.Empty;
                TextBoxNewOrderOrd2.Text = string.Empty;
                ListViewNewItems.Clear();
            }
        }

        private void MBPickNewItem_Click(object sender, EventArgs e)
        {
            using (FrmNewItemAuth form = new FrmNewItemAuth())
            {
                DialogResult result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    if (form.AuthCode == "topura")
                    {
                        foreach (var item in currentPickStop.PickViews)
                        {
                            ItemDefinition newItemDefinition = repoItemDefinition.FindBy(f => f.Item == item.OrderDetail.PartNum).FirstOrDefault();

                            item.Description = item.OrderDetail.PartDesc;
                            item.Item = item.OrderDetail.PartNum;
                            item.ItemId = newItemDefinition.Id;

                            List<Inventory> recs = GetInventory(item.ItemId);

                            if (recs.Count > 0)
                            {
                                //Prime Bin First
                                var exactInventorySequence = new List<Inventory>();
                                if (_neutronVariables.UsePrimeBin)
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
                                item.SlotQty = item.CurrentInventoryLocation.Quantity;
                                item.InventoryIndex = 0;
                                item.ReceivedDate = item.CurrentInventoryLocation.ReceivedDate;
                            }
                            else
                            {
                                MessageBox.Show("No Inventory for Item: " + item.Item);
                            }
                        }

                        currentPickStop.CurrentInventoryLocation = currentPickStop.PickViews.First().CurrentInventoryLocation;
                        currentPickStop.Description = currentPickStop.PickViews.First().Description;
                        currentPickStop.Images = currentPickStop.PickViews.First().Images;
                        currentPickStop.Inventory = currentPickStop.PickViews.First().Inventory;
                        currentPickStop.InventoryIndex = currentPickStop.PickViews.First().InventoryIndex;
                        currentPickStop.Item = currentPickStop.PickViews.First().Item;
                        currentPickStop.ItemId = currentPickStop.PickViews.First().ItemId;
                        currentPickStop.Ord1 = currentPickStop.PickViews.First().Ord1;
                        currentPickStop.Ord2 = currentPickStop.PickViews.First().Ord2;
                        currentPickStop.OrderId = currentPickStop.PickViews.First().ReplenOrderId;
                        currentPickStop.PickedQty = currentPickStop.PickViews.First().PickedQty;
                        currentPickStop.Quantity = currentPickStop.PickViews.First().Quantity;
                        currentPickStop.QuantityToBePicked = currentPickStop.PickViews.First().QuantityToBePicked;
                        currentPickStop.Slot = currentPickStop.PickViews.First().Slot;
                        currentPickStop.SlotQty = currentPickStop.PickViews.First().SlotQty;
                        currentPickStop.TotalQuantityInInventory = currentPickStop.PickViews.First().TotalQuantityInInventory;

                        UpdatePickScreen();
                    }
                }
            }
        }

        private void MBPickRefresh_Click(object sender, EventArgs e)
        {
            ShowAllOrders();
        }


        private void ButtonImageNext_Click(object sender, EventArgs e)
        {

        }

        private void ButtonImagePrevious_Click(object sender, EventArgs e)
        {

        }

        private void TextBoxPos1_TextChanged(object sender, EventArgs e)
        {


        }

        private List<ReplenOrderView> GetValidOrdersFromBindingSource(string orderNumber)
        {
            IEnumerable<ReplenOrderView> list = bindingSourceAvailableOrders.List.OfType<ReplenOrderView>();
            return list.Where(s => s.Ord1 == orderNumber).ToList();
        }

        private bool CheckForMultipleOrders(string orderNumber)
        {
            var ordersWithThisOrderNumber = new List<DataGridViewRow>();
            foreach (DataGridViewRow row in DataGridViewAvailableOrders.Rows)
            {
                string ord1 = (row.Cells["Ord1"].Value).ToString();
                if (orderNumber == ord1)
                {
                    ordersWithThisOrderNumber.Add(row);
                }
            }
            if (ordersWithThisOrderNumber.Count > 1)
            {
                return true;
            }
            return false;
        }

        private void TextBoxEnter(object sender, EventArgs e)
        {
            var textBox = (TextBox)sender;
            CurrentTextBoxPos = textBox;
            gridClickedAvailableOrders = false;
            //textBox.SelectAll();
            textBox.Focus();
        }

        private void TextBoxPosLeave(object sender, EventArgs e)
        {
            TextBox textBox = ((TextBox)sender);
            string orderNumber = textBox.Text;
            int position = textBox.Tag.ToString().ParseInt();

            if (!string.IsNullOrEmpty(orderNumber))
            {
                if (!ValidateOrderAndPosition(position, orderNumber))
                {
                    textBox.SelectAll();
                    textBox.Focus();
                }
            }
        }

        private bool ValidateOrderAndPosition(int position, string orderNumber)
        {

            List<ReplenOrderView> orders = GetValidOrdersFromBindingSource(orderNumber);
            //ordersToPick 
            if (orders == null) return false;
            var rowsWithThisOrderNumber = new List<DataGridViewRow>();
            foreach (DataGridViewRow row in DataGridViewAvailableOrders.Rows)
            {
                var ord1 = (row.Cells["Ord1"].Value).ToString();
                if (orderNumber.Trim() == ord1.Trim())
                {
                    rowsWithThisOrderNumber.Add(row);
                }
            }

            foreach (var row in rowsWithThisOrderNumber)
            {
                var chk = (DataGridViewCheckBoxCell)row.Cells[0];
                var idValue = row.Cells["Id"].Value.ToString().ParseInt();
                var ord1 = row.Cells["Ord1"].Value.ToString();
                var ord2 = row.Cells["Ord2"].Value.ToString();
                var bp = _ordersToPick.FirstOrDefault(r => r.OrderId == idValue);

                if (bp != null) continue;
                //not in a Batch POsition
                //position is good
                chk.Value = chk.TrueValue;
                AddItemToBatch(idValue, ord1, ord2);
                return true;
            }

            return false;
        }

        private void TextBoxPosKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send(keys: "{Tab}");
            }
        }



        #region Find Functions Available Orders Screen


        private void TextBoxFindAvailableOrders_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                ShowAvailableOrders();
            }
            if (e.KeyCode == Keys.Escape)
            {
                TextBoxFindAvailableOrders.Text = "";
            }
        }

        private void MBSearchAvailableOrders_Click(object sender, EventArgs e)
        {
            ShowAvailableOrders();
        }

        private void ButtonClearFindAvailableOrders_Click(object sender, EventArgs e)
        {
            TextBoxFindAvailableOrders.Text = string.Empty;
            ShowAvailableOrders();
            TextBoxFindAvailableOrders.Focus();
        }

        #endregion

        #region Find Functions Main Orders

        private void MButtonSearch_Click(object sender, EventArgs e)
        {
            ShowAllOrders();
        }

        private void TextBoxFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                ShowAllOrders();
            }
            if (e.KeyCode == Keys.Escape)
            {
                TextBoxFind.Text = "";
            }
        }
        private void ButtonClear_Click(object sender, EventArgs e)
        {
            TextBoxFind.Text = string.Empty;
            ShowAllOrders();
            TextBoxFind.Focus();
        }


        #endregion

        private void MBAvailableOrdersRefresh_Click(object sender, EventArgs e)
        {
            ShowAvailableOrders();
        }

        private void MBRefresh_Click(object sender, EventArgs e)
        {
            ShowAllOrders();
        }



        private void MBClearSelectionReturntoStockDetail_Click(object sender, EventArgs e)
        {
            DataGridViewOrderDetails.ClearSelection();
            foreach (DataGridViewRow row in DataGridViewOrderDetails.Rows)
            {
                var chk = (DataGridViewCheckBoxCell)row.Cells[0];
                chk.Value = chk.FalseValue;
            }
        }

        private void ShowOrderDetails(int orderId)
        {
            List<ReplenOrderDetailsView> details = orderDetailsRepository.GetOrderDetailsViewByOrder(orderId).ToList();

            bindingSourceOrderDetailsView.DataSource = details;
            DataGridViewOrderDetails.DataSource = bindingSourceOrderDetailsView;
            DataGridViewOrderDetails.ClearSelection();
        }

        private void CreateStoreOrderFromOrderDetailLine(ReplenOrderDetail orderDetail)
        {
            ReplenOrderDetail detailLine = repoReplenOrderDetail.FindBy(r => r.Id == orderDetail.Id).FirstOrDefault();
            if (detailLine != null)
            {
                StoreOrderDetails(detailLine);
            }
        }

        private int GetTrayNumber(string primeBin)
        {
            int result = 99;
            if (neutronLicense.CompanyCode == "TOP")
            {
                result = int.Parse(primeBin.Substring(2, 2));
            }
            return result;
        }

        private void StoreOrderDetails(ReplenOrderDetail detailLine)
        {
            ReplenOrderDetail firstRec = detailLine;
            if (firstRec != null)
            {
                var replenOrder = new ReplenOrder()
                {
                    Ord1 = firstRec.JobNum,
                    Ord2 = firstRec.EmpId,
                    Priority = GetTrayNumber(firstRec.PrimeBin),
                    LoadDate = System.DateTime.Now,
                    ShipperId = 1,
                    ShipMethodId = 1,
                    OrderStatusId = 1
                };
                try
                {
                    repoReplenOrder.Insert(replenOrder);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Insert Replen ReplenOrder Error " + ex.Message);
                }

                ItemDefinition storeItemDefinition = repoItemDefinition.FindBy(r => r.Item == detailLine.PartNum).FirstOrDefault();
                if (storeItemDefinition != null)
                {
                    Inventory rec = repoInventory.FindBy(r => r.ItemDefinitionId == storeItemDefinition.Id && r.Location.Slot == detailLine.PrimeBin).FirstOrDefault();
                    if (rec != null)
                    {
                        var replenOrderDetail = new ReplenOrderDetail()
                        {
                            ReplenOrderId = replenOrder.Id,
                            ItemDefinitionId = rec.ItemDefinitionId,
                            Quantity = (detailLine.Qty).ParseInt(),
                            DateTime = detailLine.DateTime,
                            EmpId = detailLine.EmpId,
                            JobNum = detailLine.JobNum,
                            NewBin = detailLine.NewBin,
                            PartDesc = detailLine.PartDesc,
                            PartNum = detailLine.PartNum,
                            PrimeBin = detailLine.PrimeBin,
                            Qty = detailLine.Qty,
                            TroubleBit = detailLine.TroubleBit,
                            TypeCode = detailLine.TypeCode,
                            LineStatusId = 1,
                            StationNumber = rec.Location.Station.StationNumber
                        };
                        try
                        {
                            repoReplenOrderDetail.Insert(replenOrderDetail);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Insert Store Item Error: {ex.Message}");
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Build Store ReplenOrder, no Item Defintion");
                    }
                }
                else
                {
                    MessageBox.Show($"Build Store ReplenOrder, no Inventory Item");
                }

            }
        }

        private void MBSelectAllReturntoStockDetail_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in DataGridViewOrderDetails.Rows)
            {
                var chk = (DataGridViewCheckBoxCell)row.Cells[0];
                chk.Value = chk.TrueValue;
                if (row.IsNewRow)
                {
                    chk.Value = chk.FalseValue;
                }
            }
        }


        private List<int> GetCheckedOrderDetailIds()
        {

            var orderDetailIds = new List<int>();
            foreach (DataGridViewRow row in DataGridViewOrderDetails.Rows)
            {
                if (row.Cells["IsChecked"].Value != null && (bool)row.Cells["IsChecked"].Value == true)
                {
                    orderDetailIds.Add((int)row.Cells["Id"].Value);
                }
            }
            if (orderDetailIds.Count() == 0)
            {
                MessageBox.Show(text: "No Jobs Selected.");
            }
            return orderDetailIds;
        }

        private List<ReplenOrderDetail> GetCheckedOrderDetails()
        {
            var orderDetails = new List<ReplenOrderDetail>();
            foreach (DataGridViewRow row in DataGridViewOrderDetails.Rows)
            {
                if (row.Cells["IsChecked"].Value != null && (bool)row.Cells["IsChecked"].Value == true)
                {
                    int orderDetailId = row.Cells["OrderDetailId"].Value.ToString().ParseInt();
                    ReplenOrderDetail orderDetail = repoReplenOrderDetail.FindByKey(orderDetailId);
                    if (orderDetail != null)
                    {
                        orderDetails.Add(orderDetail);
                    }
                }
            }
            if (orderDetails.Count() == 0)
            {
                MessageBox.Show(text: "No Detail Lines Selected.");
            }
            return orderDetails;
        }


        private void CreateStoreOrderFromOrderDetailComplete(ReplenOrder order)
        {
            int lineStatusComplete = 6;
            IEnumerable<ReplenOrderDetail> detailLines = repoReplenOrderDetail.FindBy(r => r.ReplenOrderId == order.Id && r.LineStatusId == lineStatusComplete).ToList();
            if (detailLines.Count() > 0)
            {
                StoreOrderDetails(detailLines);
            }

        }

        // Internal order builder for items that have been picked and then
        // the order is returned to stock
        private void StoreOrderDetails(IEnumerable<ReplenOrderDetail> hostOrderLines)
        {
            if (hostOrderLines.Count() > 0)
            {
                ReplenOrderDetail firstRec = hostOrderLines.FirstOrDefault();
                if (firstRec != null)
                {
                    var replenOrder = new ReplenOrder()
                    {
                        Ord1 = firstRec.JobNum,
                        Ord2 = firstRec.EmpId,
                        Priority = GetTrayNumber(firstRec.PrimeBin),
                        LoadDate = System.DateTime.Now,
                        ShipperId = 1,
                        ShipMethodId = 1,
                        OrderStatusId = 1
                    };
                    try
                    {
                        repoReplenOrder.Insert(replenOrder);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Insert Replen ReplenOrder Error: {ex.Message}");
                    }

                    foreach (var hostOrder in hostOrderLines)
                    {
                        ItemDefinition storeItemDefinition = repoItemDefinition.FindBy(r => r.Item == hostOrder.PartNum).FirstOrDefault();
                        if (storeItemDefinition != null)
                        {
                            Inventory rec = repoInventory.FindBy(r => r.ItemDefinitionId == storeItemDefinition.Id && r.Location.Slot == hostOrder.PrimeBin).FirstOrDefault();
                            if (rec != null)
                            {
                                var replenOrderDetail = new ReplenOrderDetail()
                                {
                                    ReplenOrderId = replenOrder.Id,
                                    ItemDefinitionId = rec.ItemDefinitionId,
                                    Quantity = (hostOrder.Qty).ParseInt(),
                                    DateTime = hostOrder.DateTime,
                                    EmpId = hostOrder.EmpId,
                                    JobNum = hostOrder.JobNum,
                                    NewBin = hostOrder.NewBin,
                                    PartDesc = hostOrder.PartDesc,
                                    PartNum = hostOrder.PartNum,
                                    PrimeBin = hostOrder.PrimeBin,
                                    Qty = hostOrder.Qty,
                                    TroubleBit = hostOrder.TroubleBit,
                                    TypeCode = hostOrder.TypeCode,
                                    LineStatusId = 1,
                                    StationNumber = rec.Location.Station.StationNumber
                                };
                                try
                                {
                                    repoReplenOrderDetail.Insert(replenOrderDetail);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"Insert Store Item Error: {ex.Message}");
                                }
                            }
                            else
                            {
                                MessageBox.Show($"Build Store ReplenOrder, no Item Defintion");
                            }
                        }
                        else
                        {
                            MessageBox.Show($"Build Store ReplenOrder, no Inventory Item");
                        }
                    }
                }
            }
        }

        private void DataGridViewOrderDetails_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var chk = (DataGridViewCheckBoxCell)DataGridViewOrderDetails.Rows[e.RowIndex].Cells[0];
                if (chk.Value == chk.TrueValue)
                {
                    DataGridViewOrderDetails.Rows[e.RowIndex].Cells[0].Value = chk.FalseValue;
                    //int id = Convert.ToInt32(DataGridViewOrderDetails.Rows[e.RowIndex].Cells["Id"].Value);
                }
                else
                {
                    DataGridViewOrderDetails.Rows[e.RowIndex].Cells[0].Value = chk.TrueValue;
                    //int id = Convert.ToInt32(DataGridViewOrderDetails.Rows[e.RowIndex].Cells["Id"].Value);
                }
            }
        }

        private void MBReleaseDetail_Click(object sender, EventArgs e)
        {
            List<ReplenOrderDetail> orderDetails = GetCheckedOrderDetails();
            if (orderDetails.Count() > 0)
            {
                int orderId = orderDetails.First().ReplenOrderId;
                foreach (var item in orderDetails)
                {
                    item.LineStatusId = 1;
                    repoReplenOrderDetail.Update(item);
                    GlobalVar.HistoryManager.SaveHistory(ActionCode.ReleaseLine, item);
                }
                ShowOrderDetails(orderId);
            }
        }

        private void MBHoldDetail_Click(object sender, EventArgs e)
        {
            List<ReplenOrderDetail> orderDetails = GetCheckedOrderDetails();
            if (orderDetails.Count() > 0)
            {
                int orderId = orderDetails.First().ReplenOrderId;
                foreach (var item in orderDetails)
                {
                    item.LineStatusId = 2;
                    repoReplenOrderDetail.Update(item);
                    GlobalVar.HistoryManager.SaveHistory(ActionCode.HoldLine, item);
                }
                ShowOrderDetails(orderId);
            }
        }

        private void DataGridPickView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            //if (e.RowIndex < 0)
            //{
            //    return;
            //}
            //DataGridViewCell totalQuantityCell = DataGridPickView.Rows[e.RowIndex].Cells["TotalQuantityInInventory"];
            //DataGridViewCell quantityCell = DataGridPickView.Rows[e.RowIndex].Cells["Quantity"];

            //int totalValue = totalQuantityCell.Value == null ? 0 : totalQuantityCell.Value.ToString().ParseInt();
            //int quantityValue = quantityCell.Value == null ? 0 : quantityCell.Value.ToString().ParseInt();

            //if (quantityValue > totalValue)
            //{
            //    DataGridPickView.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Red;
            //}
            //else
            //{
            //    DataGridPickView.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
            //}
        }

        private void MBPickScreenHotPick_Click(object sender, EventArgs e)
        {
            if (_securityProcessor.SecurityProfile[(int)NeutronSecurity.HotActions])
            {
                var item = LabelPickItemNumber.Text;
                Hide();
                using (MetroForm frm = new FrmHotAction(_station, _jsonData, _akaRepository, _neutronVariables, _nomenclature, item))
                {
                    DialogResult result = frm.ShowDialog();
                    Show();
                    Task.Run(() => _deviceManager.Reset());
                    Task.Run(() => _logger.Log($"Reset After Hot Action : [{DateTime.Now.ToLongTimeString()}]"));
                }


                //_openHotPickFromPickScreen = true;
                //LabelFormTitle.Text = _resourceManager.GetString($"HotSearch");
                //LabelFormTitle.BackColor = Color.RoyalBlue;
                //TextBoxFindItem.Text = LabelPickItemNumber.Text;
                //tabControl1.SelectedTab = HotPick;
            }

            //openHotPickFromPickScreen = true;
            //LabelFormTitle.Text = "Hot Search";
            //LabelFormTitle.BackColor = Color.Green;
            //TextBoxFindItem.Text = LabelPickItemNumber.Text;
            //tabControl1.SelectedTab = HotPick;
        }

        private void MBArchive_Click(object sender, EventArgs e)
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
            CsvUtility.SaveToCsv(DataGridPickView);
        }

        private void MBPrintOrderDetails_Click(object sender, EventArgs e)
        {
            CsvUtility.SaveToCsv(DataGridViewOrderDetails);
        }

        private void MBBackHotStore_Click(object sender, EventArgs e)
        {
            if (openHotStoreFromPickScreen)
            {
                LabelFormTitle.Text = "Selection";
                LabelFormTitle.BackColor = Color.Green;
                tabControl1.SelectedTab = PickScreen;
                openHotStoreFromPickScreen = false;
            }
            else
            {
                LabelFormTitle.Text = "Jobs";
                LabelFormTitle.BackColor = Color.Green;
                tabControl1.SelectedTab = Main;
            }
        }

      private void TextBoxPos_Click(object sender, EventArgs e)
        {
            //if you click directly in a textboxpos, you override the
            //automatic get of the next empty textbox
            //to let the automatic process know to use the manually
            //clicked textbox, set the flag to true
            // unset the flag after the automatic check runs
            CurrentTextBoxPos = sender as TextBox;
            ClearTextBoxPosBackColor();
            CurrentTextBoxPos.BackColor = Color.Yellow;
            ManualOverrideCurrentTextBoxPos = true;
        }

        private void ClearTextBoxPosBackColor()
        {
            TextBoxPos1.BackColor = Color.White;
            TextBoxPos2.BackColor = Color.White;
            TextBoxPos3.BackColor = Color.White;
            TextBoxPos4.BackColor = Color.White;
            TextBoxPos5.BackColor = Color.White;
            TextBoxPos6.BackColor = Color.White;
            TextBoxPos7.BackColor = Color.White;
            TextBoxPos8.BackColor = Color.White;
        }

        private void MBFillStarters_Click(object sender, EventArgs e)
        {
            if (DataGridViewAvailableOrders.Rows.Count <= 0) return;
            foreach (DataGridViewRow row in DataGridViewAvailableOrders.Rows)
            {
                var checkBoxCell = (DataGridViewCheckBoxCell)row.Cells[0];
                var starterValue = row.Cells["Starter"].Value.ToString();
                if (starterValue != @"S" || Convert.ToBoolean(checkBoxCell.Value) != false) continue;
                var id = Convert.ToInt32(row.Cells["Id"].Value);
                var ord1 = Convert.ToString(row.Cells["Ord1"].Value);
                var ord2 = Convert.ToString(row.Cells["Ord2"].Value);
                var idx = AddItemToBatch(id, ord1, ord2);
                if (idx == -1)
                {
                    //no more locations
                    break;
                }
                else
                {
                    row.Cells[0].Value = checkBoxCell.TrueValue;
                }
            }
        }

        private void MBFill_Click(object sender, EventArgs e)
        {
            if (DataGridViewAvailableOrders.Rows.Count <= 0) return;
            foreach (DataGridViewRow row in DataGridViewAvailableOrders.Rows)
            {
                var checkBoxCell = (DataGridViewCheckBoxCell)row.Cells[0];

                if (Convert.ToBoolean(checkBoxCell.Value) != false) continue;
                var id = Convert.ToInt32(row.Cells["Id"].Value);
                var ord1 = Convert.ToString(row.Cells["Ord1"].Value);
                var ord2 = Convert.ToString(row.Cells["Ord2"].Value);
                var idx = AddItemToBatch(id, ord1, ord2);
                if (idx == -1)
                {
                    //no more locations
                    break;
                }
                else
                {
                    row.Cells[0].Value = checkBoxCell.TrueValue;
                }
            }
        }


        private void label4_Click(object sender, EventArgs e)
        {

        }

      
        private void FrmReplen_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F12)
            {
                // MessageBox.Show("Launch Find Box in PICK");

                using (MetroForm frm = new FrmInventory(_jsonData, _station, _akaRepository, _nomenclature))
                {
                    DialogResult result = frm.ShowDialog();
                    this.Show();
                }


            }
        }

        private void MBRackBack_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Jobs";
            LabelFormTitle.BackColor = Color.Green;
            tabControl1.SelectedTab = Main;
        }

        private void MBPrintDocument_Click(object sender, EventArgs e)
        {

        }

        private void MBPrintToteLabel_Click(object sender, EventArgs e)
        {
            List<ReplenOrder> orders = GetCheckedOrdersRack();
            if (orders.Count > 0)
            {
                foreach (var order in orders)
                {
                    PrintTote(positionNumber: 1, order: order);
                    PrintJob printJob = repoPrintJob.FindBy(r => r.OrderId == order.Id && r.ToteLabel == true).FirstOrDefault();
                    if (printJob == null)
                    {
                        printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, ToteLabel = true };
                        repoPrintJob.Insert(printJob);
                    }
                }
            }
        }

        private void MBOrderComplete_Click(object sender, EventArgs e)
        {
            List<ReplenOrder> orders = GetCheckedOrdersRack();
            if (orders.Count > 0)
            {
                foreach (ReplenOrder order in orders)
                {
                    //Mark OrderDetails and ReplenOrder Status to 6
                    foreach (var detail in order.ReplenOrderDetails)
                    {
                        detail.LineStatusId = 6;
                        detail.EmpId = GlobalVar.User.EmpId;
                        repoReplenOrderDetail.Update(detail);
                    }
                    order.OrderStatusId = 6;
                    repoReplenOrder.Update(order);
                }


                var uploadProcessor = new UploadProcessor(neutronLicense, _neutronVariables, _logger);
                Task.Run(() => uploadProcessor.CreateHostFileRack(orders));

                ShowAvailableOrders();
                TextBoxFindAvailableOrdersRack.Focus();
            }

        }

        private List<ReplenOrder> GetCheckedOrdersRack()
        {
            var orders = new List<ReplenOrder>();
            foreach (DataGridViewRow row in DataGridViewAvailableOrdersRack.Rows)
            {
                if (row.Cells["IsChecked"].Value != null && (bool)row.Cells["IsChecked"].Value == true)
                {
                    var ordId = (int)row.Cells["Id"].Value;
                    ReplenOrder order = repoReplenOrder.AllInclude(o => o.ReplenOrderDetails).Where(r => r.Id == ordId).FirstOrDefault();
                    if (order != null)
                    {
                        orders.Add(order);
                    }
                }
            }
            if (orders.Count() == 0)
            {
                MessageBox.Show(text: "No Jobs Selected.");
            }
            return orders;
        }

        private void MBRefreshRack_Click(object sender, EventArgs e)
        {
            ShowAvailableOrders();
            TextBoxFindAvailableOrdersRack.Focus();
        }

        private void MbPrintAvailableOrdersRack_Click(object sender, EventArgs e)
        {
            CsvUtility.SaveToCsv(DataGridViewAvailableOrdersRack);
            TextBoxFindAvailableOrdersRack.Focus();
        }

        private void MBPrintDocumentAndToteLabel_Click(object sender, EventArgs e)
        {
            List<ReplenOrder> orders = GetCheckedOrdersRack();
            if (orders.Count > 0)
            {
                foreach (var order in orders)
                {
                    PrintTote(positionNumber: 1, order: order);
                    PrintJob printJob = repoPrintJob.FindBy(r => r.OrderId == order.Id && r.ToteLabel == true).FirstOrDefault();
                    if (printJob == null)
                    {
                        printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, ToteLabel = true };
                        repoPrintJob.Insert(printJob);
                    }
                    PrintDoc(positionNumber: 1, order: order);
                    printJob = repoPrintJob.FindBy(r => r.OrderId == order.Id && r.PickDocument == true).FirstOrDefault();
                    if (printJob == null)
                    {

                        printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, PickDocument = true };
                        repoPrintJob.Insert(printJob);
                    }
                }
            }
            TextBoxFindAvailableOrdersRack.Focus();
        }

        private void tabControl1_Enter(object sender, EventArgs e)
        {
            TextBoxFindAvailableOrdersRack.Focus();
        }

        private void MBPrint_Click(object sender, EventArgs e)
        {
            using (FrmReprint form = new FrmReprint())
            {
                DialogResult result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    if (form.printData.PrintDocument)
                    {
                        ReprintDocument(form.printData.Position);
                    }

                    if (form.printData.PrintToteLabel)
                    {
                        ReprintToteLabel(form.printData.Position);
                    }
                }
            }
        }

        private void ReprintToteLabel(int batchPosition)
        {
            foreach (var bp in _ordersToPick)
            {
                if (bp.PositionNumber != batchPosition) continue;
                if (bp.OrderId == null) continue;
                var id = bp.OrderId.Value;
                var order = repoReplenOrder.FindByKey(id);
                var printJob = repoPrintJob.FindBy(r => r.OrderId == order.Id).FirstOrDefault();
                if (printJob == null)
                {
                    PrintTote(bp.PositionNumber, order);
                    printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, ToteLabel = true };
                    repoPrintJob.Insert(printJob);
                }
                else
                {
                    PrintTote(bp.PositionNumber, order);
                    printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, ToteLabel = true };
                    repoPrintJob.Update(printJob);
                }
            }
        }

        private void ReprintDocument(int batchPosition)
        {
            foreach (var bp in _ordersToPick)
            {
                if (bp.PositionNumber != batchPosition) continue;
                if (bp.OrderId == null) continue;
                var id = bp.OrderId.Value;
                var order = repoReplenOrder.FindByKey(id);
                var printJob = repoPrintJob.FindBy(r => r.OrderId == order.Id).FirstOrDefault();
                if (printJob == null)
                {
                    PrintDoc(bp.PositionNumber, order);
                    printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, PickDocument = true };
                    repoPrintJob.Insert(printJob);
                }
                else
                {
                    PrintDoc(bp.PositionNumber, order);
                    printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, PickDocument = true };
                    repoPrintJob.Update(printJob);
                }
            }
        }

        private void TextBoxFindAvailableOrdersRack_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                foreach (DataGridViewRow row in DataGridViewAvailableOrdersRack.Rows)
                {
                    if (row.Cells["Ord1"].Value.ToString().Trim() == TextBoxFindAvailableOrdersRack.Text)
                    {
                        var chk = (DataGridViewCheckBoxCell)row.Cells[0];
                        row.Cells[0].Value = chk.TrueValue;
                        TextBoxFindAvailableOrdersRack.Text = string.Empty;
                        TextBoxFindAvailableOrdersRack.Select();
                        break;
                    }
                }
            }
        }

        private void PictureBoxItemImage_MouseEnter(object sender, EventArgs e)
        {
            PictureBoxItemImage.Location = new Point(318, 117);
            PictureBoxItemImage.Size = new Size(512, 512);
            PictureBoxItemImage.BringToFront();
        }

        private void PictureBoxItemImage_MouseLeave(object sender, EventArgs e)
        {
            PictureBoxItemImage.Location = new Point(446, 373);
            PictureBoxItemImage.Size = new Size(256, 256);
            PictureBoxItemImage.BringToFront();
        }

        private void MBDeleteOrder_Click(object sender, EventArgs e)
        {
            var recs = GetCheckedOrderIds();
            if (recs.Any())
            {
                foreach (var id in recs)
                {
                    var ord = repoReplenOrder.AllInclude(s => s.ReplenOrderDetails).FirstOrDefault(r => r.Id == id);

                    if (ord == null) continue;
                    foreach (var orderDetail in ord.ReplenOrderDetails)
                    {
                        repoReplenOrderDetail.Delete(orderDetail.Id);
                        GlobalVar.HistoryManager.SaveHistory(ActionCode.ReplenDetailDelete, orderDetail);
                    }
                    repoReplenOrder.Delete(ord.Id);
                    GlobalVar.HistoryManager.SaveHistory(ActionCode.ReplenOrderDelete, ord);
                }
            }
            ShowAllOrders();
        }

        private void SetCulture(string lang)
        {
            try
            {
                var languageDirectory = LoaderSettings.GetLanguageDirectory();
                _cultureInfo = CultureInfo.CreateSpecificCulture(lang);
                _resourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "FrmReplen", resourceDir: languageDirectory, usingResourceSet: null);
                MBMainOrderManager.Text = _resourceManager.GetString($"JobManager");
                MBMainAvailableOrders.Text = _resourceManager.GetString($"AvailableJobs");
                LabelFormHeaderText.Text = _resourceManager.GetString($"NeutronWarehouseManagement");
                MBMainNewOrder.Text = _resourceManager.GetString($"NewJob");
                MBMainLoadOrders.Text = _resourceManager.GetString($"LoadJobs");
                MBMainClose.Text = _resourceManager.GetString($"Home");
                LabelFormTitle.Text = _resourceManager.GetString($"Jobs");
                MBSelectAll.Text = _resourceManager.GetString($"SelectAll");
                MButtonClearSelection.Text = _resourceManager.GetString($"ClearSelection");
                MBOrderListingAvailable.Text = _resourceManager.GetString($"Available");
                MButtonSearch.Text = _resourceManager.GetString($"Search");
                MButtonClose.Text = _resourceManager.GetString($"Back");
                MBShowAll.Text = _resourceManager.GetString($"ShowAll");
                MBRefresh.Text = _resourceManager.GetString($"Refresh");
                MBHold.Text = _resourceManager.GetString($"Hold");
                MBRelease.Text = _resourceManager.GetString($"Release");
                MBPriority.Text = _resourceManager.GetString($"Priority");
                MBReturnToStock.Text = _resourceManager.GetString($"ReturnToStock");
                MBDeleteOrder.Text = _resourceManager.GetString($"Delete");
                MBPrintOrderListing.Text = _resourceManager.GetString($"SaveToFile");
                MBJobDetails.Text = _resourceManager.GetString($"JobDetails");
                LabelFindDescription.Text = _resourceManager.GetString($"SearchFor");
                //Available Orders
                MbPrintAvailableOrders.Text = _resourceManager.GetString($"SaveToFile");
                MBAvailableOrdersRefresh.Text = _resourceManager.GetString($"Refresh");
                MBGo.Text = _resourceManager.GetString($"Next");
                LabelAvailableOrdersSearchFor.Text = _resourceManager.GetString($"SearchFor");
                MBSearchAvailableOrders.Text = _resourceManager.GetString($"Search");
                MBFill.Text = _resourceManager.GetString($"Fill");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading languages.  FrmReplen  {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
        }

        private void MBShowAll_Click(object sender, EventArgs e)
        {
            ShowAllOrders();
            MBCompleted.Text = "Completed";
        }
    }
}
