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
using System.Text;
using System.Diagnostics;
using AsyncAwaitBestPractices;
using NeutronEvents;
using IPTI.Models;
using NeutronCore.StaticClasses;
using JetBrains.Annotations;
using NeutronData.Models.Lookups;
using RadioButton = System.Windows.Forms.RadioButton;
using Timer = System.Windows.Forms.Timer;
using NeutronData.UnitOfWorks;



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

        //private readonly GenericRepository<ItemDefinition> _repoItemDefinition =
        //    new GenericRepository<ItemDefinition>(new NeutronDb());

        //private readonly GenericRepository<Location> _repoLocation = new GenericRepository<Location>(new NeutronDb());
        //private readonly GenericRepository<SizeCode> _repoSizeCode = new GenericRepository<SizeCode>(new NeutronDb());

        //private readonly GenericRepository<VelocityCode> _repoVelocityCode =
        //    new GenericRepository<VelocityCode>(new NeutronDb());

        //private readonly GenericRepository<HeightCode> _repoHeightCode =
        //    new GenericRepository<HeightCode>(new NeutronDb());

        //private readonly GenericRepository<Inventory>
        //    _repoInventory = new GenericRepository<Inventory>(new NeutronDb());

        //private readonly GenericRepository<NeutronData.Models.Lookups.StorageType> _repoStorageType =
        //    new GenericRepository<NeutronData.Models.Lookups.StorageType>(new NeutronDb());

        //private readonly GenericRepository<UnitOfIssue> _repoUnitOfIssue =
        //    new GenericRepository<UnitOfIssue>(new NeutronDb());

        //private readonly GenericRepository<Area> _repoArea =
        //    new GenericRepository<Area>(new NeutronDb());

        private IInventoryUnitOfWork _inventoryUnitOfWork;

        private IWorkstationRepository _workstationRepository;

        private ILocationsRepository _locationsRepository;

        private InventoryRepository _inventoryRepository;
        private IJsonData _jsonData;
        private NeutronVariables _neutronVariables;
        private IAreaRepository _areaRepository;
        private IRFIDManager _rfidManager;
        private WorkstationView _workstationView;
        private IAkaRepository _akaRepository;
        private ILacProcessor _lacProcessor;
        public ItemDefinition CurrentItem { get; set; }
        public Location CurrentLocation { get; set; }
        public Inventory CurrentInventoryItem { get; set; }
        public bool CloseButtonPressed { get; set; }
        private bool _firstTimeCombo = true;
        private bool _processing = false;

        private IDynamicLogger _logger;
        private HeaderTextManager _headerTextManager;
        private List<SqlInventoryView> _currentList;
        private bool _startup = true;
        private bool _lookupsLoaded;
        private IIptiDisplayFunctions _iptiDisplayFunctions;
        private const int AreaEight = AreaNumber.Eight;
        private ToolTip _toolTip;
        private Timer _clickTimer; // Timer to delay single-click action
        private const int DoubleClickTimeout = 300; // Timeout in milliseconds
        private bool _doubleClickDetected; // Flag to track double-clicks
        private readonly int[] _moveableAreas = new int[] { 1, 2, 3, 4 };
        private ItemDefinition _workingItemDefinition = null;
        private string _newOrViewEdit = "New";
        private readonly Func<NeutronDb> _contextFactory;
        private ToolTip _toolTip;

        public FrmInventory(IJsonData jsonData, IAkaRepository akaRepository,
                ILacProcessor lacProcessor, IWorkstationRepository workstationRepository
                , WorkstationView workstationView, NeutronVariables neutronVariables
                , IAreaRepository areaRepository
                , IRFIDManager rfidManager, ILocationsRepository locationsRepository
                , IInventoryUnitOfWork inventoryUnitOfWork
                , IIptiDisplayFunctions iptiDisplayFunctions, Func<NeutronDb> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            Console.WriteLine($"1: {DateTime.Now}");
            _jsonData = jsonData;
            _akaRepository = akaRepository;
            _lacProcessor = lacProcessor;
            _workstationRepository = workstationRepository;
            _workstationView = workstationView;
            _neutronVariables = neutronVariables;
            _areaRepository = areaRepository;
            _rfidManager = rfidManager;
            _locationsRepository = locationsRepository;
            Console.WriteLine($"2: {DateTime.Now}");
            _inventoryUnitOfWork = inventoryUnitOfWork;
            Console.WriteLine($"3: {DateTime.Now}");
            _iptiDisplayFunctions = iptiDisplayFunctions;
            KeyPreview = true;
            CloseButtonPressed = false;
            _toolTip = new ToolTip();
            Console.WriteLine($"4: {DateTime.Now}");
            //Shown += async (sender, args) => await LoadLookupDataAsync();
            Init();

        }


        //public static async Task<FrmInventory> CreateAsync(IJsonData jsonData, IAkaRepository akaRepository,
        //    ILacProcessor lacProcessor, IWorkstationRepository workstationRepository
        //    , WorkstationView workstationView, NeutronVariables neutronVariables
        //    , IAreaRepository areaRepository
        //    , IRFIDManager rfidManager, ILocationsRepository locationsRepository
        //    , IInventoryUnitOfWork inventoryUnitOfWork, IIptiDisplayFunctions iptiDisplayFunctions)
        //{
        //    var frm = new FrmInventory(jsonData, akaRepository, lacProcessor, workstationRepository, workstationView,
        //        neutronVariables, areaRepository, rfidManager, locationsRepository, inventoryUnitOfWork, iptiDisplayFunctions);



        //    await frm.Init(jsonData, akaRepository, lacProcessor, workstationRepository, workstationView, neutronVariables, areaRepository, rfidManager, locationsRepository, inventoryUnitOfWork, iptiDisplayFunctions);
        //    return frm;
        //}

        private async void Init()
        {
            InitializeStationName();
            InitializeHeaderTextManager();
            ConfigureTabControl();
            ConfigureForms();
            ConfigureUserInfo();
            InitializeLogger();
            InitializeInventoryRepository();
            ConfigureRfidControls();
            await LoadLookupDataAsync();

            SetupGrids();
            // Load saved column widths
            SetColumnWidths(DataGridView1, Properties.Settings.Default.DataGridView1ColumnWidths);
            SetColumnWidths(DataGridViewInventoryLocations, Properties.Settings.Default.DataGridViewInventoryLocationsColumnWidths);
            SetColumnWidths(DataGridViewInventoryNewLocations, Properties.Settings.Default.DataGridViewInventoryNewLocationsColumnWidths);


            SetupRadioButtons();
            _toolTip = new ToolTip();
        // Initialize the timer
        _clickTimer = new Timer
            {
                Interval = DoubleClickTimeout
            };
            _clickTimer.Tick += ClickTimer_Tick;

            _logger.LogDetailAsync("Init Finished - Start Refresh").SafeFireAndForget();
            //Task.Run( async () =>
            //{
            //    await RefreshData();
            //}).Wait();
             RefreshData();
            _startup = false;

        }

        private async Task LoadLookupDataAsync()
        {
           // if (_lookupsLoaded) return;
            _lookupsLoaded = true;

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                var sizeCodesTask = Task.Run(() => _inventoryUnitOfWork.SizeCodes.All().ToList());
                var velocityCodesTask = Task.Run(() => _inventoryUnitOfWork.VelocityCodes.All().ToList());
                var heightCodesTask = Task.Run(() => _inventoryUnitOfWork.HeightCodes.All().ToList());
                var storageTypesTask = Task.Run(() => _inventoryUnitOfWork.StorageTypes.All().ToList());
                var unitOfIssueTask = Task.Run(() => _inventoryUnitOfWork.UnitOfIssue.All().ToList());
                var areaNumbersTask = Task.Run(() => _inventoryUnitOfWork.Areas.All().ToList());
                var addDetailAreasTask = Task.Run(() => _areaRepository.Lookup().ToList());

                await Task.WhenAll(sizeCodesTask, velocityCodesTask, heightCodesTask, storageTypesTask, unitOfIssueTask, areaNumbersTask, addDetailAreasTask);

                var sizeCodes = sizeCodesTask.Result;
                var velocityCodes = velocityCodesTask.Result;
                var heightCodes = heightCodesTask.Result;
                var storageTypes = storageTypesTask.Result;
                var unitOfIssues = unitOfIssueTask.Result;
                var areaNumbers = areaNumbersTask.Result;
                var addDetailAreas = addDetailAreasTask.Result;

                ComboBoxNewSizeCode.DisplayMember = "Name";
                ComboBoxNewSizeCode.ValueMember = "Id";                
                ComboBoxNewSizeCode.DataSource = sizeCodes;

                ComboBoxAddDetailSizeCode.DisplayMember = "Name";
                ComboBoxAddDetailSizeCode.ValueMember = "Id";
                ComboBoxAddDetailSizeCode.DataSource = sizeCodes;

                ComboBoxNewVelocityCode.DisplayMember = "Name";
                ComboBoxNewVelocityCode.ValueMember = "Id";
                ComboBoxNewVelocityCode.DataSource = velocityCodes;

                ComboBoxAddDetailVelocityCode.DisplayMember = "Name";
                ComboBoxAddDetailVelocityCode.ValueMember = "Id";
                ComboBoxAddDetailVelocityCode.DataSource = velocityCodes;

                ComboBoxNewHeightCode.DisplayMember = "Name";
                ComboBoxNewHeightCode.ValueMember = "Id";
                ComboBoxNewHeightCode.DataSource = heightCodes;

                ComboBoxAddDetailHeightCode.DisplayMember = "Name";
                ComboBoxAddDetailHeightCode.ValueMember = "Id";
                ComboBoxAddDetailHeightCode.DataSource = heightCodes;

                ComboBoxNewStorageType.DisplayMember = "Name";
                ComboBoxNewStorageType.ValueMember = "Id";
                ComboBoxNewStorageType.DataSource = new List<NeutronData.Models.Lookups.StorageType>(storageTypes);

                ComboBoxInventoryNewLocationsStorageType.DisplayMember = "Name";
                ComboBoxInventoryNewLocationsStorageType.ValueMember = "Id";
                ComboBoxInventoryNewLocationsStorageType.DataSource = new List<NeutronData.Models.Lookups.StorageType>(storageTypes);

                ComboBoxAddDetailStorageType.DisplayMember = "Name";
                ComboBoxAddDetailStorageType.ValueMember = "Id";
                ComboBoxAddDetailStorageType.DataSource = new List<NeutronData.Models.Lookups.StorageType>(storageTypes);

                ComboBoxNewUnitOfIssue.DisplayMember = "Name";
                ComboBoxNewUnitOfIssue.ValueMember = "Id";
                ComboBoxNewUnitOfIssue.DataSource = unitOfIssues;

                ComboBoxAreaNumber.ValueMember = "Id";
                ComboBoxAreaNumber.DisplayMember = "Name";
                ComboBoxAreaNumber.DataSource = areaNumbers;

                ComboBoxAreaNumber.SelectedIndex = 0;

                if (_workstationView.StationType.Id == (int)StationType.Supervisor)
                {
                    ComboBoxAreaNumber.SelectedIndex = ComboBoxAreaNumber.FindStringExact("All Areas");
                }
                else
                {
                    ComboBoxAreaNumber.SelectedValue = _workstationView.AreaId;
                }
                ComboBoxAddDetailArea.DisplayMember = "Name";
                ComboBoxAddDetailArea.ValueMember = "Id";
                ComboBoxAddDetailArea.DataSource = addDetailAreas;

            }
            catch (Exception ex)
            {
                var message = $"Error loading lookup data: {Environment.NewLine}{ex.Message}";
                _logger.LogDetailAsync(message).SafeFireAndForget();
                Mediator.GetInstance().OnGeneralError(this, message);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
        private void SetColumnWidths(DataGridView grid, string widthsString)
        {
            if (string.IsNullOrEmpty(widthsString)) return;

            var pairs = widthsString.Split(';');
            foreach (var pair in pairs)
            {
                var parts = pair.Split(':');
                if (parts.Length == 2 && int.TryParse(parts[1], out int width))
                {
                    var col = grid.Columns[parts[0]];
                    if (col != null)
                    {
                        col.Width = width;
                    }
                }
            }
        }
        private void SetupRadioButtons()
        {
            if ((int)ComboBoxAreaNumber.SelectedValue == AreaEight || (int)ComboBoxAreaNumber.SelectedValue == 99)
            {
                RadioButtonAllLocations.Checked = true;
            }
            else
            {
                RadioButtonExactMatch.Checked = true;
            }
        }

        private void InitializeStationName()
        {
            if (LabelStationName.InvokeRequired)
            {
                LabelStationName.Invoke(new Action(() => { LabelStationName.Text = _workstationView.ToString(); }));
            }
            else
            {
                LabelStationName.Text = _workstationView.ToString();
            }

        }

        private void InitializeHeaderTextManager()
        {
            _headerTextManager = new HeaderTextManager();
        }

        private void ConfigureTabControl()
        {
            HideTabControlTabs();
        }

        private void ConfigureForms()
        {
            SetupNewForm();
            SetupViewEditForm();
            SetupAddDetailForm();
        }

        private void ConfigureUserInfo()
        {
            if (mlUserInfo.InvokeRequired)
            {
                mlUserInfo.Invoke(new Action(() => { mlUserInfo.Text = GlobalVar.User?.UserInfo; }));
            }
            else
            {
                mlUserInfo.Text = GlobalVar.User?.UserInfo;
            }
        }

        private void InitializeLogger()
        {
            _logger = NeutronCore.Global.Logger.SetupLogger("Inventory");
        }

        private void InitializeInventoryRepository()
        {
            _inventoryRepository = new InventoryRepository(_contextFactory, _logger);
        }

        private void ConfigureRfidControls()
        {
            var isRfidEnabled = _neutronVariables.RfidEnabledInventory;
            LabelInventoryNewLocationsRfid.Visible = isRfidEnabled;
            TextBoxInventoryNewLocationsRfid.Visible = isRfidEnabled;
            LabelAddDetailRfid.Visible = isRfidEnabled;
            TextBoxAddDetailRfid.Visible = isRfidEnabled;
        }

        private void ConfigureAreaComboBox()
        {
            var areas = _inventoryUnitOfWork.Areas.All();
            ComboBoxAreaNumber.DataSource = areas;
            ComboBoxAreaNumber.ValueMember = "Id";
            ComboBoxAreaNumber.DisplayMember = "Name";
            ComboBoxAreaNumber.SelectedIndex = 0;
            if (_workstationView.StationType.Id == (int)StationType.Supervisor)
            {
                ComboBoxAreaNumber.SelectedIndex = ComboBoxAreaNumber.FindStringExact("All Areas");
            }
            else
            {
                ComboBoxAreaNumber.SelectedValue = _workstationView.AreaId;
            }
        }

        private async void ClickTimer_Tick(object sender, EventArgs e)
        {
            _clickTimer.Stop(); // Stop the timer
            if (!_doubleClickDetected) // If no double-click was detected
            {
                // Perform single-click action
                var dataGridView = DataGridView1; // (DataGridView)sender;
                dataGridView.Enabled = false;
                try
                {
                    //Get the current RowIndex of DataGridView1
                    var rowIndex = dataGridView.CurrentCell.RowIndex;

                    await DataGridViewPosition(dataGridView, rowIndex);

                    SetCurrentInventoryItem();
                }
                finally
                {
                    dataGridView.Enabled = true;
                }
                //MessageBox.Show("Cell clicked (single-click)");
            }
        }


        /// <summary>
        /// CreateParams
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                var parms = base.CreateParams;
                parms.ExStyle |= 0x02000000; // Turn on WS_EX_COMPOSITED
                //parms.Style &= ~0x02000000;  // Turn off WS_CLIPCHILDREN
                return parms;
            }
        }

        private async Task RefreshData(int recId = 0)
        {
            Cursor.Current = Cursors.WaitCursor;
            _logger.LogDetailAsync("Start Refresh").SafeFireAndForget();
            var swTotal = Stopwatch.StartNew();
            var findWhat = TextBoxFind.Text.ToLower().Trim();
            TextBoxFind.Text = string.Empty;

            var aka = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(findWhat))
                {
                    aka = GetAkaValue(findWhat);
                }

                var find = string.IsNullOrWhiteSpace(aka) ? findWhat : aka;
                if (TextBoxFind.InvokeRequired)
                {
                    TextBoxFind.Invoke(new Action(() => TextBoxFind.Text = find));
                }
                else
                {
                    TextBoxFind.Text = find;
                }

                await LoadData(find, recId);
            }
            catch (Exception ex)
            {
                var message = $"Error Loading Data: \n{ex.Message}";
                _logger.LogDetailAsync(message).SafeFireAndForget();
                Mediator.GetInstance().OnGeneralError(this, message);
            }
            swTotal.Stop();
            _logger.LogDetailAsync($"RefreshData total: {swTotal.ElapsedMilliseconds} ms").SafeFireAndForget();
            _logger.LogDetailAsync("End Refresh").SafeFireAndForget();
            Cursor.Current = Cursors.Default;
        }

        private async Task LoadData(string find, int recId)
        {
            _logger.LogDetailAsync("Start LoadData").SafeFireAndForget();
            try
            {
                var swLoadData = Stopwatch.StartNew();
                Area area = null;
                if (ComboBoxAreaNumber.InvokeRequired)
                {
                    ComboBoxAreaNumber.Invoke(new Action(() =>
                    {
                        area = (Area)ComboBoxAreaNumber.SelectedItem;
                    }));
                }
                else
                {
                    area = (Area)ComboBoxAreaNumber.SelectedItem;
                }

                var swDb = Stopwatch.StartNew();
                var views = new List<SqlInventoryView>();
                
                if (area.Name == "All Areas")
                {
                   views = await _inventoryRepository.FindInventoryViews(find);
                }
                else
                {
                    views = await _inventoryRepository.FindInventoryViewsByArea(find, area.Id);
                }
               

                swDb.Stop();
                _logger.LogDetailAsync($"LoadData DB fetch: {swDb.ElapsedMilliseconds} ms").SafeFireAndForget();

                var localViews = new List<SqlInventoryView>(views);
                _currentList = localViews;

                var swBindPrep = Stopwatch.StartNew();
                var blv = new BindingListView<SqlInventoryView>(localViews.ToList());
                _logger.LogDetailAsync($"Inventory Count: {blv.Count}").SafeFireAndForget();
                _bindingSource.DataSource = blv;
                swBindPrep.Stop();
                _logger.LogDetailAsync($"LoadData binding prep (BLV/BindingSource): {swBindPrep.ElapsedMilliseconds} ms").SafeFireAndForget();

                var swSetDataSource = Stopwatch.StartNew();
                if (DataGridView1.InvokeRequired)
                {
                    DataGridView1.Invoke(new Action(() =>
                    {
                        DataGridView1.DataSource = _bindingSource;
                    }));
                }
                else
                {
                    DataGridView1.DataSource = _bindingSource;
                }
                swSetDataSource.Stop();
                _logger.LogDetailAsync($"LoadData set DataGridView.DataSource: {swSetDataSource.ElapsedMilliseconds} ms").SafeFireAndForget();
                _logger.LogDetailAsync("DataSource Loaded").SafeFireAndForget();
                if (GetRecordCount(_bindingSource) > 0)
                {
                    if (recId != 0)
                    {
                        var idx = IndexOf(_bindingSource, recId);
                        DataGridView1.FirstDisplayedScrollingRowIndex = DataGridView1.Rows[idx].Index;
                        DataGridView1.CurrentCell = DataGridView1.Rows[idx].Cells[1];
                        DataGridView1.Rows[idx].Selected = true;
                        SetCurrentInventoryItem();
                    }


                    //DataGridView1.Refresh();

                    _logger.LogDetailAsync("Fast Auto Size Start").SafeFireAndForget();
                    var swUiPost = Stopwatch.StartNew();
                    if (DataGridView1.InvokeRequired)
                    {
                        DataGridView1.Invoke(new Action(() =>
                        {
                           // if (DataGridView1.RowCount > 0) DataGridView1.FastAutoSizeColumns();
                            DataGridView1.ClearSelection();
                        }));
                    }
                    else
                    {
                       // if (DataGridView1.RowCount > 0) DataGridView1.FastAutoSizeColumns();
                        DataGridView1.ClearSelection();
                    }
                    swUiPost.Stop();
                    _logger.LogDetailAsync($"LoadData UI post-bind (ClearSelection etc.): {swUiPost.ElapsedMilliseconds} ms").SafeFireAndForget();
                    _logger.LogDetailAsync("Fast Auto Size End").SafeFireAndForget();

                }
                else
                {
                    CurrentItem = null;
                    CurrentLocation = null;
                    CurrentInventoryItem = null;
                }

                swLoadData.Stop();
                _logger.LogDetailAsync($"LoadData total: {swLoadData.ElapsedMilliseconds} ms").SafeFireAndForget();
            }
            catch (Exception ex)
            {
                var message = $"Error Loading Data: {Environment.NewLine}{ex.Message}";
                _logger.LogDetailAsync(message).SafeFireAndForget();
                Mediator.GetInstance().OnGeneralError(this, message);
            }
            _logger.LogDetailAsync("End Load Data").SafeFireAndForget();
        }

        private string GetAkaValue(string findWhat)
        {
            if (!string.IsNullOrEmpty(findWhat))
            {
                return _akaRepository.Get(findWhat);
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the Current Inventory Item, Location and Item Definition
        /// </summary>
        private void SetCurrentInventoryItem()
        {
            _logger.LogDetailAsync("Set Current Inventory Item").SafeFireAndForget();
            if (_bindingSource.Current == null)
            {
                SetCurrentInventoryItem(null);
                return;
            }


            var inventoryView = ((ObjectView<SqlInventoryView>)_bindingSource.Current).Object;
            var inv = _inventoryUnitOfWork.Inventory.FindByKey(inventoryView.Id);
            if (inv == null)
            {
                SetCurrentInventoryItem(null);
                return;
            }

            SetCurrentInventoryItem(inv);
            _logger.LogDetailAsync("Set Current Inventory Item Done").SafeFireAndForget();
        }

        private void SetCurrentInventoryItem([CanBeNull] Inventory inventory)
        {
            _logger.LogDetailAsync("Set Current Inventory Item With Inventory").SafeFireAndForget();
            if (inventory == null)
            {
                _logger.LogDetailAsync("Set Current Inventory Item With Inventory Null").SafeFireAndForget();
                CurrentLocation = null;
                CurrentItem = null;

            }
            else
            {
                CurrentInventoryItem = inventory;
                SetCurrentLocation(inventory.LocationId);
                SetCurrentItemDefinition(inventory.ItemDefinitionId);
            }

            _logger.LogDetailAsync("Set Current Inventory Item With Inventory Done").SafeFireAndForget();
        }

        /// <summary>
        /// Sets the current item definition based on the provided item definition ID.
        /// </summary>
        /// <param name="itemDefinitionId">
        /// The unique identifier of the item definition to be set.
        /// </param>
        /// <remarks>
        /// This method retrieves the item definition from the <see cref="IInventoryUnitOfWork.ItemDefinitions"/> repository
        /// using the specified <paramref name="itemDefinitionId"/>. If the item definition is found, it updates the 
        /// <see cref="CurrentItem"/> property and adjusts the checked radio button based on the item's area ID. 
        /// If the item definition is not found, <see cref="CurrentItem"/> is set to <c>null</c>.
        /// </remarks>
        private void SetCurrentItemDefinition(int itemDefinitionId)
        {
            var itemDefinition = _inventoryUnitOfWork.ItemDefinitions.FindByKey(itemDefinitionId);
            if (itemDefinition != null)
            {
                CurrentItem = itemDefinition;
                SetCheckedRadioButton(CurrentItem.AreaId);
                return;
            }
            CurrentItem = null;
        }
        private void SetCurrentItemDefinition(ItemDefinition itemDefinition)
        {
            if (itemDefinition != null)
            {
                CurrentItem = itemDefinition;
                SetCheckedRadioButton(CurrentItem.AreaId);
                return;
            }
            CurrentItem = null;

        }

        private void SetCheckedRadioButton(int areaId)
        {
            if (areaId == AreaEight || areaId == 99)
            {
                RadioButtonAllLocations.Checked = true;
            }
            else
            {
                RadioButtonExactMatch.Checked = true;
            }
        }

        /// <summary>
        /// Updates the current location of the inventory item based on the specified location identifier.
        /// </summary>
        /// <param name="locationId">
        /// The unique identifier of the location to be set as the current location.
        /// </param>
        /// <remarks>
        /// This method retrieves the location entity using the provided <paramref name="locationId"/> 
        /// from the <see cref="IInventoryUnitOfWork.Locations"/> repository and assigns it to 
        /// the <see cref="CurrentLocation"/> property.
        /// </remarks>
        private void SetCurrentLocation(int locationId)
        {
            var location = _inventoryUnitOfWork.Locations.FindByKey(locationId);
            CurrentLocation = location;
        }
        private void SetCurrentLocation(Location location)
        {
            if (location != null)
            {
                CurrentLocation = location;
            }
            else
            {
                CurrentLocation = null;
            }
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
            if (LabelRecordCount.InvokeRequired)
            {
                LabelRecordCount.Invoke(new Action(() =>
                {
                    LabelRecordCount.Text = $"{_resourceManager.GetString($"Records")}: {count}";
                }));
            }
            else
            {
                LabelRecordCount.Text = $"{_resourceManager.GetString($"Records")}: {count}";
            }
            if (LabelAvailableLocations.InvokeRequired)
            {
                LabelAvailableLocations.Invoke(new Action(() =>
                {
                    LabelAvailableLocations.Text = $"{_resourceManager.GetString($"Records")}: {count}";
                }));
            }
            else
            {
                LabelAvailableLocations.Text = $"{_resourceManager.GetString($"Records")}: {count}";
            }
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
                //RefreshData();
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
                    _iptiDisplayFunctions.ClearBlastzone();
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
            await LoadViewEdit();

            //UpdateViewEditForm();
            //var recs = await GetInventoryViewListByItem(CurrentItem.Id);
            //UpdateDataGridViewInventoryLocations(recs);
            //MbNewAvailableLocations.Text = "Refresh"; // _resourceManager.GetString("ShowAll");
            //tabControl1.SelectedTab = tabPage2;

        }

        private void UpdateDataGridViewInventoryLocations(List<SqlInventoryView> recs)
        {

            try
            {
                if (recs == null)
                {
                    throw new ArgumentNullException(nameof(recs), "The inventory records cannot be null.");
                }
                if (DataGridViewInventoryLocations == null)
                {
                    throw new InvalidOperationException("DataGridViewInventoryLocations is not initialized.");
                }
                if (DataGridViewInventoryLocations.InvokeRequired)
                {
                    DataGridViewInventoryLocations.Invoke(new Action(() => UpdateDataGridViewInventoryLocations(recs)));
                    return;
                }
                var blv = new BindingListView<SqlInventoryView>(recs);
                _locationBindingSource.DataSource = blv;
                DataGridViewInventoryLocations.DataSource = _locationBindingSource;
                DataGridViewInventoryLocations.ClearSelection();

            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine(ex);

                //throw;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex);
                //throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                //throw;
            }


            //var blv = new BindingListView<SqlInventoryView>(recs.ToList());
            //_locationBindingSource.DataSource = blv;
            //DataGridViewInventoryLocations.DataSource = _locationBindingSource;
            //DataGridViewInventoryLocations.ClearSelection();
        }

        private void UpdateViewEditForm()
        {
            if (CurrentItem != null)
            {
                TextBoxViewEditItem.Text = CurrentItem.Item;
                TextBoxViewEditDescription.Text = CurrentItem.Description;
            }
        }

        private async Task LoadViewEdit()
        {
            try
            {
                //SetCurrentInventoryItem();
                if (CurrentItem != null)
                {
                    TextBoxViewEditItem.Text = CurrentItem.Item;
                    TextBoxViewEditDescription.Text = CurrentItem.Description;
                    var views = await GetInventoryViewListByItemName(CurrentItem.Item);
                    var localViews = new List<SqlInventoryView>(views);
                    var blv = new BindingListView<SqlInventoryView>(localViews.ToList());
                    _locationBindingSource.DataSource = blv;
                    DataGridViewInventoryLocations.DataSource = _locationBindingSource;

                    DataGridViewInventoryLocations.ClearSelection();
                    tabControl1.SelectedTab = tabPage2;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Error during Load View Edit. {ex.Message}").SafeFireAndForget();
            }
        }

        private void MButtonNew_Click(object sender, EventArgs e)
        {
            ClearNewFields();
            if (!string.IsNullOrEmpty(TextBoxFind.Text))
            {
                TextBoxNewItem.Text = TextBoxFind.Text;
                TextBoxNewArea.Focus();
            }
            MbNewAvailableLocations.Text = _resourceManager.GetString($"ShowAll");
            tabControl1.SelectedTab = tabPage3;

            //if (DataGridView1.SelectedRows.Count == 0) return;
            //var row = DataGridView1.SelectedRows[0];
            //var inventoryId = (int)row.Cells["Id"].Value;
            //var inventoryView = _inventoryRepository.GetInventoryViewById(inventoryId);

            //await NewInventoryItem(inventoryView.Item);
        }

        private async Task NewInventoryItem(ItemDefinition itemDefinition)
        {
            ClearNewFields();

            if (itemDefinition != null)
            {
                TextBoxNewItem.Text = itemDefinition.Item;
                await NewItemFind(itemDefinition.Item, itemDefinition.AreaId);
            }
            else
            {
                await NewItemFind(null, 0);
            }

            MbNewAvailableLocations.Text = _resourceManager.GetString($"ShowAll");
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
            // var areaId = (int)ComboBoxNewAreaChoice.SelectedValue;
            var areaId = TextBoxNewArea.Text.ParseInt();

            await NewItemFind(TextBoxNewItem.Text, areaId);
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
                _iptiDisplayFunctions.ClearBlastzone();
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
            var inventory = _inventoryRepository.GetInventoryById(inventoryView.Id);
            SetCurrentInventoryItem(inventory);
            
            TextBoxAddDetailItem.Text = inventoryView.Item;
            TextBoxAddDetailDescription.Text = inventoryView.Description;
            DateTimePickerAddDetailReceivedDate.Value = inventoryView.ReceivedDate;
            TextBoxAddDetailInventoryId.Text = inventoryView.Id.ToString();
            TextBoxAddDetailLocationId.Text = inventoryView.LocationId.ToString();
            TextBoxAddDetailQuantity.Text = inventoryView.Quantity.ToString();
            TextBoxAddDetailItemDefinitionId.Text = inventoryView.ItemDefinitionId.ToString();
            ComboBoxAddDetailArea.SelectedValue = inventoryView.AreaId;
            TextBoxAddDetailArea.Text = inventoryView.AreaId.ToString();
            CheckBoxAddDetailPrimeBin.Checked = inventoryView.PrimeBin;
            TextBoxAddDetailLoc1.Text = inventoryView.Loc1.ToString();
            TextBoxAddDetailLoc2.Text = inventoryView.Loc2.ToString();
            TextBoxAddDetailLoc3.Text = inventoryView.Loc3.ToString();
            TextBoxAddDetailLoc4.Text = inventoryView.Loc4.ToString();
            TextBoxAddDetailLoc5.Text = inventoryView.Loc5.ToString();
            TextBoxAddDetailSlot.Text = inventoryView.Slot;
            TextBoxPickSequence.Text = inventoryView.PickSequence.ToString();
            ComboBoxAddDetailStorageType.SelectedValue = inventoryView.StorageTypeId;
            ComboBoxAddDetailSizeCode.SelectedValue = inventoryView.SizeCodeId;
            ComboBoxAddDetailVelocityCode.SelectedValue = inventoryView.VelocityCodeId;
            ComboBoxAddDetailHeightCode.SelectedValue = inventoryView.HeightCodeId;
            TextBoxAddDetailLocationCode.Text = inventoryView.LocationCode;
            TextBoxAddDetailLotNumber.Text = inventoryView.LotNumber;
            if (inventory.ExpirationDate.HasValue)
            {
                DateTimePickerAddDetailExpirationDate.Value = inventory.ExpirationDate.Value;
                DateTimePickerAddDetailExpirationDate.Checked = true;
            }
            else
            {
                DateTimePickerAddDetailExpirationDate.Value = DateTime.Now; // Set Value first
                DateTimePickerAddDetailExpirationDate.Checked = false; // Then uncheck after
            }
            CheckBoxInUse.Checked = inventoryView.InUse;
            LabelActionAddDetail.Text = _resourceManager.GetString($"EditDetail");
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
            return _moveableAreas.Contains(_workstationView.AreaId) && _moveableAreas.Contains((int)ComboBoxAreaNumber.SelectedValue) && _workstationView.AreaId == (int)ComboBoxAreaNumber.SelectedValue;
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
                    MessageBox.Show(_resourceManager.GetString($"Message0"));
                    return;
                }

                if (await IsDuplicate(rec))
                {
                    MessageBox.Show(_resourceManager.GetString($"Message1"));
                    return;
                }

                await _inventoryUnitOfWork.Inventory.InsertAsync(rec);
                var inventoryView = _inventoryRepository.GetInventoryViewById(rec.Id);
                await GlobalVar.HistoryManager.SaveHistoryAsync(ActionCode.InventoryAdd, inventoryView);

                await RefreshData(rec.Id);

                tabControl1.SelectedTab = tabPage1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(_resourceManager.GetString($"Message3") + ex.Message);
            }
        }

        private async Task UpdateViewEdit()
        {
            var currentInventoryView = (((ObjectView<SqlInventoryView>)_bindingSource.Current).Object);
            // convert SqlInventoryView to Inventory
            var inventory = _inventoryUnitOfWork.Inventory.FindByKey(currentInventoryView.Id);
            if (inventory == null) return;

            await GlobalVar.HistoryManager.SaveHistoryAsync(ActionCode.InventoryModify, inventory, inventory.Quantity,
                true);

            var beginningQuantity = 0;
            var rec = new Inventory
            {
                Id = currentInventoryView.Id,
                LocationId = currentInventoryView.LocationId,
                ItemDefinitionId = currentInventoryView.ItemDefinitionId,
                LotNumber = TextBoxAddDetailLotNumber.Text,
                ExpirationDate = DateTimePickerAddDetailExpirationDate.Checked
                    ? (DateTime?)DateTimePickerAddDetailExpirationDate.Value
                    : null
            };
            if (!ValidateFields(rec))
            {
                MessageBox.Show(_resourceManager.GetString($"Message4"));
                return;
            }

            await _inventoryUnitOfWork.Inventory.UpdateAsync(rec);
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
            var pattern = $"[^a-zA-Z]";
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
            var pattern = $"^[0-9]+$";
            if (Regex.IsMatch(input.ToString(), pattern))
            {
                if (input <= 0)
                {
                    MessageBox.Show(_resourceManager.GetString($"Message4"));
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
            var rec = await _inventoryUnitOfWork.Inventory
                .FindByFirstOrDefaultAsync(f =>
                    f.LocationId == recIn.LocationId && f.ItemDefinitionId == recIn.ItemDefinitionId);
            if (rec == null) return false;
            MessageBox.Show(_resourceManager.GetString($"Message5"), string.Empty, MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return true;
        }

        #region Form Setup Grids

        private void SetupGrids()
        {
            DataGridView1.AutoGenerateColumns = false;
            DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridView1.AllowUserToAddRows = false;
            DataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            DataGridView1.AllowUserToResizeColumns = true; // Enable resizing for DataGridView1
            DataGridViewInventoryLocations.AllowUserToResizeColumns = true; // For other grids
            DataGridViewInventoryNewLocations.AllowUserToResizeColumns = true;
            DataGridView1.ScrollBars = ScrollBars.Both;
            DataGridViewInventoryLocations.ScrollBars = ScrollBars.Both;
            DataGridViewInventoryNewLocations.ScrollBars = ScrollBars.Both;
            // All columns
            foreach (DataGridViewColumn column in DataGridView1.Columns)
            {
                column.MinimumWidth = 50;
            }
            
            var position = _gridResourceManager.GetString($"Position");
            DataGridViewButtonColumn bCol = new DataGridViewButtonColumn
            {
                HeaderText = _gridResourceManager.GetString($""),
                Visible = _moveableAreas.Contains(_workstationView.AreaId) 
                          && _moveableAreas.Contains((int)ComboBoxAreaNumber.SelectedValue) 
                          && _workstationView.AreaId == (int)ComboBoxAreaNumber.SelectedValue,
                Name = $"Position",
                Text = position,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                UseColumnTextForButtonValue = true
            };
            DataGridView1.Columns.Add(bCol);
            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "AreaName",
                HeaderText = _gridResourceManager.GetString($"Area"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "AreaName"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = _gridResourceManager.GetString($"Item"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Item"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = _gridResourceManager.GetString($"Description"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Description"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = _gridResourceManager.GetString($"Quantity"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Quantity"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Slot",
                HeaderText = _gridResourceManager.GetString($"Slot"),
                Visible = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Slot"
            };
            DataGridView1.Columns.Add(col);
            var xcol = new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "PrimeBin",
                HeaderText = _gridResourceManager.GetString($"PrimeBin"),
                Visible = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "PrimeBin"
            };
            DataGridView1.Columns.Add(xcol);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc1",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc1", _gridResourceManager),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc1"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc2",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc2", _gridResourceManager),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc2"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc3",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc3", _gridResourceManager),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc3"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc4",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc4", _gridResourceManager),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc4"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc5",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc5", _gridResourceManager),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc5",
                Visible = false
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SizeCodeName",
                HeaderText = _gridResourceManager.GetString($"SizeCodeName"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "SizeCodeName"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "VelocityCodeName",
                HeaderText = _gridResourceManager.GetString($"VelocityCodeName"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "VelocityCodeName"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "HeightCodeName",
                HeaderText = _gridResourceManager.GetString($"HeightCodeName"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "HeightCodeName"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LocationCode",
                HeaderText = _gridResourceManager.GetString($"LocationCode"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "LocationCode",
                Visible = false
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StorageTypeName",
                HeaderText = _gridResourceManager.GetString($"StorageTypeName"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "StorageTypeName"
            };
            DataGridView1.Columns.Add(col);


            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ReceivedDate",
                HeaderText = _gridResourceManager.GetString($"ReceivedDate"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "ReceivedDate"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PickSequence",
                HeaderText = $"Pick Sequence", // _gridResourceManager.GetString("Slot"),
                Visible = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "PickSequence"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LotNumber",
                HeaderText = $"Lot Number", 
                Visible = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "LotNumber"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ExpirationDate",
                HeaderText = $"Expiration Date", 
                Visible = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "ExpirationDate"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = _gridResourceManager.GetString($"Id"),
                Visible = false,
                Name = "Id"
            };
            DataGridView1.Columns.Add(col);

            DataGridView1.EnableHeadersVisualStyles = false;
            DataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            DataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);
           
            //Location Grid
            DataGridViewInventoryLocations.AutoGenerateColumns = false;
            DataGridViewInventoryLocations.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewInventoryLocations.AllowUserToAddRows = false;
            DataGridViewInventoryLocations.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            // All columns
            foreach (DataGridViewColumn column in DataGridViewInventoryLocations.Columns)
            {
                column.MinimumWidth = 50;
            }

            bCol = new DataGridViewButtonColumn
            {
                //HeaderText = _gridResourceManager.GetString("Id"),
                Visible = _moveableAreas.Contains(_workstationView.AreaId) && _moveableAreas.Contains((int)ComboBoxAreaNumber.SelectedValue) && _workstationView.AreaId == (int)ComboBoxAreaNumber.SelectedValue,
                Name = "Position",
                Text = position,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                UseColumnTextForButtonValue = true
            };
            DataGridViewInventoryLocations.Columns.Add(bCol);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "AreaName",
                HeaderText = _gridResourceManager.GetString($"Area"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "AreaName"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = _gridResourceManager.GetString($"Quantity"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Quantity"
            };
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Slot",
                HeaderText = _gridResourceManager.GetString($"Slot"),
                Visible = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Slot"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            var ckcol = new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "PrimeBin",
                HeaderText = _gridResourceManager.GetString($"PrimeBin"),
                Visible = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "PrimeBin"
            };
            DataGridViewInventoryLocations.Columns.Add(ckcol);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc1",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc1", _gridResourceManager),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc1"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc2",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc2", _gridResourceManager),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc2"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc3",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc3", _gridResourceManager),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc3"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc4",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc4", _gridResourceManager),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc4"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc5",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc5", _gridResourceManager),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc5",
                Visible = false
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SizeCodeName",
                HeaderText = _gridResourceManager.GetString($"SizeCodeName"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "SizeCodeName"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "VelocityCodeName",
                HeaderText = _gridResourceManager.GetString($"VelocityCodeName"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "VelocityCodeName"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "HeightCodeName",
                HeaderText = _gridResourceManager.GetString($"HeightCodeName"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "HeightCodeName"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PickSequence",
                HeaderText = $"Pick Sequence", // _gridResourceManager.GetString("Slot"),
                Visible = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "PickSequence"
            };
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StorageTypeName",
                HeaderText = _gridResourceManager.GetString($"StorageTypeName"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "StorageTypeName"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LocationCode",
                HeaderText = _gridResourceManager.GetString($"LocationCode"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "LocationCode",
                Visible = false
            };
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ReceivedDate",
                HeaderText = _gridResourceManager.GetString($"ReceivedDate"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "ReceivedDate"
            };
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LotNumber",
                HeaderText = "Lot Number",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "LotNumber"
            };
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ExpirationDate",
                HeaderText = "Expiration Date",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "ExpirationDate"
            };
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = _gridResourceManager.GetString($"Id"),
                Visible = false,
                Name = "Id"
            };
            DataGridViewInventoryLocations.Columns.Add(col);

            DataGridViewInventoryLocations.EnableHeadersVisualStyles = false;
            DataGridViewInventoryLocations.ColumnHeadersDefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;
            DataGridViewInventoryLocations.ColumnHeadersDefaultCellStyle.Font =
                new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);

            //New Location Grid
            DataGridViewInventoryNewLocations.AutoGenerateColumns = false;
            DataGridViewInventoryNewLocations.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewInventoryNewLocations.AllowUserToAddRows = false;
            DataGridViewInventoryNewLocations.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            // All columns
            foreach (DataGridViewColumn column in DataGridViewInventoryNewLocations.Columns)
            {
                column.MinimumWidth = 50;
            }
            bCol = new DataGridViewButtonColumn
            {
                // HeaderText = _gridResourceManager.GetString("Id"),
                Visible = _moveableAreas.Contains(_workstationView.AreaId) && _moveableAreas.Contains((int)ComboBoxAreaNumber.SelectedValue) && _workstationView.AreaId == (int)ComboBoxAreaNumber.SelectedValue,
                Name = "Position",
                Text = position,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                UseColumnTextForButtonValue = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(bCol);
            xcol = new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "InUse",
                HeaderText = _gridResourceManager.GetString($"InUse"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "InUse"
            };
            DataGridViewInventoryNewLocations.Columns.Add(xcol);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "AreaName",
                HeaderText = _gridResourceManager.GetString($"Area"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "StationName",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Slot",
                HeaderText = _gridResourceManager.GetString($"Slot"),
                Visible = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Slot"
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);
            ckcol = new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "PrimeBin",
                HeaderText = _gridResourceManager.GetString($"PrimeBin"),
                Visible = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "PrimeBin"
            };
            DataGridViewInventoryNewLocations.Columns.Add(ckcol);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc1",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc1", _gridResourceManager),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc1",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc2",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc2", _gridResourceManager),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc2",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc3",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc3", _gridResourceManager),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc3",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc4",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc4", _gridResourceManager),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc4",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc5",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc5", _gridResourceManager),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc5",
                Visible = false
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SizeCodeName",
                HeaderText = _gridResourceManager.GetString($"SizeCodeName"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "SizeCodeName",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "VelocityCodeName",
                HeaderText = _gridResourceManager.GetString($"VelocityCodeName"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "VelocityCodeName",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "HeightCodeName",
                HeaderText = _gridResourceManager.GetString($"HeightCodeName"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "HeightCodeName",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LocationCode",
                HeaderText = _gridResourceManager.GetString($"LocationCode"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "LocationCode",
                Visible = false
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PickSequence",
                HeaderText = $"Pick Sequence", // _gridResourceManager.GetString("Slot"),
                Visible = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "PickSequence"
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);
            
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = _gridResourceManager.GetString($"Id"),
                Visible = false,
                Name = "Id"
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);

            DataGridViewInventoryNewLocations.EnableHeadersVisualStyles = false; 
            DataGridViewInventoryNewLocations.ColumnHeadersDefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;
            DataGridViewInventoryNewLocations.ColumnHeadersDefaultCellStyle.Font =
                new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);

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
            _ = LoadLookupDataAsync();
        }

        private void SetupAddDetailForm()
        {
            ComboBoxAddDetailSizeCode.DataSource = _inventoryUnitOfWork.SizeCodes.All();
            ComboBoxAddDetailSizeCode.DisplayMember = "Name";
            ComboBoxAddDetailSizeCode.ValueMember = "Id";
            ComboBoxAddDetailVelocityCode.DataSource = _inventoryUnitOfWork.VelocityCodes.All();
            ComboBoxAddDetailVelocityCode.DisplayMember = "Name";
            ComboBoxAddDetailVelocityCode.ValueMember = "Id";
            ComboBoxAddDetailHeightCode.DataSource = _inventoryUnitOfWork.HeightCodes.All();
            ComboBoxAddDetailHeightCode.DisplayMember = "Name";
            ComboBoxAddDetailHeightCode.ValueMember = "Id";
            ComboBoxAddDetailArea.DataSource = _areaRepository.Lookup();
            ComboBoxAddDetailArea.DisplayMember = "Name";
            ComboBoxAddDetailArea.ValueMember = "Id";
            ComboBoxAddDetailStorageType.DataSource = _inventoryUnitOfWork.StorageTypes.All();
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
                    var result = MessageBox.Show(_resourceManager.GetString($"Message17"), string.Empty,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes) return;
                    await DeleteInventoryItem(locView.Id, false);
                    await LoadViewEdit();
                    tabControl1.SelectedTab = tabPage2;
                }
            }
            else
            {
                MessageBox.Show(_resourceManager.GetString($"Message6"));
            }

            Cursor.Current = Cursors.Default;
        }

        public async Task DeleteInventoryItem(int invId, bool releaseOnly)
        {
            var inventoryManager = new InventoryManager(_inventoryUnitOfWork, _locationsRepository, _inventoryRepository);
            await inventoryManager.DeleteInventoryRecord(invId, releaseOnly: releaseOnly);
        }

        /// <summary>
        /// Add a new location to the inventory from the view/edit tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MbViewEditAddLocation_Click(object sender, EventArgs e)
        {
            _logger.LogDetailAsync($"-----").SafeFireAndForget();
            _newOrViewEdit = "ViewEdit";
            _processing = true;

            var inventoryView = ((ObjectView<SqlInventoryView>)_locationBindingSource.Current).Object;
            var inventory = await _inventoryUnitOfWork.Inventory.FindByFirstOrDefaultAsync(r => r.Id == inventoryView.Id);
            if (inventory == null) return;
            SetCurrentInventoryItem(inventory);
            UpdateAddLocationForm(CurrentItem);
            // Get the available locations for the current item
            var recs = await GetAvailableLocations(CurrentItem);

            UpdateDataGridViewInventoryNewLocations(recs);
            _processing = false;

            tabControl1.SelectedTab = tabPage4;
        }

        /// <summary>
        /// Add a new location to the inventory from the view/edit tab,
        /// Add Location button 
        /// 
        /// </summary>
        private void UpdateAddLocationForm(ItemDefinition itemDefinition)
        {
            LabelSlot.Text = string.Empty;
            //check for a valid CurrentItem
            if (itemDefinition != null)
            {
                //set the TextBoxes to the current item values
                TextBoxInventoryNewLocationsItem.Text = itemDefinition.Item;
                TextBoxInventoryNewLocationsDescription.Text = itemDefinition.Description;
                // set the default values for the new location
                TextBoxInventoryNewLocationsQuantity.Text = "0";
                TextBoxInventoryNewLocationsArea.Text = itemDefinition.AreaId.ToString();
                ComboBoxInventoryNewLocationsStorageType.SelectedValue = itemDefinition.StorageTypeId;
                TextBoxInventoryNewLocationsLotNumber.Text = string.Empty;
                DateTimePickerInventoryNewLocationsExpirationDate.Checked = false;

            }
        }

        /// <summary>
        /// Get the Preferred/Exact available locations for the current item
        /// </summary>
        /// <param name="itemDefinition">ItemDefinition</param>
        /// <param name="inUseType"></param>
        /// <param name="recId"></param>
        public async Task<List<LocationView>> GetAvailableLocations(ItemDefinition itemDefinition, int inUseType = 0, int recId = 0)
        {
            _logger.LogDetailAsync($"GetAvailableLocations START").SafeFireAndForget();
            var idx = recId;
            var views = new List<LocationView>();
            if (itemDefinition == null) return views;
            var areaId = itemDefinition.AreaId;
            var sizeCode = itemDefinition.SizeCodeId;
            var heightCode = itemDefinition.HeightCodeId;
            var velocityCode = itemDefinition.VelocityCodeId;
            //ComboBoxAreaNumber.SelectedValue = itemDefinition.AreaId;
            //SetupRadioButtons();

            var radioButton = GetCheckedRadioButton();
            if (radioButton == null) return views;
            switch (radioButton.Name)
            {
                case nameof(RadioButtonExactMatch):
                    {
                        _logger.LogDetailAsync($"GetAvailableLocations EXACT").SafeFireAndForget();
                        views = _locationsRepository
                            .GetAllLocationViewsExact(areaId, sizeCode, velocityCode, heightCode, false)
                            .ToList();
                        break;
                    }
                case nameof(RadioButtonAllAvailable):
                    {
                        _logger.LogDetailAsync($"GetAvailableLocations ALL AVAILABLE").SafeFireAndForget();
                        views = _locationsRepository
                            .FindLocationViewsByAreaAndInUse(areaId, false)
                            .ToList();
                        break;
                    }
                case nameof(RadioButtonAllLocations):
                    {
                        _logger.LogDetailAsync($"GetAvailableLocations ALL LOCATIONS").SafeFireAndForget();
                        views = await _locationsRepository.FindLocationViewsByArea(areaId) as List<LocationView>;
                        break;
                    }
            }

            _logger.LogDetailAsync($"GetAvailableLocations END   Count = {views?.Count}").SafeFireAndForget();
            return views;

        }

        private RadioButton GetCheckedRadioButton()
        {
            return PanelRadioButtons.Controls.OfType<RadioButton>().FirstOrDefault(radioButton => radioButton.Checked);
        }

        private void UpdateDataGridViewInventoryNewLocations(List<LocationView> locationViews)
        {
            _logger.LogDetailAsync($"-----").SafeFireAndForget();
            var blv = new BindingListView<LocationView>(locationViews);

            // set the position column visible = false
            var positionColumn = DataGridViewInventoryNewLocations.Columns["Position"];
            if (positionColumn != null)
            {
                positionColumn.Visible = _moveableAreas.Contains(_workstationView.AreaId) &&
                                         _moveableAreas.Contains((int)ComboBoxAreaNumber.SelectedValue)
                                         && _workstationView.AreaId == (int)ComboBoxAreaNumber.SelectedValue;
            }

            // _logger.LogDetailAsync($"GetAvailableLocations 1").SafeFireAndForget();
            _newLocationBindingSource.DataSource = blv;
            //_logger.LogDetailAsync($"GetAvailableLocations 2").SafeFireAndForget();

            DataGridViewInventoryNewLocations.DataSource = _newLocationBindingSource;
            DataGridViewInventoryNewLocations.ClearSelection();

            //_logger.LogDetailAsync($"GetAvailableLocations 3").SafeFireAndForget();
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

            _logger.LogDetailAsync($"UpdateDataGridViewInventoryNewLocations END").SafeFireAndForget();
        }



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

        private void FrmInventory_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !CloseButtonPressed;
            // Save column widths for each grid
            Properties.Settings.Default.DataGridView1ColumnWidths = GetColumnWidths(DataGridView1);
            Properties.Settings.Default.DataGridViewInventoryLocationsColumnWidths = GetColumnWidths(DataGridViewInventoryLocations);
            Properties.Settings.Default.DataGridViewInventoryNewLocationsColumnWidths = GetColumnWidths(DataGridViewInventoryNewLocations);
            Properties.Settings.Default.Save(); // Persist to disk
        }
        private string GetColumnWidths(DataGridView grid)
        {
            var widths = new List<string>();
            foreach (DataGridViewColumn col in grid.Columns)
            {
                widths.Add($"{col.Name}:{col.Width}");
            }
            return string.Join(";", widths); // e.g., "AreaName:100;Item:150;..."
        }
        private async Task NewItemFind(string inventoryViewItem, int areaId)
        {
            //check inventoryViewItem for null or empty
            if (string.IsNullOrWhiteSpace(inventoryViewItem)) return;
            // check areaId for 0
            if (areaId == 0) return;
            ClearNewFields();
            //if (!string.IsNullOrWhiteSpace(inventoryViewItem))
            //{
            var item = inventoryViewItem;
            try
            {
                var rec = await Task.Run(() => _inventoryUnitOfWork.ItemDefinitions
                    .FindByInclude(f => f.Item == item && f.AreaId == areaId, i => i.Area)
                    .FirstOrDefault());
                // if the item Definition is null, check the AKA table
                //var itemDefinitions = recs.ToList();
                //if (!itemDefinitions.Any()) return;
                if (rec == null) return;

                //{
                //    var possibleAka = item;
                //    var item2 = _akaRepository.Get(possibleAka);
                //    if (!string.IsNullOrEmpty(item2))
                //    {
                //        TextBoxNewItem.Text = item2;
                //        recs = await _inventoryUnitOfWork.ItemDefinitions.FindByAsync(r => r.Item == item2);
                //    }
                //}

                //if (itemDefinitions.Any())
                //{
                // create the datasource for ComboBoxNewAreaChoice
                // the Id is the Id of the ItemDefinition
                // the Name is the Name of the Area
                //var areaId = (int)ComboBoxNewAreaChoice.SelectedValue;
                //var areas = itemDefinitions.Select(r => new LookUp()
                //{
                //    Id = r.Id,
                //    Name = r.Area.Name
                //}).ToList();
                //ComboBoxNewAreaChoice.DataSource = areas;
                //ComboBoxNewAreaChoice.DisplayMember = "Name";
                //ComboBoxNewAreaChoice.ValueMember = "Id";
                ////ComboBoxNewAreaChoice.SelectedValue = areaId;

                //var rec = itemDefinitions.FirstOrDefault();

                // if (rec != null)
                // {
                SetCurrentItemDefinition(rec);

                TextBoxNewId.Text = rec.Id.ToString();
                TextBoxNewItem.Text = rec.Item;
                TextBoxNewDescription.Text = rec.Description;
                CheckBoxNewScale.Checked = rec.Scale;
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
                // }
                //}

                _firstTimeCombo = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(_resourceManager.GetString($"Message8") + ex.Message);
                TextBoxNewItem.Focus();
            }
            // }
        }

        //private void UpdateNewItem(int itemDefinitionId)
        //{
        //    // check to see if itemDefinitionId is 0
        //    if (itemDefinitionId == 0) return;
        //    try
        //    {
        //        var rec = _inventoryUnitOfWork.ItemDefinitions.FindBy(f => f.Id == itemDefinitionId).FirstOrDefault();

        //        // if the item Definition is null, check the AKA table
        //        if (rec == null)
        //        {
        //            var possibleAka = rec.Item;
        //            var item2 = _akaRepository.Get(possibleAka);
        //            if (item2 != null)
        //            {
        //                TextBoxNewItem.Text = item2;
        //                rec = _inventoryUnitOfWork.ItemDefinitions.FindBy(r => r.Item == item2).FirstOrDefault();

        //            }
        //        }

        //        if (rec != null)
        //        {
        //           // SetCurrentItemDefinition(rec.AreaId);

        //            TextBoxArea.Text = rec.Area.Name;
        //            TextBoxNewId.Text = rec.Id.ToString();
        //            TextBoxNewItem.Text = rec.Item;
        //            TextBoxNewDescription.Text = rec.Description;
        //            CheckBoxNewScale.Checked = rec.Scale;
        //            TextBoxNewLocationMax.Text = rec.LocationMax.ToString();
        //            TextBoxNewLocationMin.Text = rec.LocationMin.ToString();
        //            TextBoxNewSystemMax.Text = rec.SystemMax.ToString();
        //            TextBoxNewSystemMin.Text = rec.SystemMin.ToString();
        //            TextBoxNewWeight.Text = rec.Weight.ToString("F4");
        //            TextBoxPickMax.Text = rec.PickMax.ToString();
        //            ComboBoxNewStorageType.SelectedValue = rec.StorageTypeId;
        //            ComboBoxNewUnitOfIssue.SelectedValue = rec.UnitOfIssueId;
        //            ComboBoxNewSizeCode.SelectedValue = rec.SizeCodeId;
        //            ComboBoxNewVelocityCode.SelectedValue = rec.VelocityCodeId;
        //            ComboBoxNewHeightCode.SelectedValue = rec.HeightCodeId;
        //            MbNewAddLocation.Enabled = true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(_resourceManager.GetString($"Message8") + ex.Message + ex.InnerException?.Message);
        //        TextBoxNewItem.Focus();
        //    }
        //}

        private void UpdateNewItem(int itemDefinitionId)
        {
            if (itemDefinitionId == 0)
                return;
            try
            {
                var itemDefinition = GetItemDefinition(itemDefinitionId);
                if (itemDefinition != null)
                {
                    PopulateItemDetails(itemDefinition);
                    MbNewAddLocation.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                HandleUpdateNewItemException(ex);
            }
        }
        private ItemDefinition GetItemDefinition(int itemDefinitionId)
        {
            var itemDefinition = _inventoryUnitOfWork.ItemDefinitions
                .FindByInclude(f => f.Id == itemDefinitionId, i => i.Area)
                .FirstOrDefault();
            if (itemDefinition == null)
            {
                var akaItem = GetAkaItem(itemDefinitionId);
                if (!string.IsNullOrEmpty(akaItem))
                {
                    TextBoxNewItem.Text = akaItem;
                    itemDefinition = _inventoryUnitOfWork.ItemDefinitions
                        .FindByInclude(r => r.Item == akaItem, i => i.Area)
                        .FirstOrDefault();
                }
            }
            return itemDefinition;
        }
        private string GetAkaItem(int itemDefinitionId)
        {
            var possibleAka = _inventoryUnitOfWork.ItemDefinitions
                .FindBy(f => f.Id == itemDefinitionId)
                .FirstOrDefault()?.Item;
            return _akaRepository.Get(possibleAka);
        }
        private void PopulateItemDetails(ItemDefinition itemDefinition)
        {
            TextBoxArea.Text = itemDefinition.Area.Name;
            TextBoxNewId.Text = itemDefinition.Id.ToString();
            TextBoxNewItem.Text = itemDefinition.Item;
            TextBoxNewDescription.Text = itemDefinition.Description;
            CheckBoxNewScale.Checked = itemDefinition.Scale;
            TextBoxNewLocationMax.Text = itemDefinition.LocationMax.ToString();
            TextBoxNewLocationMin.Text = itemDefinition.LocationMin.ToString();
            TextBoxNewSystemMax.Text = itemDefinition.SystemMax.ToString();
            TextBoxNewSystemMin.Text = itemDefinition.SystemMin.ToString();
            TextBoxNewWeight.Text = itemDefinition.Weight.ToString("F4");
            TextBoxPickMax.Text = itemDefinition.PickMax.ToString();
            ComboBoxNewStorageType.SelectedValue = itemDefinition.StorageTypeId;
            ComboBoxNewUnitOfIssue.SelectedValue = itemDefinition.UnitOfIssueId;
            ComboBoxNewSizeCode.SelectedValue = itemDefinition.SizeCodeId;
            ComboBoxNewVelocityCode.SelectedValue = itemDefinition.VelocityCodeId;
            ComboBoxNewHeightCode.SelectedValue = itemDefinition.HeightCodeId;
        }
        private void HandleUpdateNewItemException(Exception ex)
        {
            var errorMessage = _resourceManager.GetString($"Message8") + ex.Message;
            if (ex.InnerException != null)
            {
                errorMessage += ex.InnerException.Message;
            }
            MessageBox.Show(errorMessage);
            TextBoxNewItem.Focus();
        }


        private async void MbNewAddLocation_Click(object sender, EventArgs e)
        {
                _logger.LogDetailAsync($"-----").SafeFireAndForget();
            _processing = true;
            _newOrViewEdit = "New";
            //var workingItemDefinitionId = (int)ComboBoxNewAreaChoice.SelectedValue;

            //_workingItemDefinition = _inventoryUnitOfWork.ItemDefinitions.FindByKey(workingItemDefinitionId);

            //if (_workingItemDefinition == null) return;

            //SetCheckedRadioButton(_workingItemDefinition.AreaId);

            //UpdateAddLocationForm(_workingItemDefinition);
            SetCheckedRadioButton(CurrentItem.AreaId);

            UpdateAddLocationForm(CurrentItem);

            // Get the available locations for the current item
            // var recs = await GetAvailableLocations(_workingItemDefinition, 0);
            var recs = await GetAvailableLocations(CurrentItem);

            UpdateDataGridViewInventoryNewLocations(recs);
            _processing = false;

            tabControl1.SelectedTab = tabPage4;



            //var itemDefinitionId = (int)ComboBoxNewAreaChoice.SelectedValue;

            //SetCurrentItemDefinition(itemDefinitionId);
            //if (CurrentItem != null)
            //{
            //    TextBoxInventoryNewLocationsItem.Text = CurrentItem.Item;
            //    TextBoxInventoryNewLocationsDescription.Text = CurrentItem.Description;
            //    var recs = await GetAvailableLocations(CurrentItem, 0);
            //    UpdateDataGridViewInventoryNewLocations(recs);
            //}

            //tabControl1.SelectedTab = tabPage4;
        }

        private async void MbNewLocationsClose_Click(object sender, EventArgs e)
        {
            LabelSlot.Text = string.Empty;
            // SetCurrentItemDefinition();
            switch (_newOrViewEdit)
            {
                case "New":
                    {
                        await NewInventoryItem(CurrentItem);
                        tabControl1.SelectedTab = tabPage3;
                        break;
                    }
                case "ViewEdit":
                    {
                        await LoadViewEdit();
                        tabControl1.SelectedTab = tabPage2;
                        break;
                    }
            }



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
            LabelSlot.Text = string.Empty;
            if (TextBoxInventoryNewLocationsQuantity.Text.ParseInt() == 0 &&
                ComboBoxInventoryNewLocationsStorageType.SelectedValue.ToString().ParseInt() ==
                (int)StorageType.Release)
            {
                MessageBox.Show(_resourceManager.GetString($"Message9"), string.Empty, MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
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
                            RFID = TextBoxInventoryNewLocationsRfid.Text,
                            LotNumber = TextBoxInventoryNewLocationsLotNumber.Text,
                            ExpirationDate = DateTimePickerInventoryNewLocationsExpirationDate.Value
                        };
                        await _inventoryUnitOfWork.Inventory.InsertAsync(inventory);
                        
                        var inventoryView = _inventoryRepository.GetInventoryViewById(inventory.Id);
                        
                        await GlobalVar.HistoryManager.SaveHistoryAsync(ActionCode.InventoryAdd, inventoryView);
                        await _locationsRepository.SetLocationInUse(inventory.LocationId, b: true);

                        //TODO  fixed locationCode 

                        //var locationCode = @"1234567890123456";


                        //_locationsRepository.SetLocationCode(inventory.LocationId, locationCode);

                        SetCurrentInventoryItem(inventory);
                        await LoadViewEdit();
                        tabControl1.SelectedTab = tabPage2;
                    }
                }
                else
                {
                    MessageBox.Show(_resourceManager.GetString($"Message10"));
                }
            }
        }

        private async Task<List<SqlInventoryView>> GetInventoryViewListByItem(int itemId)
        {
            var recs = new List<SqlInventoryView>();
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
                _logger.LogDetailAsync($"Error during Get Inventory View List By Item Event. {ex.Message}")
                    .SafeFireAndForget();
            }

            return recs;
        }

        private async Task<List<SqlInventoryView>> GetInventoryViewListByItemName(string itemName)
        {
            var recs = new List<SqlInventoryView>();
            try
            {
                using (var context = new NeutronDb())
                {
                    var findItemId = new SqlParameter("@ItemName", itemName);
                    recs = await context.Database
                        .SqlQuery<SqlInventoryView>("usp_GetInventoryViewByItemName @ItemName", findItemId).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Error during Get Inventory View List By Item Event. {ex.Message}")
                    .SafeFireAndForget();
            }

            return recs;
        }

        //private IEnumerable<SqlInventoryView> GetInventoryViewList()
        //{
        //    IEnumerable<SqlInventoryView> recs = new List<SqlInventoryView>();
        //    using (var context = new NeutronDb())
        //    {
        //        recs = context.Database.SqlQuery<SqlInventoryView>("usp_GetInventoryView").ToList();
        //    }

        //    return recs;
        //}

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
            return enumerable.SelectMany(c => GetTabControls(c, type)).Concat(enumerable)
                .Where(c => c.GetType() == type);
        }

        private async void MbAddDetailSave_Click(object sender, EventArgs e)
        {
            var deleted = false;
            var beginningQuantity = 0;
            Inventory inventory;
            if (CurrentInventoryItem != null)
            {
                var inventoryId = CurrentInventoryItem.Id;

                //update
                inventory = _inventoryUnitOfWork.Inventory.FindByKey(inventoryId);
                await GlobalVar.HistoryManager.SaveHistoryAsync(ActionCode.InventoryModify, inventory,
                    inventory.Quantity, true);

                beginningQuantity = inventory.Quantity;
                inventory.Quantity = (TextBoxAddDetailQuantity.Text).ParseInt();
                inventory.StorageTypeId =
                    ((NeutronData.Models.Lookups.StorageType)ComboBoxAddDetailStorageType.SelectedItem).Id;
                inventory.ReceivedDate = DateTimePickerAddDetailReceivedDate.Value;
                inventory.PrimeBin = CheckBoxAddDetailPrimeBin.Checked;
                inventory.AreaId = (int)ComboBoxAddDetailArea.SelectedValue;
                inventory.RFID = TextBoxAddDetailLocationCode.Text;
                inventory.LotNumber = TextBoxAddDetailLotNumber.Text;
                inventory.ExpirationDate = DateTimePickerAddDetailExpirationDate.Checked
                    ? (DateTime?)DateTimePickerAddDetailExpirationDate.Value
                    : null;

                await _inventoryUnitOfWork.Inventory.UpdateAsync(inventory);
                await GlobalVar.HistoryManager.SaveHistoryAsync(ActionCode.InventoryModify, inventory,
                    beginningQuantity, true);
            }
            else
            {
                //new
                inventory = new Inventory();
                inventory.ItemDefinitionId = (TextBoxAddDetailItemDefinitionId.Text).ParseInt();
                inventory.LocationId = (TextBoxAddDetailLocationId.Text).ParseInt();
                inventory.Quantity = (TextBoxAddDetailQuantity.Text).ParseInt();
                inventory.StorageTypeId =
                    ((NeutronData.Models.Lookups.StorageType)ComboBoxAddDetailStorageType.SelectedItem).Id;
                inventory.ReceivedDate = DateTimePickerAddDetailReceivedDate.Value;
                inventory.PrimeBin = CheckBoxAddDetailPrimeBin.Checked;
                inventory.AreaId = (int)ComboBoxAddDetailArea.SelectedValue;
                inventory.RFID = TextBoxAddDetailLocationCode.Text;
                await _inventoryUnitOfWork.Inventory.InsertAsync(inventory);
                await GlobalVar.HistoryManager.SaveHistoryAsync(ActionCode.InventoryAdd, inventory, beginningQuantity,
                    true);
            }

            var inventoryManager = new InventoryManager(_inventoryUnitOfWork, _locationsRepository, _inventoryRepository);
            var canDelete = await inventoryManager.QuickReleaseCheck(inventory);
            if (canDelete)
            {
                var sb = new StringBuilder();
                var item = inventory.ItemDefinition != null ? inventory.ItemDefinition.Item : string.Empty;
                var storageType = inventory.StorageType != null ? inventory.StorageType.Name : string.Empty;
                sb.AppendLine($"Are you sure you want to DELETE the selected Inventory Item? {Environment.NewLine}" +
                              $"ItemId: {inventory.ItemDefinitionId} Item: {item} LocationId: {inventory.LocationId}" +
                              $"Quantity: {inventory.Quantity} AreaId: {inventory.AreaId} Storage: {storageType} ");

                var result = MessageBox.Show($"{sb}", "Delete Inventory Item",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (result == DialogResult.Yes)
                {
                    _logger.LogDetailAsync($"{sb}").SafeFireAndForget();
                    deleted = await inventoryManager.ReleaseCheck(inventory);
                }
            }

            if (!deleted)
            {
                await _locationsRepository.SetLocationInUse(inventory.LocationId, b: true);
                TextBoxViewEditItem.Text = CurrentItem.Item;
                TextBoxViewEditDescription.Text = CurrentItem.Description;
            }

            var views = await GetInventoryViewListByItemName(CurrentItem.Item);
            var blv = new BindingListView<SqlInventoryView>(views.ToList());
            _locationBindingSource.DataSource = blv;
            DataGridViewInventoryLocations.DataSource = _locationBindingSource;
            DataGridViewInventoryLocations.ClearSelection();
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
                _iptiDisplayFunctions.ClearBlastzone();
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

            TextBoxNewId.Text = string.Empty;
          //  TextBoxNewItem.Text = string.Empty;
          //  TextBoxNewArea.Text = string.Empty;
            TextBoxNewDescription.Text = string.Empty;
            CheckBoxNewScale.Checked = false;
            CheckBoxInUse.Checked = false;
            // ComboBoxNewArea.SelectedValue = -1;
            TextBoxArea.Text = string.Empty;
            TextBoxNewLocationMax.Text = string.Empty;
            TextBoxNewLocationMin.Text = string.Empty;
            TextBoxNewSystemMax.Text = string.Empty;
            TextBoxNewSystemMin.Text = string.Empty;
            TextBoxNewWeight.Text = string.Empty;
            ComboBoxNewStorageType.SelectedValue = -1;
            ComboBoxNewUnitOfIssue.SelectedValue = -1;
            ComboBoxNewSizeCode.SelectedValue = -1;
            ComboBoxNewVelocityCode.SelectedValue = -1;
            ComboBoxNewHeightCode.SelectedValue = -1;
            TextBoxNewItem.Focus();
        }

        private async void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            await LoadViewEdit();
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0) // Ensure it's not a header click
            {
                _doubleClickDetected = false; // Reset the double-click flag
                _clickTimer.Start(); // Start the timer to delay single-click action
            }

            //var dataGridView = (DataGridView)sender;
            //dataGridView.Enabled = false;
            //try
            //{
            //    await DataGridViewPosition(dataGridView, e.RowIndex);
            //    SetCurrentInventoryItem();
            //}
            //finally
            //{
            //    dataGridView.Enabled = true;
            //}
        }

        private async Task DataGridViewPosition(DataGridView grid, int rowIndex)
        {
            _logger.LogDetailAsync($"{grid.Text} Row: {rowIndex}").SafeFireAndForget();
            var qty = 0;
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

                var quantity = qty.ToString();


                if (_workstationView.Blastzones.Any())
                {
                    if (_iptiDisplayFunctions != null)
                    {
                        _iptiDisplayFunctions.ClearBlastzone();
                        _iptiDisplayFunctions.TurnOnBlastzoneDisplay(trayNumber, part, quantity);
                        _iptiDisplayFunctions.TurnOnBlastzoneOrderControl(trayNumber, $"Qty: {quantity}");
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



        private async Task MoveDevice(int deviceNumber, int trayNumber, int level, int part, int quantity = 0,
            string display = "")
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
                                        GlobalVar.Shuttle.PositionDevice(deviceNumber, trayNumber, level, part,
                                            quantity,
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
                                    var msg = _resourceManager.GetString($"Message11");
                                    Mediator.GetInstance().OnGeneralError(this, msg);
                                }
                            }
                            else
                            {
                                var msg = $"{_resourceManager.GetString($"Message12")} - {hanel.Name}";
                                Mediator.GetInstance().OnGeneralError(this, msg);
                            }
                        }
                        else
                        {
                            var msg = _resourceManager.GetString($"Message13");
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
                _logger.LogDetailAsync($"Move Device - Inventory Module: {Environment.NewLine}{ex.Message}")
                    .SafeFireAndForget();
            }
        }

        //private void ClearAllShi()
        //{
        //    if (_neutronVariables.DisplaysEnabled)
        //        if (GlobalVar.Displays != null)
        //            GlobalVar.Displays.ClearAllShi();
        //}

        //private void MBPrintInventory_Click(object sender, EventArgs e)
        //{
        //    new CsvUtility(_contextFactory).SaveToCsv(DataGridView1);
        //}

        private void DataGridViewInventoryLocations_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("Click");
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
                LabelFormHeaderText.Text = _resourceManager.GetString($"NeutronWarehouseMana");
                LabelFormTitle.Text = _resourceManager.GetString($"Inventory");
                ButtonSaveToExcel.Text = _resourceManager.GetString($"SaveToFile");
                //LabelFindDescription.Text = _resourceManager.GetString("SearchFor");
                MButtonNew.Text = _resourceManager.GetString($"New");
                MButtonViewEdit.Text = _resourceManager.GetString($"View/Edit");
                MButtonClose.Text = _resourceManager.GetString($"Home");
                MButtonSearch.Text = _resourceManager.GetString($"Search");
                MbViewEditEdit.Text = _resourceManager.GetString($"EditLocation");
                MbViewEditAddLocation.Text = _resourceManager.GetString($"AddLocation");
                MbViewEditListing.Text = _resourceManager.GetString($"Listing");
                MbViewEditDelete.Text = _resourceManager.GetString($"DeleteLocation");
                MbViewEditClose.Text = _resourceManager.GetString($"Back");
                LabelViewEditDescription.Text = _resourceManager.GetString($"Description");
                LabelViewEditItem.Text = _resourceManager.GetString($"Item");
                MbNewAddLocation.Text = _resourceManager.GetString($"AddLocation");
                LabelActionNew.Text = _resourceManager.GetString($"NewInventoryItem");
                MbNewListing.Text = _resourceManager.GetString($"Listing");
                MbNewClose.Text = _resourceManager.GetString($"Back");
                MbNewFind.Text = _resourceManager.GetString($"Find");
                LabelNewItem.Text = _resourceManager.GetString($"EnterItemNumber");
                LabelNewUnitOfIssue.Text = _resourceManager.GetString($"UnitOfIssue");
                LabelNewStorageType.Text = _resourceManager.GetString($"StorageType");
                LabelNewWeight.Text = _resourceManager.GetString($"Weight");
                CheckBoxNewScale.Text = _resourceManager.GetString($"UseScale");
                LabelNewSystemMin.Text = _resourceManager.GetString($"SystemMin");
                LabelNewLocationCode.Text = _resourceManager.GetString($"LocationCode");
                LabelNewHeight.Text = _resourceManager.GetString($"Height");
                LabelNewVelocity.Text = _resourceManager.GetString($"Velocity");
                LabelNewSize.Text = _resourceManager.GetString($"Size");
                LabelNewSystemMax.Text = _resourceManager.GetString($"SystemMax");
                LabelNewLocationMin.Text = _resourceManager.GetString($"LocationMin");
                LabelNewLocationMax.Text = _resourceManager.GetString($"LocationMax");
                LabelNewDescriiption.Text = _resourceManager.GetString($"Description");
                LabelNewArea.Text = _resourceManager.GetString($"Area");
                LabelActionNewLocations.Text = _resourceManager.GetString($"NewLocations");
                MbNewLocationsListing.Text = _resourceManager.GetString($"Listing");
                MbNewAvailableLocations.Text = _resourceManager.GetString($"ShowAll");
                MbNewLocationsClose.Text = _resourceManager.GetString($"Back");
                MbNewLocationsSave.Text = _resourceManager.GetString($"Save");
                CheckBoxInventoryNewLocationsPrimeBin.Text = _resourceManager.GetString($"PrimeBin");
                LabelNewLocationsStorageType.Text = _resourceManager.GetString($"StorageType");
                LabelNewLocationsReceivedDate.Text = _resourceManager.GetString($"ReceivedDate");
                LabelNewLocationsQuantity.Text = _resourceManager.GetString($"Qty");
                LabelNewLocationsDescription.Text = _resourceManager.GetString($"Description");
                LabelNewLocationsItem.Text = _resourceManager.GetString($"Item");
                LabelActionAddDetail.Text = _resourceManager.GetString($"AddDetail");
                MbAddDetailListing.Text = _resourceManager.GetString($"Listing");
                MbAddDetailClose.Text = _resourceManager.GetString($"Back");
                MbAddDetailSave.Text = _resourceManager.GetString($"Save");
                CheckBoxAddDetailPrimeBin.Text = _resourceManager.GetString($"PrimeBin");
                LabelAddDetailReceivedDate.Text = _resourceManager.GetString($"ReceivedDate");
                LabelAddDetailDescription.Text = _resourceManager.GetString($"Description");
                LabelAddDetailItem.Text = _resourceManager.GetString($"Item");
                LabelAddDetailStorageType.Text = _resourceManager.GetString($"StorageType");
                LabelAddDetailQuantity.Text = _resourceManager.GetString($"Qty");
                GroupBoxAddDetailLocation.Text = _resourceManager.GetString($"Location");
                CheckBoxInUse.Text = _resourceManager.GetString($"InUse");
                LabelLocationCode.Text = _resourceManager.GetString($"LocationCode");
                LabelAddDetailHeight.Text = _resourceManager.GetString($"Height");
                LabelAddDetailVelocity.Text = _resourceManager.GetString($"Velocity");
                LabelAddDetailSlot.Text = _resourceManager.GetString($"Slot");
                LabelAddDetailSize.Text = _resourceManager.GetString($"Size");
                LabelAddDetailTag.Text = _resourceManager.GetString($"Tag");
                LabelAddDetailBack.Text = _resourceManager.GetString($"Back");
                LabelAddDetailOver.Text = _resourceManager.GetString($"Over");
                LabelAddDetailTray.Text = _resourceManager.GetString($"Tray");
                LabelAddDetailDevice.Text = _resourceManager.GetString($"Device");
                LabelAddDetailAreaNumber.Text = _resourceManager.GetString($"Area");
                ButtonPositionDevice.Text = _resourceManager.GetString($"PositionDevice");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading language file.  {ex.Message} {Environment.NewLine} {ex.InnerException} ");
            }
        }

        private async void ButtonPositionDevice_Click(object sender, EventArgs e)
        {
            var deviceNumber = TextBoxAddDetailLoc1.Text.ParseInt();
            var trayNumber = TextBoxAddDetailLoc2.Text.ParseInt();
            var level = TextBoxAddDetailLoc3.Text.ParseInt();
            var part = TextBoxAddDetailLoc4.Text.ParseInt();
            var qty = TextBoxAddDetailQuantity.Text.ParseInt();
            _logger.LogDetailAsync($"Device: {deviceNumber} Tray: {trayNumber} Level: {level} Part: {part}")
                .SafeFireAndForget();
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

        private async void MbNewAvailableLocations_Click(object sender, EventArgs e)
        {
            if (_startup) return;
            _logger.LogDetailAsync($"-----").SafeFireAndForget();
            var recs = await GetAvailableLocations(CurrentItem);
            UpdateDataGridViewInventoryNewLocations(recs);
        }

        private void ComboBoxNewAreaChoice_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_firstTimeCombo) return;

            var lookup = (LookUp)ComboBoxNewAreaChoice.SelectedItem;

            UpdateNewItem(lookup.Id);
        }

        private async void RadioButtonAllLocations_CheckedChanged(object sender, EventArgs e)
        {
            if (_startup) return;
            _logger.LogDetailAsync($"-----").SafeFireAndForget();
            if (_processing) return;

            var radioButton = (RadioButton)sender;
            if (radioButton.Checked)
            {
                // show the wait cursor
                Cursor.Current = Cursors.WaitCursor;
                _processing = true;
                _logger.LogDetailAsync($"-----").SafeFireAndForget();
                var recs = await GetAvailableLocations(CurrentItem);
                UpdateDataGridViewInventoryNewLocations(recs);

                _processing = false;
                // show the default cursor
                Cursor.Current = Cursors.Default;
            }

            _logger.LogDetailAsync($"RadioButtonShowAll_CheckedChanged END").SafeFireAndForget();
        }


        //private void RadioButtonShowAll_CheckedChanged(object sender, EventArgs e)
        //{
        //    _logger.LogDetailAsync($"RadioButtonShowAll_CheckedChanged START").SafeFireAndForget();
        //    if (_processing) return;

        //    if (sender is RadioButton radioButton && radioButton.Checked)
        //    {
        //        // show the wait cursor
        //        Cursor.Current = Cursors.WaitCursor;
        //        _processing = true;
        //        var inUseType = Convert.ToInt32(radioButton.Tag);
        //        if (AllAvailable)
        //        {
        //            GetAllAvailableLocations(CurrentItem, inUseType);
        //        }
        //        else
        //        {
        //            var recs = GetAvailableLocations(CurrentItem, inUseType);
        //            UpdateDataGridViewInventoryNewLocations(recs);
        //        }

        //        _processing = false;
        //        // show the default cursor
        //        Cursor.Current = Cursors.Default;
        //    }

        //    _logger.LogDetailAsync($"RadioButtonShowAll_CheckedChanged END").SafeFireAndForget();
        //}

        private async void RadioButtonAllAvailable_CheckedChanged(object sender, EventArgs e)
        {
            if (_startup) return;
            _logger.LogDetailAsync($"-----").SafeFireAndForget();
            if (_processing) return;
            var radioButton = (RadioButton)sender;
            if (radioButton.Checked)
            {
                _processing = true;
                Cursor.Current = Cursors.WaitCursor;
                var areaId = CurrentItem.AreaId;
                _logger.LogDetailAsync($"-----").SafeFireAndForget();
                var recs = await GetAvailableLocations(CurrentItem);

                UpdateDataGridViewInventoryNewLocations(recs);

                _processing = false;
                Cursor.Current = Cursors.Default;
            }

            _logger.LogDetailAsync($"RadioButtonInUse_CheckedChanged END").SafeFireAndForget();
        }
        //private void RadioButtonInUse_CheckedChanged(object sender, EventArgs e)
        //{
        //    _logger.LogDetailAsync($"RadioButtonInUse_CheckedChanged START").SafeFireAndForget();
        //    if (_processing) return;

        //    if (sender is RadioButton radioButton && radioButton.Checked)
        //    {
        //        _processing = true;
        //        Cursor.Current = Cursors.WaitCursor;
        //        var inUseType = Convert.ToInt32(radioButton.Tag);
        //        if (AllAvailable)
        //        {
        //            GetAllAvailableLocations(CurrentItem, inUseType);
        //        }
        //        else
        //        {
        //            var recs = GetAvailableLocations(CurrentItem, inUseType);
        //            UpdateDataGridViewInventoryNewLocations(recs);
        //        }

        //        _processing = false;
        //        Cursor.Current = Cursors.Default;
        //    }

        //    _logger.LogDetailAsync($"RadioButtonInUse_CheckedChanged END").SafeFireAndForget();
        //}

        private async void RadioButtonExactMatch_CheckedChanged(object sender, EventArgs e)
        {
            if (_startup) return;
            _logger.LogDetailAsync($"-----").SafeFireAndForget();
            if (_processing) return;

            if (sender is RadioButton radioButton && radioButton.Checked)
            {
                _processing = true;
                Cursor.Current = Cursors.WaitCursor;
                _logger.LogDetailAsync($"-----").SafeFireAndForget();
                var recs = await GetAvailableLocations(CurrentItem);
                UpdateDataGridViewInventoryNewLocations(recs);


                _processing = false;
                Cursor.Current = Cursors.Default;
            }

            _logger.LogDetailAsync($"RadioButtonExactMatch_CheckedChanged END").SafeFireAndForget();
        }

        //private void RadioButtonNotInUse_CheckedChanged(object sender, EventArgs e)
        //{
        //    _logger.LogDetailAsync($"RadioButtonNotInUse_CheckedChanged START").SafeFireAndForget();
        //    if (_processing) return;

        //    if (sender is RadioButton radioButton && radioButton.Checked)
        //    {
        //        _processing = true;
        //        Cursor.Current = Cursors.WaitCursor;
        //        var inUseType = Convert.ToInt32(radioButton.Tag);
        //        if (AllAvailable)
        //        {
        //            GetAllAvailableLocations(CurrentItem, inUseType);
        //        }
        //        else
        //        {
        //            var recs = GetAvailableLocations(CurrentItem, inUseType);
        //            UpdateDataGridViewInventoryNewLocations(recs);
        //        }

        //        _processing = false;
        //        Cursor.Current = Cursors.Default;
        //    }

        //    _logger.LogDetailAsync($"RadioButtonNotInUse_CheckedChanged END").SafeFireAndForget();
        //}

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
                dataTable = ToDataTable(selectedList);
            }
            else
            {
                // Create a DataTable from the List(Of T) (SqlInventoryView)
                dataTable = ToDataTable(_currentList);

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

            try
            {
                foreach (var prop in props)
                {
                    //Setting column names as Property names
                    //Unwrap Nullable<T> types, as DataSet does not support them directly
                    var colType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    dataTable.Columns.Add(prop.Name, colType);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
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
            if (_startup) return;

            DataGridView1.Columns[0].Visible = _moveableAreas.Contains(_workstationView.AreaId) &&
                                               _moveableAreas.Contains((int)ComboBoxAreaNumber.SelectedValue)
                                               && _workstationView.AreaId == (int)ComboBoxAreaNumber.SelectedValue;

            DataGridViewInventoryLocations.Columns[0].Visible = _moveableAreas.Contains(_workstationView.AreaId) &&
                                                                _moveableAreas.Contains((int)ComboBoxAreaNumber.SelectedValue)
                                                                && _workstationView.AreaId == (int)ComboBoxAreaNumber.SelectedValue;

            DataGridViewInventoryNewLocations.Columns[0].Visible = _moveableAreas.Contains(_workstationView.AreaId) &&
                                                                   _moveableAreas.Contains((int)ComboBoxAreaNumber.SelectedValue)
                                                                   && _workstationView.AreaId == (int)ComboBoxAreaNumber.SelectedValue;

            await RefreshData();
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
                            ItemDefinitionId = itemDefinitionId.ParseInt(),
                            LocationId = locationId.ParseInt(),
                            Quantity = quantity.ParseInt(),
                            ReceivedDate = DateTime.TryParse(receivedDate, out var date) ? date : DateTime.Now,
                            PrimeBin = primeBin.ParseInt() == 1 ? true : false,
                            AreaId = areaId.ParseInt(),
                            StorageTypeId = (int)GetEnumValue<StorageType>(storageTypeName),
                            RFID = rfid,
                        };
                        // add the new Inventory object to the database
                        await _inventoryUnitOfWork.Inventory.InsertAsync(inventory);
                    }
                    else
                    {
                        // get the existing Inventory object
                        var inventoryId = id.ParseInt();
                        var inventory = _inventoryUnitOfWork.Inventory.FindByKey(inventoryId);
                        if (inventory == null) continue;
                        // update the values
                        inventory.Quantity = quantity.ParseInt();
                        inventory.ReceivedDate = DateTime.TryParse(receivedDate, out var date) ? date : DateTime.Now;
                        inventory.PrimeBin = primeBin.ParseInt() == 1 ? true : false;
                        inventory.StorageTypeId = (int)GetEnumValue<StorageType>(storageTypeName);
                        inventory.RFID = rfid;
                        // update the database
                        await _inventoryUnitOfWork.Inventory.UpdateAsync(inventory);
                    }

                    // Update the progress
                    processedCount++;
                    var progressPercentage = (int)((double)processedCount / rowCount * 100);
                    //if (progressPercentage % 25 == 0)
                    //{
                    //    _logger.LogDetailAsync($"Loading {progressPercentage}% complete").SafeFireAndForget();
                    //    worker.ReportProgress(progressPercentage);
                    //}
                }

                Mediator.GetInstance().OnDisplayMessage(this, $"Load complete");
            }
            catch (Exception ex)
            {
                Mediator.GetInstance()
                    .OnGeneralError(this, $"Error Adding/Updating Records{Environment.NewLine}{ex.Message}");
            }
        }

        public static T GetEnumValue<T>(string str) where T : struct
        {
            return (T)Enum.Parse(typeof(T), str);
        }

        private void BackgroundWorkerItemDefinitions_ProgressChanged(object sender,
            System.ComponentModel.ProgressChangedEventArgs e)
        {
            ProgressBarInventory.Value = e.ProgressPercentage;
        }

        private async void BackgroundWorkerItemDefinitions_RunWorkerCompleted(object sender,
            System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            await RefreshData();
            ProgressBarInventory.Value = 0;
            Cursor.Current = Cursors.Default;
            ButtonLoadFromExcel.Enabled = true;
            ButtonSaveToExcel.Enabled = true;
            _logger.LogDetailAsync("Loading records from Excel spreadsheet complete").SafeFireAndForget();
        }

        private void ButtonFindLocation_Click(object sender, EventArgs e)
        {
            if (CurrentItem == null) return;

            if (CurrentItem.AreaId == AreaEight)
            {
                OpenSlotForm();
                return;
            }

            if (CurrentItem.AreaId == 1 || CurrentItem.AreaId == 2)
            {
                OpenBlastForm();

            }
            else
            {
                OpenDeviceForm();

            }
        }

        private void OpenDeviceForm()
        {
            LabelSlot.Text = string.Empty;
            var slot = string.Empty;
            using (var frm = new FrmGetLocationDevice(_newLocationBindingSource))
            {
                DialogResult result = frm.ShowDialog();
                if (result == DialogResult.OK)
                {
                    var rec = frm.Rec;
                    slot =
                        $"{frm.Device.PadLeft(2, '0')}-{frm.Tray.PadLeft(2, '0')}-{frm.Over.PadLeft(2, '0')}-{frm.Back.PadLeft(2, '0')}";
                    LabelSlot.Text = slot;
                    if (rec != null)
                    {
                        var index = _newLocationBindingSource.IndexOf(rec);
                        DataGridViewInventoryNewLocations.Rows[index].Selected = true;
                        // set the current record of the BindingSource
                        _newLocationBindingSource.Position = index;

                        DataGridViewInventoryNewLocations.FirstDisplayedScrollingRowIndex = index;
                    }
                }
                else
                {
                    LabelSlot.Text = string.Empty;
                    Close();
                }
            }
            // get a record from _newLocationBindingSource that matches the slot
            //var rec = _newLocationBindingSource.List.Cast<LocationView>().FirstOrDefault(r => r.Slot == slot);

            //SetCurrentLocation(rec.Id);
        }

        private void OpenBlastForm()
        {
            LabelSlot.Text = string.Empty;
            var slot = string.Empty;
            using (var frm = new FrmGetLocationBlast(_newLocationBindingSource))
            {
                DialogResult result = frm.ShowDialog();
                if (result == DialogResult.OK)
                {
                    var rec = frm.Rec;
                    slot =
                        $"{frm.Over.PadLeft(2, '0')}-{frm.Back.PadLeft(2, '0')}";
                    LabelSlot.Text = slot;
                    if (rec != null)
                    {
                        var index = _newLocationBindingSource.IndexOf(rec);
                        DataGridViewInventoryNewLocations.Rows[index].Selected = true;
                        // set the current record of the BindingSource
                        _newLocationBindingSource.Position = index;

                        DataGridViewInventoryNewLocations.FirstDisplayedScrollingRowIndex = index;
                    }
                }
                else
                {
                    LabelSlot.Text = string.Empty;
                    Close();
                }
            }
        }

        private void OpenSlotForm()
        {
            LabelSlot.Text = string.Empty;
            using (var frm = new FrmGetLocationSlot(_newLocationBindingSource))
            {
                DialogResult result = frm.ShowDialog();
                if (result == DialogResult.OK)
                {
                    var rec = frm.Rec;
                    string slot = frm.SlotNumber;
                    LabelSlot.Text = slot;
                    if (rec != null)
                    {
                        var index = _newLocationBindingSource.IndexOf(rec);
                        DataGridViewInventoryNewLocations.Rows[index].Selected = true;
                        // set the current record of the BindingSource
                        _newLocationBindingSource.Position = index;

                        DataGridViewInventoryNewLocations.FirstDisplayedScrollingRowIndex = index;
                    }
                }
                else
                {
                    LabelSlot.Text = string.Empty;
                    Close();
                }
            }
            // get a record from _newLocationBindingSource that matches the slot
            //var rec = _newLocationBindingSource.List.Cast<LocationView>().FirstOrDefault(r => r.Slot == slot);

            //SetCurrentLocation(rec.Id);
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged()
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void dateTimePicker1_ValueChanged()
        {

        }
    }
}