using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Equin.ApplicationFramework;
using JsonManager;
using MetroFramework.Forms;
using Neutron.Classes;
using Neutron.Global;
using NeutronCore.Enums;
using NeutronCore.Extensions;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.ModelViews;
using Neutron.Models;
using NeutronCore;
using NeutronCore.Global;
using NeutronData.PrintModels;
using NeutronDllu;

namespace Neutron.Forms
{
    public partial class FrmProductivity : MetroForm
    {
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;
        private ResourceManager _gridResourceManager;
        private ResourceManager _enumResourceManager;
        private BindingSource _bindingSourceSummary;
        private BindingSource _bindingSourceDetail;
        private DateTime _fromDate;
        private DateTime _toDate;
        private readonly IJsonData _jsonData;
        private readonly NeutronVariables _neutronVariables;
        private DocumentPrinterPreferences _documentPrinter;
        private ProductivityGroup _currentGroup = null;
        private bool _formInitialized;
        private bool _groupItemCheckEnabled = true;
        private bool _userItemCheckEnabled = true;
        private readonly string _fileName = "ProductivityGroups";
        private DateTime _currentFromDateTime;
        private DateTime _currentToDateTime;
        private DocumentToPrint _documentToPrint;
        private List<ActionCode> _actionCodes;
        private string _currentIDs;


        private bool _checkAllActions = false;
        private bool _clearAllActions = false;
        private bool _checkAllUsers = false;
        private bool _clearAllUsers = false;

        public FrmProductivity(IJsonData jsonData, NeutronVariables neutronVariables)
        {
            _jsonData = jsonData;
            _neutronVariables = neutronVariables;
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            HideTabControlTabs();
            DisableEvents();
            SetupCheckedListBoxGroups();
            SetupCheckedListBoxActionCodes();
            SetupGrids();
            SetInitialDateTimePickers();
            SetupCheckedListBoxUsers();
            EnableEvents();
            mlUserInfo.Text = GlobalVar.User?.UserInfo;
            _documentToPrint = new DocumentToPrint();
            _formInitialized = true;
        }

        private void FrmProductivity_Load(object sender, EventArgs e)
        {
            var date = DateTime.Now;
            DateTimePickerFrom.Value = new DateTime(2023, 1, 1, 0, 0, 0);

            DateTimePickerTo.Value = date;
            _currentFromDateTime = new DateTime(2023, 1, 1, 0, 0, 0);
            _currentToDateTime = date.LastDayOfMonth();
        }

