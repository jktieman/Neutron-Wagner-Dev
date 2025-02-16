using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AlliedLogger;
using AsyncAwaitBestPractices;
using EthernetTransmitter;
using JsonManager;
using NeutronCore.Extensions;
using NeutronCore.Global;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.ModelViews;
using NeutronData.Repositories;
using NeutronEvents;


namespace IPTI.Models
{
    // ReSharper disable once InconsistentNaming
    public class TcpIptiController : IDisplayController
    {
        private const string DisplayOc = "27";
        private const string TurnAllOff14 = "14";
        private const string TurnAllOn02 = "02";
        private const string TurnAllOff03 = "03";
        private const string TurnAllOn06 = "06";
        private const string TurnAllOff07 = "07";

        // private TcpIptiCommandCenter _tcpIptiCommandCenter;


        public CancellationTokenSource Token = new CancellationTokenSource();
        private static readonly BlockingCollection<byte[]>
            ResponseBlockingCollection = new BlockingCollection<byte[]>();

        private static readonly BlockingCollection<byte[]>
            RequestBlockingCollection = new BlockingCollection<byte[]>();

        private static readonly BlockingCollection<byte[]>
            ReceivedBlockingCollection = new BlockingCollection<byte[]>();

        private readonly List<int> _blastzoneControllers = new List<int> { 1, 2, 3 };
        private string _bliController = "04";

        private readonly List<IptiBli> _bliList = new List<IptiBli>();
        private readonly NeutronVariables _neutronVariables;
        private readonly HardwareDevice _hardwareDevice;
        private readonly TcpConfiguration _tcpConfiguration;
        private readonly IptiConfig _iptiConfig;

        private readonly IJsonData _jsonData;

        private IptiResponseManager _responseManager;

        private readonly bool _bliEnabled;
        private readonly bool _shiEnabled;

        private readonly WorkstationView _workstationView;

        private IDynamicLogger _logger;

       // private TcpTransmitter _transmitter;

        private TcpServer _tcpServer;

        public bool TcpIptiControllerEnabled = true;

        public bool Transmit { get; set; }
        private string _cError = string.Empty;
        private int _lBeacon;
        private int _rBeacon;
        private Dictionary<int, TowerLevelInfo> _towerLevelInfoList;

       // public event EventHandler<MyDataReceivedEventArgs> MyDataReceived;
        public bool Ready { get; set; }
        public TcpIptiController(IJsonData jsonData, WorkstationView workstationView, NeutronVariables neutronVariables,
            HardwareDevice hardwareDevice, TcpConfiguration tcpConfiguration, IptiConfig iptiConfig)
        {
            _workstationView = workstationView;
            _neutronVariables = neutronVariables;
            _hardwareDevice = _workstationView.BatchTable;  
            _tcpConfiguration = tcpConfiguration;
            _iptiConfig = iptiConfig;
            _jsonData = jsonData;
            _bliEnabled = _hardwareDevice.Enabled;
            _shiEnabled = _neutronVariables.ShiEnabled;

            Init();
        }

        private void Init()
        {
            _bliController = _neutronVariables.BliController.ToString().PadLeft(2, '0');
            _logger = NeutronCore.Global.Logger.SetupLogger("TCP_IptiController");
            if (_bliEnabled) FillBliList();

            if (_shiEnabled) GetTowerLevelInfoList();

            _responseManager = new IptiResponseManager(ResponseBlockingCollection, RequestBlockingCollection,
                    ReceivedBlockingCollection)
            { Transmit = false };

            IptiControllerInit();
        }

        public void SendText(string text)
        {

            if (_tcpServer != null)
            {
                _tcpServer.SendData(text);
                // Task.Delay(_iptiConfig.TransmitDelay).Wait();
            }
            else
            {
                _logger.LogDetailAsync($"Interface Client is Not Connected.").SafeFireAndForget();

            }
        }

     public Task TurnOnAllBli()
        {
            throw new NotImplementedException();
        }

        public Task ClearAllBli()
        {
            throw new NotImplementedException();
        }

        public Task ClearAllShi()
        {
            throw new NotImplementedException();
        }

        public Task TurnOnAllBlastzones()
        {
            throw new NotImplementedException();
        }

        public Task ShowBli(int address, int beacon, string text)
        {
            throw new NotImplementedException();
        }

