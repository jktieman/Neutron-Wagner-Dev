using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using JsonManager;
using MetroFramework.Forms;
using Neutron.Controllers;
using Neutron.Enums;
using Neutron.Extensions;
using Neutron.Global;
using NeutronData.DataContexts;
using NeutronData.ModelViews;
using NeutronData.Models;
using NeutronData.Models.Lookups;
using NeutronData.Repositories;
using NeutronData.SqlModelViews;
using Neutron.Classes;
using Equin.ApplicationFramework;
using System.ComponentModel;

namespace Neutron.Forms
{
    public partial class FrmInventory : MetroForm
    {
        private BindingListView<SqlInventoryView> bindingSourceEquin;
        private BindingSource bindingSource = new BindingSource();
        private BindingSource locationBindingSource = new BindingSource();
        private BindingSource newLocationBindingSource = new BindingSource();
        private LocationsRepository locationsRepository = new LocationsRepository();
        private InventoryRepository inventoryRepository = new InventoryRepository();

        private GenericRepository<ItemDefinition> repoItemDefinition = new GenericRepository<ItemDefinition>(new NeutronDb());
        private GenericRepository<Location> repoLocation = new GenericRepository<Location>(new NeutronDb());
        private GenericRepository<SizeCode> repoSizeCode = new GenericRepository<SizeCode>(new NeutronDb());
        private GenericRepository<VelocityCode> repoVelocityCode = new GenericRepository<VelocityCode>(new NeutronDb());
        private GenericRepository<HeightCode> repoHeightCode = new GenericRepository<HeightCode>(new NeutronDb());
        private GenericRepository<LocationCode> repoLocationCode = new GenericRepository<LocationCode>(new NeutronDb());
        private GenericRepository<Inventory> repoInventory = new GenericRepository<Inventory>(new NeutronDb());
        private GenericRepository<NeutronData.Models.Lookups.StorageType> repoStorageType = new GenericRepository<NeutronData.Models.Lookups.StorageType>(new NeutronDb());
        private GenericRepository<UnitOfIssue> repoUnitOfIssue = new GenericRepository<UnitOfIssue>(new NeutronDb());
        private GenericRepository<HardwareDevice> repoHardwareDevices = new GenericRepository<HardwareDevice>(new NeutronDb());
        private GenericRepository<Station> repoStation = new GenericRepository<Station>(new NeutronDb());


        public ItemDefinition CurrentItem { get; set; }
        public Location CurrentLocation { get; set; }
        public Inventory CurrentInventoryItem { get; set; }
        private TabPage previousTab;
        public bool CloseButtonPressed { get; set; }
        private bool allAvailable;
        IJsonData jsonData;

        public FrmInventory(IJsonData jsonData)
        {
            this.jsonData = jsonData;
            InitializeComponent();
            SetupGrids();
            HideTabControlTabs();
            SetupNewForm();
            SetupViewEditForm();
            SetupAddDetailForm();
            mlUserInfo.Text = Variables.user?.UserInfo;
            CloseButtonPressed = false;
            SetupShuttle();
        }

        private void SetupShuttle()
        {
            if (Variables.Shuttle == null && !Variables.ShuttleStationDisabled)
            {
                NeutronVariables neutronVariables = jsonData.LoadFile<NeutronVariables>();
                if (neutronVariables.StationNumber == default(int))
                {
                    neutronVariables.StationNumber = 1;
                    jsonData.SaveFile<NeutronVariables>(neutronVariables);
                }
                Station station = repoStation.FindByInclude(s => s.StationNumber == neutronVariables.StationNumber, c => c.TcpConfiguration).FirstOrDefault();
                List<HardwareDevice> hardwareDevices = repoHardwareDevices.All().Where(r => r.StationId == station.Id).ToList();
                Variables.Shuttle = new C3000(this, station);
            }
        }

        private async void FrmInventory_Load(object sender, EventArgs e)
        {
            int id = await LoadInventory();
        }

        // Set the focus to the passed in recId if it's passed in
        private async Task<int> LoadInventory(int recId = 0)
        {
            int idx = 0;
            IEnumerable<SqlInventoryView> views = new List<SqlInventoryView>();
            IEnumerable<SqlInventoryView> recs = new List<SqlInventoryView>();
            string findWhat = TextBoxFind.Text.ToLower().Trim();
            
            views = await Task.Run(() => inventoryRepository.GetAllInventoryViews(findWhat));
            bindingSource.DataSource = views;
          //  bindingSourceEquin = new BindingListView<SqlInventoryView>(bindingSource.List);
            
            
            DataGridView1.DataSource = bindingSource;
            //DataGridView1.DataSource = bindingSourceEquin;
            if (GetRecordCount(bindingSource) > 0)
            {
                if (recId != 0)
                {
                    idx = IndexOf(bindingSource, recId);
                    DataGridView1.FirstDisplayedScrollingRowIndex = DataGridView1.Rows[idx].Index;
                    DataGridView1.CurrentCell = DataGridView1.Rows[idx].Cells[1];
                    DataGridView1.Rows[idx].Selected = true;
                }
                else
                {
                    DataGridView1.ClearSelection();
                }
                DataGridView1.Refresh();
                DataGridView1.ClearSelection();
                SetCurrentInventoryItem();
                SetCurrentItemDefinition();
                SetCurrentLocation();
            }
            return idx;
        }

        private void SetCurrentInventoryItem()
        {
            int inventoryId = ((SqlInventoryView) bindingSource.Current).Id;
            CurrentInventoryItem = repoInventory.FindByKey(inventoryId);
        }

        private void SetCurrentItemDefinition()
        {
            int itemDefinitionId = ((SqlInventoryView) bindingSource.Current).ItemDefinitionId;
            CurrentItem = repoItemDefinition.FindByKey(itemDefinitionId);
        }

        private void SetCurrentLocation()
        {
            int locationId = ((SqlInventoryView) bindingSource.Current).LocationId;
            CurrentLocation = repoLocation.FindByKey(locationId);
        }

