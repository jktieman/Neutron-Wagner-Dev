using AlliedLogger;
using Hanel_DC.Extensions;
using HanelCommands;
using HanelCommands.Builders;
using JsonManager;
using Neutron.Controllers;
using Neutron.Global;
using Neutron.Interfaces;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronData.ModelViews;
using NeutronEvents;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Markup;
using ByteExtensions = NeutronCore.Extensions.ByteExtensions;

namespace Neutron.Forms
{
    public partial class FrmMp12DTest : Form
    {
        public static char CR = Convert.ToChar(13);
        public static char LF = Convert.ToChar(10);
        public static char AST = Convert.ToChar(42);

        private IHanelDriver _mp12D;
        private readonly IJsonData _jsonData;
        private readonly NeutronVariables _neutronVariables;
        private readonly WorkstationView _workstationView;
        private readonly IDynamicLogger _logger;
        private readonly bool _testing = true;
        private HanelCommandProcessor _hanelCommandProcessor = new HanelCommandProcessor();
        private readonly IDialogService _dialogService;

        public FrmMp12DTest(IJsonData jsonData, NeutronVariables neutronVariables, WorkstationView workstationView, IDialogService dialogService)
        {
            InitializeComponent();
            Mediator.GetInstance().DisplayMessage += OnDisplayMessageReceived;
            _jsonData = jsonData;
            _neutronVariables = neutronVariables;
            _workstationView = workstationView;
            _dialogService = dialogService;
            _logger = NeutronCore.Global.Logger.SetupLogger("HanelLog");
        }

        private void InitializeMp12DController()
        {
            bool controllerClosed = false;
            try
            {
                if (GlobalVar.Hanel != null)
                {
                    controllerClosed = CloseControllerIfOpen(GlobalVar.Hanel);
                }

                if (controllerClosed || GlobalVar.Hanel == null)
                {
                    if (_workstationView.Hanels.Count > 0)
                    {
                        _mp12D = new Mp12D(this, _workstationView, _logger, _neutronVariables.LogLevel, _dialogService);
                        if (_mp12D == null)
                        {
                            AddItemToListBox("Controller initialization failed");
                            return;
                        }
                        AddItemToListBox("Controller Initialized");
                    }
                    else
                    {
                        AddItemToListBox("Station not set up with Hanel devices.");
                    }
                }

            }
            catch (Exception ex)
            {
                AddItemToListBox($"Error initializing controller: {ex.Message}");
            }

        }

        private bool CloseControllerIfOpen(IHanelDriver hanelDriver)
        {
            bool result = false;

            if (hanelDriver != null)
            {
                result = hanelDriver.CloseController();
            }

            return result;
        }
        private void OnDisplayMessageReceived(object sender, DisplayMessageEventArgs e)
        {
            // Assuming ListBoxInformation is a ListBox control
            AddItemToListBox(e.Message);
        }

        private void ButtonInitController_Click(object sender, EventArgs e)
        {
            InitializeMp12DController();
        }

        private void ButtonGetStatus_Click(object sender, EventArgs e)
        {
            try
            {
                int deviceNumber = int.Parse(TextBoxDeviceNumber.Text);
                var status = _mp12D.GetDeviceStatus(deviceNumber);
                AddItemToListBox($"Device Status: {status.GoodStatus}");
            }
            catch (Exception ex)
            {
                AddItemToListBox($"Error getting device status: {ex.Message}");
            }

        }

        private void ButtonPositionDevice_Click(object sender, EventArgs e)
        {
            try
            {
                var deviceNumber = int.Parse(TextBoxDeviceNumber.Text);
                var trayNumber = int.Parse(TextBoxTrayNumber.Text);
                var over = int.Parse(TextBoxOver.Text);
                var back = int.Parse(TextBoxBack.Text);
                var quantity = int.Parse(TextBoxQuantity.Text);
                var display = TextBoxDisplay.Text;


                var response = _mp12D.PositionDevice(deviceNumber, trayNumber, over, back, quantity, display);
                AddItemToListBox($"Position Device Response: {response}");
            }
            catch (Exception ex)
            {
                AddItemToListBox($"Error positioning device: {ex.Message}");
            }

        }