        public Task ShowBli(int bayControllerId, int address, int beacon, string text)
        {
            throw new NotImplementedException();
        }

        public Task ShowBlastzone(int bayController, int address, int beacon, string text)
        {
            throw new NotImplementedException();
        }

        public Task ClearBlastzone()
        {
            throw new NotImplementedException();
        }

        public Task ShowBlastzoneOc(int bayController, int address, int beacon, string text)
        {
            throw new NotImplementedException();
        }

        public Task ClearBlastzoneOc(int bayControllerId, int address, int beacon, string text)
        {
            throw new NotImplementedException();
        }

        public Task ShowBli(IptiBli bli)
        {
            throw new NotImplementedException();
        }

        public Task ClearBli(IptiBli bli)
        {
            throw new NotImplementedException();
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
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Close TCP Port Exception: {ex.Message}{Environment.NewLine}{ex.InnerException}").SafeFireAndForget();
            }
        }
        //public async Task TurnOnAllBli()
        //{
        //    _logger.LogDetailAsync($"IPTI Controller - Turn On All BLI - START").SafeFireAndForget();
        //    //await _transmitter.SendDataAsync($"{_bliController}{TurnAllOn02}");
        //    _logger.LogDetailAsync($"IPTI Controller - Turn On All BLI - END").SafeFireAndForget();
        //}
        //public async Task TurnOnAllBlastzones()
        //{
        //    _logger.LogDetailAsync($"IPTI Controller - Turn On All BLI - START").SafeFireAndForget();
        //    foreach (var bayController in _blastzoneControllers)
        //    {
        //        var bayId = bayController.ToString().PadLeft(2, '0');
        //        // await _transmitter.SendData($"{bayId}{TurnAllOn02}");

        //        //await _transmitter.SendDataAsync($"{bayId}{TurnAllOn02}");

        //    }
        //    _logger.LogDetailAsync($"IPTI Controller - Turn On All BLI - END").SafeFireAndForget();
        //}
        //public async Task ClearAllBli()
        //{
        //    _logger.LogDetailAsync($"IPTI Controller - Clear All BLI - START  {DateTime.Now.ToLongTimeString()}").SafeFireAndForget();
        //    if (!_bliEnabled)
        //    {
        //        _logger.LogDetailAsync($"IPTI Controller - Clear All BLI - BLI NOT Enabled").SafeFireAndForget();
        //        return;
        //    }
        //    var bayIds = new List<string> { "01", "02", "03", "04" };

