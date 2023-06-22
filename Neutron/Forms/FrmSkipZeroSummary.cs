using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using DGVPrinterHelper;
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
                skip.AreaId = view.AreaId.ToString();
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
                skip.AreaId = view.AreaId.ToString();
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
            public string AreaId { get; set; }
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
                DataPropertyName = "AreaId",
                HeaderText = _gridResourceManager.GetString($"Area"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "AreaId"
            };
            DataGridViewSkipZeroSummary.Columns.Add(col);

            DataGridViewSkipZeroSummary.EnableHeadersVisualStyles = false;
            DataGridViewSkipZeroSummary.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            DataGridViewSkipZeroSummary.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);

            //foreach (DataGridViewColumn column in DataGridViewSkipZeroSummary.Columns)
            //{
            //    column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //    column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            //}

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
                Text = _resourceManager.GetString("Summary");
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

        private void ButtonPrint_Click(object sender, EventArgs e)
        {
            PrinterSettings myprintsettings = null;
            PageSettings mypagesettings = null;

            var printer = new DGVPrinter();
            printer.Title = "Zero Pick Report";
            printer.SubTitle = DateTime.Now.ToString("F");
            printer.SubTitleFormatFlags = StringFormatFlags.LineLimit |
                                          StringFormatFlags.NoClip;
            printer.PageNumbers = true;
            printer.PageNumberInHeader = false;
            printer.ColumnWidth = DGVPrinter.ColumnWidthSetting.Porportional;
            printer.HeaderCellAlignment = StringAlignment.Near;
            printer.Footer = "";
            printer.FooterSpacing = 15;

            

            if (myprintsettings != null)
                printer.printDocument.PrinterSettings = myprintsettings;
            if (null != mypagesettings)
                printer.printDocument.DefaultPageSettings = mypagesettings;
            if (DialogResult.OK == printer.DisplayPrintDialog()) // you may replace DisplayPrintDialog() with your own print dialog
            {
                // save users' settings
                myprintsettings = printer.PrintSettings;
                mypagesettings = printer.PageSettings;
                // print without redisplaying the printdialog
                printer.PrintNoDisplay(DataGridViewSkipZeroSummary);
            }
        }
    }
}
