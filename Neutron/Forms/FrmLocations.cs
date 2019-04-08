using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using AlliedLogger;
using EnumsNET;
using Equin.ApplicationFramework;
using JsonManager;
using MetroFramework.Forms;
using Neutron.Classes;
using Neutron.Enums;
using Neutron.Global;
using Neutron.Interfaces;
using NeutronCore;
using NeutronCore.Extensions;
using NeutronCore.Global;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Models.Lookups;
using NeutronData.ModelViews;
using NeutronData.Repositories;
using PrintRequest;
using SlotNameFactory;

namespace Neutron.Forms
{
    public partial class FrmLocations : MetroForm
    {
        private readonly IJsonData _jsonData;
        private readonly NeutronVariables _neutronVariables;
        private readonly INomenclature _nomenclature;
        private readonly StationView _station;
        private DocumentPrinterPreferences _documentPrinter;
        private LabelPrinterPreferences _labelPrinter;
        private readonly BindingSource _bindingSource = new BindingSource();
        private LocationsRepository _locationRepository;
        private DynamicLogger _logger;

        private readonly GenericRepository<HeightCode> _repoHeightCode =
            new GenericRepository<HeightCode>(new NeutronDb());

        private readonly GenericRepository<Inventory> _repoInventory = new GenericRepository<Inventory>(new NeutronDb());
        private readonly GenericRepository<Location> _repoLocation = new GenericRepository<Location>(new NeutronDb());

        private readonly GenericRepository<LocationCode> _repoLocationCode =
            new GenericRepository<LocationCode>(new NeutronDb());

        private readonly GenericRepository<SizeCode> _repoSizeCode = new GenericRepository<SizeCode>(new NeutronDb());
        private readonly StationRepository _repoStation = new StationRepository();

        private readonly GenericRepository<VelocityCode> _repoVelocityCode =
            new GenericRepository<VelocityCode>(new NeutronDb());

        private ISlot _slotName;
        private ISlotNameFactory _slotNameFactory;

        public FrmLocations(IJsonData jsonData, StationView station, INomenclature nomenclature,
            NeutronVariables neutronVariables)
        {
            InitializeComponent();
            _jsonData = jsonData;
            _station = station;
            _nomenclature = nomenclature;
            _neutronVariables = neutronVariables;
            InitForm();
        }

        private void InitForm()
        {
            _logger = CreateLog();
            KeyPreview = true;
            CloseButtonPressed = false;
            SetupGrid();
            SetupTabControl();
            SetupNewForm();
            SetupViewEditForm();
            SetupPrinters();
            mlUserInfo.Text = GlobalVar.User?.UserInfo;
           // CreateSlotNameFactory(_neutronVariables.SlotNameType);
            _locationRepository = new LocationsRepository();
            ComboBoxStationNumber.SelectedIndex = 0;
            label5.Text = _nomenclature.LabelDevice;
            label6.Text = _nomenclature.LabelTray;
            label7.Text = _nomenclature.LabelOver;
            label8.Text = _nomenclature.LabelBack;

            label17.Text = _nomenclature.LabelDevice;
            label16.Text = _nomenclature.LabelTray;
            label15.Text = _nomenclature.LabelOver;
            label14.Text = _nomenclature.LabelBack;
            if (_station.StationNumber >= 10)
            {
                CheckBoxAllStations.Checked = true;
            }

            SetupViewEditBindings();
       
            RefreshData();
            //  AutoValidate = AutoValidate.Disable;
        }

        public bool CloseButtonPressed { get; set; }

        private DynamicLogger CreateLog()
        {
            var logFileDir = LoaderSettings.GetLogFileDirectory();
            var folderName = string.Format(@"Remstar_Bpi_Shi_{0}", _station.StationNumber.ToString());
            var logActivity = LoaderSettings.EnableLogging;
            _logger = new DynamicLogger(logFileDir, folderName, logActivity);
            return _logger;
        }