        //    foreach (var bayId in bayIds)
        //    {
        //       // await _transmitter.SendDataAsync($"{bayId}{TurnAllOff14}");
        //    }
        //    _logger.LogDetailAsync($"IPTI Controller - Clear All BLI - END  {DateTime.Now.ToLongTimeString()}").SafeFireAndForget();
        //}
        //public async Task ClearAllShi()
        //{
        //    if (!_shiEnabled) return;
        //    for (var i = 2; i < 6; i++)
        //    {
        //        for (var j = 1; j < 5; j++)
        //        {
        //            var cmd = i.ToString().PadLeft(2, '0') + "39" + j.ToString().PadLeft(2, '0');
        //           // await _transmitter.SendDataAsync(cmd);
        //        }
        //    }
        //}
        //public async Task ShowShiAsync(IptiShi shi)
        //{
        //   // await _transmitter.SendDataAsync(shi.TurnOn());
        //    _logger.LogDetailAsync($"ShowShi  Turning ON BayId: {shi.BayId}  Display: {shi.DisplayId}").SafeFireAndForget();
        //}
        //public async Task ClearShi(IptiShi shi)
        //{
        //    if (_shiEnabled)
        //    {
        //       // await _transmitter.SendDataAsync(shi.TurnOff());
        //        _logger.LogDetailAsync($"ClearShi Turning OFF BayId: {shi.BayId}  Display: {shi.DisplayId}").SafeFireAndForget();
        //    }
        //}
        //public async Task ShowBlastzone(int bayControllerId, int address, int beacon, string text)
        //{
        //    _logger.LogDetailAsync($"IPTI Controller - Show Blastzone - START").SafeFireAndForget();
        //    if (!_bliEnabled) return;
        //    var bli = new IptiBli(bayControllerId, address, beacon, text);
        //    await ShowBli(bli);
        //    _logger.LogDetailAsync($"IPTI Controller - Show Blastzone - END").SafeFireAndForget();
        //}
        //public async Task ClearBlastzone()
        //{
        //    _logger.LogDetailAsync($"IPTI Controller - Clear Blastzone - START").SafeFireAndForget();
        //    if (!_bliEnabled) return;
        //    foreach (var bayController in _blastzoneControllers)
        //    {
        //        var bayId = bayController.ToString().PadLeft(2, '0');
        //      //  await _transmitter.SendDataAsync($"{bayId}{TurnAllOff14}");
        //        Thread.Sleep(100);
        //    }
        //    _logger.LogDetailAsync($"IPTI Controller - Clear Blastzone - END").SafeFireAndForget();
        //}
        //public async Task ShowBlastzoneOc(int bayControllerId, int address, int beacon, string text)
        //{
        //    _logger.LogDetailAsync($"IPTI Controller - Show Blastzone OC - START").SafeFireAndForget();
        //    var bayId = bayControllerId.ToString().PadLeft(2, '0');
        //    if (!_bliEnabled) return;
        //    _logger.LogDetailAsync($"Ipti OC Show:  BayController: {bayControllerId} Address: {address}").SafeFireAndForget();
        //    var cmd = bayId + DisplayOc + "0100" + text;
        //   // await _transmitter.SendDataAsync(cmd);
        //    _logger.LogDetailAsync($"IPTI Controller - Show Blastzone OC - END").SafeFireAndForget();
        //}
        //public async Task ClearBlastzoneOc(int bayControllerId, int address, int beacon, string text)
        //{
        //    _logger.LogDetailAsync($"IPTI Controller - Clear Blastzone OC - START").SafeFireAndForget();
        //    var bayId = bayControllerId.ToString().PadLeft(2, '0');
        //    if (!_bliEnabled) return;
        //    _logger.LogDetailAsync($"Ipti OC Clear: BayController: {bayControllerId} Address: {address}").SafeFireAndForget();
        //    var cmd = bayId + DisplayOc + "0100" + "            ";
        // //   await _transmitter.SendDataAsync(cmd);
        //    _logger.LogDetailAsync($"IPTI Controller - Clear Blastzone OC - END").SafeFireAndForget();
        //}
        //public async Task ShowBli(int bayControllerId, int address, int beacon, string text)
        //{
        //    _logger.LogDetailAsync($"IPTI Controller - Show BLI - START").SafeFireAndForget();
        //    if (!_bliEnabled) return;
        //    var bli = new IptiBli(bayControllerId, address, beacon, text);
        //    await ShowBli(bli);
        //    _logger.LogDetailAsync($"IPTI Controller - Show BLI - END").SafeFireAndForget();
        //}
        //public async Task ShowBli(int address, int beacon, string text)
        //{
        //    _logger.LogDetailAsync($"IPTI Controller - Show BLI - START").SafeFireAndForget();
        //    if (!_bliEnabled) return;
        //    var bli = new IptiBli(_bliController.ParseInt(), address, beacon, text);
        //    await ShowBli(bli);
        //    _logger.LogDetailAsync($"IPTI Controller - Show BLI - END").SafeFireAndForget();
        //}
        //public async Task ShowBli(IptiBli bli)
        //{
        //    //_ = _logger.LogDetailAsync($"IPTI Controller - Show BLI - START");
        //    //if (!_bliEnabled) return;
        //    //_ = _logger.LogDetailAsync($"Ipti BLI-Show:  BayController: {bli.BLI_BayController}  Address: {bli.BLI_Address} " +
        //    //                           $"Text: {bli.BLI_Text}");
        //    //var cmd = _tcpIptiCommandCenter.TurnOnDisplay(bli.BLI_BayController, bli.BLI_Address, bli.BLI_Text);
        //    await Task.CompletedTask;
        //    //_transmitter.SendData(cmd);
        //    //// await _transmitter.SendData(bli.TurnOn);
        //    //_ = _logger.LogDetailAsync($"IPTI Controller - Show BLI - END");
        //}
        //public async Task ClearOcAsync(int bayControllerId, int address)
        //{
        //    _logger.LogDetailAsync($"IPTI Controller - Clear OC - START").SafeFireAndForget();

