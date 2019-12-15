using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MetroFramework.Forms;
using Neutron.Global;
using Neutron.Models;
using NeutronData.DataContexts;
using NeutronData.ModelViews;
using NeutronData.Models;
using NeutronData.Repositories;
using System.Reflection;
using JsonManager;
using NeutronCore.Global;
using PrintRequest;
using System.Deployment.Application;
using Neutron.Extensions;
using NeutronCore.Enums;

namespace Neutron.Forms
{
    public partial class FrmUtilities : MetroForm
    {
        private readonly GenericRepository<HardwareDevice> _repoHardwareDevices = new GenericRepository<HardwareDevice>(new NeutronDb());
        private readonly GenericRepository<Order> _repoOrders = new GenericRepository<Order>(new NeutronDb());

        private readonly BindingSource _bindingSourceHardwareDevices = new BindingSource();
        public bool CloseButtonPressed { get; set; }
        //public bool CloseForm = false;
        private readonly IJsonData _jsonData;
        public HardwareDeviceView CurrentItem;
        public DocumentPrinterPreferences DocumentPrinter;
        public LabelPrinterPreferences LabelPrinter;
        private NeutronVariables _neutronVariables;

        public FrmUtilities(IJsonData jsonData)
        {
            InitializeComponent();
            KeyPreview = true;
            _jsonData = jsonData;
            HideTabControlTabs();
            mlUserInfo.Text = GlobalVar.User?.UserInfo;
            CloseButtonPressed = false;
            SetupGrids();
            LabelVersion.Text = ($"{ApplicationVersion.Major}.{ApplicationVersion.Minor}.{ApplicationVersion.Build}.{ApplicationVersion.Revision}.{ApplicationVersion.MajorRevision}.{ApplicationVersion.MinorRevision}");
        }

        public Version ApplicationVersion
        {
            get
            {
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    return ApplicationDeployment.CurrentDeployment.CurrentVersion;
                }
                else
                {
                    return Assembly.GetExecutingAssembly().GetName().Version;
                }
            }
        }


        private void SetupGrids()
        {
            DataGridView1.AutoGenerateColumns = false;
            DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridView1.DefaultCellStyle.ForeColor = Color.Black;
            DataGridView1.DefaultCellStyle.BackColor = Color.White;

            var colx = new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "Enabled",
                HeaderText = @"Enabled",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "Enabled",
                TrueValue = true,
                FalseValue = false
            };
            DataGridView1.Columns.Add(colx);

