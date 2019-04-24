using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using JsonManager;
using MetroFramework.Forms;
using Neutron.Enums;
using NeutronCore.Extensions;
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
using Neutron.Interfaces;
using Neutron.Models;
using NeutronCore.Global;
using NeutronCore;
using NeutronData.Interfaces;
using NeutronCore.Enums;
using NeutronEvents;
using PrintRequest;
using StorageType = Neutron.Enums.StorageType;

namespace Neutron.Forms
{
    public partial class FrmInventory : MetroForm
    {
        private BindingListView<SqlInventoryView> _bindingSourceEquin;
        private BindingListView<SqlInventoryView> _locationBindingSourceEquin;
        private BindingListView<LocationView> _newLocationBindingSourceEquin;

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

        private readonly GenericRepository<LocationCode> _repoLocationCode =
            new GenericRepository<LocationCode>(new NeutronDb());

        private readonly GenericRepository<Inventory> _repoInventory = new GenericRepository<Inventory>(new NeutronDb());

        private readonly GenericRepository<NeutronData.Models.Lookups.StorageType> _repoStorageType =
            new GenericRepository<NeutronData.Models.Lookups.StorageType>(new NeutronDb());

        private readonly GenericRepository<UnitOfIssue> _repoUnitOfIssue =
            new GenericRepository<UnitOfIssue>(new NeutronDb());

        private readonly StationRepository _repoStation = new StationRepository();

        private readonly LocationsRepository _locationsRepository;

        // private readonly HistoryManager GlobalVar.HistoryManager = new HistoryManager();
        private readonly InventoryRepository _inventoryRepository = new InventoryRepository();

        private readonly IJsonData _jsonData;
        private readonly NeutronVariables _neutronVariables;
        private readonly StationView _station;
        private readonly IAkaRepository _akaRepository;
        private readonly INomenclature _nomenclature;

        private bool _allAvailable;

        public ItemDefinition CurrentItem { get; set; }
        public Location CurrentLocation { get; set; }
        public Inventory CurrentInventoryItem { get; set; }
        public bool CloseButtonPressed { get; set; }
        private bool _firstTime = true;

