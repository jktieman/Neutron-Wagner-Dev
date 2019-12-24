using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
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
using System.Drawing.Printing;
using System.Threading.Tasks;
using EnumsNET;
using Equin.ApplicationFramework;
using Neutron.Classes;
using Neutron.Extensions;
using NeutronCore.Enums;
using NeutronData.BaseClasses;
using NeutronData.Models.Lookups;
using CommunicationType = NeutronCore.Enums.CommunicationType;
using DeviceType = NeutronData.Models.Lookups.DeviceType;

//using CommunicationType = NeutronCore.Enums.CommunicationType;
//using DeviceType = NeutronCore.Enums.DeviceType;

namespace Neutron.Forms
{
    public partial class FrmUtilities : MetroForm
    {
        private readonly GenericRepository<HardwareDevice> _repoHardwareDevices = new GenericRepository<HardwareDevice>(new NeutronDb());
        private readonly GenericRepository<Order> _repoOrders = new GenericRepository<Order>(new NeutronDb());
        private readonly StationRepository _repoStation = new StationRepository();
        private readonly GenericRepository<NeutronData.Models.Lookups.DeviceType> _repoDeviceTypes = new GenericRepository<NeutronData.Models.Lookups.DeviceType>(new NeutronDb());
        private readonly GenericRepository<NeutronData.Models.Lookups.CommunicationType> _repoCommunicationTypes = new GenericRepository<NeutronData.Models.Lookups.CommunicationType>(new NeutronDb());
        private readonly GenericRepository<TcpConfiguration> _repoTcpConfigurations = new GenericRepository<TcpConfiguration>(new NeutronDb());
        private readonly GenericRepository<SerialConfiguration> _repoSerialConfigurations = new GenericRepository<SerialConfiguration>(new NeutronDb());


        private BindingSource _bindingSourceHardwareDevices = new BindingSource();
        public bool CloseButtonPressed { get; set; }
        //public bool CloseForm = false;
        private readonly IJsonData _jsonData;
        public HardwareDeviceView CurrentItem;
        public DocumentPrinterPreferences DocumentPrinter;
        public LabelPrinterPreferences LabelPrinter;
        private readonly NeutronVariables _neutronVariables;

        //Lookup variables
        private List<LookupTable> _lookupTables = new List<LookupTable>();
        private string _currentTableName = string.Empty;
        private List<LookupData> _currentRecs;
        private BindingSource _bindingSource;

        public FrmUtilities(IJsonData jsonData, NeutronVariables neutronVariables)
        {
            InitializeComponent();
            KeyPreview = true;
            _jsonData = jsonData;
            _neutronVariables = neutronVariables;
            HideTabControlTabs();
            mlUserInfo.Text = GlobalVar.User?.UserInfo;
            CloseButtonPressed = false;
            SetupGrids();
            LabelVersion.Text = ($"{ApplicationVersion.Major}.{ApplicationVersion.Minor}.{ApplicationVersion.Build}.{ApplicationVersion.Revision}.{ApplicationVersion.MajorRevision}.{ApplicationVersion.MinorRevision}");


        }

