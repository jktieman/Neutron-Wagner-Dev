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
using AsyncAwaitBestPractices;

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
        private volatile bool _cancelPolling = false;
        private HanelCommandService _hanelCommandService;
        private volatile bool _pollingActive;

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
            _logger.LogDetailAsync("Hanel MP12D Serial Port Monitor Startup").SafeFireAndForget();
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
                        _logger.LogDetailAsync("Startup Success").SafeFireAndForget();
                        //ShowData("Startup Success");
                        _readMp12DThread = new Thread(ReadMp12D);
                        RaiseSerialDataEvent += async (sender, data) => await ProcessMp12DData(sender, data);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDetailAsync($"Open Serial Port Exception: {ex.Message}").SafeFireAndForget();
                    try
                    {
                        _serialPort?.Close();
                    }
                    catch (Exception e)
                    {
                        _logger.LogDetailAsync($"Close Exception: {e.Message}").SafeFireAndForget();
                    }

                    Thread.Sleep(500);

                    _cError = $"TRY: {i}  :SerialPort Open Exception: {ex.Message}";
                    _logger.LogDetailAsync($"Startup Fail: {Environment.NewLine} {_cError}").SafeFireAndForget();
                }
            }

            if (!IsPortOpen)
            {
                MessageBox.Show($"Port is NOT Open.");
            }
        }

        public bool IsPortOpen => _serialPort?.IsOpen ?? false;

        private async Task ProcessMp12DData(object sender, byte[] dataIn)
        {
            if (dataIn == null || dataIn.Length == 0) return;

            _logger.LogDetailAsync($"Current Data In: {Encoding.UTF8.GetString(dataIn)}").SafeFireAndForget();

            var concat = new byte[_dataIn.Length + dataIn.Length];
            Buffer.BlockCopy(_dataIn, 0, concat, 0, _dataIn.Length);
            Buffer.BlockCopy(dataIn, 0, concat, _dataIn.Length, dataIn.Length);
            _dataIn = concat;
            _logger.LogDetailAsync($"Merged Mp12D Data: {Encoding.UTF8.GetString(_dataIn)}").SafeFireAndForget();

            _dataIn = RemoveBytesBeforeAsterisk(_dataIn);
            _logger.LogDetailAsync($"Remove Bytes Before Asterisk: {Encoding.UTF8.GetString(_dataIn)}").SafeFireAndForget();

            if (_dataIn.Length >= 3)
            {
                // is there a valid command in the byte array
                var validCommand = ValidCommand(_dataIn);
                if (validCommand != null)
                {
                    try
                    {
                        _dataIn = _dataIn.Skip(validCommand.Length).ToArray();
                        // ShowData($"---------------------Complete MP12D Response------------------------");
                        _logger.LogDetailAsync($"MP12D Response: {Encoding.UTF8.GetString(_dataIn)}").SafeFireAndForget();
                        _logger.LogDetailAsync($"validCommand: {Encoding.UTF8.GetString(validCommand)}").SafeFireAndForget();

                        await ProcessDataIn(validCommand);

                    }
                    catch (Exception ex)
                    {
                        _logger.LogDetailAsync("04 Error Message: {ex.Message}").SafeFireAndForget();
                    }
                }

            }

            //_dataIn = new byte[] { };
            //}
            //else
            //{
            //    byte[] concat = new byte[_dataIn.Length + dataIn.Length];
            //    Buffer.BlockCopy(_dataIn, 0, concat, 0, _dataIn.Length);
            //    Buffer.BlockCopy(dataIn, 0, concat, _dataIn.Length, dataIn.Length);

            //    _dataIn = concat;
            //    // _logger.LogDetailAsync($"MP12D Accumulator: {_dataIn.ByteArrayToHexString()}");
            //    // _dataIn.AddRange(dataIn);
            //    // ShowData(
            //    //     $"Accumulating Incoming Command:{Environment.NewLine}{_dataIn.ToArray().ByteArrayToString()}{Environment.NewLine}");
            //}
        }

        public byte[] ValidCommand(byte[] dataIn)
        {
            byte[] byteArray = null;
            if (dataIn.Length == 0)
            {
                return null;
            }

            _logger.LogDetailAsync($"dataIn: {Encoding.UTF8.GetString(dataIn)}");
            // extract the byte array starting with 42 and ending with 10
            var startIndex = FindAsterisk(dataIn);
            _logger.LogDetailAsync($"Start Index: {startIndex}");
            if (startIndex == -1)
            {
                _logger.LogDetailAsync($"Start Index = -1 {startIndex}");
                return null;
            }
            _logger.LogDetailAsync($"Final dataIn: {Encoding.UTF8.GetString(dataIn)} ");
            startIndex = FindAsterisk(dataIn);
            _logger.LogDetailAsync($"Final dataIn Start Index: {startIndex} ");
            var endIndex = FindLineFeed(dataIn);
            _logger.LogDetailAsync($"Final dataIn End Index: {endIndex} ");
            if (endIndex == -1)
            {
                return null;
            }

            byteArray = dataIn.Skip(startIndex).Take(endIndex - startIndex + 1).ToArray();
            _logger.LogDetailAsync($"Return ByteArray: {Encoding.UTF8.GetString(byteArray)}");
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

        //private async Task ProcessMp12DData(object sender, byte[] dataIn)
        //{
        //    if (dataIn == null || dataIn.Length == 0) return;

        //    _logger.LogDetailAsync($"Process Mp12D Data: {_dataIn.ByteArrayToHexString()}").SafeFireAndForget();

        //    if (dataIn.Last() == LF)
        //    {
        //        var concat = new byte[_dataIn.Length + dataIn.Length];
        //        Buffer.BlockCopy(_dataIn, 0, concat, 0, _dataIn.Length);
        //        Buffer.BlockCopy(dataIn, 0, concat, _dataIn.Length, dataIn.Length);
        //        _dataIn = concat;

        //        if (_dataIn.Length >= 3)
        //        {
        //            if (_dataIn.First() == 42 && _dataIn.Last() == 10)
        //            {
        //                try
        //                {
        //                   // ShowData($"---------------------Complete MP12D Response------------------------");
        //                    _logger.LogDetailAsync($"MP12D Response: {_dataIn.ByteArrayToHexString()}").SafeFireAndForget();

        //                    await ProcessDataIn(_dataIn);

        //                }
        //                catch (Exception ex)
        //                {
        //                    //ShowData($"04 Error Message: {ex.Message}");
        //                    _logger.LogDetailAsync("04 Error Message: {ex.Message}").SafeFireAndForget();
        //                }
        //            }

        //            _dataIn = new byte[] { };
        //        }

        //        _dataIn = new byte[] { };
        //    }
        //    else
        //    {
        //        byte[] concat = new byte[_dataIn.Length + dataIn.Length];
        //        Buffer.BlockCopy(_dataIn, 0, concat, 0, _dataIn.Length);
        //        Buffer.BlockCopy(dataIn, 0, concat, _dataIn.Length, dataIn.Length);

        //        _dataIn = concat;
        //        // _logger.LogDetailAsync($"MP12D Accumulator: {_dataIn.ByteArrayToHexString()}");
        //        // _dataIn.AddRange(dataIn);
        //        // ShowData(
        //        //     $"Accumulating Incoming Command:{Environment.NewLine}{_dataIn.ToArray().ByteArrayToString()}{Environment.NewLine}");
        //    }
        //}

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
                            _logger.LogDetailAsync($"Error ReadMp12D:  {ex.Message}").SafeFireAndForget();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogDetailAsync($"Unexpected error in ReadMp12D: {ex.Message}").SafeFireAndForget();
                        }
                        finally
                        {
                            // Ensure the read operation is restarted
                            if (!_cancelPolling)
                            {
                                kickoffReadMp12D();
                            }
                        }
                    }, null);
                };
                kickoffReadMp12D();
            }
            catch (ThreadAbortException ex)
            {
                _logger.LogDetailAsync($"Error Thread is aborted and the code is {ex.ExceptionState}").SafeFireAndForget();
                //Thread.ResetAbort(); // Allow the thread to terminate properly
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogDetailAsync($"Error Invalid Operation Exception: {ex.Message}").SafeFireAndForget();
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Unexpected error in ReadMp12D: {ex.Message}").SafeFireAndForget();
            }
        }
        private void ShowData(string message)
        {
            // Mediator.GetInstance().OnWorkItMessageChng(new WorkItEventArgs(message));



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
                _logger.LogDetailAsync($"Send to Hanel: {Encoding.UTF8.GetString(message)}").SafeFireAndForget();
                //ShowData(message.ByteArrayToString());
                if (IsPortOpen)
                {
                    _serialPort.Write(message, 0, message.Length);
                    result = true;
                }
                else
                {
                    _logger.LogDetailAsync("Serial Port is Closed.").SafeFireAndForget();
                }

            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"SendData Error: {ex.Message}").SafeFireAndForget();
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
                _logger.LogDetailAsync("Begin Stop").SafeFireAndForget();
                _continue = false;
                _cancelPolling = true;
                _readMp12DThread?.Abort();
                _serialPort?.Close();

                _logger.LogDetailAsync("End Stop").SafeFireAndForget();
                //ShowData("End Stop");
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Exception: {ex.Message}").SafeFireAndForget();

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
        private async Task ProcessDataIn(byte[] dataIn)
        {
            var commandString = Encoding.UTF8.GetString(dataIn);
            _logger.LogDetailAsync($"PROCESS DATA IN: {commandString}").SafeFireAndForget();

            var commandSegments = commandString.Split('$');
            LogSegments(commandSegments);

            if (commandSegments.Length <= 1) return;
            if (commandSegments.Length == 2)
            {

                //if (commandString.Contains("BE"))
                //{
                _logger.LogDetailAsync($"Exactly 2 Command Segments: {commandString}").SafeFireAndForget();
                //   // await DumpStatusAsync("Odd BE Command String");
                //}
                //else
                //{
                //    _logger.LogDetailAsync($"Exactly 2 Command Segments - Does NOT Contain BE: {commandString}").SafeFireAndForget();
                //}
                return;
            }

            // There are more than 2 commandSegments

            var lift = commandSegments[0].Substring(2, 2);
            var accessPoint = commandSegments[0].Substring(4, 1);

            var deviceStatus = _currentHanelDeviceStatusList.FirstOrDefault(r => r.DeviceNumber == int.Parse(lift));

            await DumpStatusAsync("Before Command Processing", deviceStatus);

            var hanelCommandProcessor = new HanelCommandProcessor();

            hanelCommandProcessor.Process(dataIn, ref _currentHanelDeviceStatusList);

            await DumpStatusAsync("After Command Processing", deviceStatus);

            _logger.LogDetailAsync($"DataIn to Process: {commandString}").SafeFireAndForget();

            if (commandString.Contains("BE"))
            {
                _logger.LogDetailAsync($"Contains Ending BE").SafeFireAndForget();
            }
        }

        private void LogSegments(string[] commandSegments)
        {
            var sb = new StringBuilder();
            foreach (var segment in commandSegments)
            {
                sb.AppendLine(segment);
            }

            _logger.LogDetailAsync($"Command segments: {sb}").SafeFireAndForget();
        }
        private async Task DumpStatusAsync(string title, HanelDeviceStatus deviceStatus)
        {
            //foreach (var deviceStatus in _currentHanelDeviceStatusList)
            //{
            if (deviceStatus == null) return;

            var sb = new StringBuilder();
            sb.AppendLine(title);
            AppendDeviceStatus(sb, deviceStatus);
            sb.AppendLine("--------------------------------------------------");
            await _logger.LogDetailAsync(sb.ToString());
            //}
        }
        private void AppendDeviceStatus(StringBuilder sb, HanelDeviceStatus deviceStatus)
        {
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
            sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  ActiveErrorCount = {deviceStatus.ActiveErrorCount}");
            sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  CommandAccepted = {deviceStatus.CommandAccepted}");
            sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  CommandExecuted = {deviceStatus.CommandExecuted}");
            sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  Switched On = {deviceStatus.SwitchedOn}");
        }



        //private void DumpStatus()
        //{
        //    foreach (var deviceStatus in _currentHanelDeviceStatusList)
        //    {
        //        if (deviceStatus == null) continue;

        //        var sb = new StringBuilder();

        //        sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  DeviceNumber = {deviceStatus.DeviceNumber}");
        //        sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  GoodStatus = {deviceStatus.GoodStatus}");
        //        sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  LastStatus = {deviceStatus.LastStatus}");
        //        sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  LastCommand = {deviceStatus.LastCommand}");
        //        sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  Device = {deviceStatus.Device}");
        //        sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  TargetTray = {deviceStatus.TargetTray}");
        //        sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  CurrentTray = {deviceStatus.CurrentTray}");
        //        sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  InMotion = {deviceStatus.InMotion}");
        //        sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  InAlignment = {deviceStatus.InAlignment}");
        //        sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  StatusMessage = {deviceStatus.StatusMessage}");
        //        sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  ActiveErrorCount ={deviceStatus.ActiveErrorCount}");
        //        sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  CommandAccepted = {deviceStatus.CommandAccepted}");
        //        sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  CommandExecuted = {deviceStatus.CommandExecuted}");
        //        sb.AppendLine($"Device #{deviceStatus.DeviceNumber} Switched On = {deviceStatus.SwitchedOn}");
        //        sb.AppendLine($"--------------------------------------------------");
        //        sb.AppendLine();

        //        _logger.LogDetailAsync(sb.ToString()).SafeFireAndForget(); 

        //        //ShowData($"{sb.ToString()}");
        //    }
        //    //ShowData($"Device #{deviceStatus.DeviceNumber} Status Message: {deviceStatus.StatusMessage}");
        //}

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
            var activePoll = false;
            _pollingActive = true;
            try
            {
                while (!_cancelPolling)
                {
                    var statusList = _currentHanelDeviceStatusList ?? new List<HanelDeviceStatus>();
                    if (statusList.Any())
                    {
                        var acceptedStatus = statusList.Where(r => r.CommandAccepted && !r.CommandExecuted).ToList();
                        var executedStatus = statusList.Where(r => r.CommandAccepted && r.CommandExecuted).ToList();
                        var readyStatus = statusList.Where(r => !r.CommandAccepted && !r.CommandExecuted).ToList();
                        if (executedStatus.Any())
                        {
                            Mediator.GetInstance().OnTrayInPosition(this, new InPositionInfo { OneInPosition = true, TwoInPosition = false, ThreeInPosition = false });
                        }
                        if (acceptedStatus.Any())
                        {
                            activePoll = true;
                        }
                        if (readyStatus.Any())
                        {
                            // Handle ready status if needed
                        }
                    }
                    if (activePoll)
                    {
                        await _logger.LogDetailAsync("Active Polling Started").ConfigureAwait(false);
                        var response = $"{AST}{CR}{LF}";
                        SendData(response.StringToByteArray());
                        //ShowData($"Neutron Polling : {response}");
                        await _logger.LogDetailAsync($"Neutron Polling : {response}").ConfigureAwait(false);
                    }
                    await Task.Delay(500).ConfigureAwait(false);
                    //await _logger.LogDetailAsync($"Active Polling Wait 500 milliseconds ActivePoll : {activePoll}").ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await _logger.LogDetailAsync($"Exception in StartPollingAsync: {ex.Message}").ConfigureAwait(false);
            }
            finally
            {
                _pollingActive = false;
            }
        }


        //public async Task StartPollingAsync()
        //{
        //    // Task.Run(() =>
        //    // {
        //    var activePoll = false;
        //    _pollingActive = true;
        //    while (true)
        //    {
        //        //var statusList = _currentHanelDeviceStatusList.Where(r => r.LastHanelCommand != null).ToList();
        //        //var statusList = _hanelCommandService.HanelDeviceStatusList.Where(r => r.LastHanelCommand != null).ToList();
        //        var statusList = _currentHanelDeviceStatusList;

        //        if (statusList.Any())
        //        {
        //            var acceptedStatus = statusList.Where(r => r.CommandAccepted && r.CommandExecuted == false).ToList();
        //            var executedStatus = statusList.Where(r => r.CommandAccepted && r.CommandExecuted).ToList();
        //            var readyStatus = statusList.Where(r => r.CommandAccepted == false && r.CommandExecuted == false).ToList();
        //            if (executedStatus.Any())
        //            {
        //                var executed = true;
        //                //CancelPolling = false;
        //                Mediator.GetInstance().OnTrayInPosition(this, new InPositionInfo() { OneInPosition = true, TwoInPosition = false, ThreeInPosition = false });
        //            }

        //            if (acceptedStatus.Any())
        //            {
        //                var accepted = true;
        //                activePoll = true;
        //                //CancelPolling = false;
        //            }
        //            //else
        //            //{
        //            //    //CancelPolling = true;
        //            //    //break;
        //            //    activePoll = false;
        //            //    Mediator.GetInstance().OnTrayInPosition(this, new InPositionInfo(){OneInPosition = true, TwoInPosition = false, ThreeInPosition = false});
        //            //}
        //            if (readyStatus.Any())
        //            {
        //                var ready = true;
        //            }

        //        }


        //        if (activePoll)
        //        {
        //            _logger.LogDetailAsync($"Active Polling Started").SafeFireAndForget(); 
        //            //while (true)
        //            //{
        //            _logger.LogDetailAsync($"Neutron Polling : Start While Loop ActivePoll: true").SafeFireAndForget();


        //            var response = $"{AST}{CR}{LF}";
        //            SendData(response.StringToByteArray());
        //            ShowData($"Neutron Polling : {response}");
        //            //await Task.Delay(2000);
        //            _logger.LogDetailAsync($"Neutron Polling : {response}").SafeFireAndForget();
        //            //}
        //        }
        //        await Task.Delay(10000);
        //        _logger.LogDetailAsync($"Active Polling Wait 2 seconds ActivePoll : {activePoll}").SafeFireAndForget();

        //        //if (CancelPolling) break;
        //    }
        //    _pollingActive = false;

        //    // });
        //}

        public void Dispose()
        {
            _logger.LogDetailAsync($"Dispose of SerialPort Begin").SafeFireAndForget();

            _serialPort?.Dispose();

            _logger.LogDetailAsync($"Dispose of SerialPort End").SafeFireAndForget();

        }
    }
}