        public FrmInventory(IJsonData jsonData, StationView station, IAkaRepository akaRepository,
            INomenclature nomenclature)
        {
            InitializeComponent();

            KeyPreview = true;
            CloseButtonPressed = false;

            _station = station;
            _jsonData = jsonData;
            _neutronVariables = jsonData.LoadFile<NeutronVariables>();
            _nomenclature = nomenclature;
            _akaRepository = akaRepository;

            SetupGrids();
            HideTabControlTabs();
            SetupNewForm();
            SetupViewEditForm();
            SetupAddDetailForm();

            mlUserInfo.Text = GlobalVar.User?.UserInfo;

            var logFilePath = LoaderSettings.GetLogFileDirectory() + "Inventory.log";
            _locationsRepository = new LocationsRepository();

            label28.Text = _nomenclature.LabelDevice;
            label27.Text = _nomenclature.LabelTray;
            label26.Text = _nomenclature.LabelOver;
            label22.Text = _nomenclature.LabelBack;
            if (_station.StationNumber >= 10)
            {
                CheckBoxAllStations.Checked = true;
            }

            ComboBoxStationNumber.SelectedIndex = 0;
            _firstTime = false;
            //Mediator.GetInstance().InventoryFileCreated += (s, e) => MessageBox.Show("Inventory File Created."
            //    , "Inventory File", MessageBoxButtons.OK,MessageBoxIcon.Information,MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }

        private void FrmInventory_Load(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            LoadInventory();
            Cursor.Current = Cursors.Default;
        }

        // Set the focus to the passed in recId if it's passed in
        private void LoadInventory(int recId = 0)
        {
            //ItemDefinition itemDefinition;
            var findWhat = TextBoxFind.Text.ToLower().Trim();
            var find = _akaRepository.Get(findWhat);
            TextBoxFind.Text = find;

            IEnumerable<SqlInventoryView> views = CheckBoxAllStations.Checked 
                ? _inventoryRepository.FindInventoryViews(find) 
                : _inventoryRepository.FindInventoryViewsByStation(find, _station.StationId);

            //Task<IEnumerable<SqlInventoryView>> views = CheckBoxAllStations.Checked
            //    ?  _inventoryRepository.FindInventoryViews(find)
            //    : _inventoryRepository.FindInventoryViewsByStation(find, _station.StationId);

            _bindingSourceEquin = new BindingListView<SqlInventoryView>(views.ToList());
            _bindingSource.DataSource = _bindingSourceEquin;
            DataGridView1.DataSource = _bindingSource;

            if (GetRecordCount(_bindingSource) > 0)
            {
                
                //var item = views.FirstOrDefault();
                //if (item != null)
                //{
                //    SetCurrentItemDefinition(item.ItemDefinitionId);
                //    SetCurrentLocation(item.LocationId);
                //}

                if (recId != 0)
                {
                    var idx = IndexOf(_bindingSource, recId);
                    DataGridView1.FirstDisplayedScrollingRowIndex = DataGridView1.Rows[idx].Index;
                    DataGridView1.CurrentCell = DataGridView1.Rows[idx].Cells[1];
                    DataGridView1.Rows[idx].Selected = true;
                   // SetCurrentInventoryItem();
                }
                //else
                //{
                //    DataGridView1.ClearSelection();
                //}
                SetCurrentInventoryItem();
                DataGridView1.Refresh();
                DataGridView1.ClearSelection();
            }
            else
            {
                CurrentItem = null;
                CurrentLocation = null;
                CurrentInventoryItem = null;
                //LoadViewEdit();
            }
        }

        private void SetCurrentInventoryItem()
        {
            if (_bindingSource.Current != null)
            {
                var inventory = ((ObjectView<SqlInventoryView>)_bindingSource.Current).Object;
                CurrentInventoryItem = _repoInventory.FindByKey(inventory.Id);
                SetCurrentLocation(inventory.LocationId);
                SetCurrentItemDefinition(inventory.ItemDefinitionId);
            }
            else
            {
                MessageBox.Show(@"SetCurrentInventoryItem:  _bindingSource.Current is null");
            }
        }

        private void SetCurrentInventoryItem(Inventory inventory)
        {
            CurrentInventoryItem = inventory;
            SetCurrentLocation(inventory.LocationId);
            SetCurrentItemDefinition(inventory.ItemDefinitionId);
        }

        private void SetCurrentItemDefinition(int itemDefinitionId)
        {
            //if (bindingSource.Current != null)
            //{
            //    int itemDefinitionId = (((ObjectView<SqlInventoryView>) bindingSource.Current).Object).ItemDefinitionId;
            var itemDefinition = _repoItemDefinition.FindByKey(itemDefinitionId);
            if (itemDefinition == null) return;
            CurrentItem = itemDefinition;
            //}
        }

        //private void SetCurrentItemDefinition()
        //{
        //    if (_bindingSource.Current != null)
        //    {
        //        var itemDefinitionId =
        //            (((ObjectView<SqlInventoryView>)_bindingSource.Current).Object).ItemDefinitionId;
        //        CurrentItem = _repoItemDefinition.FindByKey(itemDefinitionId);
        //    }
        //}

        private void SetCurrentLocation(int locationId)
        {
            var location = _repoLocation.FindByKey(locationId);
            if (location == null) return;
            CurrentLocation = location;
        }

        //private void SetCurrentLocation()
        //{
        //    if (_bindingSource.Current != null)
        //    {
        //        var locationId = (((ObjectView<SqlInventoryView>)_bindingSource.Current).Object).LocationId;
        //        CurrentLocation = _repoLocation.FindByKey(locationId);
        //    }
        //}

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

        //private int GetRecordCount(BindingSource bs)
        //{
        //    int count = bs.Count;
        //    LabelRecordCount.Text = string.Format("Records: {0}", count.ToString());
        //    LabelAvailableLocations.Text = string.Format("Records: {0}", count.ToString());
        //    return count;
        //}

        private int GetRecordCount(BindingSource bs)
        {
            var count = bs.Count;
            LabelRecordCount.Text = $"Records: {count.ToString()}";
            LabelAvailableLocations.Text = string.Format("Records: {0}", count.ToString());
            return count;
        }

        private void DataGridViewCellFormatting(DataGridView dgv, DataGridViewCellFormattingEventArgs e)
        {
            //if (e.RowIndex < 0)
            //{
            //    return;
            //}

            //if (dgv.Columns[e.ColumnIndex].Name.Equals("StorageTypeName"))
            //{
            //    if (e.Value != null)
            //    {
            //        switch (e.Value.ToString())
            //        {
            //            case "Non-Pickable":
            //                dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Yellow;
            //                break;
            //            case "Inactive":
            //                dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.PaleGoldenrod;
            //                break;
            //            case "Static":
            //                dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightGreen;
            //                break;
            //            case "Release":
            //                dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.AliceBlue;
            //                break;
            //            default:
            //                dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
            //                break;
            //        }
            //    }
            //}
        }

        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            //var dgv = sender as DataGridView;
            //DataGridViewCellFormatting(dgv, e);
            //if (e.RowIndex < 0)
            //{
            //    return;
            //}

            //if (dgv.Columns[e.ColumnIndex].Name.Equals("StorageTypeName"))
            //{
            //    if (e.Value != null)
            //    {
            //        switch (e.Value.ToString())
            //        {
            //            case "Non-Pickable":
            //                dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Yellow;
            //                break;
            //            case "Inactive":
            //                dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.PaleGoldenrod;
            //                break;
            //            case "Static":
            //                dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightGreen;
            //                break;
            //            case "Release":
            //                dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.AliceBlue;
            //                break;
            //            default:
            //                dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
            //                break;
            //        }
            //    }
            //}
        }

        private void DataGridViewInventoryLocations_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            //var dgv = sender as DataGridView;
            //DataGridViewCellFormatting(dgv, e);
            ////if (e.RowIndex < 0)
            //{
            //    return;
            //}

            //if (dgv.Columns[e.ColumnIndex].Name.Equals("StorageTypeName"))
            //{
            //    if (e.Value != null)
            //    {
            //        switch (e.Value.ToString())
            //        {
            //            case "Non-Pickable":
            //                dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Yellow;
            //                break;
            //            case "Inactive":
            //                dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Red;
            //                break;
            //            case "Static":
            //                dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Green;
            //                break;
            //            case "Release":
            //                dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.AliceBlue;
            //                break;
            //            default:
            //                dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
            //                break;
            //        }
            //    }
            //}
        }

        #region Find Functions

