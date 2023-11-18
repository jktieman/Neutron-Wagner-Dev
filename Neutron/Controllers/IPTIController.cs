using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlliedLogger;
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
    public class IptiController : IDisplayController
    {
        private const string Bayid = "01";
        private const string DisplayOc = "27";
        private const string TurnAllOff = "03";
        private const string TurnAllOn = "02";
        private readonly string _bliController = "04";

        public CancellationTokenSource Token = new CancellationTokenSource();
        private static readonly BlockingCollection<byte[]>
            ResponseBlockingCollection = new BlockingCollection<byte[]>();

        private static readonly BlockingCollection<byte[]>
            RequestBlockingCollection = new BlockingCollection<byte[]>();

        private static readonly BlockingCollection<byte[]>
            ReceivedBlockingCollection = new BlockingCollection<byte[]>();

        private List<int> _bayControllers = new List<int> { 1, 2, 3 };

        private readonly List<Ipti_BLI> _bliList = new List<Ipti_BLI>();
        private readonly NeutronVariables _neutronVariables;

        private readonly GenericRepository<SerialConfiguration> _repoSerial = new GenericRepository<SerialConfiguration>(new NeutronDb());
        private readonly GenericRepository<HardwareDevice> _repoHardwareDevice = new GenericRepository<HardwareDevice>(new NeutronDb());

        private readonly ResponseManager _responseManager;

        private readonly bool _bliEnabled;
        private readonly bool _shiEnabled;

        private readonly WorkstationView _workstation;

        private IDynamicLogger _logger;

        private SerialPort _serialPort;
        public bool Transmit { get; set; }
        private string _cError = string.Empty;
        private int _lBeacon;
        private int _rBeacon;
        private Dictionary<int, TowerLevelInfo> _towerLevelInfoList;

        public event EventHandler<MyDataReceivedEventArgs> MyDataReceived;
        public bool Ready { get; set; }

        public IptiController(IJsonData jsonData, WorkstationView workstation, NeutronVariables neutronVariables)
        {
            _workstation = workstation;
            _neutronVariables = neutronVariables;

            _bliEnabled = _neutronVariables.BliEnabled;
            _shiEnabled = _neutronVariables.ShiEnabled;

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
        public Task TurnOnAllBli()
        {
           SendData($"{_bliController}{TurnAllOn}");
           return Task.CompletedTask;
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

                if (_serialPort.IsOpen) _serialPort.Close();
            }
            catch (Exception ex)
            {
              _ =  Task.Run(() => _logger.LogDetailAsync($"Close Serial Port Exception: {ex.Message} {Environment.NewLine} {ex.InnerException} "));
            }
        }

        public async Task ClearAllBli()
        {
            await _logger.LogDetailAsync($"IPTI Controller - Clear All BLI - START");
            if (!_bliEnabled) return;
            await _logger.LogDetailAsync($"IPTI Controller - Clear All BLI - BLI Enabled");
            foreach (var bli in _bliList)
            {
                await _logger.LogDetailAsync($"IPTI Controller - Clear All BLI - SendData Turn Off:  {bli.BLI_Address}");
                SendData(bli.TurnOff);
                await _logger.LogDetailAsync($"IPTI Controller - Clear All BLI - SendData Turn Off Return");
            }
            await _logger.LogDetailAsync($"IPTI Controller - Clear All BLI - END");
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
            foreach (var bayController in _bayControllers)
            {
                var cmd = bayController.ToString().PadLeft(2, '0') + "07";
                SendData(cmd);
            }
        }
         
        public void ShowBlastzoneOc(int bayControllerId, int address, int beacon, string text)
        {
            var bayId = bayControllerId.ToString().PadLeft(2, '0');
            if (!_bliEnabled) return;
            Task.Run(() => _logger.LogDetailAsync($"Ipti OC Show:  BayController: {bayControllerId} Address: {address}"));
            var cmd = bayId + DisplayOc + "0100" + text;
            SendData(cmd);
        }
        public void ClearBlastzoneOc(int bayControllerId, int address, int beacon, string text)
        {
            var bayId = bayControllerId.ToString().PadLeft(2, '0');
            if (!_bliEnabled) return;
            Task.Run(() => _logger.LogDetailAsync($"Ipti OC Clear: BayController: {bayControllerId} Address: {address}"));
            var cmd = bayId + DisplayOc + "0100" + "            ";
            SendData(cmd);
        }

        public void ClearAllShi()
        {
            if (!_shiEnabled) return;
            for (var i = 2; i < 6; i++)
            {
                for (var j = 1; j < 5; j++)
                {
                    var cmd = i.ToString().PadLeft(2, '0') + "39" + j.ToString().PadLeft(2, '0');
                    SendData(cmd);
                }
            }
        }
        public void ShowShi(Ipti_SHI shi)
        {
            SendData(shi.TurnOn());
            Task.Run(() => _logger.LogDetailAsync($"ShowShi  Turning ON BayId: {shi.BayId}  Display: {shi.DisplayId}"));
        }
        public void ClearShi(Ipti_SHI shi)
        {
            if (_shiEnabled)
            {
                SendData(shi.TurnOff());
                Task.Run(() => _logger.LogDetailAsync($"ClearShi Turning OFF BayId: {shi.BayId}  Display: {shi.DisplayId}"));
            }
        }
        public void ShowBli(int bayController, int address, int beacon, string text)
        {
            if (!_bliEnabled) return;
            var bli = new Ipti_BLI(bayController, address, beacon, text);
            ShowBli(bli);
        }
        public void ShowBli(Hart_BLI bli)
        {
            // throw new NotImplementedException();
        }
        public void ShowBli(Ipti_BLI bli)
        {
            if (!_bliEnabled) return;
            Task.Run(() => _logger.LogDetailAsync($"Ipti BLI Address: {bli.BLI_Address}"));
            SendData(bli.TurnOn);
        }

        public void ClearOc(int bayControllerId, int address)
        {
            var bayId = bayControllerId.ToString().PadLeft(2, '0');
            if (!_bliEnabled) return;
            Task.Run(() => _logger.LogDetailAsync($"Ipti OC Clear: BayController: {bayControllerId} Address: {address}"));
            var cmd = bayId + DisplayOc + "0100" + "            ";
            SendData(cmd);
        }
        public void ShowOc(int bayControllerId, int address, int beacon, string text)
        {
            var bayId = bayControllerId.ToString().PadLeft(2, '0');
            if (!_bliEnabled) return;
            Task.Run(() => _logger.LogDetailAsync($"Ipti OC Show:  BayController: {bayControllerId} Address: {address}"));
            var cmd = bayId + DisplayOc + "0100" + text;
            SendData(cmd);
        }

        public void ClearOc(int address)
        {
            if (!_bliEnabled) return;
            Task.Run(() => _logger.LogDetailAsync($"Ipti OC Address Clear: {address}"));
            var cmd = Bayid + DisplayOc + "0100" + "            ";
            SendData(cmd);
        }

        public void ShowOc(int address, int beacon, string text)
        {
            if (!_bliEnabled) return;
            Task.Run(() => _logger.LogDetailAsync($"Ipti OC Address: {address}"));
            var cmd = Bayid + DisplayOc + "0100" + text;
            SendData(cmd);
        }

        public void ShowShi(int device, int bin, int level, string part, string text)
        {
            if (!_shiEnabled) return;
            Task.Run(() => _logger.LogDetailAsync($"ShowShi -- Device: {device}  Bin: {bin}  Level: {level}  Part: {part}  Text: {text}"));
            var towerLevelInfo = GetShiAddress(device, level);
            var shi = new Ipti_SHI(towerLevelInfo, _lBeacon, _rBeacon, part, text);
            SendData(shi.TurnOn());
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
            Task.Run(() => _logger.LogDetailAsync($"BLI Clear Single Display. {bli.BLI_Address}"));
            SendData(bli.TurnOff);
        }


        public void ShowBli(int address, int beacon, string text)
        {
            if (!_bliEnabled) return;
            var bli = new Ipti_BLI(_neutronVariables.BliController, address, beacon, text);
            ShowBli(bli);
        }

        public int GetInitStatus()
        {
            throw new NotImplementedException();
        }
        private void FillBliList()
        {
            for (var i = 1; i <= _neutronVariables.PickBatchSize; i++)
            {
                _bliList.Add(new Ipti_BLI(i, 0, i.ToString()));
            }
            //_bliList.Add(new Ipti_BLI(1, 0, "1"));
            //_bliList.Add(new Ipti_BLI(2, 0, "2"));
            //_bliList.Add(new Ipti_BLI(3, 0, "3"));
            //_bliList.Add(new Ipti_BLI(4, 0, "4"));
            //_bliList.Add(new Ipti_BLI(5, 0, "5"));
            //_bliList.Add(new Ipti_BLI(6, 0, "6"));
            // _bliList.Add(new Ipti_BLI(7, 0, "7"));
            // _bliList.Add(new Ipti_BLI(8, 0, "8"));
        }
        private void CreateLog()
        {
            var logFileDir = LoaderSettings.GetLogFileDirectory();
            var folderName =
                $"Display Controller_{_workstation.WorkstationNumber.ToString()}";
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
                    if (_serialPort.IsOpen)
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
                            _ = _logger.LogDetailAsync($"IPTI Controller - StartTransmission - SerialPort Write {request}");
                            _serialPort.Write(request, 0, request.Length);
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

            var displayDevice = _repoHardwareDevice
                .FindBy(r => r.DeviceTypeId == (int)DeviceTypeEnum.IptiDisplays && r.WorkstationId == _workstation.WorkstationId).FirstOrDefault();
            if (displayDevice != null)
            {
                var serialConfiguration = _repoSerial.FindBy(r => r.Id == displayDevice.SerialConfigurationId).FirstOrDefault();
                if (serialConfiguration != null)
                {
                    Task.Run(() => _logger.LogDetailAsync($"Hardware Device: {displayDevice.Name}"));
                    Task.Run(() => _logger.LogDetailAsync($"Serial Address: {serialConfiguration.PortName} Baud Rate: {serialConfiguration.BaudRate.ToString()}"));
                    Task.Run(() => _logger.LogDetailAsync($"Serial Port Number: {serialConfiguration.PortNumber.ToString()}"));
                    _serialPort = new SerialPort()
                    {
                        PortName = serialConfiguration.PortName,
                        BaudRate = serialConfiguration.BaudRate,
                        Parity = serialConfiguration.Parity,
                        DataBits = serialConfiguration.DataBits,
                        StopBits = (StopBits)serialConfiguration.StopBits
                    };

                    // 1/29 _serialPort.DataReceived += SerialPortDataReceived;

                    try
                    {
                        _serialPort.Open();
                        Thread.Sleep(100);
                        //Added 1/29
                        var buffer = new byte[256];
                        Action startListen = null;

                        var onResult = new AsyncCallback(result => OnResult(result, startListen, _serialPort, buffer));
                        _ = _logger.LogDetailAsync("ButtonOpenSerialPort_Click - Task.Run(() => consumer.Start())");
                        // Task.Run(() => _responseManager.Start());

                        startListen = () => _serialPort.BaseStream.BeginRead(buffer, 0, buffer.Length, onResult, null);
                        _ = _logger.LogDetailAsync("ButtonOpenSerialPort_Click - StartListen() ");

                        startListen();
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show($"Error Initializing Serial Port. {ex.Message}");

                        _ = _logger.LogDetailAsync($"OnResult IOException : {ex.Message} {Environment.NewLine} {ex.InnerException?.Message}");
                        _serialPort.Close();
                        ReceivedBlockingCollection.CompleteAdding();
                    }


                    if (_serialPort.IsOpen)
                    {
                        SetTransmit(true);
                        Task.Factory.StartNew(StartTransmission, CancellationToken.None, TaskCreationOptions.None,
                            TaskScheduler.Default);

                        Task.Factory.StartNew(() => _responseManager.StartResponseProcessor(), CancellationToken.None,
                            TaskCreationOptions.None, TaskScheduler.Default);

                        Task.Factory.StartNew(() => _responseManager.Start(), CancellationToken.None,
                            TaskCreationOptions.None, TaskScheduler.Default);

                        //_transmitterSending = new Thread(StartTransmission) {Name = "TransmitterSending"};
                        //_transmitterSending.Start();
                        //var t = Task.Factory.StartNew(state =>
                        //{
                        //    while (_transmit)
                        //    {
                        //        if (_serialPort.IsOpen)
                        //        {
                        //            try
                        //            {
                        //                foreach (var broadcast in RequestBlockingCollection.GetConsumingEnumerable(Token.Token))
                        //                {
                        //                    if (Token.IsCancellationRequested)
                        //                    {
                        //                        return;
                        //                    }

                        //                    _ = _logger.LogDetailAsync(
                        //                        $"Start Transmission - RequestBlockingCollection Loop: {broadcast.ByteArrayToStringX2()}");
                        //                    _serialPort.Write(broadcast, 0, broadcast.Length);
                        //                }
                        //            }
                        //            catch (OperationCanceledException)
                        //            {
                        //                return;
                        //            }


                        //            //while (RequestBlockingCollection.TryTake(out request, 100))
                        //            //{
                        //            //    _ = _logger.LogDetailAsync(
                        //            //        $"Start Transmission - RequestBlockingCollection Loop: {request.ByteArrayToStringX2()}");
                        //            //    //UpdateTextBox($"Start Transmitting: {request.ByteArrayToStringX2()}");
                        //            //    _serialPort.Write(request, 0, request.Length);
                        //            //    Thread.Sleep(50);
                        //            //}
                        //        }

                        //        Thread.Sleep(50);
                        //    }
                        //}, "TaskSubscribe", TaskCreationOptions.None);

                        //_responseProcessor = new Thread(_responseManager.StartResponseProcessor)
                        //{ Name = "StartResponseProcessor" };
                        //_responseProcessor.Start();

                        //    Task.Run( () => { _responseManager.StartResponseProcessor(); }, Token);
                        //}






                        //Mediator.GetInstance().OnTransmitStateChanged(this, true);
                        //t.Wait();
                    }
                    else
                    {
                        SetTransmit(false);
                        //Mediator.GetInstance().OnTransmitStateChanged(this, false);
                    }
                }
                else
                {
                    MessageBox.Show("Serial Configuration not set in Serial Configuration.");
                }
            }
            else
            {
                MessageBox.Show("Display device not found in Hardware Devices.");
            }

            //_ = _logger.LogDetailAsync($"OnResult - startListen()");
            //startListen();
        }
        private void OnResult(IAsyncResult result, Action startListen, SerialPort serialPort, byte[] buffer)
        {
            try
            {
                if (!serialPort.IsOpen) return;

                _responseManager.Transmitting = false;
               
                var actualLength = _serialPort.BaseStream.EndRead(result);
                var received = new byte[actualLength];
                Buffer.BlockCopy(buffer, 0, received, 0, actualLength);
                _ = _logger.LogDetailAsync($"OnResult: {received.ByteArrayToStringX2()}");
                 Mediator.GetInstance().OnSerialPortWrite(this, $"OnResult False  {received.ByteArrayToStringX2()}");
                ReceivedBlockingCollection.Add(received);
            }
            catch (IOException ex)
            {
                _ = _logger.LogDetailAsync($"OnResult IOException : {ex.Message} {Environment.NewLine} {ex.InnerException?.Message}");

                Debug.Print($@"IO Exception: {ex.Message}");
                _serialPort.Close();
                ReceivedBlockingCollection.CompleteAdding();
                return;
            }

            _ = _logger.LogDetailAsync("OnResult - startListen()");
            startListen();
        }
        private bool ValidatePort(string port)
        {
            var result = port.Substring(0, 3) == "COM";
            return result;
        }
        private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //var sb = new StringBuilder();
            //var holdText = "";
            //var existing = "";

            //do
            //{
            //    existing = _serialPort.ReadExisting();
            //    holdText += existing;
            //    Thread.Sleep(20);
            //} while (existing != string.Empty);

            ////var len = holdText.Length;
            //SendData(holdText.Substring(1, 6) + Models.Global.ACK);

            //if (holdText.Length == 12 && holdText.Contains(Models.Global.ACK))
            //{
            //    var success = holdText.Substring(7, 1);
            //    SendData(holdText.Substring(1, 6) + Models.Global.ACK);
            //}


            //if (holdText.Length < 10 || holdText.Contains(Models.Global.ACK)) return;
            //var args = new MyDataReceivedEventArgs {FormText = holdText};
            //OnMyDataReceived(args);
        }
        protected virtual void OnMyDataReceived(MyDataReceivedEventArgs args)
        {
            MyDataReceived?.Invoke(this, args);
        }
        public void SendData(string baseCommand)
        {
            _ = _logger.LogDetailAsync($"IPTI Controller - SendData - Start");
            try
            {
                var portOpen = false;
                // Thread.Sleep(100);
                if (!_serialPort.IsOpen)
                {
                    _ = _logger.LogDetailAsync($"IPTI Controller - SendData - Serial Port Is Not Open");
                    if (ConnectToComPort())
                    {
                        _ = _logger.LogDetailAsync($"IPTI Controller - SendData - Connect to ComPort is TRUE");
                        portOpen = true;
                    }
                    else
                    {
                        _ = _logger.LogDetailAsync($"IPTI Controller - SendData - Com Port Error");
                        MessageBox.Show("Com Port Error");
                    }
                }
                else
                {
                    portOpen = true;
                }
                _ = _logger.LogDetailAsync($"IPTI Controller - SendData - portOpen is {portOpen}");
                if (portOpen)
                {
                    _ = _logger.LogDetailAsync($"IPTI Controller - SendData - portOpen is Open/true");
                    var command = Models.Global.SOH + baseCommand + ToHex(baseCommand) + Models.Global.ETX;
                    _ = _logger.LogDetailAsync($"IPTI Controller - SendData - Command: {command}");
                    var bytes = command.StringToByteArray();
                    _ = _logger.LogDetailAsync($"IPTI Controller - SendData - Add Command to RequestBlockingCollection");
                    RequestBlockingCollection.TryAdd(bytes);
                    Mediator.GetInstance().OnSerialPortWrite(this, "Command to Queue - " + command);
                    //_serialPort.Write(command);
                    //Mediator.GetInstance().OnSerialPortWrite(this, bytes.ByteArrayToHumanString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"Send Data Error. {ex.Message} ");
            }
        }
        private string ToHex(string value)
        {
            var values = value.ToCharArray();
            var total = values.Select(c => (int)c).Select(decValue => (int)(decimal)decValue).Sum();
            var hex = @"00" + total.ToString("X");
            return hex.Substring(hex.Length - 2, 2);
        }
        private bool ConnectToComPort()
        {
            if (_serialPort.IsOpen) _serialPort.Close();

            try
            {
                _serialPort.Open();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + @"Com Port Connection Error.");
            }

            return false;
        }
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

    }
}