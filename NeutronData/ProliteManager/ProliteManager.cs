using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AlliedLogger;
using NeutronData.Models;
using NeutronCore.Global;
//using Logger = NeutronCore.Global.Logger;
using System.IO.Ports;
using System.Text;
//using NeutronData.Migrations;
using NeutronData.Models.Lookups;
//using System.Xml.Linq;

namespace NeutronData.ProliteManager;

// create a CallType enum
public enum CallType
{
    ClearAll = 0,
    CycleCountShowQuantity = 1,
    CycleCountNoQuantity = 2,
}

/// <summary>
/// The ProliteManager class is responsible for managing the Prolite devices.
/// The Prolite devices are HardwareDevices of type Prolite.
/// </summary>
public class ProliteManager : IProliteManager
{
    private readonly NeutronVariables _neutronVariables;
    private readonly IDynamicLogger _logger;
    private SerialPort _serialPort;
    private readonly IList<Prolite> _prolites;

    public int Id { get; }
    public string Name { get; private set; }
    public int WorkstationId { get; private set; }
    public int DeviceNumber { get; private set; }
    public int DeviceTypeId { get; private set; }
    public int? CommunicationTypeId { get; private set; }
    public int? TcpConfigurationId { get; private set; }
    public int? SerialConfigurationId { get; private set; }
    public int NumberOfCarriers { get; }
    public int CarrierLevel { get; private set; }
    public int CarrierWidth { get; private set; }
    public int CarrierDepth { get; private set; }
    public bool Enabled { get; private set; }
    public int LogLevel { get; private set; }
    public bool SimulationMode { get; private set; }
    public SerialConfiguration SerialConfiguration { get; private set; }
    public TcpConfiguration TcpConfiguration { get; private set; }
    public CommunicationType CommunicationType { get; private set; }
    public DeviceType DeviceType { get; private set; }
    public int Workstation { get; private set; }