        //    var bayId = bayControllerId.ToString().PadLeft(2, '0');
        //    if (!_bliEnabled) return;
        //    _logger.LogDetailAsync($"Ipti OC Clear: BayController: {bayControllerId} Address: {address}").SafeFireAndForget();
        //    var cmd = bayId + DisplayOc + "0100" + "            ";
        //  //  await _transmitter.SendDataAsync(cmd);
        //    _logger.LogDetailAsync($"IPTI Controller - Clear OC - END").SafeFireAndForget();
        //}
        //public async Task ShowOc(int bayControllerId, int address, int beacon, string text)
        //{
        //    _logger.LogDetailAsync($"IPTI Controller - Show OC - START").SafeFireAndForget();
        //    var bayId = bayControllerId.ToString().PadLeft(2, '0');
        //    if (!_bliEnabled) return;
        //    _logger.LogDetailAsync($"Ipti OC Show:  BayController: {bayControllerId} Address: {address}").SafeFireAndForget();
        //    var cmd = bayId + DisplayOc + "0100" + text;
        //  //  await _transmitter.SendDataAsync(cmd);
        //    _logger.LogDetailAsync($"IPTI Controller - Show OC - END").SafeFireAndForget();
        //}
        //public async Task ShowShi(int device, int bin, int level, string part, string text)
        //{
        //    if (!_shiEnabled) return;
        //    _logger.LogDetailAsync($"ShowShi -- Device: {device}  Bin: {bin}  Level: {level}  Part: {part}  Text: {text}").SafeFireAndForget();
        //    var towerLevelInfo = GetShiAddress(device, level);
        //    var shi = new IptiShi(towerLevelInfo, _lBeacon, _rBeacon, part, text);
        //   // await _transmitter.SendDataAsync(shi.TurnOn());
        //}
        //public async Task ClearBli(IptiBli bli)
        //{
        //    _logger.LogDetailAsync($"IPTI Controller - Clear BLI - START").SafeFireAndForget();
        //    if (!_bliEnabled) return;
        //    _logger.LogDetailAsync($"BLI Clear Single Display:  BayController: {bli.BLI_BayController}  {bli.BLI_Address}").SafeFireAndForget();
        //  //  await _transmitter.SendDataAsync(bli.TurnOff);
        //    _logger.LogDetailAsync($"IPTI Controller - Clear BLI - END").SafeFireAndForget();
        //}

        public bool GetInitStatus()
        {
            if (_tcpServer == null) return false;

            // if (_transmitter.IsClientConnected)
            var clientList = _tcpServer.ListClients();
            return clientList.Any();
        }

        public Task ShowOc(int bayControllerId, int address, int beacon, string text)
        {
            throw new NotImplementedException();
        }

        public Task ClearOcAsync(int bayControllerId, int address)
        {
            throw new NotImplementedException();
        }

        public Task ShowShi(int device, int bin, int level, string part, string text)
        {
            throw new NotImplementedException();
        }

        public Task ShowShiAsync(IptiShi shi)
        {
            throw new NotImplementedException();
        }

        public Task ClearShi(IptiShi shi)
        {
            throw new NotImplementedException();
        }

        private void FillBliList()
        {
            for (var i = 1; i <= _neutronVariables.PickBatchSize; i++)
            {
                _bliList.Add(new IptiBli(_bliController.ParseInt(), i, 0, i.ToString()));
            }
        }
        //private async Task StartTransmission()
        //{
        //    _ = _logger.LogDetailAsync($"IPTI Controller - StartTransmission - Start");

        //    while (Transmit)
        //    {
        //        try
        //        {
        //            if (_transmitter.IsClientConnected)
        //            {
        //                foreach (var request in RequestBlockingCollection.GetConsumingEnumerable(Token.Token))
        //                {
        //                    if (Token.IsCancellationRequested)
        //                    {
        //                        _ = _logger.LogDetailAsync($"Start Transmission Cancellation Requested.");
        //                        return;
        //                    }

        //                    _ = _logger.LogDetailAsync($"IPTI Controller - Start Transmission - RequestBlockingCollection Loop: {request.ByteArrayToStringX2()}");

        //                    //for (var i = 1; i <= 100; i++)
        //                    //{
        //                    //    if (!_responseManager.Transmitting)
        //                    //    {

        //                    var trans = _responseManager.Transmitting;