        private void SetupDeviceForms()
        {
            ComboBoxViewEditDeviceStation.DataSource = _repoStation.Lookup();
            ComboBoxViewEditDeviceStation.DisplayMember = "Name";
            ComboBoxViewEditDeviceStation.ValueMember = "Id";

            ComboBoxViewEditDeviceType.DataSource = _repoDeviceTypes.All();
            ComboBoxViewEditDeviceType.DisplayMember = "Name";
            ComboBoxViewEditDeviceType.ValueMember = "Id";

            ComboBoxViewEditCommunicationType.DataSource = _repoCommunicationTypes.All();
            ComboBoxViewEditCommunicationType.DisplayMember = "Name";
            ComboBoxViewEditCommunicationType.ValueMember = "Id";

            ComboBoxViewEditTcpConfiguration.DataSource = _repoTcpConfigurations.All();
            ComboBoxViewEditTcpConfiguration.DisplayMember = "Name";
            ComboBoxViewEditTcpConfiguration.ValueMember = "Id";

            ComboBoxViewEditSerialConfiguration.DataSource = _repoSerialConfigurations.All();
            ComboBoxViewEditSerialConfiguration.DisplayMember = "Name";
            ComboBoxViewEditSerialConfiguration.ValueMember = "Id";


            ComboBoxNewDeviceStation.DataSource = _repoStation.Lookup();
            ComboBoxNewDeviceStation.DisplayMember = "Name";
            ComboBoxNewDeviceStation.ValueMember = "Id";

            ComboBoxNewDeviceType.DataSource = _repoDeviceTypes.All();
            ComboBoxNewDeviceType.DisplayMember = "Name";
            ComboBoxNewDeviceType.ValueMember = "Id";

            ComboBoxNewCommunicationType.DataSource = _repoCommunicationTypes.All();
            ComboBoxNewCommunicationType.DisplayMember = "Name";
            ComboBoxNewCommunicationType.ValueMember = "Id";

            ComboBoxNewTcpConfiguration.DataSource = _repoTcpConfigurations.All();
            ComboBoxNewTcpConfiguration.DisplayMember = "Name";
            ComboBoxNewTcpConfiguration.ValueMember = "Id";

            ComboBoxNewSerialConfiguration.DataSource = _repoSerialConfigurations.All();
            ComboBoxNewSerialConfiguration.DisplayMember = "Name";
            ComboBoxNewSerialConfiguration.ValueMember = "Id";

            //ComboBoxNewStation.SelectedIndex = ComboBoxNewStation.FindString(_station.Name);
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
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
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
                DataPropertyName = "CarrierDepth",
                HeaderText = @"Carrier Depth",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "CarrierDepth"
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
                HeaderText = @"Device Type",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "DeviceTypeName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CommunicationTypeName",
                HeaderText = @"Communication Type",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "CommunicationTypeName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TcpConfigurationName",
                HeaderText = @"TCP Configuration",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "TcpConfigurationName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SerialConfigurationName",
                HeaderText = @"Serial Configuration",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "SerialConfigurationName",
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

            tabControl2.Appearance = TabAppearance.FlatButtons;
            tabControl2.ItemSize = new Size(0, 1);
            tabControl2.SizeMode = TabSizeMode.Fixed;
            foreach (TabPage tab in tabControl2.TabPages)
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

        public int IndexOf(BindingSource bs, int value)
        {
            if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
            //var count = bs.Count;
            var itemIndex = -1;
            //for (var i = 0; i < count; i++)
            //{
            //    var rec = ((ObjectView<HardwareDeviceView>)bs[i]).Object?.Id;
            //    if (rec == value)
            //    {
            //        itemIndex = i;
            //        break;
            //    }
            //}
            // value = 44;
            itemIndex = bs.Find("Id", value);
            return itemIndex;
        }

        private int GetRecordCount(BindingSource bs)
        {
            var count = bs.Count;
            LabelRecordCount.Text = $"Records: {count.ToString()}";
            return count;
        }
    
        private void MBDevices_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Device Listing";
            LabelFormTitle.BackColor = Color.RoyalBlue;
            LoadHardwareDevices();
            tabControl1.SelectedTab = HardwareDevices;
        }

        #region Button Clicks

        #endregion

        private void MBInterfaceFilesBack_Click(object sender, EventArgs e)
        {
            BackToMain();
        }