        private void ButtonResetDevice_Click(object sender, EventArgs e)
        {
            try
            {
                _mp12D.ResetHanelDeviceStatus();
                AddItemToListBox("Device Status Reset");
            }
            catch (Exception ex)
            {
                AddItemToListBox($"Error resetting device status: {ex.Message}");
            }

        }

        private void AddItemToListBox(string item)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(AddItemToListBox), item);
            }
            else
            {
                ListBoxInformation.Items.Add(item);
                ListBoxInformation.TopIndex = ListBoxInformation.Items.Count - 1; // Scroll to the last item
            }
        }

        private void ButtonValidCommand_Click(object sender, EventArgs e)
        {
            byte[] _dataI = { 42, 71, 48, 49, 49, 36, 80, 32, 88, 83, 36, 69, 48, 48, 36, 13, 10 };
            ListBoxInformation.Items.Add(ByteExtensions.ByteArrayToString(_dataI));
            ListBoxInformation.Items.Add(ByteExtensions.ByteArrayToStringX2(_dataI));

            ListBoxInformation.Refresh();
            _dataI = RemoveBytesBeforeAsterisk(_dataI);
            ListBoxInformation.Items.Add(_dataI.ByteArrayToHexString());
            ListBoxInformation.Refresh();


            var validCommand = ValidCommand(_dataI);
            ListBoxInformation.Items.Add(validCommand.ByteArrayToHexString());
            ListBoxInformation.Items.Add(validCommand.ByteArrayToRawString());
            ListBoxInformation.Refresh();
            _dataI = _dataI.Skip(validCommand.Length).ToArray();
            ListBoxInformation.Items.Add(_dataI.ByteArrayToHexString());
            ListBoxInformation.Items.Add(_dataI.ByteArrayToHexString(false));
            ListBoxInformation.Refresh();
        }
        public byte[] ValidCommand(byte[] dataIn)
        {
            byte[] byteArray = null;
            if (dataIn.Length == 0)
            {
                return null;
            }

            _logger.LogDetailAsync($"dataIn: {dataIn.ByteArrayToHexString()}");
            // extract the byte array starting with 42 and ending with 10
            var startIndex = FindAsterisk(dataIn);
            _logger.LogDetailAsync($"Start Index: {startIndex}");
            if (startIndex == -1)
            {
                _logger.LogDetailAsync($"Start Index = -1 {startIndex}");
                return null;
            }

            //if (startIndex >= 0)
            //{
            //    while (dataIn.First() != AST)
            //    {

            //        dataIn = dataIn.Skip(1).ToArray();
            //        _logger.LogDetailAsync($"Building dataIn: {dataIn.ByteArrayToHexString()} ");
            //    }
            //}
            _logger.LogDetailAsync($"Final dataIn: {dataIn.ByteArrayToHexString()} ");

            _logger.LogDetailAsync($"Final dataIn Start Index: {startIndex} ");
            var endIndex = FindLineFeed(dataIn); // Array.IndexOf(dataIn, LF);
            _logger.LogDetailAsync($"Final dataIn End Index: {endIndex} ");
            if (endIndex == -1)
            {
                return null;
            }

            byteArray = dataIn.Skip(startIndex).Take(endIndex - startIndex + 1).ToArray();
            _logger.LogDetailAsync($"Return ByteArray: {byteArray.ByteArrayToHexString()}");
            return byteArray;

        }

        private byte[] RemoveBytesBeforeAsterisk(byte[] byteArray)
        {
            byte asterisk = (byte)'*'; // ASCII value of '*'

            int asteriskIndex = Array.IndexOf(byteArray, asterisk);
            if (asteriskIndex == -1)
            {
                // Asterisk not found, return the original array
                return byteArray;
            }

            // Create a new array starting from the asterisk index
            byte[] resultArray = byteArray.Skip(asteriskIndex).ToArray();
            ListBoxInformation.Items.Add(resultArray.ByteArrayToHexString());
            ListBoxInformation.Refresh();
            return resultArray;
        }

        private int FindLineFeed(byte[] dataIn)
        {
            byte lineFeed = (byte)'\n'; // ASCII value of '\n'

            for (int i = 0; i < dataIn.Length; i++)
            {
                if (dataIn[i] == lineFeed)
                {
                    return i; // Return the index of the line feed
                }
            }

            return -1; // Return -1 if asterisk is not found
        }

        static int FindAsterisk(byte[] byteArray)
        {
            byte asterisk = (byte)'*'; // ASCII value of '*'

            for (int i = 0; i < byteArray.Length; i++)
            {
                if (byteArray[i] == asterisk)
                {
                    return i; // Return the index of the asterisk
                }
            }

            return -1; // Return -1 if asterisk is not found
        }

        private void ButtonClear_Click(object sender, EventArgs e)
        {
            ListBoxInformation.Items.Clear();
        }

        private void ButtonProcessCommand_Click(object sender, EventArgs e)
        {
            var command = TextBoxCommand.Text;
            var dataIn = Encoding.UTF8.GetBytes(command);

            var deviceStatusList = GetHanelDeviceStatusList();

            var hanelCommandProcessor = new HanelCommandProcessor();

            hanelCommandProcessor.Process(dataIn, ref deviceStatusList);

        }

        private List<HanelDeviceStatus> GetHanelDeviceStatusList()
        {
            var deviceStatusList = new List<HanelDeviceStatus>();
            deviceStatusList.Clear();
            for (var index = 1; index <= 3; ++index)
            {
                deviceStatusList.Add(new HanelDeviceStatus(index));
                deviceStatusList[index - 1].Device = index; // Adjusted index
                deviceStatusList[index - 1].DeviceNumber = index; // Adjusted index
                deviceStatusList[index - 1].GoodStatus = true;
                deviceStatusList[index - 1].LastStatus = DateTime.Now;
                deviceStatusList[index - 1].LastCommand = DateTime.Now;
                deviceStatusList[index - 1].TargetTray = 0;
                deviceStatusList[index - 1].CurrentTray = 0;
                deviceStatusList[index - 1].InMotion = false;
                deviceStatusList[index - 1].InAlignment = true;
                deviceStatusList[index - 1].StatusMessage = "";
                deviceStatusList[index - 1].CommandSent = false;
                deviceStatusList[index - 1].CommandAccepted = false;
                deviceStatusList[index - 1].CommandExecuted = false;

                //    if (machineSimulationMode == false)
                //    {
                //        deviceStatusList[index].GoodStatus = true;
                //    }

                //    deviceStatusList[index].CommandAccepted = false;
                //deviceStatusList[index].CommandExecuted = false;
            }


            return deviceStatusList;
        }

        private void ButtonGetCurrentTrays_Click(object sender, EventArgs e)
        {
            
            _mp12D.GetTraysInWindow();
        }


        //private void ButtonGetStatus_Click(object sender, EventArgs e)
        //{
        //    int deviceNumber = int.Parse(TextBoxDeviceNumber.Text);
        //    var status = _mp12D.GetDeviceStatus(deviceNumber);
        //    MessageBox.Show($"Device Status: {status.GoodStatus}");
        //}

        //private void ButtonPositionDevice_Click(object sender, EventArgs e)
        //{
        //    int deviceNumber = int.Parse(TextBoxDeviceNumber.Text);
        //    int trayNumber = int.Parse(TextBoxTrayNumber.Text);
        //    var response = _mp12D.PositionDevice(deviceNumber, trayNumber);
        //    MessageBox.Show($"Position Device Response: {response}");
        //}

        //private void ButtonResetDevice_Click(object sender, EventArgs e)
        //{
        //    _mp12D.ResetHanelDeviceStatus();
        //    MessageBox.Show("Device Status Reset");
        //}
    }
}