        private void MButtonFind_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            LoadInventory();
            Cursor.Current = Cursors.Default;
            //FindRecord(TextBoxFind.Text.Trim());
        }

        private void FindRecord()
        {
            //var views = new List<SqlInventoryView>();
            Cursor.Current = Cursors.WaitCursor;
            LoadInventory();
            Cursor.Current = Cursors.Default;
            //var inventory = (IEnumerable<SqlInventoryView>) bindingSource.DataSource;
            //string toFind = s.Trim().ToLower();
            //if (!string.IsNullOrEmpty(s))
            //{
            //    ItemDefinition itemDefinition = repoItemDefinition.FindBy(r => r.Item == toFind).FirstOrDefault();
            //    if (itemDefinition != null)
            //    {
            //        // real itemDefinition 
            //        views = inventory.Where(d => d.ItemDefinitionId == itemDefinition.Id).ToList();
            //        //are there any inventory records
            //    }
            //    else // was not an itemDefinition, just part of something
            //    {
            //        views = inventory.Where(d => d.Item.ToLower().Contains(s) || d.Description.ToLower().Contains(s)).ToList();
            //    }
            //}
            //bindingSourceEquin = new BindingListView<SqlInventoryView>(views);
            //bindingSource.DataSource = bindingSourceEquin;
            //GetRecordCount(bindingSource);
            //DataGridView1.ClearSelection();
        }

        private void TextBoxFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                //FindRecord(TextBoxFind.Text.Trim());
                Cursor.Current = Cursors.WaitCursor;
                LoadInventory();
                Cursor.Current = Cursors.Default;
            }

            if (e.KeyCode == Keys.Escape)
            {
                TextBoxFind.Text = "";
            }
        }

        #endregion

        #region Button Clicks

        private void ButtonClear_Click(object sender, EventArgs e)
        {
            TextBoxFind.Text = string.Empty;
            Cursor.Current = Cursors.WaitCursor;
            LoadInventory();
            Cursor.Current = Cursors.Default;
            TextBoxFind.Focus();
        }

        private void MButtonClose_Click(object sender, EventArgs e)
        {
            ClearAllShi();
            CloseButtonPressed = true;
            Close();
        }

        private void MButtonViewEdit_Click(object sender, EventArgs e)
        {
            //SetCurrentItemDefinition();
            LoadViewEdit();
        }

        private void LoadViewEdit()
        {
            if (CurrentItem != null)
            {
                TextBoxViewEditId.Text = CurrentItem.Id.ToString();
                TextBoxViewEditItem.Text = CurrentItem.Item;
                TextBoxViewEditDescription.Text = CurrentItem.Description;

                var views = GetInventoryViewListByItem(CurrentItem.Id);
                _locationBindingSourceEquin = new BindingListView<SqlInventoryView>(views.ToList());
                _locationBindingSource.DataSource = _locationBindingSourceEquin;
                DataGridViewInventoryLocations.DataSource = _locationBindingSource;

                tabControl1.SelectedTab = tabPage2;
                DataGridViewInventoryLocations.ClearSelection();
            }
        }

        private void MButtonNew_Click(object sender, EventArgs e)
        {
            ClearNewFields();

            tabControl1.SelectedTab = tabPage3;
        }

        private void MbViewEditListing_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            LoadInventory();
            Cursor.Current = Cursors.Default;
            tabControl1.SelectedTab = tabPage1;
        }

        private void MbViewEditNew_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage3;
        }

        private void MbNewFind_Click(object sender, EventArgs e)
        {
            NewItemFind();
        }

        private void MbViewEditClose_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage1;
        }

        private void MbViewEditSave_Click(object sender, EventArgs e)
        {
            UpdateViewEdit();
        }

        private void MbViewEditEdit_Click(object sender, EventArgs e)
        {

            var inventoryView = ((ObjectView<SqlInventoryView>)_locationBindingSource.Current).Object;
            TextBoxAddDetailItem.Text = inventoryView.Item;
            TextBoxAddDetailDescription.Text = inventoryView.Description;
            DateTimePickerAddDetailReceivedDate.Value = inventoryView.ReceivedDate;
            TextBoxAddDetailInventoryId.Text = inventoryView.Id.ToString();
            TextBoxAddDetailLocationId.Text = inventoryView.LocationId.ToString();
            TextBoxAddDetailQuantity.Text = inventoryView.Quantity.ToString();
            TextBoxAddDetailItemDefinitionId.Text = inventoryView.ItemDefinitionId.ToString();
            ComboBoxAddDetailStation.SelectedValue = inventoryView.StationId;
            CheckBoxAddDetailPrimeBin.Checked = inventoryView.PrimeBin;
            TextBoxAddDetailLoc1.Text = inventoryView.Loc1.ToString();
            TextBoxAddDetailLoc2.Text = inventoryView.Loc2.ToString();
            TextBoxAddDetailLoc3.Text = inventoryView.Loc3.ToString();
            TextBoxAddDetailLoc4.Text = inventoryView.Loc4.ToString();
            TextBoxAddDetailLoc5.Text = inventoryView.Loc5.ToString();
            TextBoxAddDetailSlot.Text = inventoryView.Slot.ToString();
            ComboBoxAddDetailStorageType.SelectedValue = inventoryView.StorageTypeId;
            ComboBoxAddDetailSizeCode.SelectedValue = inventoryView.SizeCodeId;
            ComboBoxAddDetailVelocityCode.SelectedValue = inventoryView.VelocityCodeId;
            ComboBoxAddDetailHeightCode.SelectedValue = inventoryView.HeightCodeId;
            ComboBoxAddDetailLocationCode.SelectedValue = inventoryView.LocationCodeId;
            CheckBoxInUse.Checked = inventoryView.InUse;
            LabelActionAddDetail.Text = "Edit Detail";
            TextBoxAddDetailQuantity.Focus();


            tabControl1.SelectedTab = tabPage5;

        }

        private void MbNewListing_Click(object sender, EventArgs e)
        {

            Cursor.Current = Cursors.WaitCursor;
            LoadInventory();
            Cursor.Current = Cursors.Default;
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
            Cursor.Current = Cursors.WaitCursor;
            LoadInventory();
            Cursor.Current = Cursors.Default;
            tabControl1.SelectedTab = tabPage1;
        }

        #endregion

        private void SaveNew()
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
                    MessageBox.Show("Invalid Entry.");
                    return;
                }

                if (IsDuplicate(rec))
                {
                    MessageBox.Show("Duplicate Entry.");
                    return;
                }

                _repoInventory.Insert(rec);
                GlobalVar.HistoryManager.SaveHistory(ActionCode.InventoryAdd, rec);
                LoadInventory(rec.Id);


                tabControl1.SelectedTab = tabPage1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save Error. " + ex.Message + ex.InnerException + ex.InnerException.InnerException);
            }
        }

        private void UpdateViewEdit()
        {
            var id = (((ObjectView<SqlInventoryView>)_bindingSource.Current).Object).Id;
            var rec = new Inventory
            {
                Id = id,
                LocationId = (((ObjectView<SqlInventoryView>)_bindingSource.Current).Object).LocationId,
                ItemDefinitionId = (((ObjectView<SqlInventoryView>)_bindingSource.Current).Object).ItemDefinitionId,
            };
            if (!ValidateFields(rec))
            {
                MessageBox.Show("Invalid Entry.");
                return;
            }

            _repoInventory.Update(rec);
            GlobalVar.HistoryManager.SaveHistory(ActionCode.InventoryModify, rec);
            LoadInventory(rec.Id);


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

        private bool IsDuplicate(Inventory recIn)
        {
            var rec = _repoInventory
                .FindBy(f => f.LocationId == recIn.LocationId && f.ItemDefinitionId == recIn.ItemDefinitionId)
                .FirstOrDefault();
            if (rec != null)
            {
                MessageBox.Show("Record already exists.", "Duplicate Entry", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return true;
            }

            return false;
        }

        //private string CreateSlot(int station, string loc1, string loc2, string loc3, string loc4, string loc5)
        //{
        //    var sb = new StringBuilder();
        //    sb.Append(station.ToString().PadLeft(2, '0'));
        //    sb.Append(loc1.PadLeft(2, '0'));
        //    sb.Append(loc2.PadLeft(2, '0'));
        //    sb.Append(loc3.PadLeft(2, '0'));
        //    sb.Append(loc4.PadLeft(2, '0'));
        //    sb.Append(loc5.PadLeft(2, '0'));
        //    return sb.ToString();
        //}

        #region Form Setup Grids

        private void SetupGrids()
        {
            DataGridView1.AutoGenerateColumns = false;
            DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridView1.AllowUserToAddRows = false;

            //var w = (DataGridView1.Width - 60) / 14;

            var bCol = new DataGridViewButtonColumn
            {
                HeaderText = "",
                Visible = true,
                Name = "Position",
                Text = "Position",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                UseColumnTextForButtonValue = true
            };
            DataGridView1.Columns.Add(bCol);

            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StationName",
                HeaderText = "Station",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "StationName"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = "Item",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Item"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = "Description",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Description"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = "Quantity",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Quantity"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ReceivedDate",
                HeaderText = "Received Date",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "ReceivedDate"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Slot",
                HeaderText = "Slot",
                Visible = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Slot"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SizeCodeName",
                HeaderText = "Size Code",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "SizeCodeName"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc1",
                HeaderText = _nomenclature.LabelDevice,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc1"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc2",
                HeaderText = _nomenclature.LabelTray,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc2"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc3",
                HeaderText = _nomenclature.LabelOver,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc3"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc4",
                HeaderText = _nomenclature.LabelBack,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc4"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc5",
                HeaderText = "Tag",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc5"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "VelocityCodeName",
                HeaderText = "Velocity Code",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "VelocityCodeName"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "HeightCodeName",
                HeaderText = "Height Code",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "HeightCodeName"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LocationCodeName",
                HeaderText = "User Code",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "LocationCodeName"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StorageTypeName",
                HeaderText = "Storage Type",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "StorageTypeName"
            };
            DataGridView1.Columns.Add(col);

            var xcol = new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "PrimeBin",
                HeaderText = "Prime Bin",
                Visible = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "PrimeBin"
            };
            DataGridView1.Columns.Add(xcol);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = "Id",
                Visible = false,
                Name = "Id"
            };
            DataGridView1.Columns.Add(col);


            foreach (DataGridViewColumn column in DataGridView1.Columns)
            {
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);
            }


            //Location Grid

            DataGridViewInventoryLocations.AutoGenerateColumns = false;
            DataGridViewInventoryLocations.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewInventoryLocations.AllowUserToAddRows = false;

            bCol = new DataGridViewButtonColumn
            {
                HeaderText = "",
                Visible = true,
                Name = "Position",
                Text = "Position",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                UseColumnTextForButtonValue = true
            };
            DataGridViewInventoryLocations.Columns.Add(bCol);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StationName",
                HeaderText = "Station",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "StationName"
            };
            DataGridViewInventoryLocations.Columns.Add(col);


            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = "Quantity",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Quantity"
            };
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ReceivedDate",
                HeaderText = "Received Date",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "ReceivedDate"
            };
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Slot",
                HeaderText = "Slot",
                Visible = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Slot"
            };
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SizeCodeName",
                HeaderText = "Size Code",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "SizeCodeName"
            };
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc1",
                HeaderText = _nomenclature.LabelDevice,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc1"
            };
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc2",
                HeaderText = _nomenclature.LabelTray,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc2"
            };
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc3",
                HeaderText = _nomenclature.LabelOver,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc3"
            };
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc4",
                HeaderText = _nomenclature.LabelBack,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc4"
            };
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc5",
                HeaderText = "Tag",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc5"
            };
            DataGridViewInventoryLocations.Columns.Add(col);



            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "VelocityCodeName",
                HeaderText = "Velocity Code",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "VelocityCodeName"
            };
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "HeightCodeName",
                HeaderText = "Height Code",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "HeightCodeName"
            };
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StorageTypeName",
                HeaderText = "Storage Type",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "StorageTypeName"
            };
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LocationCodeName",
                HeaderText = "User Code",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "LocationCodeName"
            };
            DataGridViewInventoryLocations.Columns.Add(col);

            xcol = new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "PrimeBin",
                HeaderText = "Prime Bin",
                Visible = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "PrimeBin"
            };
            DataGridViewInventoryLocations.Columns.Add(xcol);


            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = "Id",
                Visible = false,
                Name = "Id"
            };
            DataGridViewInventoryLocations.Columns.Add(col);

            foreach (DataGridViewColumn column in DataGridViewInventoryLocations.Columns)
            {
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);
            }




            //New Location Grid

            DataGridViewInventoryNewLocations.AutoGenerateColumns = false;
            DataGridViewInventoryNewLocations.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewInventoryNewLocations.AllowUserToAddRows = false;

            bCol = new DataGridViewButtonColumn
            {
                HeaderText = "",
                Visible = true,
                Name = "Position",
                Text = "Position",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                UseColumnTextForButtonValue = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(bCol);

            xcol = new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "InUse",
                HeaderText = "In Use",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "InUse"
            };
            DataGridViewInventoryNewLocations.Columns.Add(xcol);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StationName",
                HeaderText = "Station",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "StationName",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);



            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Slot",
                HeaderText = "Slot",
                Visible = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Slot"
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);


            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SizeCodeName",
                HeaderText = "Size Code",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "SizeCodeName",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc1",
                HeaderText = _nomenclature.LabelDevice,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc1",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc2",
                HeaderText = _nomenclature.LabelTray,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc2",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc3",
                HeaderText = _nomenclature.LabelOver,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc3",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc4",
                HeaderText = _nomenclature.LabelBack,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc4",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc5",
                HeaderText = "Tag",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc5",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "VelocityCodeName",
                HeaderText = "Velocity Code",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "VelocityCodeName",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "HeightCodeName",
                HeaderText = "Height Code",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "HeightCodeName",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LocationCodeName",
                HeaderText = "User Code",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "LocationCodeName"
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = "Id",
                Visible = false,
                Name = "Id"
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);

            foreach (DataGridViewColumn column in DataGridViewInventoryNewLocations.Columns)
            {
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);
            }

        }


        private void DataGridViewInventoryLocations_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewPosition((DataGridView)sender, e.RowIndex);
        }

        private void DataGridViewInventoryAddDetailLocations_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewPosition((DataGridView)sender, e.RowIndex);
        }

        private void SetupNewForm()
        {
            LabelFindDescription.Text = "Search any part of the Item or Description field";

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

            ComboBoxAddDetailLocationCode.DataSource = _repoLocationCode.All();
            ComboBoxAddDetailLocationCode.DisplayMember = "Name";
            ComboBoxAddDetailLocationCode.ValueMember = "Id";

            ComboBoxAddDetailStation.DataSource = _repoStation.Lookup();
            ComboBoxAddDetailStation.DisplayMember = "Name";
            ComboBoxAddDetailStation.ValueMember = "Id";

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

        private void MbViewEditDelete_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (_bindingSource.Current != null)
            {
                var locView = ((ObjectView<SqlInventoryView>)_locationBindingSource.Current).Object;

                if (locView != null)
                {
                    // var invId = (((ObjectView<SqlInventoryView>)_bindingSource.Current).Object).Id;


                    DeleteInventoryItem(locView.Id, false);
                   // LoadInventory();
                    LoadViewEdit();
                    tabControl1.SelectedTab = tabPage2;
                    //LoadInventory();
                    //tabControl1.SelectedTab = tabPage1; 
                }
            }
            else
            {
                MessageBox.Show(@"No Inventory Records.");
            }

            Cursor.Current = Cursors.Default;
        }

        public void DeleteInventoryItem(int invId, bool releaseOnly)
        {
            var inventoryManager = new InventoryManager(_repoInventory, _locationsRepository);
            inventoryManager.DeleteInventoryRecord(invId, releaseOnly: releaseOnly);
        }

        //private void DeleteInventoryRecord(int invId, bool releaseOnly = false)
        //{
        //    var inventory = _repoInventory.FindByKey(invId);
        //    if (inventory == null) return;
        //    if (releaseOnly)
        //    {
        //        if (inventory.StorageTypeId == (int)StorageType.Release)
        //        {
        //            GlobalVar.HistoryManager.SaveHistory(ActionCode.InventoryDelete, inventory);
        //            _locationsRepository.SetLocationInUse(inventory.LocationId, b: false);
        //            _repoInventory.Delete(inventory.Id);
        //        }
        //    }
        //    else
        //    {
        //        GlobalVar.HistoryManager.SaveHistory(ActionCode.InventoryDelete, inventory);
        //        _locationsRepository.SetLocationInUse(inventory.LocationId, b: false);
        //        _repoInventory.Delete(inventory.Id);
        //    }
        //}

        private void MbViewEditAddLocation_Click(object sender, EventArgs e)
        {
            AddLocation();
        }

        private void AddLocation()
        {
           // var id = CurrentItem.Id;
          //  var itemDefinition = _repoItemDefinition.FindByKey(id);
          //  if (itemDefinition != null)
            //{
            //    TextBoxInventoryNewLocationsItem.Text = itemDefinition.Item;
            //    TextBoxInventoryNewLocationsDescription.Text = itemDefinition.Description;
            //    TextBoxNewLocationsItemDefinitionId.Text = itemDefinition.Id.ToString();
            //    ComboBoxInventoryNewLocationsStorageType.SelectedIndex = 2;
            //    GetAvailableLocations(itemDefinition);
            //}
            if(CurrentItem != null)
            {
                TextBoxInventoryNewLocationsItem.Text = CurrentItem.Item;
                TextBoxInventoryNewLocationsDescription.Text = CurrentItem.Description;
                TextBoxNewLocationsItemDefinitionId.Text = CurrentItem.Id.ToString();
                TextBoxInventoryNewLocationsQuantity.Text = "0";
                ComboBoxInventoryNewLocationsStorageType.SelectedIndex = 1;
                GetAvailableLocations(CurrentItem);
            }

            tabControl1.SelectedTab = tabPage4;
        }

        public async void GetAvailableLocations(ItemDefinition itemDefinition, int recId = 0)
        {
            var idx = recId;
            if (itemDefinition == null) return;
            
            var inUse = false;

            var views = await Task.Run(() => _locationsRepository.GetAllLocationViewsExact(itemDefinition.StationId,
                itemDefinition.SizeCodeId, itemDefinition.VelocityCodeId, itemDefinition.HeightCodeId,
                itemDefinition.LocationCodeId, inUse));

            _newLocationBindingSourceEquin = new BindingListView<LocationView>(views.ToList());
            _newLocationBindingSource.DataSource = _newLocationBindingSourceEquin;

            DataGridViewInventoryNewLocations.DataSource = _newLocationBindingSource;

            if (GetRecordCount(_newLocationBindingSource) > 0)
            {
                if (recId != 0)
                {
                    idx = IndexOf(_newLocationBindingSource, recId);
                }

                DataGridViewInventoryNewLocations.FirstDisplayedScrollingRowIndex =
                    DataGridViewInventoryNewLocations.Rows[idx].Index;
                DataGridViewInventoryNewLocations.Refresh();
                DataGridViewInventoryNewLocations.CurrentCell = DataGridViewInventoryNewLocations.Rows[idx].Cells[1];
                DataGridViewInventoryNewLocations.Rows[idx].Selected = true;
            }
            
        }

        public async void GetAllAvailableLocations(ItemDefinition itemDefinition, int recId = 0)
        {
            var idx = recId;
            IEnumerable<LocationView> views = new List<LocationView>();
            IEnumerable<LocationView> recs = new List<LocationView>();

            views = await Task.Run(() => _locationsRepository.FindLocationViewsByStation(itemDefinition.StationId));
            _newLocationBindingSourceEquin = new BindingListView<LocationView>(views.ToList());
            _newLocationBindingSource.DataSource = _newLocationBindingSourceEquin;

            DataGridViewInventoryNewLocations.DataSource = _newLocationBindingSource;

            if (GetRecordCount(_newLocationBindingSource) > 0)
            {
                if (recId != 0)
                {
                    idx = IndexOf(_newLocationBindingSource, recId);
                }

                DataGridViewInventoryNewLocations.FirstDisplayedScrollingRowIndex =
                    DataGridViewInventoryNewLocations.Rows[idx].Index;
                DataGridViewInventoryNewLocations.Refresh();
                DataGridViewInventoryNewLocations.CurrentCell = DataGridViewInventoryNewLocations.Rows[idx].Cells[1];
                DataGridViewInventoryNewLocations.Rows[idx].Selected = true;
            }
        }

        private void FrmInventory_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !CloseButtonPressed;
        }

        private void TextBoxNewItem_Leave(object sender, EventArgs e)
        {
            // get the item and fill in the form
            NewItemFind();
        }

        private void NewItemFind()
        {
            var item = TextBoxNewItem.Text;
            try
            {
                var rec = _repoItemDefinition.FindBy(f => f.Item == item).FirstOrDefault();

                if (rec != null)
                {
                    TextBoxNewId.Text = rec.Id.ToString();
                    TextBoxNewItem.Text = rec.Item;
                    TextBoxNewDescription.Text = rec.Description;
                    CheckBoxNewScale.Checked = rec.Scale;
                    ComboBoxNewStation.SelectedValue = rec.StationId;
                    TextBoxNewLocationMax.Text = rec.LocationMax.ToString();
                    TextBoxNewLocationMin.Text = rec.LocationMin.ToString();
                    TextBoxNewSystemMax.Text = rec.SystemMax.ToString();
                    TextBoxNewSystemMin.Text = rec.SystemMin.ToString();
                    TextBoxNewWeight.Text = rec.Weight.ToString("F4");
                    ComboBoxNewStorageType.SelectedValue = rec.StorageTypeId;
                    ComboBoxNewUnitOfIssue.SelectedValue = rec.UnitOfIssueId;
                    ComboBoxNewSizeCode.SelectedValue = rec.SizeCodeId;
                    ComboBoxNewVelocityCode.SelectedValue = rec.VelocityCodeId;
                    ComboBoxNewHeightCode.SelectedValue = rec.HeightCodeId;
                    ComboBoxNewLocationCode.SelectedValue = rec.LocationCodeId;

                    MbNewAddLocation.Enabled = true;
                }
                else
                {
                    MessageBox.Show("Item not found.");
                    MbNewAddLocation.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("New Item Leave: " + ex.Message + ex.InnerException.Message);
                TextBoxNewItem.Focus();
            }
        }

        private void MbNewAddLocation_Click(object sender, EventArgs e)
        {
            var id = (TextBoxNewId.Text).ParseInt();
           // CurrentItem = _repoItemDefinition.FindByKey(id);
            SetCurrentItemDefinition(id);
            if (CurrentItem != null)
            {
                TextBoxInventoryNewLocationsItem.Text = CurrentItem.Item;
                TextBoxInventoryNewLocationsDescription.Text = CurrentItem.Description;
                GetAvailableLocations(CurrentItem);
            }

            tabControl1.SelectedTab = tabPage4;
        }

        private void MbNewLocationsClose_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage2;
           // SetCurrentItemDefinition();
            LoadViewEdit();
        }

        private void MbNewLocationsListing_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            LoadInventory();
            Cursor.Current = Cursors.Default;

            tabControl1.SelectedTab = tabPage1;
        }

        private void MbNewLocationsSave_Click(object sender, EventArgs e)
        {
            if (TextBoxInventoryNewLocationsQuantity.Text.ParseInt() == 0 &&
                ComboBoxInventoryNewLocationsStorageType.SelectedValue.ToString().ParseInt() ==
                (int)StorageType.Release)
            {
                MessageBox.Show(
                    @"You can't create an Inventory Item with a Release storage type and (0) quantity.",
                    @"Inventory Manager", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                TextBoxInventoryNewLocationsQuantity.Focus();
            }
            else
            {

                var locView = ((ObjectView<LocationView>)_newLocationBindingSource.Current).Object;

                if (locView != null)
                {
                    //var inv = _repoInventory
                    //    .FindBy(r => r.ItemDefinitionId == CurrentItem.Id && r.LocationId == locView.Id)
                    //    .FirstOrDefault();
                    //if (inv == null)
                    //{
                        var inventory = new Inventory()
                        {
                            ItemDefinitionId = CurrentItem.Id,
                            LocationId = locView.Id,
                            Quantity = TextBoxInventoryNewLocationsQuantity.Text.ParseInt(),
                            ReceivedDate = DateTimePickerInventoryNewLocationsReceivedDate.Value,
                            StorageTypeId =
                                ComboBoxInventoryNewLocationsStorageType.SelectedValue.ToString().ParseInt(),
                            PrimeBin = CheckBoxInventoryNewLocationsPrimeBin.Checked,
                            StationId = CurrentItem.StationId
                        };
                        // MessageBox.Show(@"Inserting new Inventory Item.");
                        _repoInventory.Insert(inventory);
                        GlobalVar.HistoryManager.SaveHistory(ActionCode.InventoryAdd, inventory);
                        _locationsRepository.SetLocationInUse(inventory.LocationId, b: true);

                        SetCurrentInventoryItem(inventory);
                        LoadViewEdit();
                        tabControl1.SelectedTab = tabPage2;
                    //}
                    //else
                    //{
                    //    MessageBox.Show(@"That Inventory Item Already Exists.", caption: @"Inventory Manager",
                    //        buttons: MessageBoxButtons.OK);
                    //    TextBoxInventoryNewLocationsQuantity.Focus();
                    //}
                }

                //SetCurrentItemDefinition();
                //LoadViewEdit();
                //tabControl1.SelectedTab = tabPage2;

            }


        }

        private IEnumerable<SqlInventoryView> GetInventoryViewListByItem(int itemId)
        {
            IEnumerable<SqlInventoryView> projection;
            using (var context = new NeutronDb())
            {
                var findItemId = new SqlParameter("@ItemId", itemId);
                projection = context.Database
                    .SqlQuery<SqlInventoryView>("usp_GetInventoryViewByItem @ItemId", findItemId).ToList();

            }

            return projection;
        }

        private IEnumerable<SqlInventoryView> GetInventoryViewList()
        {
            IEnumerable<SqlInventoryView> projection;

            using (var context = new NeutronDb())
            {
                projection = context.Database.SqlQuery<SqlInventoryView>("usp_GetInventoryView").ToList();
            }

            return projection;
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
            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(0, 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;
            foreach (TabPage tab in tabControl1.TabPages)
            {
                tab.Text = string.Empty;
            }
        }

        private void MbAddDetailSave_Click(object sender, EventArgs e)
        {
            var inventory = new Inventory();
            var inventoryId = TextBoxAddDetailInventoryId.Text;
            if (!string.IsNullOrEmpty(inventoryId))
            {
                //update
                inventory = _repoInventory.FindByKey(inventoryId.ParseInt());
                inventory.Quantity = (TextBoxAddDetailQuantity.Text).ParseInt();
                inventory.StorageTypeId =
                    (ComboBoxAddDetailStorageType.SelectedItem as NeutronData.Models.Lookups.StorageType).Id;
                inventory.ReceivedDate = DateTimePickerAddDetailReceivedDate.Value;
                inventory.PrimeBin = CheckBoxAddDetailPrimeBin.Checked;
                inventory.StationId = _station.StationId;
                _repoInventory.Update(inventory);
                GlobalVar.HistoryManager.SaveHistory(ActionCode.InventoryModify, inventory);
            }
            else
            {
                //new
                inventory.ItemDefinitionId = (TextBoxAddDetailItemDefinitionId.Text).ParseInt();
                inventory.LocationId = (TextBoxAddDetailLocationId.Text).ParseInt();
                inventory.Quantity = (TextBoxAddDetailQuantity.Text).ParseInt();
                inventory.StorageTypeId =
                    (ComboBoxAddDetailStorageType.SelectedItem as NeutronData.Models.Lookups.StorageType).Id;
                inventory.ReceivedDate = DateTimePickerAddDetailReceivedDate.Value;
                inventory.PrimeBin = CheckBoxAddDetailPrimeBin.Checked;
                inventory.StationId = _station.StationId;
                _repoInventory.Insert(inventory);
                GlobalVar.HistoryManager.SaveHistory(ActionCode.InventoryAdd, inventory);
            }

            if (inventory.Quantity == 0 && inventory.StorageTypeId == (int)StorageType.Release)
            {
                DeleteInventoryItem(inventory.Id, releaseOnly: true);
                // GlobalVar.HistoryManager.SaveHistory(ActionCode.InventoryDelete, inventory);
            }
            else
            {
                _locationsRepository.SetLocationInUse(inventory.LocationId, b: true);
                TextBoxViewEditId.Text = CurrentItem.Id.ToString();
                TextBoxViewEditItem.Text = CurrentItem.Item;
                TextBoxViewEditDescription.Text = CurrentItem.Description;
            }

            var views = GetInventoryViewListByItem(CurrentItem.Id);
            _locationBindingSourceEquin = new BindingListView<SqlInventoryView>(views.ToList());
            _locationBindingSource.DataSource = _locationBindingSourceEquin;
            DataGridViewInventoryLocations.DataSource = _locationBindingSource;

            tabControl1.SelectedTab = tabPage2;
        }



        private void MbAddDetailClose_Click(object sender, EventArgs e)
        {
            //LoadInventory();
            //SetCurrentItemDefinition();
            LoadViewEdit();
            tabControl1.SelectedTab = tabPage2;
        }

        private void MbAddDetailListing_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            LoadInventory();
            Cursor.Current = Cursors.Default;

            tabControl1.SelectedTab = tabPage1;
        }

        private void ClearAddDetailFields()
        {
            TextBoxAddDetailInventoryId.Text = "";
            TextBoxAddDetailLocationId.Text = "";
            TextBoxAddDetailQuantity.Text = "";
            TextBoxAddDetailItemDefinitionId.Text = "";
            ComboBoxAddDetailStation.SelectedValue = -1;
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
            ComboBoxAddDetailLocationCode.SelectedValue = -1;
        }

        private void ClearNewFields()
        {
            TextBoxNewId.Text = "";
            TextBoxNewItem.Text = "";
            TextBoxNewDescription.Text = "";
            CheckBoxNewScale.Checked = false;
            CheckBoxInUse.Checked = false;
            ComboBoxNewStation.SelectedValue = -1;
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
            ComboBoxNewLocationCode.SelectedValue = -1;
        }

        public bool AllAvailable
        {
            get { return _allAvailable; }
            set
            {
                _allAvailable = value;
                MbNewAvailableLocations.Text = _allAvailable ? "Preferred" : "Show All";
            }
        }

        private void MbNewAvailableLocations_Click(object sender, EventArgs e)
        {
            AllAvailable = !_allAvailable;
            if (_allAvailable)
            {
                GetAllAvailableLocations(CurrentItem);
            }
            else
            {
                GetAvailableLocations(CurrentItem);
            }
        }

        private void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            ////if button cell position device, otherwise open View/Edit
            //var grid = (DataGridView) sender;
            //if (e.RowIndex <= 0) return;
            //else
            //{
            //    SetCurrentItemDefinition();
            //    TextBoxViewEditId.Text = CurrentItem.Id.ToString();
            //    TextBoxViewEditItem.Text = CurrentItem.Item;
            //    TextBoxViewEditDescription.Text = CurrentItem.Description;
            //    var views = (IEnumerable<SqlInventoryView>) GetInventoryViewListByItem(CurrentItem.Id);
            //    locationBindingSourceEquin = new BindingListView<SqlInventoryView>(views.ToList());

            //    locationBindingSource.DataSource = locationBindingSourceEquin;

            //    DataGridViewInventoryLocations.DataSource = locationBindingSource;
            //    GetRecordCount(locationBindingSource);

            //    tabControl1.SelectedTab = tabPage2;
            //}
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewPosition((DataGridView)sender, e.RowIndex);
            SetCurrentInventoryItem();
        }

        private void DataGridViewPosition(DataGridView grid, int rowIndex)
        {
            var text = "----";
            if (rowIndex >= 0)
            {
                if (grid.CurrentCell.ColumnIndex == grid.Columns["Position"].Index)
                {
                    var deviceNumber = grid["Loc1", rowIndex].Value.ToString().ParseInt();
                    var trayNumber = grid["Loc2", rowIndex].Value.ToString().ParseInt();
                    var level = grid["Loc3", rowIndex].Value.ToString().ParseInt();
                    var partition = grid["Loc4", rowIndex].Value.ToString();
                    var part = grid["Loc4", rowIndex].Value.ToString().ParseInt();
                    var quantity = grid["Quantity", rowIndex].Value.ToString().ParseInt();
                    var display = string.Empty;
                    if (_neutronVariables.ShuttleEnabled)
                    {
                        var hardwareDevice = _station.HardwareDevices.FirstOrDefault(s => s.DeviceNumber == deviceNumber);
                        if (hardwareDevice != null)
                        {
                            if (hardwareDevice.Enabled == true)
                            {

                                if (GlobalVar.Shuttle != null)
                                {
                                    var response = Task.Run(() =>
                                        GlobalVar.Shuttle.PositionDevice(deviceNumber, trayNumber, level, part,
                                            quantity, display));
                                    if (response.Result != DeviceResponse.Success)
                                    {
                                        MessageBox.Show(response.Result.AsString(EnumFormat.Description),
                                            caption: "Device Information"
                                            , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show($"Device Controller is not properly initialized.");
                                }

                            }
                            else
                            {
                                MessageBox.Show($"Hardware Device {hardwareDevice.Name} is not enabled.");
                            }

                        }
                        else
                        {
                            MessageBox.Show($"Hardware Device not found.");

                        }
                    }

                    if (_neutronVariables.DisplaysEnabled)
                    {
                        if (GlobalVar.Displays != null)
                        {
                            ClearAllShi();
                            if (grid.Columns.Contains(columnName: "Quantity"))
                            {
                                var qty = grid["Quantity", rowIndex].Value.ToString();
                                text = ($"{qty.PadLeft(6, paddingChar: ' ')}");

                                GlobalVar.Displays.ShowShi(deviceNumber, trayNumber, level, partition, text);
                            }
                        }
                    }
                }
            }
        }

        private void ClearAllShi()
        {
            if (_neutronVariables.DisplaysEnabled)
                if (GlobalVar.Displays != null)
                    GlobalVar.Displays.ClearAllShi();
        }

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

        private void CheckBoxAllStations_CheckedChanged(object sender, EventArgs e)
        {
            if(!_firstTime) FindRecord();
        }

        private void MBCreateInventoryFile_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            MBCreateInventoryFile.Enabled = false;
            var comboBoxValue = ComboBoxStationNumber.Text;
            CreateInventoryFileByStation(comboBoxValue);
            Cursor.Current = Cursors.Default;
            MBCreateInventoryFile.Enabled = true;
        }

        private void CreateInventoryFileByStation(string stationNumber)
        {
            var fileName = GetFileName();
            Task.Run(() => CsvUtility.SaveToCsv(fileName, stationNumber));
        }

        private static string GetFileName()
        {
            var rootDirectory = Environment.ExpandEnvironmentVariables(@"%SystemDrive%\Neutron\CSV\");
            if (!Directory.Exists(rootDirectory))
            {
                Directory.CreateDirectory(rootDirectory);
            }
            var sfd = new SaveFileDialog
            {
                InitialDirectory = rootDirectory,
                Filter = "CSV (*.csv)|*.csv",
                FileName = "Output.csv"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return sfd.FileName;
            MessageBox.Show("Data will be exported and you will be notified when it is ready.");
            if (!File.Exists(sfd.FileName)) return sfd.FileName;
            try
            {
                File.Delete(sfd.FileName);
            }
            catch (IOException ex)
            {
                MessageBox.Show("It wasn't possible to write the data to the disk." + ex.Message);
            }

            return sfd.FileName;
        }
    }
}