        private void MbNomenclatureBack_Click(object sender, EventArgs e)
        {
            BackToMain();
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
            // _neutronVariables = new NeutronVariables();
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

        private void MBOptions_Click(object sender, EventArgs e)
        {

            LabelFormTitle.Text = "Options";
            LabelFormTitle.BackColor = Color.RoyalBlue;
            tabControl1.SelectedTab = DisplayListing;

            //var neutronVariables = _jsonData.LoadFile<NeutronVariables>();

            CheckBoxCreateStoreOrderWithRts.Checked = _neutronVariables.CreateStoreOrderWithRts;
            CheckBoxShuttleEnabled.Checked = _neutronVariables.ShuttleEnabled;
            CheckBoxSendAllPicksToHost.Checked = _neutronVariables.SendAllPicksToHost;
            CheckBoxUsePrimeBin.Checked = _neutronVariables.UsePrimeBin;
            CheckBoxUseLAC.Checked = _neutronVariables.UseLAC;
            CheckBoxUseMenuSecurity.Checked = _neutronVariables.UseMenuSecurity;
            CheckBoxUseReturnToStock.Checked = _neutronVariables.UseReturnToStock;
            TextBoxStationNumber.Text = _neutronVariables.StationNumber.ToString();
            ComboBoxDeviceDriver.SelectedIndex = ComboBoxDeviceDriver.FindStringExact(_neutronVariables.DeviceDriver);
            CheckBoxSimulationMode.Checked = _neutronVariables.SimulationMode;
            NumericUpDownLogLevel.Value = _neutronVariables.LogLevel == 0 ? NumericUpDownLogLevel.Minimum : _neutronVariables.LogLevel;
            ComboBoxSlotFormat.SelectedIndex = ComboBoxSlotFormat.FindStringExact(_neutronVariables.SlotNameType);
            CheckBoxAutoLogOff.Checked = _neutronVariables.AutoLogOff;
            CheckBoxCheckForUsedItem.Checked = _neutronVariables.CheckForUsedItem;
            CheckBoxRunLoaderOnStartup.Checked = _neutronVariables.RunLoaderOnStartup;
            CheckBoxDisplaysEnabled.Checked = _neutronVariables.DisplaysEnabled;
            CheckBoxEnableDocumentPrinter.Checked = _neutronVariables.EnableDocumentPrinter;
            CheckBoxEnableLabelPrinter.Checked = _neutronVariables.EnableLabelPrinter;
            CheckBoxPinLoginOnly.Checked = _neutronVariables.PinLoginOnly;
            ComboBoxPickBatchSize.SelectedIndex = ComboBoxPickBatchSize.FindStringExact(_neutronVariables.PickBatchSize.ToString());
            ComboBoxStoreBatchSize.SelectedIndex = ComboBoxStoreBatchSize.FindStringExact(_neutronVariables.StoreBatchSize.ToString());
            CheckBoxBliEnabled.Checked = _neutronVariables.BliEnabled;
            CheckBoxShiEnabled.Checked = _neutronVariables.ShiEnabled;
            CheckBoxParkPositionAfterBatch.Checked = _neutronVariables.ParkPositionAfterBatch;
            CheckBoxUsePr1Processor.Checked = _neutronVariables.UsePr1Processor;
            CheckBoxUsePr1StyleInputProcessor.Checked = _neutronVariables.UsePr1StyleInputProcessor;
            CheckBoxUsePr1StyleOutputProcessor.Checked = _neutronVariables.UsePr1StyleOutputProcessor;
            TextBoxFieldDelimiter.Text = _neutronVariables.FieldDelimiter;
            CheckBoxAutoEnlargeImage.Checked = _neutronVariables.AutoEnlargeImage;
            CheckBoxIptiDisplays.Checked = _neutronVariables.IptiDisplays;
            CheckBoxLoadRackOrders.Checked = _neutronVariables.LoadRackOrders;
            CheckBoxSerialPicking.Checked = _neutronVariables.SerialPicking;
            CheckBoxPrintPreview.Checked = _neutronVariables.PrintPreview;
            CheckBoxUpdateItemDefinitionDescription.Checked = _neutronVariables.UpdateItemDefinitionDescription;
            CheckBoxPrintPackingListStart.Checked = _neutronVariables.PrintPackingListStart;
            CheckBoxPrintPackingListEnd.Checked = _neutronVariables.PrintPackingListEnd;
            CheckBoxPrintPackingListManual.Checked = _neutronVariables.PrintPackingListManual;

            SetPickMethod(_neutronVariables.PickMethod);
        }

        private void MBPrintSetUpSave_Click(object sender, EventArgs e)
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Saving Document Printer Information.  {ex.Message} {Environment.NewLine} {ex.InnerException}");
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Saving Label Printer Information.  {ex.Message} {Environment.NewLine} {ex.InnerException}");
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

        public void LoadDocumentPrinterPreferences()
        {
            DocumentPrinter = _jsonData.LoadFile<DocumentPrinterPreferences>();
            TextBoxDocumentPrinter.Text = DocumentPrinter.PrinterName;
            TextBoxDocumentLeftMargin.Text = DocumentPrinter.LeftMargin.ToString();
            TextBoxDocumentTopMargin.Text = DocumentPrinter.TopMargin.ToString();
            TextBoxDocumentRightMargin.Text = DocumentPrinter.RightMargin.ToString();
            TextBoxDocumentBottomMargin.Text = DocumentPrinter.BottomMargin.ToString();
        }

        public void LoadLabelPrinterPreferences()
        {
            LabelPrinter = _jsonData.LoadFile<LabelPrinterPreferences>();
            TextBoxLabelPrinter.Text = LabelPrinter.PrinterName;
            TextBoxLabelHomeX.Text = LabelPrinter.HomeX.ToString();
            TextBoxLabelHomeY.Text = LabelPrinter.HomeY.ToString();
        }

        private void MBPrinterSetup_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Printer Settings";
            LabelFormTitle.BackColor = Color.RoyalBlue;
            LoadDocumentPrinterPreferences();
            LoadLabelPrinterPreferences();
            tabControl1.SelectedTab = PrintSettings;
        }

        private void ButtonPrintTestDocument_Click(object sender, EventArgs e)
        {
            var printer = GetCurrentDocumentPrinter();
            if (printer != null)
            {
                var order = TextBoxTestOrderNumber.Text;
                if (!string.IsNullOrEmpty(order))
                {
                    var ord = _repoOrders.FindBy(r => r.Ord1 == order).FirstOrDefault();
                    if (ord != null)
                    {
                        var packingList = GetPackingList(ord.Id);
                        foreach (var pack in packingList)
                        {
                            pack.BatchPosition = "1";
                        }

                        DocumentToPrint.PrintPackingList(packingList, printer, _neutronVariables.PrintPreview);
                    }
                    else
                    {
                        MessageBox.Show("Order not found.");
                    }
                }
                else
                {
                    MessageBox.Show("Invalid Order.");
                }
            }
            else
            {
                MessageBox.Show("Invalid Printer.");
            }
        }

        private List<PackingList> GetPackingList(int orderId)
        {
            var outs = new List<PackingList>();
            using (var context = new NeutronDb())
            {
                var paramOrderId = new SqlParameter(parameterName: "@ORDERID", value: orderId);
                outs = context.Database.SqlQuery<PackingList>("usp_GetPackingList @ORDERID", new object[] { paramOrderId }).ToList();
            }
            return outs;
        }

        private DocumentPrinterPreferences GetCurrentDocumentPrinter()
        {
            DocumentPrinterPreferences printer = null;
            if (ValidPrinterName(TextBoxDocumentPrinter.Text))
            {
                printer = new DocumentPrinterPreferences
                {
                    PrinterName = TextBoxDocumentPrinter.Text,
                    LeftMargin = int.Parse(TextBoxDocumentLeftMargin.Text),
                    TopMargin = int.Parse(TextBoxDocumentTopMargin.Text),
                    RightMargin = int.Parse(TextBoxDocumentRightMargin.Text),
                    BottomMargin = int.Parse(TextBoxDocumentBottomMargin.Text)
                };
            }

            return printer;
        }

