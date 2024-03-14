using Hanel_DC.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AlliedLogger;
using System.Windows.Forms;
using Hanel_DC.EventArgs;
using RJCP.IO.Ports;
using HanelCommands;
using HanelCommands.Builders;
using NeutronEvents;

namespace Hanel_DC
{
    public class HanelMp12DSerialPortMonitor : IDisposable
    {
        private readonly string _comPort;
        private readonly int _baudRate;
        private readonly Parity _parity;
        private readonly int _dataBits;
        private readonly StopBits _stopBits;
        private string _cError;
        private List<HanelDeviceStatus> _currentHanelDeviceStatusList;
        private IDynamicLogger _logger;

        public static char ZERO = Convert.ToChar(30);
        public static char SOH = Convert.ToChar(1);
        public static char ETX = Convert.ToChar(3);
        public static char ACK = Convert.ToChar(6);
        public static char THREE = Convert.ToChar(33);
        public static char FOUR = Convert.ToChar(34);
        public static char CR = Convert.ToChar(13);
        public static char LF = Convert.ToChar(10);
        public static char AST = Convert.ToChar(42);


        private SerialPortStream _serialPort;

        private byte[] _dataIn = new byte[] { };
        private bool _continue = true;
        private Thread _readMp12DThread;
        private StringComparer _stringComparer = StringComparer.OrdinalIgnoreCase;
        public event EventHandler<byte[]> RaiseSerialDataEvent;
        private byte[] _lastMessageSent;
        public bool CancelPolling = false;
        private HanelCommandService _hanelCommandService;
        private bool _pollingActive;

        public event EventHandler<SerialPortInfoEventArgs> SerialPortInfoHandler;
        public event EventHandler<HanelDeviceStatusEventArgs> HanelDeviceStatusHandler;

        public HanelMp12DSerialPortMonitor(string comPort
            , int baudRate
            , int dataBits
            , Parity parity
            , StopBits stopBits
            , ref string cError
            , ref List<HanelDeviceStatus> currentHanelDeviceStatusList)
        {
            _comPort = comPort;
            _baudRate = baudRate;
            _parity = parity;
            _dataBits = dataBits;
            _stopBits = stopBits;
            _cError = cError;
            _currentHanelDeviceStatusList = currentHanelDeviceStatusList;
            _lastMessageSent = new byte[] { };

            Init();
        }

        private void Init()
        {
            _logger = NeutronCore.Global.Logger.SetupLogger("SerialPortData");
            _ = _logger.LogDetailAsync("Hanel MP12D Serial Port Monitor Startup");
            InitSerialPort();
            _hanelCommandService = new HanelCommandService(_currentHanelDeviceStatusList.Count, _currentHanelDeviceStatusList);
            Task.Run(StartPollingAsync);
        }


        protected virtual void OnSerialPortInfo(SerialPortInfoEventArgs e)
        {
            var handler = SerialPortInfoHandler;
            handler?.Invoke(this, e);

        }

        protected virtual void OnHanelDeviceStatus(HanelDeviceStatusEventArgs e)
        {
            var handler = HanelDeviceStatusHandler;
            handler?.Invoke(this, e);
        }

        private void InitSerialPort()
        {
            _serialPort = new SerialPortStream(_comPort, _baudRate, _dataBits, RJCP.IO.Ports.Parity.None, RJCP.IO.Ports.StopBits.One)
            {
                WriteTimeout = 200
            };

            for (var i = 0; i <= 60; i++)
            {
                try
                {
                    _serialPort?.Open();
                    if (_serialPort != null && _serialPort.IsOpen)
                    {
                        _ = _logger.LogDetailAsync("Startup Success");
                        ShowData("Startup Success");
                        _readMp12DThread = new Thread(ReadMp12D);
                        RaiseSerialDataEvent += ProcessMp12DData;

                        break;
                    }
                }
                catch (Exception ex)
                {
                    _ = _logger.LogDetailAsync($"Open Serial Port Exception: {ex.Message}");
                    try
                    {
                        _serialPort?.Close();
                    }
                    catch (Exception e)
                    {
                        _ = _logger.LogDetailAsync($"Close Exception: {e.Message}");
                    }

                    Thread.Sleep(500);

                    _cError = $"TRY: {i}  :SerialPort Open Exception: {ex.Message}";
                    _ = _logger.LogDetailAsync($"Startup Fail: {Environment.NewLine} {_cError}");
                }
            }

            if (!IsPortOpen)
            {
                MessageBox.Show($"Port is NOT Open.");
            }
        }

        public bool IsPortOpen => _serialPort?.IsOpen ?? false;

