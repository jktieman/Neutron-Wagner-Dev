using MetroFramework.Forms;
using Neutron.Classes;
using NeutronCore.Extensions;
using Neutron.Global;
using NeutronData.DataContexts;
using NeutronData.Interfaces;
using NeutronData.Models;
using NeutronData.ModelViews;
using NeutronData.Repositories;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Equin.ApplicationFramework;
using Neutron.Interfaces;
using NeutronCore.Enums;

namespace Neutron.Forms
{
    public partial class FrmHistory : MetroForm
    {
       // private GenericRepository<History> _repoHistory = new GenericRepository<History>(new NeutronDb());
        readonly IAkaRepository _akaRepository;
        private readonly INomenclature _nomenclature;
        private BindingListView<HistoryView> _bindingSourceEquin;
        private readonly BindingSource _bindingSource = new BindingSource();

        public FrmHistory(IAkaRepository akaRepository, INomenclature nomenclature)
        {
            InitializeComponent();
            HideTabControlTabs();
            SetupCheckListBoxActionCodes();
            _akaRepository = akaRepository;
            _nomenclature = nomenclature;
            SetupGrids();
            mlUserInfo.Text = GlobalVar.User?.UserInfo;
        }

        private void HideTabControlTabs()
        {
            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(0, 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;
        }

        private void SetupGrids()
        {

            DataGridView1.AutoGenerateColumns = false;
            DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridView1.DefaultCellStyle.ForeColor = Color.Black;
            DataGridView1.DefaultCellStyle.BackColor = Color.White;

            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = @"Id",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Id",
                Visible = false
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ActionCode",
                HeaderText = @"Action Code",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "ActionCode",
                Visible = false
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ActionCodeName",
                HeaderText = @"Action Name",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "ActionCodeName",
                Visible = true
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ActionDateTime",
                HeaderText = @"Action Date",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "ActionDateTime",
                Visible = true
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord1",
                HeaderText = @"Order",
                DefaultCellStyle = {Alignment = DataGridViewContentAlignment.MiddleRight},
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Ord1",
                Visible = true
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord2",
                HeaderText = @"Order2",
                DefaultCellStyle = {Alignment = DataGridViewContentAlignment.MiddleRight},
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Ord2",
                Visible = true
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = @"Item",
                DefaultCellStyle = {Alignment = DataGridViewContentAlignment.MiddleCenter},
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Item",
                Visible = true
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = @"Description",
                DefaultCellStyle = {Alignment = DataGridViewContentAlignment.MiddleLeft},
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Description",
                Visible = true
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "RequestedQuantity",
                HeaderText = @"Requested",
                DefaultCellStyle = {Alignment = DataGridViewContentAlignment.MiddleLeft},
                Name = "RequestedQuantity",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Visible = true
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "IssuedQuantity",
                HeaderText = @"Issued",
                DefaultCellStyle = {Alignment = DataGridViewContentAlignment.MiddleRight},
                Name = "IssuedQuantity",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Visible = true
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StationId",
                HeaderText = @"Station",
                Visible = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "StationId",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc1",
                HeaderText = _nomenclature.LabelDevice,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Loc1",
                Visible = true
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc2",
                HeaderText = _nomenclature.LabelTray,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Loc2",
                Visible = true
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc3",
                HeaderText = _nomenclature.LabelOver,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Loc3",
                Visible = true
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc4",
                HeaderText = _nomenclature.LabelBack,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Loc4",
                Visible = true
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc5",
                HeaderText = @"Tag",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Loc5",
                Visible = true
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Slot",
                HeaderText = @"Slot",
                Visible = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Slot"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderId",
                HeaderText = @" Order Id",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "OrderId",
                Visible = false
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderDetailId",
                HeaderText = @"Order Detail Id",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "OrderDetailId",
                Visible = false
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "EmpId",
                HeaderText = @"EmpId",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "EmpId",
                Visible = false
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "EmployeeName",
                DefaultCellStyle = {Alignment = DataGridViewContentAlignment.MiddleRight},
                HeaderText = @"Employee Name",
                Name = "EmployeeName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Visible = true
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CostCenter",
                HeaderText = @"Cost Center",
                DefaultCellStyle = {Alignment = DataGridViewContentAlignment.MiddleCenter},
                Name = "CostCenter",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Visible = true
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TransmitDate",
                HeaderText = @"Transmit DateTime",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "TransmitDate",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Visible = true
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderInfo",
                HeaderText = @"Order Info",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "OrderInfo",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Visible = true
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderDetailInfo",
                HeaderText = @"Order Detail Info",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "OrderDetailInfo",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Visible = true
            };
            DataGridView1.Columns.Add(col);
        }