        private void SetupPrinters()
        {
            _documentPrinter = _jsonData.LoadFile<DocumentPrinterPreferences>();
            _labelPrinter = _jsonData.LoadFile<LabelPrinterPreferences>();
        }

        //private void CreateSlotNameFactory(string slotType)
        //{
        //    switch (slotType)
        //    {
        //        case "Default":
        //            _slotNameFactory = new DefaultSlotNameFactory();
        //            break;
        //        case "T101-01-01":
        //            _slotNameFactory = new Type1SlotNameFactory();
        //            break;
        //        case "V101":
        //            _slotNameFactory = new Type2SlotNameFactory();
        //            break;
        //        case "01--01--01--01":
        //            _slotNameFactory = new Type3SlotNameFactory();
        //            break;
        //        default:
        //            _slotNameFactory = new DefaultSlotNameFactory();
        //            break;
        //    }
        //}

        private void FrmLocations_Load(object sender, EventArgs e)
        {

        }

        // Set the focus to the passed in recId if it's passed in
        private void RefreshData(int recId = 0)
        {
            Cursor.Current = Cursors.WaitCursor;
            IEnumerable<LocationView> views;
            IEnumerable<LocationView> recs;
            var idx = 0;
            var find = TextBoxFind.Text.ToLower().Trim();

            if (CheckBoxAllStations.Checked)
                views = _locationRepository.FindLocationViews(find);
            else
                views = _locationRepository.FindLocationViewsByStation(find, _station.StationId);


            if (MButtonAllLocations.Text == "Available")
                recs = views;
            else
                recs = views.Where(v => v.InUse == false).ToList();

            var blv = new BindingListView<LocationView>(recs.ToList());

            _bindingSource.DataSource = blv;
            DataGridView1.AutoGenerateColumns = false;
            DataGridView1.DataSource = _bindingSource;

            if (GetRecordCount() > 0)
            {
                if (recId != 0) idx = IndexOf(recId);
                DataGridView1.FirstDisplayedScrollingRowIndex = DataGridView1.Rows[idx].Index;
                DataGridView1.Refresh();
                DataGridView1.CurrentCell = DataGridView1.Rows[idx].Cells[1];
                DataGridView1.Rows[idx].Selected = true;
            }

            DataGridView1.ClearSelection();
            Cursor.Current = Cursors.Default;
        }

        public int IndexOf(int value)
        {
            var count = _bindingSource.Count;
            var itemIndex = -1;
            for (var i = 0; i < count; i++)
            {
                var rec = ((ObjectView<LocationView>) _bindingSource[i]).Object.Id;
                if (rec == value)
                {
                    itemIndex = i;
                    break;
                }
            }

            return itemIndex;
        }

        private void SetupViewEditBindings()
        {
            TextBoxViewEditId.DataBindings.Add("Text", _bindingSource, "Id");
            ComboBoxViewEditStation.DataBindings.Add("SelectedValue", _bindingSource, "StationId");
            ComboBoxViewEditDevice.DataBindings.Add("SelectedValue", _bindingSource, "Loc1");
            TextBoxViewEditLoc2.DataBindings.Add("Text", _bindingSource, "Loc2");
            TextBoxViewEditLoc3.DataBindings.Add("Text", _bindingSource, "Loc3");
            TextBoxViewEditLoc4.DataBindings.Add("Text", _bindingSource, "Loc4");
            TextBoxViewEditLoc5.DataBindings.Add("Text", _bindingSource, "Loc5");
            TextBoxViewEditSlot.DataBindings.Add("Text", _bindingSource, "Slot");
            ComboBoxViewEditSizeCode.DataBindings.Add("SelectedValue", _bindingSource, "SizeCodeId");
            ComboBoxViewEditVelocityCode.DataBindings.Add("SelectedValue", _bindingSource, "VelocityCodeId");
            ComboBoxViewEditHeightCode.DataBindings.Add("SelectedValue", _bindingSource, "HeightCodeId");
            ComboBoxViewEditLocationCode.DataBindings.Add("SelectedValue", _bindingSource, "LocationCodeId");
            CheckBoxInUse.DataBindings.Add("Checked", _bindingSource, "InUse");
        }

