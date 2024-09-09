using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlliedLogger;
using JsonManager;
using MetroFramework.Forms;
using Neutron.Enums;
using Neutron.Global;
using NeutronData.DataContexts;
using NeutronData.ModelViews;
using NeutronData.Models;
using NeutronData.Models.Lookups;
using NeutronData.Repositories;
using NeutronData.SqlModelViews;
using Neutron.Classes;
using Equin.ApplicationFramework;
using EnumsNET;
using NeutronCore.Extensions;
using Neutron.Interfaces;
using Neutron.Models;
using NeutronCore.Global;
using NeutronCore;
using NeutronData.Interfaces;
using NeutronCore.Enums;
using StationType = NeutronCore.Enums.StationType;
using StorageType = Neutron.Enums.StorageType;
using ExcelManager;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using AsyncAwaitBestPractices;
using NeutronEvents;
using IPTI.Models;



namespace Neutron.Forms
{
    public partial class FrmInventory : MetroForm
    {
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;
        private ResourceManager _gridResourceManager;

        private readonly BindingSource _bindingSource = new BindingSource();
        private readonly BindingSource _locationBindingSource = new BindingSource();
        private readonly BindingSource _newLocationBindingSource = new BindingSource();
        private readonly GenericRepository<ItemDefinition> _repoItemDefinition =
            new GenericRepository<ItemDefinition>(new NeutronDb());
        private readonly GenericRepository<Location> _repoLocation = new GenericRepository<Location>(new NeutronDb());
        private readonly GenericRepository<SizeCode> _repoSizeCode = new GenericRepository<SizeCode>(new NeutronDb());
        private readonly GenericRepository<VelocityCode> _repoVelocityCode =
            new GenericRepository<VelocityCode>(new NeutronDb());
        private readonly GenericRepository<HeightCode> _repoHeightCode =
            new GenericRepository<HeightCode>(new NeutronDb());
        private readonly GenericRepository<Inventory> _repoInventory = new GenericRepository<Inventory>(new NeutronDb());
        private readonly GenericRepository<NeutronData.Models.Lookups.StorageType> _repoStorageType =
            new GenericRepository<NeutronData.Models.Lookups.StorageType>(new NeutronDb());
        private readonly GenericRepository<UnitOfIssue> _repoUnitOfIssue =
            new GenericRepository<UnitOfIssue>(new NeutronDb());
        private readonly GenericRepository<Area> _repoArea =
            new GenericRepository<Area>(new NeutronDb());

        private readonly IWorkstationRepository _workstationRepository;

        private readonly ILocationsRepository _locationsRepository;

        private InventoryRepository _inventoryRepository;
        private readonly IJsonData _jsonData;
        private readonly NeutronVariables _neutronVariables;
        private readonly IAreaRepository _areaRepository;
        private readonly IRFIDManager _rfidManager;
        private readonly WorkstationView _workstationView;
        private readonly IAkaRepository _akaRepository;
        private readonly ILacProcessor _lacProcessor;
        private bool _allAvailable;
        public ItemDefinition CurrentItem { get; set; }
        public Location CurrentLocation { get; set; }
        public Inventory CurrentInventoryItem { get; set; }
        public bool CloseButtonPressed { get; set; }
        private bool _firstTime = true;
        private bool _firstTimeCombo = true;
        private bool _processing = false;

        private IDynamicLogger _logger;
        private HeaderTextManager _headerTextManager;
        private List<SqlInventoryView> _currentList;
        private bool _startup = true;
        private IIptiDisplayFunctions _iptiDisplayFunctions;
        private const int AreaEight = 8;

        public FrmInventory(IJsonData jsonData, IAkaRepository akaRepository,
            ILacProcessor lacProcessor, IWorkstationRepository workstationRepository
            , WorkstationView workstationView, NeutronVariables neutronVariables
            , IAreaRepository areaRepository
            , IRFIDManager rfidManager, ILocationsRepository locationsRepository
            , IIptiDisplayFunctions iptiDisplayFunctions)
        {
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);

            _jsonData = jsonData;
            _akaRepository = akaRepository;
            _lacProcessor = lacProcessor;
            _workstationRepository = workstationRepository;
            _workstationView = workstationView;
            _neutronVariables = neutronVariables;
            _areaRepository = areaRepository;
            _rfidManager = rfidManager;
            _locationsRepository = locationsRepository;
            _iptiDisplayFunctions = iptiDisplayFunctions;
            KeyPreview = true;
            CloseButtonPressed = false;


            Init().SafeFireAndForget();
        }

        private async Task Init()
        {
            LabelStationName.Text = _workstationView.ToString();
            _headerTextManager = new HeaderTextManager();

            HideTabControlTabs();
            SetupNewForm();
            SetupViewEditForm();
            SetupAddDetailForm();
            mlUserInfo.Text = GlobalVar.User?.UserInfo;
            _logger = NeutronCore.Global.Logger.SetupLogger("Inventory");
            _inventoryRepository = new InventoryRepository(_logger);

            LabelInventoryNewLocationsRfid.Visible = _neutronVariables.RfidEnabledInventory;
            TextBoxInventoryNewLocationsRfid.Visible = _neutronVariables.RfidEnabledInventory;
            LabelAddDetailRfid.Visible = _neutronVariables.RfidEnabledInventory;
            TextBoxAddDetailRfid.Visible = _neutronVariables.RfidEnabledInventory;

            var areas = _repoArea.All();
            ComboBoxAreaNumber.DataSource = areas;
            ComboBoxAreaNumber.ValueMember = "Id";
            ComboBoxAreaNumber.DisplayMember = "Name";
            ComboBoxAreaNumber.SelectedIndex = 0;

            if (_workstationView.StationType.Id == (int)NeutronCore.Enums.StationType.Supervisor)
            {
                ComboBoxAreaNumber.SelectedIndex = ComboBoxAreaNumber.FindStringExact("All Areas");
            }
            else
            {
                ComboBoxAreaNumber.SelectedValue = _workstationView.AreaId;
            }
            SetupGrids();
            _firstTime = false;
            //Mediator.GetInstance().InventoryFileCreated += (s, e) => MessageBox.Show("Inventory File Created."
            //    , "Inventory File", MessageBoxButtons.OK,MessageBoxIcon.Information,MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            _startup = false;

            await RefreshData();

        }

