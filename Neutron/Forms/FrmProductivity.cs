using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using NeutronCore.Global;
using PrintRequest;

namespace Neutron.Forms
{
    public partial class FrmProductivity : MetroForm
    {
        private BindingSource _bindingSourceSummary;
        private BindingSource _bindingSourceDetail;
        private DateTime _fromDate;
        private DateTime _toDate;
        private readonly IJsonData _jsonData;
        private DocumentPrinterPreferences _documentPrinter;
        private ProductivityGroup _currentGroup = null;
        private bool _formInitialized;
        private bool _groupItemCheckEnabled = true;
        private bool _userItemCheckEnabled = true;
        private readonly string _fileName = "ProductivityGroups";

        public FrmProductivity(IJsonData jsonData)
        {
            _jsonData = jsonData;
            InitializeComponent();
            HideTabControlTabs();
            DisableEvents();

            SetupCheckedListBoxGroups();
            SetupCheckedListBoxActionCodes();
            SetupGrids();
            SetInitialDateTimePickers();

            SetupCheckedListBoxUsers();

            EnableEvents();
            mlUserInfo.Text = GlobalVar.User?.UserInfo;
            _formInitialized = true;
        }

        private void EnableEvents()
        {
            CheckedListBoxUsers.ItemCheck += new ItemCheckEventHandler(this.CheckedListBoxUsers_ItemCheck);
        }

        private void DisableEvents()
        {
            CheckedListBoxUsers.ItemCheck -= new ItemCheckEventHandler(this.CheckedListBoxUsers_ItemCheck);
        }

        private void SetInitialDateTimePickers()
        {
            var today = DateTime.Today;
            DateTimePickerFrom.Value = today.FirstDayOfMonth();
            DateTimePickerTo.Value = today;
        }

