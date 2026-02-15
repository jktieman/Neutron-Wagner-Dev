using MetroFramework.Forms;
using Neutron.Classes;
using NeutronCore.Extensions;
using Neutron.Global;
using NeutronData.Interfaces;
using NeutronData.ModelViews;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Equin.ApplicationFramework;
using NeutronCore;
using NeutronCore.Enums;
using Neutron.Models;
using NeutronData.Repositories;
using NeutronData.DataContexts;
using NeutronData.Models;

namespace Neutron.Forms
{
    public partial class FrmHistory : MetroForm
    {
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;
        private ResourceManager _gridResourceManager;
        private ResourceManager _enumResourceManager;

        readonly IAkaRepository _akaRepository;
        private readonly IHistoryManager _historyManager;
        private readonly WorkstationView _workstationView;
        private BindingListView<HistoryView> _bindingSourceEquin;
        private DateTime _currentFromDateTime;
        private DateTime _currentToDateTime;
        private readonly HeaderTextManager _headerTextManager;
        private readonly GenericRepository<History> _repoHistory;
        private readonly Func<NeutronDb> _contextFactory;

        public FrmHistory(IAkaRepository akaRepository, IHistoryManager historyManager, WorkstationView workstationView, Func<NeutronDb> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            _repoHistory = new GenericRepository<History>(contextFactory);

            HideTabControlTabs();
            _akaRepository = akaRepository;
            _historyManager = historyManager;
            _workstationView = workstationView;
            _headerTextManager = new HeaderTextManager();

            mlUserInfo.Text = GlobalVar.User?.UserInfo;

        }

        private void FrmHistory_Load(object sender, EventArgs e)
        {

            var date = DateTime.Now;
            DateTimePickerFrom.Value = new DateTime(2023, 1, 1, 0, 0, 0);
            DateTimePickerTo.Value = date;
            _currentFromDateTime = new DateTime(2023, 1, 1, 0, 0, 0);
            _currentToDateTime = date;

            SetupGrids();

        }