        private int GetRecordCount()
        {
            var count = _bindingSource.Count;
            LabelRecordCount.Text = string.Format("Records: {0}", count.ToString());
            return count;
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var grid = (DataGridView) sender;
            if (e.RowIndex >= 0)
                if (grid.CurrentCell.ColumnIndex == grid.Columns["Position"].Index)
                {
                    var deviceNumber = grid["Loc1", e.RowIndex].Value.ToString().ParseInt();
                    var trayNumber = grid["Loc2", e.RowIndex].Value.ToString().ParseInt();
                    var level = grid["Loc3", e.RowIndex].Value.ToString().ParseInt();
                    var partition = grid["Loc4", e.RowIndex].Value.ToString();
                    var part = grid["Loc4", e.RowIndex].Value.ToString().ParseInt();
                    _logger.Log($"Device Number: {deviceNumber}  Tray: {trayNumber}  Level: {level}  Part: {partition}");
                    _logger.Log($"Shuttle Enabled - {_neutronVariables.ShuttleEnabled}");

                    MoveDevice(deviceNumber, trayNumber, level, part, 0, "");
                    TurnOnShi(deviceNumber, trayNumber, level, partition);
                }
        }

        private void ClearAllShi()
        {
            if (_neutronVariables.DisplaysEnabled)
                if (GlobalVar.Displays != null)
                    GlobalVar.Displays.ClearAllShi();
        }

        private void TurnOnShi(int deviceNumber, int trayNumber, int level, string partition)
        {
            var lArrow = "";
            var rArrow = "";
            ClearAllShi();

            if (_neutronVariables.DisplaysEnabled)
                if (GlobalVar.Displays != null)
                {
                    if (deviceNumber == 2 || deviceNumber == 4)
                    {
                        lArrow = "";
                        rArrow = ">";
                    }
                    else
                    {
                        lArrow = "<";
                        rArrow = "";
                    }

                    if (deviceNumber == 3 &&  _station.StationNumber == 3)
                    {
                        lArrow = "";
                        rArrow = ">";
                    }

                    var text = $"{lArrow}-----{rArrow}";

                    GlobalVar.Displays.ShowShi(deviceNumber, trayNumber, level, partition, text);
                }
        }

