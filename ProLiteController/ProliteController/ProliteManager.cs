using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using AlliedLogger;
using NeutronCore.Global;
using NeutronData.ModelViews;
using System.Threading.Tasks;
using NeutronEvents;
using AsyncAwaitBestPractices;
using System.IO;
using System.Text;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;


namespace ProliteController
{
    // create a CallType enum
    public enum CallType
    {
        ClearAll = 0,
        CycleCountShowQuantity = 1,
        CycleCountNoQuantity = 2,
    }

    /// <summary>
    /// The `ProLiteManager` class in the `ProliteController` namespace is a crucial component
    /// for managing Prolite devices in your application.It implements the `IProLiteManager` interface.
    /// This class provides functionality to interact with Prolite devices, which are a type of HardwareDevice.
    /// It allows you to add and remove Prolite devices, turn them on or off, clear them, and perform
    /// other operations.The class also maintains a list of Prolite devices for easy management.
    /// The `ProLiteManager` class is initialized with parameters such as `portName`, `baudRate`
    /// , `parity`, `dataBits`, `stopBits`, `neutronVariables`, and `workstationView`.
    /// These parameters are likely used to establish and manage a connection with the Prolite devices.
    ///   The class also includes logging capabilities, as seen with the `_logger` instance,
    /// which is used to log details about operations performed on the Prolite devices.
    ///In summary, the `ProLiteManager` class is a comprehensive manager for Prolite devices,
    /// providing a range of functionalities to interact with and control these devices.
    /// </summary>
    public class ProLiteManager : IProLiteManager
    {
        private readonly string _portName;
        private readonly int _baudRate;
        private readonly Parity _parity;
        private readonly int _dataBits;
        private readonly int _stopBits;
        private readonly NeutronVariables _neutronVariables;
        private readonly WorkstationView _workstationView;
        private IDynamicLogger _logger;
        private SerialPort _serialPort;
        public IList<Prolite> Prolites { get; private set; }
        private string _lastCommand = string.Empty;
        public bool ProliteManagerEnabled = true;
        private bool _proliteBusy = false;

        private readonly BlockingCollection<string> _proliteCommandQueue = new BlockingCollection<string>();
        private static BackgroundWorker _proliteCommandQueueProcessor;
        private bool _proliteBusyClearing;
        private string _currentCommand;

        /// <summary>
        /// Takes a string and returns the corresponding Parity enum value.
        /// </summary>
        /// <param name="parity"></param>
        /// <returns></returns>
        private Parity GetParity(string parity)
        {
            switch (parity)
            {
                case "Even":
                    return Parity.Even;
                case "Mark":
                    return Parity.Mark;
                case "None":
                    return Parity.None;
                case "Odd":
                    return Parity.Odd;
                case "Space":
                    return Parity.Space;
                default:
                    return Parity.None;
            }
        }
        /// <summary>
        /// Takes an integer and returns the corresponding StopBits enum value.
        /// </summary>
        /// <param name="stopBits"></param>
        /// <returns></returns>
        private StopBits GetStopBits()
        {
            switch (_stopBits)
            {
                case 1:
                    return StopBits.One;
                case 2:
                    return StopBits.Two;
                case 3:
                    return StopBits.OnePointFive;
                default:
                    return StopBits.One;
            }
        }

        public ProLiteManager(string portName, int baudRate, Parity parity, int dataBits, int stopBits,
            NeutronVariables neutronVariables, WorkstationView workstationView)
        {
            _portName = portName;
            _baudRate = baudRate;
            _parity = parity;
            _dataBits = dataBits;
            _stopBits = stopBits;

            _neutronVariables = neutronVariables;
            _workstationView = workstationView;
            Prolites = new List<Prolite>();
            _logger = NeutronCore.Global.Logger.SetupLogger("ProLiteManager");

        }

        public async Task StartProcessingCommands()
        {
            await InitSerialPort();
            InitBackgroundWorker();
            _proliteCommandQueueProcessor.RunWorkerAsync();
        }

        private void StopProcessingCommands()
        {
            StopBackgroundWorker();
        }

