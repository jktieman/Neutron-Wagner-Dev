using AlliedLogger;
using Hart_DisplayControllers;
using JsonManager;
using NeutronCore;
using NeutronCore.Extensions;
using NeutronCore.Global;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.ModelViews;
using NeutronData.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NeutronCore.Models;
using NeutronData.Models.Lookups;

namespace Neutron.Controllers
{
    public class DisplayController : IDisplayController
    {
        private readonly GenericRepository<SerialConfiguration> _repoSerial = new GenericRepository<SerialConfiguration>(new NeutronDb());
        private readonly GenericRepository<HardwareDevice> _repoHardwareDevice = new GenericRepository<HardwareDevice>(new NeutronDb());

        private Hart_DisplayController _hartDisplayController;

        private string _cError = string.Empty;
        private DynamicLogger _logger;
        private string _logFileDir = string.Empty;
        private readonly WorkstationView _workstationView;
        private readonly List<Hart_BLI> _bliList = new List<Hart_BLI>();
        private readonly List<Hart_SHI> _shiList = new List<Hart_SHI>();
        private readonly List<Hart_SHI> _shiListClear = new List<Hart_SHI>();
        private int _lBeacon = 0;
        private int _rBeacon = 0;
        private readonly bool _bliEnabled = true;
        private readonly bool _shiEnabled = true;
        private readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;
        private readonly Hart_SHI Global_Module;
        List<Hart_BLI> blisOn = new List<Hart_BLI>();
        List<Hart_SHI> shisOn = new List<Hart_SHI>();
        public bool Ready { get; set; }
        public event EventHandler<IptiController.MySerialDataReceivedEventArgs> MySerialDataReceived;

        public DisplayController(IJsonData jsonData, WorkstationView workstationView)
        {
            _workstationView = workstationView;
            _neutronVariables = jsonData.LoadFile<NeutronVariables>();
            _neutronLicense = jsonData.LoadFile<NeutronLicense>();
            _bliEnabled = _neutronVariables.BliEnabled;
            _shiEnabled = _neutronVariables.ShiEnabled;
            Global_Module = new Hart_SHI(0, 0, 0, "", "");
            CreateLog();

            FillBliList();
            FileShiList();
            FileShiListClear();

            var hartLog = ($"{_logFileDir}Hart");


            _hartDisplayController = new Hart_DisplayController(Hart_DisplayController.Controller_Type_Remstar_BPI_SHI(), hartLog);

            Task.Run(() => _logger.LogDetailAsync(msg: @"HartDisplayController has been created: "));
            if (HartDisplayControllerInit())
            {
                Ready = true;
            }
            else
            {
                Task.Run(() => _logger.LogDetailAsync(@"HartDisplayController failed Initialization"));
                Ready = false;
            }
        }