        private void SetupCheckListBoxActionCodes()
        {
            var actionCodes = ((ActionCode[]) Enum.GetValues(typeof(ActionCode))).ToList();
            var codes = new Dictionary<int, string>();
            foreach (var code in actionCodes)
            {
                codes.Add((int)code, code.GetEnumDescription());
            }
            CheckedListBoxActionCodes.DataSource = new BindingSource(codes, null);
            CheckedListBoxActionCodes.DisplayMember = "Value";
            CheckedListBoxActionCodes.ValueMember = "Key";
        }

        private void ButtonCheckAll_Click(object sender, EventArgs e)
        {
            SelectAllCheckBoxes(checkThem: true);
        }

        private void ButtonClearAll_Click(object sender, EventArgs e)
        {
            SelectAllCheckBoxes(checkThem: false);
        }

        private void SelectAllCheckBoxes(bool checkThem)
        {
            for (var i = 0; i <= (CheckedListBoxActionCodes.Items.Count - 1); i++)
            {
                CheckedListBoxActionCodes.SetItemCheckState(i, checkThem ? CheckState.Checked : CheckState.Unchecked);
            }
        }

        private void ButtonRun_Click(object sender, EventArgs e)
        {
            GetHistoryRecords();
        }

        private void GetHistoryRecords()
        {
            var fromDate = GetFromDate();
            var toDate = GetToDate();
            string codes = GetCodes();
            string findWhat = TextBoxFind.Text.Trim().ToLower();
            string find = _akaRepository.Get(findWhat);
            TextBoxFind.Text = find;
            var history = GlobalVar.HistoryManager.GetHistoryRecords(codes,fromDate, toDate, find);

            _bindingSourceEquin = new BindingListView<HistoryView>(history);

            DataGridView1.DataSource = _bindingSourceEquin;
    }

        private string GetCodes()
        {
            var codes = new List<string>();

            foreach (KeyValuePair<int, string> item in CheckedListBoxActionCodes.CheckedItems)
            {
                codes.Add(item.Key.ToString());
            }
            var result = string.Join(",", codes);

            return result;
        }

        private DateTime GetToDate()
        {
            var toDate = new DateTime();
            var today = DateTime.Now;

            if (RadioButtonToday.Checked)
            {
                toDate = new DateTime(today.Year, today.Month, today.Day, 23, 59, 59, 999);
            }
            else if (RadioButtonWeek.Checked)
            {
                var date = today.LastDayOfWeek();
                toDate = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999);
            }
            else if (RadioButtonMonth.Checked)
            {
                var date = DateTimePickerFrom.Value;
                var lastDay = date.LastDayOfMonth();
                toDate = new DateTime(lastDay.Year, lastDay.Month, lastDay.Day, 23, 59, 59, 999);
            }
            else if (RadioButtonDateRange.Checked)
            {
                var date = DateTimePickerTo.Value;
                toDate = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999);
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
                var date = DateTimePickerFrom.Value;
                fromDate = new DateTime(date.Year, 1, date.Day, 0, 0, 0, 0);
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
            Cursor.Current = Cursors.WaitCursor;
            CsvUtility.SaveToCsv(DataGridView1);
            Cursor.Current = Cursors.Default;
        }

        private void MButtonRun_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            GetHistoryRecords();
            Cursor.Current = Cursors.Default;
        }
    }

}