        public int IndexOf(BindingSource bs, int id)
        {
            int count = bs.Count;
            int itemIndex = -1;
            for (int i = 0; i < count; i++)
            {
                int rec = ((SqlInventoryView) bs[i]).Id;
                if (rec == id)
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
            LabelAvailableLocations.Text = string.Format("Records: {0}", count.ToString());
            return count;
        }

        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var dgv = sender as DataGridView;

            if (e.RowIndex < 0)
            {
                return;
            }

            if (dgv.Columns[e.ColumnIndex].Name.Equals("StorageTypeName"))
            {
                if (e.Value != null)
                {
                    switch (e.Value.ToString())
                    {
                        case "Non-Pickable":
                            dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Yellow;
                            break;
                        case "Inactive":
                            dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.PaleGoldenrod;
                            break;
                        case "Static":
                            dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightGreen;
                            break;
                        case "Release":
                            dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.AliceBlue;
                            break;
                        default:
                            dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
                            break;
                    }
                }
            }
        }

        private void DataGridViewInventoryLocations_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var dgv = sender as DataGridView;

            if (e.RowIndex < 0)
            {
                return;
            }

            if (dgv.Columns[e.ColumnIndex].Name.Equals("StorageTypeName"))
            {
                if (e.Value != null)
                {
                    switch (e.Value.ToString())
                    {
                        case "Non-Pickable":
                            dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Yellow;
                            break;
                        case "Inactive":
                            dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Red;
                            break;
                        case "Static":
                            dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Green;
                            break;
                        case "Release":
                            dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.AliceBlue;
                            break;
                        default:
                            dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
                            break;
                    }
                }
            }
        }

        #region Find Functions
        private async void MButtonFind_Click(object sender, EventArgs e)
        {
            await LoadInventory();

            FindRecord(TextBoxFind.Text.Trim());
        }

        private async void FindRecord(string s)
        {
            string toFind = s.Trim().ToLower();
            try
            {
                if (string.IsNullOrEmpty(s))
                {
                    await LoadInventory();
                }
                else
                {
                    var inventory = (IEnumerable<SqlInventoryView>) bindingSource.DataSource;
                    bindingSource.DataSource = inventory.Where(d => d.Item.ToLower().Contains(s) || d.Description.ToLower().Contains(s)).ToList();
                    GetRecordCount(bindingSource);
                    DataGridView1.ClearSelection();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Find Error: " + ex.Message);
            }
        }

        private async void TextBoxFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                await LoadInventory();
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
           await LoadInventory();
            TextBoxFind.Focus();
        }

        private void MButtonClose_Click(object sender, EventArgs e)
        {
            CloseButtonPressed = true;
            this.Close();
        }

        private void MButtonViewEdit_Click(object sender, EventArgs e)
        {
            SetCurrentItemDefinition();
            LoadViewEdit();
        }

        private void LoadViewEdit()
        {
            TextBoxViewEditId.Text = CurrentItem.Id.ToString();
            TextBoxViewEditItem.Text = CurrentItem.Item;
            TextBoxViewEditDescription.Text = CurrentItem.Description;
            locationBindingSource.DataSource = GetInventoryViewListByItem(CurrentItem.Id);
            DataGridViewInventoryLocations.DataSource = locationBindingSource;

            previousTab = tabControl1.SelectedTab;
            tabControl1.SelectedTab = tabPage2;
            DataGridViewInventoryLocations.ClearSelection();
        }

        private void MButtonNew_Click(object sender, EventArgs e)
        {
            previousTab = tabControl1.SelectedTab;
            tabControl1.SelectedTab = tabPage3;
        }

        private async void MbViewEditListing_Click(object sender, EventArgs e)
        {
            previousTab = tabControl1.SelectedTab;
            await LoadInventory();
            tabControl1.SelectedTab = tabPage1;
        }

        private void MbViewEditNew_Click(object sender, EventArgs e)
        {
            previousTab = tabControl1.SelectedTab;
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

            SqlInventoryView inventoryView = ((SqlInventoryView) locationBindingSource.Current);
            SetCurrentItemDefinition();
            SetCurrentLocation();
            TextBoxAddDetailItem.Text = CurrentItem.Item;
            TextBoxAddDetailDescription.Text = CurrentItem.Description;
            TextBoxAddDetailInventoryId.Text = inventoryView.Id.ToString();
            //Location location = inventoryView.Location;
            TextBoxAddDetailLocationId.Text = CurrentLocation.Id.ToString();
            TextBoxAddDetailQuantity.Text = inventoryView.Quantity.ToString();
            TextBoxAddDetailItemDefinitionId.Text = CurrentItem.Id.ToString();
            ComboBoxAddDetailStation.SelectedValue = CurrentLocation.StationId;
            CheckBoxAddDetailPrimeBin.Checked = inventoryView.PrimeBin;
            TextBoxAddDetailLoc1.Text = CurrentLocation.Loc1.ToString();
            TextBoxAddDetailLoc2.Text = CurrentLocation.Loc2.ToString();
            TextBoxAddDetailLoc3.Text = CurrentLocation.Loc3.ToString();
            TextBoxAddDetailLoc4.Text = CurrentLocation.Loc4.ToString();
            TextBoxAddDetailLoc5.Text = CurrentLocation.Loc5.ToString();
            TextBoxAddDetailSlot.Text = CurrentLocation.Slot.ToString();
            ComboBoxAddDetailStorageType.SelectedValue = inventoryView.StorageTypeId;
            ComboBoxAddDetailSizeCode.SelectedValue = CurrentLocation.SizeCodeId;
            ComboBoxAddDetailVelocityCode.SelectedValue = CurrentLocation.VelocityCodeId;
            ComboBoxAddDetailHeightCode.SelectedValue = CurrentLocation.HeightCodeId;
            ComboBoxAddDetailLocationCode.SelectedValue = CurrentLocation.LocationCodeId;
            LabelActionAddDetail.Text = "Edit Detail";
            TextBoxAddDetailQuantity.Focus();

            previousTab = tabControl1.SelectedTab;
            tabControl1.SelectedTab = tabPage5;

        }

        private async void MbNewListing_Click(object sender, EventArgs e)
        {
            previousTab = tabControl1.SelectedTab;
            await LoadInventory();
            tabControl1.SelectedTab = tabPage1;
        }

        private void MbNewViewEdit_Click(object sender, EventArgs e)
        {
            previousTab = tabControl1.SelectedTab;
            tabControl1.SelectedTab = tabPage2;
        }