            colx = new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "SimulationMode",
                HeaderText = @"Simulation",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "SimulationMode",
                TrueValue = true,
                FalseValue = false
            };
            DataGridView1.Columns.Add(colx);

            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StationName",
                HeaderText = @"Station Name",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "StationName"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "DeviceNumber",
                HeaderText = @"Device Number",
                DefaultCellStyle = {Alignment = DataGridViewContentAlignment.MiddleRight},
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "DeviceNumber"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Name",
                HeaderText = @"Name",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Name"
            };
            DataGridView1.Columns.Add(col);

           
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "NumberOfCarriers",
                HeaderText = @"Total Carriers",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "NumberOfCarriers"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CarrierWidth",
                HeaderText = @"Carrier Width",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "CarrierWidth"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CarrierHeight",
                HeaderText = @"Carrier Height",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "CarrierHeight"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LogLevel",
                HeaderText = @"Log Level",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "LogLevel"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "DeviceTypeName",
                HeaderText = @"Device Type Name",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "DeviceTypeName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = @"Id",
                Visible = false,
                Name = "Id"
            };
            DataGridView1.Columns.Add(col);

        }

        private void MBMainClose_Click(object sender, EventArgs e)
        {
            CloseButtonPressed = true;
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

        private void FrmUtilities_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!CloseButtonPressed)
            {
                e.Cancel = true;
            }
        }

        //private void MBMainSqlServer_Click(object sender, EventArgs e)
        //{
        //    LabelFormTitle.Text = "Sql Server Setup";
        //    LabelFormTitle.BackColor = Color.Turquoise;
        //    tabControl1.SelectedTab = SqlServer;
        //}

        //private void MBMainInterfaceFiles_Click(object sender, EventArgs e)
        //{
        //    LabelFormTitle.Text = "Interface Files";
        //    LabelFormTitle.BackColor = Color.Turquoise;
        //    tabControl1.SelectedTab = InterfaceFiles;
        //}


        //private void MBMainSpare1_Click(object sender, EventArgs e)
        //{
        //    LabelFormTitle.Text = "Spare 1";
        //    LabelFormTitle.BackColor = Color.Turquoise;
        //    tabControl1.SelectedTab = Spare1;
        //}

        //private void MBMainSpare2_Click(object sender, EventArgs e)
        //{
        //    LabelFormTitle.Text = "Spare 2";
        //    LabelFormTitle.BackColor = Color.Turquoise;
        //    tabControl1.SelectedTab = Spare2;
        //}

        //private void MBSqlServerBack_Click(object sender, EventArgs e)
        //{
        //    LabelFormTitle.Text = "System";
        //    LabelFormTitle.BackColor = Color.Turquoise;
        //    tabControl1.SelectedTab = Main;
        //}

        //private void MBInterfaceFilesBack_Click(object sender, EventArgs e)
        //{
        //    LabelFormTitle.Text = "System";
        //    LabelFormTitle.BackColor = Color.Turquoise;
        //    tabControl1.SelectedTab = Main;
        //}

        //private void MBSpare1Back_Click(object sender, EventArgs e)
        //{
        //    LabelFormTitle.Text = "System";
        //    LabelFormTitle.BackColor = Color.Turquoise;
        //    tabControl1.SelectedTab = Main;
        //}

        //private void MBSpare2Back_Click(object sender, EventArgs e)
        //{
        //    LabelFormTitle.Text = "System";
        //    LabelFormTitle.BackColor = Color.Turquoise;
        //    tabControl1.SelectedTab = Main;
        //}

        private void FrmUtilities_Load(object sender, EventArgs e)
        {
            var result = LoadHardwareDevices();

            try
            {
                DocumentPrinter = LoadDocumentPrinterPreferences();

                TextBoxDocumentPrinter.Text = DocumentPrinter.PrinterName;
                TextBoxDocumentLeftMargin.Text = DocumentPrinter.LeftMargin.ToString();
                TextBoxDocumentTopMargin.Text = DocumentPrinter.TopMargin.ToString();
                TextBoxDocumentRightMargin.Text = DocumentPrinter.RightMargin.ToString();
                TextBoxDocumentBottomMargin.Text = DocumentPrinter.BottomMargin.ToString();

            }
            catch (Exception)
            {

            }

            try
            {
                LabelPrinter = LoadLabelPrinterPreferences();

                TextBoxLabelPrinter.Text = LabelPrinter.PrinterName;
                TextBoxLabelHomeX.Text = LabelPrinter.HomeX.ToString();
                TextBoxLabelHomeY.Text = LabelPrinter.HomeY.ToString();
            }
            catch (Exception)
            {

            }
        }
        // Set the focus to the passed in recId if it's passed in
        private int LoadHardwareDevices(int recId = 0)
        {
            var idx = 0;
            // var hardwareDeviceViews = new List<HardwareDeviceView>();
            var recs = _repoHardwareDevices.AllInclude(r => r.Station, r => r.DeviceType).ToList();
            //create the view
            if (recs.Count > 0)
            {
                var hardwareDeviceViews = recs.Select(r => new HardwareDeviceView
                {
                    Id = r.Id
                    , DeviceNumber = r.DeviceNumber
                    , Name = r.Name
                    , StationName = r.Station.Name
                    , NumberOfCarriers = int.Parse(r.NumberOfCarriers.ToString())
                    , CarrierWidth = r.CarrierWidth
                    , CarrierDepth = r.CarrierDepth
                    , DeviceTypeName = r.DeviceType.Name
                    , Enabled = r.Enabled
                    , SimulationMode = r.SimulationMode
                    , LogLevel = r.LogLevel
                    , 
                }).ToList();

                _bindingSourceHardwareDevices.DataSource = hardwareDeviceViews;

                DataGridView1.DataSource = _bindingSourceHardwareDevices;

                if (GetRecordCount(_bindingSourceHardwareDevices) > 0)
                {
                    if (recId != 0)
                    {
                        idx = IndexOf(_bindingSourceHardwareDevices, recId);
                        DataGridView1.FirstDisplayedScrollingRowIndex = DataGridView1.Rows[idx].Index;
                    }
                    else
                    {
                        DataGridView1.ClearSelection();
                    }
                    DataGridView1.Refresh();

                    CurrentItem = ((HardwareDeviceView)_bindingSourceHardwareDevices.Current);
                }
            }
            return idx;
        }

        public int IndexOf(BindingSource bs, int value)
        {
            var count = bs.Count;
            var itemIndex = -1;
            for (var i = 0; i < count; i++)
            {
                var rec = ((OrderView)bs[i]).Id;
                if (rec == value)
                {
                    itemIndex = i;
                    break;
                }
            }
            return itemIndex;
        }


        private int GetRecordCount(BindingSource bs)
        {
            var count = bs.Count;
            LabelRecordCount.Text = string.Format("Records: {0}", count.ToString());
            return count;
        }
        //private void ButtonSave_Click(object sender, EventArgs e)
        //{
        //    string configFilePath = Properties.Settings.Default.ConfigFilePath;

        //    LoaderSettings.HostOrderDirectory = this.HostOrderDirectory.Text;
        //    LoaderSettings.HostOrderFile = this.HostOrderFile.Text;
        //    LoaderSettings.HostUploadDirectory = this.HostUploadDirectory.Text;
        //    LoaderSettings.HostUploadFile = this.HostUploadFile.Text;
        //    LoaderSettings.EnableLogging = this.EnableLogging.Checked.ToString().ToLower();
        //    LoaderSettings.Save(configFilePath);
        //}



        private void MBDevices_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Device Listing";
            LabelFormTitle.BackColor = Color.RoyalBlue;
            tabControl1.SelectedTab = HardwareDevices;
            LoadHardwareDevices();
        }

        private void MBDeviceListingBack_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Devices";
            LabelFormTitle.BackColor = Color.RoyalBlue;
            tabControl1.SelectedTab = Main;
        }

        #region Button Clicks
        //private void ButtonClear_Click(object sender, EventArgs e)
        //{
        //    TextBoxFind.Text = string.Empty;
        //    RefreshData();
        //    TextBoxFind.Focus();
        //}

        //private void MButtonClose_Click(object sender, EventArgs e)
        //{
        //    this.Close();
        //}

        //private void MButtonViewEdit_Click(object sender, EventArgs e)
        //{
        //    previousTab = tabControl1.SelectedTab;
        //    tabControl1.SelectedTab = ViewEdit;
        //}

        //private void MButtonNew_Click(object sender, EventArgs e)
        //{
        //    previousTab = tabControl1.SelectedTab;
        //    tabControl1.SelectedTab = New;
        //}

        //private void MbViewEditListing_Click(object sender, EventArgs e)
        //{
        //    previousTab = tabControl1.SelectedTab;
        //    tabControl1.SelectedTab = Listing;
        //}

        //private void MbViewEditNew_Click(object sender, EventArgs e)
        //{
        //    previousTab = tabControl1.SelectedTab;
        //    tabControl1.SelectedTab = New;
        //}

        //private void MbViewEditClose_Click(object sender, EventArgs e)
        //{
        //    Back();
        //}

        //private void MbViewEditSave_Click(object sender, EventArgs e)
        //{
        //    UpdateViewEdit();
        //}

        //private void MbNewListing_Click(object sender, EventArgs e)
        //{
        //    previousTab = tabControl1.SelectedTab;
        //    tabControl1.SelectedTab = Listing;
        //}

        //private void MbNewViewEdit_Click(object sender, EventArgs e)
        //{
        //    previousTab = tabControl1.SelectedTab;
        //    tabControl1.SelectedTab = ViewEdit;
        //}

        //private void MbNewSave_Click(object sender, EventArgs e)
        //{
        //    SaveNew();
        //}

        //private void MbNewClose_Click(object sender, EventArgs e)
        //{
        //    Back();
        //}
        #endregion

        private void MBUtilitiesSpare4_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Testing";
            LabelFormTitle.BackColor = Color.RoyalBlue;
            tabControl1.SelectedTab = DisplayListing;
        }

        private void MBInterfaceFilesBack_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Devices";
            LabelFormTitle.BackColor = Color.RoyalBlue;
            tabControl1.SelectedTab = Main;
        }

        private void MbViewEditClose_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Devices";
            LabelFormTitle.BackColor = Color.RoyalBlue;
            tabControl1.SelectedTab = Main;
        }

        private void MbNomenclatureClose_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Devices";
            LabelFormTitle.BackColor = Color.RoyalBlue;
            tabControl1.SelectedTab = Main;
        }

        private string GetPickMethod()
        {
            var a = from RadioButton r in GroupBoxPickMethod.Controls where r.Checked == true select r.Name;
            return a.First();
        }

        private void SetPickMethod(string pickMethod)
        {
            switch (pickMethod)
            {
                case "RadioButtonPrimeBinFirst":
                    RadioButtonPrimeBinFirst.Checked = true;
                    break;
                case "RadioButtonPrimeBinLast":
                    RadioButtonPrimeBinLast.Checked = true;
                    break;
                case "RadioButtonFifo":
                    RadioButtonFifo.Checked = true;
                    break;
                case "RadioButtonLifo":
                    RadioButtonLifo.Checked = true;
                    break;
                default:
                    RadioButtonFifo.Checked = true;
                    break;
            }
        }

        private void MBSaveVariables_Click(object sender, EventArgs e)
        {
            _neutronVariables = new NeutronVariables();
            _neutronVariables.CreateStoreOrderWithRts = CheckBoxCreateStoreOrderWithRts.Checked;
            _neutronVariables.ShuttleEnabled = CheckBoxShuttleEnabled.Checked;
            _neutronVariables.SendAllPicksToHost = CheckBoxSendAllPicksToHost.Checked;
            _neutronVariables.UsePrimeBin = CheckBoxUsePrimeBin.Checked;
            _neutronVariables.PickMethod = GetPickMethod();
            _neutronVariables.UseLAC = CheckBoxUseLAC.Checked;
            _neutronVariables.UseMenuSecurity = CheckBoxUseMenuSecurity.Checked;
            _neutronVariables.UseReturnToStock = CheckBoxUseReturnToStock.Checked;
            _neutronVariables.StationNumber = int.Parse(TextBoxStationNumber.Text.ToString());
            _neutronVariables.DeviceDriver = ComboBoxDeviceDriver.SelectedItem.ToString();
            _neutronVariables.SimulationMode = CheckBoxSimulationMode.Checked;
            _neutronVariables.LogLevel = Convert.ToInt32(NumericUpDownLogLevel.Value);
            _neutronVariables.SlotNameType = ComboBoxSlotFormat.SelectedItem.ToString();
            _neutronVariables.AutoLogOff = CheckBoxAutoLogOff.Checked;
            _neutronVariables.CheckForUsedItem = CheckBoxCheckForUsedItem.Checked;
            _neutronVariables.RunLoaderOnStartup = CheckBoxRunLoaderOnStartup.Checked;
            _neutronVariables.DisplaysEnabled = CheckBoxDisplaysEnabled.Checked;
            _neutronVariables.EnableDocumentPrinter = CheckBoxEnableDocumentPrinter.Checked;
            _neutronVariables.EnableLabelPrinter = CheckBoxEnableLabelPrinter.Checked;
            _neutronVariables.PinLoginOnly = CheckBoxPinLoginOnly.Checked;
            _neutronVariables.PickBatchSize = ComboBoxPickBatchSize.SelectedItem.ToString().ParseInt();
            _neutronVariables.StoreBatchSize = ComboBoxStoreBatchSize.SelectedItem.ToString().ParseInt();
            _neutronVariables.BliEnabled = CheckBoxBliEnabled.Checked;
            _neutronVariables.ShiEnabled = CheckBoxShiEnabled.Checked;
            _neutronVariables.ParkPositionAfterBatch = CheckBoxParkPositionAfterBatch.Checked;
            _neutronVariables.UsePr1Processor = CheckBoxUsePr1Processor.Checked;
            _neutronVariables.UsePr1StyleInputProcessor = CheckBoxUsePr1StyleInputProcessor.Checked;
            _neutronVariables.UsePr1StyleOutputProcessor = CheckBoxUsePr1StyleOutputProcessor.Checked;
            _neutronVariables.FieldDelimiter = TextBoxFieldDelimiter.Text;
            _neutronVariables.AutoEnlargeImage = CheckBoxAutoEnlargeImage.Checked;
            _neutronVariables.IptiDisplays = CheckBoxIptiDisplays.Checked;
            _neutronVariables.LoadRackOrders = CheckBoxLoadRackOrders.Checked;
            _neutronVariables.SerialPicking = CheckBoxSerialPicking.Checked;
            _neutronVariables.PrintPreview = CheckBoxPrintPreview.Checked;
            _neutronVariables.UpdateItemDefinitionDescription = CheckBoxUpdateItemDefinitionDescription.Checked;
            _neutronVariables.PrintPackingListStart = CheckBoxPrintPackingListStart.Checked;
            _neutronVariables.PrintPackingListEnd = CheckBoxPrintPackingListEnd.Checked;
            _neutronVariables.PrintPackingListManual = CheckBoxPrintPackingListManual.Checked;

            _jsonData.SaveFile<NeutronVariables>(_neutronVariables);

            UpdateSimulationMode();

        }

        private void UpdateSimulationMode()
        {
            var recs = _repoHardwareDevices.All().ToList();
            if (_neutronVariables.SimulationMode)
            {
                foreach (var rec in recs)
                {
                    rec.SimulationMode = true;
                    rec.LogLevel = 8;
                    _repoHardwareDevices.Update(rec);
                }
            }
            else
            {
                foreach (var rec in recs)
                {
                    rec.SimulationMode = false;
                    rec.LogLevel = 2;
                    _repoHardwareDevices.Update(rec);
                }
            }
        }

        private void MBVariables_Click(object sender, EventArgs e)
        {

            LabelFormTitle.Text = "Options";
            LabelFormTitle.BackColor = Color.RoyalBlue;
            tabControl1.SelectedTab = DisplayListing;

            var neutronVariables = _jsonData.LoadFile<NeutronVariables>();

            CheckBoxCreateStoreOrderWithRts.Checked = neutronVariables.CreateStoreOrderWithRts;
            CheckBoxShuttleEnabled.Checked = neutronVariables.ShuttleEnabled;
            CheckBoxSendAllPicksToHost.Checked = neutronVariables.SendAllPicksToHost;
            CheckBoxUsePrimeBin.Checked = neutronVariables.UsePrimeBin;
            CheckBoxUseLAC.Checked = neutronVariables.UseLAC;
            CheckBoxUseMenuSecurity.Checked = neutronVariables.UseMenuSecurity;
            CheckBoxUseReturnToStock.Checked = neutronVariables.UseReturnToStock;
            TextBoxStationNumber.Text = neutronVariables.StationNumber.ToString();
            ComboBoxDeviceDriver.SelectedIndex = ComboBoxDeviceDriver.FindStringExact(neutronVariables.DeviceDriver);
            CheckBoxSimulationMode.Checked = neutronVariables.SimulationMode;
            NumericUpDownLogLevel.Value = neutronVariables.LogLevel == 0 ? NumericUpDownLogLevel.Minimum : neutronVariables.LogLevel;
            ComboBoxSlotFormat.SelectedIndex = ComboBoxSlotFormat.FindStringExact(neutronVariables.SlotNameType);
            CheckBoxAutoLogOff.Checked = neutronVariables.AutoLogOff;
            CheckBoxCheckForUsedItem.Checked = neutronVariables.CheckForUsedItem;
            CheckBoxRunLoaderOnStartup.Checked = neutronVariables.RunLoaderOnStartup;
            CheckBoxDisplaysEnabled.Checked = neutronVariables.DisplaysEnabled;
            CheckBoxEnableDocumentPrinter.Checked = neutronVariables.EnableDocumentPrinter;
            CheckBoxEnableLabelPrinter.Checked = neutronVariables.EnableLabelPrinter;
            CheckBoxPinLoginOnly.Checked = neutronVariables.PinLoginOnly;
            ComboBoxPickBatchSize.SelectedIndex = ComboBoxPickBatchSize.FindStringExact(neutronVariables.PickBatchSize.ToString());
            ComboBoxStoreBatchSize.SelectedIndex = ComboBoxStoreBatchSize.FindStringExact(neutronVariables.StoreBatchSize.ToString());
            CheckBoxBliEnabled.Checked = neutronVariables.BliEnabled;
            CheckBoxShiEnabled.Checked = neutronVariables.ShiEnabled;
            CheckBoxParkPositionAfterBatch.Checked = neutronVariables.ParkPositionAfterBatch;
            CheckBoxUsePr1Processor.Checked = neutronVariables.UsePr1Processor;
            CheckBoxUsePr1StyleInputProcessor.Checked = neutronVariables.UsePr1StyleInputProcessor;
            CheckBoxUsePr1StyleOutputProcessor.Checked = neutronVariables.UsePr1StyleOutputProcessor;
            TextBoxFieldDelimiter.Text = neutronVariables.FieldDelimiter;
            CheckBoxAutoEnlargeImage.Checked = neutronVariables.AutoEnlargeImage;
            CheckBoxIptiDisplays.Checked = neutronVariables.IptiDisplays;
            CheckBoxLoadRackOrders.Checked = neutronVariables.LoadRackOrders;
            CheckBoxSerialPicking.Checked = neutronVariables.SerialPicking;
            CheckBoxPrintPreview.Checked = neutronVariables.PrintPreview;
            CheckBoxUpdateItemDefinitionDescription.Checked = neutronVariables.UpdateItemDefinitionDescription;
            CheckBoxPrintPackingListStart.Checked = neutronVariables.PrintPackingListStart;
            CheckBoxPrintPackingListEnd.Checked = neutronVariables.PrintPackingListEnd;
            CheckBoxPrintPackingListManual.Checked = neutronVariables.PrintPackingListManual;

            SetPickMethod(neutronVariables.PickMethod);
        }

        private void MBUtilitiesSpare1_Click(object sender, EventArgs e)
        {
        }

        private void MButtonViewEdit_Click(object sender, EventArgs e)
        {

        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            DocumentPrinter = new DocumentPrinterPreferences
            {
                PrinterName = TextBoxDocumentPrinter.Text,
                LeftMargin = int.Parse(TextBoxDocumentLeftMargin.Text),
                TopMargin = int.Parse(TextBoxDocumentTopMargin.Text),
                RightMargin = int.Parse(TextBoxDocumentRightMargin.Text),
                BottomMargin = int.Parse(TextBoxDocumentBottomMargin.Text)
            };
            try
            {
                _jsonData.SaveFile<DocumentPrinterPreferences>(DocumentPrinter);

                //using (StreamWriter file = File.CreateText(documentPreferencesFilename))
                //{
                //    using (JsonWriter writer = new JsonTextWriter(file))
                //    {
                //        var serializer = new JsonSerializer();
                //        serializer.Serialize(writer, documentPrinterPreferences);
                //    }
                //}
            }
            catch (Exception)
            {
            }

            LabelPrinter = new LabelPrinterPreferences
            {
                PrinterName = TextBoxLabelPrinter.Text,
                HomeX = int.Parse(TextBoxLabelHomeX.Text),
                HomeY = int.Parse(TextBoxLabelHomeY.Text)
            };
            try
            {
                _jsonData.SaveFile<LabelPrinterPreferences>(LabelPrinter);

                //using (StreamWriter file = File.CreateText(labelPreferencesFilename))
                //{
                //    using (JsonWriter writer = new JsonTextWriter(file))
                //    {
                //        var serializer = new JsonSerializer();
                //        serializer.Serialize(writer, labelPrinterPreferences);
                //    }
                //}
            }
            catch (Exception)
            {
            }
        }

        private void ButtonDocumentPrinter_Click(object sender, EventArgs e)
        {
            var pd = new PrintDialog();
            pd.ShowDialog();
            TextBoxDocumentPrinter.Text = pd.PrinterSettings.PrinterName;
        }

        private void ButtonLabelPrinter_Click(object sender, EventArgs e)
        {
            var pd = new PrintDialog();
            pd.ShowDialog();
            TextBoxLabelPrinter.Text = pd.PrinterSettings.PrinterName;
        }

        public DocumentPrinterPreferences LoadDocumentPrinterPreferences()
        {
            var result = new DocumentPrinterPreferences();
            result = _jsonData.LoadFile<DocumentPrinterPreferences>();

            //using (StreamReader r = new StreamReader(this.documentPreferencesFilename))
            //{
            //    string json = r.ReadToEnd();
            //    result = JsonConvert.DeserializeObject<DocumentPrinterPreferences>(json);
            //}
            return result;
        }

        public LabelPrinterPreferences LoadLabelPrinterPreferences()
        {
            var result = new LabelPrinterPreferences();
            result = _jsonData.LoadFile<LabelPrinterPreferences>();

            //using (StreamReader r = new StreamReader(this.labelPreferencesFilename))
            //{
            //    string json = r.ReadToEnd();
            //    result = JsonConvert.DeserializeObject<LabelPrinterPreferences>(json);
            //}
            return result;
        }

        private void MBPrinterSetup_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Printer Settings";
            LabelFormTitle.BackColor = Color.RoyalBlue;
            tabControl1.SelectedTab = PrintSettings;
        }

        private void MBPickListBack_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Devices";
            LabelFormTitle.BackColor = Color.RoyalBlue;
            tabControl1.SelectedTab = Main;
        }

        private void ButtonPrintTextDocument_Click(object sender, EventArgs e)
        {
            var order = TextBoxTestOrderNumber.Text;
            DocumentToPrint.Print(1, order, DocumentPrinter);
        }

        private void ButtonPrintTestToteLabel_Click(object sender, EventArgs e)
        {
            try
            {
                var order = TextBoxTestOrderNumber.Text;
                var ord = _repoOrders.FindBy(r => r.Ord1 == order).FirstOrDefault();
                if (ord != null)
                {
                    ToteToPrint.Print(1, ord, LabelPrinter);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Printing Tote Label.  {ex.Message} \r\n {ex.InnerException}");
            }
        }

        private void ButtonPrintTestShortReport_Click(object sender, EventArgs e)
        {

        }


        private void MtLookups_Click(object sender, EventArgs e)
        {

        }

        private void MBLookups_Click(object sender, EventArgs e)
        {

            Hide();
            using (MetroForm frm = new FrmLookups())
            {
                var result = frm.ShowDialog();
                Show();
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void ButtonNomenclature_Click(object sender, EventArgs e)
        {
            var nomenclature = _jsonData.LoadFile<Nomenclature>();
            TextBoxPickAccept.Text = nomenclature.MBPickAccept;
            TextBoxStoreAccept.Text = nomenclature.MBStoreAccept;
            TextBoxDelete.Text = nomenclature.MBDelete;
            TextBoxDevice.Text = nomenclature.LabelDevice;
            TextBoxTray.Text = nomenclature.LabelTray;
            TextBoxOver.Text = nomenclature.LabelOver;
            TextBoxBack.Text = nomenclature.LabelBack;

            tabControl1.SelectedTab = Nomenclature;
        }

        private void MbNomenclatureSave_Click(object sender, EventArgs e)
        {
            var nomenclature = new Nomenclature();
            nomenclature.MBPickAccept = TextBoxPickAccept.Text;
            nomenclature.MBStoreAccept = TextBoxStoreAccept.Text;
            nomenclature.MBDelete = TextBoxDelete.Text;
            nomenclature.LabelDevice = TextBoxDevice.Text;
            nomenclature.LabelTray = TextBoxTray.Text;
            nomenclature.LabelOver = TextBoxOver.Text;
            nomenclature.LabelBack = TextBoxBack.Text;

            _jsonData.SaveFile<Nomenclature>(nomenclature);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            var counter = 0;

            using (var db = new NeutronDb())
            {
                var orders = db.Orders.Where(d => d.OrderStatusId != 6).ToList();
                MessageBox.Show($"Orders to Process: {orders.Count}");

                foreach (var order in orders)
                {

                    var linesNotComplete = db.OrderDetails.Where(r => r.OrderId == order.Id && r.LineStatusId != 6).ToList();
                    if (linesNotComplete.Count == 0)
                    {
                        order.OrderStatusId = 6;
                        counter++;
                        GlobalVar.HistoryManager.SaveHistory(ActionCode.OrderComplete, order: order);
                        db.SaveChanges();

                    }

                    //var details = db.OrderDetails.Where(o => o.OrderId == order.Id).ToList();

                    //var ordercomplete = true;
                    //foreach (var orderDetail in details)
                    //{
                    //    if (orderDetail.LineStatusId != 6)
                    //    {
                    //        ordercomplete = false;
                    //        break;
                    //    }
                    //}

                    //if (ordercomplete)
                    //{
                    //    order.OrderStatusId = 6;
                    //}
                    //db.SaveChanges();
                }

                MessageBox.Show($"Process Complete: Records: {counter}");

            }
        }

        //private void CheckForOrderComplete(Order order)
        //{
        //    var repoOrderDetails = new GenericRepository<OrderDetail>(new NeutronDb());
        //    var repoOrders = new GenericRepository<Order>(new NeutronDb());

        //    var linesNotComplete = repoOrderDetails.FindBy(r => r.OrderId == order.Id).Where(r => r.LineStatusId != 6)
        //        .ToList();
        //    if (linesNotComplete.Count != 0) return;
        //    order.OrderStatusId = 6;
        //    GlobalVar.HistoryManager.SaveHistory(ActionCode.OrderComplete, order: order);
        //    repoOrders.Update(order);

        //}
    }

}