        private void ProcessMp12DData(object sender, byte[] dataIn)
        {
            if (dataIn == null || dataIn.Length == 0) return;

            if (dataIn.Last() == LF)
            {
                var concat = new byte[_dataIn.Length + dataIn.Length];
                Buffer.BlockCopy(_dataIn, 0, concat, 0, _dataIn.Length);
                Buffer.BlockCopy(dataIn, 0, concat, _dataIn.Length, dataIn.Length);
                _dataIn = concat;

                if (_dataIn.Length >= 3)
                {
                    if (_dataIn.First() == 42 && _dataIn.Last() == 10)
                    {
                        try
                        {
                           // ShowData($"---------------------Complete MP12D Response------------------------");
                            _ = _logger.LogDetailAsync($"MP12D Response: {_dataIn.ByteArrayToHexString()}");

                            ProcessDataIn(_dataIn);
                            //  var response = ProcessResponse(_dataIn);
                            //  ShowData($"Response: {response}");

                            //  SendData(Encoding.UTF8.GetBytes(response));

                            //ShowData($"04 Mp12D Data: {_dataIn.ToArray().ByteArrayToHexString()}");
                            //var commandToPass = _dataIn.GetRange(0, 4).ToArray().ByteArrayToString();
                            //var textToPass = _dataIn.GetRange(4, 4).ToArray().ByteArrayToString();
                            //ShowData($"04 Command To Pass: {commandToPass}  Text To Pass: {textToPass}");
                            //var cmd1 = BuildCommand(commandToPass, textToPass);
                            //ShowData($"04 Built Command: {cmd1}");
                            //cmdBytes = Encoding.UTF8.GetBytes(cmd1);
                            //_portIpti.Write(cmdBytes, 0, cmdBytes.Length);
                        }
                        catch (Exception ex)
                        {
                            ShowData($"04 Error Message: {ex.Message}");
                            _ = _logger.LogDetailAsync("04 Error Message: {ex.Message}");
                        }
                    }

                    //if (_dataIn[0] == 48 && _dataIn[1] == 51)
                    //{
                    //    ShowData($"3---------------------------------------------------------3");
                    //    //var data = Encoding.UTF8.GetString(_dataIn.ToArray());
                    //    ShowData($"03 Mp12D Data: {_dataIn.ToArray().ByteArrayToHexString()}");
                    //    var cmd3 = BuildCommand("03");
                    //    ShowData($"03 Built Command: {cmd3}");
                    //    cmdBytes = Encoding.UTF8.GetBytes(cmd3);
                    //    ;
                    //    _portIpti.Write(cmdBytes, 0, cmdBytes.Length);

                    //    if (_dataIn.Count > 3)
                    //    {
                    //        ShowData($"3------------------------34-------------------------------4");
                    //        var newData = _dataIn.GetRange(3, _dataIn.Count - 3);
                    //        if (newData.Count == 9 && newData[0] == 48 && newData[1] == 52)
                    //        {
                    //            ShowData($"34 New Data: {newData.ToArray().ByteArrayToHexString()}");
                    //            var commandToPass = newData.GetRange(0, 4).ToArray().ByteArrayToString();
                    //            var textToPass = newData.GetRange(4, 4).ToArray().ByteArrayToString();
                    //            var cmd4 = BuildCommand(commandToPass, textToPass);
                    //            ShowData($"34 Built Command: {cmd4}");
                    //            cmdBytes = Encoding.UTF8.GetBytes(cmd4);
                    //            _portIpti.Write(cmdBytes, 0, cmdBytes.Length);
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    //var data = Encoding.ASCII.GetString(_dataIn.ToArray());
                    //    ShowData($"ELSE CLEAR Data: {_dataIn.ToArray().ByteArrayToStringX2()}");
                    //    _dataIn.Clear();
                    //}

                    _dataIn = new byte[] { };
                }

                _dataIn = new byte[] { };
            }
            else
            {
                byte[] concat = new byte[_dataIn.Length + dataIn.Length];
                Buffer.BlockCopy(_dataIn, 0, concat, 0, _dataIn.Length);
                Buffer.BlockCopy(dataIn, 0, concat, _dataIn.Length, dataIn.Length);

                _dataIn = concat;
                // _logger.LogDetailAsync($"MP12D Accumulator: {_dataIn.ByteArrayToHexString()}");
                // _dataIn.AddRange(dataIn);
                // ShowData(
                //     $"Accumulating Incoming Command:{Environment.NewLine}{_dataIn.ToArray().ByteArrayToString()}{Environment.NewLine}");
            }
        }