        private void MoveDevice(int deviceNumber, int trayNumber, int level = 0, int partition = 0, int quantity = 0, string display = "")
        {
            if (_neutronVariables.ShuttleEnabled)
            {
                var hardwareDevice = _station.HardwareDevices.FirstOrDefault(s => s.DeviceNumber == deviceNumber);
                if (hardwareDevice != null)
                {
                    if (hardwareDevice.Enabled)
                    {
                        if (GlobalVar.Shuttle != null)
                        {
                            var response = GlobalVar.Shuttle.PositionDevice(deviceNumber, trayNumber, level, partition, quantity, display );
                            if (response != DeviceResponse.Success)
                                MessageBox.Show(response.AsString(EnumFormat.Description), "Device Information"
                                    , MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            MessageBox.Show("Device Controller is not properly initialized.");
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Hardware Device {hardwareDevice.Name} is not enabled.");
                    }
                }
                else
                {
                    MessageBox.Show("Hardware Device not found.");
                }
            }
        }

        private void SaveNew()
        {
            var stId = ((Station) ComboBoxNewStation.SelectedItem).Id;
            var deviceNumber = ((HardwareDeviceLookup) ComboBoxNewDevice.SelectedItem).Id;

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
                            var rec = _repoLocation.All().Where(r =>
                                r.StationId == stId && r.Loc1 == deviceNumber && r.Loc2 == loc2
                                && r.Loc3 == loc3 && r.Loc4 == loc4 && r.Loc5 == loc5).FirstOrDefault();
                            if (rec == null)
                            {
                                _slotName = GlobalVar.SlotNameFactory.CreateSlotName(stId, deviceNumber, loc2, loc3, loc4, loc5);

                                var loc = new Location
                                {
                                    StationId = stId,
                                    Loc1 = deviceNumber,
                                    Loc2 = loc2,
                                    Loc3 = loc3,
                                    Loc4 = loc4,
                                    Loc5 = loc5,
                                    Slot = _slotName.SlotName,
                                    InUse = CheckBoxInUseNew.Checked,
                                    SizeCodeId = (ComboBoxNewSizeCode.SelectedItem as SizeCode).Id,
                                    VelocityCodeId = (ComboBoxNewVelocityCode.SelectedItem as VelocityCode).Id,
                                    HeightCodeId = (ComboBoxNewHeightCode.SelectedItem as HeightCode).Id,
                                    LocationCodeId = (ComboBoxNewLocationCode.SelectedItem as LocationCode).Id
                                };

                                TextBoxNewSlot.Text = _slotName.SlotName;
                                try
                                {
                                    _repoLocation.Insert(loc);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Error Inserting Location.  " + ex.Message + "\n\r" +
                                                    ex.InnerException);
                                }

                                RefreshData(loc.Id);
                                tabControl1.SelectedTab = tabPage1;
                            }
                            else
                            {
                                MessageBox.Show("Location already exists.", "Duplicate Entry", MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Invalid Entry.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid Entry.");
                    }
                }
                else
                {
                    MessageBox.Show("Invalid Entry.");
                }
            }
            else
            {
                MessageBox.Show("Invalid Entry.");
            }
        }

