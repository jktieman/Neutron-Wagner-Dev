using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlliedLogger;
using EthernetTransmitter;
using Hart_DisplayControllers;
using JsonManager;
using NeutronCore.Extensions;
using Neutron.Models;
using NeutronCore;
using NeutronCore.Enums;
using NeutronCore.Global;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.ModelViews;
using NeutronData.Repositories;
using NeutronEvents;


namespace Neutron.Controllers
{
    // ReSharper disable once InconsistentNaming
    public class TCP_IptiController : IDisplayController
    {
        private const string Bayid = "01";
        private const string DisplayOc = "27";
        private const string TurnAllOff = "03";
        private const string TurnAllOn = "02";

        public CancellationTokenSource Token = new CancellationTokenSource();
        private static readonly BlockingCollection<byte[]>
            ResponseBlockingCollection = new BlockingCollection<byte[]>();

        private static readonly BlockingCollection<byte[]>
            RequestBlockingCollection = new BlockingCollection<byte[]>();

        private static readonly BlockingCollection<byte[]>
            ReceivedBlockingCollection = new BlockingCollection<byte[]>();

        private readonly List<int> _blastzoneControllers = new List<int> { 1, 2, 3 };
        private string _bliController = "04";

        private readonly List<Ipti_BLI> _bliList = new List<Ipti_BLI>();
        private readonly NeutronVariables _neutronVariables;
        private readonly HardwareDevice _hardwareDevice;

        private readonly GenericRepository<TcpConfiguration> _repoTcp = new GenericRepository<TcpConfiguration>(new NeutronDb());
        private readonly GenericRepository<HardwareDevice> _repoHardwareDevice = new GenericRepository<HardwareDevice>(new NeutronDb());

        private readonly ResponseManager _responseManager;

        private readonly bool _bliEnabled;
        private readonly bool _shiEnabled;

        private readonly WorkstationView _workstation;

        private IDynamicLogger _logger;

        private TcpTransmitter _transmitter;

        public bool Transmit { get; set; }
        private string _cError = string.Empty;
        private int _lBeacon;
        private int _rBeacon;
        private Dictionary<int, TowerLevelInfo> _towerLevelInfoList;

        public event EventHandler<MyDataReceivedEventArgs> MyDataReceived;
        public bool Ready { get; set; }

        public TCP_IptiController(IJsonData jsonData, WorkstationView workstation, NeutronVariables neutronVariables,
            HardwareDevice hardwareDevice)
        {
            _workstation = workstation;
            _neutronVariables = neutronVariables;
            _hardwareDevice = hardwareDevice;

            _bliEnabled = _hardwareDevice.Enabled;    
            _shiEnabled = _neutronVariables.ShiEnabled;
            _bliController = _neutronVariables.BliController.ToString().PadLeft(2, '0');
            CreateLog();

            if (_bliEnabled) FillBliList();

            if (_shiEnabled) GetTowerLevelInfoList();

            _responseManager = new ResponseManager(ResponseBlockingCollection, RequestBlockingCollection,
                ReceivedBlockingCollection)
            { Transmit = false };

            IptiControllerInit();
        }

        public void SetTransmit(bool value)
        {
            Transmit = value;
            _responseManager.Transmit = value;
        }
        public void CloseController()
        {
            try
            {
                Token.Cancel();
                SetTransmit(false);
                ReceivedBlockingCollection.CompleteAdding();
                ResponseBlockingCollection.CompleteAdding();
                RequestBlockingCollection.CompleteAdding();
                _transmitter.CloseConnection();
            }
            catch (Exception ex)
            {
                Task.Run(() => _logger.LogDetailAsync($"Close TCP Port Exception: {ex.Message} \r\n {ex.InnerException} "));
            }
        }