        public void ReadMp12D()
        {
            try
            {
                byte[] buffer = new byte[4096];
                Action kickoffReadMp12D = null;
                kickoffReadMp12D = delegate
                {
                    _serialPort.BeginRead(buffer, 0, buffer.Length, delegate (IAsyncResult ar)
                    {
                        try
                        {
                            int actualLength = _serialPort.EndRead(ar);
                            byte[] received = new byte[actualLength];
                            Buffer.BlockCopy(buffer, 0, received, 0, actualLength);
                            RaiseSerialDataEvent?.Invoke(this, received);
                        }
                        catch (IOException ex)
                        {
                            ShowData($"ReadMp12D:  {ex.Message}");
                        }

                        kickoffReadMp12D();
                    }, null);
                };
                kickoffReadMp12D();
            }
            catch (ThreadAbortException ex)
            {
                Console.WriteLine("Thread is aborted and the code is "
                                  + ex.ExceptionState);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Invalid Operation Exception: {ex.Message}");
            }
        }

        private void ShowData(string message)
        {
            Mediator.GetInstance().OnWorkItMessageChng(new WorkItEventArgs(message));



            //var e = new SerialPortInfoEventArgs(serialPortInfo: message);

            //OnSerialPortInfo(e);
        }

        public bool SendData(string message)
        {
            return SendData(message.StringToByteArray());
        }

        public bool SendData(byte[] message)
        {
            var result = false;
            try
            {
                if (_lastMessageSent.Length == 0)
                {
                    _lastMessageSent = message;
                }
                _ = _logger.LogDetailAsync($"Send to Hanel: {message.ByteArrayToHexString()}");
                ShowData(message.ByteArrayToString());
                if (IsPortOpen)
                {
                    _serialPort.Write(message, 0, message.Length);
                    return true;
                }
                else
                {
                    _ = _logger.LogDetailAsync("Serial Port is Closed.");
                }

            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"SendData Error: {ex.Message}");
                throw;
            }
            return result;
        }

        public void Start()
        {
            _readMp12DThread?.Start();
        }

