using Equin.ApplicationFramework;
using NeutronData.Models;
using ReplenService;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Neutron.Forms
{
    public partial class FrmReplenishments : Form
    {
        private readonly IReplenRepository _replenRepository;

        private BindingListView<Replenishment> _bindingListView;
        private BindingSource _bindingSource;

        // UI Controls
        private DataGridView _dataGridViewReplenishments;
        private TextBox _textBoxSearch;
        private Button _buttonSearch;
        private Button _buttonClear;
        private Button _buttonRefresh;
        private Label _labelRecordCount;
        private ComboBox _comboBoxFilterColumn;
        public FrmReplenishments(IReplenRepository replenRepository)
        {
            InitializeComponent();
            _replenRepository = replenRepository;
            _bindingSource = new BindingSource();
            InitializeControls();
        }

        //--------------------
        private void InitializeControls()
        {
            // Search TextBox
            _textBoxSearch = new TextBox
            {
                Location = new Point(100, 15),
                Size = new Size(200, 23),
                Name = "TextBoxSearch"
            };
            _textBoxSearch.KeyDown += TextBoxSearch_KeyDown;

            // Filter Column ComboBox
            _comboBoxFilterColumn = new ComboBox
            {
                Location = new Point(310, 15),
                Size = new Size(120, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Name = "ComboBoxFilterColumn"
            };
            _comboBoxFilterColumn.Items.AddRange(new object[] { "All", "Item", "Description", "Area" });
            _comboBoxFilterColumn.SelectedIndex = 1;

            // Search Button
            _buttonSearch = new Button
            {
                Location = new Point(440, 15),
                Size = new Size(80, 23),
                Text = "Search",
                Name = "ButtonSearch"
            };
            _buttonSearch.Click += ButtonSearch_Click;

            // Clear Button
            _buttonClear = new Button
            {
                Location = new Point(530, 15),
                Size = new Size(80, 23),
                Text = "Clear",
                Name = "ButtonClear"
            };
            _buttonClear.Click += ButtonClear_Click;

            // Refresh Button
            _buttonRefresh = new Button
            {
                Location = new Point(620, 15),
                Size = new Size(80, 23),
                Text = "Refresh",
                Name = "ButtonRefresh"
            };
            _buttonRefresh.Click += ButtonRefresh_Click;

            // Record Count Label
            _labelRecordCount = new Label
            {
                Location = new Point(710, 18),
                Size = new Size(150, 20),
                Text = "Records: 0",
                Name = "LabelRecordCount"
            };

            // DataGridView
            _dataGridViewReplenishments = new DataGridView
            {
                Location = new Point(12, 50),
                Size = new Size(900, 570),
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                MultiSelect = true,
                DefaultCellStyle = { ForeColor = Color.Black, BackColor = Color.White },
                ScrollBars = ScrollBars.Both,
                Name = "DataGridViewReplenishments"
            };

            SetupColumns();

            // Add controls to form
            Controls.AddRange(new Control[]
            {
            new Label { Location = new Point(12, 18), Size = new Size(80, 20), Text = "Search:" },
            _textBoxSearch,
            _comboBoxFilterColumn,
            _buttonSearch,
            _buttonClear,
            _buttonRefresh,
            _labelRecordCount,
            _dataGridViewReplenishments
            });
        }

        private void SetupColumns()
        {
            // Item Column
            var colItem = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = "Item",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Item"
            };
            _dataGridViewReplenishments.Columns.Add(colItem);
            
            //// Area 8 Column
            //var colArea = new DataGridViewTextBoxColumn
            //{
            //    DataPropertyName = "Area",
            //    HeaderText = "Area",
            //    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
            //    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
            //    Name = "Area"
            //};
            //_dataGridViewReplenishments.Columns.Add(colArea);

            // QuantityInEight Column
            var colQuantityInEight = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "QuantityInEight",
                HeaderText = "OC Inventory",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "QuantityInEight"
            };
            _dataGridViewReplenishments.Columns.Add(colQuantityInEight);

            // System ReplenArea Column 
            var colReplenArea = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ReplenArea",
                HeaderText = "Replen Area",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "ReplenArea"
            };
            _dataGridViewReplenishments.Columns.Add(colReplenArea);
            
           

            // Replen Area Inventory Column
            var colReplenAreaQuantity = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ReplenAreaQuantity",
                HeaderText = "Current Inv",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "ReplenAreaQuantity"
            };
            _dataGridViewReplenishments.Columns.Add(colReplenAreaQuantity);

            // System Min Column
            var colSystemMin = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SystemMin",
                HeaderText = "System Min",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "SystemMin"
            };
            _dataGridViewReplenishments.Columns.Add(colSystemMin);

            // System Max Column
            var colSystemMax = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SystemMax",
                HeaderText = "System Max",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "SystemMax"
            };
            _dataGridViewReplenishments.Columns.Add(colSystemMax);

            // Quantity Needed Column
            var colQtyNeeded = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "QuantityNeeded",
                HeaderText = "Qty Needed",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "QuantityNeeded"
            };
            _dataGridViewReplenishments.Columns.Add(colQtyNeeded);

            // Style headers
            _dataGridViewReplenishments.EnableHeadersVisualStyles = false;
            _dataGridViewReplenishments.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _dataGridViewReplenishments.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);
            _dataGridViewReplenishments.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
            _dataGridViewReplenishments.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        }

        private async void FrmReplenishments_Load(object sender, EventArgs e)
        {
            await LoadReplenishmentsAsync();
        }

        private async void ButtonRefresh_Click(object sender, EventArgs e)
        {
            await LoadReplenishmentsAsync();
        }

        private async Task LoadReplenishmentsAsync()
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                _buttonRefresh.Enabled = false;

                var replenishments = await _replenRepository.GetReplenishments();

                // Create BindingListView for sorting and filtering
                _bindingListView = new BindingListView<Replenishment>(replenishments);
                _bindingSource.DataSource = _bindingListView;

                _dataGridViewReplenishments.DataSource = _bindingSource;
                _dataGridViewReplenishments.ClearSelection();
                _dataGridViewReplenishments.Refresh();

                UpdateRecordCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading replenishments: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                _buttonRefresh.Enabled = true;
            }
        }

        private void ButtonSearch_Click(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void TextBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ApplyFilter();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                _textBoxSearch.Text = string.Empty;
                ClearFilter();
                e.Handled = true;
            }
        }

        private void ButtonClear_Click(object sender, EventArgs e)
        {
            _textBoxSearch.Text = string.Empty;
            _comboBoxFilterColumn.SelectedIndex = 0;
            ClearFilter();
        }

        private void ApplyFilter()
        {
            if (_bindingListView == null) return;

            var searchText = _textBoxSearch.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                ClearFilter();
                return;
            }

            var filterColumn = _comboBoxFilterColumn.SelectedItem?.ToString() ?? "All";

            try
            {
                switch (filterColumn)
                {
                    case "Item":
                        _bindingListView.ApplyFilter(r =>
                            r.Item != null && r.Item.ToLower().Contains(searchText));
                        break;

                    //case "Description":
                    //    _bindingListView.ApplyFilter(r =>
                    //        r.QuantityInEight != null && r.QuantityInEight.ToLower().Contains(searchText));
                    //    break;

                    //case "Area":
                    //    if (int.TryParse(searchText, out int areaId))
                    //    {
                    //        _bindingListView.ApplyFilter(r => r.AreaId == areaId);
                    //    }
                    //    break;

                    default: // "All"
                        //_bindingListView.ApplyFilter(r =>
                        //    (r.Item != null && r.Item.ToLower().Contains(searchText)) ||
                        //    (r.Description != null && r.Description.ToLower().Contains(searchText)) ||
                        //    (r.AreaId.ToString().Contains(searchText)));
                        break;
                }

                _dataGridViewReplenishments.ClearSelection();
                UpdateRecordCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying filter: {ex.Message}",
                    "Filter Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void ClearFilter()
        {
            if (_bindingListView == null) return;

            try
            {
                _bindingListView.RemoveFilter();
                _dataGridViewReplenishments.ClearSelection();
                UpdateRecordCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error clearing filter: {ex.Message}",
                    "Filter Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void UpdateRecordCount()
        {
            var count = _bindingSource?.Count ?? 0;
            _labelRecordCount.Text = $"Records: {count}";
        }
    }
    //--------------------
}