        private bool HartDisplayControllerInit()
        {
            var result = false;

            var serialConfigurationId = _repoHardwareDevice.All().FirstOrDefault(r => r.DeviceTypeId == (int)NeutronCore.Enums.DeviceType.RemstarDisplays && r.WorkstationId == _workstationView.WorkstationId)?.SerialConfigurationId;

            if (serialConfigurationId == null) return false;

            var serialConfiguration = _repoSerial.FindByKey(serialConfigurationId);
            if (serialConfiguration == null) return false;

            Task.Run(() => _logger.LogDetailAsync($"Serial Address: {serialConfiguration.PortName} Baud Rate: {serialConfiguration.BaudRate.ToString()}"));
            Task.Run(() => _logger.LogDetailAsync($"Serial Port Number: {serialConfiguration.PortNumber.ToString()}"));


            if (_hartDisplayController.Init_Controller(serialConfiguration.PortNumber, serialConfiguration.SimulationMode, serialConfiguration.LogLevel, ref _cError))
            {
                Task.Run(() => _logger.LogDetailAsync("Initialization Requested"));
                result = true;
            }
            else
            {
                Task.Run(() => _logger.LogDetailAsync("Problem requesting initialization. " + _cError));
                MessageBox.Show($"{_cError}", "Display Controller Initialization", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }


            return result;
        }

        public int GetInitStatus()
        {
            var stat = InitStatus();
            Task.Run(() => _logger.LogDetailAsync($"Get Init Status - Return: {stat}"));
            return stat;
        }

        private int InitStatus()
        {
            // Note that the sequesnce of the following assignments is critical. Success must be first. Others follow in any sequence.
            var success = _hartDisplayController.Init_Success;
            var initCode = _hartDisplayController.LastStatus_Code;
            var initMsg = _hartDisplayController.LastStatus_Message;

            Task.Run(() => _logger.LogDetailAsync($"HartDisplayController InitStatus: Success: {success} initCode: {initCode} initMsg: {initMsg}"));
            if (success)
            {
                // life is good, you can drive the device
                Task.Run(() => _logger.LogDetailAsync($"HartDisplayController InitStatus: success is {success}"));
                if (initCode == 0)
                {
                    // life is good, no warning messages
                    Task.Run(() => _logger.LogDetailAsync($"HartDisplayController Initialization is complete and was successful initCode is {initCode}"));
                }
                else
                {
                    // You need to report the warning to the operator or to a log that is monitored frequently
                    Task.Run(() => _logger.LogDetailAsync($"HartDisplayController Warning - Initialization was successful but there is a warning." + Environment.NewLine +
                    "Please provide the following information to your IT support." + Environment.NewLine +
                    "Code is: " + initCode.ToString() + Environment.NewLine +
                    "Message is: " + initMsg));
                }
            }
            else
            {
                // Darn, cannot drive the device at this time
                if (_hartDisplayController.Init_PercentageComplete == 0)
                {
                    Task.Run(() => _logger.LogDetailAsync($"Not Initialized.  Code is: {initCode.ToString()}  Message is: {initMsg}"));
                }
                else if (_hartDisplayController.Init_PercentageComplete < 100)
                {
                    Task.Run(() => _logger.LogDetailAsync($"Initialization is in progress.  Init {_hartDisplayController.Init_PercentageComplete.ToString()}% complete..."));
                    Task.Run(() => _logger.LogDetailAsync($"Code is: {initCode.ToString()}  Message is: {initMsg}"));
                }
                else
                {
                    Task.Run(() => _logger.LogDetailAsync($"Initialization was unsuccessful.  Code is: {initCode.ToString()}  Message is: {initMsg}"));
                }
            }
            return initCode;
        }

        private void CreateLog()
        {
            _logFileDir = LoaderSettings.GetLogFileDirectory();
            var folderName = string.Format(format: @"Display Controller_{0}", arg0: _workstationView.WorkstationId.ToString());
            var logActivity = LoaderSettings.EnableLogging;
            Task.Run(() => _logger = new DynamicLogger(_logFileDir, folderName, logActivity));
        }

        public void CloseController()
        {
            try
            {
                if (_hartDisplayController != null)
                {
                    _hartDisplayController.Close_Controller(ref _cError);
                    Task.Run(() => _logger.LogDetailAsync($"Close Display Controller Closed {_cError}"));
                }
            }
            catch (Exception ex)
            {
                Task.Run(() => _logger.LogDetailAsync($"Close Display Controller Exception: {_cError} {Environment.NewLine}{ex.Message} {Environment.NewLine} {ex.InnerException} "));
            }
        }

        private int GetAddress(int device, int level)
        {
            var address = string.Empty;

            switch (device)
            {
                case 1:
                    _lBeacon = 2;
                    _rBeacon = 0;
                    address = $"10{level}";
                    break;
                case 2:
                    _lBeacon = 0;
                    _rBeacon = 2;
                    address = $"10{level}";
                    break;
                case 3:
                    _lBeacon = 0;
                    _rBeacon = 2;
                    address = $"20{level}";
                    break;
                case 4:
                    _lBeacon = 2;
                    _rBeacon = 0;
                    address = $"20{level}";
                    break;
            }


            Task.Run(() => _logger.LogDetailAsync($"Get Address Returned: {address}"));

            return int.Parse(address);

        }

        public void ClearAllBli()
        {
            Task.Run(() => _logger.LogDetailAsync($"BLI Clear All Displays."));

            if (!_hartDisplayController.Clear(_bliList, ref _cError))
            {
                Task.Run(() => _logger.LogDetailAsync($"BLI Clear All Display Error. \r\n  {_cError}"));
            }

        }

        //this is the one Neutron uses
        public void ClearAllShi()
        {
            if (!_hartDisplayController.Clear(Global_Module, ref _cError))
            {
                Task.Run(() => _logger.LogDetailAsync($"SHI Clear All Display Error.  {Environment.NewLine}{_cError}"));
            }
        }

        //this is the one Neutron uses
        public void ShowShi(int device, int bin, int level, string part, string text)
        {
            Task.Run(() => _logger.LogDetailAsync($"ShowShi -- Device: {device}  Bin: {bin}  Level: {level}  Part: {part}  Text: {text}"));
            var address = GetAddress(device, level);

            var shi = new Hart_SHI(address, _lBeacon, _rBeacon, part, text);

            if (!_hartDisplayController.Show(shi, ref _cError))
            {
                Task.Run(() => _logger.LogDetailAsync($"SHI Show Single Display Error.  {shi.SHI_Address}\r\n {_cError}"));
            }
        }

        public void ShowAllShi()
        {
            Task.Run(() => _logger.LogDetailAsync("SHI Show All Displays."));

            if (!_hartDisplayController.Show(_shiList, ref _cError))
            {
                Task.Run(() => _logger.LogDetailAsync("SHI Show All Display Error."));
            }
        }

        public void ShowBli(int address, int beacon, string text)
        {
            if (_bliEnabled)
            {
                Task.Run(() => _logger.LogDetailAsync($"BLI Address: {address}"));
                var bli = new Hart_BLI(address, 2, text);
                if (!blisOn.Contains(bli))
                {
                    blisOn.Add(bli);
                }
                Task.Run(() => _logger.LogDetailAsync($"BLI On: {bli.BLI_Address}"));
                Thread.Sleep(10);
                if (!_hartDisplayController.Show(bli, ref _cError))
                {
                    Task.Run(() => _logger.LogDetailAsync($"BLI Show Single Display Error.  { bli.BLI_Address}\r\n {_cError}"));
                }
            }
        }

        public void ShowBli(Hart_BLI bli)
        {
            if (_bliEnabled)
            {
                if (!blisOn.Contains(bli))
                {
                    blisOn.Add(bli);
                }
                Task.Run(() => _logger.LogDetailAsync($"Hart BLI Address: {bli.BLI_Address}"));
                Thread.Sleep(10);
                if (!_hartDisplayController.Show(bli, ref _cError))
                {
                    Task.Run(() => _logger.LogDetailAsync($"Hart BLI Show Single Display Error.  { bli.BLI_Address}\r\n {_cError}"));
                }
            }
        }

        public void ShowShi(Hart_SHI shi)
        {
            Task.Run(() => _logger.LogDetailAsync($"Hart SHI Show: {shi.SHI_Address}"));
            Thread.Sleep(10);
            if (!_hartDisplayController.Show(shi, ref _cError))
            {
                Task.Run(() => _logger.LogDetailAsync($"Hart SHI Show Single Display Error 2.  {shi.SHI_Address}\r\n {_cError}"));
            }
        }

        public void ClearBli(Hart_BLI bli)
        {
            Task.Run(() => _logger.LogDetailAsync($"BLI Clear Single Display. {bli.BLI_Address}"));
            if (!_hartDisplayController.Clear(bli, ref _cError))
            {
                Task.Run(() => _logger.LogDetailAsync($"BLI Clear Single Display Error.  {bli.BLI_Address}\r\n {_cError}"));
            }
        }

        public void ClearShi(Hart_SHI shi)
        {
            Task.Run(() => _logger.LogDetailAsync($"SHI Clear Single Display.  {shi.SHI_Address}"));
            if (!_hartDisplayController.Clear(shi, ref _cError))
            {
                Task.Run(() => _logger.LogDetailAsync($"SHI Clear Single Display Error.   {shi.SHI_Address} \r\n { _cError}"));
            }
        }

        public void ShowOc(int address, int beacon, string text)
        {
            //throw new NotImplementedException();
        }

        public void ClearOc(int address)
        {
            //throw new NotImplementedException();
        }

        private void FileShiList()
        {
            var lBeacon = 2;
            var rBeacon = 2;
            for (var i = 1; i <= 7; i++)
            {
                var address = ($"10{i}").ParseInt();
                var shi = new Hart_SHI(address, lBeacon, rBeacon, DisplayArea_2: @"--", DisplayArea_6: @"------");
                _shiList.Add(shi);
            }
            for (var i = 1; i <= 7; i++)
            {
                var address = ($"20{i}").ParseInt();
                var shi = new Hart_SHI(address, lBeacon, rBeacon, DisplayArea_2: @"--", DisplayArea_6: @"------");
                _shiList.Add(shi);
            }
            var sb = new StringBuilder();
            foreach (var item in _shiList)
            {
                sb.AppendLine($"SHI - {item.SHI_Address}");
            }
            Task.Run(() => _logger.LogDetailAsync($"SHI Listing\n\r {sb.ToString()}"));
        }

        private void FileShiListClear()
        {
            var lBeacon = 0;
            var rBeacon = 0;
            for (var i = 1; i <= 7; i++)
            {
                var address = ($"10{i}").ParseInt();
                var shi = new Hart_SHI(address, lBeacon, rBeacon, DisplayArea_2: @"--", DisplayArea_6: @"------");
                _shiListClear.Add(shi);
            }
            for (var i = 1; i <= 7; i++)
            {
                var address = ($"20{i}").ParseInt();
                var shi = new Hart_SHI(address, lBeacon, rBeacon, DisplayArea_2: @"--", DisplayArea_6: @"------");
                _shiListClear.Add(shi);
            }
            var sb = new StringBuilder();
            foreach (var item in _shiListClear)
            {
                sb.AppendLine($"SHI - {item.SHI_Address}");
            }
            Task.Run(() => _logger.LogDetailAsync($"SHI Listing\n\r {sb.ToString()}"));
        }

        private void FillBliList()
        {
            for (var i = 1; i <= 16; i++)
            {
                var bli = new Hart_BLI(i, 2, @"------");
                _bliList.Add(bli);
            }
            var sb = new StringBuilder();
            foreach (var item in _bliList)
            {
                sb.AppendLine($"BLI - {item.BLI_Address}");
            }
            Task.Run(() => _logger.LogDetailAsync($"BLI Listing\n\r {sb.ToString()}"));
        }

        //private void FillBliListStations4_5()
        //{
        //    for (int i = 1; i <= 4; i++)
        //    {
        //        var bli = new Hart_BLI(i, 2, @"------");
        //        bliList.Add(bli);
        //    }
        //    var sb = new StringBuilder();
        //    foreach (var item in bliList)
        //    {
        //        sb.AppendLine($"BLI - {item.BLI_Address}");
        //    }
        //    Task.Run(() => _logger.LogDetailAsync($"BLI Listing\n\r {sb.ToString()}"));
        //}


        //public void ShowListBli(List<Hart_BLI> blis)
        //{
        //    if (bliEnabled)
        //    {
        //        Task.Run(() => logger.Log("BLI Show List Displays."));
        //        if (!HartDisplayController.Show(blis, ref cError))
        //        {
        //            Task.Run(() => logger.Log("BLI Show List Display Error."));
        //        }
        //    }
        //}

        //public void ShowListShi(List<Hart_SHI> shis)
        //{
        //    if (shiEnabled)
        //    {
        //        Task.Run(() => logger.Log("SHI Show List Displays."));
        //        if (!HartDisplayController.Show(shis, ref cError))
        //        {
        //            Task.Run(() => logger.Log("SHI Show List Display Error."));
        //        }
        //    }
        //}

        //public void ClearListBli(List<Hart_BLI> blis)
        //{
        //    if (bliEnabled)
        //    {
        //        Task.Run(() => logger.Log("BLI Clear List Displays."));
        //        if (!HartDisplayController.Clear(blis, ref cError))
        //        {
        //            Task.Run(() => logger.Log("BLI Clear List Display Error."));
        //        }
        //    }
        //}

        //public void ClearListShi(List<Hart_SHI> shis)
        //{
        //    if (shiEnabled)
        //    {
        //        Task.Run(() => logger.Log("SHI Clear List Displays."));
        //        if (!HartDisplayController.Clear(shis, ref cError))
        //        {
        //            Task.Run(() => logger.Log("SHI Clear List Display Error."));
        //        }
        //    }
        //}

        public void ShowBli(Ipti_BLI bli)
        {
            throw new NotImplementedException();
        }

        public void ClearBli(Ipti_BLI bli)
        {
            throw new NotImplementedException();
        }
    }
}