        private void EnableEvents()
        {
            CheckedListBoxUsers.ItemCheck += new ItemCheckEventHandler(CheckedListBoxUsers_ItemCheck);
        }
        private void DisableEvents()
        {
            CheckedListBoxUsers.ItemCheck -= new ItemCheckEventHandler(CheckedListBoxUsers_ItemCheck);
        }
        private void SetInitialDateTimePickers()
        {
            var today = DateTime.Today;
            DateTimePickerFrom.Value = today.FirstDayOfMonth();
            DateTimePickerTo.Value = today.LastDayOfMonth();
            _currentFromDateTime = today.FirstDayOfMonth();
            _currentToDateTime = today.LastDayOfMonth();
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
        #region SetupCheckedListBoxes
        private void SetupCheckedListBoxGroups()
        {
            CheckedListBoxGroups.Items.Clear();
            var currentGroups = _jsonData.LoadFile<List<ProductivityGroup>>(_fileName);
            if (currentGroups.Count == 0) return;
            foreach (var productivityGroup in currentGroups)
            {
                CheckedListBoxGroups.Items.Add(productivityGroup);
            }
            CheckedListBoxGroups.DisplayMember = "Name";
            CheckedListBoxGroups.ValueMember = "Name";
            CheckedListBoxGroups.SetItemCheckState(0, CheckState.Checked);
            _currentGroup = (ProductivityGroup)CheckedListBoxGroups.Items[0];
        }
        private void SetupCheckedListBoxUsers()
        {
            var users = new List<User>();
            if (_currentGroup != null)
            {
                var currentUserIds = _currentGroup.UserIdString.CsvIdString;
                if (!string.IsNullOrEmpty(currentUserIds))
                {
                    var nums = currentUserIds.Split(',').Select(int.Parse).ToArray();
                    if (nums.Length > 0)
                    {
                        using (var db = new NeutronDb())
                        {
                            users = db.Users.Where(r => nums.Contains(r.Id)).OrderBy(o => o.Lastname).ToList();
                        }
                    }
                }
            }
            else
            {
                using (var db = new NeutronDb())
                {
                    users = db.Users.OrderBy(o => o.Lastname).ToList();
                }
            }
            CheckedListBoxUsers.Items.Clear();
            foreach (var user in users)
            {
                CheckedListBoxUsers.Items.Add(user);
            }
            CheckedListBoxUsers.DisplayMember = "FullName";
            CheckedListBoxUsers.ValueMember = "Id";
            for (var i = 0; i < CheckedListBoxUsers.Items.Count; i++)
            {
                CheckedListBoxUsers.SetItemChecked(i, true);
            }
        }

        private void SetupCheckedListBoxActionCodes()
        {
            var codes = new Dictionary<int, string>();

            _actionCodes = ((ActionCode[])Enum.GetValues(typeof(ActionCode))).ToList();
            var actionCodeDictionary = NeutronCore.Extensions.EnumExtensions.EnumToDictionary<ActionCode>();
            _currentIDs = _jsonData.LoadFile<ActionIdString>().CsvIdString;
            CheckedListBoxActionCodes.DataSource = new BindingSource(actionCodeDictionary, null);
            CheckedListBoxActionCodes.DisplayMember = "Value";
            CheckedListBoxActionCodes.ValueMember = "Key";

            if (!string.IsNullOrEmpty(_currentIDs))
            {
                var nums = _currentIDs.Split(',').Select(int.Parse).ToList();

                foreach (var num in nums)
                {
                    foreach (var item in actionCodeDictionary)
                    {
                        if (!item.Key.Equals(num)) continue;
                        if (!codes.ContainsKey(item.Key)) codes.Add(item.Key, item.Value);
                        break;
                    }
                }


                CheckedListBoxActionCodes.DataSource = new BindingSource(codes, null);
                for (var i = 0; i < CheckedListBoxActionCodes.Items.Count; i++)
                {
                    CheckedListBoxActionCodes.SetItemCheckState(i, CheckState.Checked);
                }
            }
        }


        //private void SetupCheckedListBoxActionCodes()
        //{
        //    var actionCodes = ((ActionCode[])Enum.GetValues(typeof(ActionCode)))
        //        .Select(r => new EnumModel() { Id = (int)r, Name = r.GetEnumDescription() }).ToList();
        //    var currentIds = _jsonData.LoadFile<ActionIdString>().CsvIdString;
        //    if (!string.IsNullOrEmpty(currentIds))
        //    {
        //        var nums = currentIds.Split(',').Select(int.Parse).ToArray();
        //        if (nums.Length > 0)
        //        {
        //            CheckedListBoxActionCodes.DataSource = actionCodes.Where(r => nums.Contains(r.Id)).OrderBy(o => o.Name).ToList();
        //            CheckedListBoxActionCodes.DisplayMember = "Name";
        //            CheckedListBoxActionCodes.ValueMember = "Id";
        //        }
        //    }
        //    else
        //    {
        //        CheckedListBoxActionCodes.DataSource = new BindingSource(actionCodes, null);
        //        CheckedListBoxActionCodes.DisplayMember = "Name";
        //        CheckedListBoxActionCodes.ValueMember = "Id";
        //    }
        //}
        #endregion
        private void SetupGrids()
        {
            DataGridView1.AutoGenerateColumns = false;
            DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridView1.DefaultCellStyle.ForeColor = Color.Black;
            DataGridView1.DefaultCellStyle.BackColor = Color.White;
            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Date",
                HeaderText = _gridResourceManager.GetString("Date"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Date",
                Visible = true
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Employee",
                HeaderText = _gridResourceManager.GetString("Employee"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Employee",
                Visible = true
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Action",
                HeaderText = _gridResourceManager.GetString("Action"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Action",
                Visible = true
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Lines",
                HeaderText = _gridResourceManager.GetString("Lines"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Lines",
                Visible = true
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Pieces",
                HeaderText = _gridResourceManager.GetString("Pieces"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Pieces",
                Visible = true
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Orders",
                HeaderText = _gridResourceManager.GetString("Orders"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Orders",
                Visible = true
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Workstation",
                HeaderText = _gridResourceManager.GetString("Workstation"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                Name = "Workstation",
                Visible = true
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "UserId",
                HeaderText = _gridResourceManager.GetString("UserId"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "UserId",
                Visible = false
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ActionCodeId",
                HeaderText = _gridResourceManager.GetString("ActionCodeId"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "ActionCodeId",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Visible = false
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "WorkstationId",
                HeaderText = _gridResourceManager.GetString("WorkstationId"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "WorkstationId",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Visible = false
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ActionCode",
                HeaderText = _gridResourceManager.GetString("ActionCode"),
                Visible = false,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "ActionCode",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(col);

            DataGridView1.EnableHeadersVisualStyles = false;
            DataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            DataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);

            //foreach (DataGridViewColumn column in DataGridView1.Columns)
            //{
            //    column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //    column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            //}
            //DataGridView2
            DataGridView2.AutoGenerateColumns = false;
            DataGridView2.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridView2.DefaultCellStyle.ForeColor = Color.Black;
            DataGridView2.DefaultCellStyle.BackColor = Color.White;
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Date",
                HeaderText = _gridResourceManager.GetString("Date"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Date",
                Visible = true
            };
            DataGridView2.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Employee",
                HeaderText = _gridResourceManager.GetString("Employee"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Employee",
                Visible = true
            };
            DataGridView2.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Action",
                HeaderText = _gridResourceManager.GetString("Action"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Action",
                Visible = true
            };
            DataGridView2.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Order",
                HeaderText = _gridResourceManager.GetString("Order"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Order",
                Visible = true
            };
            DataGridView2.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Reservation",
                HeaderText = _gridResourceManager.GetString("Reservation"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Reservation",
                Visible = true
            };
            DataGridView2.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = _gridResourceManager.GetString("Item"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Item",
                Visible = true
            };
            DataGridView2.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = _gridResourceManager.GetString("Description"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Description",
                Visible = true
            };
            DataGridView2.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Requested",
                HeaderText = _gridResourceManager.GetString("Requested"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Requested",
                Visible = true
            };
            DataGridView2.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Issued",
                HeaderText = _gridResourceManager.GetString("Issued"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Issued",
                Visible = true
            };
            DataGridView2.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Workstation",
                HeaderText = _gridResourceManager.GetString("Workstation"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                Name = "Workstation",
                Visible = true
            };
            DataGridView2.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "UserId",
                HeaderText = _gridResourceManager.GetString("UserId"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "UserId",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Visible = false
            };
            DataGridView2.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ActionCodeId",
                HeaderText = _gridResourceManager.GetString("ActionCodeId"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "ActionCodeId",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Visible = false
            };
            DataGridView2.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "WorkstationId",
                HeaderText = _gridResourceManager.GetString("WorkstationId"),
                Visible = false,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "WorkstationId",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView2.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ActionCode",
                HeaderText = _gridResourceManager.GetString("ActionCode"),
                Visible = false,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "ActionCode",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView2.Columns.Add(col);

            DataGridView2.EnableHeadersVisualStyles = false;
            DataGridView2.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            DataGridView2.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);

            //foreach (DataGridViewColumn column in DataGridView2.Columns)
            //{
            //    column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //    column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            //}
        }
        private void ButtonCheckAllUsers_Click(object sender, EventArgs e)
        {
            _checkAllUsers = true;
            SelectAllUserCheckBoxes(checkThem: true);
            var userIds = GetUserIds();
            var codes = GetCodes();
            GetData(userIds, codes);
            _checkAllUsers = false;
        }
        private void ButtonClearAllUsers_Click(object sender, EventArgs e)
        {
            _clearAllUsers = true;
            SelectAllUserCheckBoxes(checkThem: false);
            ClearAll();
            _clearAllUsers = false;
        }
        private void SelectAllUserCheckBoxes(bool checkThem)
        {
            for (var i = 0; i < (CheckedListBoxUsers.Items.Count); i++)
            {
                CheckedListBoxUsers.SetItemCheckState(i, checkThem ? CheckState.Checked : CheckState.Unchecked);
            }
        }
        private void ButtonCheckAllActions_Click(object sender, EventArgs e)
        {
            _checkAllActions = true;
            SelectAllActionCheckBoxes(checkThem: true);
            var userIds = GetUserIds();
            var codes = GetCodes();
            GetData(userIds, codes);
            _checkAllActions = false;
        }
        private void ButtonClearAllActions_Click(object sender, EventArgs e)
        {
            _clearAllActions = true;
            SelectAllActionCheckBoxes(checkThem: false);
            ClearAll();
            _clearAllActions = false;
        }
        private void SelectAllActionCheckBoxes(bool checkThem)
        {
            for (var i = 0; i < (CheckedListBoxActionCodes.Items.Count); i++)
            {
                CheckedListBoxActionCodes.SetItemCheckState(i, checkThem ? CheckState.Checked : CheckState.Unchecked);
            }
        }
        private List<string> GetUserIds()
        {
            //foreach (User item in CheckedListBoxUsers.CheckedItems)
            //{
            //    userList.Add(item.Id.ToString());
            //}
            return (from User item in CheckedListBoxUsers.CheckedItems select item.Id.ToString()).ToList();
        }
        private List<string> GetCodes()
        {
            return (from KeyValuePair<int, string> item in CheckedListBoxActionCodes.CheckedItems select item.Key.ToString()).ToList();
            //var codes = new List<string>();
            //foreach (KeyValuePair<int, string> item in CheckedListBoxActionCodes.CheckedItems)
            //{
            //    codes.Add(item.Key.ToString());
            //}
            //return codes;
        }
        //private DateTime GetToDate()
        //{
        //    var toDate = new DateTime();
        //    var today = DateTime.Now;
        //    if (RadioButtonToday.Checked)
        //    {
        //        toDate = new DateTime(today.Year, today.Month, today.Day, 23, 59, 59, 500);
        //    }
        //    else if (RadioButtonWeek.Checked)
        //    {
        //        var date = today.LastDayOfWeek();
        //        toDate = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 500);
        //    }
        //    else if (RadioButtonMonth.Checked)
        //    {
        //        // var date = DateTimePickerFrom.Value;
        //        var lastDay = today.LastDayOfMonth();
        //        toDate = new DateTime(lastDay.Year, lastDay.Month, lastDay.Day, 23, 59, 59, 500);
        //    }
        //    else if (RadioButtonDateRange.Checked)
        //    {
        //        var date = DateTimePickerTo.Value;
        //        toDate = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 500);
        //    }
        //    return toDate;
        //}
        //private DateTime GetFromDate()
        //{
        //    var fromDate = new DateTime();
        //    var today = DateTime.Now;
        //    if (RadioButtonToday.Checked)
        //    {
        //        fromDate = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0, 0);
        //    }
        //    else if (RadioButtonWeek.Checked)
        //    {
        //        var date = today.FirstDayOfWeek();
        //        fromDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0);
        //    }
        //    else if (RadioButtonMonth.Checked)
        //    {
        //        var date = today.FirstDayOfMonth();
        //        fromDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0);
        //    }
        //    else if (RadioButtonDateRange.Checked)
        //    {
        //        var date = DateTimePickerFrom.Value;
        //        fromDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0);
        //    }
        //    return fromDate;
        //}
        /// <summary>
        /// Returns the first day of the week that the specified
        /// date is in using the current culture. 
        /// </summary>
        public static DateTime GetFirstDayOfWeek(DateTime dayInWeek)
        {
            var defaultCultureInfo = CultureInfo.CurrentCulture;
            return GetFirstDayOfWeek(dayInWeek, defaultCultureInfo);
        }
        /// <summary>
        /// Returns the first day of the week that the specified date 
        /// is in. 
        /// </summary>
        public static DateTime GetFirstDayOfWeek(DateTime dayInWeek, CultureInfo cultureInfo)
        {
            DayOfWeek firstDay = cultureInfo.DateTimeFormat.FirstDayOfWeek;
            DateTime firstDayInWeek = dayInWeek.Date;
            while (firstDayInWeek.DayOfWeek != firstDay)
                firstDayInWeek = firstDayInWeek.AddDays(-1);
            return firstDayInWeek;
        }
        private void MBSaveHistory_Click(object sender, EventArgs e)
        {
            CsvUtility.SaveToCsv(DataGridView1);
        }
        private void MBSaveSummary_Click(object sender, EventArgs e)
        {
            CsvUtility.SaveToCsv(DataGridView1);
        }
        private void MBSaveDetail_Click(object sender, EventArgs e)
        {
            CsvUtility.SaveToCsv(DataGridView2);
        }
        private void MButtonRun_Click(object sender, EventArgs e)
        {
            var userIds = GetUserIds();
            var codes = GetCodes();
            _currentFromDateTime = DateTimePickerFrom.Value;
            var date = DateTimePickerTo.Value;
            _currentToDateTime = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
            GetData(userIds, codes);
        }
        private void GetData()
        {
            var userIds = GetUserIds();
            var codes = GetCodes();
            GetData(userIds, codes);
        }
        private void UpdateSummaryTotals(List<ProductivitySummary> recs)
        {
            TextBoxTotalLinesSummary.Text = recs.Sum(r => r.Lines).ToString();
            TextBoxTotalPiecesSummary.Text = recs.Sum(r => r.Pieces).ToString();
            TextBoxTotalOrdersSummary.Text = recs.Sum(r => r.Orders).ToString();
        }
        private void UpdateDetailGrid()
        {
            var currentItem = ((ObjectView<ProductivitySummary>)_bindingSourceSummary.Current).Object;
            var recs = GetProductivityDetailRecords(currentItem.ActionCodeId, _fromDate, _toDate, currentItem.UserId,
                currentItem.WorkstationId);
            var blv = new BindingListView<ProductivityDetail>(recs);
            _bindingSourceDetail = new BindingSource { DataSource = blv };
            DataGridView2.DataSource = _bindingSourceDetail;
            DataGridView2.Refresh();
            if (_bindingSourceDetail.Count > 0)
            {
                UpdateDetailTotals(recs);
            }
            DataGridView2.ClearSelection();
        }
        private void UpdateDetailTotals(List<ProductivityDetail> recs)
        {
            TextBoxTotalLinesDetail.Text = recs.Count.ToString();
            TextBoxTotalPiecesDetail.Text = recs.Sum(r => r.Issued).ToString();
            TextBoxTotalOrdersDetail.Text = recs.Select(r => r.OrderId).Distinct().Count().ToString();
        }
        private List<ProductivityDetail> GetProductivityDetailRecords(int currentItemActionCodeId, DateTime fromDate,
            DateTime toDate, int currentItemUserId, int currentItemStationId)
        {
            var details = new List<ProductivityDetail>();
            using (var context = new NeutronDb())
            {
                var paramCodes = new SqlParameter("@CodeId", currentItemActionCodeId);
                var paramFromDate = new SqlParameter("@FromDate", fromDate);
                var paramToDate = new SqlParameter("@ToDate", toDate);
                var paramFind = new SqlParameter("@UserId", currentItemUserId);
                var paramStation = new SqlParameter("@StationId", currentItemStationId);
                var parameters = new object[] { paramCodes, paramFromDate, paramToDate, paramFind, paramStation };
                try
                {
                    var det = context.Database.SqlQuery<ProductivityDetail>(
                        "usp_GetProductivityDetail @CodeId, @FromDate, @ToDate, @UserId, @StationId", parameters);
                    if (det != null)
                    {
                        details = det.ToList();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error Connecting to SQL Server (ALL).  {ex.Message} {Environment.NewLine} {ex.InnerException}");
                }
            }
            return details;
        }
        public List<ProductivitySummary> GetProductivitySummaryRecords(string codes, DateTime fromDate, DateTime toDate,
            string userIds)
        {
            var summary = new List<ProductivitySummary>();
            using (var context = new NeutronDb())
            {
                var paramCodes = new SqlParameter("@Codes", codes);
                var paramFromDate = new SqlParameter("@FromDate", fromDate);
                var paramToDate = new SqlParameter("@ToDate", toDate);
                var paramFind = new SqlParameter("@UserIds", userIds);
                var parameters = new object[] { paramCodes, paramFromDate, paramToDate, paramFind };
                try
                {
                    var summ = context.Database.SqlQuery<ProductivitySummary>(
                        "usp_GetProductivitySummary @Codes, @FromDate, @ToDate, @UserIds", parameters);
                    if (summ != null)
                    {
                        summary = summ.ToList();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error Connecting to SQL Server (ALL).  {ex.Message} {Environment.NewLine} {ex.InnerException}");
                }
            }
            return summary;
        }
        private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // UpdateDetailGrid();
        }
        private void SplitContainer1_SplitterMoving(object sender, SplitterCancelEventArgs e)
        {
            //var gridBottom = DataGridView1.Height;
            //           var buttonYLocation = gridBottom + 9;
            //           var point = new Point(ButtonPrintSummary.Location.X, buttonYLocation);
            //           ButtonPrintSummary.Location = point;
        }
        private void SplitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            var yValue = DataGridView1.Height + 10;
            ButtonPrintSummary.Location = new Point { X = ButtonPrintSummary.Location.X, Y = yValue };
            LabelTotalLines.Location = new Point { X = LabelTotalLines.Location.X, Y = yValue + 3 };
            TextBoxTotalLinesSummary.Location = new Point { X = TextBoxTotalLinesSummary.Location.X, Y = yValue };
            LabelTotalPieces.Location = new Point { X = LabelTotalPieces.Location.X, Y = yValue + 3 };
            TextBoxTotalPiecesSummary.Location = new Point { X = TextBoxTotalPiecesSummary.Location.X, Y = yValue };
            LabelTotalOrders.Location = new Point { X = LabelTotalOrders.Location.X, Y = yValue + 3 };
            TextBoxTotalOrdersSummary.Location = new Point { X = TextBoxTotalOrdersSummary.Location.X, Y = yValue };
            //Detail Panel
            yValue = DataGridView2.Height + 10;
            ButtonPrintDetail.Location = new Point { X = ButtonPrintDetail.Location.X, Y = yValue };
            LabelTotalLinesDetail.Location = new Point { X = LabelTotalLinesDetail.Location.X, Y = yValue + 3 };
            TextBoxTotalLinesDetail.Location = new Point { X = TextBoxTotalLinesDetail.Location.X, Y = yValue };
            LabelTotalPiecesDetail.Location = new Point { X = LabelTotalPiecesDetail.Location.X, Y = yValue + 3 };
            TextBoxTotalPiecesDetail.Location = new Point { X = TextBoxTotalPiecesDetail.Location.X, Y = yValue };
            LabelTotalOrdersDetail.Location = new Point { X = LabelTotalOrdersDetail.Location.X, Y = yValue + 3 };
            TextBoxTotalOrdersDetail.Location = new Point { X = TextBoxTotalOrdersDetail.Location.X, Y = yValue };
        }
        private void SplitContainer2_SplitterMoved(object sender, SplitterEventArgs e)
        {
            //Users
            var yValue = CheckedListBoxUsers.Height + CheckedListBoxUsers.Location.Y + 8;
            var yValueConfigure = CheckedListBoxUsers.Height + CheckedListBoxUsers.Location.Y + 36;
            ButtonCheckAllUsers.Location = new Point { X = ButtonCheckAllUsers.Location.X, Y = yValue };
            ButtonClearAllUsers.Location = new Point { X = ButtonClearAllUsers.Location.X, Y = yValue };
            ButtonConfigureUsers.Location = new Point { X = ButtonConfigureUsers.Location.X, Y = yValueConfigure };
            //Actions
            yValue = CheckedListBoxActionCodes.Height + CheckedListBoxActionCodes.Location.Y + 8;
            yValueConfigure = CheckedListBoxActionCodes.Height + CheckedListBoxActionCodes.Location.Y + 36;
            ButtonCheckAllActions.Location = new Point { X = ButtonCheckAllActions.Location.X, Y = yValue };
            ButtonClearAllActions.Location = new Point { X = ButtonClearAllActions.Location.X, Y = yValue };
            ButtonConfigureActions.Location = new Point { X = ButtonConfigureActions.Location.X, Y = yValueConfigure };
            yValue = CheckedListBoxUsers.Location.Y - 8;
            CheckedListBoxGroups.Height = yValue;
        }
        private void ButtonConfigureUsers_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmDefineUserGroup(_jsonData))
            {
                frm.ShowDialog();
                Show();
                _groupItemCheckEnabled = false;
                SetupCheckedListBoxGroups();
                _groupItemCheckEnabled = true;
                // SetupCheckedListBoxUsers();
                UpdateCheckedListBoxUsers();
            }
        }
        private void CheckedListBoxUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            //GetData();
        }
        private void ClearAll()
        {
            DataGridView1.DataSource = null;
            DataGridView2.DataSource = null;
            TextBoxTotalLinesDetail.Text = string.Empty;
            TextBoxTotalLinesSummary.Text = string.Empty;
            TextBoxTotalOrdersDetail.Text = string.Empty;
            TextBoxTotalOrdersSummary.Text = string.Empty;
            TextBoxTotalPiecesDetail.Text = string.Empty;
            TextBoxTotalPiecesSummary.Text = string.Empty;
        }
        private void DataGridView1_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            _bindingSourceSummary.Position = e.RowIndex;
            UpdateDetailGrid();
        }
        private void GetData(List<string> checkedUserItems, List<string> checkedActionCodes)
        {
            if (checkedUserItems.Count > 0)
            {
                if (checkedActionCodes.Count > 0)
                {
                    _fromDate = _currentFromDateTime;  // GetFromDate();
                    _toDate = _currentToDateTime;  // GetToDate();

                    var codes = string.Join(",", checkedActionCodes);
                    var userIds = string.Join(",", checkedUserItems);
                    var recs = GetProductivitySummaryRecords(codes, _fromDate, _toDate, userIds);
                    if (recs.Count > 0)
                    {
                        var blv = new BindingListView<ProductivitySummary>(recs);
                        _bindingSourceSummary = new BindingSource { DataSource = blv };
                        DataGridView1.DataSource = _bindingSourceSummary;
                        DataGridView1.Refresh();
                        UpdateSummaryTotals(recs);
                        UpdateDetailGrid();
                    }
                    else
                    {
                        ClearAll();
                    }
                }
                else
                {
                    ClearAll();
                }
            }
            else
            {
                ClearAll();
            }
        }
        private void ButtonConfigureActions_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmDefineActionGroup(_jsonData, _actionCodes, _currentIDs))
            {
                frm.ShowDialog();
                Show();
                SetupCheckedListBoxActionCodes();
            }
        }
        private void CheckedListBoxActionCodes_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (!_formInitialized) return;

            var checkedItems = new List<string>();
            foreach (KeyValuePair<int, string> item in CheckedListBoxActionCodes.CheckedItems)
                checkedItems.Add(item.Key.ToString());
            if (e.NewValue == CheckState.Checked)
                checkedItems.Add(((KeyValuePair<int, string>)CheckedListBoxActionCodes.Items[e.Index]).Key.ToString());
            else
                checkedItems.Remove(((KeyValuePair<int, string>)CheckedListBoxActionCodes.Items[e.Index]).Key.ToString());
            if (_checkAllActions || _clearAllActions) return;
            var userIds = GetUserIds();
            GetData(userIds, checkedItems);
        }

        private void ButtonPrintSummary_Click(object sender, EventArgs e)
        {
            var totalLines = TextBoxTotalLinesSummary.Text.ParseInt();
            var totalPieces = TextBoxTotalPiecesSummary.Text.ParseInt();
            var totalOrders = TextBoxTotalOrdersSummary.Text.ParseInt();
            var summaryList = new List<ProductivitySummary>();
            _documentPrinter = _jsonData.LoadFile<DocumentPrinterPreferences>();
            foreach (var item in _bindingSourceSummary)
            {
                var rec = ((ObjectView<ProductivitySummary>)item).Object;
                rec.TotalLines = totalLines;
                rec.TotalPieces = totalPieces;
                rec.TotalOrders = totalOrders;
                summaryList.Add(rec);
            }
            _documentToPrint.PrintSummary(summaryList, _documentPrinter, _neutronVariables.PrintPreview);
        }
        private void ButtonPrintDetail_Click(object sender, EventArgs e)
        {
            foreach (var item in _bindingSourceSummary)
            {
                var currentItem = ((ObjectView<ProductivitySummary>)item).Object;
                var recs = GetProductivityDetailRecords(currentItem.ActionCodeId, _fromDate, _toDate, currentItem.UserId,
                    currentItem.AreaId);
                var totalLines = recs.Count;
                var totalPieces = recs.Sum(r => r.Issued);
                var totalOrders = recs.Select(r => r.OrderId).Distinct().Count();
                var blv = new BindingListView<ProductivityDetail>(recs);
                _bindingSourceDetail = new BindingSource { DataSource = blv };
                var detailList = new List<ProductivityDetail>();
                _documentPrinter = _jsonData.LoadFile<DocumentPrinterPreferences>();
                foreach (var det in _bindingSourceDetail)
                {
                    var rec = ((ObjectView<ProductivityDetail>)det).Object;
                    rec.TotalLines = totalLines;
                    rec.TotalPieces = totalPieces;
                    rec.TotalOrders = totalOrders;
                    detailList.Add(rec);
                }
                _documentToPrint.PrintDetail(detailList, _documentPrinter, _neutronVariables.PrintPreview);
            }
        }
        private void DateTimePicker_Enter(object sender, EventArgs e)
        {
            RadioButtonDateRange.Checked = true;
        }
        private void DateTimePickerFrom_ValueChanged(object sender, EventArgs e)
        {
            if (!_formInitialized) return;
            var date = DateTimePickerFrom.Value;
            if (DateTimePickerTo.Value < DateTimePickerFrom.Value)
            {
                DateTimePickerTo.Value = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
            }
            DateTimePickerFrom.Value = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
            _currentFromDateTime = DateTimePickerFrom.Value;
            _currentToDateTime = DateTimePickerTo.Value;
            GetData();
        }
        private void RadioButtonDate(object sender, EventArgs e)
        {
            GetData();
        }
        private void DateTimePickerTo_ValueChanged(object sender, EventArgs e)
        {
            if (!_formInitialized) return;

            var date = DateTimePickerTo.Value;

            if (DateTimePickerTo.Value < DateTimePickerFrom.Value)
            {
                DateTimePickerFrom.Value = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
            }
            DateTimePickerTo.Value = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
            _currentFromDateTime = DateTimePickerFrom.Value;
            _currentToDateTime = DateTimePickerTo.Value;
            GetData();
        }
        private void CheckedListBoxGroups_ItemCheck(object sender, ItemCheckEventArgs e)
        {

            if (!_formInitialized || !_groupItemCheckEnabled) return;

            if (e.NewValue != CheckState.Checked)
            {
                _currentGroup = null;
                UpdateCheckedListBoxUsers();
                GetData(new List<string>(), GetCodes());
                _groupItemCheckEnabled = true;
                return;
            }
            _checkAllUsers = true;
            var selectedIndexes = CheckedListBoxGroups.CheckedIndices;
            if (selectedIndexes.Count > 0)
            {
                _groupItemCheckEnabled = false;
                CheckedListBoxGroups.SetItemChecked(selectedIndexes[0], false);
                _groupItemCheckEnabled = true;
            }
            _currentGroup = (ProductivityGroup)CheckedListBoxGroups.SelectedItem;
            UpdateCheckedListBoxUsers();
            var checkedItems = _currentGroup.UserIdString.CsvIdString.Split(',').ToList();
            var codes = GetCodes();
            GetData(checkedItems, codes);
            _checkAllUsers = false;
        }
        private void CheckedListBoxUsers_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (!_formInitialized || !_userItemCheckEnabled) return;
            var checkedItems = new List<string>();
            foreach (User item in CheckedListBoxUsers.CheckedItems)
                checkedItems.Add(item.Id.ToString());
            if (e.NewValue == CheckState.Checked)
                checkedItems.Add(((User)CheckedListBoxUsers.Items[e.Index]).Id.ToString());
            else
                checkedItems.Remove(((User)CheckedListBoxUsers.Items[e.Index]).Id.ToString());
            if (_checkAllUsers || _clearAllUsers) return;
            var codes = GetCodes();
            GetData(checkedItems, codes);
        }
        private void UpdateCheckedListBoxUsers()
        {
            CheckedListBoxUsers.Items.Clear();
            if (_currentGroup == null) return;
            var currentUserIds = _currentGroup.UserIdString.CsvIdString;
            if (string.IsNullOrEmpty(currentUserIds)) return;
            var nums = currentUserIds.Split(',').Select(int.Parse).ToArray();
            if (nums.Length <= 0) return;
            List<User> users;
            using (var db = new NeutronDb())
            {
                users = db.Users.Where(r => nums.Contains(r.Id)).OrderBy(o => o.Lastname).ToList();
            }
            foreach (var user in users)
            {
                CheckedListBoxUsers.Items.Add(user);
            }
            for (var i = 0; i < CheckedListBoxUsers.Items.Count; i++)
            {
                CheckedListBoxUsers.SetItemChecked(i, true);
            }
        }

        private void RadioButtonToday_CheckedChanged(object sender, EventArgs e)
        {
            if (!((RadioButton)sender).Checked) return;
            var date = DateTime.Now;
            DateTimePickerFrom.Value = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
            DateTimePickerTo.Value = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
            _currentFromDateTime = DateTimePickerFrom.Value;
            _currentToDateTime = DateTimePickerTo.Value;
            GetData();
        }

        private void RadioButtonWeek_CheckedChanged(object sender, EventArgs e)
        {
            if (!((RadioButton)sender).Checked) return;
            var date = DateTime.Now;
            //var firstDay = date.FirstDayOfWeek();
            //var lastDay = date.LastDayOfWeek();
            DateTimePickerFrom.Value = date.FirstDayOfWeek(); // new DateTime(firstDay.Year, firstDay.Month, firstDay.Day, 0, 0, 0);
            DateTimePickerTo.Value = date.LastDayOfWeek();  // new DateTime(lastDay.Year, lastDay.Month, lastDay.Day, 23, 59, 59);
            _currentFromDateTime = DateTimePickerFrom.Value;
            _currentToDateTime = DateTimePickerTo.Value;
            GetData();
        }

        private void RadioButtonMonth_CheckedChanged(object sender, EventArgs e)
        {
            if (!((RadioButton)sender).Checked) return;
            var date = DateTime.Now;
            // var firstDay = date.FirstDayOfMonth();
            // var lastDay = date.LastDayOfMonth();
            DateTimePickerFrom.Value = date.FirstDayOfMonth(); // new DateTime(firstDay.Year, firstDay.Month, firstDay.Day, 0, 0, 0);
            DateTimePickerTo.Value = date.LastDayOfMonth(); // new DateTime(lastDay.Year, lastDay.Month, lastDay.Day, 23, 59, 59);
            _currentFromDateTime = DateTimePickerFrom.Value;
            _currentToDateTime = DateTimePickerTo.Value;
            GetData();
        }

        private void RadioButtonDateRange_CheckedChanged(object sender, EventArgs e)
        {
            if (!((RadioButton)sender).Checked) return;
            //var date = DateTime.Now;
            //var firstDay = date.FirstDayOfMonth();
            //DateTimePickerFrom.Value = new DateTime(firstDay.Year, firstDay.Month, firstDay.Day, 0, 0, 0);
            //DateTimePickerTo.Value = date;
            _currentFromDateTime = DateTimePickerFrom.Value;
            _currentToDateTime = DateTimePickerTo.Value;
            GetData();
        }

        private void DateTimePickerFrom_Enter(object sender, EventArgs e)
        {
            RadioButtonDateRange.Checked = true;
        }

        private void DateTimePickerTo_Enter(object sender, EventArgs e)
        {
            RadioButtonDateRange.Checked = true;
        }

        private void SetCulture(string lang)
        {
            try
            {
                var languageDirectory = LoaderSettings.GetLanguageDirectory();
                _cultureInfo = CultureInfo.CreateSpecificCulture(lang);
                _resourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "FrmProductivity",
               resourceDir: languageDirectory, usingResourceSet: null);
                _gridResourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "GridHeaders",
                    resourceDir: languageDirectory, usingResourceSet: null);
                _enumResourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "EnumDescriptions",
                    resourceDir: languageDirectory, usingResourceSet: null);
                ButtonConfigureUsers.Text = _resourceManager.GetString("ConfigureUsers");
                ButtonClearAllUsers.Text = _resourceManager.GetString("ClearAll");
                ButtonCheckAllUsers.Text = _resourceManager.GetString("CheckAll");
                ButtonConfigureActions.Text = _resourceManager.GetString("ConfigureActions");
                ButtonClearAllActions.Text = _resourceManager.GetString("ClearAll");
                ButtonCheckAllActions.Text = _resourceManager.GetString("CheckAll");
                ButtonPrintSummary.Text = _resourceManager.GetString("PrintSummary");
                LabelTotalOrders.Text = _resourceManager.GetString("TotalOrders");
                LabelTotalPieces.Text = _resourceManager.GetString("TotalPieces");
                LabelTotalLines.Text = _resourceManager.GetString("TotalLines");
                ButtonPrintDetail.Text = _resourceManager.GetString("PrintDetail");
                LabelTotalOrdersDetail.Text = _resourceManager.GetString("TotalOrders");
                LabelTotalPiecesDetail.Text = _resourceManager.GetString("TotalPieces");
                LabelTotalLinesDetail.Text = _resourceManager.GetString("TotalLines");
                LabelTo.Text = _resourceManager.GetString("To");
                LabelFrom.Text = _resourceManager.GetString("From");
                RadioButtonDateRange.Text = _resourceManager.GetString("DateRange");
                RadioButtonMonth.Text = _resourceManager.GetString("Month");
                RadioButtonWeek.Text = _resourceManager.GetString("Week");
                RadioButtonToday.Text = _resourceManager.GetString("Today");
                MButtonClose.Text = _resourceManager.GetString("Close");
                MBSaveDetail.Text = _resourceManager.GetString("SaveDetailtoFile");
                MBSaveSummary.Text = _resourceManager.GetString("SaveSummarytoFile");
                MButtonRun.Text = _resourceManager.GetString("Refresh");
                LabelFormTitle.Text = _resourceManager.GetString("Productivity");
                LabelFormHeaderText.Text = _resourceManager.GetString("NeutronWarehouseMana");
                Text = _resourceManager.GetString("Productivity");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading language file.  { ex.Message} { Environment.NewLine} { ex.InnerException} ");
            }
        }
    }
}