        private void FrmHistory_Shown(object sender, EventArgs e)
        {
            SetupCheckListBoxActionCodes();
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
        private void SetupGrids()
        {

            DataGridView1.AutoGenerateColumns = false;
            DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridView1.DefaultCellStyle.ForeColor = Color.Black;
            DataGridView1.DefaultCellStyle.BackColor = Color.White;
            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = _gridResourceManager.GetString($"Id"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Id",
                Visible = false
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ActionCode",
                HeaderText = _gridResourceManager.GetString($"ActionCode"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "ActionCode",
                Visible = true
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ActionCodeName",
                HeaderText = _gridResourceManager.GetString($"ActionCodeName"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "ActionCodeName",
                Visible = true
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ActionDateTime",
                HeaderText = _gridResourceManager.GetString($"ActionDateTime"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "ActionDateTime",
                Visible = true
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord1",
                HeaderText = _gridResourceManager.GetString($"Ord1"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Ord1",
                Visible = true
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord2",
                HeaderText = _gridResourceManager.GetString($"Ord2"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Ord2",
                Visible = true
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = _gridResourceManager.GetString($"Item"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Item",
                Visible = true
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = _gridResourceManager.GetString($"Description"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Description",
                Visible = true
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "RequestedQuantity",
                HeaderText = _gridResourceManager.GetString($"RequestedQuantity"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "RequestedQuantity",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Visible = true
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "IssuedQuantity",
                HeaderText = _gridResourceManager.GetString($"IssuedQuantity"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "IssuedQuantity",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Visible = true
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "AreaId",
                HeaderText = _gridResourceManager.GetString($"Area"),
                Visible = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "AreaId",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "WorkstationName",
                HeaderText = _gridResourceManager.GetString($"Workstation"),
                Visible = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "WorkstationName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };

            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc1",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc1", _gridResourceManager),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Loc1",
                Visible = true
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc2",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc2", _gridResourceManager),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Loc2",
                Visible = true
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc3",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc3", _gridResourceManager),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Loc3",
                Visible = true
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc4",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc4", _gridResourceManager),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Loc4",
                Visible = true
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc5",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc5", _gridResourceManager),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Loc5",
                Visible = false
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Slot",
                HeaderText = _gridResourceManager.GetString($"Slot"),
                Visible = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Slot"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderId",
                HeaderText = _gridResourceManager.GetString($"OrderId"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "OrderId",
                Visible = false
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderDetailId",
                HeaderText = _gridResourceManager.GetString($"OrderDetailId"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "OrderDetailId",
                Visible = false
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "EmpId",
                HeaderText = _gridResourceManager.GetString($"EmpId"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "EmpId",
                Visible = false
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "EmployeeName",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                HeaderText = _gridResourceManager.GetString($"EmployeeName"),
                Name = "EmployeeName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Visible = true
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CostCenter",
                HeaderText = _gridResourceManager.GetString($"CostCenter"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "CostCenter",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Visible = true
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TransmitDate",
                HeaderText = _gridResourceManager.GetString($"TransmitDate"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "TransmitDate",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Visible = true
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderInfo",
                HeaderText = _gridResourceManager.GetString($"OrderInfo"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "OrderInfo",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Visible = true
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderDetailInfo",
                HeaderText = _gridResourceManager.GetString($"OrderDetailInfo"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "OrderDetailInfo",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Visible = true
            };
            DataGridView1.Columns.Add(col);
        }

        private void SetupCheckListBoxActionCodes()
        {
            // Guard check: Skip re-initialization if already populated
            if (CheckedListBoxActionCodes.Items.Count > 0)
            {
                return;
            }

            var actionCodes = ((ActionCode[])Enum.GetValues(typeof(ActionCode))).ToList();

            CheckedListBoxActionCodes.Items.Clear();
            foreach (var code in actionCodes)
            {
                var displayText = _enumResourceManager.GetString(code.ToString());
                CheckedListBoxActionCodes.Items.Add(new KeyValuePair<int, string>((int)code, displayText));
            }

            CheckedListBoxActionCodes.DisplayMember = "Value";
            CheckedListBoxActionCodes.ValueMember = "Key";

        }

        private void ButtonCheckAll_Click(object sender, EventArgs e)
        {
            SelectAllCheckBoxes(checkThem: true);
            //Run();
        }
        private void ButtonClearAll_Click(object sender, EventArgs e)
        {
            SelectAllCheckBoxes(checkThem: false);
            //Run();
        }
        private void SelectAllCheckBoxes(bool checkThem)
        {
            for (var i = 0; i <= (CheckedListBoxActionCodes.Items.Count - 1); i++)
            {
                CheckedListBoxActionCodes.SetItemCheckState(i, checkThem ? CheckState.Checked : CheckState.Unchecked);
            }
        }
        //private void ButtonRun_Click(object sender, EventArgs e)
        //{
        //    GetHistoryRecords();
        //}
        private void GetHistoryRecords()
        {
            var fromDate = _currentFromDateTime;
            var toDate = _currentToDateTime;
            var codes = GetCodes();
            var findWhat = TextBoxFind.Text.Trim().ToLower();
            var history = _historyManager.GetHistoryRecords(codes, fromDate, toDate, findWhat);
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
                var date = today.LastDayOfMonth();
                toDate = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999);
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
                var date = DateTime.Now;
                fromDate = new DateTime(date.Year, date.Month, 1, 0, 0, 0, 0);
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
            new CsvUtility(_contextFactory).SaveToCsv(DataGridView1);
            Cursor.Current = Cursors.Default;
        }
        private void MButtonRun_Click(object sender, EventArgs e)
        {
            Run();
        }

        private void Run()
        {
            Cursor.Current = Cursors.WaitCursor;
            // get the date based on the Checked RadiobuttonToday, RadioButtonWeek, RadioButtonMonth, RadioButtonDateRange
            var selectedRadioButton = PanelDateRanges.Controls.OfType<RadioButton>().FirstOrDefault(r => r.Checked);
            if (selectedRadioButton == RadioButtonToday)
            {
                _currentFromDateTime = DateTime.Today;
                _currentToDateTime = DateTime.Today.AddDays(1).AddSeconds(-1);
            }
            else if (selectedRadioButton == RadioButtonWeek)
            {
                _currentFromDateTime = GetFirstDayOfWeek(DateTime.Today);
                _currentToDateTime = _currentFromDateTime.AddDays(7).AddSeconds(-1);
            }
            else if (selectedRadioButton == RadioButtonMonth)
            {
                _currentFromDateTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                _currentToDateTime = _currentFromDateTime.AddMonths(1).AddSeconds(-1);
            }
            else if (selectedRadioButton == RadioButtonDateRange)
            {
                _currentFromDateTime = DateTimePickerFrom.Value;
                _currentToDateTime = DateTimePickerTo.Value.AddDays(1).AddSeconds(-1);
            }

            //_currentFromDateTime = DateTimePickerFrom.Value;
            //_currentToDateTime = DateTimePickerTo.Value;
            GetHistoryRecords();
            Cursor.Current = Cursors.Default;
        }
        private void SetCulture(string lang)
        {
            try
            {
                var languageDirectory = LoaderSettings.GetLanguageDirectory();
                _cultureInfo = CultureInfo.CreateSpecificCulture(lang);

                _resourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "FrmHistory",
                    resourceDir: languageDirectory, usingResourceSet: null);
                _gridResourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "GridHeaders",
                    resourceDir: languageDirectory, usingResourceSet: null);
                _enumResourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "EnumDescriptions",
                    resourceDir: languageDirectory, usingResourceSet: null);

                ButtonClearAll.Text = _resourceManager.GetString($"ClearAll");
                ButtonCheckAll.Text = _resourceManager.GetString($"CheckAll");
                GroupBoxActionCodes.Text = _resourceManager.GetString($"ActionCodes");
                LabelTo.Text = _resourceManager.GetString($"To");
                LabelFrom.Text = _resourceManager.GetString($"From");
                RadioButtonDateRange.Text = _resourceManager.GetString($"DateRange");
                RadioButtonMonth.Text = _resourceManager.GetString($"Month");
                RadioButtonWeek.Text = _resourceManager.GetString($"Week");
                RadioButtonToday.Text = _resourceManager.GetString($"Today");
                LabelFindDescription.Text = _resourceManager.GetString($"SearchForPartofOrderorItem");
                MButtonClose.Text = _resourceManager.GetString($"Close");
                MBHistoryTransmitSelected.Text = _resourceManager.GetString($"TransmitSelected");
                MBSaveHistory.Text = _resourceManager.GetString($"SavetoFile");
                MButtonRun.Text = _resourceManager.GetString($"Run");
                LabelFormTitle.Text = _resourceManager.GetString($"History");
                mlUserInfo.Text = _resourceManager.GetString($"Login");
                LabelFormHeaderText.Text = _resourceManager.GetString($"NeutronWarehouseManagement");
                Text = _resourceManager.GetString($"History");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading language file.  {ex.Message} {Environment.NewLine} {ex.InnerException} ");
            }
        }

        private void RadioButtonToday_CheckedChanged(object sender, EventArgs e)
        {
            if (!((RadioButton)sender).Checked) return;
            Cursor.Current = Cursors.WaitCursor;
            var date = DateTime.Now;
            DateTimePickerFrom.Value = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
            DateTimePickerTo.Value = date;
            _currentFromDateTime = DateTimePickerFrom.Value;
            _currentToDateTime = DateTimePickerTo.Value;
            //GetHistoryRecords();
            Cursor.Current = Cursors.Default;
        }

        private void RadioButtonWeek_CheckedChanged(object sender, EventArgs e)
        {
            if (!((RadioButton)sender).Checked) return;
            Cursor.Current = Cursors.WaitCursor;
            var date = DateTime.Now;
            var firstDay = date.FirstDayOfWeek();
            DateTimePickerFrom.Value = new DateTime(firstDay.Year, firstDay.Month, firstDay.Day, 0, 0, 0);
            DateTimePickerTo.Value = date;
            _currentFromDateTime = DateTimePickerFrom.Value;
            _currentToDateTime = DateTimePickerTo.Value;
            // GetHistoryRecords();
            Cursor.Current = Cursors.Default;
        }

        private void RadioButtonMonth_CheckedChanged(object sender, EventArgs e)
        {
            if (!((RadioButton)sender).Checked) return;
            Cursor.Current = Cursors.WaitCursor;
            var date = DateTime.Now;
            var firstDay = date.FirstDayOfMonth();
            DateTimePickerFrom.Value = new DateTime(firstDay.Year, firstDay.Month, firstDay.Day, 0, 0, 0);
            DateTimePickerTo.Value = date;
            _currentFromDateTime = DateTimePickerFrom.Value;
            _currentToDateTime = DateTimePickerTo.Value;
            // GetHistoryRecords();
            Cursor.Current = Cursors.Default;
        }

        private void RadioButtonDateRange_CheckedChanged(object sender, EventArgs e)
        {
            if (!((RadioButton)sender).Checked) return;
            Cursor.Current = Cursors.WaitCursor;
            var date = DateTime.Now;
            var firstDay = date.FirstDayOfMonth();
            DateTimePickerFrom.Value = new DateTime(firstDay.Year, firstDay.Month, firstDay.Day, 0, 0, 0);
            DateTimePickerTo.Value = date;
            _currentFromDateTime = DateTimePickerFrom.Value;
            _currentToDateTime = DateTimePickerTo.Value;
            // GetHistoryRecords();
            Cursor.Current = Cursors.Default;
        }

        private void DateTimePickerFrom_Enter(object sender, EventArgs e)
        {
            RadioButtonDateRange.Checked = true;
        }

        private void DateTimePickerTo_Enter(object sender, EventArgs e)
        {
            RadioButtonDateRange.Checked = true;
        }

        private void TextBoxFind_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                //Run();
            }
        }

        private void MBHistoryTransmitSelected_Click(object sender, EventArgs e)
        {
            TransmitSelectedRecords();
        }

        private void TransmitSelectedRecords()
        {
            foreach (DataGridViewRow row in DataGridView1.SelectedRows)
            {
                var id = (int)row.Cells["Id"].Value;
                var history = _repoHistory.FindByKey(id);
                history.TransmitDateTime = null;
                _repoHistory.Update(history);

            }
        }
    }
}