        private static bool ValidPrinterName(string printer)
        {
            return PrinterSettings.InstalledPrinters.Cast<string>().Any(installedPrinter => installedPrinter == printer);
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
                MessageBox.Show($"Error Printing Tote Label.  {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
        }

        private void ButtonPrintTestShortReport_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Set up Test Short Report.");
        }

        private void MBLookups_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Lookup Tables";
            LabelFormTitle.BackColor = Color.RoyalBlue;
            SetupLookupGrid();
            LoadLookups();
            tabControl1.SelectedTab = ManageLookups;
        }

        private void SetupLookupGrid()
        {
            DataGridViewLookups.AutoGenerateColumns = false;

            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = "Id",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Id",
                Visible = false,
            };
            DataGridViewLookups.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Sequence",
                HeaderText = "Sequence",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Sequence"
            };
            DataGridViewLookups.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Name",
                HeaderText = "Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                Name = "Name"
            };
            DataGridViewLookups.Columns.Add(col);
        }

        private void MBNomenclature_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Nomenclature";
            LabelFormTitle.BackColor = Color.RoyalBlue;
            LoadNomenclature();
            tabControl1.SelectedTab = Nomenclature;
        }

        private void LoadNomenclature()
        {
            var nomenclature = _jsonData.LoadFile<Nomenclature>();
            TextBoxPickAccept.Text = nomenclature.MBPickAccept;
            TextBoxStoreAccept.Text = nomenclature.MBStoreAccept;
            TextBoxDelete.Text = nomenclature.MBDelete;
            TextBoxDevice.Text = nomenclature.LabelDevice;
            TextBoxTray.Text = nomenclature.LabelTray;
            TextBoxOver.Text = nomenclature.LabelOver;
            TextBoxBack.Text = nomenclature.LabelBack;
        }

        private void MbNomenclatureSave_Click(object sender, EventArgs e)
        {
            var nomenclature = new Nomenclature
            {
                MBPickAccept = TextBoxPickAccept.Text,
                MBStoreAccept = TextBoxStoreAccept.Text,
                MBDelete = TextBoxDelete.Text,
                LabelDevice = TextBoxDevice.Text,
                LabelTray = TextBoxTray.Text,
                LabelOver = TextBoxOver.Text,
                LabelBack = TextBoxBack.Text
            };

            _jsonData.SaveFile<Nomenclature>(nomenclature);
        }

        private void MBPrintSetUpBack_Click(object sender, EventArgs e)
        {
            BackToMain();
        }

        private void MBHardwareDevicesBack_Click(object sender, EventArgs e)
        {
            BackToMain();
        }

        private void BackToMain()
        {
            LabelFormTitle.Text = "Utilities";
            LabelFormTitle.BackColor = Color.RoyalBlue;
            tabControl1.SelectedTab = Main;
        }

        //Lookup Tables

        private void MBManageLookupsBack_Click(object sender, EventArgs e)
        {
            BackToMain();
        }

        private int RefreshData(int recId = 0)
        {
            _currentTableName = ((LookupTable)ListBoxCodeNames.SelectedItem).TableName;
            LabelLookupName.Text = ((LookupTable)ListBoxCodeNames.SelectedItem).Name;
            _currentRecs = GetTableData(_currentTableName);
            var blv = new BindingListView<LookupData>(_currentRecs);
            _bindingSource = new BindingSource { DataSource = blv };
            DataGridViewLookups.DataSource = _bindingSource.DataSource;

            var idx = 0;

            if (GetRecordCount() <= 0) return idx;
            if (recId != 0)
            {
                idx = IndexOf(_bindingSource, recId);
            }
            DataGridViewLookups.FirstDisplayedScrollingRowIndex = DataGridViewLookups.Rows[idx].Index;
            DataGridViewLookups.Refresh();
            DataGridViewLookups.CurrentCell = DataGridViewLookups.Rows[idx].Cells[1];
            DataGridViewLookups.Rows[idx].Selected = true;
            return idx;
        }

        private int GetRecordCount()
        {
            int count = DataGridViewLookups.RowCount;
            return count;
        }

        private void LoadLookups()
        {
            try
            {
                using (var context = new NeutronDb())
                {
                    _lookupTables = context.LookupTables.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading Lookup Tables. " + ex.Message);
            }
            ListBoxCodeNames.DataSource = _lookupTables;
        }

        private List<LookupData> GetTableData(string tableName)
        {
            _currentTableName = tableName;
            var recs = new List<LookupData>();
            try
            {
                using (var context = new NeutronDb())
                {
                    switch (tableName)
                    {
                        case "SizeCodes":
                            recs = context.SizeCodes.Select(s => new LookupData() { Id = s.Id, Name = s.Name, Sequence = s.Sequence }).OrderBy(o => o.Sequence).ToList();
                            break;
                        case "VelocityCodes":
                            recs = context.VelocityCodes.Select(s => new LookupData() { Id = s.Id, Name = s.Name, Sequence = s.Sequence }).OrderBy(o => o.Sequence).ToList();
                            break;
                        case "HeightCodes":
                            recs = context.HeightCodes.Select(s => new LookupData() { Id = s.Id, Name = s.Name, Sequence = s.Sequence }).OrderBy(o => o.Sequence).ToList();
                            break;
                        case "LocationCodes":
                            recs = context.LocationCodes.Select(s => new LookupData() { Id = s.Id, Name = s.Name, Sequence = s.Sequence }).OrderBy(o => o.Sequence).ToList();
                            break;
                        case "ShipMethods":
                            recs = context.ShipMethods.Select(s => new LookupData() { Id = s.Id, Name = s.Name, Sequence = s.Sequence }).OrderBy(o => o.Sequence).ToList();
                            break;
                        case "Shippers":
                            recs = context.Shippers.Select(s => new LookupData() { Id = s.Id, Name = s.Name, Sequence = s.Sequence }).OrderBy(o => o.Sequence).ToList();
                            break;
                        case "UnitOfIssues":
                            recs = context.UnitOfIssues.Select(s => new LookupData() { Id = s.Id, Name = s.Name, Sequence = s.Sequence }).OrderBy(o => o.Sequence).ToList();
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading Lookup Tables. " + ex.Message);
            }
            return recs;
        }

        private void ListBoxCodeNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshData();
        }

        private async void MButtonSave_Click(object sender, EventArgs e)
        {
            var s = _currentRecs;
            var t = DataGridViewLookups.DataSource;
            await UpdateTableData();
        }

        private async Task UpdateTableData()
        {
            try
            {
                using (var context = new NeutronDb())
                {
                    foreach (var rec in _currentRecs)
                    {
                        switch (_currentTableName)
                        {
                            case "SizeCodes":
                                var sizeCode = new SizeCode() { Id = rec.Id, Name = rec.Name, Sequence = rec.Sequence };
                                context.SizeCodes.AddOrUpdate(sizeCode);
                                await context.SaveChangesAsync();
                                break;
                            case "VelocityCodes":
                                var velocityCode = new VelocityCode() { Id = rec.Id, Name = rec.Name, Sequence = rec.Sequence };
                                context.VelocityCodes.AddOrUpdate(velocityCode);
                                await context.SaveChangesAsync();
                                break;
                            case "HeightCodes":
                                var heightCode = new HeightCode() { Id = rec.Id, Name = rec.Name, Sequence = rec.Sequence };
                                context.HeightCodes.AddOrUpdate(heightCode);
                                await context.SaveChangesAsync();
                                break;
                            case "LocationCodes":
                                var locationCode = new LocationCode() { Id = rec.Id, Name = rec.Name, Sequence = rec.Sequence };
                                context.LocationCodes.AddOrUpdate(locationCode);
                                await context.SaveChangesAsync();
                                break;
                            case "ShipMethods":
                                var shipMethod = new ShipMethod() { Id = rec.Id, Name = rec.Name, Sequence = rec.Sequence };
                                context.ShipMethods.AddOrUpdate(shipMethod);
                                await context.SaveChangesAsync();
                                break;
                            case "Shippers":
                                var shipper = new Shipper() { Id = rec.Id, Name = rec.Name, Sequence = rec.Sequence };
                                context.Shippers.AddOrUpdate(shipper);
                                await context.SaveChangesAsync();
                                break;
                            case "UnitOfIssues":
                                var item = new UnitOfIssue() { Id = rec.Id, Name = rec.Name, Sequence = rec.Sequence };
                                context.UnitOfIssues.AddOrUpdate(item);
                                await context.SaveChangesAsync();
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating Lookup Tables.  {ex.Message}");
            }
        }

        private void MBAddNew_Click(object sender, EventArgs e)
        {
            var r = new LookupData { Name = "", Sequence = 100 };
            _bindingSource.Add(r);
            DataGridViewLookups.DataSource = null;
            DataGridViewLookups.DataSource = _bindingSource.DataSource;
            DataGridViewLookups.Update();
        }

        private void MBPrintLookup_Click(object sender, EventArgs e)
        {
            CsvUtility.SaveToCsv(DataGridViewLookups);
        }

        private void MBPrintHardwareDevices_Click(object sender, EventArgs e)
        {
            CsvUtility.SaveToCsv(DataGridView1);
        }

        private void MBHardwareDevicesViewEdit_Click(object sender, EventArgs e)
        {
            SetupDeviceForms();
            ClearHardwareDeviceForm();
            LoadViewEditDeviceData();
            tabControl2.SelectedTab = ViewEdit;
        }

        private void LoadViewEditDeviceData()
        {
            var id = ((ObjectView<HardwareDeviceView>)_bindingSourceHardwareDevices.Current).Object.Id;
            var hardwareDevice = _repoHardwareDevices.FindByKey(id);

            TextBoxViewEditDeviceName.Text = hardwareDevice.Name;
            ComboBoxViewEditDeviceStation.SelectedValue = hardwareDevice.StationId;
            ComboBoxViewEditDeviceType.SelectedValue = hardwareDevice.DeviceTypeId;
            TextBoxViewEditDeviceNumber.Text = hardwareDevice.DeviceNumber.ToString();
            TextBoxViewEditNumberOfCarriers.Text = hardwareDevice.NumberOfCarriers.ToString();
            TextBoxViewEditCarrierWidth.Text = hardwareDevice.CarrierWidth.ToString();
            TextBoxViewEditCarrierDepth.Text = hardwareDevice.CarrierDepth.ToString();
            CheckBoxViewEditDeviceEnabled.Checked = hardwareDevice.Enabled;
            CheckBoxViewEditSimulationMode.Checked = hardwareDevice.SimulationMode;
            NumericUpDownViewEditDeviceLogLevel.Text = hardwareDevice.LogLevel.ToString();
            if (hardwareDevice.CommunicationTypeId != null)
            {
                ComboBoxViewEditCommunicationType.SelectedValue = hardwareDevice.CommunicationTypeId;
            }

            if (hardwareDevice.TcpConfigurationId != null)
            {
                ComboBoxViewEditTcpConfiguration.SelectedValue = hardwareDevice.TcpConfigurationId;
            }
            if (hardwareDevice.SerialConfigurationId != null)
            {
                ComboBoxViewEditSerialConfiguration.SelectedValue = hardwareDevice.SerialConfigurationId;
            }

            SetCommunicationDisplay(hardwareDevice.CommunicationTypeId);

        }

        private void SetCommunicationDisplay(int? communicationTypeId)
        {
            switch (communicationTypeId)
            {
                case null:
                    {
                        ComboBoxViewEditTcpConfiguration.Visible = false;
                        LabelViewEditDeviceTcpConfiguration.Visible = false;
                        ComboBoxViewEditSerialConfiguration.Visible = false;
                        LabelViewEditDeviceSerialConfiguration.Visible = false;

                        ComboBoxNewTcpConfiguration.Visible = false;
                        LabelNewDeviceTcpConfiguration.Visible = false;
                        ComboBoxNewSerialConfiguration.Visible = false;
                        LabelNewDeviceSerialConfiguration.Visible = false;
                        break;
                    }

                case 1:
                    {
                        ComboBoxViewEditTcpConfiguration.Visible = true;
                        ComboBoxViewEditTcpConfiguration.Location = new Point(803, 355);
                        LabelViewEditDeviceTcpConfiguration.Visible = true;
                        LabelViewEditDeviceTcpConfiguration.Location = new Point(601, 351);
                        ComboBoxViewEditSerialConfiguration.Visible = false;
                        LabelViewEditDeviceSerialConfiguration.Visible = false;

                        ComboBoxNewTcpConfiguration.Visible = true;
                        ComboBoxNewTcpConfiguration.Location = new Point(803, 355);
                        LabelNewDeviceTcpConfiguration.Visible = true;
                        LabelNewDeviceTcpConfiguration.Location = new Point(601, 351);
                        ComboBoxNewSerialConfiguration.Visible = false;
                        LabelNewDeviceSerialConfiguration.Visible = false;
                        break;
                    }
                case 2:
                    {
                        ComboBoxViewEditSerialConfiguration.Visible = true;
                        ComboBoxViewEditSerialConfiguration.Location = new Point(803, 355);
                        LabelViewEditDeviceSerialConfiguration.Visible = true;
                        LabelViewEditDeviceSerialConfiguration.Location = new Point(601, 351);
                        ComboBoxViewEditTcpConfiguration.Visible = false;
                        LabelViewEditDeviceTcpConfiguration.Visible = false;

                        ComboBoxNewSerialConfiguration.Visible = true;
                        ComboBoxNewSerialConfiguration.Location = new Point(803, 355);
                        LabelNewDeviceSerialConfiguration.Visible = true;
                        LabelNewDeviceSerialConfiguration.Location = new Point(601, 351);
                        ComboBoxNewTcpConfiguration.Visible = false;
                        LabelNewDeviceTcpConfiguration.Visible = false;
                        break;
                    }

                case 3:
                    {
                        ComboBoxViewEditTcpConfiguration.Visible = false;
                        LabelViewEditDeviceTcpConfiguration.Visible = false;
                        ComboBoxViewEditSerialConfiguration.Visible = false;
                        LabelViewEditDeviceSerialConfiguration.Visible = false;

                        ComboBoxNewTcpConfiguration.Visible = false;
                        LabelNewDeviceTcpConfiguration.Visible = false;
                        ComboBoxNewSerialConfiguration.Visible = false;
                        LabelNewDeviceSerialConfiguration.Visible = false;
                        break;
                    }
            }
        }

        private void ClearHardwareDeviceForm()
        {
            TextBoxViewEditDeviceName.Text = "";
            ComboBoxViewEditDeviceStation.SelectedIndex = -1;
            ComboBoxViewEditDeviceType.SelectedIndex = -1;
            TextBoxViewEditDeviceNumber.Text = "";
            TextBoxViewEditNumberOfCarriers.Text = "0";
            TextBoxViewEditCarrierWidth.Text = "0";
            TextBoxViewEditCarrierDepth.Text = "0";
            CheckBoxViewEditDeviceEnabled.Checked = false;
            CheckBoxViewEditSimulationMode.Checked = false;
            NumericUpDownViewEditDeviceLogLevel.Text = "";
            ComboBoxViewEditCommunicationType.SelectedValue = 3;
            ComboBoxViewEditTcpConfiguration.SelectedIndex = -1;
            ComboBoxViewEditTcpConfiguration.Visible = false;
            ComboBoxViewEditSerialConfiguration.SelectedIndex = -1;
            ComboBoxViewEditSerialConfiguration.Visible = false;

            TextBoxNewDeviceName.Text = "";
            ComboBoxNewDeviceStation.SelectedIndex = -1;
            ComboBoxNewDeviceType.SelectedIndex = -1;
            TextBoxNewDeviceNumber.Text = "";
            TextBoxNewNumberOfCarriers.Text = "0";
            TextBoxNewCarrierWidth.Text = "0";
            TextBoxNewCarrierDepth.Text = "0";
            CheckBoxNewDeviceEnabled.Checked = false;
            CheckBoxNewSimulationMode.Checked = false;
            NumericUpDownNewDeviceLogLevel.Text = "";
            ComboBoxNewCommunicationType.SelectedValue = 3;
            ComboBoxNewTcpConfiguration.SelectedIndex = -1;
            ComboBoxNewTcpConfiguration.Visible = false;
            ComboBoxNewSerialConfiguration.SelectedIndex = -1;
            ComboBoxNewSerialConfiguration.Visible = false;
        }


        private void MBHardwareDevicesNew_Click(object sender, EventArgs e)
        {
            SetupDeviceForms();
            ClearHardwareDeviceForm();
            SetCommunicationDisplay(null);
            tabControl2.SelectedTab = New;
        }

        private void MBHardwareDevicesListing_Click(object sender, EventArgs e)
        {
            tabControl2.SelectedTab = Listing;
        }

        private void MBNewDeviceBack_Click(object sender, EventArgs e)
        {
            tabControl2.SelectedTab = Listing;
        }

        private void MBNewDeviceListing_Click(object sender, EventArgs e)
        {
            tabControl2.SelectedTab = Listing;
        }

        private void MBViewEditDeviceListing_Click(object sender, EventArgs e)
        {
            tabControl2.SelectedTab = Listing;
        }

        private void MBViewEditDeviceBack_Click(object sender, EventArgs e)
        {
            var id = ((ObjectView<HardwareDeviceView>)_bindingSourceHardwareDevices.Current).Object.Id;
            LoadHardwareDevices(id);
            tabControl2.SelectedTab = Listing;
        }

        private void MBNewDeviceSave_Click(object sender, EventArgs e)
        {
            SaveNewHardwareDevice();
            var id = ((ObjectView<HardwareDeviceView>)_bindingSourceHardwareDevices.Current).Object.Id;
            LoadHardwareDevices(id);
            tabControl2.SelectedTab = Listing;
        }

        private void SaveNewHardwareDevice()
        {
            var hardwareDevice = new HardwareDevice
            {
                DeviceNumber = TextBoxNewDeviceNumber.Text.ParseInt(),
                DeviceTypeId = ((DeviceType) ComboBoxNewDeviceType.SelectedItem).Id,
                Name = TextBoxNewDeviceName.Text,
                StationId = ((Station) ComboBoxNewDeviceStation.SelectedItem).Id,
                NumberOfCarriers = TextBoxNewNumberOfCarriers.Text.ParseInt(),
                CarrierWidth = TextBoxNewCarrierWidth.Text.ParseInt(),
                CarrierDepth = TextBoxNewCarrierDepth.Text.ParseInt(),
                Enabled = CheckBoxNewDeviceEnabled.Checked,
                TcpConfigurationId = ((TcpConfiguration) ComboBoxNewTcpConfiguration.SelectedItem)?.Id,
                CommunicationTypeId =
                    ((NeutronData.Models.Lookups.CommunicationType) ComboBoxNewCommunicationType.SelectedItem)?.Id,
                SerialConfigurationId = ((SerialConfiguration) ComboBoxNewSerialConfiguration.SelectedItem)?.Id,
                SimulationMode = CheckBoxNewSimulationMode.Checked,
                LogLevel = NumericUpDownNewDeviceLogLevel.Text.ParseInt(),
            };
            _repoHardwareDevices.Insert(hardwareDevice);
        }

        private void MBViewEditDeviceSave_Click(object sender, EventArgs e)
        {
            UpdateViewEditHardwareDevice();
            var id = ((ObjectView<HardwareDeviceView>)_bindingSourceHardwareDevices.Current).Object.Id;
            LoadHardwareDevices(id);
            tabControl2.SelectedTab = Listing;
        }

        private void UpdateViewEditHardwareDevice()
        {
            var hardwareDeviceView = ((ObjectView<HardwareDeviceView>)_bindingSourceHardwareDevices.Current).Object;
            var hardwareDevice = _repoHardwareDevices.FindByKey(hardwareDeviceView.Id);
            if (hardwareDevice != null)
            {


                hardwareDevice.DeviceNumber = TextBoxViewEditDeviceNumber.Text.ParseInt();
                hardwareDevice.DeviceTypeId = ((DeviceType)ComboBoxViewEditDeviceType.SelectedItem).Id;
                hardwareDevice.Name = TextBoxViewEditDeviceName.Text;
                hardwareDevice.StationId = ((Station)ComboBoxViewEditDeviceStation.SelectedItem).Id;
                hardwareDevice.NumberOfCarriers = TextBoxViewEditNumberOfCarriers.Text.ParseInt();
                hardwareDevice.CarrierWidth = TextBoxViewEditCarrierWidth.Text.ParseInt();
                hardwareDevice.CarrierDepth = TextBoxViewEditCarrierDepth.Text.ParseInt();
                hardwareDevice.Enabled = CheckBoxViewEditDeviceEnabled.Checked;
                hardwareDevice.TcpConfigurationId = ((TcpConfiguration)ComboBoxViewEditTcpConfiguration.SelectedItem)?.Id;
                hardwareDevice.CommunicationTypeId = ((NeutronData.Models.Lookups.CommunicationType)ComboBoxViewEditCommunicationType.SelectedItem)?.Id;
                hardwareDevice.SerialConfigurationId = ((SerialConfiguration)ComboBoxViewEditSerialConfiguration.SelectedItem)?.Id;
                hardwareDevice.SimulationMode = CheckBoxViewEditSimulationMode.Checked;
                hardwareDevice.LogLevel = NumericUpDownViewEditDeviceLogLevel.Text.ParseInt();
            }
            _repoHardwareDevices.Update(hardwareDevice);
        }

        private void MBViewEditDeviceDelete_Click(object sender, EventArgs e)
        {

        }

        // Set the focus to the passed in recId if it's passed in
        private int LoadHardwareDevices(int recId = 0)
        {
            var idx = -1;
            Cursor.Current = Cursors.WaitCursor;
            var recs = _repoHardwareDevices.AllInclude(r => r.Station
                , r => r.DeviceType
                , r => r.CommunicationType
                , r => r.TcpConfiguration
                , r => r.SerialConfiguration
                ).ToList();
            //create the view
            if (recs.Count > 0)
            {
                var hardwareDeviceViews = recs.Select(r => new HardwareDeviceView
                {
                    Id = r.Id
                    , DeviceNumber = r.DeviceNumber
                    , Name = r.Name
                    , StationId = r.StationId
                    , StationName = r.Station.Name
                    , NumberOfCarriers = int.Parse(r.NumberOfCarriers.ToString())
                    , CarrierWidth = r.CarrierWidth
                    , CarrierDepth = r.CarrierDepth
                    , DeviceTypeId = r.DeviceTypeId
                    , DeviceTypeName = r.DeviceType.Name ?? string.Empty
                    , Enabled = r.Enabled
                    , SimulationMode = r.SimulationMode
                    , LogLevel = r.LogLevel
                    , SerialConfigurationId = r.SerialConfigurationId
                    , SerialConfigurationName = r.SerialConfiguration == null ? string.Empty : r.SerialConfiguration.Name
                    , CommunicationTypeId = r.CommunicationTypeId ?? null
                    , CommunicationTypeName = r.CommunicationType == null ? string.Empty : r.CommunicationType.Name
                    , TcpConfigurationId = r.TcpConfigurationId ?? null
                    , TcpConfigurationName = r.TcpConfiguration == null ? string.Empty : r.TcpConfiguration.Name
                }).ToList();

                var blv = new BindingListView<HardwareDeviceView>(hardwareDeviceViews.ToList());
                _bindingSourceHardwareDevices.DataSource = blv;
                DataGridView1.DataSource = _bindingSourceHardwareDevices;

                if (GetRecordCount(_bindingSourceHardwareDevices) > 0)
                {
                    if (recId != 0)
                    {
                        idx = IndexOf(_bindingSourceHardwareDevices, recId);
                        if (idx >= 0)
                        {
                            DataGridView1.FirstDisplayedScrollingRowIndex = DataGridView1.Rows[idx].Index;
                        }
                    }
                    else
                    {
                        DataGridView1.ClearSelection();
                    }
                    DataGridView1.Refresh();
                    CurrentItem = ((ObjectView<HardwareDeviceView>)_bindingSourceHardwareDevices.Current).Object;
                }
            }
            Cursor.Current = Cursors.Default;
            return idx;
        }

        private void ComboBoxViewEditCommunicationType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var comboBox = (ComboBox)sender;
            if (comboBox.SelectedIndex == -1)
                SetCommunicationDisplay(null);
            else
            {
                var communicationType = ((NeutronData.Models.Lookups.CommunicationType)comboBox.SelectedItem).Id;
                SetCommunicationDisplay(communicationType);
            }
        }

        private void ComboBoxNewCommunicationType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var comboBox = (ComboBox)sender;
            if (comboBox.SelectedIndex == -1)
                SetCommunicationDisplay(null);
            else
            {
                var communicationType = ((NeutronData.Models.Lookups.CommunicationType)comboBox.SelectedItem).Id;
                SetCommunicationDisplay(communicationType);
            }
        }
    }
}