        private void UpdateViewEdit()
        {
            var id = ((ObjectView<LocationView>) _bindingSource.Current).Object.Id;
            var stId = ((Station) ComboBoxViewEditStation.SelectedItem).Id;
            var deviceNumber = ((HardwareDeviceLookup) ComboBoxViewEditDevice.SelectedItem).Id;
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

                            _slotName = GlobalVar.SlotNameFactory.CreateSlotName(stId, deviceNumber, loc2, loc3, loc4, loc5);
                            var rec = new Location
                            {
                                Id = id,
                                StationId = stId,
                                Loc1 = deviceNumber,
                                Loc2 = loc2,
                                Loc3 = loc3,
                                Loc4 = loc4,
                                Loc5 = loc5,
                                Slot = _slotName.SlotName,
                                InUse = CheckBoxInUse.Checked,
                                SizeCodeId = (ComboBoxViewEditSizeCode.SelectedItem as SizeCode).Id,
                                VelocityCodeId = (ComboBoxViewEditVelocityCode.SelectedItem as VelocityCode).Id,
                                HeightCodeId = (ComboBoxViewEditHeightCode.SelectedItem as HeightCode).Id,
                                LocationCodeId = (ComboBoxViewEditLocationCode.SelectedItem as LocationCode).Id
                            };

                            TextBoxViewEditSlot.Text = _slotName.SlotName;

                            try
                            {
                                _repoLocation.Update(rec);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Error Updating Location.  " + ex.Message + "\n\r" + ex.InnerException);
                            }

                            RefreshData(rec.Id);
                            tabControl1.SelectedTab = tabPage1;
                        }
                        else
                        {
                            MessageBox.Show("Invalid Entry.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid Entry.");
                    }
                }
                else
                {
                    MessageBox.Show("Invalid Entry.");
                }
            }
            else
            {
                MessageBox.Show("Invalid Entry.");
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
                if (input <= 0)
                {
                    MessageBox.Show("Entry must be greater than zero.");
                    return false;
                }

                return true;
            }

            return false;
        }

        private bool IsDuplicate(Location loc)
        {
            var result = false;
            Location rec;
            try
            {
                rec = _repoLocation.All().Where(r =>
                    r.StationId == loc.StationId && r.Loc1 == loc.Loc1 && r.Loc2 == loc.Loc2
                    && r.Loc3 == loc.Loc3 && r.Loc4 == loc.Loc4 && r.Loc5 == loc.Loc5).FirstOrDefault();
                if (rec != null)
                {
                    MessageBox.Show("Location already exists.", "Duplicate Entry", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    result = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error checking for duplicate location.  " + ex.Message + "  " + ex.InnerException);
            }

            return result;
        }

        private void MbViewEditDelete_Click(object sender, EventArgs e)
        {
            var id = ((ObjectView<LocationView>)_bindingSource.Current).Object.Id;
            if (!HasInventory(id))
            {
                var result = MessageBox.Show("Are you sure you want to delete this location?", "Location",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes) return;
                _repoLocation.Delete(id);
                RefreshData();
                tabControl1.SelectedTab = tabPage1;
            }
            else
            {
                MessageBox.Show("Location has existing inventory, unable to delete.");
            }
        }

        private bool HasInventory(int id)
        {
            var result = false;
            var rec = _repoInventory.FindBy(r => r.LocationId == id).FirstOrDefault();
            if (rec != null) result = true;
            return result;
        }

        private void MButtonAllLocations_Click(object sender, EventArgs e)
        {
            switch (MButtonAllLocations.Text)
            {
                case "Available":
                    MButtonAllLocations.Text = "Show All";
                    LabelFormTitle.Text = "Available Locations";
                    break;
                case "Show All":
                    MButtonAllLocations.Text = "Available";
                    LabelFormTitle.Text = "All Locations";
                    break;
            }

            RefreshData();
        }

        private void FrmLocations_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !CloseButtonPressed;
        }

        private void MBPrintLocations_Click(object sender, EventArgs e)
        {
            CsvUtility.SaveToCsv(DataGridView1);
        }

        private void MbSaveAsDefault_Click(object sender, EventArgs e)
        {
            var station = ((Station) ComboBoxNewStation.SelectedItem)?.Id ?? 1;
            var loc1 = ((HardwareDeviceLookup) ComboBoxNewDevice.SelectedItem)?.Id ?? 1;
            var loc2 = string.IsNullOrEmpty(TextBoxNewLoc2.Text) ? "0" : TextBoxNewLoc2.Text;
            var loc3 = string.IsNullOrEmpty(TextBoxNewLoc3.Text) ? "0" : TextBoxNewLoc3.Text;
            var loc4 = string.IsNullOrEmpty(TextBoxNewLoc4.Text) ? "0" : TextBoxNewLoc4.Text;
            var loc5 = string.IsNullOrEmpty(TextBoxNewLoc5.Text) ? "0" : TextBoxNewLoc5.Text;
            var rec = new Location
            {
                StationId = station,
                Loc1 = loc1,
                Loc2 = loc2.ParseInt(),
                Loc3 = loc3.ParseInt(),
                Loc4 = loc4.ParseInt(),
                Loc5 = loc5.ParseInt(),
                Slot = TextBoxNewSlot.Text,
                SizeCodeId = ((SizeCode) ComboBoxNewSizeCode.SelectedItem).Id,
                VelocityCodeId = ((VelocityCode) ComboBoxNewVelocityCode.SelectedItem).Id,
                HeightCodeId = ((HeightCode) ComboBoxNewHeightCode.SelectedItem).Id,
                LocationCodeId = ((LocationCode) ComboBoxNewLocationCode.SelectedItem).Id,
                InUse = CheckBoxInUseNew.Checked
            };

            try
            {
                _jsonData.SaveFile(rec);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Saving Default Location.  " + ex.Message + "\n\r" + ex.InnerException);
            }
        }

        private void MbLoadDefault_Click(object sender, EventArgs e)
        {
            var item = _jsonData.LoadFile<Location>();
            ComboBoxNewStation.SelectedValue = item.StationId;
            ComboBoxNewDevice.SelectedValue = item.Loc1;
            TextBoxNewLoc2.Text = item.Loc2.ToString();
            TextBoxNewLoc3.Text = item.Loc3.ToString();
            TextBoxNewLoc4.Text = item.Loc4.ToString();
            TextBoxNewLoc5.Text = item.Loc5.ToString();
            TextBoxNewSlot.Text = item.Slot;
            ComboBoxNewSizeCode.SelectedValue = item.SizeCodeId;
            ComboBoxNewVelocityCode.SelectedValue = item.VelocityCodeId;
            ComboBoxNewHeightCode.SelectedValue = item.HeightCodeId;
            ComboBoxNewLocationCode.SelectedValue = item.LocationCodeId;
            CheckBoxInUseNew.Checked = item.InUse;
        }

        private void CheckBoxAllStations_CheckedChanged(object sender, EventArgs e)
        {
            RefreshData();
        }

        private void ButtonAvailableLocations_Click(object sender, EventArgs e)
        {
            PrintAvailableLocations();
        }

        private void PrintAvailableLocations()
        {
            if (!_neutronVariables.EnableDocumentPrinter) return;
            var locations = GetAvailableLocations();
            DocumentToPrint.PrintAvailableLocations(locations, _documentPrinter);
        }

        private List<Location> GetAvailableLocations()
        {
            var stationId = ComboBoxStationNumber.Text.ParseInt();
            List<Location> outs;
            using (var context = new NeutronDb())
            {
                outs = context.Locations.Include("Station")
                    .Include("SizeCode")
                    .Include("VelocityCode")
                    .Include("HeightCode")
                    .Include("LocationCode")
                    .Where(l => l.InUse == false && l.StationId == stationId).ToList();
            }

            return outs;
        }

        private void ComboBoxNewStation_SelectedIndexChanged(object sender, EventArgs e)
        {
            var stationId = ((Station) ComboBoxNewStation.SelectedItem)?.Id ?? 1;
            var sv = _repoStation.GetStationView(stationId);

            ComboBoxNewDevice.DataSource = sv.HardwareDevices
                .Select(s => new HardwareDeviceLookup {Id = s.DeviceNumber, Name = s.Name}).ToList();
            ComboBoxNewDevice.DisplayMember = "Name";
            ComboBoxNewDevice.ValueMember = "Id";
            ComboBoxNewDevice.Refresh();
        }

        private void ComboBoxViewEditStation_SelectedIndexChanged(object sender, EventArgs e)
        {
            var stationId = ((Station) ComboBoxViewEditStation.SelectedItem)?.Id ?? 1;
            var sv = _repoStation.GetStationView(stationId);

            ComboBoxViewEditDevice.DataSource = sv.HardwareDevices
                .Select(s => new HardwareDeviceLookup {Id = s.DeviceNumber, Name = s.Name}).ToList();
            ComboBoxViewEditDevice.DisplayMember = "Name";
            ComboBoxViewEditDevice.ValueMember = "Id";
            ComboBoxViewEditDevice.Refresh();
        }

        #region Find Functions

        private void MButtonFind_Click(object sender, EventArgs e)
        {
            RefreshData();
        }

        private void FindRecord(string s)
        {
            RefreshData();
        }

        private void TextBoxFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return) RefreshData();
        }

        #endregion

        #region Button Clicks

        private void ButtonClear_Click(object sender, EventArgs e)
        {
            TextBoxFind.Text = string.Empty;
            RefreshData();
            TextBoxFind.Focus();
        }

        private void MButtonClose_Click(object sender, EventArgs e)
        {
            ClearAllShi();
            CloseButtonPressed = true;
        }

        private void MButtonViewEdit_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage2;
        }

        private void MButtonNew_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage3;
        }

        private void MbViewEditListing_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage1;
        }

        private void MbViewEditNew_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage3;
        }

        private void MbViewEditClose_Click(object sender, EventArgs e)
        {
            var id = ((ObjectView<LocationView>)_bindingSource.Current).Object.Id;
            RefreshData(id);
            tabControl1.SelectedTab = tabPage1;
        }

        private void MbViewEditSave_Click(object sender, EventArgs e)
        {
            UpdateViewEdit();
        }

        private void MbNewListing_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage1;
        }

        private void MbNewViewEdit_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage2;
        }

        private void MbNewSave_Click(object sender, EventArgs e)
        {
            SaveNew();
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

            var w = (DataGridView1.Width - 60) / 12;

            var bCol = new DataGridViewButtonColumn
            {
                HeaderText = "",
                Visible = true,
                Name = "Position",
                Text = "Position",
                UseColumnTextForButtonValue = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(bCol);

            var xcol = new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "InUse"
                , HeaderText = "In Use"
                , AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                , Name = "InUse"
            };
            DataGridView1.Columns.Add(xcol);


            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StationName"
                , HeaderText = "Station"
                , AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                , Name = "StationName"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Slot"
                , HeaderText = "Slot"
                , Visible = true
                , AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                , Name = "Slot"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc1"
                , HeaderText = _nomenclature.LabelDevice
                , AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                , Name = "Loc1"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc2"
                , HeaderText = _nomenclature.LabelTray
                , AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                , Name = "Loc2"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc3"
                , HeaderText = _nomenclature.LabelOver
                , AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                , Name = "Loc3"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc4"
                , HeaderText = _nomenclature.LabelBack
                , AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                , Name = "Loc4"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc5"
                , HeaderText = "Tag"
                , AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                , Name = "Loc5"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SizeCodeName"
                , HeaderText = "Size Code"
                , AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                , Name = "SizeCodeName"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "VelocityCodeName"
                , HeaderText = "Velocity Code"
                , AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                , Name = "VelocityCodeName"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "HeightCodeName"
                , HeaderText = "Height Code"
                , AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                , Name = "HeightCodeName"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LocationCodeName"
                , HeaderText = "Location Code"
                , AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                , Name = "LocationCodeName"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id"
                , HeaderText = "Id"
                , Visible = false
                , Name = "Id"
                , AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(col);

          

            foreach (DataGridViewColumn column in DataGridView1.Columns)
            {
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);
            }
        }

        private void SetupTabControl()
        {
            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(0, 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;
            foreach (TabPage tab in tabControl1.TabPages) tab.Text = string.Empty;
        }

        private void SetupNewForm()
        {
            LabelFindDescription.Text = "Search any part of Slot field";

            ComboBoxNewSizeCode.DataSource = _repoSizeCode.All();
            ComboBoxNewSizeCode.DisplayMember = "Name";
            ComboBoxNewSizeCode.ValueMember = "Id";

            ComboBoxNewVelocityCode.DataSource = _repoVelocityCode.All();
            ComboBoxNewVelocityCode.DisplayMember = "Name";
            ComboBoxNewVelocityCode.ValueMember = "Id";

            ComboBoxNewHeightCode.DataSource = _repoHeightCode.All();
            ComboBoxNewHeightCode.DisplayMember = "Name";
            ComboBoxNewHeightCode.ValueMember = "Id";

            ComboBoxNewLocationCode.DataSource = _repoLocationCode.All();
            ComboBoxNewLocationCode.DisplayMember = "Name";
            ComboBoxNewLocationCode.ValueMember = "Id";

            ComboBoxNewStation.DataSource = _repoStation.Lookup();
            ComboBoxNewStation.DisplayMember = "Name";
            ComboBoxNewStation.ValueMember = "Id";
            ComboBoxNewStation.SelectedIndex = ComboBoxNewStation.FindString(_station.Name);

            ComboBoxNewDevice.DataSource = _station.HardwareDevices
                .Select(s => new HardwareDeviceLookup {Id = s.DeviceNumber, Name = s.Name}).ToList();
            ComboBoxNewDevice.DisplayMember = "Name";
            ComboBoxNewDevice.ValueMember = "Id";
        }

        private void SetupViewEditForm()
        {
            LabelFindDescription.Text = "Search any part of Slot field";

            ComboBoxViewEditSizeCode.DataSource = _repoSizeCode.All();
            ComboBoxViewEditSizeCode.DisplayMember = "Name";
            ComboBoxViewEditSizeCode.ValueMember = "Id";

            ComboBoxViewEditVelocityCode.DataSource = _repoVelocityCode.All();
            ComboBoxViewEditVelocityCode.DisplayMember = "Name";
            ComboBoxViewEditVelocityCode.ValueMember = "Id";

            ComboBoxViewEditHeightCode.DataSource = _repoHeightCode.All();
            ComboBoxViewEditHeightCode.DisplayMember = "Name";
            ComboBoxViewEditHeightCode.ValueMember = "Id";

            ComboBoxViewEditLocationCode.DataSource = _repoLocationCode.All();
            ComboBoxViewEditLocationCode.DisplayMember = "Name";
            ComboBoxViewEditLocationCode.ValueMember = "Id";

            ComboBoxViewEditStation.DataSource = _repoStation.Lookup();
            ComboBoxViewEditStation.DisplayMember = "Name";
            ComboBoxViewEditStation.ValueMember = "Id";
            ComboBoxNewStation.SelectedIndex = ComboBoxNewStation.FindString(_station.Name);

            ComboBoxViewEditDevice.DataSource = _station.HardwareDevices
                .Select(s => new HardwareDeviceLookup {Id = s.DeviceNumber, Name = s.Name}).ToList();
            ComboBoxViewEditDevice.DisplayMember = "Name";
            ComboBoxViewEditDevice.ValueMember = "Id";
        }

        #endregion

        #region Return Key Functions

        private void ComboBoxNewDevice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return) TextBoxNewLoc2.Focus();
        }

        private void TextBoxNewLoc2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return) TextBoxNewLoc3.Focus();
        }

        private void TextBoxNewLoc3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return) TextBoxNewLoc4.Focus();
        }

        private void TextBoxNewLoc4_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return) TextBoxNewLoc5.Focus();
        }

        private void TextBoxNewLoc5_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return) ComboBoxNewSizeCode.Focus();
        }

        private void ComboBoxNewSizeCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return) ComboBoxNewVelocityCode.Focus();
        }

        private void ComboBoxNewVelocityCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return) ComboBoxNewHeightCode.Focus();
        }

        private void ComboBoxNewHeightCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return) ComboBoxNewLocationCode.Focus();
        }

        private void ComboBoxNewLocationCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return) TextBoxNewLoc2.Focus();
        }

        private void ComboBoxViewEditDevice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return) TextBoxViewEditLoc2.Focus();
        }

        private void TextBoxViewEditLoc2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return) TextBoxViewEditLoc3.Focus();
        }

        private void TextBoxViewEditLoc3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return) TextBoxViewEditLoc4.Focus();
        }

        private void TextBoxViewEditLoc4_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return) TextBoxViewEditLoc5.Focus();
        }

        private void TextBoxViewEditLoc5_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return) ComboBoxViewEditSizeCode.Focus();
        }

        private void ComboBoxViewEditSizeCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return) ComboBoxViewEditVelocityCode.Focus();
        }

        private void ComboBoxViewEditVelocityCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return) ComboBoxViewEditHeightCode.Focus();
        }

        private void ComboBoxViewEditHeightCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return) ComboBoxViewEditLocationCode.Focus();
        }

        private void ComboBoxViewEditLocationCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return) ComboBoxViewEditDevice.Focus();
        }

        private void tabControl1_Enter(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 1) ComboBoxViewEditDevice.Focus();
            if (tabControl1.SelectedIndex == 2) ComboBoxNewDevice.Focus();
        }

        #endregion
    }
}