        public void Stop()
        {
            try
            {
                _ = _logger.LogDetailAsync("Begin Stop");
                _continue = false;
                CancelPolling = true;
                _readMp12DThread?.Abort();
                _serialPort?.Close();

                _ = _logger.LogDetailAsync("End Stop");
                ShowData("End Stop");
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"Exception: {ex.Message}");

            }
        }

        public void ShowText(int nAccess, string displayText, ref string cError)
        {
            var lift = nAccess.ToString().PadLeft(2, '0');
            var accessPoint = "1";
            var response = $"{AST}G{lift}{accessPoint}$M XO$X01001PRESS RETURN$X03010DO IT NOW${CR}{LF}";
            SendData(response.StringToByteArray());
        }

        /// <summary>
        /// Based on the input from the MP12D
        /// What is Neutron's response back
        /// </summary>
        /// <param name="dataIn">A Byte array of data from the MP12D</param>
        /// <returns>Returns a string response to the MP12D</returns>
        private void ProcessDataIn(byte[] dataIn)
        {
            var commandString = Encoding.UTF8.GetString(dataIn);

            var commandSegments = commandString.Split('$');

            if (commandSegments.Length <= 1) return;
            if (commandSegments.Length == 2)
            {

                if (commandString.Contains("BE"))
                {
                    _ = _logger.LogDetailAsync($"Contains BE: {commandString}");
                    //_cancelPolling = true;
                    //Thread.Sleep(500);
                    // var response = $"{AST}{CR}{LF}";
                    // SendData(response.StringToByteArray());
                    DumpStatus();
                    return;
                }
                else
                {
                    _ = _logger.LogDetailAsync($"Does NOT Contain BE: {commandString}");
                    return;
                }
            }

            // There are more than 2 commandSegments

            var lift = commandSegments[0].Substring(2, 2);
            var accessPoint = commandSegments[0].Substring(4, 1);

            var device = _currentHanelDeviceStatusList.FirstOrDefault(r => r.DeviceNumber == int.Parse(lift));

            DumpStatus();

            var hanelCommandProcessor = new HanelCommandProcessor();

            hanelCommandProcessor.Process(dataIn, _currentHanelDeviceStatusList);
            //device = _currentTrayStatusList.FirstOrDefault(r => r.DeviceNumber == int.Parse(lift));
            DumpStatus();

            _ = _logger.LogDetailAsync($"DataIn to Process: {commandString}");
            var sb = new StringBuilder();
            //var response = _hanelCommandService.GetResponse(dataIn);

            if (commandString.Contains("BE"))
            {
                _ = _logger.LogDetailAsync($"Contains BE");
                //_cancelPolling = true;
                //response = $"{AST}{CR}{LF}";
                //SendData(response.StringToByteArray());
            }
        }
        private void DumpStatus()
        {
            foreach (var deviceStatus in _currentHanelDeviceStatusList)
            {
                if (deviceStatus == null) continue;

                var sb = new StringBuilder();

                sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  DeviceNumber = {deviceStatus.DeviceNumber}");
                sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  GoodStatus = {deviceStatus.GoodStatus}");
                sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  LastStatus = {deviceStatus.LastStatus}");
                sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  LastCommand = {deviceStatus.LastCommand}");
                sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  Device = {deviceStatus.Device}");
                sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  TargetTray = {deviceStatus.TargetTray}");
                sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  CurrentTray = {deviceStatus.CurrentTray}");
                sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  InMotion = {deviceStatus.InMotion}");
                sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  InAlignment = {deviceStatus.InAlignment}");
                sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  StatusMessage = {deviceStatus.StatusMessage}");
                sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  ActiveErrorCount ={deviceStatus.ActiveErrorCount}");
                sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  CommandAccepted = {deviceStatus.CommandAccepted}");
                sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  CommandExecuted = {deviceStatus.CommandExecuted}");
                sb.AppendLine($"Device #{deviceStatus.DeviceNumber} Switched On = {deviceStatus.SwitchedOn}");
                sb.AppendLine($"--------------------------------------------------");
                sb.AppendLine();

                _logger.LogDetailAsync(sb.ToString());

                //ShowData($"{sb.ToString()}");
            }
            //ShowData($"Device #{deviceStatus.DeviceNumber} Status Message: {deviceStatus.StatusMessage}");
        }

        private void UpdateCurrentTray(string lift, string accessPoint, string tray)
        {
            if (int.TryParse(lift, out var lft))
            {
                var device = _currentHanelDeviceStatusList.FirstOrDefault(r => r.DeviceNumber == lft);
                if (device != null)
                {
                    if (int.TryParse(tray, out var t))
                    {
                        device.CurrentTray = t;
                        var args = new HanelDeviceStatusEventArgs(device);
                        OnHanelDeviceStatus(args);
                    }
                }
            }
        }

        private string GetSegmentValue(string[] commandSegments, string segmentType)
        {
            var result = commandSegments.FirstOrDefault(r => r.StartsWith(segmentType));
            return result == null || result.Length == 1 ? "0" : result.Substring(1);
        }

        public async Task StartPollingAsync()
        {
            // Task.Run(() =>
            // {
            var activePoll = false;
            _pollingActive = true;
            while (true)
            {
                //var statusList = _currentHanelDeviceStatusList.Where(r => r.LastHanelCommand != null).ToList();
                //var statusList = _hanelCommandService.HanelDeviceStatusList.Where(r => r.LastHanelCommand != null).ToList();
                var statusList = _currentHanelDeviceStatusList;

                if (statusList.Any())
                {
                    var acceptedStatus = statusList.Where(r => r.CommandAccepted && r.CommandExecuted == false).ToList();
                    var executedStatus = statusList.Where(r => r.CommandAccepted && r.CommandExecuted).ToList();
                    var readyStatus = statusList.Where(r => r.CommandAccepted == false && r.CommandExecuted == false).ToList();
                    if (executedStatus.Any())
                    {
                        var executed = true;
                        //CancelPolling = false;
                        Mediator.GetInstance().OnTrayInPosition(this, new InPositionInfo() { OneInPosition = true, TwoInPosition = false, ThreeInPosition = false });
                    }
                   
                    if (acceptedStatus.Any())
                    {
                        var accepted = true;
                        activePoll = true;
                        //CancelPolling = false;
                    }
                    //else
                    //{
                    //    //CancelPolling = true;
                    //    //break;
                    //    activePoll = false;
                    //    Mediator.GetInstance().OnTrayInPosition(this, new InPositionInfo(){OneInPosition = true, TwoInPosition = false, ThreeInPosition = false});
                    //}
                    if (readyStatus.Any())
                    {
                        var ready = true;
                    }

                }


                if (activePoll)
                {
                    await _logger.LogDetailAsync($"Active Polling Started");
                    //while (true)
                    //{
                    await _logger.LogDetailAsync($"Neutron Polling : Start While Loop ActivePoll: {activePoll}");


                    var response = $"{AST}{CR}{LF}";
                    SendData(response.StringToByteArray());
                    ShowData($"Neutron Polling : {response}");
                    //await Task.Delay(2000);
                    await _logger.LogDetailAsync($"Neutron Polling : {response}");
                    //}
                }
                await Task.Delay(10000);
                await _logger.LogDetailAsync($"Active Polling Wait 2 seconds ActivePoll : {activePoll}");

                //if (CancelPolling) break;
            }
            _pollingActive = false;

            // });
        }

        public void Dispose()
        {
            _ = _logger.LogDetailAsync($"Dispose of SerialPort Begin");

            _serialPort?.Dispose();

            _ = _logger.LogDetailAsync($"Dispose of SerialPort End");

        }
    }
}