        private void MbNewSave_Click(object sender, EventArgs e)
        {
            SaveNew();
        }

        private async void MbNewClose_Click(object sender, EventArgs e)
        {
            await LoadInventory();
            tabControl1.SelectedTab = tabPage1;
        }
        #endregion

        private async void SaveNew()
        {
            //if (this.ValidateChildren())
            //{
            try
            {
                //int station = (ComboBoxViewEditStation.SelectedItem as Station).Id;
                //string slt = CreateSlot(station, TextBoxNewLoc1.Text, TextBoxNewLoc2.Text, TextBoxNewLoc3.Text, TextBoxNewLoc4.Text, TextBoxNewLoc5.Text);
                var rec = new Inventory
                {
                    //LocationId = ((SqlInventoryView) bindingSource.Current).LocationId,
                    ItemDefinitionId = ((SqlInventoryView) bindingSource.Current).ItemDefinitionId,

                    //Quantity = TextBoxNewInventoryQuantity.Text.ParseInt(),
                    // ReceivedDate = DateTimePickerNewReceivedDate.Value
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
                //TextBoxNewSlot.Text = slt;
                repoInventory.Insert(rec);
                await LoadInventory(rec.Id);

                previousTab = tabControl1.SelectedTab;
                tabControl1.SelectedTab = tabPage1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save Error. " + ex.Message + ex.InnerException + ex.InnerException.InnerException);
            }
        }

        private async void UpdateViewEdit()
        {
            int id = ((SqlInventoryView) bindingSource.Current).Id;
            //int station = (ComboBoxViewEditStation.SelectedItem as Station).Id;
            //string slt = CreateSlot(station, TextBoxViewEditLoc1.Text, TextBoxViewEditLoc2.Text, TextBoxViewEditLoc3.Text, TextBoxViewEditLoc4.Text, TextBoxViewEditLoc5.Text);
            var rec = new Inventory
            {
                Id = id,
                LocationId = ((SqlInventoryView) bindingSource.Current).LocationId,
                ItemDefinitionId = ((SqlInventoryView) bindingSource.Current).ItemDefinitionId,
                //Quantity = int.Parse(TextBoxViewEditQuantity.Text)
                //StationId = station,
                //Loc1 = int.Parse(TextBoxViewEditLoc1.Text),
                //Loc2 = int.Parse(TextBoxViewEditLoc2.Text),
                //Loc3 = int.Parse(TextBoxViewEditLoc3.Text),
                //Loc4 = int.Parse(TextBoxViewEditLoc4.Text),
                //Loc5 = int.Parse(TextBoxViewEditLoc5.Text),
                //Slot = slt,
                //SizeCodeId = (ComboBoxViewEditSizeCode.SelectedItem as SizeCode).Id,
                //VelocityCodeId = (ComboBoxViewEditVelocityCode.SelectedItem as VelocityCode).Id,
                //HeightCodeId = (ComboBoxViewEditHeightCode.SelectedItem as HeightCode).Id
            };
            if (!ValidateFields(rec))
            {
                MessageBox.Show("Invalid Entry.");
                return;
            }
            //TextBoxViewEditSlot.Text = slt;
            repoInventory.Update(rec);
            await LoadInventory(rec.Id);

            previousTab = tabControl1.SelectedTab;
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

        private bool IsDuplicate(Inventory recIn)
        {
            Inventory rec = repoInventory.FindBy(f => f.LocationId == recIn.LocationId && f.ItemDefinitionId == recIn.ItemDefinitionId).FirstOrDefault();
            if (rec != null)
            {
                MessageBox.Show("Record already exists.", "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true;
            }
            return false;
        }

        private string CreateSlot(int station, string loc1, string loc2, string loc3, string loc4, string loc5)
        {
            var sb = new StringBuilder();
            sb.Append(station.ToString().PadLeft(2, '0'));
            sb.Append(loc1.PadLeft(2, '0'));
            sb.Append(loc2.PadLeft(2, '0'));
            sb.Append(loc3.PadLeft(2, '0'));
            sb.Append(loc4.PadLeft(2, '0'));
            sb.Append(loc5.PadLeft(2, '0'));
            return sb.ToString();
        }

        #region Form Setup Grids
        private void SetupGrids()
        {
            DataGridView1.AutoGenerateColumns = false;
            DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridView1.AllowUserToAddRows = false;

            var bCol = new DataGridViewButtonColumn();
            bCol.HeaderText = "";
            bCol.Visible = true;
            bCol.Name = "Position";
            bCol.Text = "Position";
            bCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            bCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            bCol.UseColumnTextForButtonValue = true;
            DataGridView1.Columns.Add(bCol);

            var col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "StationName";
            col.HeaderText = "Station";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.Name = "StationName";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "StorageTypeName";
            col.HeaderText = "Storage Type";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.Name = "StorageTypeName";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Item";
            col.HeaderText = "Item";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            col.Name = "Item";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Description";
            col.HeaderText = "Description";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            col.Name = "Description";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Quantity";
            col.HeaderText = "Quantity";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Quantity";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Slot";
            col.HeaderText = "Slot";
            col.Visible = true;
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            col.Name = "Slot";
            DataGridView1.Columns.Add(col);

            var xcol = new DataGridViewCheckBoxColumn();
            xcol.DataPropertyName = "PrimeBin";
            xcol.HeaderText = "Prime Bin";
            xcol.Visible = true;
            xcol.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            xcol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            xcol.Name = "PrimeBin";
            DataGridView1.Columns.Add(xcol);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Loc1";
            col.HeaderText = "Device";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Loc1";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Loc2";
            col.HeaderText = "Tray";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Loc2";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Loc3";
            col.HeaderText = "Over";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Loc3";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Loc4";
            col.HeaderText = "Back";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Loc4";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Loc5";
            col.HeaderText = "Tag";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Loc5";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "SizeCodeName";
            col.HeaderText = "Size Code";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "SizeCodeName";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "VelocityCodeName";
            col.HeaderText = "Velocity Code";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "VelocityCodeName";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "HeightCodeName";
            col.HeaderText = "Height Code";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            col.Name = "HeightCodeName";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "LocationCodeName";
            col.HeaderText = "User Code";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            col.Name = "LocationCodeName";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "LocationCodeName";
            col.HeaderText = "User Code";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            col.Name = "LocationCodeName";
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Id";
            col.HeaderText = "Id";
            col.Visible = false;
            col.Name = "Id";
            DataGridView1.Columns.Add(col);

            //Location Grid

            DataGridViewInventoryLocations.AutoGenerateColumns = false;
            DataGridViewInventoryLocations.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewInventoryLocations.AllowUserToAddRows = false;

            bCol = new DataGridViewButtonColumn();
            bCol.HeaderText = "";
            bCol.Visible = true;
            bCol.Name = "Position";
            bCol.Text = "Position";
            bCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            bCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            bCol.UseColumnTextForButtonValue = true;
            DataGridViewInventoryLocations.Columns.Add(bCol);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "StorageTypeName";
            col.HeaderText = "Storage Type";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.Name = "StorageTypeName";
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Quantity";
            col.HeaderText = "Quantity";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Quantity";
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Slot";
            col.HeaderText = "Slot";
            col.Visible = true;
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            col.Name = "Slot";
            DataGridViewInventoryLocations.Columns.Add(col);

            xcol = new DataGridViewCheckBoxColumn();
            xcol.DataPropertyName = "PrimeBin";
            xcol.HeaderText = "Prime Bin";
            xcol.Visible = true;
            xcol.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            xcol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            xcol.Name = "PrimeBin";
            DataGridViewInventoryLocations.Columns.Add(xcol);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "StationName";
            col.HeaderText = "Station";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            col.Name = "StationName";
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Loc1";
            col.HeaderText = "Device";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Loc1";
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Loc2";
            col.HeaderText = "Tray";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Loc2";
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Loc3";
            col.HeaderText = "Over";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Loc3";
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Loc4";
            col.HeaderText = "Back";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Loc4";
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Loc5";
            col.HeaderText = "Tag";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Loc5";
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "SizeCodeName";
            col.HeaderText = "Size Code";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            col.Name = "SizeCodeName";
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "VelocityCodeName";
            col.HeaderText = "Velocity Code";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            col.Name = "VelocityCodeName";
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "HeightCodeName";
            col.HeaderText = "Height Code";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            col.Name = "HeightCodeName";
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "LocationCodeName";
            col.HeaderText = "User Code";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            col.Name = "LocationCodeName";
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "ReceivedDate";
            col.HeaderText = "Received Date";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.Name = "ReceivedDate";
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Id";
            col.HeaderText = "Id";
            col.Visible = false;
            col.Name = "Id";
            DataGridViewInventoryLocations.Columns.Add(col);

            //New Location Grid

            DataGridViewInventoryNewLocations.AutoGenerateColumns = false;
            DataGridViewInventoryNewLocations.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewInventoryNewLocations.AllowUserToAddRows = false;

            bCol = new DataGridViewButtonColumn();
            bCol.HeaderText = "";
            bCol.Visible = true;
            bCol.Name = "Position";
            bCol.Text = "Position";
            bCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            bCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            bCol.UseColumnTextForButtonValue = true;
            DataGridViewInventoryNewLocations.Columns.Add(bCol);

            xcol = new DataGridViewCheckBoxColumn();
            xcol.DataPropertyName = "InUse";
            xcol.HeaderText = "In Use";
            xcol.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            xcol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            xcol.Name = "InUse";
            DataGridViewInventoryNewLocations.Columns.Add(xcol);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Slot";
            col.HeaderText = "Slot";
            col.Visible = true;
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            col.Name = "Slot";
            DataGridViewInventoryNewLocations.Columns.Add(col);

            xcol = new DataGridViewCheckBoxColumn();
            xcol.DataPropertyName = "PrimeBin";
            xcol.HeaderText = "Prime Bin";
            xcol.Visible = true;
            xcol.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            xcol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            xcol.Name = "PrimeBin";
            DataGridViewInventoryNewLocations.Columns.Add(xcol);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "StationName";
            col.HeaderText = "Station";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            col.Name = "StationName";
            col.ReadOnly = true;
            DataGridViewInventoryNewLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Loc1";
            col.HeaderText = "Device";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Loc1";
            col.ReadOnly = true;
            DataGridViewInventoryNewLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Loc2";
            col.HeaderText = "Tray";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Loc2";
            col.ReadOnly = true;
            DataGridViewInventoryNewLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Loc3";
            col.HeaderText = "Over";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Loc3";
            col.ReadOnly = true;
            DataGridViewInventoryNewLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Loc4";
            col.HeaderText = "Back";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Loc4";
            col.ReadOnly = true;
            DataGridViewInventoryNewLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Loc5";
            col.HeaderText = "Tag";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.Name = "Loc5";
            col.ReadOnly = true;
            DataGridViewInventoryNewLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "SizeCodeName";
            col.HeaderText = "Size Code";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            col.Name = "SizeCodeName";
            col.ReadOnly = true;
            DataGridViewInventoryNewLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "VelocityCodeName";
            col.HeaderText = "Velocity Code";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            col.Name = "VelocityCodeName";
            col.ReadOnly = true;
            DataGridViewInventoryNewLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "HeightCodeName";
            col.HeaderText = "Height Code";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            col.Name = "HeightCodeName";
            col.ReadOnly = true;
            DataGridViewInventoryNewLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "LocationCodeName";
            col.HeaderText = "User Code";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            col.Name = "LocationCodeName";
            DataGridViewInventoryNewLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "ReceivedDate";
            col.HeaderText = "Received Date";
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.Name = "ReceivedDate";
            DataGridViewInventoryNewLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Id";
            col.HeaderText = "Id";
            col.Visible = false;
            col.Name = "Id";
            DataGridViewInventoryNewLocations.Columns.Add(col);
        }


        private void DataGridViewInventoryLocations_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var grid = (DataGridView) sender;
            if (grid.CurrentCell.RowIndex < 0) return;
            if (grid.CurrentCell.ColumnIndex == grid.Columns["Position"].Index)
            {
                int loc1 = grid["Loc1", e.RowIndex].Value.ToString().ParseInt();
                int loc2 = grid["Loc2", e.RowIndex].Value.ToString().ParseInt();
                Task.Run(() => Variables.Shuttle.PositionDevice(loc1, loc2));
            }
        }

        private void DataGridViewInventoryAddDetailLocations_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //if button cell position device, otherwise open View/Edit
            var grid = (DataGridView) sender;
            if (e.RowIndex < 0) return;
            if (e.ColumnIndex == grid.Columns["Position"].Index)
            {
                int loc1 = grid["Loc1", e.RowIndex].Value.ToString().ParseInt();
                int loc2 = grid["Loc2", e.RowIndex].Value.ToString().ParseInt();
                Task.Run(() => Variables.Shuttle.PositionDevice(loc1, loc2));
            }
            else
            {
                //LocationView location = ((LocationView) newLocationBindingSource.Current);

                //if (location.Id != 0)
                //{
                //    TextBoxAddDetailLocationId.Text = location.Id.ToString();
                //    TextBoxAddDetailItemDefinitionId.Text = CurrentItem.Id.ToString();
                //    ComboBoxAddDetailStation.SelectedValue = location.StationId;
                //    TextBoxAddDetailLoc1.Text = location.Loc1.ToString();
                //    TextBoxAddDetailLoc2.Text = location.Loc2.ToString();
                //    TextBoxAddDetailLoc3.Text = location.Loc3.ToString();
                //    TextBoxAddDetailLoc4.Text = location.Loc4.ToString();
                //    TextBoxAddDetailLoc5.Text = location.Loc5.ToString();
                //    TextBoxAddDetailSlot.Text = location.Slot.ToString();
                //    ComboBoxAddDetailStorageType.SelectedValue = CurrentItem.StorageTypeId;
                //    ComboBoxAddDetailSizeCode.SelectedValue = location.SizeCodeId;
                //    ComboBoxAddDetailVelocityCode.SelectedValue = location.VelocityCodeId;
                //    ComboBoxAddDetailHeightCode.SelectedValue = location.HeightCodeId;
                //    ComboBoxAddDetailLocationCode.SelectedValue = location.LocationCodeId;
                //    LabelActionAddDetail.Text = "Add Detail";
                //    TextBoxAddDetailQuantity.Focus();
                //}
                //else
                //{
                //    MessageBox.Show("No Available locations.", "Locations", MessageBoxButtons.OK);
                //}
            }
        }

        private void SetupNewForm()
        {
            LabelFindDescription.Text = "Search any part of the Item or Description field";

            ComboBoxNewSizeCode.DataSource = repoSizeCode.All();
            ComboBoxNewSizeCode.DisplayMember = "Name";
            ComboBoxNewSizeCode.ValueMember = "Id";

            ComboBoxNewVelocityCode.DataSource = repoVelocityCode.All();
            ComboBoxNewVelocityCode.DisplayMember = "Name";
            ComboBoxNewVelocityCode.ValueMember = "Id";

            ComboBoxNewHeightCode.DataSource = repoHeightCode.All();
            ComboBoxNewHeightCode.DisplayMember = "Name";
            ComboBoxNewHeightCode.ValueMember = "Id";

            ComboBoxNewLocationCode.DataSource = repoLocationCode.All();
            ComboBoxNewLocationCode.DisplayMember = "Name";
            ComboBoxNewLocationCode.ValueMember = "Id";

            ComboBoxNewStation.DataSource = repoStation.All();
            ComboBoxNewStation.DisplayMember = "Name";
            ComboBoxNewStation.ValueMember = "Id";

            ComboBoxNewStorageType.DataSource = repoStorageType.All();
            ComboBoxNewStorageType.DisplayMember = "Name";
            ComboBoxNewStorageType.ValueMember = "Id";

            ComboBoxInventoryNewLocationsStorageType.DataSource = repoStorageType.All();
            ComboBoxInventoryNewLocationsStorageType.DisplayMember = "Name";
            ComboBoxInventoryNewLocationsStorageType.ValueMember = "Id";

            ComboBoxNewUnitOfIssue.DataSource = repoUnitOfIssue.All();
            ComboBoxNewUnitOfIssue.DisplayMember = "Name";
            ComboBoxNewUnitOfIssue.ValueMember = "Id";
        }

        private void SetupAddDetailForm()
        {
            ComboBoxAddDetailSizeCode.DataSource = repoSizeCode.All();
            ComboBoxAddDetailSizeCode.DisplayMember = "Name";
            ComboBoxAddDetailSizeCode.ValueMember = "Id";

            ComboBoxAddDetailVelocityCode.DataSource = repoVelocityCode.All();
            ComboBoxAddDetailVelocityCode.DisplayMember = "Name";
            ComboBoxAddDetailVelocityCode.ValueMember = "Id";

            ComboBoxAddDetailHeightCode.DataSource = repoHeightCode.All();
            ComboBoxAddDetailHeightCode.DisplayMember = "Name";
            ComboBoxAddDetailHeightCode.ValueMember = "Id";

            ComboBoxAddDetailLocationCode.DataSource = repoLocationCode.All();
            ComboBoxAddDetailLocationCode.DisplayMember = "Name";
            ComboBoxAddDetailLocationCode.ValueMember = "Id";

            ComboBoxAddDetailStation.DataSource = repoStation.All();
            ComboBoxAddDetailStation.DisplayMember = "Name";
            ComboBoxAddDetailStation.ValueMember = "Id";

            ComboBoxAddDetailStorageType.DataSource = repoStorageType.All();
            ComboBoxAddDetailStorageType.DisplayMember = "Name";
            ComboBoxAddDetailStorageType.ValueMember = "Id";
        }

        private void SetupViewEditForm()
        {
            //LabelFindDescription.Text = "Search any part of the Item or Description field";

            //ComboBoxViewEditSizeCode.DataSource = repoSizeCode.All();
            //ComboBoxViewEditSizeCode.DisplayMember = "Name";
            //ComboBoxViewEditSizeCode.ValueMember = "Id";

            //ComboBoxViewEditVelocityCode.DataSource = repoVelocityCode.All();
            //ComboBoxViewEditVelocityCode.DisplayMember = "Name";
            //ComboBoxViewEditVelocityCode.ValueMember = "Id";

            //ComboBoxViewEditHeightCode.DataSource = repoHeightCode.All();
            //ComboBoxViewEditHeightCode.DisplayMember = "Name";
            //ComboBoxViewEditHeightCode.ValueMember = "Id";

            //ComboBoxViewEditStation.DataSource = repoStation.All();
            //ComboBoxViewEditStation.DisplayMember = "Name";
            //ComboBoxViewEditStation.ValueMember = "Id";
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
            int inventoryId = ((SqlInventoryView) bindingSource.Current).Id;
            Inventory inventory = repoInventory.FindByKey(inventoryId);
            inventory.StorageTypeId = (int) Enums.StorageType.Inactive;
            repoInventory.Update(inventory);

            locationsRepository.SetLocationInUse(inventory.LocationId, false);

            LoadViewEdit();
        }

        private void MbViewEditAddLocation_Click(object sender, EventArgs e)
        {
            AddLocation();
        }

        private void AddLocation()
        {
            int id = CurrentItem.Id;
            ItemDefinition itemDefinition = repoItemDefinition.FindByKey(id);
            if (itemDefinition != null)
            {
                TextBoxInventoryNewLocationsItem.Text = itemDefinition.Item;
                TextBoxInventoryNewLocationsDescription.Text = itemDefinition.Description;
                TextBoxNewLocationsItemDefinitionId.Text = itemDefinition.Id.ToString();
                GetAvailableLocations(itemDefinition);
            }

            previousTab = tabControl1.SelectedTab;
            tabControl1.SelectedTab = tabPage4;
        }

        public async void GetAvailableLocations(ItemDefinition itemDefinition, int recId = 0)
        {
            IEnumerable<LocationView> views = new List<LocationView>();
            int idx = recId;
            if (itemDefinition == null)
            {
                return;
            }
            bool inUse = false;

            views = await Task.Run(() => locationsRepository.GetAllLocationViewsExact(itemDefinition.StationId, itemDefinition.SizeCodeId, itemDefinition.VelocityCodeId, itemDefinition.HeightCodeId, itemDefinition.LocationCodeId, inUse));

            newLocationBindingSource.DataSource = views;
            DataGridViewInventoryNewLocations.DataSource = newLocationBindingSource;

            if (GetRecordCount(newLocationBindingSource) > 0)
            {
                if (recId != 0)
                {
                    idx = IndexOf(newLocationBindingSource, recId);
                }
                DataGridViewInventoryNewLocations.FirstDisplayedScrollingRowIndex = DataGridViewInventoryNewLocations.Rows[idx].Index;
                DataGridViewInventoryNewLocations.Refresh();
                DataGridViewInventoryNewLocations.CurrentCell = DataGridViewInventoryNewLocations.Rows[idx].Cells[1];
                DataGridViewInventoryNewLocations.Rows[idx].Selected = true;
            }
        }

        public async void GetAllAvailableLocations(int recId = 0)
        {
            int idx = recId;
            IEnumerable<LocationView> views = new List<LocationView>();
            IEnumerable<LocationView> recs = new List<LocationView>();

            views = await Task.Run(() => locationsRepository.GetAllLocationViews());

            newLocationBindingSource.DataSource = views;
            DataGridViewInventoryNewLocations.DataSource = newLocationBindingSource;

            if (GetRecordCount(newLocationBindingSource) > 0)
            {
                if (recId != 0)
                {
                    idx = IndexOf(newLocationBindingSource, recId);
                }
                DataGridViewInventoryNewLocations.FirstDisplayedScrollingRowIndex = DataGridViewInventoryNewLocations.Rows[idx].Index;
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
            string item = TextBoxNewItem.Text;
            try
            {
                ItemDefinition rec = repoItemDefinition.FindBy(f => f.Item == item).FirstOrDefault();

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
                    TextBoxNewWeight.Text = rec.Weight.ToString();
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
            int id = (TextBoxNewId.Text).ParseInt();
            CurrentItem = repoItemDefinition.FindByKey(id);
            if (CurrentItem != null)
            {
                TextBoxInventoryNewLocationsItem.Text = CurrentItem.Item;
                TextBoxInventoryNewLocationsDescription.Text = CurrentItem.Description;
                GetAvailableLocations(CurrentItem);
            }

            previousTab = tabControl1.SelectedTab;
            tabControl1.SelectedTab = tabPage4;
        }

        private void MbNewLocationsClose_Click(object sender, EventArgs e)
        {
            Back();
        }

        private async void MbNewLocationsListing_Click(object sender, EventArgs e)
        {
            await LoadInventory();
            previousTab = tabControl1.SelectedTab;
            tabControl1.SelectedTab = tabPage1;
        }

        private void MbNewLocationsSave_Click(object sender, EventArgs e)
        {
            var locView = (LocationView) newLocationBindingSource.Current;

            if (locView != null)
            {
                var inventory = new Inventory()
                {
                    ItemDefinitionId = CurrentItem.Id,
                    LocationId = locView.Id,
                    Quantity = TextBoxInventoryNewLocationsQuantity.Text.ParseInt(),
                    ReceivedDate = DateTimePickerInventoryNewLocationsReceivedDate.Value,
                    StorageTypeId = ComboBoxInventoryNewLocationsStorageType.SelectedValue.ToString().ParseInt(),
                    PrimeBin = CheckBoxInventoryNewLocationsPrimeBin.Checked
                };
                repoInventory.Insert(inventory);
                //locationsRepository.SetLocationInUse(inventory.LocationId, true);
                if (inventory.StorageTypeId == (int) Enums.StorageType.Inactive)
                {
                    locationsRepository.SetLocationInUse(inventory.LocationId, false);
                }
                else
                {
                    locationsRepository.SetLocationInUse(inventory.LocationId, true);
                }
            }
            AddLocation();

            //TextBoxViewEditId.Text = CurrentItem.Id.ToString();
            //TextBoxViewEditItem.Text = CurrentItem.Item;
            //TextBoxViewEditDescription.Text = CurrentItem.Description;
            //locationBindingSource.DataSource = GetInventoryViewListByItem(CurrentItem.Id);
            //DataGridViewInventoryLocations.DataSource = locationBindingSource;
            //previousTab = tabControl1.SelectedTab;
            //tabControl1.SelectedTab = tabPage2;
        }

        private IEnumerable<SqlInventoryView> GetInventoryViewListByItem(int itemId)
        {
            IEnumerable<SqlInventoryView> projection = new List<SqlInventoryView>();
            using (var context = new NeutronDb())
            {
                var findItemId = new SqlParameter("@ItemId", itemId);
                projection = context.Database.SqlQuery<SqlInventoryView>("usp_GetInventoryViewByItem @ItemId", findItemId).ToList();

            }
            return projection;
        }

        private IEnumerable<SqlInventoryView> GetInventoryViewList()
        {
            IEnumerable<SqlInventoryView> projection = new List<SqlInventoryView>();

            using (var context = new NeutronDb())
            {
                projection = context.Database.SqlQuery<SqlInventoryView>("usp_GetInventoryView").ToList();
            }
            return projection;
        }



        //private IEnumerable<LocationView> GetLocationViewLocations(int station, int sizeCode, int velocityCode
        //    , int heightCode, int locationCode)
        //{
        //    IEnumerable<LocationView> projection = new List<LocationView>();

        //    //List<Location> recs = repoLocation.AllInclude(h => h.Station, h => h.SizeCode, h => h.VelocityCode
        //    //    , h => h.HeightCode).Where(r => r.SizeCodeId == sizeCode && r.VelocityCodeId == velocityCode
        //    //    && r.HeightCodeId == heightCode).ToList();

        //    //List<Location> recs = repoLocation.All().Where(r => r.SizeCodeId == sizeCode && r.VelocityCodeId == velocityCode
        //    //   && r.HeightCodeId == heightCode).ToList();

        //    IEnumerable<Location> recs = GetAvailableLocationsByCodes(station, sizeCode, velocityCode, heightCode, locationCode);

        //    if (recs.Count() > 0)
        //    {
        //        projection = recs.Select(r => new LocationView
        //        {
        //            Id = r.Id,
        //            StationId = r.StationId,
        //            StationName = r.Station.Name,
        //            Loc1 = r.Loc1,
        //            Loc2 = r.Loc2,
        //            Loc3 = r.Loc3,
        //            Loc4 = r.Loc4,
        //            Loc5 = r.Loc5,
        //            Slot = r.Slot,
        //            InUse = r.InUse,
        //            SizeCodeId = r.SizeCodeId,
        //            VelocityCodeId = r.VelocityCodeId,
        //            HeightCodeId = r.HeightCodeId,
        //            LocationCodeId = r.LocationCodeId,
        //            SizeCodeName = r.SizeCode.Name,
        //            VelocityCodeName = r.VelocityCode.Name,
        //            HeightCodeName = r.HeightCode.Name,
        //            LocationCodeName = r.LocationCode.Name,
        //        }).OrderBy(o => o.StationId)
        //                        .ThenBy(o => o.Loc1)
        //                        .ThenBy(o => o.Loc2)
        //                        .ThenBy(o => o.Loc3)
        //                        .ThenBy(o => o.Loc4)
        //                        .ThenBy(o => o.Loc5)
        //                        .ThenBy(o => o.SizeCodeName)
        //                        .ThenBy(o => o.VelocityCodeName)
        //                        .ThenBy(o => o.HeightCodeName)
        //                        .ToList();
        //    }
        //    return projection;
        //}

        //private IEnumerable<LocationView> GetLocationViewLocationsAll()
        //{
        //    IEnumerable<LocationView> projection = new List<LocationView>();

        //    //List<Location> recs = repoLocation.AllInclude(h => h.Station, h => h.SizeCode, h => h.VelocityCode
        //    //    , h => h.HeightCode).Where(r => r.SizeCodeId == sizeCode && r.VelocityCodeId == velocityCode
        //    //    && r.HeightCodeId == heightCode).ToList();

        //    //List<Location> recs = repoLocation.All().Where(r => r.SizeCodeId == sizeCode && r.VelocityCodeId == velocityCode
        //    //   && r.HeightCodeId == heightCode).ToList();

        //    IEnumerable<Location> recs = GetAvailableLocationsAll();

        //    if (recs.Count() > 0)
        //    {
        //        projection = recs.Select(r => new LocationView
        //        {
        //            Id = r.Id,
        //            StationId = r.StationId,
        //            StationName = r.Station.Name,
        //            Loc1 = r.Loc1,
        //            Loc2 = r.Loc2,
        //            Loc3 = r.Loc3,
        //            Loc4 = r.Loc4,
        //            Loc5 = r.Loc5,
        //            Slot = r.Slot,
        //            InUse = r.InUse,
        //            SizeCodeId = r.SizeCodeId,
        //            VelocityCodeId = r.VelocityCodeId,
        //            HeightCodeId = r.HeightCodeId,
        //            LocationCodeId = r.LocationCodeId,
        //            SizeCodeName = r.SizeCode.Name,
        //            VelocityCodeName = r.VelocityCode.Name,
        //            HeightCodeName = r.HeightCode.Name,
        //            LocationCodeName = r.LocationCode.Name
        //        }).ToList();
        //        //}).OrderBy(o => o.StationId)
        //        //                .ThenBy(o => o.Loc1)
        //        //                .ThenBy(o => o.Loc2)
        //        //                .ThenBy(o => o.Loc3)
        //        //                .ThenBy(o => o.Loc4)
        //        //                .ThenBy(o => o.Loc5)
        //        //                .ThenBy(o => o.SizeCodeName)
        //        //                .ThenBy(o => o.VelocityCodeName)
        //        //                .ThenBy(o => o.HeightCodeName)
        //        //                .ToList();
        //    }
        //    return projection;
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
            string inventoryId = TextBoxAddDetailInventoryId.Text;
            if (!string.IsNullOrEmpty(inventoryId))
            {
                //update
                inventory = repoInventory.FindByKey(inventoryId.ParseInt());
                inventory.Quantity = (TextBoxAddDetailQuantity.Text).ParseInt();
                inventory.StorageTypeId = (ComboBoxAddDetailStorageType.SelectedItem as NeutronData.Models.Lookups.StorageType).Id;
                inventory.ReceivedDate = DateTimePickerAddDetailReceivedDate.Value;
                inventory.PrimeBin = CheckBoxAddDetailPrimeBin.Checked;
                repoInventory.Update(inventory);

            }
            else
            {
                //new
                inventory.ItemDefinitionId = (TextBoxAddDetailItemDefinitionId.Text).ParseInt();
                inventory.LocationId = (TextBoxAddDetailLocationId.Text).ParseInt();
                inventory.Quantity = (TextBoxAddDetailQuantity.Text).ParseInt();
                inventory.StorageTypeId = (ComboBoxAddDetailStorageType.SelectedItem as NeutronData.Models.Lookups.StorageType).Id;
                inventory.ReceivedDate = DateTimePickerAddDetailReceivedDate.Value;
                inventory.PrimeBin = CheckBoxAddDetailPrimeBin.Checked;
                repoInventory.Insert(inventory);
            }
            if (inventory.StorageTypeId == (int) Enums.StorageType.Inactive)
            {
                locationsRepository.SetLocationInUse(inventory.LocationId, b: false);
            }
            else
            {
                locationsRepository.SetLocationInUse(inventory.LocationId, b: true);
            }

            TextBoxViewEditId.Text = CurrentItem.Id.ToString();
            TextBoxViewEditItem.Text = CurrentItem.Item;
            TextBoxViewEditDescription.Text = CurrentItem.Description;
            locationBindingSource.DataSource = GetInventoryViewListByItem(CurrentItem.Id);
            DataGridViewInventoryLocations.DataSource = locationBindingSource;
            previousTab = tabControl1.SelectedTab;
            tabControl1.SelectedTab = tabPage2;
        }



        private async void MbAddDetailClose_Click(object sender, EventArgs e)
        {
            await LoadInventory();
            tabControl1.SelectedTab = tabPage1;
        }

        private async void MbAddDetailListing_Click(object sender, EventArgs e)
        {
            await LoadInventory();
            previousTab = tabControl1.SelectedTab;
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
            get
            {
                return allAvailable;
            }
            set
            {
                allAvailable = value;
                MbNewAvailableLocations.Text = allAvailable ? "Available (Match)" : "Available (All)";
            }
        }

        private void MbNewAvailableLocations_Click(object sender, EventArgs e)
        {
            AllAvailable = !allAvailable;
            if (allAvailable)
            {
                GetAllAvailableLocations();
            }
            else
            {
                GetAvailableLocations(CurrentItem);
            }
        }


        //public IEnumerable<Location> GetAvailableLocations()
        //{
        //    IEnumerable<Location> recs = new List<Location>();
        //    using (var db = new NeutronDb())
        //    {
        //        recs = db.Locations.Where(c => !db.Inventory.Select(r => r.LocationId).Contains(c.Id)).ToList();
        //    }
        //    return recs;
        //}

        //public IEnumerable<Location> GetAvailableLocationsByCodes(int station, int size, int vel, int height, int loc)
        //{
        //    List<SizeCode> sizeCodes = repoSizeCode.All().OrderBy(o => o.Sequence).ToList();
        //    int index = sizeCodes.FindIndex(s => s.Id == size);
        //    SizeCode rec = sizeCodes[index];
        //    string nextRec = sizeCodes[index].Name;
        //    // rec = sizeCodes[index + 1];
        //    // nextRec = sizeCodes[index + 1].Name;

        //    IEnumerable<Location> recs = new List<Location>();
        //    for (int i = index; i < sizeCodes.Count; i++)
        //    {
        //        int sizeCodeId = sizeCodes[i].Id;
        //        using (var db = new NeutronDb())
        //        {
        //            recs = db.Locations.Include(h => h.Station).Include(h => h.SizeCode).Include(h => h.VelocityCode)
        //                .Include(h => h.HeightCode).Include(l => l.LocationCode).Where(c => !db.Inventory.Select(r => r.LocationId).Contains(c.Id)
        //            && c.StationId == station && c.SizeCodeId == sizeCodeId && c.VelocityCodeId == vel && c.HeightCodeId == height
        //            && c.LocationCodeId == loc)
        //            .OrderBy(o => o.StationId).ThenBy(o => o.SizeCodeId).ThenBy(o => o.VelocityCodeId).ThenBy(o => o.HeightCodeId)
        //            .ToList();

        //        }
        //        if (recs.Count() > 0)
        //        {
        //            return recs;
        //        }
        //    }
        //    return recs;
        //}

        //public IEnumerable<Location> GetAvailableLocationsAll()
        //{
        //    IEnumerable<Location> recs = new List<Location>();
        //    try
        //    {
        //        recs = locationsRepository.AvailableLocationsAll();

        //        //using (var db = new NeutronDb())
        //        //{
        //        //    recs = db.Locations.Include(h => h.Station).Include(h => h.SizeCode).Include(h => h.VelocityCode)
        //        //        .Include(h => h.HeightCode).Include(l => l.LocationCode).Where(c => !db.Inventory.Select(r => r.LocationId).Contains(c.Id))
        //        //        .ToList();
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Get Available Locations All Error.  " + ex.Message + "  " + ex.InnerException);
        //    }

        //    return recs;
        //}

        private void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            //if button cell position device, otherwise open View/Edit
            var grid = (DataGridView) sender;
            if (e.RowIndex <= 0) return;
            else
            {
                SetCurrentItemDefinition();
                // CurrentItem = ((InventoryView) bindingSource.Current).ItemDefinition;
                TextBoxViewEditId.Text = CurrentItem.Id.ToString();
                TextBoxViewEditItem.Text = CurrentItem.Item;
                TextBoxViewEditDescription.Text = CurrentItem.Description;
                locationBindingSource.DataSource = (IEnumerable<SqlInventoryView>) GetInventoryViewListByItem(CurrentItem.Id);
                DataGridViewInventoryLocations.DataSource = locationBindingSource;
                GetRecordCount(locationBindingSource);
                previousTab = tabControl1.SelectedTab;
                tabControl1.SelectedTab = tabPage2;
            }
        }

        private void Back()
        {
            tabControl1.SelectedTab = previousTab;
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var grid = (DataGridView) sender;
            if (e.RowIndex < 0) return;
            if (e.ColumnIndex == grid.Columns["Position"].Index)
            {
                int loc1 = grid["Loc1", e.RowIndex].Value.ToString().ParseInt();
                int loc2 = grid["Loc2", e.RowIndex].Value.ToString().ParseInt();
                Task.Run(() => Variables.Shuttle.PositionDevice(loc1, loc2));
            }
        }

        private void MBPrintInventory_Click(object sender, EventArgs e)
        {
            CsvUtility.SaveToCsv(DataGridView1);
        }
    }
}
