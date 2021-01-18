using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NeutronCore;
using NeutronData.ModelViews;

namespace Neutron.Forms
{
    public partial class FrmSkipZeroSummary : Form
    {
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;
        private ResourceManager _gridResourceManager;
        private bool _skipZeroGridReady = false;

        public FrmSkipZeroSummary(List<PickView> skipPickableViews, List<PickView> zeroPickableViews)
        {
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            SetupSkipZeroGrid(new object());


            var skipZeroList = new List<SkipZero>();
            foreach (var view in skipPickableViews)
            {
                var skip = new SkipZero();
                skip.Action = "Skip Pick";
                skip.StationNumber = view.StationNumber.ToString();
                skip.Ord1 = view.Ord1;
                skip.Ord2 = view.Ord2;
                skip.Item = view.Item;
                skip.Description = view.Description;
                skip.Quantity = view.Quantity.ToString();
                skipZeroList.Add(skip);
            }
            foreach (var view in zeroPickableViews)
            {
                var skip = new SkipZero();
                skip.Action = "Zero Pick";
                skip.StationNumber = view.StationNumber.ToString();
                skip.Ord1 = view.Ord1;
                skip.Ord2 = view.Ord2;
                skip.Item = view.Item;
                skip.Description = view.Description;
                skip.Quantity = view.Quantity.ToString();
                skipZeroList.Add(skip);
            }
            DataGridViewSkipZeroSummary.DataSource = skipZeroList;
        }

        private void ButtonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private class SkipZero
        {
            public string Action { get; set; }
            public string StationNumber { get; set; }
            public string Ord1 { get; set; }
            public string Ord2 { get; set; }
            public string Item { get; set; }
            public string Description { get; set; }
            public string Quantity { get; set; }
        }

        private void FrmSkipZeroSummary_Load(object sender, EventArgs e)
        {
            

        }

        private void SetupSkipZeroGrid(object state)
        {
            var result = false;

            DataGridViewSkipZeroSummary.AutoGenerateColumns = false;
            DataGridViewSkipZeroSummary.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewSkipZeroSummary.DefaultCellStyle.ForeColor = Color.Black;
            DataGridViewSkipZeroSummary.DefaultCellStyle.BackColor = Color.White;

            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Action",
                HeaderText = _gridResourceManager.GetString($"Action"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Action"
            };
            DataGridViewSkipZeroSummary.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord1",
                HeaderText = _gridResourceManager.GetString($"Ord1"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Ord1"
            };
            DataGridViewSkipZeroSummary.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord2",
                HeaderText = _gridResourceManager.GetString($"Ord2"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Ord2"
            };
            DataGridViewSkipZeroSummary.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = _gridResourceManager.GetString($"Item"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Item"
            };
            DataGridViewSkipZeroSummary.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = _gridResourceManager.GetString($"Description"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Description"
            };
            DataGridViewSkipZeroSummary.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = _gridResourceManager.GetString($"Quantity"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Quantity"
            };
            DataGridViewSkipZeroSummary.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StationNumber",
                HeaderText = _gridResourceManager.GetString($"Station"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "Station"
            };
            DataGridViewSkipZeroSummary.Columns.Add(col);

           foreach (DataGridViewColumn column in DataGridViewSkipZeroSummary.Columns)
            {
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            }

            _skipZeroGridReady = true;
        }

        private void SetCulture(string lang)
        {
            try
            {
                var languageDirectory = LoaderSettings.GetLanguageDirectory();
                _cultureInfo = CultureInfo.CreateSpecificCulture(lang);

                _resourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "FrmSkipZeroSummary",
                    resourceDir: languageDirectory, usingResourceSet: null);
                _gridResourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "GridHeaders",
                    resourceDir: languageDirectory, usingResourceSet: null);

                ButtonClose.Text = _resourceManager.GetString("Close");
                this.Text = _resourceManager.GetString("Summary");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading language file.  { ex.Message} { Environment.NewLine} { ex.InnerException} ");
            }
        }

        private void DataGridViewSkipZeroSummary_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            DataGridViewSkipZeroSummary.ClearSelection();
        }
    }
}
