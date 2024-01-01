using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using AlliedLogger;
using NeutronCore.Global;
using NeutronCore.Enums;
using NeutronData.ModelViews;


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
    /// The ProLiteManager class is responsible for managing the Prolite devices.
    /// The Prolite devices are HardwareDevices of type Prolite.
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
        private readonly IDynamicLogger _logger;
        private SerialPort _serialPort;
        private readonly IList<Prolite> _prolites;
        private string _lastCommand = string.Empty;

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
            _prolites = new List<Prolite>();
            _logger = NeutronCore.Global.Logger.SetupLogger("ProLiteManager");

            InitSerialPort();
        }

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
            _ = _logger.LogDetailAsync($"Add Prolite Id: {deviceNumber} - {name}");
            try
            {
                var pro = _prolites.FirstOrDefault(p => p.DeviceNumber == deviceNumber);
                if (pro is null)
                {
                    var prolite = new Prolite(id, name, deviceNumber, enabled);

                    _prolites.Add(prolite);
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"Add Prolite Error: {ex.Message}");
            }
        }
        public void RemoveProlite(int deviceNumber)
        {
            _ = _logger.LogDetailAsync($"Remove Prolite: {deviceNumber}");
            try
            {
                var prolite = _prolites.FirstOrDefault(p => p.DeviceNumber == deviceNumber);

                if (prolite != null)
                {
                    _prolites.Remove(prolite);
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"Remove Prolite Error: {ex.Message}");
            }
        }
        public void TurnOn(int deviceNumber, int level, int part, int quantity)
        {
            _ = _logger.LogDetailAsync($"Turn ON Prolite Device: {deviceNumber} Level: {level}  Part: {part}  Quantity: {quantity}");
            try
            {
                var prolite = _prolites.FirstOrDefault(p => p.DeviceNumber == deviceNumber);
                if (prolite == null) return;
                var cmd = prolite.TurnOn(level, part, quantity);
                _lastCommand = cmd;
                _serialPort.Write(cmd);

            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"Turn ON Prolite Error: {ex.Message}");
            }
        }

        public void TurnOnHot(int deviceNumber)
        {
            _ = _logger.LogDetailAsync($"Turn ON Prolite Device: {deviceNumber} HOT");
            try
            {
                var prolite = _prolites.FirstOrDefault(p => p.DeviceNumber == deviceNumber);
                if (prolite == null) return;
                var cmd = prolite.TurnOnHot();
                _lastCommand = cmd;
                if (!string.IsNullOrEmpty(cmd)) _serialPort.Write(cmd);
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"Turn ON Prolite Hot Error: {ex.Message}");
            }
        }

        public void TurnOnBlindCycle(int deviceNumber, int level, int part)
        {
            _ = _logger.LogDetailAsync($"Turn ON Prolite Device: {deviceNumber} Blind Cycle");
            try
            {
                var prolite = _prolites.FirstOrDefault(p => p.DeviceNumber == deviceNumber);
                if (prolite == null) return;
                var cmd = prolite.TurnOnBlindCycle(level, part);
                _lastCommand = cmd;
                if (!string.IsNullOrEmpty(cmd)) _serialPort.Write(cmd);
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"Turn ON Prolite Hot Error: {ex.Message}");
            }
        }

        // clear the prolite display
        public void ClearProlite(int deviceNumber)
        {
            _ = _logger.LogDetailAsync($"Clear Prolite: {deviceNumber}");
            
            try
            {
                var prolite = _prolites.FirstOrDefault(p => p.DeviceNumber == deviceNumber);
                if (prolite is null) return;
                var cmd = prolite.Clear();
                _lastCommand = cmd;
                if (!string.IsNullOrEmpty(cmd)) _serialPort.Write(cmd);
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"Clear Prolite Error: {ex.Message}");
            }
        }

        // turn off the all prolite displays
        public void ClearAllProlites()
        {
            _ = _logger.LogDetailAsync($"Turn OFF ALL Prolites");
            try
            {
                foreach (var prolite in _prolites)
                {
                    _serialPort.Write(prolite.Clear());
                    //ClearProlite(prolite.DeviceNumber);
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"Turn OFF Prolite Error: {ex.Message}");
            }
            _ = _logger.LogDetailAsync($"Turn OFF ALL Prolites Complete");
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

        private void InitSerialPort()
        {
            _ = _logger.LogDetailAsync($"Init Serial Port");

            var stopBits = GetStopBits(); // StopBits.One;

            _serialPort = new SerialPort(_portName, _baudRate, _parity, _dataBits, stopBits);

            _serialPort.WriteTimeout = 200;
            _serialPort.DataReceived += SerialPortOnDataReceived;
            int i;
            for (i = 0; i <= 60; i++)
            {
                try
                {
                    _serialPort?.Open();
                    if (_serialPort != null && _serialPort.IsOpen)
                    {
                        _ = _logger.LogDetailAsync("Startup Success");
                        //ShowData("Startup Success");
                        //_readMp12DThread = new Thread(ReadMp12D);
                        //RaiseSerialDataEvent += ProcessMp12DData;

                        break;
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        _serialPort?.Close();
                    }
                    catch (Exception e)
                    {
                        _ = _logger.LogDetailAsync($"Close Exception Number {i}: {e.Message}");
                    }

                    Thread.Sleep(500);

                    var error = $"SerialPort Open Exception Number {i}: {ex.Message}";
                    _ = _logger.LogDetailAsync($"Startup Fail Number {i}: {Environment.NewLine} {error}");
                }
            }

            if (!IsPortOpen)
            {
                _ = _logger.LogDetailAsync($"Port is NOT Open.  Number of fails: {i}");
            }
        }

        private void SerialPortOnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var serialPort = (SerialPort)sender;
            var data = string.Empty;
            string existing;

            while (true)
            {
                existing = serialPort.ReadExisting();
                data += existing;
                Thread.Sleep(1000);
                if (string.IsNullOrEmpty(existing)) break;
            }

            if (data.Length > 0)
            {
                _ = _logger.LogDetailAsync($"Pro-Lite Serial Data Received: {data}");
               // ProcessSerialData(data);
            }
        }

        private void ProcessSerialData(string data)
        {
            if (data.Length < 11)
            {
                _serialPort.Write(_lastCommand);
                _ = _logger.LogDetailAsync($"Pro-Lite Write Data: {_lastCommand}");
            }
        }

        public bool IsPortOpen => _serialPort?.IsOpen ?? false;
    }
}
