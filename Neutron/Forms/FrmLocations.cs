using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlliedLogger;
using EnumsNET;
using Equin.ApplicationFramework;
using JsonManager;
using MetroFramework.Forms;
using Neutron.Enums;
using NeutronCore.Extensions;
using Neutron.Global;
using Neutron.Interfaces;
using Neutron.Models;
using NeutronCore;
using NeutronCore.Enums;
using NeutronCore.Global;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Models.Lookups;
using NeutronData.ModelViews;
using NeutronData.PrintModels;
using NeutronData.Repositories;
using SlotNameFactory;
using NeutronData.Interfaces;
using StationType = NeutronCore.Enums.StationType;
using ExcelManager;
using System.Reflection;
using NeutronEvents;
using IPTI.Models;
using AsyncAwaitBestPractices;

namespace Neutron.Forms
{
    public partial class FrmLocations : MetroForm
    {
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;
        private ResourceManager _gridResourceManager;
        private readonly IJsonData _jsonData;
        private readonly NeutronVariables _neutronVariables;
        private readonly ILacProcessor _lacProcessor;
        //private readonly IHistoryManager _historyManager;
        private readonly WorkstationView _workstationView;
        private DocumentPrinterPreferences _documentPrinter;
        private LabelPrinterPreferences _labelPrinter;
        private readonly BindingSource _bindingSource = new BindingSource();
        private LocationsRepository _locationRepository;
        private IDynamicLogger _logger;

        private readonly GenericRepository<HeightCode> _repoHeightCode =
            new GenericRepository<HeightCode>(new NeutronDb());

        private readonly GenericRepository<Inventory>
            _repoInventory = new GenericRepository<Inventory>(new NeutronDb());

        private readonly GenericRepository<Location> _repoLocation = new GenericRepository<Location>(new NeutronDb());

        private readonly GenericRepository<StorageDevice> _repoDevices =
            new GenericRepository<StorageDevice>(new NeutronDb());

        private readonly GenericRepository<SizeCode> _repoSizeCode = new GenericRepository<SizeCode>(new NeutronDb());

        private readonly IWorkstationRepository _workstationRepository;

        private readonly GenericRepository<VelocityCode> _repoVelocityCode =
            new GenericRepository<VelocityCode>(new NeutronDb());

        private readonly GenericRepository<Area> _repoArea =
            new GenericRepository<Area>(new NeutronDb());

        private ISlot _slotName;
        private HeaderTextManager _headerTextManager;
        private List<LocationView> _currentList;
        private bool _startup = true;
        private readonly IIptiDisplayFunctions _iptiDisplayFunctions;

        public FrmLocations(IJsonData jsonData, IWorkstationRepository workstationRepository,
            WorkstationView workstationView, NeutronVariables neutronVariables, ILacProcessor lacProcessor,
            IHistoryManager historyManager, IIptiDisplayFunctions iptiDisplayFunctions)
        {
            InitializeComponent();
            _workstationRepository = workstationRepository;
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            _jsonData = jsonData;
            _workstationView = workstationView;
            _neutronVariables = neutronVariables;
            _lacProcessor = lacProcessor;
            //_historyManager = historyManager;
            _iptiDisplayFunctions = iptiDisplayFunctions;


            InitForm();
        }

        private void InitForm()
        {
            _logger = NeutronCore.Global.Logger.SetupLogger("Locations");

            KeyPreview = true;
            CloseButtonPressed = false;
            _headerTextManager = new HeaderTextManager();
            SetupGrid();
            HideTabControlTabs();
            SetupPrinters();
            mlUserInfo.Text = GlobalVar.User?.UserInfo;
            _locationRepository = new LocationsRepository();
            LabelStationName.Text = _workstationView.ToString();

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

            _startup = false;
            RefreshData();
        }

        public bool CloseButtonPressed { get; set; }

        private void SetupPrinters()
        {
            _documentPrinter = _jsonData.LoadFile<DocumentPrinterPreferences>();
            _labelPrinter = _jsonData.LoadFile<LabelPrinterPreferences>();
        }

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

        private void FrmLocations_Load(object sender, EventArgs e)
        {
        }