    /// <summary>
    /// Takes a string and returns the corresponding Parity enum value.
    /// </summary>
    /// <param name="parity"></param>
    /// <returns></returns>
    private static Parity GetParity(string parity)
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
    private static StopBits GetStopBits(int stopBits)
    {
        switch (stopBits)
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


    public ProliteManager(HardwareDevice hardwareDevice, NeutronVariables neutronVariables)
    {
        _neutronVariables = neutronVariables;
        _prolites = new List<Prolite>();
        _logger = NeutronCore.Global.Logger.SetupLogger("ProliteManager");

        Id = hardwareDevice.Id;
        Name = hardwareDevice.Name;
        DeviceNumber = hardwareDevice.DeviceNumber;
        DeviceTypeId = hardwareDevice.DeviceTypeId;
        CommunicationTypeId = hardwareDevice.CommunicationTypeId;
        TcpConfigurationId = hardwareDevice.TcpConfigurationId;
        SerialConfigurationId = hardwareDevice.SerialConfigurationId;
        NumberOfCarriers = hardwareDevice.NumberOfCarriers;
        CarrierLevel = hardwareDevice.CarrierLevel;
        CarrierWidth = hardwareDevice.CarrierWidth;
        CarrierDepth = hardwareDevice.CarrierDepth;
        Enabled = hardwareDevice.Enabled;
        LogLevel = hardwareDevice.LogLevel;
        SimulationMode = hardwareDevice.SimulationMode;
        SerialConfiguration = hardwareDevice.SerialConfiguration;
        TcpConfiguration = hardwareDevice.TcpConfiguration;
        CommunicationType = hardwareDevice.CommunicationType;
        DeviceType = hardwareDevice.DeviceType;
        WorkstationId = hardwareDevice.WorkstationId;
        InitSerialPort();
    }

    /// <summary>
    /// Add a ProLite device to the list of Prolites.
    /// A Prolite is a HardwareDevice of type Prolite.
    /// </summary>
    /// <param name="hardwareDevice"></param>
    public void AddProlite(HardwareDevice hardwareDevice)
    {
        _logger.LogDetailAsync($"Add Prolite: {hardwareDevice.Name}");
        try
        {
            var pro = _prolites.FirstOrDefault(p => p.DeviceNumber == hardwareDevice.DeviceNumber);
            if (pro is null)
            {
                var prolite = new Prolite(hardwareDevice);

                _prolites.Add(prolite);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDetailAsync($"Add Prolite Error: {ex.Message}");
        }
    }

    public void RemoveProlite(int deviceNumber)
    {
        _logger.LogDetailAsync($"Remove Prolite: {deviceNumber}");
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
            _logger.LogDetailAsync($"Remove Prolite Error: {ex.Message}");
        }
    }

    public void TurnOn(int deviceNumber, int level, int part, int quantity)
    {
        _logger.LogDetailAsync($"Turn ON Prolite Device: {deviceNumber} Level: {level}  Part: {part}  Quantity: {quantity}");
        try
        {
            var prolite = _prolites.FirstOrDefault(p => p.DeviceNumber == deviceNumber);
            if (prolite != null)
            {
                var cmd = prolite.TurnOn(level, part, quantity);
                var msg = Encoding.UTF8.GetBytes(cmd);
                if (!string.IsNullOrEmpty(cmd)) _serialPort.Write(msg,0, msg.Length);
                // if (!string.IsNullOrEmpty(cmd)) _serialPort.Write(cmd);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDetailAsync($"Turn ON Prolite Error: {ex.Message}");
        }
    }

    public void TurnOnHot(int deviceNumber)
    {
        _logger.LogDetailAsync($"Turn ON Prolite Device: {deviceNumber} HOT");
        try
        {
            var prolite = _prolites.FirstOrDefault(p => p.DeviceNumber == deviceNumber);
            if (prolite != null)
            {
                var cmd = prolite.TurnOnHot();
                if (!string.IsNullOrEmpty(cmd)) _serialPort.Write(cmd);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDetailAsync($"Turn ON Prolite Hot Error: {ex.Message}");
        }
    }

    public void TurnOnBlindCycle(int deviceNumber, int level, int part)
    {
        _logger.LogDetailAsync($"Turn ON Prolite Device: {deviceNumber} Blind Cycle");
        try
        {
            var prolite = _prolites.FirstOrDefault(p => p.DeviceNumber == deviceNumber);
            if (prolite != null)
            {
                var cmd = prolite.TurnOnBlindCycle(level, part);
                if (!string.IsNullOrEmpty(cmd)) _serialPort.Write(cmd);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDetailAsync($"Turn ON Prolite Hot Error: {ex.Message}");
        }
    }

    // clear the prolite display
    public void ClearProlite(int deviceNumber)
    {
        _logger.LogDetailAsync($"Clear Prolite: {deviceNumber}");
        try
        {
            var prolite = _prolites.FirstOrDefault(p => p.DeviceNumber == deviceNumber);
            var cmd = prolite?.Clear();
            if (!string.IsNullOrEmpty(cmd)) _serialPort.Write(cmd);
        }
        catch (Exception ex)
        {
            _logger.LogDetailAsync($"Clear Prolite Error: {ex.Message}");
        }
    }

    // turn off the all prolite displays
    public void ClearAllProlites()
    {
        _logger.LogDetailAsync($"Turn OFF ALL Prolites");
        try
        {
            foreach (var prolite in _prolites)
            {
                var cmd = prolite?.Clear();
                if (!string.IsNullOrEmpty(cmd)) _serialPort.Write(cmd);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDetailAsync($"Turn OFF Prolite Error: {ex.Message}");
        }
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
        _logger.LogDetailAsync($"Init Serial Port");

        var serialConfig = SerialConfiguration;

        var stopBits = GetStopBits(serialConfig.StopBits); // StopBits.One;

        _serialPort = new SerialPort(serialConfig.PortName, serialConfig.BaudRate, serialConfig.Parity
            , serialConfig.DataBits, stopBits);

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
                    _logger.LogDetailAsync("Startup Success");
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
                    _logger.LogDetailAsync($"Close Exception Number {i}: {e.Message}");
                }

                Thread.Sleep(500);

                var error = $"SerialPort Open Exception Number {i}: {ex.Message}";
                _logger.LogDetailAsync($"Startup Fail Number {i}: {Environment.NewLine} {error}");
            }
        }

        if (!IsPortOpen)
        {
            _logger.LogDetailAsync($"Port is NOT Open.  Number of fails: {i}");
        }
    }

    private void SerialPortOnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        var serialPort = (SerialPort)sender;
        var data = serialPort.ReadExisting();
        _logger.LogDetailAsync($"Serial Data Received: {data}");
    }

    public bool IsPortOpen => _serialPort?.IsOpen ?? false;


}