        private void StopBackgroundWorker()
        {
            _proliteCommandQueueProcessor.CancelAsync();
        }

        private void InitBackgroundWorker()
        {
            _proliteCommandQueueProcessor = new BackgroundWorker
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = true
            };
            _proliteCommandQueueProcessor.DoWork += ProliteCommandQueueProcessorDoWork;
            _proliteCommandQueueProcessor.RunWorkerCompleted += ProliteCommandQueueProcessorRunWorkerCompleted;
        }

        private void ProliteCommandQueueProcessorDoWork(object sender, DoWorkEventArgs e)
        {
            var counter = 0;
            //_logger.LogDetailAsync($"Command Queue Processing DoWork").SafeFireAndForget();
            if (_proliteCommandQueueProcessor.CancellationPending)
            {
                e.Cancel = true;
            }

            while (!_proliteCommandQueueProcessor.CancellationPending)
            {
                var busy = _proliteBusy;
                Thread.Sleep(millisecondsTimeout: 100);
                foreach (var command in _proliteCommandQueue.GetConsumingEnumerable())
                {
                    _currentCommand = command;
                    busy = _proliteBusy;
                    while (_proliteBusy)
                    {
                        Thread.Sleep(100);
                        counter += 100;
                        if (counter >= 10000) break;
                        _logger.LogDetailAsync($"DoWork Waiting, Prolite is Busy Current Wait Time: {counter}").SafeFireAndForget();
                    }
                    _proliteBusy = true;
                    SerialPortWrite(command);

                    _logger.LogDetailAsync($"Command Queue Processing: {command} ProliteBusy = {_proliteBusy}").SafeFireAndForget();
                }
            }

        }

        private void ProliteCommandQueueProcessorRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {

            }
            else
            {
                var result = e.Result;
            }
        }



        private async Task InitSerialPort()
        {
            _logger.LogDetailAsync($"Initializing Serial Port").SafeFireAndForget();
            var stopBits = GetStopBits();
            _serialPort = new SerialPort(_portName, _baudRate, _parity, _dataBits, stopBits)
            {
                WriteTimeout = 200
            };
            if (_serialPort == null) return;
            _serialPort.DataReceived += SerialPortOnDataReceived;
            await TryOpenSerialPort();
            if (!IsPortOpen)
            {
                var message = $"Prolite Port is NOT Open. Disabling All Prolites.";
                _logger.LogDetailAsync(message).SafeFireAndForget();
                Mediator.GetInstance().OnGeneralError(this, message);
                ProliteManagerEnabled = false;
            }
        }
        private async Task TryOpenSerialPort()
        {
            for (var i = 0; i <= 6; i++)
            {
                try
                {
                    _serialPort?.Open();
                    if (_serialPort != null && _serialPort.IsOpen)
                    {
                        _logger.LogDetailAsync("Startup Success").SafeFireAndForget();
                        ProliteManagerEnabled = true;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    await HandleSerialPortExceptionAsync(i, ex);
                    await Task.Delay(500);
                }
            }
        }


        //private async Task InitSerialPort()
        //{
        //    _logger.LogDetailAsync($"Init Serial Port").SafeFireAndForget();
        //    var stopBits = GetStopBits();
        //    _serialPort = new SerialPort(_portName, _baudRate, _parity, _dataBits, stopBits)
        //    {
        //        WriteTimeout = 200
        //    };
        //    if (_serialPort == null) return;
        //    _serialPort.DataReceived += SerialPortOnDataReceived;
        //    for (var i = 0; i <= 6; i++)
        //    {
        //        try
        //        {
        //            _serialPort?.Open();
        //            if (_serialPort != null && _serialPort.IsOpen)
        //            {
        //                _logger.LogDetailAsync("Startup Success").SafeFireAndForget();
        //                ProliteManagerEnabled = true;
        //                break;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            await HandleSerialPortExceptionAsync(i, ex);
        //            await Task.Delay(500);
        //        }
        //    }
        //    if (!IsPortOpen)
        //    {
        //        var message = $"Prolite Port is NOT Open  Disable All Prolites.";
        //        _logger.LogDetailAsync(message).SafeFireAndForget();
        //        Mediator.GetInstance().OnGeneralError(this, message);
        //        ProliteManagerEnabled = false;
        //    }
        //}


        /// <summary>
        /// Add a ProLite device to the list of Prolites.
        /// A Prolite is a HardwareDevice of type Prolite.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="deviceNumber"></param>
        /// <param name="enabled"></param>
        public void AddProlite(int id, string name, int deviceNumber, bool enabled)
        {
            _logger.LogDetailAsync($"Add Prolite Id: {deviceNumber} - {name}").SafeFireAndForget();
            try
            {
                var pro = Prolites.FirstOrDefault(p => p.DeviceNumber == deviceNumber);
                if (pro is null)
                {
                    var prolite = new Prolite(id, name, deviceNumber, ProliteManagerEnabled);

                    Prolites.Add(prolite);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Add Prolite Error: {ex.Message}").SafeFireAndForget();
            }
        }
        public void RemoveProlite(int deviceNumber)
        {
            _logger.LogDetailAsync($"Remove Prolite: {deviceNumber}").SafeFireAndForget();
            try
            {
                var prolite = Prolites.FirstOrDefault(p => p.DeviceNumber == deviceNumber);

                if (prolite != null)
                {
                    Prolites.Remove(prolite);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Remove Prolite Error: {ex.Message}").SafeFireAndForget();
            }
        }

        private void SerialPortWrite(string cmd)
        {
            var hexCmd = BitConverter.ToString(Encoding.Default.GetBytes(cmd));
            _logger.LogDetailAsync($"Serial Port Write HEX => [ {hexCmd} ]").SafeFireAndForget();
            // CHECK TO SEE IF SERIALPORT IS OPEN
            if (!_serialPort.IsOpen)
            {
                var message = $"Serial Port is NOT Open. Cannot write command: {cmd}";
                _logger.LogDetailAsync(message).SafeFireAndForget();
                Mediator.GetInstance().OnGeneralError(this, message);
                return;
            }
            _serialPort.Write(cmd);
            //_logger.LogDetailAsync($"End: {cmd}").SafeFireAndForget();
        }
        public void TurnOn(int deviceNumber, int level, int part, int quantity)
        {
            _logger.LogDetailAsync($"Turn ON Prolite Device: {deviceNumber} Level: {level}  Part: {part}  Quantity: {quantity} ProliteBusyClearing: {_proliteBusyClearing}").SafeFireAndForget();

            var counter = 0;
            while (_proliteBusyClearing)
            {
                Thread.Sleep(100);
                counter += 100;
                if (counter >= 10000) break;
                _logger.LogDetailAsync($"Prolite is Busy Clearing Current Wait Time: {counter}").SafeFireAndForget();
            }

            try
            {
                var prolite = Prolites.FirstOrDefault(p => p.DeviceNumber == deviceNumber);
                if (prolite == null) return;
                if (prolite.Enabled == false) return;

                var cmd = prolite.TurnOn(level, part, quantity);
                _lastCommand = cmd;
                var hexCmd = BitConverter.ToString(Encoding.Default.GetBytes(cmd));
                _logger.LogDetailAsync($"Adding to QUEUE: {hexCmd}").SafeFireAndForget();
                _proliteCommandQueue.Add(cmd);

            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Turn ON Prolite Error: {ex.Message}").SafeFireAndForget();
            }
        }

        public void TurnOnLocation(int deviceNumber, int tray, int level, int part, int quantity)
        {
            _logger.LogDetailAsync($"Turn ON Prolite Location -  Device Number: {deviceNumber}  Tray: {tray}  Level: {level}  Part: {part}  Quantity: {quantity}").SafeFireAndForget();

            //var counter = 0;
            //while (_proliteBusy)
            //{
            //    Thread.Sleep(100);
            //    counter += 100;
            //    if (counter >= 10000) break;
            //    _logger.LogDetailAsync($"Prolite is Busy Current Wait Time: {counter}").SafeFireAndForget();
            //}

            try
            {
                //_proliteBusy = true;
                //_logger.LogDetailAsync($"Turn On Location Prolite Busy TRUE: {_proliteBusy}").SafeFireAndForget();
                var prolite = Prolites.FirstOrDefault(p => p.DeviceNumber == deviceNumber);
                if (prolite == null) return;
                if (prolite.Enabled == false) return;

                var cmd = prolite.TurnOnLocation(tray, level, part);
                _lastCommand = cmd;
                //SerialPortWrite(cmd);
                _logger.LogDetailAsync($"Adding to QUEUE: {cmd}").SafeFireAndForget();
                _proliteCommandQueue.Add(cmd);
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Turn ON Prolite Error: {ex.Message}").SafeFireAndForget();
            }
            //finally
            //{
            //    _proliteBusy = false;
            //    _logger.LogDetailAsync($"Turn On Location Prolite Busy TRUE: {_proliteBusy}").SafeFireAndForget();
            //}
        }

        public void TurnOnHot(int deviceNumber)
        {
            _logger.LogDetailAsync($"Turn ON Prolite Device: {deviceNumber} HOT").SafeFireAndForget();
            //var counter = 0;
            //while (_proliteBusy)
            //{
            //    Thread.Sleep(100);
            //    counter += 100;
            //    if (counter >= 10000) break;
            //    _logger.LogDetailAsync($"Prolite is Busy Current Wait Time: {counter}").SafeFireAndForget();
            //}

            try
            {
                //_proliteBusy = true;
                //_logger.LogDetailAsync($"Turn On Hot Prolite Busy TRUE: {_proliteBusy}").SafeFireAndForget();
                var prolite = Prolites.FirstOrDefault(p => p.DeviceNumber == deviceNumber);
                if (prolite == null) return;
                if (prolite.Enabled == false) return;
                var cmd = prolite.TurnOnHot();
                _lastCommand = cmd;
                if (!string.IsNullOrEmpty(cmd))
                {
                    //SerialPortWrite(cmd);
                    _logger.LogDetailAsync($"Adding to QUEUE: {cmd}").SafeFireAndForget();
                    _proliteCommandQueue.Add(cmd); ;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Turn ON Prolite Hot Error: {ex.Message}").SafeFireAndForget();
            }
            //finally
            //{
            //    _proliteBusy = false;
            //    _logger.LogDetailAsync($"Turn On Hot Prolite Busy TRUE: {_proliteBusy}").SafeFireAndForget();
            //}
        }

        public void TurnOnBlindCycle(int deviceNumber, int level, int part)
        {
            _logger.LogDetailAsync($"Turn ON Prolite Device: {deviceNumber} Blind Cycle").SafeFireAndForget();

            //var counter = 0;
            //while (_proliteBusy)
            //{
            //    Thread.Sleep(100);
            //    counter += 100;
            //    if (counter >= 10000) break;
            //    _logger.LogDetailAsync($"Prolite is Busy Current Wait Time: {counter}").SafeFireAndForget();
            //}

            try
            {
                //_proliteBusy = true;
                //_logger.LogDetailAsync($"Turn On Blind Cycle Prolite Busy TRUE: {_proliteBusy}").SafeFireAndForget();
                var prolite = Prolites.FirstOrDefault(p => p.DeviceNumber == deviceNumber);
                if (prolite == null) return;
                if (prolite.Enabled == false) return;
                var cmd = prolite.TurnOnBlindCycle(level, part);
                _lastCommand = cmd;
                if (!string.IsNullOrEmpty(cmd))
                {
                    //SerialPortWrite(cmd);
                    _logger.LogDetailAsync($"Adding to QUEUE: {cmd}").SafeFireAndForget();
                    _proliteCommandQueue.Add(cmd);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Turn ON Prolite Hot Error: {ex.Message}").SafeFireAndForget();
            }
            //finally
            //{
            //    _proliteBusy = false;
            //    _logger.LogDetailAsync($"Turn On Blind Cycle Prolite Busy TRUE: {_proliteBusy}").SafeFireAndForget();
            //}
        }

        // clear the prolite display
        public void ClearProlite(int deviceNumber)
        {

            _logger.LogDetailAsync($"Clear Prolite: {deviceNumber}").SafeFireAndForget();

            //var counter = 0;
            //while (_proliteBusy)
            //{
            //    Thread.Sleep(100);
            //    counter += 100;
            //    if (counter >= 10000) break;
            //    _logger.LogDetailAsync($"Prolite is Busy Current Wait Time: {counter}").SafeFireAndForget();
            //}

            try
            {
                _proliteBusyClearing = true;
                _logger.LogDetailAsync($"Clear All Prolite - Prolite Busy TRUE: {_proliteBusyClearing}").SafeFireAndForget();
                var prolite = Prolites.FirstOrDefault(p => p.DeviceNumber == deviceNumber);
                if (prolite is null) return;
                if (prolite.Enabled == false) return;
                var cmd = prolite.Clear();
                _lastCommand = cmd;
                if (!string.IsNullOrEmpty(cmd))
                {
                    //SerialPortWrite(cmd);
                    _logger.LogDetailAsync($"Adding to QUEUE: {cmd}").SafeFireAndForget();
                    _proliteCommandQueue.Add(cmd);
                    _proliteBusyClearing = false;
                }

            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Clear Prolite Error: {ex.Message}").SafeFireAndForget();
            }
            //finally
            //{
            //    _proliteBusy = false;
            //    _logger.LogDetailAsync($"Clear All Prolite - Prolite Busy TRUE: {_proliteBusy}").SafeFireAndForget();
            //}
        }
        /// <summary>
        /// Asynchronously clears all enabled Prolite devices managed by this instance.
        /// </summary>
        /// <remarks>
        /// This method iterates over all Prolite devices managed by this instance and sends a clear command to each enabled device.
        /// If a device is not enabled, it is skipped. If an error occurs while trying to clear a device, an error message is logged.
        /// </remarks>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        /// <exception cref="IOException">
        /// Thrown when an I/O error occurs while trying to clear a Prolite device.
        /// </exception>
        /// <exception cref="Exception">
        /// Thrown when an unspecified error occurs while trying to clear a Prolite device.
        /// </exception>
        public async Task ClearAllProlites()
        {

            _logger.LogDetailAsync($"Clear All Prolites").SafeFireAndForget();
            _proliteBusyClearing = true;
            _logger.LogDetailAsync($"Clear All Prolites - Prolite Busy Clearing: {_proliteBusyClearing}").SafeFireAndForget();
            if (Prolites == null)
            {
                _logger.LogDetailAsync($"Prolites are empty").SafeFireAndForget();
                return;
            }
            foreach (var prolite in Prolites)
            {
                if (prolite.Enabled == false) continue;
                try
                {
                    _logger.LogDetailAsync($"Clear Prolite {prolite.DeviceNumber}").SafeFireAndForget();
                    var cmd = prolite.Clear();

                    //SerialPortWrite(cmd);
                    _logger.LogDetailAsync($"Adding to QUEUE: {cmd}").SafeFireAndForget();
                    _proliteCommandQueue.Add(cmd);
                    // await Task.Delay(100);
                }
                catch (IOException ex)
                {
                    await _logger.LogDetailAsync(
                        $"An error occurred while trying to clear Prolite {prolite.DeviceNumber}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    await _logger.LogDetailAsync($"Turn OFF Prolite {prolite.DeviceNumber} Error: {ex.Message}");
                }
            }
            _proliteBusyClearing = false;
            _logger.LogDetailAsync($"Clear All Prolites - Prolite Busy Clearing: {_proliteBusyClearing}").SafeFireAndForget();

        }

        bool IProLiteManager.IsProliteManagerEnabled()
        {
            return ProliteManagerEnabled;
        }

        public List<Prolite> GetProlites()
        {
            return Prolites.ToList();
        }


        private string GetProliteNumber(int proliteNumber)
        {
            switch (proliteNumber)
            {
                case 1:
                    return "<ID01>";
                case 2:
                    return "<ID02>";
                case 3:
                    return "<ID03>";
                case 4:
                    return "<ID04>";
                default:
                    return "<ID01>";
            }
        }

        private async Task HandleSerialPortExceptionAsync(int i, Exception ex)
        {
            try
            {
                _serialPort?.Close();
            }
            catch (Exception e)
            {
                _logger.LogDetailAsync($"Close Exception Number {i}: {e.Message}").SafeFireAndForget();
            }
            var error = $"SerialPort Open Exception Number {i}: {ex.Message}";
            await _logger.LogDetailAsync($"Startup Fail Number {i}: {Environment.NewLine} {error}");
        }


        //private void InitSerialPort()
        //{
        //    _logger.LogDetailAsync($"Init Serial Port").SafeFireAndForget();

        //    var stopBits = GetStopBits(); // StopBits.One;

        //    _serialPort = new SerialPort(_portName, _baudRate, _parity, _dataBits, stopBits);

        //    _serialPort.WriteTimeout = 200;
        //    _serialPort.DataReceived += SerialPortOnDataReceived;
        //    int i;
        //    for (i = 0; i <= 6; i++)
        //    {
        //        try
        //        {
        //            _serialPort?.Open();
        //            if (_serialPort != null && _serialPort.IsOpen)
        //            {
        //                _logger.LogDetailAsync("Startup Success").SafeFireAndForget();
        //                //ShowData("Startup Success");
        //                //_readMp12DThread = new Thread(ReadMp12D);
        //                //RaiseSerialDataEvent += ProcessMp12DData;

        //                break;
        //            }
        //        }
        //        catch (IOException ex)
        //        {

        //            try
        //            {
        //                _serialPort?.Close();
        //            }
        //            catch (Exception e)
        //            {
        //                MessageBox.Show($"{ex}");
        //                _logger.LogDetailAsync($"Close Exception Number {i}: {e.Message}").SafeFireAndForget();
        //            }

        //            Thread.Sleep(500);

        //            var error = $"SerialPort Open Exception Number {i}: {ex.Message}";
        //            _logger.LogDetailAsync($"Startup Fail Number {i}: {Environment.NewLine} {error}").SafeFireAndForget();
        //        }
        //    }


        //    if (!IsPortOpen)
        //    {
        //        _logger.LogDetailAsync($"Port is NOT Open.  Number of fails: {i}").SafeFireAndForget();
        //    }
        //}

        private void SerialPortOnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //_logger.LogDetailAsync($"Pro-Lite Serial Data Received: ProliteBusy = {_proliteBusy} Current Command: {_currentCommand}").SafeFireAndForget();

            var serialPort = (SerialPort)sender;
            var data = string.Empty;

            while (true)
            {
                var existing = serialPort.ReadExisting();
                data += existing;
                Thread.Sleep(100);
                if (string.IsNullOrEmpty(existing)) break;
            }

            if (data.Length > 0)
            {
                // convert data to hex
                data = data.Trim();
                ProcessSerialData(data);
            }
            else
            {
                _logger.LogDetailAsync($"No data received from Pro-Lite").SafeFireAndForget();
            }
        }

        private void ProcessSerialData(string data)
        {
            var hexData = BitConverter.ToString(Encoding.Default.GetBytes(data));  //.Replace("-", "");
            var hexCurrentCommand = BitConverter.ToString(Encoding.Default.GetBytes(_currentCommand));  //.Replace("-", "");
            _logger.LogDetailAsync($"Pro-Lite Serial Data Received Current Command: {hexCurrentCommand}  Response: (Hex): [ {hexData} ] TURN OFF _proliteBusy").SafeFireAndForget();
            _proliteBusy = false;
            _currentCommand = string.Empty;

            //_logger.LogDetailAsync($"Pro-Lite Serial Data Received: ProliteBusy = {_proliteBusy}").SafeFireAndForget();

            //if (data.Length < 11)
            //{

            //    _serialPort.Write(_lastCommand);
            //    _logger.LogDetailAsync($"Pro-Lite Write Data: {_lastCommand}").SafeFireAndForget();
            //}
        }

        public bool IsPortOpen => _serialPort?.IsOpen ?? false;
    }
}
