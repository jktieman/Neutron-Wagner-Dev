using Equin.ApplicationFramework;
using MetroFramework.Forms;
using Neutron.Global;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NeutronData.ModelViews;
using NeutronData.Models.Lookups;
using Neutron.Classes;
using JsonManager;
using System.Globalization;
using System.Resources;
using System.Threading;
using AlliedLogger;
using Neutron.Interfaces;
using NeutronCore.Extensions;
using NeutronCore;
using NeutronCore.Enums;
using NeutronData.Interfaces;
using StorageType = NeutronData.Models.Lookups.StorageType;
using ExcelManager;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;
using Neutron.Extensions;
using NeutronEvents;

namespace Neutron.Forms
{
    public partial class FrmItemDefinitions : MetroForm
    {
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;
        private ResourceManager _gridResourceManager;
        private BindingSource _bindingSource = new BindingSource();
        private BindingSource _akaBindingSource = new BindingSource();
        private BindingSource _existingItemsBindingSource = new BindingSource();


        private readonly ItemDefinitionsRepository _itemDefinitionsRepository = new ItemDefinitionsRepository();
        private readonly GenericRepository<SizeCode> _repoSizeCode = new GenericRepository<SizeCode>(new NeutronDb());
        private readonly GenericRepository<VelocityCode> _repoVelocityCode = new GenericRepository<VelocityCode>(new NeutronDb());
        private readonly GenericRepository<HeightCode> _repoHeightCode = new GenericRepository<HeightCode>(new NeutronDb());
        private readonly GenericRepository<Area> _repoArea = new GenericRepository<Area>(new NeutronDb());

        private readonly GenericRepository<ItemDefinition> _repoItemDefinition = new GenericRepository<ItemDefinition>(new NeutronDb());
        private readonly GenericRepository<StorageType> _repoStorageType = new GenericRepository<StorageType>(new NeutronDb());
        private readonly GenericRepository<UnitOfIssue> _repoUnitOfIssue = new GenericRepository<UnitOfIssue>(new NeutronDb());
        private readonly GenericRepository<Inventory> _repoInventory = new GenericRepository<Inventory>(new NeutronDb());
        private readonly GenericRepository<OrderDetail> _repoOrderDetails = new GenericRepository<OrderDetail>(new NeutronDb());
        private IDynamicLogger _logger;
        readonly IJsonData _jsonData;
        private readonly WorkstationView _workstation;
        private readonly IAkaRepository _akaRepository;
        private readonly IImageManager _imageManager;
        private readonly IAreaRepository _areaRepository;
        private readonly IHistoryManager _historyManager;
        private List<ItemDefinitionView> _currentList;
        private readonly IWorkstationRepository _workstationRepository;

        public bool CloseButtonPressed { get; set; }
        private BackgroundWorker _dgvColumnWidthSizer;
        private bool _startup = true;
        // private readonly List<Workstation> _pickStations;

        public FrmItemDefinitions(IWorkstationRepository workstationRepository, IJsonData jsonData, WorkstationView workstation
            , IAkaRepository akaRepository, IImageManager imageManager
            , IAreaRepository areaRepository, IHistoryManager historyManager)
        {
            InitializeComponent();
            _workstationRepository = workstationRepository;
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            _dgvColumnWidthSizer = new BackgroundWorker();
            _dgvColumnWidthSizer.DoWork += DgvColumnWidthSizerOnDoWork;
            _dgvColumnWidthSizer.RunWorkerCompleted += DgvColumnWidthSizerOnRunWorkerCompleted;
            KeyPreview = true;
            _workstation = workstation;
            _logger = NeutronCore.Global.Logger.SetupLogger("ItemDefinitions");
            //CreateLog();
            _jsonData = jsonData;
            CloseButtonPressed = false;
            SetupGrid();
            SetupTabControl();

            _akaRepository = akaRepository;
            _imageManager = imageManager;
            _areaRepository = areaRepository;
            _historyManager = historyManager;
            mlUserInfo.Text = GlobalVar.User?.UserInfo;
            LabelStationName.Text = _workstation.ToString();

            SetupViewEditBindings();



            var areas = _repoArea.All();
            ComboBoxAreaNumber.DataSource = areas;
            ComboBoxAreaNumber.ValueMember = "Id";
            ComboBoxAreaNumber.DisplayMember = "Name";
            ComboBoxAreaNumber.SelectedIndex = 0;

            if (_workstation.StationType.Id == (int)NeutronCore.Enums.StationType.Supervisor)
            {
                ComboBoxAreaNumber.SelectedIndex = ComboBoxAreaNumber.FindStringExact("All Areas");
            }
            else
            {
                ComboBoxAreaNumber.SelectedValue = _workstation.AreaId;
            }

            SetupNewForm();
            SetupViewEditForm();

            _startup = false;
            RefreshData();

        }

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