        // Set the focus to the passed in recId if it's passed in
        private void RefreshData(int recId = 0)
        {
            Cursor.Current = Cursors.WaitCursor;
            var idx = 0;
            var find = TextBoxFind.Text.ToLower().Trim();

            try
            {
                if (_workstationView != null)
                {
                    var area = ((Area)ComboBoxAreaNumber.SelectedItem);

                    var views = area.Name == "All Areas"
                        ? _locationRepository.FindLocationViewsBySlot(find).ToList()
                        : _locationRepository.FindLocationViewsByAreaAndSlot(area.Id, find).ToList();

                    List<LocationView> recs;
                    if (MButtonAllLocations.Text == _resourceManager.GetString("Available"))
                    {
                        recs = views;
                        // Create DataTable from List<LocationView>
                        _currentList = recs;
                    }
                    else
                    {
                        recs = views.Where(v => v.InUse == false).ToList();
                        // Create DataTable from List<LocationView>
                        _currentList = recs;
                    }
                    // convert List<LocationView> into a BindingSource

                    var blv = new BindingListView<LocationView>(recs.ToList());
                    _bindingSource.DataSource = blv;
                    DataGridView1.AutoGenerateColumns = false;
                    DataGridView1.DataSource = _bindingSource;
                    if (GetRecordCount(_bindingSource) > 0)
                    {
                        if (recId != 0) idx = IndexOf(recId);
                        DataGridView1.FirstDisplayedScrollingRowIndex = DataGridView1.Rows[idx].Index;
                        DataGridView1.Refresh();
                        DataGridView1.CurrentCell = DataGridView1.Rows[idx].Cells[1];
                        DataGridView1.Rows[idx].Selected = true;
                    }

                    DataGridView1.ClearSelection();
                    if (DataGridView1.RowCount > 0) DataGridView1.FastAutoSizeColumns();
                    Cursor.Current = Cursors.Default;
                }
            }
            catch (Exception ex)
            {
                var message = $"Error Loading Data: {Environment.NewLine}{ex.Message}";
                _logger.LogDetailAsync(message).SafeFireAndForget();
                Mediator.GetInstance().OnGeneralError(this, message);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        //                                                              
        private List<T> Concat<T>(params List<T>[] lists)
        {
            return lists.Aggregate(new List<T>(), (x, y) => x.Concat(y).ToList());
            ;
        }

        public int IndexOf(int value)
        {
            var count = _bindingSource.Count;
            var itemIndex = -1;
            for (var i = 0; i < count; i++)
            {
                var rec = ((ObjectView<LocationView>)_bindingSource[i]).Object.Id;
                if (rec == value)
                {
                    itemIndex = i;
                    break;
                }
            }

            return itemIndex;
        }
        private int GetRecordCount(BindingSource bindingSource)
        {
            var count = _bindingSource.Count;
            LabelRecordCount.Text = $"{_resourceManager.GetString("Records")}: {count}";
            return count;
        }

        private async void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            _logger.LogDetailAsync($"Cell Click").SafeFireAndForget();

            var grid = sender as DataGridView;
            var rowIndex = e.RowIndex;
            var colIndex = e.ColumnIndex;

            var quantity = "00";
            var qty = 0;
            var display = string.Empty;
            _logger.LogDetailAsync($"DataGridViewPosition").SafeFireAndForget();
            try
            {
                if (rowIndex < 0) return;
                if (grid == null) return;

                if (!grid.Columns.Contains("Position")) return;
                if (grid.CurrentCell.ColumnIndex != grid.Columns["Position"]?.Index) return;
                if (!grid.Columns.Contains(columnName: "Loc1")) return;
                var deviceNumber = grid["Loc1", rowIndex].Value.ToString().ParseInt();
                if (!grid.Columns.Contains(columnName: "Loc2")) return;
                var trayNumber = grid["Loc2", rowIndex].Value.ToString().ParseInt();
                if (!grid.Columns.Contains(columnName: "Loc3")) return;
                var level = grid["Loc3", rowIndex].Value.ToString().ParseInt();
                if (!grid.Columns.Contains(columnName: "Loc4")) return;
                var part = grid["Loc4", rowIndex].Value.ToString().ParseInt();

                // if there are Hanels on this station
                // check to see if this lift is already moving before trying to move again
                if (_workstationView.Hanels.Any())
                {
                    if (GlobalVar.Hanel == null)
                    {
                        return;
                    }
                    var status = GlobalVar.Hanel.GetDeviceStatus(deviceNumber);

                    if (status.CommandAccepted && !status.CommandExecuted)
                    {
                        var message =
                            $"Unable to move Tower {deviceNumber}.{Environment.NewLine}It is currently moving Tray {status.TargetTray} into position.";
                        MessageBox.Show(message, "Lift Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }

                // if it's a Blastzone, turn on the Location
                // and turn on the Prolite, if they're Enabled

                if (_workstationView.Blastzones.Any())
                {
                    if (_iptiDisplayFunctions != null)
                    {
                        await _iptiDisplayFunctions.ClearBlastzone();
                        await _iptiDisplayFunctions.TurnOnBlastzoneDisplay(trayNumber, part, quantity);
                        await _iptiDisplayFunctions.TurnOnBlastzoneOrderControl(trayNumber, $"Qty: {quantity}");
                    }
                }

                if (_workstationView.Prolites.Any())
                {
                    var proliteNumber = 1;
                    if (_workstationView.Prolites.Count > 1)
                    {
                        proliteNumber = deviceNumber;
                    }
                    // only one Prolite, so it doesn't matter what device number
                    // just light up the first Prolite
                    _workstationView.ProLiteManager?.ClearProlite(proliteNumber);
                    Thread.Sleep(500);
                    _workstationView.ProLiteManager?.TurnOnLocation(proliteNumber, trayNumber, level, part, qty);

                }

                // if there are Hanels on this station
                if (_workstationView.Hanels.Any())
                {
                    MoveDevice(deviceNumber, trayNumber, level, part, qty, display);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {Environment.NewLine} {ex.Message}");
            }
        }

        private void DataGridViewPosition(DataGridView grid, int rowIndex)
        {
            var qty = 0;
            var display = string.Empty;
            _logger.LogDetailAsync($"DataGridViewPosition").SafeFireAndForget();
            try
            {
                if (rowIndex < 0) return;
                if (!grid.Columns.Contains("Position")) return;
                if (grid.CurrentCell.ColumnIndex != grid.Columns["Position"]?.Index) return;


                if (!grid.Columns.Contains(columnName: "Loc1")) return;
                var deviceNumber = grid["Loc1", rowIndex].Value.ToString().ParseInt();
                if (!grid.Columns.Contains(columnName: "Loc2")) return;
                var trayNumber = grid["Loc2", rowIndex].Value.ToString().ParseInt();
                if (!grid.Columns.Contains(columnName: "Loc3")) return;
                var level = grid["Loc3", rowIndex].Value.ToString().ParseInt();
                if (!grid.Columns.Contains(columnName: "Loc4")) return;
                var part = grid["Loc4", rowIndex].Value.ToString().ParseInt();

                MoveDevice(deviceNumber, trayNumber, level, part, qty, display);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to move device. {Environment.NewLine} {ex.Message}");
            }
        }
        //private void ClearAllShi()
        //{
        //    if (_neutronVariables.DisplaysEnabled)
        //        if (GlobalVar.Displays != null)
        //           // GlobalVar.Displays.ClearAllShi();
        //}
        //private void TurnOnShi(int deviceNumber, int trayNumber, int level, string partition)
        //{
        //    string lArrow;
        //    string rArrow;
        //    // ClearAllShi();
        //    if (_neutronVariables.DisplaysEnabled)
        //        if (GlobalVar.Displays != null)
        //        {
        //            if (deviceNumber == 2 || deviceNumber == 4)
        //            {
        //                lArrow = "";
        //                rArrow = ">";
        //            }
        //            else
        //            {
        //                lArrow = "<";
        //                rArrow = "";
        //            }

        //            if (deviceNumber == 3 && _workstationView.WorkstationId == 3)
        //            {
        //                lArrow = "";
        //                rArrow = ">";
        //            }

        //            var text = $"{lArrow}-----{rArrow}";
        //          //  GlobalVar.Displays.ShowShi(deviceNumber, trayNumber, level, partition, text);
        //        }
        //}
        private void MoveDevice(int deviceNumber, int trayNumber, int level, int part, int quantity = 0, string display = "")
        {
            try
            {
                Thread.Sleep(100);
                if (_lacProcessor.MovePermitted(_workstationView.WorkstationId, deviceNumber, trayNumber))
                {
                    if (_neutronVariables.ShuttleEnabled)
                    {
                        //var hanels = _workstationView.HardwareDevices.Where(r => r.DeviceTypeId == (int)DeviceTypeEnum.Hanel12D).ToList();
                        //var hanel = hanels.FirstOrDefault(r => r.DeviceNumber == deviceNumber);

                        var hanel = _workstationView.Hanels.FirstOrDefault(r => r.DeviceNumber == deviceNumber);

                        //var hardwareDevice =
                        //    _workstationView.HardwareDevices.FirstOrDefault(s => s.DeviceNumber == deviceNumber);

                        if (hanel != null)
                        {
                            if (hanel.Enabled)
                            {
                                if (GlobalVar.Shuttle != null)
                                {
                                    var response = GlobalVar.Shuttle.PositionDevice(deviceNumber, trayNumber, level, part, quantity,
                                            display);
                                    if (response != DeviceResponse.Success)
                                    {
                                        MessageBox.Show(response.AsString(EnumFormat.Description),
                                            caption: string.Empty, buttons: MessageBoxButtons.OK,
                                            icon: MessageBoxIcon.Error);
                                    }
                                }
                                else if (GlobalVar.Hanel != null)
                                {
                                    _logger.LogDetailAsync($"Call Hanel.Position Device").SafeFireAndForget();
                                    var response = GlobalVar.Hanel.PositionDevice(deviceNumber, trayNumber, level, part, quantity,
                                            display);

                                    if (response != DeviceResponse.Success)
                                    {
                                        MessageBox.Show(response.AsString(EnumFormat.Description),
                                            caption: string.Empty, buttons: MessageBoxButtons.OK,
                                            icon: MessageBoxIcon.Error);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show(_resourceManager.GetString("Message11"));
                                }
                            }
                            else
                            {
                                MessageBox.Show(
                                    $"{_resourceManager.GetString("Message12")} - {hanel.Name}");
                            }
                        }
                        else
                        {
                            MessageBox.Show(_resourceManager.GetString("Message13"));
                        }
                    }
                }
                else
                {
                    _logger.LogDetailAsync($"Location Access Denied").SafeFireAndForget();
                    MessageBox.Show($"Location Access Denied");
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Move Device - Location Module: {Environment.NewLine}{ex.Message}").SafeFireAndForget();

            }

            _logger.LogDetailAsync($"MoveDevice END.").SafeFireAndForget();
        }

        private async Task<bool> SaveNew()
        {
            var result = false;
            var area = (Area)ComboBoxNewArea.SelectedItem;
            if (area == null) return false;
            var areaId = area.Id;

            var device = (StorageDevice)ComboBoxNewDevice.SelectedItem;
            if (device == null) return false;
            var deviceNumber = device.StorageDeviceNumber;

            // if StorageDeviceType is Rack
            // then Loc2, Loc3, Loc4, Loc5 must be 0
            // and the Slot is used to pick/store Location
            if (device.StorageDeviceType.Name == "Rack")
            {
                var slot = TextBoxNewSlot.Text;
                var rec = await _repoLocation.FindByFirstOrDefaultAsync(r =>
                    r.AreaId == areaId && r.Loc1 == deviceNumber && r.Slot == slot);
                if (rec == null)
                {
                    var loc = new Location
                    {
                        AreaId = areaId,
                        Loc1 = deviceNumber,
                        Loc2 = TextBoxNewLoc2.Text.ParseInt(),
                        Loc3 = TextBoxNewLoc3.Text.ParseInt(),
                        Loc4 = TextBoxNewLoc4.Text.ParseInt(),
                        Loc5 = TextBoxNewLoc5.Text.ParseInt(),
                        Slot = slot,
                        PickSequence = TextBoxNewPickSequence.Text.ParseInt(),
                        SizeCodeId = (int)ComboBoxNewSizeCode.SelectedValue,
                        VelocityCodeId = (int)ComboBoxNewVelocityCode.SelectedValue,
                        HeightCodeId = (int)ComboBoxNewHeightCode.SelectedValue,
                        LocationCode = TextBoxNewLocationCode.Text,
                        InUse = CheckBoxInUseNew.Checked
                    };
                    await _repoLocation.InsertAsync(loc);
                    await GlobalVar.HistoryManager.SaveHistoryAsync(ActionCode.LocationAdd, loc);
                    result = true;
                }
                else
                {
                    MessageBox.Show(_resourceManager.GetString("Message3"));
                }
            }
            else
            {
                if (IntegerValidator(TextBoxNewLoc2.Text.ParseInt()))
                {
                    var loc2 = TextBoxNewLoc2.Text.ParseInt();
                    if (IntegerValidator(TextBoxNewLoc3.Text.ParseInt()))
                    {
                        var loc3 = TextBoxNewLoc3.Text.ParseInt();
                        if (IntegerValidator(TextBoxNewLoc4.Text.ParseInt()))
                        {
                            var loc4 = TextBoxNewLoc4.Text.ParseInt();
                            if (IntegerValidator(TextBoxNewLoc5.Text.ParseInt()))
                            {
                                var loc5 = TextBoxNewLoc5.Text.ParseInt();

                                var rec = await _repoLocation.FindByAsync(r =>
                                    r.AreaId == areaId && r.Loc1 == deviceNumber && r.Loc2 == loc2
                                    && r.Loc3 == loc3 && r.Loc4 == loc4 && r.Loc5 == loc5);
                                if (!rec.Any())
                                {
                                    var slotName = TextBoxNewSlot.Text;
                                    //var slotName = GlobalVar.SlotNameFactory
                                    //    .CreateSlotName(areaId, deviceNumber, loc2, loc3, loc4, loc5).SlotName;
                                    var loc = new Location
                                    {
                                        AreaId = areaId,
                                        Loc1 = deviceNumber,
                                        Loc2 = loc2,
                                        Loc3 = loc3,
                                        Loc4 = loc4,
                                        Loc5 = loc5,
                                        Slot = slotName,
                                        PickSequence = TextBoxNewPickSequence.Text.ParseInt(),
                                        InUse = CheckBoxInUseNew.Checked,
                                        SizeCodeId = ((SizeCode)ComboBoxNewSizeCode.SelectedItem).Id,
                                        VelocityCodeId = ((VelocityCode)ComboBoxNewVelocityCode.SelectedItem).Id,
                                        HeightCodeId = ((HeightCode)ComboBoxNewHeightCode.SelectedItem).Id,
                                        LocationCode = TextBoxNewLocationCode.Text
                                    };
                                    TextBoxNewSlot.Text = slotName;
                                    try
                                    {
                                        await _repoLocation.InsertAsync(loc);
                                        await GlobalVar.HistoryManager.SaveHistoryAsync(ActionCode.LocationAdd, loc);
                                        result = true;
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show(_resourceManager.GetString("Message3") + ex.Message + "\n\r" +
                                                        ex.InnerException);
                                    }

                                    //RefreshData();
                                    //tabControl1.SelectedTab = tabPage1;
                                }
                                else
                                {
                                    MessageBox.Show(_resourceManager.GetString("Message4"), string.Empty,
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                            else
                            {
                                MessageBox.Show(_resourceManager.GetString("Message5"));
                            }
                        }
                        else
                        {
                            MessageBox.Show(_resourceManager.GetString("Message6"));
                        }
                    }
                    else
                    {
                        MessageBox.Show(_resourceManager.GetString("Message7"));
                    }
                }
                else
                {
                    MessageBox.Show(_resourceManager.GetString("Message8"));
                }
            }

            return result;
        }

        private async Task UpdateViewEdit()
        {
            var locationView = ((ObjectView<LocationView>)_bindingSource.Current).Object;
            if (locationView == null) return;
            var id = locationView.Id;
            var loc = await _repoLocation.FindByKeyAsync(id);
            if (loc == null) return;
            var area = (Area)ComboBoxViewEditArea.SelectedItem;
            if (area == null) return;
            var areaId = area.Id;

            var device = ((StorageDevice)ComboBoxViewEditDevice.SelectedItem);
            if (device == null) return;
            var deviceNumber = device.StorageDeviceNumber;
            // record the original values in History
            await GlobalVar.HistoryManager.SaveHistoryAsync(ActionCode.LocationModify, loc);

            //-------------------------------------------------

            // if StorageDeviceType is Rack
            // then Loc2, Loc3, Loc4, Loc5 must be 0
            // and the Slot is used to pick/store Location
            if (device.StorageDeviceType.Name == "Rack")
            {
                var slot = TextBoxViewEditSlot.Text;

                loc.AreaId = areaId;
                loc.Loc1 = deviceNumber;
                loc.Loc2 = TextBoxViewEditLoc2.Text.ParseInt();
                loc.Loc3 = TextBoxViewEditLoc3.Text.ParseInt();
                loc.Loc4 = TextBoxViewEditLoc4.Text.ParseInt();
                loc.Loc5 = TextBoxViewEditLoc5.Text.ParseInt();
                loc.Slot = slot;
                loc.PickSequence = TextBoxViewEditPickSequence.Text.ParseInt();
                loc.SizeCodeId = (int)ComboBoxViewEditSizeCode.SelectedValue;
                loc.VelocityCodeId = (int)ComboBoxViewEditVelocityCode.SelectedValue;
                loc.HeightCodeId = (int)ComboBoxViewEditHeightCode.SelectedValue;
                loc.LocationCode = TextBoxViewEditLocationCode.Text;
                loc.InUse = CheckBoxInUse.Checked;

                await _repoLocation.UpdateAsync(loc);
                await GlobalVar.HistoryManager.SaveHistoryAsync(ActionCode.LocationModify, loc);
            }
            else
            {
                if (IntegerValidator(TextBoxViewEditLoc2.Text.ParseInt()))
                {
                    var loc2 = TextBoxViewEditLoc2.Text.ParseInt();
                    if (IntegerValidator(TextBoxViewEditLoc3.Text.ParseInt()))
                    {
                        var loc3 = TextBoxViewEditLoc3.Text.ParseInt();
                        if (IntegerValidator(TextBoxViewEditLoc4.Text.ParseInt()))
                        {
                            var loc4 = TextBoxViewEditLoc4.Text.ParseInt();
                            if (IntegerValidator(TextBoxViewEditLoc5.Text.ParseInt()))
                            {
                                var loc5 = TextBoxViewEditLoc5.Text.ParseInt();

                                _slotName = GlobalVar.SlotNameFactory.CreateSlotName(areaId, deviceNumber, loc2, loc3, loc4,
                                    loc5);
                                var slotName = _slotName.SlotName;

                                loc.AreaId = areaId;
                                loc.Loc1 = deviceNumber;
                                loc.Loc2 = loc2;
                                loc.Loc3 = loc3;
                                loc.Loc4 = loc4;
                                loc.Loc5 = loc5;
                                loc.Slot = slotName;
                                loc.PickSequence = TextBoxViewEditPickSequence.Text.ParseInt();
                                loc.InUse = CheckBoxInUse.Checked;
                                loc.SizeCodeId = ((SizeCode)ComboBoxViewEditSizeCode.SelectedItem).Id;
                                loc.VelocityCodeId = ((VelocityCode)ComboBoxViewEditVelocityCode.SelectedItem).Id;
                                loc.HeightCodeId = ((HeightCode)ComboBoxViewEditHeightCode.SelectedItem).Id;
                                loc.LocationCode = TextBoxViewEditLocationCode.Text;

                                TextBoxViewEditSlot.Text = slotName;
                                try
                                {
                                    await _repoLocation.UpdateAsync(loc);
                                    await GlobalVar.HistoryManager.SaveHistoryAsync(ActionCode.LocationModify, loc);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"{_resourceManager.GetString("Message9")}{Environment.NewLine}" +
                                                    $"{ex.Message}{Environment.NewLine} {ex.InnerException}");
                                }

                                RefreshData();
                                tabControl1.SelectedTab = tabPage1;
                            }
                            else
                            {
                                MessageBox.Show(_resourceManager.GetString("Message10"));
                            }
                        }
                        else
                        {
                            MessageBox.Show(_resourceManager.GetString("Message11"));
                        }
                    }
                    else
                    {
                        MessageBox.Show(_resourceManager.GetString("Message12"));
                    }
                }

                else
                {
                    MessageBox.Show(_resourceManager.GetString("Message13"));
                }
            }
        }

        private bool ValidateFields(Location rec)
        {
            if (!IntegerValidator(rec.Loc1)) return false;
            if (!IntegerValidator(rec.Loc2)) return false;
            if (!IntegerValidator(rec.Loc3)) return false;
            if (!IntegerValidator(rec.Loc4)) return false;
            if (!IntegerValidator(rec.Loc5)) return false;
            return true;
        }

        private bool StringValidator(string input)
        {
            var pattern = "[^a-zA-Z]";
            if (Regex.IsMatch(input, pattern))
                return true;
            return false;
        }

        //validate integer 
        private bool IntegerValidator(int input)
        {
            var pattern = "^[0-9]+$";
            if (Regex.IsMatch(input.ToString(), pattern))
            {
                if (input < 0)
                {
                    MessageBox.Show(_resourceManager.GetString("Message14"));
                    return false;
                }

                return true;
            }

            return false;
        }

        private bool IsDuplicate(Location loc)
        {
            var result = false;
            try
            {
                var rec = _repoLocation.All().FirstOrDefault(r =>
                    r.AreaId == loc.AreaId && r.Loc1 == loc.Loc1 && r.Loc2 == loc.Loc2
                    && r.Loc3 == loc.Loc3 && r.Loc4 == loc.Loc4 && r.Loc5 == loc.Loc5);
                if (rec != null)
                {
                    MessageBox.Show(_resourceManager.GetString("Message15"), string.Empty, MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    result = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{_resourceManager.GetString("Message16")}{Environment.NewLine}" +
                                $"{ex.Message}{Environment.NewLine}{ex.InnerException}");
            }

            return result;
        }

        private async void MbViewEditDelete_Click(object sender, EventArgs e)
        {
            var locationView = ((ObjectView<LocationView>)_bindingSource.Current).Object;
            var loc = await _repoLocation.FindByKeyAsync(locationView.Id);
            if (!await LocationHasInventory(loc.Id))
            {
                var result = MessageBox.Show(_resourceManager.GetString("Message17"), string.Empty,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes) return;
                var deleted = await _repoLocation.DeleteAsync(loc.Id);
                await GlobalVar.HistoryManager.SaveHistoryAsync(ActionCode.LocationDelete, loc);
                RefreshData();
                tabControl1.SelectedTab = tabPage1;
            }
            else
            {
                MessageBox.Show(_resourceManager.GetString("Message18"));
            }
        }

        /// <summary>
        /// Checks to see if this location is in the Inventory table
        /// It will return true even if the quantity is zero
        /// </summary>
        /// <param name="locationId">This is the Id field in <see cref="Location">Location class</see>/></param>
        /// <returns></returns>
        private async Task<bool> LocationHasInventory(int locationId)
        {
            var result = await _locationRepository.IsInInventory(locationId);
            return result;
        }
        /// <summary>
        /// Toggles the button between Show All and Available
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MButtonAllLocations_Click(object sender, EventArgs e)
        {
            if (MButtonAllLocations.Text == _resourceManager.GetString("ShowAll"))
            {
                MButtonAllLocations.Text = _resourceManager.GetString("Available");
                LabelFormTitle.Text = _resourceManager.GetString("AllLocations");
            }
            //else (MButtonAllLocations.Text == _resourceManager.GetString("ShowAll"))
            else
            {
                MButtonAllLocations.Text = _resourceManager.GetString("ShowAll");
                LabelFormTitle.Text = _resourceManager.GetString("AvailableLocations");
            }

            RefreshData();
        }

        private void FrmLocations_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !CloseButtonPressed;
        }
        private void MbSaveAsDefault_Click(object sender, EventArgs e)
        {
            var area = ((Area)ComboBoxNewArea.SelectedItem);
            if (area == null) return;
            var loc1 = ((StorageDevice)ComboBoxNewDevice.SelectedItem)?.StorageDeviceNumber ?? 1;
            var loc2 = string.IsNullOrEmpty(TextBoxNewLoc2.Text) ? "0" : TextBoxNewLoc2.Text;
            var loc3 = string.IsNullOrEmpty(TextBoxNewLoc3.Text) ? "0" : TextBoxNewLoc3.Text;
            var loc4 = string.IsNullOrEmpty(TextBoxNewLoc4.Text) ? "0" : TextBoxNewLoc4.Text;
            var loc5 = string.IsNullOrEmpty(TextBoxNewLoc5.Text) ? "0" : TextBoxNewLoc5.Text;
            var rec = new Location
            {
                AreaId = area.Id,
                Loc1 = loc1,
                Loc2 = loc2.ParseInt(),
                Loc3 = loc3.ParseInt(),
                Loc4 = loc4.ParseInt(),
                Loc5 = loc5.ParseInt(),
                Slot = TextBoxNewSlot.Text,
                PickSequence = TextBoxNewPickSequence.Text.ParseInt(),
                SizeCodeId = ((SizeCode)ComboBoxNewSizeCode.SelectedItem).Id,
                VelocityCodeId = ((VelocityCode)ComboBoxNewVelocityCode.SelectedItem).Id,
                HeightCodeId = ((HeightCode)ComboBoxNewHeightCode.SelectedItem).Id,
                LocationCode = TextBoxNewLocationCode.Text,
                InUse = CheckBoxInUseNew.Checked
            };
            try
            {
                _jsonData.SaveFile(rec);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{_resourceManager.GetString("Message19")}{Environment.NewLine}" +
                                $"{ex.Message}{Environment.NewLine}{ex.InnerException}");
            }
        }

        private void MbLoadDefault_Click(object sender, EventArgs e)
        {
            var location = _jsonData.LoadFile<Location>();
            ComboBoxNewArea.SelectedValue = location.AreaId;
            ComboBoxAreaNumber.SelectedValue = location.AreaId;

            var locId = _repoDevices.FindBy(r => r.StorageDeviceNumber == location.Loc1).FirstOrDefault();
            if (locId == null) return;

            ComboBoxNewDevice.SelectedValue = locId;
            TextBoxNewLoc2.Text = location.Loc2.ToString();
            TextBoxNewLoc3.Text = location.Loc3.ToString();
            TextBoxNewLoc4.Text = location.Loc4.ToString();
            TextBoxNewLoc5.Text = location.Loc5.ToString();
            TextBoxNewSlot.Text = location.Slot;
            ComboBoxNewSizeCode.SelectedValue = location.SizeCodeId;
            ComboBoxNewVelocityCode.SelectedValue = location.VelocityCodeId;
            ComboBoxNewHeightCode.SelectedValue = location.HeightCodeId;
            TextBoxNewLocationCode.Text = location.LocationCode;
            CheckBoxInUseNew.Checked = location.InUse;
            //if (!location.Area.Devices.Any()) return;
            //var storageDevice = location.Area.Devices.FirstOrDefault();
            //if (storageDevice == null ) return;

            ButtonPositionDevice.Visible = WorkstationCanPositionDevice();
        }

        private List<Location> GetAvailableLocations()
        {
            var areaId = ((Area)ComboBoxAreaNumber.SelectedItem).Id;
            List<Location> recs;
            using (var context = new NeutronDb())
            {
                recs = context.Locations.Include("Area")
                    .Include("SizeCode")
                    .Include("VelocityCode")
                    .Include("HeightCode")
                    .Where(l => l.InUse == false && l.AreaId == areaId).ToList();
            }

            return recs;
        }

        private void ComboBoxNewArea_SelectedIndexChanged(object sender, EventArgs e)
        {
            var area = ((Area)ComboBoxNewArea.SelectedItem);
            if (area == null) return;

            ComboBoxNewDevice.DataSource = _repoDevices.All().Where(d => d.AreaId == area.Id).ToList();
            ComboBoxNewDevice.DisplayMember = "Name";
            ComboBoxNewDevice.ValueMember = "Id";
            ComboBoxNewDevice.Refresh();

            LabelNewDevice.Text = area.LocationType.Loc1Label;
            LabelNewTray.Text = area.LocationType.Loc2Label;
            LabelNewOver.Text = area.LocationType.Loc3Label;
            LabelNewBack.Text = area.LocationType.Loc4Label;
            LabelNewTag.Text = area.LocationType.Loc5Label;

            if (ComboBoxNewDevice.Items.Count <= 0) return;
            var device = (StorageDevice)ComboBoxNewDevice.Items[0];

            ButtonPositionDeviceNew.Visible = WorkstationCanPositionDevice();

        }
        private void ComboBoxViewEditArea_SelectedIndexChanged(object sender, EventArgs e)
        {
            var area = ((Area)ComboBoxViewEditArea.SelectedItem);
            if (area == null) return;

            var devices = _repoDevices.All().Where(d => d.AreaId == area.Id).ToList();
            ComboBoxViewEditDevice.DataSource = devices;

            ComboBoxViewEditDevice.DisplayMember = "Name";
            ComboBoxViewEditDevice.ValueMember = "Id";
            ComboBoxViewEditDevice.Refresh();

            LabelViewEditDevice.Text = area.LocationType.Loc1Label;
            LabelViewEditTray.Text = area.LocationType.Loc2Label;
            LabelViewEditOver.Text = area.LocationType.Loc3Label;
            LabelViewEditBack.Text = area.LocationType.Loc4Label;
            LabelViewEditTag.Text = area.LocationType.Loc5Label;

            if (ComboBoxViewEditDevice.Items.Count <= 0) return;
            var device = (StorageDevice)ComboBoxViewEditDevice.Items[0];
            ButtonPositionDevice.Visible = WorkstationCanPositionDevice();

        }
        /// <summary>
        /// If a device is a carousel, vertical or blastzone then it can be positioned
        /// so return true
        /// </summary>
        /// <returns></returns>
        private bool WorkstationCanPositionDevice()
        {
            var moveableAreas = new int[] { 1, 2, 3, 4 };
           // return _workstationView.AreaId != 8 && _workstationView.AreaId == ((Area)ComboBoxAreaNumber.SelectedItem).AreaNumber;
           return moveableAreas.Contains(_workstationView.AreaId);
        }

        #region Find Functions
        /// <summary>
        /// Handles the Click event of the MButtonFind control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void MButtonFind_Click(object sender, EventArgs e)
        {
            RefreshData();
        }

        /// <summary>
        /// Handles the KeyDown event of the Find TextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="KeyEventArgs"/> that contains the event data.</param>
        /// <remarks>
        /// If the Return key is pressed, the data is refreshed.
        /// </remarks>
        private void HandleFindTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (IsReturnKeyPressed(e.KeyCode))
            {
                RefreshData();
            }
        }
        private bool IsReturnKeyPressed(Keys key)
        {
            return key == Keys.Return;
        }

        #endregion

        #region Button Clicks

        /// <summary>
        /// Handles the Click event of the Clear button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        /// <remarks>
        /// When the Clear button is clicked, this method clears the search box, refreshes the data, and sets the focus back to the search box.
        /// </remarks>
        private void ButtonClear_Click(object sender, EventArgs e)
        {
            ClearSearchBox();
            RefreshData();
            SetFocusToSearchBox();
        }

        // Clears the text in the search box
        private void ClearSearchBox()
        {
            TextBoxFind.Text = string.Empty;
        }
        // Sets the focus to the search box
        private void SetFocusToSearchBox()
        {
            TextBoxFind.Focus();
        }

        /// <summary>
        /// Handles the Click event of the MButtonClose control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// This method is asynchronous and will close the form when invoked.
        /// </remarks>
        private async void MButtonClose_Click(object sender, EventArgs e)
        {
            await CloseFormAsync();
        }

        /// <summary>
        /// Closes the form and performs necessary cleanup operations.
        /// </summary>
        private async Task CloseFormAsync()
        {
            // Indicate that the close button was pressed
            CloseButtonPressed = true;
            // Clear all Prolites if the ProLiteManager is available
            _workstationView?.ProLiteManager?.ClearAllProlites();
            // Clear the Blastzone if the IPTI Display Functions are available
            if (_iptiDisplayFunctions != null)
            {
                await _iptiDisplayFunctions.ClearBlastzone().ConfigureAwait(false);
            }
            // Close the form
            Close();
        }


        private void MButtonViewEdit_Click(object sender, EventArgs e)
        {
            ViewEditLocation();
        }

        private void ViewEditLocation()
        {
            if (DataGridView1.RowCount <= 0) return;
            SetupViewEditForm();
            // LoadViewEditData();
            //var grid = (LocationView)DataGridView1;
            //if (grid.CurrentRow != null)
            //{
            //    var location = (LocationView) grid.CurrentRow.DataBoundItem;
            //}
            tabControl1.SelectedTab = tabPage2;
        }

        //private void LoadViewEditData()
        //{

        //    var id = ((ObjectView<LocationView>)_bindingSource.Current).Object.Id;
        //    var location = _repoLocation.FindByKey(id);
        //    if (location is null) return;

        //    var device = _repoDevices.FindBy(r => r.StorageDeviceNumber == location.Loc1
        //                                            && r.AreaId == location.AreaId).FirstOrDefault();
        //    if (device is null) return;

        //    var area = _repoArea.FindBy(r => r.Id == location.AreaId).FirstOrDefault();
        //    if (area is null) return;

        //    ComboBoxViewEditArea.SelectedValue = area.Id;
        //    ComboBoxViewEditDevice.SelectedValue = device.Id;
        //    TextBoxViewEditLoc2.Text = location.Loc2.ToString();
        //    TextBoxViewEditLoc3.Text = location.Loc3.ToString();
        //    TextBoxViewEditLoc4.Text = location.Loc4.ToString();
        //    TextBoxViewEditLoc5.Text = location.Loc5.ToString();
        //    TextBoxViewEditSlot.Text = location.Slot;
        //    TextBoxViewEditPickSequence.Text = location.PickSequence.ToString();
        //    ComboBoxViewEditSizeCode.SelectedValue = location.SizeCodeId;
        //    ComboBoxViewEditVelocityCode.SelectedValue = location.VelocityCodeId;
        //    ComboBoxViewEditHeightCode.SelectedValue = location.HeightCodeId;
        //    TextBoxViewEditLocationCode.Text = location.LocationCode;
        //    CheckBoxInUse.Checked = location.InUse;

        //    ButtonPositionDevice.Visible = WorkstationCanPositionDevice();
        //}

        private void MButtonNew_Click(object sender, EventArgs e)
        {
            SetupNewForm();
            tabControl1.SelectedTab = tabPage3;
        }

        private void MbViewEditListing_Click(object sender, EventArgs e)
        {
            RefreshData();
            //SetupViewEditForm();
            tabControl1.SelectedTab = tabPage1;
        }

        //private void MbViewEditNew_Click(object sender, EventArgs e)
        //{
        //    tabControl1.SelectedTab = tabPage3;
        //}

        private void MbViewEditClose_Click(object sender, EventArgs e)
        {
            RefreshData();
            tabControl1.SelectedTab = tabPage1;
        }

        private async void MbViewEditSave_Click(object sender, EventArgs e)
        {
            await UpdateViewEdit();
            RefreshData();
            tabControl1.SelectedTab = tabPage1;
        }

        private void MbNewListing_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage1;
        }

        //private void MbNewViewEdit_Click(object sender, EventArgs e)
        //{
        //    tabControl1.SelectedTab = tabPage2;
        //}

        private async void MbNewSave_Click(object sender, EventArgs e)
        {
            if (!await SaveNew()) return;
            RefreshData();
            tabControl1.SelectedTab = tabPage1;
        }

        private void MbNewClose_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage1;
        }

        #endregion

        #region Form Setup

        private void SetupGrid()
        {

            DataGridView1.AutoGenerateColumns = false;
            DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            var bCol = new DataGridViewButtonColumn
            {
                HeaderText = string.Empty,
                Visible = _workstationView.AreaId != 8,
                //Visible = _workstationView.StationTypeId != (int)NeutronCore.Enums.StationType.RackTablet &&
                //          _workstationView.StationTypeId != (int)NeutronCore.Enums.StationType.Supervisor,
                Name = "Position",
                Text = _gridResourceManager.GetString("Position"),
                UseColumnTextForButtonValue = true,
            };



            DataGridView1.Columns.Add(bCol);
            var xcol = new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "InUse",
                HeaderText = _gridResourceManager.GetString("InUse"),
                Name = "InUse"
            };
            DataGridView1.Columns.Add(xcol);
            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "AreaId",
                HeaderText = _gridResourceManager.GetString("Area"),
                Name = "AreaId"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc1",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc1", _gridResourceManager),
                Name = "Loc1"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc2",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc2", _gridResourceManager),
                Name = "Loc2"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc3",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc3", _gridResourceManager),
                Name = "Loc3"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc4",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc4", _gridResourceManager),
                Name = "Loc4"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc5",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc5", _gridResourceManager),
                Name = "Loc5"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Slot",
                HeaderText = _gridResourceManager.GetString("Slot"),
                Visible = true,
                Name = "Slot"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PickSequence",
                HeaderText = _gridResourceManager.GetString("PickSequence"),
                Visible = true,
                Name = "PickSequence"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SizeCodeName",
                HeaderText = _gridResourceManager.GetString("SizeCodeName"),
                Name = "SizeCodeName"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "VelocityCodeName",
                HeaderText = _gridResourceManager.GetString("VelocityCodeName"),
                Name = "VelocityCodeName"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "HeightCodeName",
                HeaderText = _gridResourceManager.GetString("HeightCodeName"),
                Name = "HeightCodeName"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LocationCode",
                HeaderText = _gridResourceManager.GetString("LocationCode"),
                Name = "LocationCode"
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
            DataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Bold);