        /// <summary>
        /// CreateParams
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                var parms = base.CreateParams;
                parms.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                //parms.Style &= ~0x02000000;  // Turn off WS_CLIPCHILDREN
                return parms;
            }
        }

        /// <summary>
        /// Loads the inventory by AreaId or All if it is a Supervisor Station
        /// If the Find textbox has any value, it will search for it
        /// The process looks in the AKA file first
        /// If a Record id is passed is the focus is on that record
        /// </summary>
        /// <param name="recId">Record Id to set the focus</param>
        //private async Task RefreshDataz(int recId = 0)
        //{
        //    // Cursor.Current = Cursors.WaitCursor;
        //    var aka = string.Empty;
        //    var findWhat = TextBoxFind.Text.ToLower().Trim();
        //    try
        //    {

        //        // If the findWhat is not empty, then look in the AKA file first
        //        if (!string.IsNullOrEmpty(findWhat))
        //        {
        //            aka = await Task.Run(() => _akaRepository.Get(findWhat));
        //        }

        //        var find = string.IsNullOrWhiteSpace(aka) ? findWhat : aka;
        //        TextBoxFind.Text = find;

        //        if (_workstationView != null)
        //        {
        //            var area = (Area)ComboBoxAreaNumber.SelectedItem;
        //            var views = area.Name == "All Areas"
        //                ? _inventoryRepository.FindInventoryViews(find).ToList()
        //                : _inventoryRepository.FindInventoryViewsByArea(find, area.Id).ToList();

        //            _currentList = views;
        //            var localViews = new List<SqlInventoryView>(views);

        //            var blv = new BindingListView<SqlInventoryView>(localViews.ToList());
        //            _logger.LogDetailAsync($"Inventory Count: {blv.Count}").SafeFireAndForget();
        //            _bindingSource.DataSource = blv;
        //            DataGridView1.DataSource = _bindingSource;

        //            _logger.LogDetailAsync($"Inventory Count: Grid Done").SafeFireAndForget();
        //            if (GetRecordCount(_bindingSource) > 0)
        //            {
        //                if (recId != 0)
        //                {
        //                    var idx = IndexOf(_bindingSource, recId);
        //                    DataGridView1.FirstDisplayedScrollingRowIndex = DataGridView1.Rows[idx].Index;
        //                    DataGridView1.CurrentCell = DataGridView1.Rows[idx].Cells[1];
        //                    DataGridView1.Rows[idx].Selected = true;
        //                }
        //                SetCurrentInventoryItem();
        //                DataGridView1.Refresh();
        //                DataGridView1.ClearSelection();
        //                if (DataGridView1.RowCount > 0) DataGridView1.FastAutoSizeColumns();
        //            }
        //            else
        //            {
        //                CurrentItem = null;
        //                CurrentLocation = null;
        //                CurrentInventoryItem = null;
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        var message = $"Error Loading Data: {Environment.NewLine}{ex.Message}";
        //        _logger.LogDetailAsync(message).SafeFireAndForget();
        //        Mediator.GetInstance().OnGeneralError(this, message);
        //    }
        //    finally
        //    {
        //        //  Cursor.Current = Cursors.Default;
        //    }
        //}

        private async Task RefreshData(int recId = 0)
        {
            var findWhat = TextBoxFind.Text.ToLower().Trim();
            var aka = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(findWhat))
                {
                    aka = await GetAkaValue(findWhat);
                }

                var find = string.IsNullOrWhiteSpace(aka) ? findWhat : aka;
                TextBoxFind.Text = find;
                await LoadData(find, recId);
            }
            catch (Exception ex)
            {
                var message = $"Error Loading Data: {Environment.NewLine}{ex.Message}";
                _logger.LogDetailAsync(message).SafeFireAndForget();
                Mediator.GetInstance().OnGeneralError(this, message);
            }
        }

        private async Task LoadData(string find, int recId)
        {
            try
            {
                var area = (Area)ComboBoxAreaNumber.SelectedItem;
                var views = area.Name == "All Areas"
                    ? _inventoryRepository.FindInventoryViews(find).ToList()
                    : _inventoryRepository.FindInventoryViewsByArea(find, area.Id).ToList();

                var localViews = new List<SqlInventoryView>(views);
                _currentList = localViews;
                var blv = new BindingListView<SqlInventoryView>(localViews.ToList());
                _logger.LogDetailAsync($"Inventory Count: {blv.Count}").SafeFireAndForget();
                _bindingSource.DataSource = blv;
                DataGridView1.DataSource = _bindingSource;
                _logger.LogDetailAsync($"Inventory Count: Grid Done").SafeFireAndForget();
                if (GetRecordCount(_bindingSource) > 0)
                {
                    if (recId != 0)
                    {
                        var idx = IndexOf(_bindingSource, recId);
                        DataGridView1.FirstDisplayedScrollingRowIndex = DataGridView1.Rows[idx].Index;
                        DataGridView1.CurrentCell = DataGridView1.Rows[idx].Cells[1];
                        DataGridView1.Rows[idx].Selected = true;
                    }
                    await SetCurrentInventoryItem();
                    DataGridView1.Refresh();
                    DataGridView1.ClearSelection();
                    if (DataGridView1.RowCount > 0) DataGridView1.FastAutoSizeColumns();
                }
                else
                {
                    CurrentItem = null;
                    CurrentLocation = null;
                    CurrentInventoryItem = null;
                }
            }
            catch (Exception ex)
            {
                var message = $"Error Loading Data: {Environment.NewLine}{ex.Message}";
                _logger.LogDetailAsync(message).SafeFireAndForget();
                Mediator.GetInstance().OnGeneralError(this, message);
            }
        }

        private async Task<string> GetAkaValue(string findWhat)
        {
            if (!string.IsNullOrEmpty(findWhat))
            {
                return await Task.Run(() => _akaRepository.Get(findWhat));
            }
            return string.Empty;
        }

        /// <summary>
        /// Sets the Current Inventory Item, Location and Item Definition
        /// </summary>
        private async Task SetCurrentInventoryItem()
        {
            if (_bindingSource.Current == null) return;
            var inventory = ((ObjectView<SqlInventoryView>)_bindingSource.Current).Object;
            var inv = await _repoInventory.FindByKeyAsync(inventory.Id);
            await SetCurrentInventoryItem(inv);
        }
        private async Task SetCurrentInventoryItem(Inventory inventory)
        {
            CurrentInventoryItem = inventory;
            await SetCurrentLocation(inventory.LocationId);
            await SetCurrentItemDefinition(inventory.ItemDefinitionId);
        }

        /// <summary>
        /// Sets the Current Item Definition
        /// </summary>
        /// <param name="itemDefinitionId">Item Definition Id</param>
        private async Task SetCurrentItemDefinition(int itemDefinitionId)
        {
            var itemDefinition = await _repoItemDefinition.FindByKeyAsync(itemDefinitionId);
            CurrentItem = itemDefinition;
        }

        /// <summary>
        /// Sets the Current Location
        /// </summary>
        /// <param name="locationId">Location Id</param>
        private async Task SetCurrentLocation(int locationId)
        {
            var location = await _repoLocation.FindByKeyAsync(locationId);
            CurrentLocation = location;
        }

        public int IndexOf(BindingSource bs, int id)
        {
            var count = bs.Count;
            var itemIndex = -1;
            for (var i = 0; i < count; i++)
            {
                var rec = ((SqlInventoryView)bs[i]).Id;
                if (rec == id)
                {
                    itemIndex = i;
                    break;
                }
            }
            return itemIndex;
        }

        /// <summary>
        /// Display the record count
        /// </summary>
        /// <param name="bs"></param>
        /// <returns></returns>
        private int GetRecordCount(BindingSource bs)
        {
            var count = bs.Count;
            LabelRecordCount.Text = $"{_resourceManager.GetString("Records")}: {count}";
            LabelAvailableLocations.Text = $"{_resourceManager.GetString("Records")}: {count}";
            return count;
        }

        #region Find Functions
        private async void MButtonFind_Click(object sender, EventArgs e)
        {
           await FindRecord();
        }
        private async Task FindRecord()
        {
            Cursor.Current = Cursors.WaitCursor;
            await RefreshData();
            Cursor.Current = Cursors.Default;
        }
        private async void TextBoxFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                await FindRecord();
         //              Cursor.Current = Cursors.WaitCursor;
         //await RefreshData();
         //       Cursor.Current = Cursors.Default;
            }
            if (e.KeyCode == Keys.Escape)
            {
                TextBoxFind.Text = "";
            }
        }
        #endregion
        #region Button Clicks
        private async void ButtonClear_Click(object sender, EventArgs e)
        {
            TextBoxFind.Text = string.Empty;
            Cursor.Current = Cursors.WaitCursor;
            await RefreshData();
            Cursor.Current = Cursors.Default;
            TextBoxFind.Focus();
        }
        /// <summary>
        /// Handles the Click event of the MButtonClose control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// This method performs the following actions:
        /// - If the ProLiteManager of the workstation view is not null, it clears all Prolites.
        /// - If the iptiDisplayFunctions is not null, it clears the Blastzone.
        /// - Sets the CloseButtonPressed flag to true.
        /// - Closes the form.
        /// Any exceptions that occur during these operations are logged.
        /// </remarks>
        private async void MButtonClose_Click(object sender, EventArgs e)
        {
            try
            {
                if (_workstationView.ProLiteManager != null)
                {
                    await _workstationView.ProLiteManager.ClearAllProlites();
                }
                if (_iptiDisplayFunctions != null)
                {
                    await _iptiDisplayFunctions.ClearBlastzone();
                }
                CloseButtonPressed = true;
                Close();
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Error during Button Close Event. {ex.Message}").SafeFireAndForget();
            }
        }
        private async void MButtonViewEdit_Click(object sender, EventArgs e)
        {
            if (DataGridView1.RowCount <= 0) return;
            UpdateViewEditForm();
            var recs = await GetInventoryViewListByItem(CurrentItem.Id);
            UpdateDataGridViewInventoryLocations(recs);
            tabControl1.SelectedTab = tabPage2;

        }

        private void UpdateDataGridViewInventoryLocations(List<SqlInventoryView> recs)
        {
            var blv = new BindingListView<SqlInventoryView>(recs.ToList());
            _locationBindingSource.DataSource = blv;
            DataGridViewInventoryLocations.DataSource = _locationBindingSource;
            DataGridViewInventoryLocations.ClearSelection();
        }

        private void UpdateViewEditForm()
        {
            if (CurrentItem != null)
            {
                TextBoxViewEditId.Text = CurrentItem.Id.ToString();
                TextBoxViewEditItem.Text = CurrentItem.Item;
                TextBoxViewEditDescription.Text = CurrentItem.Description;
                //var views = await GetInventoryViewListByItem(CurrentItem.Id);
                //var blv = new BindingListView<SqlInventoryView>(views.ToList());
                //_locationBindingSource.DataSource = blv;
                //DataGridViewInventoryLocations.DataSource = _locationBindingSource;
                //tabControl1.SelectedTab = tabPage2;
                //DataGridViewInventoryLocations.ClearSelection();
            }
        }
        private async Task LoadViewEdit()
        {
            try
            {

                await SetCurrentInventoryItem();
                if (CurrentItem != null)
                {
                    TextBoxViewEditId.Text = CurrentItem.Id.ToString();
                    TextBoxViewEditItem.Text = CurrentItem.Item;
                    TextBoxViewEditDescription.Text = CurrentItem.Description;
                    var views = await GetInventoryViewListByItem(CurrentItem.Id);
                    var localViews = new List<SqlInventoryView>(views);
                    var blv = new BindingListView<SqlInventoryView>(localViews.ToList());
                    _locationBindingSource.DataSource = blv;
                    DataGridViewInventoryLocations.DataSource = _locationBindingSource;
                    tabControl1.SelectedTab = tabPage2;
                    DataGridViewInventoryLocations.ClearSelection();
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Error during Load View Edit. {ex.Message}").SafeFireAndForget();
            }
        }
        private async void MButtonNew_Click(object sender, EventArgs e)
        {
           await NewInventoryItem();
        }
        private async Task NewInventoryItem()
        {
            ClearNewFields();
            var inventoryView = GetSelectedInventoryView(DataGridView1);
            if (inventoryView != null)
            {
                TextBoxNewItem.Text = inventoryView.Item;
                await NewItemFind();
            }

            tabControl1.SelectedTab = tabPage3;
        }
        private async void MbViewEditListing_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            await RefreshData();
            Cursor.Current = Cursors.Default;
            tabControl1.SelectedTab = tabPage1;
        }
        private void MbViewEditNew_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage3;
        }
        private async void MbNewFind_Click(object sender, EventArgs e)
        {
            await NewItemFind();
        }
        private async void MbViewEditClose_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            await RefreshData();
            //  ClearAllShi();

            if (_workstationView.ProLiteManager != null)
            {
                await _workstationView.ProLiteManager.ClearAllProlites();
            }

            if (_iptiDisplayFunctions != null)
            {
                await _iptiDisplayFunctions.ClearBlastzone();
            }


            Cursor.Current = Cursors.Default;
            tabControl1.SelectedTab = tabPage1;
        }
        private async void MbViewEditSave_Click(object sender, EventArgs e)
        {
            await UpdateViewEdit();
        }
        private void MbViewEditEdit_Click(object sender, EventArgs e)
        {
            EditLocation();
        }
        private void EditLocation()
        {
            var inventoryView = ((ObjectView<SqlInventoryView>)_locationBindingSource.Current).Object;
            TextBoxAddDetailItem.Text = inventoryView.Item;
            TextBoxAddDetailDescription.Text = inventoryView.Description;
            DateTimePickerAddDetailReceivedDate.Value = inventoryView.ReceivedDate;
            TextBoxAddDetailInventoryId.Text = inventoryView.Id.ToString();
            TextBoxAddDetailLocationId.Text = inventoryView.LocationId.ToString();
            TextBoxAddDetailQuantity.Text = inventoryView.Quantity.ToString();
            TextBoxAddDetailItemDefinitionId.Text = inventoryView.ItemDefinitionId.ToString();
            ComboBoxAddDetailArea.SelectedValue = inventoryView.AreaId;
            CheckBoxAddDetailPrimeBin.Checked = inventoryView.PrimeBin;
            TextBoxAddDetailLoc1.Text = inventoryView.Loc1.ToString();
            TextBoxAddDetailLoc2.Text = inventoryView.Loc2.ToString();
            TextBoxAddDetailLoc3.Text = inventoryView.Loc3.ToString();
            TextBoxAddDetailLoc4.Text = inventoryView.Loc4.ToString();
            TextBoxAddDetailLoc5.Text = inventoryView.Loc5.ToString();
            TextBoxAddDetailSlot.Text = inventoryView.Slot.ToString();
            TextBoxPickSequence.Text = inventoryView.PickSequence.ToString();
            ComboBoxAddDetailStorageType.SelectedValue = inventoryView.StorageTypeId;
            ComboBoxAddDetailSizeCode.SelectedValue = inventoryView.SizeCodeId;
            ComboBoxAddDetailVelocityCode.SelectedValue = inventoryView.VelocityCodeId;
            ComboBoxAddDetailHeightCode.SelectedValue = inventoryView.HeightCodeId;
            TextBoxAddDetailLocationCode.Text = inventoryView.LocationCode;
            CheckBoxInUse.Checked = inventoryView.InUse;
            LabelActionAddDetail.Text = _resourceManager.GetString("EditDetail");
            TextBoxAddDetailQuantity.Focus();
            ButtonPositionDevice.Visible = WorkstationCanPositionDevice();
            tabControl1.SelectedTab = tabPage5;
        }

        /// <summary>
        /// If a device is a carousel, vertical or blastzone then it can be positioned
        /// so return true
        /// </summary>
        /// <returns></returns>
        private bool WorkstationCanPositionDevice()
        {
            //return _workstationView.AreaId != AreaEight && _workstationView.AreaId == ((Area)ComboBoxAreaNumber.SelectedItem).AreaNumber;
            var moveableAreas = new int[] { 1, 2, 3, 4 };
            // return _workstationView.AreaId != 8 && _workstationView.AreaId == ((Area)ComboBoxAreaNumber.SelectedItem).AreaNumber;
            return moveableAreas.Contains(_workstationView.AreaId);
        }
        private async void MbNewListing_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            await RefreshData();
            Cursor.Current = Cursors.Default;
            tabControl1.SelectedTab = tabPage1;
        }
        private void MbNewViewEdit_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage2;
        }
        private async void MbNewSave_Click(object sender, EventArgs e)
        {
            await SaveNew();
        }
        private async void MbNewClose_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            await RefreshData();
            Cursor.Current = Cursors.Default;
            tabControl1.SelectedTab = tabPage1;
        }
        #endregion
        private async Task SaveNew()
        {
            try
            {
                var rec = new Inventory
                {
                    ItemDefinitionId =
                        (((ObjectView<SqlInventoryView>)_bindingSource.Current).Object).ItemDefinitionId,
                };
                if (!ValidateFields(rec))
                {
                    MessageBox.Show(_resourceManager.GetString("Message0"));
                    return;
                }
                if (await IsDuplicate(rec))
                {
                    MessageBox.Show(_resourceManager.GetString("Message1"));
                    return;
                }
                await _repoInventory.InsertAsync(rec);

                await GlobalVar.HistoryManager.SaveHistoryAsync(ActionCode.InventoryAdd, rec);

                await RefreshData(rec.Id);

                tabControl1.SelectedTab = tabPage1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(_resourceManager.GetString("Message3") + ex.Message + ex.InnerException + ex.InnerException.InnerException);
            }
        }
        private async Task UpdateViewEdit()
        {
            var currentInventoryView = (((ObjectView<SqlInventoryView>)_bindingSource.Current).Object);
            // convert SqlInventoryView to Inventory
            var inventory = await _repoInventory.FindByKeyAsync(currentInventoryView.Id);
            if (inventory == null) return;

            await GlobalVar.HistoryManager.SaveHistoryAsync(ActionCode.InventoryModify, inventory, inventory.Quantity, true);

            var beginningQuantity = 0;
            var rec = new Inventory
            {
                Id = currentInventoryView.Id,
                LocationId = currentInventoryView.LocationId,
                ItemDefinitionId = currentInventoryView.ItemDefinitionId,
            };
            if (!ValidateFields(rec))
            {
                MessageBox.Show(_resourceManager.GetString("Message4"));
                return;
            }
            await _repoInventory.UpdateAsync(rec);
            await GlobalVar.HistoryManager.SaveHistoryAsync(ActionCode.InventoryModify, rec, beginningQuantity, true);
            await RefreshData(rec.Id);
            tabControl1.SelectedTab = tabPage1;
        }
        private bool ValidateFields(Inventory rec)
        {
            //if (!IntegerValidator(rec.Loc1))
            //{
            //    return false;
            //}
            //if (!IntegerValidator(rec.Loc2))
            //{
            //    return false;
            //}
            //if (!IntegerValidator(rec.Loc3))
            //{
            //    return false;
            //}
            //if (!IntegerValidator(rec.Loc4))
            //{
            //    return false;
            //}
            //if (!IntegerValidator(rec.Loc5))
            //{
            //    return false;
            //}
            return true;
        }
        private bool StringValidator(string input)
        {
            var pattern = "[^a-zA-Z]";
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
            var pattern = "^[0-9]+$";
            if (Regex.IsMatch(input.ToString(), pattern))
            {
                if (input <= 0)
                {
                    MessageBox.Show(_resourceManager.GetString("Message4"));
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        private async Task<bool> IsDuplicate(Inventory recIn)
        {
            var rec = await _repoInventory
                .FindByFirstOrDefaultAsync(f => f.LocationId == recIn.LocationId && f.ItemDefinitionId == recIn.ItemDefinitionId);
            if (rec == null) return false;
            MessageBox.Show(_resourceManager.GetString("Message5"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return true;
        }
        #region Form Setup Grids
        private void SetupGrids()
        {
            DataGridView1.AutoGenerateColumns = false;
            DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridView1.AllowUserToAddRows = false;
            var position = _gridResourceManager.GetString("Position");
            var bCol = new DataGridViewButtonColumn
            {
                HeaderText = _gridResourceManager.GetString(""),
                Visible = _workstationView.AreaId != 8 && _workstationView.AreaId == (int)ComboBoxAreaNumber.SelectedValue,
                // Visible = _workstationView.StationTypeId != (int)StationType.RackTablet && _workstationView.StationTypeId != (int)StationType.Supervisor,
                Name = "Position",
                Text = position,
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                UseColumnTextForButtonValue = true
            };
            DataGridView1.Columns.Add(bCol);
            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "AreaName",
                HeaderText = _gridResourceManager.GetString("Area"),
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "AreaName"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = _gridResourceManager.GetString("Item"),
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Item"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = _gridResourceManager.GetString("Description"),
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Description"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = _gridResourceManager.GetString("Quantity"),
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Quantity"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Slot",
                HeaderText = _gridResourceManager.GetString("Slot"),
                Visible = true,
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Slot"
            };
            DataGridView1.Columns.Add(col);
            var xcol = new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "PrimeBin",
                HeaderText = _gridResourceManager.GetString("PrimeBin"),
                Visible = true,
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "PrimeBin"
            };
            DataGridView1.Columns.Add(xcol);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc1",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc1", _gridResourceManager),
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc1"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc2",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc2", _gridResourceManager),
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc2"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc3",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc3", _gridResourceManager),
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc3"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc4",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc4", _gridResourceManager),
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc4"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc5",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc5", _gridResourceManager),
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc5"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SizeCodeName",
                HeaderText = _gridResourceManager.GetString("SizeCodeName"),
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "SizeCodeName"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "VelocityCodeName",
                HeaderText = _gridResourceManager.GetString("VelocityCodeName"),
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "VelocityCodeName"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "HeightCodeName",
                HeaderText = _gridResourceManager.GetString("HeightCodeName"),
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "HeightCodeName"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LocationCode",
                HeaderText = _gridResourceManager.GetString("LocationCode"),
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "LocationCode"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StorageTypeName",
                HeaderText = _gridResourceManager.GetString("StorageTypeName"),
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "StorageTypeName"
            };
            DataGridView1.Columns.Add(col);


            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ReceivedDate",
                HeaderText = _gridResourceManager.GetString("ReceivedDate"),
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "ReceivedDate"
            };
            DataGridView1.Columns.Add(col);


            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PickSequence",
                HeaderText = $"Pick Sequence",  // _gridResourceManager.GetString("Slot"),
                Visible = true,
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "PickSequence"
            };
            DataGridView1.Columns.Add(col);


            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = _gridResourceManager.GetString("Id"),
                Visible = false,
                Name = "Id"
            };
            DataGridView1.Columns.Add(col);

            DataGridView1.EnableHeadersVisualStyles = false;
            DataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            DataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);

            //foreach (DataGridViewColumn column in DataGridView1.Columns)
            //{
            //    column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //    column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);
            //}
            //Location Grid
            DataGridViewInventoryLocations.AutoGenerateColumns = false;
            DataGridViewInventoryLocations.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewInventoryLocations.AllowUserToAddRows = false;
            bCol = new DataGridViewButtonColumn
            {
                //HeaderText = _gridResourceManager.GetString("Id"),
                Visible = _workstationView.StationTypeId != (int)StationType.RackTablet && _workstationView.StationTypeId != (int)StationType.Supervisor,
                Name = "Position",
                Text = position,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                UseColumnTextForButtonValue = true
            };
            DataGridViewInventoryLocations.Columns.Add(bCol);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "AreaName",
                HeaderText = _gridResourceManager.GetString("Area"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "AreaName"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = _gridResourceManager.GetString("Quantity"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Quantity"
            };
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Slot",
                HeaderText = _gridResourceManager.GetString("Slot"),
                Visible = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Slot"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            var ckcol = new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "PrimeBin",
                HeaderText = _gridResourceManager.GetString("PrimeBin"),
                Visible = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "PrimeBin"
            };
            DataGridViewInventoryLocations.Columns.Add(ckcol);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc1",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc1", _gridResourceManager),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc1"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc2",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc2", _gridResourceManager),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc2"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc3",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc3", _gridResourceManager),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc3"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc4",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc4", _gridResourceManager),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc4"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc5",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc5", _gridResourceManager),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc5"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SizeCodeName",
                HeaderText = _gridResourceManager.GetString("SizeCodeName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "SizeCodeName"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "VelocityCodeName",
                HeaderText = _gridResourceManager.GetString("VelocityCodeName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "VelocityCodeName"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "HeightCodeName",
                HeaderText = _gridResourceManager.GetString("HeightCodeName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "HeightCodeName"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PickSequence",
                HeaderText = $"Pick Sequence",  // _gridResourceManager.GetString("Slot"),
                Visible = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "PickSequence"
            };
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StorageTypeName",
                HeaderText = _gridResourceManager.GetString("StorageTypeName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "StorageTypeName"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LocationCode",
                HeaderText = _gridResourceManager.GetString("LocationCode"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "LocationCode"
            };
            DataGridViewInventoryLocations.Columns.Add(col);




            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ReceivedDate",
                HeaderText = _gridResourceManager.GetString("ReceivedDate"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "ReceivedDate"
            };
            DataGridViewInventoryLocations.Columns.Add(col);


            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = _gridResourceManager.GetString("Id"),
                Visible = false,
                Name = "Id"
            };
            DataGridViewInventoryLocations.Columns.Add(col);

            DataGridViewInventoryLocations.EnableHeadersVisualStyles = false;
            DataGridViewInventoryLocations.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            DataGridViewInventoryLocations.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);

            //foreach (DataGridViewColumn column in DataGridViewInventoryLocations.Columns)
            //{
            //    column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //    column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);
            //}
            //New Location Grid
            DataGridViewInventoryNewLocations.AutoGenerateColumns = false;
            DataGridViewInventoryNewLocations.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewInventoryNewLocations.AllowUserToAddRows = false;
            bCol = new DataGridViewButtonColumn
            {
                // HeaderText = _gridResourceManager.GetString("Id"),
                Visible = _workstationView.StationTypeId != (int)StationType.RackTablet && _workstationView.StationTypeId != (int)StationType.Supervisor,
                Name = "Position",
                Text = position,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                UseColumnTextForButtonValue = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(bCol);
            xcol = new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "InUse",
                HeaderText = _gridResourceManager.GetString("InUse"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "InUse"
            };
            DataGridViewInventoryNewLocations.Columns.Add(xcol);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "AreaName",
                HeaderText = _gridResourceManager.GetString("Area"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "StationName",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Slot",
                HeaderText = _gridResourceManager.GetString("Slot"),
                Visible = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Slot"
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);
            ckcol = new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "PrimeBin",
                HeaderText = _gridResourceManager.GetString("PrimeBin"),
                Visible = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "PrimeBin"
            };
            DataGridViewInventoryNewLocations.Columns.Add(ckcol);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc1",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc1", _gridResourceManager),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc1",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc2",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc2", _gridResourceManager),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc2",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc3",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc3", _gridResourceManager),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc3",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc4",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc4", _gridResourceManager),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc4",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc5",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc5", _gridResourceManager),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc5",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SizeCodeName",
                HeaderText = _gridResourceManager.GetString("SizeCodeName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "SizeCodeName",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "VelocityCodeName",
                HeaderText = _gridResourceManager.GetString("VelocityCodeName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "VelocityCodeName",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "HeightCodeName",
                HeaderText = _gridResourceManager.GetString("HeightCodeName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "HeightCodeName",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LocationCode",
                HeaderText = _gridResourceManager.GetString("LocationCode"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "LocationCode"
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PickSequence",
                HeaderText = $"Pick Sequence",  // _gridResourceManager.GetString("Slot"),
                Visible = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "PickSequence"
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);



            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = _gridResourceManager.GetString("Id"),
                Visible = false,
                Name = "Id"
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);

            DataGridViewInventoryNewLocations.EnableHeadersVisualStyles = false;
            DataGridViewInventoryNewLocations.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            DataGridViewInventoryNewLocations.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);

            //foreach (DataGridViewColumn column in DataGridViewInventoryNewLocations.Columns)
            //{
            //    column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //    column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);
            //}
        }
        private async void DataGridViewInventoryLocations_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            await DataGridViewPosition((DataGridView)sender, e.RowIndex);
        }
        private async void DataGridViewInventoryAddDetailLocations_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            await DataGridViewPosition((DataGridView)sender, e.RowIndex);
        }
        private void SetupNewForm()
        {
            //LabelFindDescription.Text = "Search any part of the Item or Description field";
            ComboBoxNewSizeCode.DataSource = _repoSizeCode.All();
            ComboBoxNewSizeCode.DisplayMember = "Name";
            ComboBoxNewSizeCode.ValueMember = "Id";
            ComboBoxNewVelocityCode.DataSource = _repoVelocityCode.All();
            ComboBoxNewVelocityCode.DisplayMember = "Name";
            ComboBoxNewVelocityCode.ValueMember = "Id";
            ComboBoxNewHeightCode.DataSource = _repoHeightCode.All();
            ComboBoxNewHeightCode.DisplayMember = "Name";
            ComboBoxNewHeightCode.ValueMember = "Id";
            //ComboBoxNewArea.DataSource = _areaRepository.Lookup();
            //ComboBoxNewArea.DisplayMember = "Name";
            //ComboBoxNewArea.ValueMember = "Id";
            ComboBoxNewStorageType.DataSource = _repoStorageType.All();
            ComboBoxNewStorageType.DisplayMember = "Name";
            ComboBoxNewStorageType.ValueMember = "Id";
            ComboBoxInventoryNewLocationsStorageType.DataSource = _repoStorageType.All();
            ComboBoxInventoryNewLocationsStorageType.DisplayMember = "Name";
            ComboBoxInventoryNewLocationsStorageType.ValueMember = "Id";
            ComboBoxNewUnitOfIssue.DataSource = _repoUnitOfIssue.All();
            ComboBoxNewUnitOfIssue.DisplayMember = "Name";
            ComboBoxNewUnitOfIssue.ValueMember = "Id";
        }
        private void SetupAddDetailForm()
        {
            ComboBoxAddDetailSizeCode.DataSource = _repoSizeCode.All();
            ComboBoxAddDetailSizeCode.DisplayMember = "Name";
            ComboBoxAddDetailSizeCode.ValueMember = "Id";
            ComboBoxAddDetailVelocityCode.DataSource = _repoVelocityCode.All();
            ComboBoxAddDetailVelocityCode.DisplayMember = "Name";
            ComboBoxAddDetailVelocityCode.ValueMember = "Id";
            ComboBoxAddDetailHeightCode.DataSource = _repoHeightCode.All();
            ComboBoxAddDetailHeightCode.DisplayMember = "Name";
            ComboBoxAddDetailHeightCode.ValueMember = "Id";
            ComboBoxAddDetailArea.DataSource = _areaRepository.Lookup();
            ComboBoxAddDetailArea.DisplayMember = "Name";
            ComboBoxAddDetailArea.ValueMember = "Id";
            ComboBoxAddDetailStorageType.DataSource = _repoStorageType.All();
            ComboBoxAddDetailStorageType.DisplayMember = "Name";
            ComboBoxAddDetailStorageType.ValueMember = "Id";
        }
        private void SetupViewEditForm()
        {
        }
        #endregion
        #region Return Key Functions
        //private void TextBoxNewLoc1_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        TextBoxNewLoc2.Focus();
        //    }
        //}
        //private void TextBoxNewLoc2_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        TextBoxNewLoc3.Focus();
        //    }
        //}
        //private void TextBoxNewLoc3_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        TextBoxNewLoc4.Focus();
        //    }
        //}
        //private void TextBoxNewLoc4_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        TextBoxNewLoc5.Focus();
        //    }
        //}
        //private void TextBoxNewLoc5_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        ComboBoxNewSizeCode.Focus();
        //    }
        //}
        //private void ComboBoxNewSizeCode_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        ComboBoxNewVelocityCode.Focus();
        //    }
        //}
        //private void ComboBoxNewVelocityCode_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        ComboBoxNewHeightCode.Focus();
        //    }
        //}
        //private void ComboBoxNewHeightCode_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        TextBoxNewLoc2.Focus();
        //    }
        //}
        //private void TextBoxViewEditLoc1_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        TextBoxViewEditLoc2.Focus();
        //    }
        //}
        //private void TextBoxViewEditLoc2_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        TextBoxViewEditLoc3.Focus();
        //    }
        //}
        //private void TextBoxViewEditLoc3_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        TextBoxViewEditLoc4.Focus();
        //    }
        //}
        //private void TextBoxViewEditLoc4_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        TextBoxViewEditLoc5.Focus();
        //    }
        //}
        //private void TextBoxViewEditLoc5_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        ComboBoxViewEditSizeCode.Focus();
        //    }
        //}
        //private void ComboBoxViewEditSizeCode_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        ComboBoxViewEditVelocityCode.Focus();
        //    }
        //}
        //private void ComboBoxViewEditVelocityCode_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        ComboBoxViewEditHeightCode.Focus();
        //    }
        //}
        //private void ComboBoxViewEditHeightCode_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        TextBoxViewEditLoc1.Focus();
        //    }
        //}
        //private void tabControl1_Enter(object sender, EventArgs e)
        //{
        //    //if (tabControl1.SelectedIndex == 1)
        //    //{
        //    //    TextBoxViewEditLoc1.Focus();
        //    //}
        //    //if (tabControl1.SelectedIndex == 2)
        //    //{
        //    //    TextBoxNewLoc1.Focus();
        //    //}
        //}
        #endregion
        private async void MbViewEditDelete_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (_bindingSource.Current != null)
            {
                var locView = ((ObjectView<SqlInventoryView>)_locationBindingSource.Current).Object;
                if (locView != null)
                {
                    var result = MessageBox.Show(_resourceManager.GetString("Message17"), string.Empty,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes) return;
                    await DeleteInventoryItemAsync(locView.Id, false);
                    await LoadViewEdit();
                    tabControl1.SelectedTab = tabPage2;
                }
            }
            else
            {
                MessageBox.Show(_resourceManager.GetString("Message6"));
            }
            Cursor.Current = Cursors.Default;
        }
        public async Task DeleteInventoryItemAsync(int invId, bool releaseOnly)
        {
            var inventoryManager = new InventoryManager(_repoInventory, _locationsRepository);
            await inventoryManager.DeleteInventoryRecordAsync(invId, releaseOnly: releaseOnly);
        }
      
        /// <summary>
        /// Add a new location to the inventory from the view/edit tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MbViewEditAddLocation_Click(object sender, EventArgs e)
        {
            const int inUse = 0; // default to not in use

            _processing = true;
            UpdateAddLocationForm();
            // Get the available locations for the current item
            var recs = GetAvailableLocations(CurrentItem, inUse);

            UpdateDataGridViewInventoryNewLocations(recs);
            _processing = false;

            tabControl1.SelectedTab = tabPage4;
        }
        /// <summary>
        /// Add a new location to the inventory from the view/edit tab,
        /// Add Location button 
        /// 
        /// </summary>
        private void UpdateAddLocationForm()
        {
            //check for a valid CurrentItem
            if (CurrentItem != null)
            {
                //set the TextBoxes to the current item values
                TextBoxInventoryNewLocationsItem.Text = CurrentItem.Item;
                TextBoxInventoryNewLocationsDescription.Text = CurrentItem.Description;
                TextBoxNewLocationsItemDefinitionId.Text = CurrentItem.Id.ToString();
                // set the default values for the new location
                TextBoxInventoryNewLocationsQuantity.Text = "0";
                ComboBoxInventoryNewLocationsStorageType.SelectedIndex = 1;
            }
        }

        /// <summary>
        /// Get the Preferred/Exact available locations for the current item
        /// </summary>
        /// <param name="itemDefinition">ItemDefinition</param>
        /// <param name="inUse"></param>
        /// <param name="recId"></param>
        public List<LocationView> GetAvailableLocations(ItemDefinition itemDefinition, int inUse, int recId = 0)
        {
            _logger.LogDetailAsync($"GetAvailableLocations START").SafeFireAndForget();
            var idx = recId;
            List<LocationView> views = new List<LocationView>();
            if (itemDefinition == null) return views;

            // var areaId = itemDefinition.AreaId;
            // var inUse = GetInUse();
            // var area = _areaRepository.GetArea(itemDefinition.AreaId);
            var area = itemDefinition.Area;
            if (area == null) return views;

            //views = await _locationsRepository.GetAllLocationViewsExact(area.Id, itemDefinition.SizeCodeId,
            //   itemDefinition.VelocityCodeId, itemDefinition.HeightCodeId, inUse);

            if (inUse == 2)
            {
                views = _locationsRepository.GetAllLocationViewsExact(area.Id, itemDefinition.SizeCodeId,
                    itemDefinition.VelocityCodeId, itemDefinition.HeightCodeId, inUse).ToList();
            }
            else
            {
                // find all locations in this area that are in use or not in use
                var use = Convert.ToBoolean(inUse);
                views = _locationsRepository.GetAllLocationViewsExactByInUse(area.Id, itemDefinition.SizeCodeId,
                    itemDefinition.VelocityCodeId, itemDefinition.HeightCodeId, inUse).ToList();
            }
            _logger.LogDetailAsync($"GetAvailableLocations Views Count = {views.Count()}").SafeFireAndForget();
            return views;

        }

        private void UpdateDataGridViewInventoryNewLocations(List<LocationView> locationViews)
        {
            int idx;
            // if (inUse != 2)
            //{
            //    var use = Convert.ToBoolean(inUse);
            //    views = views.Where(r => r.InUse == use).ToList();
            //}

            var blv = new BindingListView<LocationView>(locationViews);

            _logger.LogDetailAsync($"GetAvailableLocations 1").SafeFireAndForget();
            _newLocationBindingSource.DataSource = blv;
            _logger.LogDetailAsync($"GetAvailableLocations 2").SafeFireAndForget();

            DataGridViewInventoryNewLocations.DataSource = _newLocationBindingSource;

            _logger.LogDetailAsync($"GetAvailableLocations 3").SafeFireAndForget();
            if (GetRecordCount(_newLocationBindingSource) > 0)
            {
                //_logger.LogDetailAsync($"GetAvailableLocations 4").SafeFireAndForget();
                //if (recId != 0)
                //{
                //    _logger.LogDetailAsync($"GetAvailableLocations 5").SafeFireAndForget();
                //    idx = IndexOf(_newLocationBindingSource, recId);
                //}
                //_logger.LogDetailAsync($"GetAvailableLocations 6").SafeFireAndForget();
                //DataGridViewInventoryNewLocations.FirstDisplayedScrollingRowIndex = DataGridViewInventoryNewLocations.Rows[idx].Index;
                //_logger.LogDetailAsync($"GetAvailableLocations 7").SafeFireAndForget();
                //DataGridViewInventoryNewLocations.Refresh();
                //_logger.LogDetailAsync($"GetAvailableLocations 8").SafeFireAndForget();
                //DataGridViewInventoryNewLocations.CurrentCell = DataGridViewInventoryNewLocations.Rows[idx].Cells[1];
                //_logger.LogDetailAsync($"GetAvailableLocations 9").SafeFireAndForget();
                //DataGridViewInventoryNewLocations.Rows[idx].Selected = true;

            }
            _logger.LogDetailAsync($"GetAvailableLocations END").SafeFireAndForget();
        }


        ///// <summary>
        ///// Get the Preferred/Exact available locations for the current item
        ///// </summary>
        ///// <param name="itemDefinition">ItemDefinition</param>
        ///// <param name="inUse"></param>
        ///// <param name="recId"></param>
        //public void GetAvailableLocations(ItemDefinition itemDefinition, int inUse, int recId = 0)
        //{
        //    _logger.LogDetailAsync($"GetAvailableLocations START").SafeFireAndForget();
        //    var idx = recId;
        //    if (itemDefinition == null) return;
        //    IEnumerable<LocationView> views;
        //    // var areaId = itemDefinition.AreaId;
        //    // var inUse = GetInUse();
        //    // var area = _areaRepository.GetArea(itemDefinition.AreaId);
        //    var area = itemDefinition.Area;
        //    if (area == null) return;

        //    //views = await _locationsRepository.GetAllLocationViewsExact(area.Id, itemDefinition.SizeCodeId,
        //    //   itemDefinition.VelocityCodeId, itemDefinition.HeightCodeId, inUse);

        //    if (inUse == 2)
        //    {
        //        views = _locationsRepository.GetAllLocationViewsExact(area.Id, itemDefinition.SizeCodeId,
        //            itemDefinition.VelocityCodeId, itemDefinition.HeightCodeId, inUse).ToList();
        //    }
        //    else
        //    {
        //        // find all locations in this area that are in use or not in use
        //        var use = Convert.ToBoolean(inUse);
        //        views = _locationsRepository.GetAllLocationViewsExactByInUse(area.Id, itemDefinition.SizeCodeId,
        //            itemDefinition.VelocityCodeId, itemDefinition.HeightCodeId, inUse).ToList();
        //    }
        //    _logger.LogDetailAsync($"GetAvailableLocations Views Count = {views.Count()}").SafeFireAndForget();


        //    // if (inUse != 2)
        //    //{
        //    //    var use = Convert.ToBoolean(inUse);
        //    //    views = views.Where(r => r.InUse == use).ToList();
        //    //}

        //    var blv = new BindingListView<LocationView>(views.ToList());
        //    _logger.LogDetailAsync($"GetAvailableLocations 1").SafeFireAndForget();
        //    _newLocationBindingSource.DataSource = blv;
        //    _logger.LogDetailAsync($"GetAvailableLocations 2").SafeFireAndForget();
        //    DataGridViewInventoryNewLocations.DataSource = _newLocationBindingSource;
        //    _logger.LogDetailAsync($"GetAvailableLocations 3").SafeFireAndForget();
        //    if (GetRecordCount(_newLocationBindingSource) > 0)
        //    {
        //        _logger.LogDetailAsync($"GetAvailableLocations 4").SafeFireAndForget();
        //        if (recId != 0)
        //        {
        //            _logger.LogDetailAsync($"GetAvailableLocations 5").SafeFireAndForget();
        //            idx = IndexOf(_newLocationBindingSource, recId);
        //        }
        //        _logger.LogDetailAsync($"GetAvailableLocations 6").SafeFireAndForget();
        //        DataGridViewInventoryNewLocations.FirstDisplayedScrollingRowIndex = DataGridViewInventoryNewLocations.Rows[idx].Index;
        //        _logger.LogDetailAsync($"GetAvailableLocations 7").SafeFireAndForget();
        //        DataGridViewInventoryNewLocations.Refresh();
        //        _logger.LogDetailAsync($"GetAvailableLocations 8").SafeFireAndForget();
        //        DataGridViewInventoryNewLocations.CurrentCell = DataGridViewInventoryNewLocations.Rows[idx].Cells[1];
        //        _logger.LogDetailAsync($"GetAvailableLocations 9").SafeFireAndForget();
        //        DataGridViewInventoryNewLocations.Rows[idx].Selected = true;

        //    }
        //    _logger.LogDetailAsync($"GetAvailableLocations END").SafeFireAndForget();
        //}
        private List<InventoryView> GetSelectedInventoryViews(DataGridView dataGridView)
        {
            var inventoryViews = new List<InventoryView>();
            foreach (DataGridViewRow row in dataGridView.SelectedRows)
            {
                var inventoryId = (int)row.Cells["Id"].Value;
                var inventoryView = _inventoryRepository.GetInventoryViewById(inventoryId);
                if (inventoryView != null)
                {
                    inventoryViews.Add(inventoryView);
                }
            }
            return inventoryViews;
        }
        private InventoryView GetSelectedInventoryView(DataGridView dataGridView)
        {
            if (dataGridView.SelectedRows.Count == 0) return null;
            var row = dataGridView.SelectedRows[0];
            var inventoryId = (int)row.Cells["Id"].Value;
            var inventoryView = _inventoryRepository.GetInventoryViewById(inventoryId);
            return inventoryView;
        }

        /// <summary>
        /// Revised edition that uses PickStations
        /// A Workstation can have multiple PickStations
        /// The WorkstationView has a variable, Stations with the Stations that this Workstation Controls
        /// </summary>
        /// <param name="itemDefinition"></param>
        /// <param name="inUse"></param>
        /// <param name="recId"></param>
        //public async void GetAvailableLocationsByStation(ItemDefinition itemDefinition, int recId = 0)
        //{
        //    var areas = _areaRepository.GetAllPickableAreas();
        //    //string areas = string.Empty;
        //    var idx = recId;
        //    var areaIds = _areaRepository.GetAllPickableAreaIds();
        //    //var areaIds  = _workstationView.Areas.Select(r => new List<string> {r.Id.ToString()}).ToList();
        //    //if (areaIds.Any())
        //    //{
        //    //    areas = string.Join(",", areaIds);
        //    //}
        //    var areasAsString = string.Join(",", areaIds);
        //    var inUse = 0;
        //    var area = _areaRepository.GetArea(itemDefinition.AreaId);
        //    if (area != null)
        //    {
        //        var views = await _locationsRepository.GetAllLocationViewsExactByAreas(areasAsString, itemDefinition.SizeCodeId,
        //            itemDefinition.VelocityCodeId, itemDefinition.HeightCodeId, inUse);

        //        var blv = new BindingListView<LocationView>(views.ToList());
        //        _newLocationBindingSource.DataSource = blv;
        //        DataGridViewInventoryNewLocations.DataSource = _newLocationBindingSource;
        //        if (GetRecordCount(_newLocationBindingSource) > 0)
        //        {
        //            if (recId != 0)
        //            {
        //                idx = IndexOf(_newLocationBindingSource, recId);
        //            }

        //            DataGridViewInventoryNewLocations.FirstDisplayedScrollingRowIndex =
        //                DataGridViewInventoryNewLocations.Rows[idx].Index;
        //            DataGridViewInventoryNewLocations.Refresh();
        //            DataGridViewInventoryNewLocations.CurrentCell =
        //                DataGridViewInventoryNewLocations.Rows[idx].Cells[1];
        //            DataGridViewInventoryNewLocations.Rows[idx].Selected = true;
        //        }
        //    }
        //}
        public void GetAllAvailableLocations(ItemDefinition itemDefinition, int inUse, int recId = 0)
        {
            _logger.LogDetailAsync("GetAllAvailableLocations START").SafeFireAndForget();

            var idx = recId;

            if (itemDefinition == null) return;
            IEnumerable<LocationView> views;
            var areaId = itemDefinition.AreaId;
            //var inUse = GetInUse();
            if (inUse == 2)
            {
                // find all locations in this area regardless of in use
                views = _locationsRepository.FindLocationViewsByArea(areaId);
            }
            else
            {
                // find all locations in this area that are in use or not in use FindLocationViewsByAreaAndInUse
                //var use = Convert.ToBoolean(inUse);
                views = _locationsRepository.FindLocationViewsByAreaAndInUse(areaId, inUse);
            }

            if (views != null)
            {
                _logger.LogDetailAsync($"GetAllAvailableLocations views.Count: {views.Count()} ").SafeFireAndForget();

                var blv = new BindingListView<LocationView>(views.ToList());

                _logger.LogDetailAsync("Step 1").SafeFireAndForget();
                _newLocationBindingSource.DataSource = blv;
            }

            _logger.LogDetailAsync("Step 2").SafeFireAndForget();
            DataGridViewInventoryNewLocations.DataSource = _newLocationBindingSource;
            _logger.LogDetailAsync("Step 3").SafeFireAndForget();

            if (GetRecordCount(_newLocationBindingSource) <= 0) return;

            if (recId != 0)
            {
                idx = IndexOf(_newLocationBindingSource, recId);
            }

            DataGridViewInventoryNewLocations.FirstDisplayedScrollingRowIndex =
                DataGridViewInventoryNewLocations.Rows[idx].Index;

            DataGridViewInventoryNewLocations.Refresh();

            DataGridViewInventoryNewLocations.CurrentCell =
                DataGridViewInventoryNewLocations.Rows[idx].Cells[1];

            DataGridViewInventoryNewLocations.Rows[idx].Selected = true;
            _logger.LogDetailAsync("GetAllAvailableLocations END").SafeFireAndForget();
        }
        /// <summary>
        /// Gets the in use value from the radio buttons
        /// InUse = 1 = In Use;
        /// InUse = 0 = Not In Use;
        /// InUse = 2 = Show All
        /// </summary>
        /// <returns></returns>
        private int GetInUse()
        {
            _logger.LogDetailAsync("GetInUse").SafeFireAndForget();
            // check to see which radio button is selected
            var inUse = 0;
            if (RadioButtonInUse.Checked)
            {
                inUse = 1;
            }

            if (RadioButtonNotInUse.Checked)
            {
                inUse = 0;
            }

            if (RadioButtonShowAll.Checked)
            {
                inUse = 2;
            }
            return inUse;
        }
        private void FrmInventory_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !CloseButtonPressed;
        }
        private async void TextBoxNewItem_Leave(object sender, EventArgs e)
        {
            // get the item and fill in the form
            await NewItemFind();
        }
        private async Task NewItemFind()
        {
            var item = TextBoxNewItem.Text;
            if (string.IsNullOrWhiteSpace(item)) return;

            try
            {
                var recs = await _repoItemDefinition.FindByAsync(f => f.Item == item);
                // if the item Definition is null, check the AKA table
                if (recs == null)
                {
                    var possibleAka = item;
                    var item2 = _akaRepository.Get(possibleAka);
                    if (item2 != null)
                    {
                        TextBoxNewItem.Text = item2;
                        recs = await _repoItemDefinition.FindByAsync(r => r.Item == item2);
                    }
                }

                if (recs != null)
                {
                    // create the datasource for ComboBoxNewAreaChoice
                    // the Id is the Id of the ItemDefinition
                    // the Name is the Name of the Area

                    var areas = recs.Select(r => new LookUp()
                    {
                        Id = r.Id
                        ,
                        Name = r.Area.Name
                    }).ToList();
                    ComboBoxNewAreaChoice.DataSource = areas;
                    ComboBoxNewAreaChoice.DisplayMember = "Name";
                    ComboBoxNewAreaChoice.ValueMember = "Id";

                    var rec = recs.FirstOrDefault();
                    TextBoxNewId.Text = rec.Id.ToString();
                    TextBoxNewItem.Text = rec.Item;
                    TextBoxNewDescription.Text = rec.Description;
                    CheckBoxNewScale.Checked = rec.Scale;
                    //ComboBoxNewArea.SelectedValue = rec.AreaId;
                    TextBoxArea.Text = rec.Area.Name;
                    TextBoxNewLocationMax.Text = rec.LocationMax.ToString();
                    TextBoxNewLocationMin.Text = rec.LocationMin.ToString();
                    TextBoxNewSystemMax.Text = rec.SystemMax.ToString();
                    TextBoxNewSystemMin.Text = rec.SystemMin.ToString();
                    TextBoxNewWeight.Text = rec.Weight.ToString("F4");
                    TextBoxPickMax.Text = rec.PickMax.ToString();
                    ComboBoxNewStorageType.SelectedValue = rec.StorageTypeId;
                    ComboBoxNewUnitOfIssue.SelectedValue = rec.UnitOfIssueId;
                    ComboBoxNewSizeCode.SelectedValue = rec.SizeCodeId;
                    ComboBoxNewVelocityCode.SelectedValue = rec.VelocityCodeId;
                    ComboBoxNewHeightCode.SelectedValue = rec.HeightCodeId;
                    MbNewAddLocation.Enabled = true;
                }
                _firstTimeCombo = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(_resourceManager.GetString("Message8") + ex.Message + ex.InnerException.Message);
                TextBoxNewItem.Focus();
            }
        }
        private void UpdateNewItem(int itemDefinitionId)
        {
            // check to see if itemDefinitionId is 0
            if (itemDefinitionId == 0) return;
            try
            {
                var rec = _repoItemDefinition.FindBy(f => f.Id == itemDefinitionId).FirstOrDefault();

                // if the item Definition is null, check the AKA table
                if (rec == null)
                {
                    var possibleAka = rec.Item;
                    var item2 = _akaRepository.Get(possibleAka);
                    if (item2 != null)
                    {
                        TextBoxNewItem.Text = item2;
                        rec = _repoItemDefinition.FindBy(r => r.Item == item2).FirstOrDefault();

                    }
                }

                if (rec != null)
                {

                    TextBoxArea.Text = rec.Area.Name;
                    TextBoxNewId.Text = rec.Id.ToString();
                    TextBoxNewItem.Text = rec.Item;
                    TextBoxNewDescription.Text = rec.Description;
                    CheckBoxNewScale.Checked = rec.Scale;
                    // ComboBoxNewArea.SelectedValue = rec.AreaId;
                    TextBoxNewLocationMax.Text = rec.LocationMax.ToString();
                    TextBoxNewLocationMin.Text = rec.LocationMin.ToString();
                    TextBoxNewSystemMax.Text = rec.SystemMax.ToString();
                    TextBoxNewSystemMin.Text = rec.SystemMin.ToString();
                    TextBoxNewWeight.Text = rec.Weight.ToString("F4");
                    TextBoxPickMax.Text = rec.PickMax.ToString();
                    ComboBoxNewStorageType.SelectedValue = rec.StorageTypeId;
                    ComboBoxNewUnitOfIssue.SelectedValue = rec.UnitOfIssueId;
                    ComboBoxNewSizeCode.SelectedValue = rec.SizeCodeId;
                    ComboBoxNewVelocityCode.SelectedValue = rec.VelocityCodeId;
                    ComboBoxNewHeightCode.SelectedValue = rec.HeightCodeId;
                    MbNewAddLocation.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(_resourceManager.GetString("Message8") + ex.Message + ex.InnerException.Message);
                TextBoxNewItem.Focus();
            }
        }
        private async void MbNewAddLocation_Click(object sender, EventArgs e)
        {
            var inUse = 0; // 0 = not in use, 1 = in use, 2 = all

            // get the text of a ComboBoxNewAreaChoice
            var itemDefinitionId = (int)ComboBoxNewAreaChoice.SelectedValue;
            //string areaText = ComboBoxNewAreaChoice.SelectedItem.ToString();
          //  var item = TextBoxNewItem.Text;
          //  var area = await _repoArea.FindByFirstOrDefaultAsync(r => r.Name == areaText);
         //   var itemDefinition = _repoItemDefinition.FindByFirstOrDefaultAsync(r => r.Item == item && r.AreaId == area.Id);
         //   var id = (TextBoxNewId.Text).ParseInt();
            
            
            await SetCurrentItemDefinition(itemDefinitionId);
            if (CurrentItem != null)
            {
                TextBoxInventoryNewLocationsItem.Text = CurrentItem.Item;
                TextBoxInventoryNewLocationsDescription.Text = CurrentItem.Description;
                GetAvailableLocations(CurrentItem, inUse);
            }
            tabControl1.SelectedTab = tabPage4;
        }
        private async void MbNewLocationsClose_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage2;
            // SetCurrentItemDefinition();
            await LoadViewEdit();
        }
        private async void MbNewLocationsListing_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            await RefreshData();
            Cursor.Current = Cursors.Default;
            tabControl1.SelectedTab = tabPage1;
        }
        private async void MbNewLocationsSave_Click(object sender, EventArgs e)
        {
            if (TextBoxInventoryNewLocationsQuantity.Text.ParseInt() == 0 &&
                ComboBoxInventoryNewLocationsStorageType.SelectedValue.ToString().ParseInt() ==
                (int)StorageType.Release)
            {
                MessageBox.Show(_resourceManager.GetString("Message9"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                TextBoxInventoryNewLocationsQuantity.Focus();
            }
            else
            {
                if (_newLocationBindingSource.Count > 0)
                {
                    var locView = ((ObjectView<LocationView>)_newLocationBindingSource.Current).Object;
                    if (locView != null)
                    {
                        var inventory = new Inventory()
                        {
                            ItemDefinitionId = CurrentItem.Id,
                            LocationId = locView.Id,
                            Quantity = TextBoxInventoryNewLocationsQuantity.Text.ParseInt(),
                            ReceivedDate = DateTimePickerInventoryNewLocationsReceivedDate.Value,
                            StorageTypeId =
                                ComboBoxInventoryNewLocationsStorageType.SelectedValue.ToString().ParseInt(),
                            PrimeBin = CheckBoxInventoryNewLocationsPrimeBin.Checked,
                            AreaId = CurrentItem.AreaId,
                            RFID = TextBoxInventoryNewLocationsRfid.Text
                        };
                        await _repoInventory.InsertAsync(inventory);
                        await GlobalVar.HistoryManager.SaveHistoryAsync(ActionCode.InventoryAdd, inventory);
                        await _locationsRepository.SetLocationInUse(inventory.LocationId, b: true);

                        //TODO  fixed locationCode 

                        //var locationCode = @"1234567890123456";


                        //_locationsRepository.SetLocationCode(inventory.LocationId, locationCode);

                        await SetCurrentInventoryItem(inventory);
                        await LoadViewEdit();
                        tabControl1.SelectedTab = tabPage2;
                    }
                }
                else
                {
                    MessageBox.Show(_resourceManager.GetString("Message10"));
                }
            }
        }
        private async Task<List<SqlInventoryView>> GetInventoryViewListByItem(int itemId)
        {
            List<SqlInventoryView> recs = new List<SqlInventoryView>();
            try
            {
                using (var context = new NeutronDb())
                {
                    var findItemId = new SqlParameter("@ItemId", itemId);
                    recs = await context.Database
                        .SqlQuery<SqlInventoryView>("usp_GetInventoryViewByItem @ItemId", findItemId).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Error during Get Inventory View List By Item Event. {ex.Message}").SafeFireAndForget();
            }
            return recs;
        }
        private IEnumerable<SqlInventoryView> GetInventoryViewList()
        {
            IEnumerable<SqlInventoryView> recs = new List<SqlInventoryView>();
            using (var context = new NeutronDb())
            {
                recs = context.Database.SqlQuery<SqlInventoryView>("usp_GetInventoryView").ToList();
            }
            return recs;
        }
        private void TextBoxNewItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                SendKeys.Send("{Tab}");
            }
        }
        private void HideTabControlTabs()
        {
            var controls = GetTabControls(this, typeof(TabControl));
            foreach (var control1 in controls)
            {
                var control = (TabControl)control1;
                control.Appearance = TabAppearance.FlatButtons;
                control.ItemSize = new Size(0, 1);
                control.SizeMode = TabSizeMode.Fixed;
                foreach (TabPage tab in control.TabPages)
                {
                    tab.Text = string.Empty;
                }
            }
        }
        private IEnumerable<Control> GetTabControls(Control control, Type type)
        {
            var controls = control.Controls.Cast<Control>();
            var enumerable = controls.ToList();
            return enumerable.SelectMany(c => GetTabControls(c, type)).Concat(enumerable).Where(c => c.GetType() == type);
        }
        private async void MbAddDetailSave_Click(object sender, EventArgs e)
        {
            var beginningQuantity = 0;
            var inventory = new Inventory();
            var inventoryId = TextBoxAddDetailInventoryId.Text;
            if (!string.IsNullOrEmpty(inventoryId))
            {
                //update
                inventory = await _repoInventory.FindByKeyAsync(inventoryId.ParseInt());
                await GlobalVar.HistoryManager.SaveHistoryAsync(ActionCode.InventoryModify, inventory, inventory.Quantity, true);

                beginningQuantity = inventory.Quantity;
                inventory.Quantity = (TextBoxAddDetailQuantity.Text).ParseInt();
                inventory.StorageTypeId = ((NeutronData.Models.Lookups.StorageType)ComboBoxAddDetailStorageType.SelectedItem).Id;
                inventory.ReceivedDate = DateTimePickerAddDetailReceivedDate.Value;
                inventory.PrimeBin = CheckBoxAddDetailPrimeBin.Checked;
                inventory.AreaId = (int)ComboBoxAddDetailArea.SelectedValue;
                inventory.RFID = TextBoxAddDetailLocationCode.Text;
                await _repoInventory.UpdateAsync(inventory);
                await GlobalVar.HistoryManager.SaveHistoryAsync(ActionCode.InventoryModify, inventory, beginningQuantity, true);
            }
            else
            {
                //new
                inventory.ItemDefinitionId = (TextBoxAddDetailItemDefinitionId.Text).ParseInt();
                inventory.LocationId = (TextBoxAddDetailLocationId.Text).ParseInt();
                inventory.Quantity = (TextBoxAddDetailQuantity.Text).ParseInt();
                inventory.StorageTypeId = ((NeutronData.Models.Lookups.StorageType)ComboBoxAddDetailStorageType.SelectedItem).Id;
                inventory.ReceivedDate = DateTimePickerAddDetailReceivedDate.Value;
                inventory.PrimeBin = CheckBoxAddDetailPrimeBin.Checked;
                inventory.AreaId = (int)ComboBoxAddDetailArea.SelectedValue;
                inventory.RFID = TextBoxAddDetailLocationCode.Text;
                await _repoInventory.InsertAsync(inventory);
                await GlobalVar.HistoryManager.SaveHistoryAsync(ActionCode.InventoryAdd, inventory, beginningQuantity, true);
            }

            var inventoryManager = new InventoryManager(_repoInventory, _locationsRepository);
            var deleted = await inventoryManager.ReleaseCheckAsync(inventory);
            
            //if (inventory.Quantity == 0 && inventory.StorageTypeId == (int)StorageType.Release)
            //{
            //    await DeleteInventoryItemAsync(inventory.Id, releaseOnly: true);
            //}
            //else
            if (!deleted)
            {
                await _locationsRepository.SetLocationInUse(inventory.LocationId, b: true);
                TextBoxViewEditId.Text = CurrentItem.Id.ToString();
                TextBoxViewEditItem.Text = CurrentItem.Item;
                TextBoxViewEditDescription.Text = CurrentItem.Description;
            }
            
            var views = await GetInventoryViewListByItem(CurrentItem.Id);
            var blv = new BindingListView<SqlInventoryView>(views.ToList());
            _locationBindingSource.DataSource = blv;
            DataGridViewInventoryLocations.DataSource = _locationBindingSource;
            tabControl1.SelectedTab = tabPage2;
        }
        private async void MbAddDetailClose_Click(object sender, EventArgs e)
        {
            //_ = RefreshData();
            //SetCurrentItemDefinition();
            await LoadViewEdit();
            // ClearAllShi();
            if (_workstationView.ProLiteManager != null)
            {
                await _workstationView.ProLiteManager.ClearAllProlites();
            }

            if (_iptiDisplayFunctions != null)
            {
                await _iptiDisplayFunctions.ClearBlastzone();
            }
            tabControl1.SelectedTab = tabPage2;
        }
        private async void MbAddDetailListing_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            await RefreshData();
            Cursor.Current = Cursors.Default;
            tabControl1.SelectedTab = tabPage1;
        }
        private void ClearAddDetailFields()
        {
            TextBoxAddDetailInventoryId.Text = "";
            TextBoxAddDetailLocationId.Text = "";
            TextBoxAddDetailQuantity.Text = "";
            TextBoxAddDetailItemDefinitionId.Text = "";
            ComboBoxAddDetailArea.SelectedValue = -1;
            TextBoxAddDetailLoc1.Text = "";
            TextBoxAddDetailLoc2.Text = "";
            TextBoxAddDetailLoc3.Text = "";
            TextBoxAddDetailLoc4.Text = "";
            TextBoxAddDetailLoc5.Text = "";
            TextBoxAddDetailSlot.Text = "";
            CheckBoxInUse.Checked = false;
            ComboBoxAddDetailStorageType.SelectedValue = -1;
            ComboBoxAddDetailSizeCode.SelectedValue = -1;
            ComboBoxAddDetailVelocityCode.SelectedValue = -1;
            ComboBoxAddDetailHeightCode.SelectedValue = -1;
        }
        private void ClearNewFields()
        {
            TextBoxNewId.Text = "";
            TextBoxNewItem.Text = "";
            TextBoxNewDescription.Text = "";
            CheckBoxNewScale.Checked = false;
            CheckBoxInUse.Checked = false;
            // ComboBoxNewArea.SelectedValue = -1;
            TextBoxArea.Text = "";
            TextBoxNewLocationMax.Text = "";
            TextBoxNewLocationMin.Text = "";
            TextBoxNewSystemMax.Text = "";
            TextBoxNewSystemMin.Text = "";
            TextBoxNewWeight.Text = "";
            ComboBoxNewStorageType.SelectedValue = -1;
            ComboBoxNewUnitOfIssue.SelectedValue = -1;
            ComboBoxNewSizeCode.SelectedValue = -1;
            ComboBoxNewVelocityCode.SelectedValue = -1;
            ComboBoxNewHeightCode.SelectedValue = -1;
        }

        private async void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            await LoadViewEdit();
        }

        private async void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //if (_bindingSource.Current == null) return;
            // var inventory = ((ObjectView<SqlInventoryView>)_bindingSource.Current).Object;
            // if (_workstationView.AreaId != inventory.AreaId)
            // {
            // MessageBox.Show($"The SKU is not in this Area.");
            //return;
            // }
            await DataGridViewPosition((DataGridView)sender, e.RowIndex);
            await SetCurrentInventoryItem();
        }
        private async Task DataGridViewPosition(DataGridView grid, int rowIndex)
        {
            //if (_workstationView.AreaId == AreaEight) return;

            var qty = 0;
            var quantity = "0";
            var display = string.Empty;

            try
            {
                if (rowIndex < 0) return;
                if (!grid.Columns.Contains("Position")) return;
                //var position = grid["Position", rowIndex].Value.ToString().ParseInt();

                if (grid.Columns["Position"] != null)
                {
                    if (grid.CurrentCell.ColumnIndex != grid.Columns["Position"].Index) return;
                }

                if (!grid.Columns.Contains(columnName: "Loc1")) return;
                var deviceNumber = grid["Loc1", rowIndex].Value.ToString().ParseInt();
                if (!grid.Columns.Contains(columnName: "Loc2")) return;
                var trayNumber = grid["Loc2", rowIndex].Value.ToString().ParseInt();
                if (!grid.Columns.Contains(columnName: "Loc3")) return;
                var level = grid["Loc3", rowIndex].Value.ToString().ParseInt();
                if (!grid.Columns.Contains(columnName: "Loc4")) return;
                var part = grid["Loc4", rowIndex].Value.ToString().ParseInt();
                if (grid.Columns.Contains(columnName: "Quantity"))
                {
                    qty = grid["Quantity", rowIndex].Value.ToString().ParseInt();
                }

                //if (_workstationView.StationType.Id == (int)StationType.EBin)
                //{
                //    var eBinDisplayManager = new EBinDisplayManager();
                //    var response = eBinDisplayManager.TurnOnDisplay(deviceNumber, trayNumber, level, part, qty, display);
                //    MessageBox.Show($"{response}", "EBin Display Command", MessageBoxButtons.OK);
                //}
                //MoveDevice(deviceNumber, trayNumber, level, part, qty, display);

                // if there are Hanels on this station
                // check to see if this lift is already moving before trying to move again
                if (_workstationView.Hanels.Any())
                {
                    if (GlobalVar.Hanel != null)
                    {
                        var status = GlobalVar.Hanel.GetDeviceStatus(deviceNumber);

                        if (status.CommandAccepted && !status.CommandExecuted)
                        {
                            var message =
                                $"Unable to move Tower {deviceNumber}.{Environment.NewLine}It is currently moving Tray {status.TargetTray} into position.";
                            MessageBox.Show(message, "Lift Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            await MoveDevice(deviceNumber, trayNumber, level, part, qty, display);
                        }

                    }
                }


                // if it's a Blastzone, turn on the Location
                // and turn on the Prolite, if they're Enabled

                quantity = qty.ToString();

             
                    if (_workstationView.Blastzones.Any())
                    {
                        if (_iptiDisplayFunctions != null)
                        {
                            await _iptiDisplayFunctions.ClearBlastzone();
                            await _iptiDisplayFunctions.TurnOnBlastzoneDisplay(trayNumber, part, quantity);
                            await _iptiDisplayFunctions.TurnOnBlastzoneOrderControl(trayNumber, $"Qty: {quantity}");
                        }
                    } 
                
                
                //Prolites
                if (_workstationView.ProLiteManager != null)
                {
                    if (_workstationView.ProLiteManager.GetProlites().Count > 0)
                    {
                        var proliteNumber = 1;
                        if (_workstationView.ProLiteManager.GetProlites().Count > 1)
                        {
                            proliteNumber = deviceNumber;
                        }
                        // only one Prolite, so it doesn't matter what device number
                        // just light up the first Prolite
                        if (_workstationView.ProLiteManager != null)
                        {
                            await _workstationView.ProLiteManager.ClearAllProlites();
                            _workstationView.ProLiteManager.TurnOn(proliteNumber, level, part, qty);
                        }

                    } 
                }

                // if there are Hanels on this station
                //if (_workstationView.Hanels.Any() )
                //{
                //    await MoveDevice(deviceNumber, trayNumber, level, part, qty, display);
                //}

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to move device. {Environment.NewLine} {ex.Message}");
            }
        }



        private async Task MoveDevice(int deviceNumber, int trayNumber, int level, int part, int quantity = 0, string display = "")
        {
            try
            {
                await Task.Delay(100);
                if (_lacProcessor.MovePermitted(_workstationView.WorkstationId, deviceNumber, trayNumber))
                {
                    if (_neutronVariables.ShuttleEnabled)
                    {

                        var hanel = _workstationView.Hanels.FirstOrDefault(r => r.DeviceNumber == deviceNumber);

                        if (hanel != null)
                        {
                            if (hanel.Enabled)
                            {
                                if (GlobalVar.Shuttle != null)
                                {
                                    var response = await Task.Run(() =>
                                        GlobalVar.Shuttle.PositionDevice(deviceNumber, trayNumber, level, part, quantity,
                                            display));
                                    if (response != DeviceResponse.Success)
                                    {
                                        var msg = response.AsString(EnumFormat.Description);
                                        Mediator.GetInstance().OnGeneralError(this, msg);
                                    }
                                }
                                else if (GlobalVar.Hanel != null)
                                {
                                    var response = await Task.Run(() =>
                                        GlobalVar.Hanel.PositionDevice(deviceNumber, trayNumber, level, part, quantity,
                                            display));
                                    if (response != DeviceResponse.Success)
                                    {
                                        var msg = response.AsString(EnumFormat.Description);
                                        Mediator.GetInstance().OnGeneralError(this, msg);
                                    }
                                }
                                else
                                {
                                    var msg = _resourceManager.GetString("Message11");
                                    Mediator.GetInstance().OnGeneralError(this, msg);
                                }
                            }
                            else
                            {
                                var msg = $"{_resourceManager.GetString("Message12")} - {hanel.Name}";
                                Mediator.GetInstance().OnGeneralError(this, msg);
                            }
                        }
                        else
                        {
                            var msg = _resourceManager.GetString("Message13");
                            Mediator.GetInstance().OnGeneralError(this, msg);
                        }
                    }
                }
                else
                {
                    _logger.LogDetailAsync($"Location Access Denied").SafeFireAndForget();
                    var msg = "Location Access Denied";
                    Mediator.GetInstance().OnGeneralError(this, msg);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Move Device - Inventory Module: {Environment.NewLine}{ex.Message}").SafeFireAndForget();
            }
        }
        //private void ClearAllShi()
        //{
        //    if (_neutronVariables.DisplaysEnabled)
        //        if (GlobalVar.Displays != null)
        //            GlobalVar.Displays.ClearAllShi();
        //}
        private void MBPrintInventory_Click(object sender, EventArgs e)
        {
            CsvUtility.SaveToCsv(DataGridView1);
        }
        private void DataGridViewInventoryLocations_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("Click");
        }
        private void DataGridView1_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("Click");
        }
        private async void CheckBoxAllStations_CheckedChanged(object sender, EventArgs e)
        {
            if (!_firstTime) await FindRecord();
        }

        private void SetCulture(string lang)
        {
            try
            {
                var languageDirectory = LoaderSettings.GetLanguageDirectory();
                _cultureInfo = CultureInfo.CreateSpecificCulture(lang);
                _resourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "FrmInventory",
               resourceDir: languageDirectory, usingResourceSet: null);
                _gridResourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "GridHeaders",
                    resourceDir: languageDirectory, usingResourceSet: null);
                LabelFormHeaderText.Text = _resourceManager.GetString("NeutronWarehouseMana");
                LabelFormTitle.Text = _resourceManager.GetString("Inventory");
                ButtonSaveToExcel.Text = _resourceManager.GetString("SaveToFile");
                //LabelFindDescription.Text = _resourceManager.GetString("SearchFor");
                MButtonNew.Text = _resourceManager.GetString("New");
                MButtonViewEdit.Text = _resourceManager.GetString("View/Edit");
                MButtonClose.Text = _resourceManager.GetString("Home");
                MButtonSearch.Text = _resourceManager.GetString("Search");
                MbViewEditEdit.Text = _resourceManager.GetString("EditLocation");
                MbViewEditAddLocation.Text = _resourceManager.GetString("AddLocation");
                MbViewEditListing.Text = _resourceManager.GetString("Listing");
                MbViewEditDelete.Text = _resourceManager.GetString("DeleteLocation");
                MbViewEditClose.Text = _resourceManager.GetString("Back");
                LabelViewEditDescription.Text = _resourceManager.GetString("Description");
                LabelViewEditItem.Text = _resourceManager.GetString("Item");
                MbNewAddLocation.Text = _resourceManager.GetString("AddLocation");
                LabelActionNew.Text = _resourceManager.GetString("NewInventoryItem");
                MbNewListing.Text = _resourceManager.GetString("Listing");
                MbNewClose.Text = _resourceManager.GetString("Back");
                MbNewFind.Text = _resourceManager.GetString("Find");
                LabelNewItem.Text = _resourceManager.GetString("EnterItemNumber");
                LabelNewUnitOfIssue.Text = _resourceManager.GetString("UnitOfIssue");
                LabelNewStorageType.Text = _resourceManager.GetString("StorageType");
                LabelNewWeight.Text = _resourceManager.GetString("Weight");
                CheckBoxNewScale.Text = _resourceManager.GetString("UseScale");
                LabelNewSystemMin.Text = _resourceManager.GetString("SystemMin");
                LabelNewLocationCode.Text = _resourceManager.GetString("LocationCode");
                LabelNewHeight.Text = _resourceManager.GetString("Height");
                LabelNewVelocity.Text = _resourceManager.GetString("Velocity");
                LabelNewSize.Text = _resourceManager.GetString("Size");
                LabelNewSystemMax.Text = _resourceManager.GetString("SystemMax");
                LabelNewLocationMin.Text = _resourceManager.GetString("LocationMin");
                LabelNewLocationMax.Text = _resourceManager.GetString("LocationMax");
                LabelNewDescriiption.Text = _resourceManager.GetString("Description");
                LabelNewArea.Text = _resourceManager.GetString("Area");
                LabelActionNewLocations.Text = _resourceManager.GetString("NewLocations");
                MbNewLocationsListing.Text = _resourceManager.GetString("Listing");
                MbNewAvailableLocations.Text = _resourceManager.GetString("ShowAll");
                MbNewLocationsClose.Text = _resourceManager.GetString("Back");
                MbNewLocationsSave.Text = _resourceManager.GetString("Save");
                CheckBoxInventoryNewLocationsPrimeBin.Text = _resourceManager.GetString("PrimeBin");
                LabelNewLocationsStorageType.Text = _resourceManager.GetString("StorageType");
                LabelNewLocationsReceivedDate.Text = _resourceManager.GetString("ReceivedDate");
                LabelNewLocationsQuantity.Text = _resourceManager.GetString("Qty");
                LabelNewLocationsDescription.Text = _resourceManager.GetString("Description");
                LabelNewLocationsItem.Text = _resourceManager.GetString("Item");
                LabelActionAddDetail.Text = _resourceManager.GetString("AddDetail");
                MbAddDetailListing.Text = _resourceManager.GetString("Listing");
                MbAddDetailClose.Text = _resourceManager.GetString("Back");
                MbAddDetailSave.Text = _resourceManager.GetString("Save");
                CheckBoxAddDetailPrimeBin.Text = _resourceManager.GetString("PrimeBin");
                LabelAddDetailReceivedDate.Text = _resourceManager.GetString("ReceivedDate");
                LabelAddDetailDescription.Text = _resourceManager.GetString("Description");
                LabelAddDetailItem.Text = _resourceManager.GetString("Item");
                LabelAddDetailStorageType.Text = _resourceManager.GetString("StorageType");
                LabelAddDetailQuantity.Text = _resourceManager.GetString("Qty");
                GroupBoxAddDetailLocation.Text = _resourceManager.GetString("Location");
                CheckBoxInUse.Text = _resourceManager.GetString("InUse");
                LabelLocationCode.Text = _resourceManager.GetString("LocationCode");
                LabelAddDetailHeight.Text = _resourceManager.GetString("Height");
                LabelAddDetailVelocity.Text = _resourceManager.GetString("Velocity");
                LabelAddDetailSlot.Text = _resourceManager.GetString("Slot");
                LabelAddDetailSize.Text = _resourceManager.GetString("Size");
                LabelAddDetailTag.Text = _resourceManager.GetString("Tag");
                LabelAddDetailBack.Text = _resourceManager.GetString("Back");
                LabelAddDetailOver.Text = _resourceManager.GetString("Over");
                LabelAddDetailTray.Text = _resourceManager.GetString("Tray");
                LabelAddDetailDevice.Text = _resourceManager.GetString("Device");
                LabelAddDetailArea.Text = _resourceManager.GetString("Area");
                ButtonPositionDevice.Text = _resourceManager.GetString("PositionDevice");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading language file.  {ex.Message} {Environment.NewLine} {ex.InnerException} ");
            }
        }
        private async void ButtonPositionDevice_Click(object sender, EventArgs e)
        {
            var deviceNumber = TextBoxAddDetailLoc1.Text.ParseInt();
            var trayNumber = TextBoxAddDetailLoc2.Text.ParseInt();
            var level = TextBoxAddDetailLoc3.Text.ParseInt();
            var part = TextBoxAddDetailLoc4.Text.ParseInt();
            var qty = TextBoxAddDetailQuantity.Text.ParseInt();
            _logger.LogDetailAsync($"Device: {deviceNumber} Tray: {trayNumber} Level: {level} Part: {part}").SafeFireAndForget();
            await MoveDevice(deviceNumber, trayNumber, level, part, qty, string.Empty);
        }
        private async void DataGridView1_DoubleClick(object sender, EventArgs e)
        {
            await LoadViewEdit();
        }
        private void DataGridViewInventoryLocations_DoubleClick(object sender, EventArgs e)
        {
            EditLocation();
        }

        public bool AllAvailable
        {
            get { return _allAvailable; }
            set
            {
                _allAvailable = value;
                MbNewAvailableLocations.Text = _allAvailable ? _resourceManager.GetString("Preferred") : _resourceManager.GetString("ShowAll");
            }
        }
        private void MbNewAvailableLocations_Click(object sender, EventArgs e)
        {
            AllAvailable = !_allAvailable;
            GetAvailable();
        }
        private void GetAvailable()
        {
            _processing = true;
            Cursor.Current = Cursors.WaitCursor;
            var inUse = 2; // 0 = all, 1 = in use, 2 = not in use
            if (RadioButtonShowAll.Checked) inUse = 0;            
            if (RadioButtonInUse.Checked) inUse = 1;
            if (RadioButtonNotInUse.Checked) inUse = 0;

            if (AllAvailable)
            {
                GetAllAvailableLocations(CurrentItem, inUse);
            }
            else
            {
                GetAvailableLocations(CurrentItem, inUse);
            }

            Cursor.Current = Cursors.Default;
            _processing = false;
        }
        private void ComboBoxNewAreaChoice_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_firstTimeCombo) return;

            var lookup = (LookUp)ComboBoxNewAreaChoice.SelectedItem;

            UpdateNewItem(lookup.Id);
        }

        private void RadioButtonShowAll_CheckedChanged(object sender, EventArgs e)
        {
            _logger.LogDetailAsync($"RadioButtonShowAll_CheckedChanged START").SafeFireAndForget();
            if (_processing) return;

            if (sender is RadioButton radioButton && radioButton.Checked)
            {
                // show the wait cursor
                Cursor.Current = Cursors.WaitCursor;
                _processing = true;
                var inUse = Convert.ToInt32(radioButton.Tag);
                if (AllAvailable)
                {
                    GetAllAvailableLocations(CurrentItem, inUse);
                }
                else
                {
                    GetAvailableLocations(CurrentItem, inUse);
                }
                _processing = false;
                // show the default cursor
                Cursor.Current = Cursors.Default;
            }
            _logger.LogDetailAsync($"RadioButtonShowAll_CheckedChanged END").SafeFireAndForget();
        }

        private void RadioButtonInUse_CheckedChanged(object sender, EventArgs e)
        {
            _logger.LogDetailAsync($"RadioButtonInUse_CheckedChanged START").SafeFireAndForget();
            if (_processing) return;

            if (sender is RadioButton radioButton && radioButton.Checked)
            {
                _processing = true;
                Cursor.Current = Cursors.WaitCursor;
                var inUse = Convert.ToInt32(radioButton.Tag);
                if (AllAvailable)
                {
                    GetAllAvailableLocations(CurrentItem, inUse);
                }
                else
                {
                    GetAvailableLocations(CurrentItem, inUse);
                }
                _processing = false;
                Cursor.Current = Cursors.Default;
            }
            _logger.LogDetailAsync($"RadioButtonInUse_CheckedChanged END").SafeFireAndForget();
        }

        private void RadioButtonNotInUse_CheckedChanged(object sender, EventArgs e)
        {
            _logger.LogDetailAsync($"RadioButtonNotInUse_CheckedChanged START").SafeFireAndForget();
            if (_processing) return;

            if (sender is RadioButton radioButton && radioButton.Checked)
            {
                _processing = true;
                Cursor.Current = Cursors.WaitCursor;
                var inUse = Convert.ToInt32(radioButton.Tag);
                if (AllAvailable)
                {
                    GetAllAvailableLocations(CurrentItem, inUse);
                }
                else
                {
                    GetAvailableLocations(CurrentItem, inUse);
                }
                _processing = false;
                Cursor.Current = Cursors.Default;
            }
            _logger.LogDetailAsync($"RadioButtonNotInUse_CheckedChanged END").SafeFireAndForget();
        }

        private List<SqlInventoryView> GetSelectedItems(DataGridView dataGridView)
        {
            // Create a list of SqlInventoryView
            var selectedList = new List<SqlInventoryView>();
            // Loop through the selected rows
            foreach (DataGridViewRow row in dataGridView.SelectedRows)
            {
                // Get the SqlInventoryView from the row  ((ObjectView<SqlInventoryView>)_bindingSource.Current).Object;
                var sqlInventoryView = ((ObjectView<SqlInventoryView>)row.DataBoundItem).Object;
                // Add the SqlInventoryView to the list
                selectedList.Add(sqlInventoryView);
            }
            return selectedList;
        }

        private void ButtonSaveToExcel_Click(object sender, EventArgs e)
        {
            ButtonLoadFromExcel.Enabled = false;
            ButtonSaveToExcel.Enabled = false;
            Cursor.Current = Cursors.WaitCursor;
            SaveToExcel();
        }

        private void SaveToExcel()
        {
            _logger.LogDetailAsync("Saving records to Excel spreadsheet").SafeFireAndForget();

            DataTable dataTable;
            // Initialize the Excel Service
            var excelService = new ExcelService();
            if (CheckBoxUseSelectedItems.Checked)
            {
                var selectedList = GetSelectedItems(DataGridView1);
                // Create a DataTable from the List(Of T) (SqlInventoryView)
                dataTable = ToDataTable<SqlInventoryView>(selectedList);
            }
            else
            {
                // Create a DataTable from the List(Of T) (SqlInventoryView)
                dataTable = ToDataTable<SqlInventoryView>(_currentList);

            }

            // Generate the Excel file
            excelService.Generate(dataTable);
            _logger.LogDetailAsync($"Saved {dataTable.Rows.Count} records to Excel spreadsheet").SafeFireAndForget();
            ButtonLoadFromExcel.Enabled = true;
            ButtonSaveToExcel.Enabled = true;
            Cursor.Current = Cursors.Default;
        }

        private void ButtonLoadFromExcel_Click(object sender, EventArgs e)
        {
            ButtonLoadFromExcel.Enabled = false;
            ButtonSaveToExcel.Enabled = false;
            Cursor.Current = Cursors.WaitCursor;
            LoadFromExcel();
        }

        private void LoadFromExcel()
        {
            _logger.LogDetailAsync("Loading records from Excel spreadsheet").SafeFireAndForget();
            var excelService = new ExcelService();
            var dataTable = excelService.Update();
            BackgroundWorkerInventory.RunWorkerAsync(dataTable);
        }
        public DataTable ToDataTable<T>(List<T> items)
        {
            var dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var action = new DataColumn("Action", typeof(string));
            dataTable.Columns.Add(action);


            foreach (var prop in props)
            {
                //Setting column names as Property names
                var col = new DataColumn(prop.Name, prop.PropertyType);
                dataTable.Columns.Add(prop.Name, prop.PropertyType);
            }
            foreach (var item in items)
            {
                // add 1 to the props.Length to account for the Action column
                var values = new object[props.Length + 1];
                // set the default action to [M]odify
                values[0] = "M";
                // starting at 1 to skip the Action column
                for (var i = 0; i < props.Length; i++)
                {
                    //inserting property values to dataTable rows
                    values[i + 1] = props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check dataTable
            return dataTable;
        }

        private async void ComboBoxAreaNumber_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_startup)
            {
                DataGridView1.Columns[0].Visible = _workstationView.AreaId != 8 &&
                                   _workstationView.AreaId == (int)ComboBoxAreaNumber.SelectedValue;
                await RefreshData();
            }
        }

        private async void BackgroundWorkerItemDefinitions_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!(sender is BackgroundWorker worker)) return;
            if (!(e.Argument is DataTable dataTable)) return;
            var rowCount = dataTable.Rows.Count;
            var processedCount = 0;
            // loop over the rows in the DataTable
            _logger.LogDetailAsync($"Loading {rowCount} records from Excel spreadsheet").SafeFireAndForget();

            try
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    // get the values from the row
                    var id = row["Id"].ToString();
                    // Check to see if this row contains the header record
                    // if so, skip it
                    // Id is the first column in the spreadsheet
                    if (id.Equals("Id")) continue;
                    var item = row["Item"].ToString();
                    var description = row["Description"].ToString();
                    var quantity = row["Quantity"].ToString();
                    var primeBin = row["PrimeBin"].ToString();
                    var loc1 = row["Loc1"].ToString();
                    var loc2 = row["Loc2"].ToString();
                    var loc3 = row["Loc3"].ToString();
                    var loc4 = row["Loc4"].ToString();
                    var loc5 = row["Loc5"].ToString();
                    var slot = row["Slot"].ToString();
                    var pickSequence = row["PickSequence"].ToString();
                    var inUse = row["InUse"].ToString();
                    var receivedDate = row["ReceivedDate"].ToString();
                    var areaId = row["AreaId"].ToString();
                    var areaName = row["AreaName"].ToString();
                    var storageTypeId = row["StorageTypeId"].ToString();
                    var storageTypeName = row["StorageTypeName"].ToString();
                    var sizeCodeId = row["SizeCodeId"].ToString();
                    var sizeCodeName = row["SizeCodeName"].ToString();
                    var velocityCodeId = row["VelocityCodeId"].ToString();
                    var velocityCodeName = row["VelocityCodeName"].ToString();
                    var heightCodeId = row["HeightCodeId"].ToString();
                    var heightCodeName = row["HeightCodeName"].ToString();
                    var locationCode = row["LocationCode"].ToString();
                    var itemDefinitionId = row["ItemDefinitionId"].ToString();
                    var locationId = row["LocationId"].ToString();
                    var unitOfIssueId = row["UnitOfIssueId"].ToString();
                    var unitOfIssueName = row["UnitOfIssueName"].ToString();
                    var locationMax = row["LocationMax"].ToString();
                    var rfid = row["RFID"].ToString();


                    // if the Id is empty, this is a new row
                    // create a new Inventory object
                    if (string.IsNullOrWhiteSpace(id))
                    {
                        var inventory = new Inventory
                        {
                            ItemDefinitionId = id.ParseInt(),
                            LocationId = locationId.ParseInt(),
                            Quantity = quantity.ParseInt(),
                            ReceivedDate = DateTime.TryParse(receivedDate, out var date) ? date : DateTime.Now,
                            PrimeBin = primeBin.ParseInt() == 1 ? true : false,
                            AreaId = areaId.ParseInt(),
                            StorageTypeId = (int)GetEnumValue<StorageType>(storageTypeName),
                            RFID = rfid,
                        };
                        // add the new Inventory object to the database
                        await _repoInventory.InsertAsync(inventory);
                    }
                    else
                    {
                        // get the existing Inventory object
                        var inventory = await _repoInventory.FindByKeyAsync(id.ParseInt());
                        if (inventory == null) continue;
                        // update the values
                        inventory.Quantity = quantity.ParseInt();
                        inventory.ReceivedDate = DateTime.TryParse(receivedDate, out var date) ? date : DateTime.Now;
                        inventory.PrimeBin = primeBin.ParseInt() == 1 ? true : false;
                        inventory.StorageTypeId = (int)GetEnumValue<StorageType>(storageTypeName);
                        inventory.RFID = rfid;
                        // update the database
                        await _repoInventory.UpdateAsync(inventory);
                    }
                    // Update the progress
                    processedCount++;
                    var progressPercentage = (int)((double)processedCount / rowCount * 100);
                    if (progressPercentage % 25 == 0)
                    {
                        _logger.LogDetailAsync($"Loading {progressPercentage}% complete").SafeFireAndForget();
                        worker.ReportProgress(progressPercentage);
                    }
                }
                Mediator.GetInstance().OnDisplayMessage(this, $"Load complete");
            }
            catch (Exception ex)
            {
                Mediator.GetInstance().OnGeneralError(this, $"Error Adding/Updating Records{Environment.NewLine}{ex.Message}");
            }
        }

        public static T GetEnumValue<T>(string str) where T : struct
        {
            return (T)Enum.Parse(typeof(T), str);
        }

        private void BackgroundWorkerItemDefinitions_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            ProgressBarInventory.Value = e.ProgressPercentage;
        }

        private async void BackgroundWorkerItemDefinitions_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            await RefreshData();
            ProgressBarInventory.Value = 0;
            Cursor.Current = Cursors.Default;
            ButtonLoadFromExcel.Enabled = true;
            ButtonSaveToExcel.Enabled = true;
            _logger.LogDetailAsync("Loading records from Excel spreadsheet complete").SafeFireAndForget();
        }
    }
}