        private void DgvColumnWidthSizerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SetAutoSizeColumnsWidth(DataGridView1, (int[])e.Result);
        }
        private void DgvColumnWidthSizerOnDoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = GetAutoSizeColumnsWidth(DataGridView1);
        }
        private int[] GetAutoSizeColumnsWidth(DataGridView grid)
        {
            var src = ((IEnumerable)grid.DataSource)
                .Cast<object>()
                .Select(x => x.GetType()
                    .GetProperties()
                    .Select(p => p.GetValue(x, null)?.ToString() ?? string.Empty)
                    .ToArray()
                );
            int[] widths = new int[grid.Columns.Count];
            // Iterate through the columns.
            for (int i = 0; i < grid.Columns.Count; i++)
            {
                // Leverage Linq enumerator to rapidly collect all the rows into a string array, making sure to exclude null values.
                string[] colStringCollection = src.Where(r => r[i] != null).Select(r => r[i].ToString()).ToArray();
                // Sort the string array by string lengths.
                colStringCollection = colStringCollection.OrderBy((x) => x.Length).ToArray();
                // Get the last and longest string in the array.
                string longestColString = colStringCollection.Last();
                // Use the graphics object to measure the string size.
                var colWidth = TextRenderer.MeasureText(longestColString, grid.Font);
                // If the calculated width is larger than the column header width, set the new column width.
                if (colWidth.Width > grid.Columns[i].HeaderCell.Size.Width)
                {
                    widths[i] = (int)colWidth.Width;
                }
                else // Otherwise, set the column width to the header width.
                {
                    widths[i] = grid.Columns[i].HeaderCell.Size.Width;
                }
            }
            return widths;
        }
        public void SetAutoSizeColumnsWidth(DataGridView grid, int[] widths)
        {
            for (int i = 0; i < grid.Columns.Count; i++)
            {
                grid.Columns[i].Width = widths[i];
            }
        }
        private void RefreshData(int recId = 0)
        {

            Cursor.Current = Cursors.WaitCursor;
            var idx = 0;
            var aka = string.Empty;
            var findWhat = TextBoxFind.Text.ToLower().Trim();
            if (!string.IsNullOrEmpty(findWhat))
            {
                aka = _akaRepository.Get(findWhat);
            }
            try
            {
                var find = string.IsNullOrWhiteSpace(aka) ? findWhat : aka;
                TextBoxFind.Text = find;

                var area = (Area)ComboBoxAreaNumber.SelectedItem;

                var views = area.Name == "All Areas" 
                    ? _itemDefinitionsRepository.FindItemDefinitionViews(find).ToList()
                    : _itemDefinitionsRepository.FindItemDefinitionViewsByArea(find, area.Id).ToList();

                _currentList = views;

                var blv = new BindingListView<ItemDefinitionView>(views.ToList());
                _bindingSource.DataSource = blv;

                DataGridView1.DataSource = _bindingSource;
                if (GetRecordCount(_bindingSource) > 0)
                {
                    if (recId != 0)
                    {
                        idx = IndexOf(_bindingSource, recId);
                    }
                    DataGridView1.FirstDisplayedScrollingRowIndex = DataGridView1.Rows[idx].Index;
                    DataGridView1.Refresh();
                    DataGridView1.CurrentCell = DataGridView1.Rows[idx].Cells[1];
                    DataGridView1.Rows[idx].Selected = true;
                }
                DataGridView1.Columns[2].Width = 300;
                if (DataGridView1.RowCount > 0) DataGridView1.FastAutoSizeColumns();
                Cursor.Current = Cursors.Default;
            }
            catch (Exception ex)
            {
                var message = $"Error Loading Data: {Environment.NewLine}{ex.Message}";
                _logger.LogDetailAsync(message);
                Mediator.GetInstance().OnGeneralError(this, message);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
        public int IndexOf(BindingSource bindingSource, int value)
        {
            var count = bindingSource.Count;
            var itemIndex = 4;
            for (var i = 0; i < count; i++)
            {
                ItemDefinitionView rec = ((ObjectView<ItemDefinitionView>)bindingSource[i]).Object;
                if (rec.Id == value)
                {
                    itemIndex = i;
                    break;
                }
            }
            return itemIndex;
        }
        private void SetupViewEditBindings()
        {
            ComboBoxViewEditArea.DataBindings.Add("SelectedValue", _bindingSource, "AreaId");
            TextBoxViewEditItem.DataBindings.Add("Text", _bindingSource, "Item");
            TextBoxViewEditDescription.DataBindings.Add("Text", _bindingSource, "Description");
            TextBoxViewEditLocationMax.DataBindings.Add("Text", _bindingSource, "LocationMax");
            TextBoxViewEditLocationMin.DataBindings.Add("Text", _bindingSource, "LocationMin");
            TextBoxViewEditSystemMax.DataBindings.Add("Text", _bindingSource, "SystemMax");
            TextBoxViewEditSystemMin.DataBindings.Add("Text", _bindingSource, "SystemMin");
            TextBoxViewEditPickMax.DataBindings.Add("Text", _bindingSource, "PickMax");
            ComboBoxViewEditSizeCode.DataBindings.Add("SelectedValue", _bindingSource, "SizeCodeId");
            ComboBoxViewEditVelocityCode.DataBindings.Add("SelectedValue", _bindingSource, "VelocityCodeId");
            ComboBoxViewEditHeightCode.DataBindings.Add("SelectedValue", _bindingSource, "HeightCodeId");
            ComboBoxViewEditStorageType.DataBindings.Add("SelectedValue", _bindingSource, "StorageTypeId");
            ComboBoxViewEditUnitOfIssue.DataBindings.Add("SelectedValue", _bindingSource, "UnitOfIssueId");
            TextBoxViewEditWeight.DataBindings.Add("Text", _bindingSource, "Weight");
            CheckBoxViewEditScale.DataBindings.Add("Checked", _bindingSource, "Scale", false, DataSourceUpdateMode.OnPropertyChanged);
        }
        private int GetRecordCount(BindingSource bindingSource)
        {
            var count = bindingSource.Count;
            LabelRecordCount.Text = $"{_resourceManager.GetString("Records")}: {count.ToString()}";
            return count;
        }
        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //tabControl1.SelectedTab = tabPage2;
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
            if (e.KeyCode == Keys.Return)
            {
                RefreshData();
            }
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
            CloseButtonPressed = true;
        }
        private void MButtonViewEdit_Click(object sender, EventArgs e)
        {
            ViewEditItemDefinition();
        }

        private void ViewEditItemDefinition()
        {
            if (_bindingSource.Count <= 0) return;

            // Check Inventory and OrderDetails for this item
            var itemDefinitionView = ((ObjectView<ItemDefinitionView>)_bindingSource.Current).Object;
            if (itemDefinitionView == null) return;
            var item = itemDefinitionView.Item;

            var recs = _repoInventory.All().Where(r => r.ItemDefinitionId == itemDefinitionView.Id).ToList();
            var msg = $"{recs.Count} {_resourceManager.GetString("Message14")} {Environment.NewLine}";
            var recs2 = _repoOrderDetails.All().Where(r => r.ItemDefinitionId == itemDefinitionView.Id && r.LineStatusId != (int)LineStatus.Complete).ToList();
            msg += $"{recs2.Count} {_resourceManager.GetString("Message13")}{Environment.NewLine}";
            if (recs.Count == 0)
            {
                ComboBoxViewEditArea.Enabled = true;
                LabelViewEditChangeStationWarning.ForeColor = Color.Black;
                msg += _resourceManager.GetString("Message11");
            }
            else
            {
                ComboBoxViewEditArea.Enabled = false;
                LabelViewEditChangeStationWarning.ForeColor = Color.Red;
                msg += _resourceManager.GetString("Message12");
            }
            PictureBoxViewEditImage.LoadAsync(_imageManager.GetImageFile(itemDefinitionView.Item));
            LabelViewEditChangeStationWarning.Text = msg;

            var existingItems = _itemDefinitionsRepository.FindItemDefinitionViewsByItem(item).ToList();
            _existingItemsBindingSource.DataSource = existingItems;
            DataGridViewViewEditExistingItems.DataSource = _existingItemsBindingSource;

            var akas = _akaRepository.GetAkas(item).ToList();
            _akaBindingSource.DataSource = akas;
            ListBoxViewEditAkas.DataSource = _akaBindingSource;


            tabControl1.SelectedTab = ViewEdit;
        }
        private void MButtonNew_Click(object sender, EventArgs e)
        {
            NewItem();
        }
        private void NewItem()
        {
            //CheckBoxAllStations.Checked = true;
            RefreshData();
            TextBoxNewItem.Visible = true;
            TextBoxNewDescription.Visible = true;
            LabelNewDescription.Visible = true;
            LabelNewItem.Visible = true;
            var item = _jsonData.LoadFile<ItemDefinition>();
            ComboBoxNewArea.SelectedValue = string.IsNullOrEmpty(item.AreaId.ToString()) ? _workstation.AreaId : item.AreaId;
            TextBoxNewItem.Text = string.Empty;
            TextBoxNewDescription.Text = string.Empty;
            TextBoxNewLocationMax.Text = string.IsNullOrEmpty(item.LocationMax.ToString()) ? "0" : item.LocationMax.ToString();
            TextBoxNewLocationMin.Text = string.IsNullOrEmpty(item.LocationMin.ToString()) ? "0" : item.LocationMin.ToString();
            TextBoxNewSystemMax.Text = string.IsNullOrEmpty(item.SystemMax.ToString()) ? "0" : item.SystemMax.ToString();
            TextBoxNewSystemMin.Text = string.IsNullOrEmpty(item.SystemMin.ToString()) ? "0" : item.SystemMin.ToString();
            TextBoxNewPickMax.Text = string.IsNullOrEmpty(item.PickMax.ToString()) ? "0" : item.PickMax.ToString();

            ComboBoxNewSizeCode.SelectedValue = string.IsNullOrEmpty(item.SizeCodeId.ToString())
                ? ((SizeCode)ComboBoxNewSizeCode.Items[0]).Id
                : item.SizeCodeId;
            ComboBoxNewVelocityCode.SelectedValue = string.IsNullOrEmpty(item.VelocityCodeId.ToString())
                ? ((VelocityCode)ComboBoxNewVelocityCode.Items[0]).Id
                : item.VelocityCodeId;
            ComboBoxNewHeightCode.SelectedValue = string.IsNullOrEmpty(item.HeightCodeId.ToString())
                ? ((HeightCode)ComboBoxNewHeightCode.Items[0]).Id
                : item.HeightCodeId;
            ComboBoxNewStorageType.SelectedValue = string.IsNullOrEmpty(item.StorageTypeId.ToString())
                ? ((StorageType)ComboBoxNewStorageType.Items[0]).Id
                : item.StorageTypeId;
            ComboBoxNewUnitOfIssue.SelectedValue = string.IsNullOrEmpty(item.UnitOfIssueId.ToString())
                ? ((UnitOfIssue)ComboBoxNewUnitOfIssue.Items[0]).Id
                : item.UnitOfIssueId;
            TextBoxNewWeight.Text = string.IsNullOrEmpty(item.Weight.ToString(CultureInfo.InvariantCulture))
                ? "0"
                : item.Weight.ToString(CultureInfo.InvariantCulture);
            CheckBoxNewScale.Checked = item.Scale;
            PictureBoxNewImage.LoadAsync(_imageManager.GetImageFile());

            tabControl1.SelectedTab = New;
        }
        private void MbViewEditListing_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = Listing;
        }
        private void MbViewEditNew_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = New;
        }
        private void MbViewEditClose_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = Listing;
        }
        private void MbViewEditSave_Click(object sender, EventArgs e)
        {
            MbViewEditSave.Enabled = false;
            UpdateViewEdit();
            MbViewEditSave.Enabled = true;
        }
        private void MbNewListing_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = Listing;
        }
        private void MbNewViewEdit_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = ViewEdit;
        }
        private void MbNewSave_Click(object sender, EventArgs e)
        {
            MbNewSave.Enabled = false;
            SaveNew();
            MbNewSave.Enabled = true;
        }
        private void MbNewClose_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = Listing;
        }
        #endregion
        /// <summary>
        /// Save a New Item Definition
        /// </summary>
        private  void SaveNew()
        {

            if (!string.IsNullOrEmpty(TextBoxNewItem.Text.Trim()))
            {
                var item = TextBoxNewItem.Text.Trim();
                var areaId = ((Area)ComboBoxNewArea.SelectedItem).Id;

                if (IsDuplicate(item, areaId)) return;

                if ((UnitOfIssue)ComboBoxNewUnitOfIssue.SelectedItem == null) return;
                var unitOfIssueId = ((UnitOfIssue)ComboBoxNewUnitOfIssue.SelectedItem).Id;
                if ((StorageType)ComboBoxNewStorageType.SelectedItem == null) return;
                var storageTypeId = ((StorageType)ComboBoxNewStorageType.SelectedItem).Id;
                if ((SizeCode)ComboBoxNewSizeCode.SelectedItem == null) return;
                var sizeCodeId = ((SizeCode)ComboBoxNewSizeCode.SelectedItem).Id;
                if ((VelocityCode)ComboBoxNewVelocityCode.SelectedItem == null) return;
                var velocityCodeId = ((VelocityCode)ComboBoxNewVelocityCode.SelectedItem).Id;
                if ((HeightCode)ComboBoxNewHeightCode.SelectedItem == null) return;
                var heightCodeId = ((HeightCode)ComboBoxNewHeightCode.SelectedItem).Id;

                var weight = string.IsNullOrEmpty(TextBoxNewWeight.Text) ? "0" : TextBoxNewWeight.Text;
                var locationMax = string.IsNullOrEmpty(TextBoxNewLocationMax.Text) ? "0" : TextBoxNewLocationMax.Text;
                var locationMin = string.IsNullOrEmpty(TextBoxNewLocationMin.Text) ? "0" : TextBoxNewLocationMin.Text;
                var systemMax = string.IsNullOrEmpty(TextBoxNewSystemMax.Text) ? "0" : TextBoxNewSystemMax.Text;
                var systemMin = string.IsNullOrEmpty(TextBoxNewSystemMin.Text) ? "0" : TextBoxNewSystemMin.Text;
                var pickMax = string.IsNullOrEmpty(TextBoxNewPickMax.Text) ? "0" : TextBoxNewPickMax.Text;

                if (!string.IsNullOrEmpty(TextBoxNewDescription.Text.Trim()))
                {
                    var description = TextBoxNewDescription.Text.Trim();
                    var itemDef = _repoItemDefinition.FindBy(f => f.Item == item && f.AreaId == areaId).FirstOrDefault();
                    if (itemDef == null)
                    {
                        var rec = new ItemDefinition()
                        {
                            AreaId = areaId,
                            Item = item,
                            Description = description,
                            UnitOfIssueId = unitOfIssueId,
                            SizeCodeId = sizeCodeId,
                            VelocityCodeId = velocityCodeId,
                            HeightCodeId = heightCodeId,
                            LocationMax = locationMax.ParseInt(),
                            LocationMin = locationMin.ParseInt(),
                            SystemMax = systemMax.ParseInt(),
                            SystemMin = systemMin.ParseInt(),
                            StorageTypeId = storageTypeId,
                            PickMax = pickMax.ParseInt(),
                            Weight = float.Parse(weight),
                            Scale = CheckBoxNewScale.Checked
                        };
                        try
                        {
                             _repoItemDefinition.InsertAsync(rec);
                            _historyManager.SaveHistoryAsync(ActionCode.ItemAdd, rec);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"{_resourceManager.GetString("Message0")}{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.InnerException}");
                        }
                        RefreshData();
                        tabControl1.SelectedTab = Listing;
                    }
                    else
                    {
                        MessageBox.Show($"{_resourceManager.GetString("Message1")}: {itemDef.AreaId}.", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show(_resourceManager.GetString("Message2"));
                }
            }
            else
            {
                MessageBox.Show(_resourceManager.GetString("Message3"));
            }
        }
        private void UpdateViewEdit()
        {

            var id = ((ObjectView<ItemDefinitionView>)_bindingSource.Current).Object.Id;
            var area = (Area)ComboBoxViewEditArea.SelectedItem;
            if (area != null)
            {
                var areaId = area.Id;


                if (!string.IsNullOrEmpty(TextBoxViewEditItem.Text))
                {
                    var item = TextBoxViewEditItem.Text;

                    if ((UnitOfIssue)ComboBoxViewEditUnitOfIssue.SelectedItem == null) return;
                    var unitOfIssueId = ((UnitOfIssue)ComboBoxViewEditUnitOfIssue.SelectedItem).Id;
                    if ((StorageType)ComboBoxViewEditStorageType.SelectedItem == null) return;
                    var storageTypeId = ((StorageType)ComboBoxViewEditStorageType.SelectedItem).Id;
                    if ((SizeCode)ComboBoxViewEditSizeCode.SelectedItem == null) return;
                    var sizeCodeId = ((SizeCode)ComboBoxViewEditSizeCode.SelectedItem).Id;
                    if ((VelocityCode)ComboBoxViewEditVelocityCode.SelectedItem == null) return;
                    var velocityCodeId = ((VelocityCode)ComboBoxViewEditVelocityCode.SelectedItem).Id;
                    if ((HeightCode)ComboBoxViewEditHeightCode.SelectedItem == null) return;
                    var heightCodeId = ((HeightCode)ComboBoxViewEditHeightCode.SelectedItem).Id;



                    var weight = string.IsNullOrEmpty(TextBoxViewEditWeight.Text) ? "0" : TextBoxViewEditWeight.Text;
                    var locationMax = string.IsNullOrEmpty(TextBoxViewEditLocationMax.Text)
                        ? "0"
                        : TextBoxViewEditLocationMax.Text;
                    var locationMin = string.IsNullOrEmpty(TextBoxViewEditLocationMin.Text)
                        ? "0"
                        : TextBoxViewEditLocationMin.Text;
                    var systemMax = string.IsNullOrEmpty(TextBoxViewEditSystemMax.Text) ? "0" : TextBoxViewEditSystemMax.Text;
                    var systemMin = string.IsNullOrEmpty(TextBoxViewEditSystemMin.Text) ? "0" : TextBoxViewEditSystemMin.Text;
                    var pickMax = string.IsNullOrEmpty(TextBoxViewEditPickMax.Text) ? "0" : TextBoxViewEditPickMax.Text;


                    if (!string.IsNullOrEmpty(TextBoxViewEditDescription.Text))
                    {
                        var description = TextBoxViewEditDescription.Text;
                        var itemDef = _repoItemDefinition.FindByKey(id);
                        if (itemDef != null)
                        {
                            itemDef.AreaId = areaId;
                            itemDef.Item = item;
                            itemDef.Description = description;
                            itemDef.LocationMax = locationMax.ParseInt();
                            itemDef.LocationMin = locationMin.ParseInt();
                            itemDef.SystemMax = systemMax.ParseInt();
                            itemDef.SystemMin = systemMin.ParseInt();
                            itemDef.SizeCodeId = sizeCodeId;
                            itemDef.VelocityCodeId = velocityCodeId;
                            itemDef.HeightCodeId = heightCodeId;
                            itemDef.StorageTypeId = storageTypeId;
                            itemDef.UnitOfIssueId = unitOfIssueId;
                            itemDef.PickMax = pickMax.ParseInt();
                            itemDef.Weight = float.Parse(weight);
                            itemDef.Scale = CheckBoxViewEditScale.Checked;
                            try
                            {
                                _repoItemDefinition.Update(itemDef);
                                _historyManager.SaveHistoryAsync(ActionCode.ItemModify, itemDef);

                                var recs = _repoOrderDetails.All()
                                    .Where(r => r.ItemDefinitionId == itemDef.Id && r.LineStatusId != (int)LineStatus.Available).ToList();
                                foreach (var rec in recs)
                                {
                                    rec.AreaId = areaId;
                                    _repoOrderDetails.Update(rec);


                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(
                                    $"{_resourceManager.GetString("Message4")}{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.InnerException}");
                            }

                            RefreshData();
                            tabControl1.SelectedTab = Listing;
                        }
                        else
                        {
                            MessageBox.Show(_resourceManager.GetString("Message5"), string.Empty, MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show(_resourceManager.GetString("Message6"));
                    }
                }
            }
            else

            {
                MessageBox.Show(_resourceManager.GetString("Message7"));
            }
        }
        //private bool ValidateFields(ItemDefinition rec)
        //{
        //    if (!StringValidator(rec.Item))
        //    {
        //        return false;
        //    }
        //    if (!StringValidator(rec.Description))
        //    {
        //        return false;
        //    }
        //    if (!IntegerValidator(rec.LocationMax))
        //    {
        //        return false;
        //    }
        //    if (!IntegerValidator(rec.LocationMin))
        //    {
        //        return false;
        //    }
        //    if (!IntegerValidator(rec.SystemMax))
        //    {
        //        return false;
        //    }
        //    if (!IntegerValidator(rec.SystemMin))
        //    {
        //        return false;
        //    }
        //    if (!floatValidator(rec.Weight))
        //    {
        //        return false;
        //    }
        //    return true;
        //}
        //private bool floatValidator(float input)
        //{
        //    if (input >= 0)
        //    {
        //        return true;
        //    }
        //    return false;
        //}
        //private bool StringValidator(string input)
        //{
        //    string pattern = "[^a-zA-Z]";
        //    if (Regex.IsMatch(input, pattern))
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}
        ////validate integer 
        //private bool IntegerValidator(int input)
        //{
        //    string pattern = "^[0-9]+$";
        //    if (Regex.IsMatch(input.ToString(), pattern))
        //    {
        //        if (input < 0)
        //        {
        //            MessageBox.Show("Entry must be greater than zero.");
        //            return false;
        //        }
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}
        private bool IsDuplicate(string item, int areaId)
        {
            var rec = _repoItemDefinition.FindBy(f => f.Item == item && f.AreaId == areaId).FirstOrDefault();
            if (rec == null) return false;
            MessageBox.Show($"{_resourceManager.GetString("Message8")}: {rec.Item}  {rec.AreaId}", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return true;
        }
        #region Form Setup
        private void SetupGrid()
        {
            DataGridView1.AutoGenerateColumns = false;
            DataGridView1.SelectionMode = DataGridViewSelectionMode.CellSelect;
            var w = (DataGridView1.Width - 60) / 10;
            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "AreaId",
                HeaderText = _gridResourceManager.GetString("Area"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "AreaId"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = _gridResourceManager.GetString("Item"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Item"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = _gridResourceManager.GetString("Description"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Description"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "UnitOfIssueName",
                HeaderText = _gridResourceManager.GetString("UnitOfIssueName"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "UnitOfIssueName"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LocationMax",
                HeaderText = _gridResourceManager.GetString("LocationMax"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "LocationMax"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LocationMin",
                HeaderText = _gridResourceManager.GetString("LocationMin"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "LocationMin"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SystemMax",
                HeaderText = _gridResourceManager.GetString("SystemMax"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "SystemMax"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SystemMin",
                HeaderText = _gridResourceManager.GetString("SystemMin"),
                Visible = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "SystemMin"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SizeCodeName",
                HeaderText = _gridResourceManager.GetString("SizeCodeName"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "SizeCodeName"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "VelocityCodeName",
                HeaderText = _gridResourceManager.GetString("VelocityCodeName"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "VelocityCodeName"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "HeightCodeName",
                HeaderText = _gridResourceManager.GetString("HeightCodeName"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "HeightCodeName"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StorageTypeName",
                HeaderText = _gridResourceManager.GetString("StorageTypeName"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "StorageTypeName"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PickMax",
                HeaderText = _gridResourceManager.GetString("PickMax"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "PickMax"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Weight",
                HeaderText = _gridResourceManager.GetString("Weight"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Weight"
            };
            DataGridView1.Columns.Add(col);
            var ckcol = new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "Scale",
                HeaderText = _gridResourceManager.GetString("Scale"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "Scale"
            };
            DataGridView1.Columns.Add(ckcol);
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

            DataGridViewExistingItems.AutoGenerateColumns = false;
            DataGridViewExistingItems.SelectionMode = DataGridViewSelectionMode.CellSelect;

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "AreaId",
                HeaderText = _gridResourceManager.GetString("Area"),
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } },
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "AreaId"
            };
            DataGridViewExistingItems.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = _gridResourceManager.GetString("Item"),
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } },
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Item"
            };
            DataGridViewExistingItems.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = _gridResourceManager.GetString("Description"),
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } },
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                Name = "Description"
            };
            DataGridViewExistingItems.Columns.Add(col);
            DataGridViewExistingItems.EnableHeadersVisualStyles = false;
            DataGridViewExistingItems.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            DataGridViewExistingItems.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);

            DataGridViewViewEditExistingItems.AutoGenerateColumns = false;
            DataGridViewViewEditExistingItems.SelectionMode = DataGridViewSelectionMode.CellSelect;

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "AreaId",
                HeaderText = _gridResourceManager.GetString("Area"),
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } },
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "AreaId"
            };
            DataGridViewViewEditExistingItems.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = _gridResourceManager.GetString("Item"),
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } },
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Item"
            };
            DataGridViewViewEditExistingItems.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = _gridResourceManager.GetString("Description"),
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } },
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                Name = "Description"
            };
            DataGridViewViewEditExistingItems.Columns.Add(col);
            DataGridViewViewEditExistingItems.EnableHeadersVisualStyles = false;
            DataGridViewViewEditExistingItems.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            DataGridViewViewEditExistingItems.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);
        }
        private void SetupTabControl()
        {
            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(0, 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;
            foreach (TabPage tab in tabControl1.TabPages)
            {
                tab.Text = string.Empty;
            }
        }
        private void SetupNewForm()
        {
            //LabelFindDescription.Text = "Search any part of Item or Description fields";
            ComboBoxNewSizeCode.DataSource = _repoSizeCode.All();
            ComboBoxNewSizeCode.DisplayMember = "Name";
            ComboBoxNewSizeCode.ValueMember = "Id";
            ComboBoxNewVelocityCode.DataSource = _repoVelocityCode.All();
            ComboBoxNewVelocityCode.DisplayMember = "Name";
            ComboBoxNewVelocityCode.ValueMember = "Id";
            ComboBoxNewHeightCode.DataSource = _repoHeightCode.All();
            ComboBoxNewHeightCode.DisplayMember = "Name";
            ComboBoxNewHeightCode.ValueMember = "Id";
            ComboBoxNewArea.DataSource = _areaRepository.GetAllAreas();
            ComboBoxNewArea.DisplayMember = "Name";
            ComboBoxNewArea.ValueMember = "Id";
            ComboBoxNewStorageType.DataSource = _repoStorageType.All();
            ComboBoxNewStorageType.DisplayMember = "Name";
            ComboBoxNewStorageType.ValueMember = "Id";
            ComboBoxNewUnitOfIssue.DataSource = _repoUnitOfIssue.All();
            ComboBoxNewUnitOfIssue.DisplayMember = "Name";
            ComboBoxNewUnitOfIssue.ValueMember = "Id";
        }
        private void SetupViewEditForm()
        {

            //LabelFindDescription.Text = "Search any part of Item or Description fields";
            ComboBoxViewEditSizeCode.DataSource = _repoSizeCode.All();
            ComboBoxViewEditSizeCode.DisplayMember = "Name";
            ComboBoxViewEditSizeCode.ValueMember = "Id";
            ComboBoxViewEditVelocityCode.DataSource = _repoVelocityCode.All();
            ComboBoxViewEditVelocityCode.DisplayMember = "Name";
            ComboBoxViewEditVelocityCode.ValueMember = "Id";
            ComboBoxViewEditHeightCode.DataSource = _repoHeightCode.All();
            ComboBoxViewEditHeightCode.DisplayMember = "Name";
            ComboBoxViewEditHeightCode.ValueMember = "Id";
            ComboBoxViewEditArea.DataSource = _areaRepository.GetAllAreas();
            ComboBoxViewEditArea.DisplayMember = "Name";
            ComboBoxViewEditArea.ValueMember = "Id";
            ComboBoxViewEditStorageType.DataSource = _repoStorageType.All();
            ComboBoxViewEditStorageType.DisplayMember = "Name";
            ComboBoxViewEditStorageType.ValueMember = "Id";
            ComboBoxViewEditUnitOfIssue.DataSource = _repoUnitOfIssue.All();
            ComboBoxViewEditUnitOfIssue.DisplayMember = "Name";
            ComboBoxViewEditUnitOfIssue.ValueMember = "Id";
        }
        #endregion
        #region Return Key Functions
        private void TextBoxNewItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                TextBoxNewDescription.Focus();
            }
        }
        private void TextBoxNewDescription_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                TextBoxNewLocationMax.Focus();
            }
        }
        private void TextBoxNewLocationMax_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                TextBoxNewLocationMin.Focus();
            }
        }
        private void TextBoxNewLocationMin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                TextBoxNewSystemMax.Focus();
            }
        }
        private void TextBoxNewSystemMax_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                ComboBoxNewSizeCode.Focus();
            }
        }
        private void ComboBoxNewSizeCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                ComboBoxNewVelocityCode.Focus();
            }
        }
        private void ComboBoxNewVelocityCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                ComboBoxNewHeightCode.Focus();
            }
        }
        private void ComboBoxNewHeightCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                TextBoxNewDescription.Focus();
            }
        }
        private void TextBoxViewEditItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                TextBoxViewEditDescription.Focus();
            }
        }
        private void TextBoxViewEditDescription_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                TextBoxViewEditLocationMax.Focus();
            }
        }
        private void TextBoxViewEditLocationMax_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                TextBoxViewEditLocationMin.Focus();
            }
        }
        private void TextBoxViewEditLocationMin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                TextBoxViewEditSystemMax.Focus();
            }
        }
        private void TextBoxViewEditSystemMax_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                ComboBoxViewEditSizeCode.Focus();
            }
        }
        private void ComboBoxViewEditSizeCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                ComboBoxViewEditVelocityCode.Focus();
            }
        }
        private void ComboBoxViewEditVelocityCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                ComboBoxViewEditHeightCode.Focus();
            }
        }
        private void tabControl1_Enter(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex != 1)
            {
            }
            else
            {
                TextBoxViewEditItem.Focus();
            }
            if (tabControl1.SelectedIndex == 2)
            {
                TextBoxNewItem.Focus();
            }
        }
        #endregion
        private void MbViewEditDelete_Click(object sender, EventArgs e)
        {
            var itemDefinitionView = ((ObjectView<ItemDefinitionView>)_bindingSource.Current).Object;
            var itemDefinition = _repoItemDefinition.FindByKey(itemDefinitionView.Id);
            if (itemDefinition is null) return;

            if (CheckForInventory(itemDefinition.Id)) return;
            _historyManager.SaveHistoryAsync(ActionCode.ItemDelete, itemDefinition);
            _repoItemDefinition.Delete(itemDefinition.Id);

            TextBoxFind.Text = string.Empty;
            RefreshData();
            TextBoxFind.Focus();
            tabControl1.SelectedTab = Listing;
        }
        private bool CheckForInventory(int id)
        {
            var recs = _repoInventory.FindBy(r => r.ItemDefinitionId == id).ToList();
            if (recs.Count <= 0) return false;
            MessageBox.Show($"{_resourceManager.GetString("Message9")} {recs.Count}");
            return true;
        }
        /// <summary>
        /// When the user leaves the new item text box,
        /// fill in the other fields and
        /// update the Existing Items Grid
        /// and the AKA List Box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxNewItem_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TextBoxNewItem.Text)) return;
            var item = TextBoxNewItem.Text;
            // see if the item exists
            var itemDefinition = _repoItemDefinition.FindBy(r => r.Item == item).FirstOrDefault();

            // if the item Definition is null, check the AKA table
            if (itemDefinition is null)
            {
                var aka = _akaRepository.Get(item);
                if (!string.IsNullOrEmpty(aka))
                {
                    TextBoxNewItem.Text = aka;
                    item = aka;
                    itemDefinition = _repoItemDefinition.FindBy(r => r.Item == aka).FirstOrDefault();
                }
            }


            if (itemDefinition != null)
            {
                // if it does, populate the fields
                TextBoxNewDescription.Text = itemDefinition.Description;
                ComboBoxNewArea.SelectedValue = itemDefinition.AreaId;
                TextBoxNewLocationMax.Text = itemDefinition.LocationMax.ToString();
                TextBoxNewLocationMin.Text = itemDefinition.LocationMin.ToString();
                TextBoxNewSystemMax.Text = itemDefinition.SystemMax.ToString();
                ComboBoxNewSizeCode.SelectedValue = itemDefinition.SizeCode;
                ComboBoxNewVelocityCode.SelectedValue = itemDefinition.VelocityCode;
                ComboBoxNewHeightCode.SelectedValue = itemDefinition.HeightCode;
                ComboBoxNewUnitOfIssue.SelectedValue = itemDefinition.UnitOfIssueId;
                TextBoxNewWeight.Text = itemDefinition.Weight.ToString("F4");
                TextBoxNewDescription.Focus();
                TextBoxNewDescription.SelectAll();


                var existingItems = _itemDefinitionsRepository.FindItemDefinitionViewsByItem(item).ToList();
                _existingItemsBindingSource.DataSource = existingItems;
                DataGridViewExistingItems.DataSource = _existingItemsBindingSource;

                var akas = _akaRepository.GetAkas(item).ToList();
                _akaBindingSource.DataSource = akas;
                ListBoxAkas.DataSource = _akaBindingSource;

            }


            //DataGridViewExistingItems.DataSource = _itemDefinitionsRepository.FindItemDefinitionViewsByItem(item).ToList();
            //// DataGridViewExistingItems.DataSource = _repoItemDefinition.FindBy(r => r.Item == item).ToList(); 
            //ListBoxAkas.DataSource = _akaRepository.GetAkas(item).ToList();
        }
        private void MBPrintItemDefinitions_Click(object sender, EventArgs e)
        {
            CsvUtility.SaveToCsv(DataGridView1);
        }
        private void MbSaveAsDefault_Click(object sender, EventArgs e)
        {
            var area = ((Area)ComboBoxNewArea.SelectedItem);

            if (area != null)
            {
                var weight = string.IsNullOrEmpty(TextBoxNewWeight.Text) ? "0" : TextBoxNewWeight.Text;
                var locationMax = string.IsNullOrEmpty(TextBoxNewLocationMax.Text) ? "0" : TextBoxNewLocationMax.Text;
                var locationMin = string.IsNullOrEmpty(TextBoxNewLocationMin.Text) ? "0" : TextBoxNewLocationMin.Text;
                var systemMax = string.IsNullOrEmpty(TextBoxNewSystemMax.Text) ? "0" : TextBoxNewSystemMax.Text;
                var systemMin = string.IsNullOrEmpty(TextBoxNewSystemMin.Text) ? "0" : TextBoxNewSystemMin.Text;
                var rec = new ItemDefinition()
                {
                    AreaId = area.Id,
                    Item = string.Empty,
                    Description = string.Empty,
                    LocationMax = locationMax.ParseInt(),
                    LocationMin = locationMin.ParseInt(),
                    SystemMax = systemMax.ParseInt(),
                    SystemMin = systemMin.ParseInt(),
                    SizeCodeId = ((SizeCode)ComboBoxNewSizeCode.SelectedItem).Id,
                    VelocityCodeId = ((VelocityCode)ComboBoxNewVelocityCode.SelectedItem).Id,
                    HeightCodeId = ((HeightCode)ComboBoxNewHeightCode.SelectedItem).Id,
                    StorageTypeId = ((StorageType)ComboBoxNewStorageType.SelectedItem).Id,
                    UnitOfIssueId = ((UnitOfIssue)ComboBoxNewUnitOfIssue.SelectedItem).Id,
                    Weight = float.Parse(weight),
                    Scale = CheckBoxNewScale.Checked
                };
                try
                {
                    _jsonData.SaveFile(rec);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{_resourceManager.GetString("Message10")}{Environment.NewLine}" +
                                    $"{ex.Message}{ex.InnerException}");
                }
                //NewItem();
            }
        }
        private void MbLoadDefault_Click(object sender, EventArgs e)
        {
            var item = _jsonData.LoadFile<ItemDefinition>();
            TextBoxNewItem.Text = item.Item;
            TextBoxNewDescription.Text = item.Description;
            ComboBoxNewArea.SelectedValue = string.IsNullOrEmpty(item.AreaId.ToString()) ? _workstation.AreaId : item.AreaId;
            TextBoxNewLocationMax.Text = string.IsNullOrEmpty(item.LocationMax.ToString()) ? "0" : item.LocationMax.ToString();
            TextBoxNewLocationMin.Text = string.IsNullOrEmpty(item.LocationMin.ToString()) ? "0" : item.LocationMin.ToString();
            TextBoxNewSystemMax.Text = string.IsNullOrEmpty(item.SystemMax.ToString()) ? "0" : item.SystemMax.ToString();
            TextBoxNewSystemMin.Text = string.IsNullOrEmpty(item.SystemMin.ToString()) ? "0" : item.SystemMin.ToString();
            ComboBoxNewSizeCode.SelectedValue = string.IsNullOrEmpty(item.SizeCodeId.ToString())
                ? ((SizeCode)ComboBoxNewSizeCode.Items[0]).Id
                : item.SizeCodeId;
            ComboBoxNewVelocityCode.SelectedValue = string.IsNullOrEmpty(item.VelocityCodeId.ToString())
                ? ((VelocityCode)ComboBoxNewVelocityCode.Items[0]).Id
                : item.VelocityCodeId;
            ComboBoxNewHeightCode.SelectedValue = string.IsNullOrEmpty(item.HeightCodeId.ToString())
                ? ((HeightCode)ComboBoxNewHeightCode.Items[0]).Id
                : item.HeightCodeId;
            ComboBoxNewStorageType.SelectedValue = string.IsNullOrEmpty(item.StorageTypeId.ToString())
                ? ((StorageType)ComboBoxNewStorageType.Items[0]).Id
                : item.StorageTypeId;
            ComboBoxNewUnitOfIssue.SelectedValue = string.IsNullOrEmpty(item.UnitOfIssueId.ToString())
                ? ((UnitOfIssue)ComboBoxNewUnitOfIssue.Items[0]).Id
                : item.UnitOfIssueId;
            TextBoxNewWeight.Text = string.IsNullOrEmpty(item.Weight.ToString(CultureInfo.InvariantCulture))
                ? "0"
                : item.Weight.ToString(CultureInfo.InvariantCulture);
            CheckBoxNewScale.Checked = item.Scale;
        }
        private void CheckBoxAllStations_CheckedChanged(object sender, EventArgs e)
        {
            RefreshData();
        }
        private void FrmItemDefinitions_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !CloseButtonPressed;
        }
        private void SetCulture(string lang)
        {
            try
            {
                var languageDirectory = LoaderSettings.GetLanguageDirectory();
                _cultureInfo = CultureInfo.CreateSpecificCulture(lang);
                _resourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "FrmItemDefinitions",
               resourceDir: languageDirectory, usingResourceSet: null);
                _gridResourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "GridHeaders",
                    resourceDir: languageDirectory, usingResourceSet: null);
                LabelFormHeaderText.Text = _resourceManager.GetString("NeutronWarehouseMana");
                LabelFormTitle.Text = _resourceManager.GetString("ItemDefinitions");
                LabelFindDescription.Text = _resourceManager.GetString("SearchFor");
                MButtonNew.Text = _resourceManager.GetString("New");
                ButtonSaveToExcel.Text = _resourceManager.GetString("SaveToFile");
                MButtonViewEdit.Text = _resourceManager.GetString("View/Edit");
                MButtonClose.Text = _resourceManager.GetString("Home");
                MButtonSearch.Text = _resourceManager.GetString("Search");
                LabelAction.Text = _resourceManager.GetString("View/Edit");
                MbViewEditListing.Text = _resourceManager.GetString("Listing");
                MbViewEditDelete.Text = _resourceManager.GetString("Delete");
                MbViewEditClose.Text = _resourceManager.GetString("Back");
                MbViewEditSave.Text = _resourceManager.GetString("Save");
                LabelViewEditWeight.Text = _resourceManager.GetString("Weight");
                CheckBoxViewEditScale.Text = _resourceManager.GetString("UseScale");
                LabelViewEditUnitOfIssue.Text = _resourceManager.GetString("UnitOfIssue");
                LabelViewEditStorageType.Text = _resourceManager.GetString("StorageType");
                LabelViewEditHeight.Text = _resourceManager.GetString("Height");
                LabelViewEditVelocity.Text = _resourceManager.GetString("Velocity");
                LabelViewEditSystemMin.Text = _resourceManager.GetString("SystemMin");
                LabelViewEditSize.Text = _resourceManager.GetString("Size");
                LabelViewEditSystemMax.Text = _resourceManager.GetString("SystemMax");
                LabelViewEditLocationMin.Text = _resourceManager.GetString("LocationMin");
                LabelViewEditLocationMax.Text = _resourceManager.GetString("LocationMax");
                LabelViewEditDexcription.Text = _resourceManager.GetString("Description");
                LabelViewEditItem.Text = _resourceManager.GetString("Item");
                LabelViewEditArea.Text = _resourceManager.GetString("Area");
                LabelActionNew.Text = _resourceManager.GetString("New");
                MbNewListing.Text = _resourceManager.GetString("Listing");
                MbLoadDefault.Text = _resourceManager.GetString("LoadDefault");
                MbSaveAsDefault.Text = _resourceManager.GetString("SaveAsDefault");
                MbNewViewEdit.Text = _resourceManager.GetString("View/Edit");
                MbNewClose.Text = _resourceManager.GetString("Back");
                MbNewSave.Text = _resourceManager.GetString("Save");
                LabelNewUnitOfIssue.Text = _resourceManager.GetString("UnitOfIssue");
                LabelNewStorageType.Text = _resourceManager.GetString("StorageType");
                LabelNewWeight.Text = _resourceManager.GetString("Weight");
                CheckBoxNewScale.Text = _resourceManager.GetString("UseScale");
                LabelNewSystemMin.Text = _resourceManager.GetString("SystemMin");
                LabelNewHeight.Text = _resourceManager.GetString("Height");
                LabelNewVelocity.Text = _resourceManager.GetString("Velocity");
                LabelNewSize.Text = _resourceManager.GetString("Size");
                LabelNewSystemMax.Text = _resourceManager.GetString("SystemMax");
                LabelNewLocationMin.Text = _resourceManager.GetString("LocationMin");
                LabelNewLocationMax.Text = _resourceManager.GetString("LocationMax");
                LabelNewDescription.Text = _resourceManager.GetString("Description");
                LabelNewItem.Text = _resourceManager.GetString("Item");
                LabelNewArea.Text = _resourceManager.GetString("Area");
                LabelNewDefaultImage.Text = _resourceManager.GetString("DefaultImage");
                LabelViewEditDefaultImage.Text = _resourceManager.GetString("DefaultImage");
                LabelNewPickMax.Text = _resourceManager.GetString("PickMax");
                LabelViewEditPickMax.Text = _resourceManager.GetString("PickMax");
                LabelNewPickMaxInfo.Text = _resourceManager.GetString("PickMaxInfo");
                LabelViewEditPickMaxInfo.Text = _resourceManager.GetString("PickMaxInfo");
                LabelSelectArea.Text = _resourceManager.GetString("SelectArea");
                //_resourceManager.GetString("Message0");
                //_resourceManager.GetString("Message1");
                //_resourceManager.GetString("Message2");
                //_resourceManager.GetString("Message3");
                //_resourceManager.GetString("Message4");
                //_resourceManager.GetString("Message5");
                //_resourceManager.GetString("Message6");
                //_resourceManager.GetString("Message7");
                //_resourceManager.GetString("Message8");
                //_resourceManager.GetString("Message9");
                //_resourceManager.GetString("Message10");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading language file.  {ex.Message} {Environment.NewLine} {ex.InnerException} ");
            }
        }
        private void DataGridView1_DoubleClick(object sender, EventArgs e)
        {
            ViewEditItemDefinition();
        }

        private void DataGridViewViewEditExistingItems_DoubleClick(object sender, EventArgs e)
        {
            ShowExistingAreaViewEdit();
        }

        private void ShowExistingAreaViewEdit()
        {
            var itemDef = (ItemDefinitionView)DataGridViewViewEditExistingItems.CurrentRow?.DataBoundItem;
            if (itemDef == null) return;
            int id = itemDef.Id;
            var index = _bindingSource.Find("Id", id);
            if (index == -1)
            {
                MessageBox.Show(
                    $"Selection not in this Area.  If you want to view this selection, change to All Areas.");
                return;
            }
            
            _bindingSource.Position = index;

            ViewEditItemDefinition();
        }

        private void DataGridViewExistingItems_DoubleClick(object sender, EventArgs e)
        {
            ShowExistingArea();
        }

        private void DataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            var grid = sender as DataGridView;
            var columnIndex = e.ColumnIndex;
            var rowIndex = e.RowIndex;
            FormUtilities.CopyCellToClipBoard(grid, columnIndex, rowIndex);
        }


        private void ShowExistingArea()
        {
            var itemDef = (ItemDefinitionView)DataGridViewExistingItems.CurrentRow?.DataBoundItem;
            if (itemDef == null) return;
            int id = itemDef.Id;
            var index = _bindingSource.Find("Id", id);

            _bindingSource.Position = index;

            ViewEditItemDefinition();
        }

        private void TextBoxAka_Enter(object sender, EventArgs e)
        {
            // if (TextBoxAka.Text != "Add AKA here...") return;
            TextBoxAka.ForeColor = Color.Black;
            TextBoxAka.Text = "";
        }

        private void TextBoxAka_Leave(object sender, EventArgs e)
        {
            //TextBoxAka.ForeColor = Color.Gray;
            //TextBoxAka.Text = "Add AKA here...";
        }

        private void MbAddAka_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TextBoxAka.Text)) return;
            try
            {
                var itemDefinitionView = ((ObjectView<ItemDefinitionView>)_bindingSource.Current).Object;
                if (itemDefinitionView == null) return;
                var item = itemDefinitionView.Item;

                var aka = new AkaType { Aka = TextBoxAka.Text, Item = item };
                _akaRepository.Insert(aka);
                _akaBindingSource.Add(aka.Aka);
                TextBoxAka.Text = "Add AKA Here";
                TextBoxAka.ForeColor = Color.Gray;
                TextBoxAka.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Add AKA Error: {ex.Message} {Environment.NewLine} {ex.InnerException}");
                throw;
            }
        }

        private void ListBoxViewEditAkas_Click(object sender, EventArgs e)
        {
            var aka = (string)ListBoxViewEditAkas.SelectedItem;
            if (aka == null) return;
            TextBoxAka.Text = aka;
            TextBoxAka.ForeColor = Color.Black;

        }

        private void MbDeleteAka_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TextBoxAka.Text)) return;
            try
            {

                var aka = _akaRepository.GetAka(TextBoxAka.Text);
                if (aka == null) return;
                _akaRepository.Delete(aka);
                _akaBindingSource.Remove(aka.Aka);
                TextBoxAka.Text = "Add AKA Here";
                TextBoxAka.ForeColor = Color.Gray;
                TextBoxAka.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Add AKA Error: {ex.Message} {Environment.NewLine} {ex.InnerException}");
                throw;
            }
        }

        private void DataGridViewViewEditExistingItems_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            ShowExistingAreaViewEdit();
        }

        private List<ItemDefinitionView> GetSelectedItems(DataGridView dataGridView)
        {
            // Create a list of ItemDefinitionView
            var selectedList = new List<ItemDefinitionView>();
            // Loop through the selected rows
            foreach (DataGridViewRow row in dataGridView.SelectedRows)
            {
                // Get the ItemDefinitionView from the row  ((ObjectView<ItemDefinitionView>)_bindingSource.Current).Object;
                var itemDefinitionView = ((ObjectView<ItemDefinitionView>)row.DataBoundItem).Object;
                // Add the ItemDefinitionView to the list
                selectedList.Add(itemDefinitionView);
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
            _ = _logger.LogDetailAsync("Saving records to Excel spreadsheet");

            DataTable dataTable;
            // Initialize the Excel Service
            var excelService = new ExcelService();
            if (CheckBoxUseSelectedItems.Checked)
            {
                var selectedList = GetSelectedItems(DataGridView1);
                // Create a DataTable from the List(Of T) (ItemDefinitionView)
                dataTable = ToDataTable<ItemDefinitionView>(selectedList);
            }
            else
            {
                // Create a DataTable from the List(Of T) (ItemDefinitionView)
                dataTable = ToDataTable<ItemDefinitionView>(_currentList);
            }

            // Generate the Excel file
            excelService.Generate(dataTable);
            _ = _logger.LogDetailAsync($"Saved {dataTable.Rows.Count} records to Excel spreadsheet");
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
            _ = _logger.LogDetailAsync("Loading records from Excel spreadsheet");
            var excelService = new ExcelService();
            var dataTable = excelService.Update();
            BackgroundWorkerItemDefinitions.RunWorkerAsync(dataTable);
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

        private void ComboBoxAreaNumber_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_startup)
            {
                RefreshData();
            }
        }

        private void BackgroundWorkerItemDefinitions_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (!(sender is BackgroundWorker worker)) return;
            if (!(e.Argument is DataTable dataTable)) return;
            var rowCount = dataTable.Rows.Count;
            var processedCount = 0;
            // loop over the rows in the DataTable
            _ = _logger.LogDetailAsync($"Loading {rowCount} records from Excel spreadsheet");

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
                    var areaId = row["AreaId"].ToString();
                    var item = row["Item"].ToString();
                    var description = row["Description"].ToString();
                    var unitOfIssueId = row["UnitOfIssueId"].ToString();
                    var sizeCodeId = row["SizeCodeId"].ToString();
                    var velocityCodeId = row["VelocityCodeId"].ToString();
                    var heightCodeId = row["HeightCodeId"].ToString();
                    var locationMax = row["LocationMax"].ToString();
                    var locationMin = row["LocationMin"].ToString();
                    var systemMax = row["SystemMax"].ToString();
                    var systemMin = row["SystemMin"].ToString();
                    var storageTypeId = row["StorageTypeId"].ToString();
                    var pickMax = row["PickMax"].ToString();
                    var weight = row["Weight"].ToString();
                    var scale = row["Scale"].ToString();

                    // if the Id is empty, this is a new row
                    // create a new ItemDefinition object
                    if (string.IsNullOrWhiteSpace(id))
                    {
                        var itemDefinition = new ItemDefinition
                        {
                            AreaId = areaId.ParseInt(),
                            Item = item,
                            Description = description,
                            UnitOfIssueId = unitOfIssueId.ParseInt(),
                            SizeCodeId = sizeCodeId.ParseInt(),
                            VelocityCodeId = velocityCodeId.ParseInt(),
                            HeightCodeId = heightCodeId.ParseInt(),
                            LocationMax = locationMax.ParseInt(),
                            LocationMin = locationMin.ParseInt(),
                            SystemMax = systemMax.ParseInt(),
                            SystemMin = systemMin.ParseInt(),
                            StorageTypeId = storageTypeId.ParseInt(),
                            PickMax = pickMax.ParseInt(),
                            Weight = Convert.ToSingle(weight),
                            Scale = scale.ParseInt() == 1 ? true : false
                        };
                        // add the new ItemDefinition object to the database
                        _repoItemDefinition.Insert(itemDefinition);
                    }
                    else
                    {
                        // get the existing ItemDefinition object
                        var itemDefinition = _repoItemDefinition.FindByKey(id.ParseInt());
                        if (itemDefinition == null) continue;
                        // update the values
                        itemDefinition.AreaId = areaId.ParseInt();
                        itemDefinition.Item = item;
                        itemDefinition.Description = description;
                        itemDefinition.UnitOfIssueId = unitOfIssueId.ParseInt();
                        itemDefinition.SizeCodeId = sizeCodeId.ParseInt();
                        itemDefinition.VelocityCodeId = velocityCodeId.ParseInt();
                        itemDefinition.HeightCodeId = heightCodeId.ParseInt();
                        itemDefinition.LocationMax = locationMax.ParseInt();
                        itemDefinition.LocationMin = locationMin.ParseInt();
                        itemDefinition.SystemMax = systemMax.ParseInt();
                        itemDefinition.SystemMin = systemMin.ParseInt();
                        itemDefinition.StorageTypeId = storageTypeId.ParseInt();
                        itemDefinition.PickMax = pickMax.ParseInt();
                        itemDefinition.Weight = Convert.ToSingle(weight);
                        itemDefinition.Scale = scale.ParseInt() == 1 ? true : false;
                        // update the database
                        _repoItemDefinition.Update(itemDefinition);
                    }
                    // Update the progress
                    processedCount++;
                    var progressPercentage = (int)((double)processedCount / rowCount * 100);
                    if (progressPercentage % 25 == 0)
                    {
                        _ = _logger.LogDetailAsync($"Loading {progressPercentage}% complete");
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

        private void BackgroundWorkerItemDefinitions_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            ProgressBarItemDefinitions.Value = e.ProgressPercentage;
        }

        private void BackgroundWorkerItemDefinitions_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            RefreshData();
            ProgressBarItemDefinitions.Value = 0;
            Cursor.Current = Cursors.Default;
            ButtonLoadFromExcel.Enabled = true;
            ButtonSaveToExcel.Enabled = true;
            _ = _logger.LogDetailAsync("Loading records from Excel spreadsheet complete");
        }

        private void TextBoxFind_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            var textBox = sender as TextBox;
            FormUtilities.PasteFromClipboard(textBox);
        }
    }
}