        public async Task TurnOnAllBli()
        {
            await _logger.LogDetailAsync($"IPTI Controller - Turn On All BLI - START");

            await _transmitter.SendData($"{_bliController}{TurnAllOn}");

           await _logger.LogDetailAsync($"IPTI Controller - Turn On All BLI - END");
        }
        public async Task ClearAllBli()
        {
            await _logger.LogDetailAsync($"IPTI Controller - Clear All BLI - START");
            if (!_bliEnabled)
            {
                await _logger.LogDetailAsync($"IPTI Controller - Clear All BLI - BLI NOT Enabled");
                return;
            }
            
            await _transmitter.SendData($"{_bliController}{TurnAllOff}");


            //foreach (var bli in _bliList)
            //{
            //    _ = _logger.LogDetailAsync($"IPTI Controller - Clear All BLI - SendData Turn Off: BayController: {bli.BLI_BayController} Address: {bli.BLI_Address}");
            //    _transmitter.SendData(bli.TurnOff);
            //    _ = _logger.LogDetailAsync($"IPTI Controller - Clear All BLI - SendData Turn Off Return");
            //}
            await _logger.LogDetailAsync($"IPTI Controller - Clear All BLI - END");
        }
        public void ClearAllShi()
        {
            if (!_shiEnabled) return;
            for (var i = 2; i < 6; i++)
            {
                for (var j = 1; j < 5; j++)
                {
                    var cmd = i.ToString().PadLeft(2, '0') + "39" + j.ToString().PadLeft(2, '0');
                    _transmitter.SendData(cmd);
                }
            }
        }
        public void ShowShi(Ipti_SHI shi)
        {
            _transmitter.SendData(shi.TurnOn());
            Task.Run(() => _logger.LogDetailAsync($"ShowShi  Turning ON BayId: {shi.BayId}  Display: {shi.DisplayId}"));
        }
        public void ClearShi(Ipti_SHI shi)
        {
            if (_shiEnabled)
            {
                _transmitter.SendData(shi.TurnOff());
                Task.Run(() => _logger.LogDetailAsync($"ClearShi Turning OFF BayId: {shi.BayId}  Display: {shi.DisplayId}"));
            }
        }

        public void ShowBlastzone(int bayControllerId, int address, int beacon, string text)
        {
            if (!_bliEnabled) return;
            var bli = new Ipti_BLI(bayControllerId, address, beacon, text);
            ShowBli(bli);
        }
        public void ClearBlastzone()
        {
            if (!_bliEnabled) return;
            foreach (var bayController in _blastzoneControllers)
            {
                //for (var i = 1; i <= 2; i++)
                //{
                //    var bli = new Ipti_BLI(bayController, i, 0, i.ToString());
                //    _transmitter.SendData(bli.TurnOff);
                //}

                var bli = new Ipti_BLI(bayController, 1, 0, 1.ToString());
                _transmitter.SendData(bli.TurnAllOff);
            }
        }

        public void ShowBlastzoneOc(int bayControllerId, int address, int beacon, string text)
        {
            var bayId = bayControllerId.ToString().PadLeft(2, '0');
            if (!_bliEnabled) return;
            Task.Run(() => _logger.LogDetailAsync($"Ipti OC Show:  BayController: {bayControllerId} Address: {address}"));
            var cmd = bayId + DisplayOc + "0100" + text;
            _transmitter.SendData(cmd);
        }
        public void ClearBlastzoneOc(int bayControllerId, int address, int beacon, string text)
        {
            var bayId = bayControllerId.ToString().PadLeft(2, '0');
            if (!_bliEnabled) return;
            Task.Run(() => _logger.LogDetailAsync($"Ipti OC Clear: BayController: {bayControllerId} Address: {address}"));
            var cmd = bayId + DisplayOc + "0100" + "            ";
            _transmitter.SendData(cmd);
        }

        public void ShowBli(int bayControllerId, int address, int beacon, string text)
        {
            if (!_bliEnabled) return;
            var bli = new Ipti_BLI(bayControllerId, address, beacon, text);
            ShowBli(bli);
        }