            //foreach (DataGridViewColumn column in DataGridView1.Columns)
            //{
            //    column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //    column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Bold);
            //    //column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);
            //}
        }


        //private void SetupTabControl()
        //{
        //    tabControl1.Appearance = TabAppearance.FlatButtons;
        //    tabControl1.ItemSize = new Size(0, 1);
        //    tabControl1.SizeMode = TabSizeMode.Fixed;
        //    foreach (TabPage tab in tabControl1.TabPages) tab.Text = string.Empty;
        //}

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

        private void SetupNewForm()
        {
            //var id = ((ObjectView<LocationView>)_bindingSource.Current).Object.Id;
            //var location = _repoLocation.FindByKey(id);
            //if (location != null)
            //{
            //    var device = _repoDevices.FindBy(r => r.StorageDeviceNumber == location.Loc1
            //                                          && r.AreaId == location.AreaId).FirstOrDefault();
            //    if (device is null) return;
            //    ButtonPositionDeviceNew.Visible = WorkstationCanPositionDevice();

            //    var area = _repoArea.FindBy(r => r.Id == location.AreaId).FirstOrDefault();
            //    if (area is null) return;

            //    ComboBoxNewArea.DataSource = _repoArea.All();
            //    ComboBoxNewArea.DisplayMember = "Name";
            //    ComboBoxNewArea.ValueMember = "Id";

            //    ComboBoxNewArea.SelectedValue = area.Id;

            //    ComboBoxNewDevice.DataSource = _repoDevices.All().Where(d => d.AreaId == area.Id).ToList();
            //    ComboBoxNewDevice.DisplayMember = "Name";
            //    ComboBoxNewDevice.ValueMember = "Id";

            //    ComboBoxNewDevice.SelectedValue = device.Id;
            //}
            //else
            //{
            ComboBoxNewArea.DataSource = _repoArea.All();
            ComboBoxNewArea.DisplayMember = "Name";
            ComboBoxNewArea.ValueMember = "Id";
            ComboBoxNewArea.SelectedIndex = ComboBoxNewArea.FindString(_workstationView.Area.Name);

            ComboBoxNewDevice.DataSource = _repoDevices.All().Where(d => d.AreaId == _workstationView.AreaId).ToList();
            ComboBoxNewDevice.DisplayMember = "Name";
            ComboBoxNewDevice.ValueMember = "Id";
            // }

            //LabelFindDescription.Text = "Search any part of Slot field";
            ComboBoxNewSizeCode.DataSource = _repoSizeCode.All();
            ComboBoxNewSizeCode.DisplayMember = "Name";
            ComboBoxNewSizeCode.ValueMember = "Id";
            ComboBoxNewVelocityCode.DataSource = _repoVelocityCode.All();
            ComboBoxNewVelocityCode.DisplayMember = "Name";
            ComboBoxNewVelocityCode.ValueMember = "Id";
            ComboBoxNewHeightCode.DataSource = _repoHeightCode.All();
            ComboBoxNewHeightCode.DisplayMember = "Name";
            ComboBoxNewHeightCode.ValueMember = "Id";

            TextBoxNewLoc2.Text = string.Empty;
            TextBoxNewLoc3.Text = string.Empty;
            TextBoxNewLoc4.Text = string.Empty;
            TextBoxNewLoc5.Text = string.Empty;
            TextBoxNewSlot.Text = string.Empty;





        }

        private void SetupViewEditForm()
        {
            var id = ((ObjectView<LocationView>)_bindingSource.Current).Object.Id;
            var location = _repoLocation.FindByKey(id);
            if (location == null) return;
            var area = _repoArea.FindBy(r => r.Id == location.AreaId).FirstOrDefault();
            if (area is null) return;
            //if (location != null)
            //{
            var device = _repoDevices.FindBy(r => r.StorageDeviceNumber == location.Loc1
                                                  && r.AreaId == location.AreaId).FirstOrDefault();
            if (device is null) return;



            ComboBoxViewEditArea.DataSource = _repoArea.All();
            ComboBoxViewEditArea.DisplayMember = "Name";
            ComboBoxViewEditArea.ValueMember = "Id";

            ComboBoxViewEditArea.SelectedValue = area.Id;

            ComboBoxViewEditDevice.DataSource = _repoDevices.All().Where(d => d.AreaId == area.Id).ToList();
            ComboBoxViewEditDevice.DisplayMember = "Name";
            ComboBoxViewEditDevice.ValueMember = "Id";

            ComboBoxViewEditDevice.SelectedValue = device.Id;

            TextBoxViewEditLoc2.Text = location.Loc2.ToString();
            TextBoxViewEditLoc3.Text = location.Loc3.ToString();
            TextBoxViewEditLoc4.Text = location.Loc4.ToString();
            TextBoxViewEditLoc5.Text = location.Loc5.ToString();
            TextBoxViewEditSlot.Text = location.Slot;
            TextBoxViewEditPickSequence.Text = location.PickSequence.ToString();

            ComboBoxViewEditSizeCode.DataSource = _repoSizeCode.All();
            ComboBoxViewEditSizeCode.DisplayMember = "Name";
            ComboBoxViewEditSizeCode.ValueMember = "Id";
            ComboBoxViewEditSizeCode.SelectedValue = location.SizeCodeId;

            ComboBoxViewEditVelocityCode.DataSource = _repoVelocityCode.All();
            ComboBoxViewEditVelocityCode.DisplayMember = "Name";
            ComboBoxViewEditVelocityCode.ValueMember = "Id";
            ComboBoxViewEditVelocityCode.SelectedValue = location.VelocityCodeId;

            ComboBoxViewEditHeightCode.DataSource = _repoHeightCode.All();
            ComboBoxViewEditHeightCode.DisplayMember = "Name";
            ComboBoxViewEditHeightCode.ValueMember = "Id";
            ComboBoxViewEditHeightCode.SelectedValue = location.HeightCodeId;
            TextBoxViewEditLocationCode.Text = location.LocationCode;
            CheckBoxInUse.Checked = location.InUse;

            ButtonPositionDevice.Visible = WorkstationCanPositionDevice();

        }

        #endregion

        #region Return Key Functions

        //private void ComboBoxNewDevice_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return) TextBoxNewLoc2.Focus();
        //}

        //private void TextBoxNewLoc2_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return) TextBoxNewLoc3.Focus();
        //}

        //private void TextBoxNewLoc3_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return) TextBoxNewLoc4.Focus();
        //}

        //private void TextBoxNewLoc4_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return) TextBoxNewLoc5.Focus();
        //}

        //private void TextBoxNewLoc5_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return) ComboBoxNewSizeCode.Focus();
        //}

        //private void ComboBoxNewSizeCode_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return) ComboBoxNewVelocityCode.Focus();
        //}

        //private void ComboBoxNewVelocityCode_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return) ComboBoxNewHeightCode.Focus();
        //}

        //private void ComboBoxViewEditDevice_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return) TextBoxViewEditLoc2.Focus();
        //}

        //private void TextBoxViewEditLoc2_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return) TextBoxViewEditLoc3.Focus();
        //}

        //private void TextBoxViewEditLoc3_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return) TextBoxViewEditLoc4.Focus();
        //}

        //private void TextBoxViewEditLoc4_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return) TextBoxViewEditLoc5.Focus();
        //}

        //private void TextBoxViewEditLoc5_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return) ComboBoxViewEditSizeCode.Focus();
        //}

        //private void ComboBoxViewEditSizeCode_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return) ComboBoxViewEditVelocityCode.Focus();
        //}

        //private void ComboBoxViewEditVelocityCode_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return) ComboBoxViewEditHeightCode.Focus();
        //}

        private void tabControl1_Enter(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 1) ComboBoxViewEditDevice.Focus();
            if (tabControl1.SelectedIndex == 2) ComboBoxNewDevice.Focus();
        }

        #endregion

        private void SetCulture(string lang)
        {
            try
            {
                var languageDirectory = LoaderSettings.GetLanguageDirectory();
                _cultureInfo = CultureInfo.CreateSpecificCulture(lang);
                _resourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "FrmLocations",
                    resourceDir: languageDirectory, usingResourceSet: null);
                _gridResourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "GridHeaders",
                    resourceDir: languageDirectory, usingResourceSet: null);
                LabelFormHeaderText.Text = _resourceManager.GetString("NeutronWarehouseMana");
                LabelFormTitle.Text = _resourceManager.GetString("AvailableLocations");
                MButtonNew.Text = _resourceManager.GetString("New");
                ButtonLoadFromExcel.Text = _resourceManager.GetString("LoadFromExcel");
                ButtonSaveToExcel.Text = _resourceManager.GetString("SaveToExcel");
                MButtonAllLocations.Text = _resourceManager.GetString("Available");
                MButtonViewEdit.Text = _resourceManager.GetString("View/Edit");
                MButtonClose.Text = _resourceManager.GetString("Home");
                MButtonSearch.Text = _resourceManager.GetString("Search");
                LabelAction.Text = _resourceManager.GetString("View/Edit");
                MbViewEditListing.Text = _resourceManager.GetString("Listing");
                MbViewEditDelete.Text = _resourceManager.GetString("Delete");
                MbViewEditClose.Text = _resourceManager.GetString("Back");
                MbViewEditSave.Text = _resourceManager.GetString("Save");
                CheckBoxInUse.Text = _resourceManager.GetString("InUse");
                LabelViewEditLocationCode.Text = _resourceManager.GetString("LocationCode");
                LabelViewEditHeight.Text = _resourceManager.GetString("Height");
                LabelViewEditVelocity.Text = _resourceManager.GetString("Velocity");
                LabelViewEditSlot.Text = _resourceManager.GetString("Slot");
                LabelViewEditSize.Text = _resourceManager.GetString("Size");
                LabelViewEditTag.Text = _resourceManager.GetString("Tag");
                LabelViewEditBack.Text = _resourceManager.GetString("Back");
                LabelViewEditOver.Text = _resourceManager.GetString("Over");
                LabelViewEditTray.Text = _resourceManager.GetString("Tray");
                LabelViewEditDevice.Text = _resourceManager.GetString("Device");
                LabelViewEditArea.Text = _resourceManager.GetString("Area");
                LabelViewEditArea.Text = _resourceManager.GetString("Area");
                MbLoadDefault.Text = _resourceManager.GetString("LoadDefault");
                MbSaveAsDefault.Text = _resourceManager.GetString("SaveAsDefault");
                LabelActionNew.Text = _resourceManager.GetString("New");
                MbNewListing.Text = _resourceManager.GetString("Listing");
                MbNewClose.Text = _resourceManager.GetString("Back");
                MbNewSave.Text = _resourceManager.GetString("Save");
                LabelNewArea.Text = _resourceManager.GetString("Area");
                CheckBoxInUseNew.Text = _resourceManager.GetString("InUse");
                LabelNewLocationCode.Text = _resourceManager.GetString("LocationCode");
                LabelNewSlot.Text = _resourceManager.GetString("Slot");
                LabelNewHeight.Text = _resourceManager.GetString("Height");
                LabelNewVelocity.Text = _resourceManager.GetString("Velocity");
                LabelNewSize.Text = _resourceManager.GetString("Size");
                LabelNewTag.Text = _resourceManager.GetString("Tag");
                LabelNewBack.Text = _resourceManager.GetString("Back");
                LabelNewOver.Text = _resourceManager.GetString("Over");
                LabelNewTray.Text = _resourceManager.GetString("Tray");
                LabelNewDevice.Text = _resourceManager.GetString("Device");
                LabelFindDescription.Text = _resourceManager.GetString("SearchAnyPartOfSlot");
                ButtonPositionDevice.Text = _resourceManager.GetString("PositionDevice");
                ButtonPositionDeviceNew.Text = _resourceManager.GetString("PositionDevice");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading language file.  {ex.Message} {Environment.NewLine} {ex.InnerException} ");
            }
        }

        private void ButtonPositionDevice_Click(object sender, EventArgs e)
        {
            var device = (StorageDevice)ComboBoxViewEditDevice.SelectedItem;
            if (device == null) return;
            var deviceNumber = device.StorageDeviceNumber;
            if (!IntegerValidator(TextBoxViewEditLoc2.Text.ParseInt())) return;
            var trayNumber = TextBoxViewEditLoc2.Text.ParseInt();
            if (!IntegerValidator(TextBoxViewEditLoc3.Text.ParseInt())) return;
            var level = TextBoxViewEditLoc3.Text.ParseInt();
            if (!IntegerValidator(TextBoxViewEditLoc4.Text.ParseInt())) return;
            var part = TextBoxViewEditLoc4.Text.ParseInt();
            _logger.LogDetailAsync($"Storage Device: {deviceNumber} Tray: {trayNumber} Level: {level} Part: {part}").SafeFireAndForget();
            MoveDevice(deviceNumber, trayNumber, level, part);
        }

        private void ButtonPositionDeviceNew_Click(object sender, EventArgs e)
        {
            var device = ((StorageDevice)ComboBoxNewDevice.SelectedItem);
            if (device == null) return;
            var deviceNumber = device.StorageDeviceNumber;
            if (string.IsNullOrEmpty(TextBoxNewLoc2.Text) || TextBoxNewLoc2.Text == "0") return;
            if (!IntegerValidator(TextBoxNewLoc2.Text.ParseInt())) return;
            var trayNumber = TextBoxNewLoc2.Text.ParseInt();
            if (string.IsNullOrEmpty(TextBoxNewLoc3.Text) || TextBoxNewLoc3.Text == "0") return;
            if (!IntegerValidator(TextBoxNewLoc3.Text.ParseInt())) return;
            var level = TextBoxNewLoc3.Text.ParseInt();
            if (string.IsNullOrEmpty(TextBoxNewLoc4.Text) || TextBoxNewLoc4.Text == "0") return;
            if (!IntegerValidator(TextBoxNewLoc4.Text.ParseInt())) return;
            var part = TextBoxNewLoc4.Text.ParseInt();
            _logger.LogDetailAsync($"Storage Device: {deviceNumber} Tray: {trayNumber} Level: {level} Part: {part}").SafeFireAndForget();
            MoveDevice(deviceNumber, trayNumber, level, part);
        }

        private void GetSlotString(TextBox textBox, bool newLocation)
        {
            if (_workstationView.Area.LocationTypeId == (int)LocationTypeEnum.Rack) return;

            if (newLocation)
            {
                try
                {

                    TextBoxNewSlot.Text = string.Empty;

                    var area = ((Area)ComboBoxNewArea.SelectedItem);
                    if (area == null) return;
                    var areaNumber = area.AreaNumber;

                    var device = ((StorageDevice)ComboBoxNewDevice.SelectedItem);
                    if (device == null) return;
                    var deviceNumber = device.StorageDeviceNumber;

                    if (string.IsNullOrEmpty(TextBoxNewLoc2.Text) || TextBoxNewLoc2.Text == "0") return;
                    if (!IntegerValidator(TextBoxNewLoc2.Text.ParseInt())) return;
                    var loc2 = TextBoxNewLoc2.Text.ParseInt();

                    if (string.IsNullOrEmpty(TextBoxNewLoc3.Text) || TextBoxNewLoc3.Text == "0") return;
                    if (!IntegerValidator(TextBoxNewLoc3.Text.ParseInt())) return;
                    var loc3 = TextBoxNewLoc3.Text.ParseInt();

                    if (string.IsNullOrEmpty(TextBoxNewLoc4.Text) || TextBoxNewLoc4.Text == "0") return;
                    if (!IntegerValidator(TextBoxNewLoc4.Text.ParseInt())) return;
                    var loc4 = TextBoxNewLoc4.Text.ParseInt();

                    if (string.IsNullOrEmpty(TextBoxNewLoc5.Text) || TextBoxNewLoc5.Text == "0")
                    {
                        TextBoxNewLoc5.Text = "1";
                    }
                    if (!IntegerValidator(TextBoxNewLoc5.Text.ParseInt())) return;
                    var loc5 = TextBoxNewLoc5.Text.ParseInt();

                    textBox.Text = GlobalVar.SlotNameFactory
                        .CreateSlotName(areaNumber, deviceNumber, loc2, loc3, loc4, loc5).SlotName;

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"GetSlotString Error: {ex.Message}");
                    throw;
                }
            }
            else  //ViewEditLocation
            {
                try
                {

                    TextBoxViewEditSlot.Text = string.Empty;

                    var area = ((Area)ComboBoxViewEditArea.SelectedItem);
                    if (area == null) return;
                    var areaNumber = area.AreaNumber;

                    var device = ((StorageDevice)ComboBoxViewEditDevice.SelectedItem);
                    if (device == null) return;
                    var deviceNumber = device.StorageDeviceNumber;

                    if (string.IsNullOrEmpty(TextBoxViewEditLoc2.Text) || TextBoxViewEditLoc2.Text == "0") return;
                    if (!IntegerValidator(TextBoxViewEditLoc2.Text.ParseInt())) return;
                    var loc2 = TextBoxViewEditLoc2.Text.ParseInt();

                    if (string.IsNullOrEmpty(TextBoxViewEditLoc3.Text) || TextBoxViewEditLoc3.Text == "0") return;
                    if (!IntegerValidator(TextBoxViewEditLoc3.Text.ParseInt())) return;
                    var loc3 = TextBoxViewEditLoc3.Text.ParseInt();

                    if (string.IsNullOrEmpty(TextBoxViewEditLoc4.Text) || TextBoxViewEditLoc4.Text == "0") return;
                    if (!IntegerValidator(TextBoxViewEditLoc4.Text.ParseInt())) return;
                    var loc4 = TextBoxViewEditLoc4.Text.ParseInt();

                    if (string.IsNullOrEmpty(TextBoxViewEditLoc5.Text) || TextBoxViewEditLoc5.Text == "0") return;
                    if (!IntegerValidator(TextBoxViewEditLoc5.Text.ParseInt())) return;
                    var loc5 = TextBoxViewEditLoc5.Text.ParseInt();

                    textBox.Text = GlobalVar.SlotNameFactory
                        .CreateSlotName(areaNumber, deviceNumber, loc2, loc3, loc4, loc5).SlotName;

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"GetSlotString Error: {ex.Message}");
                    throw;
                }
            }
        }

        private void ComboBoxViewEditDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetSlotString(TextBoxViewEditSlot, false);
        }

        private void ComboBoxNewDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetSlotString(TextBoxNewSlot, true);
        }

        private void DataGridView1_DoubleClick(object sender, EventArgs e)
        {
            ViewEditLocation();
        }

        private void TextBoxNewLoc2_Leave(object sender, EventArgs e)
        {
            GetSlotString(TextBoxNewSlot, true);
        }

        private void TextBoxNewLoc3_Leave(object sender, EventArgs e)
        {
            GetSlotString(TextBoxNewSlot, true);
        }
        private void TextBoxNewLoc4_Leave(object sender, EventArgs e)
        {
            GetSlotString(TextBoxNewSlot, true);
        }

        private void TextBoxNewLoc5_Leave(object sender, EventArgs e)
        {
            //GetSlotString(TextBoxNewSlot, true);
        }

        private void TextBoxViewEditLoc2_Leave(object sender, EventArgs e)
        {
            GetSlotString(TextBoxViewEditSlot, false);
        }
        private void TextBoxViewEditLoc3_Leave(object sender, EventArgs e)
        {
            GetSlotString(TextBoxViewEditSlot, false);
        }
        private void TextBoxViewEditLoc4_Leave(object sender, EventArgs e)
        {
            GetSlotString(TextBoxViewEditSlot, false);
        }

        private void TextBoxViewEditLoc5_Leave(object sender, EventArgs e)
        {
            //GetSlotString(TextBoxViewEditSlot, false);
        }
        private List<LocationView> GetSelectedItems(DataGridView dataGridView)
        {
            // Create a list of LocationView
            var selectedList = new List<LocationView>();
            // Loop through the selected rows
            foreach (DataGridViewRow row in dataGridView.SelectedRows)
            {
                // Get the LocationView from the row  ((ObjectView<LocationView>)_bindingSource.Current).Object;
                var locationView = ((ObjectView<LocationView>)row.DataBoundItem).Object;
                // Add the LocationView to the list
                selectedList.Add(locationView);
            }
            return selectedList;
        }
        /// <summary>
        /// Handles the Click event of the SaveToExcel button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        /// <remarks>
        /// This method disables the LoadFromExcel and SaveToExcel buttons, sets the cursor to a wait cursor, and then asynchronously saves the data to Excel.
        /// </remarks>
        private async void ButtonSaveToExcel_Click(object sender, EventArgs e)
        {
            ButtonLoadFromExcel.Enabled = false;
            ButtonSaveToExcel.Enabled = false;
            Cursor.Current = Cursors.WaitCursor;
            await SaveToExcelAsync();
        }
        private async Task SaveToExcelAsync()
        {
            await _logger.LogDetailAsync("Saving records to Excel spreadsheet");
            var listToExport = CheckBoxUseSelectedItems.Checked ? GetSelectedItems(DataGridView1) : _currentList;
            var dataTable = ToDataTable(listToExport);
            var excelService = new ExcelService();
            excelService.Generate(dataTable);
            await _logger.LogDetailAsync($"Saved {dataTable.Rows.Count} records to Excel spreadsheet");
            ResetUi();
        }
        private void ResetUi()
        {
            ButtonLoadFromExcel.Enabled = true;
            ButtonSaveToExcel.Enabled = true;
            Cursor.Current = Cursors.Default;
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
                // Create a DataTable from the List(Of T) (LocationView)
                dataTable = ToDataTable(selectedList);
            }
            else
            {
                // Create a DataTable from the List(Of T) (LocationView)
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
            SetLoadingState(true);
            LoadFromExcel();
        }
        private void SetLoadingState(bool isLoading)
        {
            ButtonLoadFromExcel.Enabled = !isLoading;
            ButtonSaveToExcel.Enabled = !isLoading;
            Cursor.Current = isLoading ? Cursors.WaitCursor : Cursors.Default;
        }

        private void LoadFromExcel()
        {
            _logger.LogDetailAsync("Loading records from Excel spreadsheet").SafeFireAndForget();
            var excelService = new ExcelService();
            var dataTable = excelService.Update();
            BackgroundWorkerLocations.RunWorkerAsync(dataTable);
        }

        public DataTable ToDataTable<T>(List<T> items)
        {
            var dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            // the first column is the Action column
            // by default all records are saved with a [M]odify action
            // the user can change the action to [D]elete or [A]dd
            // the action is not saved to the database
            // it is used to determine what to do with the record
            // before adding the List(Of T) to the database
            // the Action is not displayed in the DataGridView
            // the Action is displayed in the Excel spreadsheet
            // so we need to add it first, before the other columns
            var action = new DataColumn("Action", typeof(string));
            dataTable.Columns.Add(action);

            foreach (var prop in props)
            {
                //Setting column names as Property names
                var col = new DataColumn(prop.Name, prop.PropertyType);
                dataTable.Columns.Add(prop.Name, prop.PropertyType);
            }
            // by default all records are saved with a [M]odify action
            // the user can change the action to [D]elete or [A]dd
            // the action is not saved to the database
            // it is used to determine what to do with the record

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

        private void ComboBoxAreaNumber_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_startup)
            {
                DataGridView1.Columns[0].Visible = _workstationView.AreaId != 8 &&
                                                   _workstationView.AreaId == (int)ComboBoxAreaNumber.SelectedValue;
                RefreshData();
            }
        }

        private void BackgroundWorkerLocations_DoWork(object sender, DoWorkEventArgs e)
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
                    var action = row["Action"].ToString();
                    var id = row["Id"].ToString();
                    // Check to see if this row contains the header record
                    // if so, skip it
                    // Id is the first column in the spreadsheet
                    //if (id.Equals("Id")) continue;
                    var areaId = row["AreaId"].ToString();
                    var loc1 = row["Loc1"].ToString();
                    var loc2 = row["Loc2"].ToString();
                    var loc3 = row["Loc3"].ToString();
                    var loc4 = row["Loc4"].ToString();
                    var loc5 = row["Loc5"].ToString();
                    var slot = row["Slot"].ToString();
                    var pickSequence = row["PickSequence"].ToString();
                    var sizeCodeId = row["SizeCodeId"].ToString();
                    var velocityCodeId = row["VelocityCodeId"].ToString();
                    var heightCodeId = row["HeightCodeId"].ToString();
                    var locationCode = row["LocationCode"].ToString();
                    var inUse = row["InUse"].ToString().ToBoolean();

                    // if the Id is empty, this is a new row
                    // create a new Location object
                    if (action.Equals("A", StringComparison.CurrentCultureIgnoreCase))
                    {
                        var location = new Location
                        {
                            AreaId = areaId.ParseInt(),
                            Loc1 = loc1.ParseInt(),
                            Loc2 = loc2.ParseInt(),
                            Loc3 = loc3.ParseInt(),
                            Loc4 = loc4.ParseInt(),
                            Loc5 = loc5.ParseInt(),
                            Slot = slot,
                            PickSequence = pickSequence.ParseInt(),
                            SizeCodeId = sizeCodeId.ParseInt(),
                            VelocityCodeId = velocityCodeId.ParseInt(),
                            HeightCodeId = heightCodeId.ParseInt(),
                            LocationCode = locationCode,
                            InUse = inUse
                        };
                        _repoLocation.Insert(location);
                    }
                    else if (action.Equals("D", StringComparison.CurrentCultureIgnoreCase))
                    {
                        // get the existing Location object
                        var location = _repoLocation.FindByKey(id.ParseInt());
                        if (location == null) continue;
                        // delete the record
                        _repoLocation.Delete(location.Id);
                    }
                    else if (action.Equals("M", StringComparison.CurrentCultureIgnoreCase))
                    {
                        // get the existing Location object
                        var location = _repoLocation.FindByKey(id.ParseInt());
                        if (location == null) continue;

                        // update the values
                        location.AreaId = areaId.ParseInt();
                        location.Loc1 = loc1.ParseInt();
                        location.Loc2 = loc2.ParseInt();
                        location.Loc3 = loc3.ParseInt();
                        location.Loc4 = loc4.ParseInt();
                        location.Loc5 = loc5.ParseInt();
                        location.Slot = slot;
                        location.PickSequence = pickSequence.ParseInt();
                        location.SizeCodeId = sizeCodeId.ParseInt();
                        location.VelocityCodeId = velocityCodeId.ParseInt();
                        location.HeightCodeId = heightCodeId.ParseInt();
                        location.LocationCode = locationCode;
                        location.InUse = inUse;

                        // update the database
                        _repoLocation.Update(location);
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
            }
            catch (Exception ex)
            {
                Mediator.GetInstance().OnGeneralError(this, $"Error Adding/Updating Records{Environment.NewLine}{ex.Message}");
            }
            Mediator.GetInstance().OnDisplayMessage(this, $"Load complete");
        }

        private void BackgroundWorkerLocations_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            ProgressBarLocations.Value = e.ProgressPercentage;
        }

        private void BackgroundWorkerLocations_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            RefreshData();
            ProgressBarLocations.Value = 0;
            Cursor.Current = Cursors.Default;
            ButtonLoadFromExcel.Enabled = true;
            ButtonSaveToExcel.Enabled = true;
            _logger.LogDetailAsync("Loading records from Excel spreadsheet complete").SafeFireAndForget();
        }
    }
}