        //                    Mediator.GetInstance().OnSerialPortWrite(this, $"Write Command:  {trans.ToString()} - {request.ByteArrayToStringX2()}");
        //                    _ = _logger.LogDetailAsync($"IPTI Controller - StartTransmission - TCP Transmitter Write {request}");
        //                  //  await _transmitter.SendDataAsync(request.ByteArrayToString());
        //                    _responseManager.Transmitting = true;
        //                    Thread.Sleep(50);


        //                    // break;
        //                    //    }
        //                    //    Thread.Sleep(i * 20);
        //                    //    if (i != 100) continue;
        //                    //    Mediator.GetInstance().OnSerialPortWrite(this, "Serial Timeout.");
        //                    //    _ = _logger.LogDetailAsync("SerialPort Write Request Time Out.");
        //                    //}



        //                }
        //            }
        //        }
        //        catch (OperationCanceledException)
        //        {
        //            _ = _logger.LogDetailAsync($"CATCH - Start Transmission Operation Canceled Requested.");
        //            return;
        //        }

        //        Thread.Sleep(50);
        //    }

        //    _ = _logger.LogDetailAsync($"EXITING Start Transmission.");
        //}
        public void IptiControllerInit()
        {
            _logger.LogDetailAsync($"Begin Init Controller").SafeFireAndForget();
            var loggingMessage = string.Empty;

            var displayDevice = _hardwareDevice;

            try
            {
                if (displayDevice != null)
                {
                    if (displayDevice.CommunicationTypeId == 1)
                    {
                        //var tcpConfiguration = _repoTcp.FindBy(r => r.Id == displayDevice.TcpConfiguration.Id).FirstOrDefault();
                        if (_tcpConfiguration != null)
                        {
                            _logger.LogDetailAsync($"Hardware Device: {displayDevice.Name}").SafeFireAndForget();
                            _logger.LogDetailAsync($"TCP Address: {_tcpConfiguration.IPAddress} Port: {_tcpConfiguration.Port}").SafeFireAndForget();
                            //_transmitter = new TcpTransmitter(_tcpConfiguration.IPAddress, _tcpConfiguration.Port, _iptiConfig.TransmitDelay);

                            _tcpServer = new TcpServer(_tcpConfiguration.IPAddress, _tcpConfiguration.Port, _iptiConfig.TransmitDelay);


                            //if (_transmitter.IsClientConnected)
                            //{
                            //    SetTransmit(true);
                            //    //Task.Factory.StartNew(StartTransmission, CancellationToken.None, TaskCreationOptions.None,
                            //    //    TaskScheduler.Default);

                            //    //Task.Factory.StartNew(() => _responseManager.StartResponseProcessor(), CancellationToken.None,
                            //    //    TaskCreationOptions.None, TaskScheduler.Default);

                            //    //Task.Factory.StartNew(() => _responseManager.Start(), CancellationToken.None,
                            //    //    TaskCreationOptions.None, TaskScheduler.Default);

                            //}
                            //else
                            //{
                            //    SetTransmit(false);
                            //}
                        }
                        else
                        {
                            _logger.LogDetailAsync("TCP Configuration not set in TCP Configuration.").SafeFireAndForget();
                        }
                    }
                    else
                    {
                        _logger.LogDetailAsync("Display device not set to TCP in Hardware Devices.").SafeFireAndForget();
                    }
                }
                else
                {
                    _logger.LogDetailAsync("Display device not found in Hardware Devices.").SafeFireAndForget();
                }
            }
            catch (Exception ex)
            {
                var message = $"Error loading IPTI Light Controller. {Environment.NewLine}{ex.Message}";
                _logger.LogDetailAsync(message).SafeFireAndForget();
                Mediator.GetInstance().OnGeneralError(this, message);
                TcpIptiControllerEnabled = false;
            }
        }

        public void DisposeServer()
        {
            _tcpServer?.DisposeServer();
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

            _logger.LogDetailAsync($"Get Address Returned: {address}").SafeFireAndForget();

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
                throw new Exception($"Unable to retrieve SHI Info.  {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
            return rec;
        }
        public void GetTowerLevelInfoList()
        {
            var towerList = new Dictionary<int, TowerLevelInfo>();
            if (_workstationView.WorkstationNumber == 1)
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

            if (_workstationView.WorkstationNumber == 2)
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

            if (_workstationView.WorkstationNumber == 3)
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