        private void HideTabControlTabs()
        {
            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(0, 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;
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

            var actionCodes = ((ActionCode[])Enum.GetValues(typeof(ActionCode)))
                .Select(r => new EnumModel() { Id = (int)r, Name = r.GetEnumDescription() }).ToList();

            var currentIds = _jsonData.LoadFile<ActionIdString>().CsvIdString;
            if (!string.IsNullOrEmpty(currentIds))
            {
                var nums = currentIds.Split(',').Select(int.Parse).ToArray();
                if (nums.Length > 0)
                {
                    CheckedListBoxActionCodes.DataSource = actionCodes.Where(r => nums.Contains(r.Id)).OrderBy(o => o.Name).ToList();
                    CheckedListBoxActionCodes.DisplayMember = "Name";
                    CheckedListBoxActionCodes.ValueMember = "Id";
                }
            }
            else
            {
                CheckedListBoxActionCodes.DataSource = new BindingSource(actionCodes, null);
                CheckedListBoxActionCodes.DisplayMember = "Name";
                CheckedListBoxActionCodes.ValueMember = "Id";
            }
        }
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
                HeaderText = @"Date",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Date",
                Visible = true
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Employee",
                HeaderText = @"Employee",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Employee",
                Visible = true
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Action",
                HeaderText = @"Action",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Action",
                Visible = true
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Lines",
                HeaderText = @"Lines",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Lines",
                Visible = true
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Pieces",
                HeaderText = @"Pieces",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Pieces",
                Visible = true
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Orders",
                HeaderText = @"Orders",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Orders",
                Visible = true
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Station",
                HeaderText = @"Station",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                Name = "Station",
                Visible = true
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "UserId",
                HeaderText = @"UserId",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "UserId",
                Visible = false
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ActionCodeId",
                HeaderText = @"ActionCodeId",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "ActionCodeId",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Visible = false
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StationId",
                HeaderText = @"StationId",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "StationId",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Visible = false
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ActionCode",
                HeaderText = @"ActionCode",
                Visible = false,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "ActionCode",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(col);

            foreach (DataGridViewColumn column in DataGridView1.Columns)
            {
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            }

            //DataGridView2

            DataGridView2.AutoGenerateColumns = false;
            DataGridView2.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridView2.DefaultCellStyle.ForeColor = Color.Black;
            DataGridView2.DefaultCellStyle.BackColor = Color.White;

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Date",
                HeaderText = @"Date",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Date",
                Visible = true
            };
            DataGridView2.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Employee",
                HeaderText = @"Employee",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Employee",
                Visible = true
            };
            DataGridView2.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Action",
                HeaderText = @"Action",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Action",
                Visible = true
            };
            DataGridView2.Columns.Add(col);


            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Order",
                HeaderText = @"Order",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Order",
                Visible = true
            };
            DataGridView2.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Reservation",
                HeaderText = @"Res",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Reservation",
                Visible = true
            };
            DataGridView2.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = @"Item",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Item",
                Visible = true
            };
            DataGridView2.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = @"Description",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Description",
                Visible = true
            };
            DataGridView2.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Requested",
                HeaderText = @"Req",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Requested",
                Visible = true
            };
            DataGridView2.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Issued",
                HeaderText = @"Iss",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Issued",
                Visible = true
            };
            DataGridView2.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Station",
                HeaderText = @"Station",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                Name = "Station",
                Visible = true
            };
            DataGridView2.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "UserId",
                HeaderText = @"UserId",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "UserId",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Visible = false
            };
            DataGridView2.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ActionCodeId",
                HeaderText = @"ActionCodeId",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "ActionCodeId",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Visible = false
            };
            DataGridView2.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StationId",
                HeaderText = @"Station",
                Visible = false,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "StationId",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView2.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ActionCode",
                HeaderText = @"ActionCode",
                Visible = false,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "ActionCode",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView2.Columns.Add(col);

            foreach (DataGridViewColumn column in DataGridView2.Columns)
            {
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            }

        }



        //private void ButtonCheckAll_Click(object sender, EventArgs e)
        //{
        //    SelectAllCheckBoxes(checkThem: true);
        //}

        //private void ButtonClearAll_Click(object sender, EventArgs e)
        //{
        //    SelectAllCheckBoxes(checkThem: false);
        //}

        //private void SelectAllCheckBoxes(bool checkThem)
        //{
        //    for (var i = 0; i <= (CheckedListBoxActionCodes.Items.Count - 1); i++)
        //    {
        //        CheckedListBoxActionCodes.SetItemCheckState(i, checkThem ? CheckState.Checked : CheckState.Unchecked);
        //    }
        //}


        private void ButtonCheckAllUsers_Click(object sender, EventArgs e)
        {
            SelectAllUserCheckBoxes(checkThem: true);
            var userIds = GetUserIds();
            var codes = GetCodes();
            GetData(userIds, codes);
        }

        private void ButtonClearAllUsers_Click(object sender, EventArgs e)
        {
            SelectAllUserCheckBoxes(checkThem: false);
            ClearAll();
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
            SelectAllActionCheckBoxes(checkThem: true);
            var userIds = GetUserIds();
            var codes = GetCodes();
            GetData(userIds, codes);
        }

        private void ButtonClearAllActions_Click(object sender, EventArgs e)
        {
            SelectAllActionCheckBoxes(checkThem: false);
            ClearAll();
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
            var userList = new List<string>();

            foreach (User item in CheckedListBoxUsers.CheckedItems)
            {
                userList.Add(item.Id.ToString());
            }
            return userList;
        }

        private List<string> GetCodes()
        {
            var codes = new List<string>();

            foreach (EnumModel item in CheckedListBoxActionCodes.CheckedItems)
            {
                codes.Add(item.Id.ToString());
            }
            return codes;
        }

        private DateTime GetToDate()
        {
            var toDate = new DateTime();
            var today = DateTime.Now;

            if (RadioButtonToday.Checked)
            {
                toDate = new DateTime(today.Year, today.Month, today.Day, 23, 59, 59, 500);
            }
            else if (RadioButtonWeek.Checked)
            {
                var date = today.LastDayOfWeek();
                toDate = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 500);
            }
            else if (RadioButtonMonth.Checked)
            {
                // var date = DateTimePickerFrom.Value;
                var lastDay = today.LastDayOfMonth();
                toDate = new DateTime(lastDay.Year, lastDay.Month, lastDay.Day, 23, 59, 59, 500);
            }
            else if (RadioButtonDateRange.Checked)
            {
                var date = DateTimePickerTo.Value;
                toDate = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 500);
            }

            return toDate;
        }

        private DateTime GetFromDate()
        {
            var fromDate = new DateTime();
            var today = DateTime.Now;

            if (RadioButtonToday.Checked)
            {
                fromDate = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0, 0);
            }
            else if (RadioButtonWeek.Checked)
            {
                var date = today.FirstDayOfWeek();
                fromDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0);
            }
            else if (RadioButtonMonth.Checked)
            {
                var date = today.FirstDayOfMonth();
                fromDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0);
            }
            else if (RadioButtonDateRange.Checked)
            {
                var date = DateTimePickerFrom.Value;
                fromDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0);
            }

            return fromDate;
        }

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
                currentItem.StationId);
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
                    MessageBox.Show($"Error Connecting to SQL Server (ALL).  {ex.Message} \r\n {ex.InnerException}");
                }
            }

            return details;
        }

        //public List<HistoryView> GetHistoryRecordsByUser(string empId)
        //{
        //    DateTime today = DateTime.Now;
        //    var fromDate = new DateTime(2015, 1, 1, 23, 59, 59, 999);
        //    var toDate = new DateTime(today.Year, today.Month, today.Day, 23, 59, 59, 999);
        //    string codes = GetCodes();
        //    string find = string.Empty;

        //    var history = new List<HistoryView>();
        //    using (var context = new NeutronDb())
        //    {
        //        var paramCodes = new SqlParameter("@Codes", codes);
        //        var paramFromDate = new SqlParameter("@FromDate", fromDate);
        //        var paramToDate = new SqlParameter("@ToDate", toDate);
        //        var paramFind = new SqlParameter("@Find", find);
        //        var parameters = new object[] { paramCodes, paramFromDate, paramToDate, paramFind };
        //        try
        //        {
        //            var hist = context.Database.SqlQuery<HistoryView>("usp_GetHistoryFind @Codes, @FromDate, @ToDate, @Find", parameters);
        //            if (hist != null)
        //            {
        //                history = hist.Where(h => h.EmpId == empId).OrderByDescending(o => o.ActionDateTime).ToList();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show($"Error Connecting to SQL Server (USER).  {ex.Message} \r\n {ex.InnerException}");
        //        }
        //    }
        //    return history;
        //}

        //public List<HistoryView> GetHistoryRecords()
        //{
        //    DateTime today = DateTime.Now;
        //    var fromDate = new DateTime(2015, 1, 1, 23, 59, 59, 999);
        //    var toDate = new DateTime(today.Year, today.Month, today.Day, 23, 59, 59, 999);
        //    string codes = GetCodes();
        //    string find = string.Empty;

        //    var history = new List<HistoryView>();
        //    using (var context = new NeutronDb())
        //    {
        //        var paramCodes = new SqlParameter("@Codes", codes);
        //        var paramFromDate = new SqlParameter("@FromDate", fromDate);
        //        var paramToDate = new SqlParameter("@ToDate", toDate);
        //        var paramFind = new SqlParameter("@Find", find);
        //        var parameters = new object[] { paramCodes, paramFromDate, paramToDate, paramFind };
        //        try
        //        {
        //            var hist = context.Database.SqlQuery<HistoryView>("usp_GetHistoryFind @Codes, @FromDate, @ToDate, @Find", parameters);
        //            if (hist != null)
        //            {
        //                history = hist.ToList();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show($"Error Connecting to SQL Server (ALL).  {ex.Message} \r\n {ex.InnerException}");
        //        }
        //    }
        //    return history;
        //}

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
                    MessageBox.Show($"Error Connecting to SQL Server (ALL).  {ex.Message} \r\n {ex.InnerException}");
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
                    _fromDate = GetFromDate();
                    _toDate = GetToDate();
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
            using (var frm = new FrmDefineActionGroup(_jsonData))
            {
                frm.ShowDialog();
                Show();
                SetupCheckedListBoxActionCodes();
            }
        }

        private void CheckedListBoxActionCodes_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            var checkedItems = new List<string>();
            foreach (EnumModel item in CheckedListBoxActionCodes.CheckedItems)
                checkedItems.Add(item.Id.ToString());

            if (e.NewValue == CheckState.Checked)
                checkedItems.Add(((EnumModel)CheckedListBoxActionCodes.Items[e.Index]).Id.ToString());
            else
                checkedItems.Remove(((EnumModel)CheckedListBoxActionCodes.Items[e.Index]).Id.ToString());

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
            var neutronVariables = _jsonData.LoadFile<NeutronVariables>();
            foreach (var item in _bindingSourceSummary)
            {
                var rec = ((ObjectView<ProductivitySummary>)item).Object;
                rec.TotalLines = totalLines;
                rec.TotalPieces = totalPieces;
                rec.TotalOrders = totalOrders;
                summaryList.Add(rec);
            }
            DocumentToPrint.PrintSummary(summaryList, _documentPrinter, neutronVariables.PrintPreview);
        }

        private void ButtonPrintDetail_Click(object sender, EventArgs e)
        {
            foreach (var item in _bindingSourceSummary)
            {
                var currentItem = ((ObjectView<ProductivitySummary>)item).Object;
                var recs = GetProductivityDetailRecords(currentItem.ActionCodeId, _fromDate, _toDate, currentItem.UserId,
                    currentItem.StationId);
                var totalLines = recs.Count;
                var totalPieces = recs.Sum(r => r.Issued);
                var totalOrders = recs.Select(r => r.OrderId).Distinct().Count();
                var blv = new BindingListView<ProductivityDetail>(recs);
                _bindingSourceDetail = new BindingSource { DataSource = blv };
                var detailList = new List<ProductivityDetail>();
                _documentPrinter = _jsonData.LoadFile<DocumentPrinterPreferences>();
                var neutronVariables = _jsonData.LoadFile<NeutronVariables>();
                foreach (var det in _bindingSourceDetail)
                {
                    var rec = ((ObjectView<ProductivityDetail>)det).Object;
                    rec.TotalLines = totalLines;
                    rec.TotalPieces = totalPieces;
                    rec.TotalOrders = totalOrders;
                    detailList.Add(rec);
                }

                DocumentToPrint.PrintDetail(detailList, _documentPrinter, neutronVariables.PrintPreview);
            }
        }

        private void DateTimePicker_Enter(object sender, EventArgs e)
        {
            RadioButtonDateRange.Checked = true;
        }

        private void DateTimePickerFrom_ValueChanged(object sender, EventArgs e)
        {
            if (DateTimePickerTo.Value < DateTimePickerFrom.Value)
            {
                DateTimePickerTo.Value = DateTimePickerFrom.Value;
            }
            GetData();
        }

        private void RadioButtonDate(object sender, EventArgs e)
        {
            GetData();
        }

        private void DateTimePickerTo_ValueChanged(object sender, EventArgs e)
        {
            if (DateTimePickerTo.Value < DateTimePickerFrom.Value)
            {
                DateTimePickerTo.Value = DateTimePickerFrom.Value;
            }
            GetData();
        }

        private void CheckedListBoxGroups_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // _groupItemCheckEnabled = true;
            if (!_formInitialized || !_groupItemCheckEnabled) return;

            

            if (e.NewValue != CheckState.Checked)
            {
                _currentGroup = null;
                _groupItemCheckEnabled = true;
                return;
            }

            var selectedIndexes = CheckedListBoxGroups.CheckedIndices;
            if (selectedIndexes.Count > 0)
            {
                _groupItemCheckEnabled = false;
                CheckedListBoxGroups.SetItemChecked(selectedIndexes[0], false);
                _groupItemCheckEnabled = true;
            }

            _currentGroup = (ProductivityGroup)CheckedListBoxGroups.SelectedItem;
           // SetupCheckedListBoxUsers();
            UpdateCheckedListBoxUsers();
            var checkedItems = _currentGroup.UserIdString.CsvIdString.Split(',').ToList();

            //var checkedItems = new List<string>();
            //foreach (ProductivityGroup item in CheckedListBoxGroups.CheckedItems)
            //    checkedItems.Add(item.UserIdString.CsvIdString);


            //if (e.NewValue == CheckState.Checked)
            //    checkedItems.Add(((ProductivityGroup)CheckedListBoxGroups.Items[e.Index]).UserIdString.CsvIdString);
            //else
            //    checkedItems.Remove(((ProductivityGroup)CheckedListBoxGroups.Items[e.Index]).UserIdString.CsvIdString);






            var codes = GetCodes();

            GetData(checkedItems, codes);
        }

        private void CheckedListBoxUsers_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // _userItemCheckEnabled = true;

            if (!_formInitialized || !_userItemCheckEnabled) return;
            var checkedItems = new List<string>();
            foreach (User item in CheckedListBoxUsers.CheckedItems)
                checkedItems.Add(item.Id.ToString());

            if (e.NewValue == CheckState.Checked)
                checkedItems.Add(((User)CheckedListBoxUsers.Items[e.Index]).Id.ToString());
            else
                checkedItems.Remove(((User)CheckedListBoxUsers.Items[e.Index]).Id.ToString());

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
    }
}
