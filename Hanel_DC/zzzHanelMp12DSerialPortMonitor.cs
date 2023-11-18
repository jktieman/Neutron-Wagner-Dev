using Hanel_DC.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AlliedLogger;
using System.Windows.Forms;
using RJCP.IO.Ports;
using Hanel_DC.Hanel_DeviceControllers;
using System.Collections;

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
        private readonly List<HanelDeviceStatusType> _currentTrayStatusList;
        private readonly IDynamicLogger _logger;

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
        public bool CancelPolling = true;

        public HanelMp12DSerialPortMonitor(string comPort
            , int baudRate
            , int dataBits
            , Parity parity
            , StopBits stopBits
            , ref string cError
            , ref List<HanelDeviceStatusType> currentTrayStatusList
            , IDynamicLogger logger)
        {
            _comPort = comPort;
            _baudRate = baudRate;
            _parity = parity;
            _dataBits = dataBits;
            _stopBits = stopBits;
            _cError = cError;
            _currentTrayStatusList = currentTrayStatusList;
            var folderName = "SerialPortData";
            var logActivity = "true";
            var dir = new FileInfo(logger.FilePath);
            _lastMessageSent = new byte[] { };
            _logger = new DynamicLogger(dir.DirectoryName, folderName, logActivity);
            _logger.LogDetailAsync("Hanel MP12D Serial Port Monitor Startup");
            InitSerialPort();

        }


        private void InitSerialPort()
        {
            _serialPort = new SerialPortStream(_comPort, _baudRate, _dataBits, _parity, _stopBits)
            {
                WriteTimeout = 500
            };

            for (var i = 0; i <= 60; i++)
            {
                try
                {
                    _serialPort.Open();
                    if (_serialPort.IsOpen)
                    {
                        _logger.LogDetailAsync("Startup Success");
                        _readMp12DThread = new Thread(ReadMp12D);
                        RaiseSerialDataEvent += ProcessMp12DData;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        _serialPort.Close();
                    }
                    catch (Exception e)
                    {
                        _logger.LogDetailAsync($"Close Exception: {e.Message}");
                    }
                    Thread.Sleep(500);

                    _cError = $"SerialPort Open Exception: {ex.Message}";
                    _logger.LogDetailAsync($"Startup Fail: {Environment.NewLine} {_cError}");
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
                            ShowData($"---------------------Complete MP12D Response------------------------");
                            _logger.LogDetailAsync($"MP12D Response: {_dataIn.ByteArrayToHexString()}");

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
                            _logger.LogDetailAsync("04 Error Message: {ex.Message}");
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
            Console.WriteLine($"Neutron: {message}");
        }
        public void SendData(byte[] message)
        {
            try
            {
                if (_lastMessageSent.Length == 0)
                {
                    _lastMessageSent = message;
                }
                _logger.LogDetailAsync($"Send to Hanel: {message.ByteArrayToHexString()}");
                ShowData(message.ByteArrayToString());
                if (IsPortOpen)
                {
                    _serialPort.Write(message, 0, message.Length);
                }
                else
                {
                    _logger.LogDetailAsync("Serial Port is Closed.");
                }

            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"SendData Error: {ex.Message}");
                throw;
            }

        }

        public void Start()
        {
            _readMp12DThread.Start();
        }

        public void Stop()
        {
            try
            {
                _logger.LogDetailAsync("Begin");
                _continue = false;
                CancelPolling = true;
                _readMp12DThread.Abort();
                _serialPort.Close();
                _serialPort.Dispose();

                _logger.LogDetailAsync("End");
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Exception: {ex.Message}");

            }
        }

        public void ShowText(int nAccess, string displayText, ref string cError)
        {
            var lift = nAccess.ToString().PadLeft(2, '0');
            var ap = "1";
            var response = $"{AST}G{lift}{ap}$M XO$X01001PRESS RETURN$X03010DO IT NOW${CR}{LF}";
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
            var arr = System.Text.Encoding.UTF8.GetString(dataIn.ToArray());

            // ShowData($"DataIn to Process: {arr}");
            _logger.LogDetailAsync($"DataIn to Process: {arr}");
            var sb = new StringBuilder();
            string response;

            if (arr.Contains("BE"))
            {
                _logger.LogDetailAsync($"Contains BE");
                //_cancelPolling = true;
                //response = $"{AST}{CR}{LF}";
                //SendData(response.StringToByteArray());
            }

            if (arr.Contains("XS"))
            {
                var lift = arr.Substring(2, 2);
                var ap = arr.Substring(4, 1);
                _logger.LogDetailAsync($"Contains XS");
                if (arr.Contains("E02"))
                {
                    _logger.LogDetailAsync($"Contains E02");
                    SendData(_lastMessageSent);
                }
                if (arr.Contains("E00"))
                {
                    _logger.LogDetailAsync($"Contains E00");
                    CancelPolling = false;
                    // Task.Run(StartPollingAsync);
                    var lft = int.Parse(lift);
                    var status = _currentTrayStatusList.FirstOrDefault(r => r.Device == lft);

                    if (status != null)
                    {
                        status.CommandAccepted = true;
                        status.StatusMessage = $"Hello World, you're wonderful!!!";
                    }
                    //response = $"{AST}{CR}{LF}";
                    //SendData(response.StringToByteArray());
                }
            }

            if (arr.Contains("XA"))
            {
                var commandSegments = arr.Split('$');
                var lift = commandSegments[0].Substring(2, 2);
                var accessPoint = commandSegments[0].Substring(4, 1);


                _logger.LogDetailAsync($"Contains XA");
                _lastMessageSent = new byte[] { };
                CancelPolling = true;
                var lft = int.Parse(lift);
                var status = _currentTrayStatusList.FirstOrDefault(r => r.Device == lft);
                if (status != null)
                {
                    status.CommandAccepted = true;
                    status.StatusMessage = "Yipee";
                    // no need to set CommandExecuted to true
                    // when you set CommandAccepted to true and reach this 
                    // line in the program, you know it has been executed
                    // and it's time for a new command.

                    if (arr.Contains("A12"))
                    {
                        var result = commandSegments.FirstOrDefault(r => r.StartsWith("E"));
                        var eValue = result == null || result.Length <= 1 ? "00" : result.Substring(1);
                        _logger.LogDetailAsync(
                            $"A12 Query Value:{eValue}  Status-Current Tray:{status.CurrentTray} InMotion={status.InMotion} InAlignment={status.InAlignment}");

                        switch (eValue)
                        {
                            // Command was executed, tray number in access point returned
                            case "00":
                                {
                                    result = commandSegments.FirstOrDefault(r => r.StartsWith("T"));
                                    var tray = string.IsNullOrEmpty(result) ? "000" : result.Substring(1);

                                    if (tray == "000")
                                    {
                                        status.CurrentTray = 0;
                                        status.InMotion = false;
                                        status.InAlignment = true;
                                    }
                                    else
                                    {
                                        status.CurrentTray = int.Parse(tray);
                                        status.InMotion = false;
                                        status.InAlignment = true;
                                    }
                                    break;
                                }
                            // No shelf in the access point
                            case "01":
                                {
                                    status.CurrentTray = 0;
                                    status.InMotion = false;
                                    status.InAlignment = true;
                                    _logger.LogDetailAsync(
                                        $"Query Result:01  Status-Current Tray:0 InMotion=false InAlignment=true");
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                        _logger.LogDetailAsync($"A12 Query Result:{eValue}  Status-Current Tray:{status.CurrentTray}" +
                                               $" InMotion={status.InMotion} InAlignment={status.InAlignment}");

                    }
                }
                // do nothing
            }
        }

        public async Task StartPollingAsync()
        {
            // Task.Run(() =>
            // {
            _logger.LogDetailAsync($"Polling Started");
            while (true)
            {
                _logger.LogDetailAsync($"Neutron Polling : Start While Loop");
                if (CancelPolling) break;
                var response = $"{AST}{CR}{LF}";
                SendData(response.StringToByteArray());
                await Task.Delay(1000);
                _logger.LogDetailAsync($"Neutron Polling : {response}");
            }

            // });
        }

        public void Dispose()
        {
            _logger.LogDetailAsync($"Dispose of SerialPort Begin");

            _serialPort?.Dispose();

            _logger.LogDetailAsync($"Dispose of SerialPort End");

        }
    }
}