        public void ShowBli(int address, int beacon, string text)
        {
            if (!_bliEnabled) return;
            var bli = new Ipti_BLI(_neutronVariables.BliController, address, beacon, text);
            ShowBli(bli);
        }
        public void ShowBli(Hart_BLI bli)
        {
            // throw new NotImplementedException();
        }
        public void ShowBli(Ipti_BLI bli)
        {
            if (!_bliEnabled) return;
            Task.Run(() => _logger.LogDetailAsync($"Ipti BLI-Show:  BayController: {bli.BLI_BayController}  Address: {bli.BLI_Address}"));
            _transmitter.SendData(bli.TurnOn);
        }
        public void ClearOc(int bayControllerId, int address)
        {
            var bayId = bayControllerId.ToString().PadLeft(2, '0');
            if (!_bliEnabled) return;
            Task.Run(() => _logger.LogDetailAsync($"Ipti OC Clear: BayController: {bayControllerId} Address: {address}"));
            var cmd = bayId + DisplayOc + "0100" + "            ";
            _transmitter.SendData(cmd);
        }
        public void ShowOc(int bayControllerId, int address, int beacon, string text)
        {
            var bayId = bayControllerId.ToString().PadLeft(2, '0');
            if (!_bliEnabled) return;
            Task.Run(() => _logger.LogDetailAsync($"Ipti OC Show:  BayController: {bayControllerId} Address: {address}"));
            var cmd = bayId + DisplayOc + "0100" + text;
            _transmitter.SendData(cmd);
        }
        public void ShowShi(int device, int bin, int level, string part, string text)
        {
            if (!_shiEnabled) return;
            Task.Run(() => _logger.LogDetailAsync($"ShowShi -- Device: {device}  Bin: {bin}  Level: {level}  Part: {part}  Text: {text}"));
            var towerLevelInfo = GetShiAddress(device, level);
            var shi = new Ipti_SHI(towerLevelInfo, _lBeacon, _rBeacon, part, text);
            _transmitter.SendData(shi.TurnOn());
        }
        public void ShowShi(Hart_SHI shi)
        {
            //  throw new NotImplementedException();
        }
        public void ClearBli(Hart_BLI bli)
        {
            //  throw new NotImplementedException();
        }
        public void ClearShi(Hart_SHI shi)
        {
            //  throw new NotImplementedException();
        }
        public void ClearBli(Ipti_BLI bli)
        {
            if (!_bliEnabled) return;
            Task.Run(() => _logger.LogDetailAsync($"BLI Clear Single Display:  BayController: {bli.BLI_BayController}  {bli.BLI_Address}"));
            _transmitter.SendData(bli.TurnOff);
        }
        public int GetInitStatus()
        {
            if (_transmitter.IsClientConnected)
            {
                return 0;
            }
            return 1;
        }
        private void FillBliList()
        {
            for (var i = 1; i <= _neutronVariables.PickBatchSize; i++)
            {
                _bliList.Add(new Ipti_BLI(_neutronVariables.BliController, i, 0, i.ToString()));
            }
        }
        private void CreateLog()
        {
            var logFileDir = LoaderSettings.GetLogFileDirectory();
            var folderName =
                $"TCP_IPTI Display Controller_{_workstation.WorkstationNumber}";
            var logActivity = LoaderSettings.EnableLogging;
            _logger = new DynamicLogger(logFileDir, folderName, logActivity);
        }
        private void StartTransmission()
        {
            _ = _logger.LogDetailAsync($"IPTI Controller - StartTransmission - Start");
            while (Transmit)
            {
                try
                {
                    if (_transmitter.IsClientConnected)
                    {
                        foreach (var request in RequestBlockingCollection.GetConsumingEnumerable(Token.Token))
                        {
                            if (Token.IsCancellationRequested)
                            {
                                _ = _logger.LogDetailAsync($"Start Transmission Cancellation Requested.");
                                return;
                            }

                            _ = _logger.LogDetailAsync($"IPTI Controller - Start Transmission - RequestBlockingCollection Loop: {request.ByteArrayToStringX2()}");

                            //for (var i = 1; i <= 100; i++)
                            //{
                            //    if (!_responseManager.Transmitting)
                            //    {

                            var trans = _responseManager.Transmitting;

                            Mediator.GetInstance().OnSerialPortWrite(this, $"Write Command:  {trans.ToString()} - {request.ByteArrayToStringX2()}");
                            _ = _logger.LogDetailAsync($"IPTI Controller - StartTransmission - TCP Transmitter Write {request}");
                            _transmitter.SendData(request.ByteArrayToString());
                            _responseManager.Transmitting = true;
                            Thread.Sleep(50);


                            // break;
                            //    }
                            //    Thread.Sleep(i * 20);
                            //    if (i != 100) continue;
                            //    Mediator.GetInstance().OnSerialPortWrite(this, "Serial Timeout.");
                            //    _ = _logger.LogDetailAsync("SerialPort Write Request Time Out.");
                            //}



                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    _ = _logger.LogDetailAsync($"CATCH - Start Transmission Operation Canceled Requested.");
                    return;
                }

                Thread.Sleep(50);
            }

            _ = _logger.LogDetailAsync($"EXITING Start Transmission.");
        }
        public void IptiControllerInit()
        {
            var loggingMessage = string.Empty;

            var displayDevice = _hardwareDevice;

            if (displayDevice != null)
            {
                var tcpConfiguration = _repoTcp.FindBy(r => r.Id == displayDevice.TcpConfiguration.Id).FirstOrDefault();
                if (tcpConfiguration != null)
                {
                    Task.Run(() => _logger.LogDetailAsync($"Hardware Device: {displayDevice.Name}"));
                    Task.Run(() => _logger.LogDetailAsync($"TCP Address: {tcpConfiguration.IPAddress} Port: {tcpConfiguration.Port}"));
                    _transmitter = new TcpTransmitter(tcpConfiguration.IPAddress, tcpConfiguration.Port, _logger);

                    if (_transmitter.IsClientConnected)
                    {
                        SetTransmit(true);
                        Task.Factory.StartNew(StartTransmission, CancellationToken.None, TaskCreationOptions.None,
                            TaskScheduler.Default);

                        Task.Factory.StartNew(() => _responseManager.StartResponseProcessor(), CancellationToken.None,
                            TaskCreationOptions.None, TaskScheduler.Default);

                        Task.Factory.StartNew(() => _responseManager.Start(), CancellationToken.None,
                            TaskCreationOptions.None, TaskScheduler.Default);

                    }
                    else
                    {
                        SetTransmit(false);
                    }
                }
                else
                {
                    MessageBox.Show("TCP Configuration not set in TCP Configuration.");
                }
            }
            else
            {
                MessageBox.Show("Display device not found in Hardware Devices.");
            }
        }


        #region Tower Code

        private string GetAddress(int device, int level)
        {
            var address = string.Empty;
            switch (device)
            {
                case 1:
                    _lBeacon = 2;
                    _rBeacon = 0;
                    address = $"1{level.ToString().PadLeft(2, '0')}";
                    break;
                case 2:
                    _lBeacon = 0;
                    _rBeacon = 2;
                    address = $"1{level.ToString().PadLeft(2, '0')}";
                    break;
                case 3:
                    _lBeacon = 2;
                    _rBeacon = 0;
                    address = $"2{level.ToString().PadLeft(2, '0')}";
                    break;
                case 4:
                    _lBeacon = 0;
                    _rBeacon = 2;
                    address = $"2{level.ToString().PadLeft(2, '0')}";
                    break;
            }

            Task.Run(() => _logger.LogDetailAsync($"Get Address Returned: {address}"));

            return address;
        }

        private TowerLevelInfo GetShiAddress(int device, int level)
        {
            TowerLevelInfo rec = null;
            try
            {
                var key = device * 10 + level;
                _towerLevelInfoList.TryGetValue(key, out rec);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to retrieve SHI Info.  {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
            return rec;
        }

        public void GetTowerLevelInfoList()
        {
            var towerList = new Dictionary<int, TowerLevelInfo>();
            if (_workstation.WorkstationNumber == 1)
            {
                var rec = new TowerLevelInfo { Device = 1, Level = 1, BayId = "04", Display = "01", ArrowDirection = "Left" };
                towerList.Add(11, rec);
                rec = new TowerLevelInfo { Device = 1, Level = 2, BayId = "04", Display = "02", ArrowDirection = "Left" };
                towerList.Add(12, rec);
                rec = new TowerLevelInfo { Device = 1, Level = 3, BayId = "04", Display = "03", ArrowDirection = "Left" };
                towerList.Add(13, rec);
                rec = new TowerLevelInfo { Device = 1, Level = 4, BayId = "04", Display = "04", ArrowDirection = "Left" };
                towerList.Add(14, rec);
                rec = new TowerLevelInfo { Device = 1, Level = 5, BayId = "05", Display = "01", ArrowDirection = "Left" };
                towerList.Add(15, rec);
                rec = new TowerLevelInfo { Device = 1, Level = 6, BayId = "05", Display = "02", ArrowDirection = "Left" };
                towerList.Add(16, rec);
                rec = new TowerLevelInfo { Device = 1, Level = 7, BayId = "05", Display = "03", ArrowDirection = "Left" };
                towerList.Add(17, rec);
                rec = new TowerLevelInfo { Device = 1, Level = 8, BayId = "05", Display = "04", ArrowDirection = "Left" };
                towerList.Add(18, rec);
                rec = new TowerLevelInfo { Device = 2, Level = 1, BayId = "04", Display = "01", ArrowDirection = "Right" };
                towerList.Add(21, rec);
                rec = new TowerLevelInfo { Device = 2, Level = 2, BayId = "04", Display = "02", ArrowDirection = "Right" };
                towerList.Add(22, rec);
                rec = new TowerLevelInfo { Device = 2, Level = 3, BayId = "04", Display = "03", ArrowDirection = "Right" };
                towerList.Add(23, rec);
                rec = new TowerLevelInfo { Device = 2, Level = 4, BayId = "04", Display = "04", ArrowDirection = "Right" };
                towerList.Add(24, rec);
                rec = new TowerLevelInfo { Device = 2, Level = 5, BayId = "05", Display = "01", ArrowDirection = "Right" };
                towerList.Add(25, rec);
                rec = new TowerLevelInfo { Device = 2, Level = 6, BayId = "05", Display = "02", ArrowDirection = "Right" };
                towerList.Add(26, rec);
                rec = new TowerLevelInfo { Device = 2, Level = 7, BayId = "05", Display = "03", ArrowDirection = "Right" };
                towerList.Add(27, rec);
                rec = new TowerLevelInfo { Device = 2, Level = 8, BayId = "05", Display = "04", ArrowDirection = "Right" };
                towerList.Add(28, rec);
                rec = new TowerLevelInfo { Device = 3, Level = 1, BayId = "02", Display = "01", ArrowDirection = "Left" };
                towerList.Add(31, rec);
                rec = new TowerLevelInfo { Device = 3, Level = 2, BayId = "02", Display = "02", ArrowDirection = "Left" };
                towerList.Add(32, rec);
                rec = new TowerLevelInfo { Device = 3, Level = 3, BayId = "02", Display = "03", ArrowDirection = "Left" };
                towerList.Add(33, rec);
                rec = new TowerLevelInfo { Device = 3, Level = 4, BayId = "02", Display = "04", ArrowDirection = "Left" };
                towerList.Add(34, rec);
                rec = new TowerLevelInfo { Device = 3, Level = 5, BayId = "03", Display = "01", ArrowDirection = "Left" };
                towerList.Add(35, rec);
                rec = new TowerLevelInfo { Device = 3, Level = 6, BayId = "03", Display = "02", ArrowDirection = "Left" };
                towerList.Add(36, rec);
                rec = new TowerLevelInfo { Device = 3, Level = 7, BayId = "03", Display = "03", ArrowDirection = "Left" };
                towerList.Add(37, rec);
                rec = new TowerLevelInfo { Device = 3, Level = 8, BayId = "03", Display = "04", ArrowDirection = "Left" };
                towerList.Add(38, rec);
                rec = new TowerLevelInfo { Device = 4, Level = 1, BayId = "02", Display = "01", ArrowDirection = "Right" };
                towerList.Add(41, rec);
                rec = new TowerLevelInfo { Device = 4, Level = 2, BayId = "02", Display = "02", ArrowDirection = "Right" };
                towerList.Add(42, rec);
                rec = new TowerLevelInfo { Device = 4, Level = 3, BayId = "02", Display = "03", ArrowDirection = "Right" };
                towerList.Add(43, rec);
                rec = new TowerLevelInfo { Device = 4, Level = 4, BayId = "02", Display = "04", ArrowDirection = "Right" };
                towerList.Add(44, rec);
                rec = new TowerLevelInfo { Device = 4, Level = 5, BayId = "03", Display = "01", ArrowDirection = "Right" };
                towerList.Add(45, rec);
                rec = new TowerLevelInfo { Device = 4, Level = 6, BayId = "03", Display = "02", ArrowDirection = "Right" };
                towerList.Add(46, rec);
                rec = new TowerLevelInfo { Device = 4, Level = 7, BayId = "03", Display = "03", ArrowDirection = "Right" };
                towerList.Add(47, rec);
                rec = new TowerLevelInfo { Device = 4, Level = 8, BayId = "03", Display = "04", ArrowDirection = "Right" };
                towerList.Add(48, rec);
            }

            if (_workstation.WorkstationNumber == 2)
            {
                var rec = new TowerLevelInfo { Device = 1, Level = 1, BayId = "02", Display = "01", ArrowDirection = "Left" };
                towerList.Add(11, rec);
                rec = new TowerLevelInfo { Device = 1, Level = 2, BayId = "02", Display = "02", ArrowDirection = "Left" };
                towerList.Add(12, rec);
                rec = new TowerLevelInfo { Device = 1, Level = 3, BayId = "02", Display = "03", ArrowDirection = "Left" };
                towerList.Add(13, rec);
                rec = new TowerLevelInfo { Device = 1, Level = 4, BayId = "02", Display = "04", ArrowDirection = "Left" };
                towerList.Add(14, rec);
                rec = new TowerLevelInfo { Device = 1, Level = 5, BayId = "03", Display = "01", ArrowDirection = "Left" };
                towerList.Add(15, rec);
                rec = new TowerLevelInfo { Device = 1, Level = 6, BayId = "03", Display = "02", ArrowDirection = "Left" };
                towerList.Add(16, rec);
                rec = new TowerLevelInfo { Device = 1, Level = 7, BayId = "03", Display = "03", ArrowDirection = "Left" };
                towerList.Add(17, rec);
                rec = new TowerLevelInfo { Device = 1, Level = 8, BayId = "03", Display = "04", ArrowDirection = "Left" };
                towerList.Add(18, rec);
                rec = new TowerLevelInfo { Device = 2, Level = 1, BayId = "02", Display = "01", ArrowDirection = "Right" };
                towerList.Add(21, rec);
                rec = new TowerLevelInfo { Device = 2, Level = 2, BayId = "02", Display = "02", ArrowDirection = "Right" };
                towerList.Add(22, rec);
                rec = new TowerLevelInfo { Device = 2, Level = 3, BayId = "02", Display = "03", ArrowDirection = "Right" };
                towerList.Add(23, rec);
                rec = new TowerLevelInfo { Device = 2, Level = 4, BayId = "02", Display = "04", ArrowDirection = "Right" };
                towerList.Add(24, rec);
                rec = new TowerLevelInfo { Device = 2, Level = 5, BayId = "03", Display = "01", ArrowDirection = "Right" };
                towerList.Add(25, rec);
                rec = new TowerLevelInfo { Device = 2, Level = 6, BayId = "03", Display = "02", ArrowDirection = "Right" };
                towerList.Add(26, rec);
                rec = new TowerLevelInfo { Device = 2, Level = 7, BayId = "03", Display = "03", ArrowDirection = "Right" };
                towerList.Add(27, rec);
                rec = new TowerLevelInfo { Device = 2, Level = 8, BayId = "03", Display = "04", ArrowDirection = "Right" };
                towerList.Add(28, rec);
                rec = new TowerLevelInfo { Device = 3, Level = 1, BayId = "04", Display = "01", ArrowDirection = "Left" };
                towerList.Add(31, rec);
                rec = new TowerLevelInfo { Device = 3, Level = 2, BayId = "04", Display = "02", ArrowDirection = "Left" };
                towerList.Add(32, rec);
                rec = new TowerLevelInfo { Device = 3, Level = 3, BayId = "04", Display = "03", ArrowDirection = "Left" };
                towerList.Add(33, rec);
                rec = new TowerLevelInfo { Device = 3, Level = 4, BayId = "04", Display = "04", ArrowDirection = "Left" };
                towerList.Add(34, rec);
                rec = new TowerLevelInfo { Device = 3, Level = 5, BayId = "05", Display = "01", ArrowDirection = "Left" };
                towerList.Add(35, rec);
                rec = new TowerLevelInfo { Device = 3, Level = 6, BayId = "05", Display = "02", ArrowDirection = "Left" };
                towerList.Add(36, rec);
                rec = new TowerLevelInfo { Device = 3, Level = 7, BayId = "05", Display = "03", ArrowDirection = "Left" };
                towerList.Add(37, rec);
                rec = new TowerLevelInfo { Device = 3, Level = 8, BayId = "05", Display = "04", ArrowDirection = "Left" };
                towerList.Add(38, rec);
                rec = new TowerLevelInfo { Device = 4, Level = 1, BayId = "04", Display = "01", ArrowDirection = "Right" };
                towerList.Add(41, rec);
                rec = new TowerLevelInfo { Device = 4, Level = 2, BayId = "04", Display = "02", ArrowDirection = "Right" };
                towerList.Add(42, rec);
                rec = new TowerLevelInfo { Device = 4, Level = 3, BayId = "04", Display = "03", ArrowDirection = "Right" };
                towerList.Add(43, rec);
                rec = new TowerLevelInfo { Device = 4, Level = 4, BayId = "04", Display = "04", ArrowDirection = "Right" };
                towerList.Add(44, rec);
                rec = new TowerLevelInfo { Device = 4, Level = 5, BayId = "05", Display = "01", ArrowDirection = "Right" };
                towerList.Add(45, rec);
                rec = new TowerLevelInfo { Device = 4, Level = 6, BayId = "05", Display = "02", ArrowDirection = "Right" };
                towerList.Add(46, rec);
                rec = new TowerLevelInfo { Device = 4, Level = 7, BayId = "05", Display = "03", ArrowDirection = "Right" };
                towerList.Add(47, rec);
                rec = new TowerLevelInfo { Device = 4, Level = 8, BayId = "05", Display = "04", ArrowDirection = "Right" };
                towerList.Add(48, rec);
            }

            if (_workstation.WorkstationNumber == 3)
            {
                var rec = new TowerLevelInfo { Device = 1, Level = 1, BayId = "02", Display = "01", ArrowDirection = "Left" };
                towerList.Add(11, rec);
                rec = new TowerLevelInfo { Device = 1, Level = 2, BayId = "02", Display = "02", ArrowDirection = "Left" };
                towerList.Add(12, rec);
                rec = new TowerLevelInfo { Device = 1, Level = 3, BayId = "02", Display = "03", ArrowDirection = "Left" };
                towerList.Add(13, rec);
                rec = new TowerLevelInfo { Device = 1, Level = 4, BayId = "02", Display = "04", ArrowDirection = "Left" };
                towerList.Add(14, rec);
                rec = new TowerLevelInfo { Device = 1, Level = 5, BayId = "03", Display = "01", ArrowDirection = "Left" };
                towerList.Add(15, rec);
                rec = new TowerLevelInfo { Device = 1, Level = 6, BayId = "03", Display = "02", ArrowDirection = "Left" };
                towerList.Add(16, rec);
                rec = new TowerLevelInfo { Device = 1, Level = 7, BayId = "03", Display = "03", ArrowDirection = "Left" };
                towerList.Add(17, rec);
                rec = new TowerLevelInfo { Device = 1, Level = 8, BayId = "03", Display = "04", ArrowDirection = "Left" };
                towerList.Add(18, rec);
                rec = new TowerLevelInfo { Device = 2, Level = 1, BayId = "02", Display = "01", ArrowDirection = "Right" };
                towerList.Add(21, rec);
                rec = new TowerLevelInfo { Device = 2, Level = 2, BayId = "02", Display = "02", ArrowDirection = "Right" };
                towerList.Add(22, rec);
                rec = new TowerLevelInfo { Device = 2, Level = 3, BayId = "02", Display = "03", ArrowDirection = "Right" };
                towerList.Add(23, rec);
                rec = new TowerLevelInfo { Device = 2, Level = 4, BayId = "02", Display = "04", ArrowDirection = "Right" };
                towerList.Add(24, rec);
                rec = new TowerLevelInfo { Device = 2, Level = 5, BayId = "03", Display = "01", ArrowDirection = "Right" };
                towerList.Add(25, rec);
                rec = new TowerLevelInfo { Device = 2, Level = 6, BayId = "03", Display = "02", ArrowDirection = "Right" };
                towerList.Add(26, rec);
                rec = new TowerLevelInfo { Device = 2, Level = 7, BayId = "03", Display = "03", ArrowDirection = "Right" };
                towerList.Add(27, rec);
                rec = new TowerLevelInfo { Device = 2, Level = 8, BayId = "03", Display = "04", ArrowDirection = "Right" };
                towerList.Add(28, rec);
                rec = new TowerLevelInfo { Device = 3, Level = 1, BayId = "04", Display = "01", ArrowDirection = "Right" };
                towerList.Add(31, rec);
                rec = new TowerLevelInfo { Device = 3, Level = 2, BayId = "04", Display = "02", ArrowDirection = "Right" };
                towerList.Add(32, rec);
                rec = new TowerLevelInfo { Device = 3, Level = 3, BayId = "04", Display = "03", ArrowDirection = "Right" };
                towerList.Add(33, rec);
                rec = new TowerLevelInfo { Device = 3, Level = 4, BayId = "04", Display = "04", ArrowDirection = "Right" };
                towerList.Add(34, rec);
                rec = new TowerLevelInfo { Device = 3, Level = 5, BayId = "05", Display = "01", ArrowDirection = "Right" };
                towerList.Add(35, rec);
                rec = new TowerLevelInfo { Device = 3, Level = 6, BayId = "05", Display = "02", ArrowDirection = "Right" };
                towerList.Add(36, rec);
                rec = new TowerLevelInfo { Device = 3, Level = 7, BayId = "05", Display = "03", ArrowDirection = "Right" };
                towerList.Add(37, rec);
                rec = new TowerLevelInfo { Device = 3, Level = 8, BayId = "05", Display = "04", ArrowDirection = "Right" };
                towerList.Add(38, rec);
            }

            _towerLevelInfoList = towerList;
        }

        #endregion
    }
}