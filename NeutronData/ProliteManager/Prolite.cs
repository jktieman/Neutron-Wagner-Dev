using AlliedLogger;
using NeutronData.Models;


namespace NeutronData.ProliteManager;

public class Prolite : IProlite
{

    private string _proliteNumber = "<ID01>";
    private readonly IDynamicLogger _logger;
    private readonly HardwareDevice _hardwareDevice;

    public int Id { get; }
    public string Name { get; }
    public int DeviceNumber { get; }
    public bool Enabled { get; }

    /// <summary>
    /// An individual Prolite device
    /// </summary>
    /// <param name="hardwareDevice"></param>
    public Prolite(HardwareDevice hardwareDevice)
    {
        _logger = NeutronCore.Global.Logger.SetupLogger("Prolite");
        _hardwareDevice = hardwareDevice;

        Id = hardwareDevice.Id;
        Name = hardwareDevice.Name;
        DeviceNumber = hardwareDevice.DeviceNumber;
        Enabled = hardwareDevice.Enabled;
           
        Init();
    }
    /// <summary>
    /// Initialize the Prolite device with the device number
    /// </summary>
    private void Init()
    {
        _proliteNumber = $"<ID{DeviceNumber:D2}>";
    }

    /// <summary>
    /// Turn on the Prolite device
    /// Showing the level, partition, and quantity
    /// </summary>
    /// <param name="level"></param>
    /// <param name="part"></param>
    /// <param name="quantity"></param>
    /// <returns></returns>
    public string TurnOn(int level, int part, int quantity)
    {
        var work = $"W: {level}";
        work += $" D: {part}";
        work += $" Q: {quantity}";

        var cmd = $"{_proliteNumber}<PA><FQ><CC>{work}";

        _logger.LogDetailAsync($"TurnOn: {cmd}");

        return cmd;
    }

    /// <summary>
    /// Clear the Prolite device
    /// </summary>
    /// <returns></returns>
    public string Clear()
    {
        var cmd = string.Empty;
        _logger.LogDetailAsync($"Clear");
           
        if (Enabled)
        {
            var work = "  ";
            cmd = $"{_proliteNumber}<PA>{work}";
        }
        return cmd;
    }

    public string TurnOnBlindCycle(int level, int part)
    {
        var work = $"W: {level}";
        work += $" D: {part}";

        var cmd = $"{_proliteNumber}<PA><FQ><CC>{work}";

        _logger.LogDetailAsync($"TurnOn Blind Cycle: {cmd}");

        return cmd;
    }

    public string TurnOnHot()
    {
        var work = $"<<<- HOT ->>>";
           

        var cmd = $"{_proliteNumber}<PA><FQ><CC>{work}";

        _logger.LogDetailAsync($"TurnOn HOT: {cmd}");

        return cmd;
    }

}