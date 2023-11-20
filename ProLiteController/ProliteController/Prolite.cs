using System;
using AlliedLogger;
using Logger = NeutronCore.Global.Logger;

namespace ProliteController
{
    public class Prolite : IProlite
    {

       private string _proliteNumber = "<ID01>";
       private readonly IDynamicLogger _logger;

        public int Id { get; }
        public string Name { get; }
        public int DeviceNumber { get; }
        public bool Enabled { get; }

        /// <summary>
        /// An individual Prolite device
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="deviceNumber"></param>
        /// <param name="enabled"></param>
        public Prolite(int id, string name, int deviceNumber, bool enabled)
        {
            _logger = Logger.SetupLogger("Prolite");
            
            Id = id;
            Name = name;
            DeviceNumber = deviceNumber;
            Enabled = enabled;
           
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
            var work = $"<CN>W:<CC>{level}";
            work += $"<CN> D:<CC>{part}";
            work += $"<CN> Q:<CE>{quantity}";

            var cmd = $"{_proliteNumber}<PA><FC>{work}{Environment.NewLine}";

            _ = _logger.LogDetailAsync($"TurnOn: {cmd}");

            return cmd;
        }

        /// <summary>
        /// Clear the Prolite device
        /// </summary>
        /// <returns></returns>
        public string Clear()
        {
            var cmd = string.Empty;
         _ = _logger.LogDetailAsync($"Clear");
           
            if (Enabled)
            {
                 var work = "  ";
            cmd = $"{_proliteNumber}<PA>{work}{Environment.NewLine}";
            }
            return cmd;
        }

        public string TurnOnBlindCycle(int level, int part)
        {
            var work = $"<CN>W:<CC>{level}";
            work += $"<CN> D:<CC>{part}";

            var cmd = $"{_proliteNumber}<PA><FC>{work}{Environment.NewLine}";

            _ = _logger.LogDetailAsync($"TurnOn Blind Cycle: {cmd}");

            return cmd;
        }

        public string TurnOnHot()
        {
            var work = $"<<<- HOT ->>>";
           

            var cmd = $"{_proliteNumber}<PA><FQ><CC>{work}{Environment.NewLine}";

         _ = _logger.LogDetailAsync($"TurnOn HOT: {cmd}");

            return cmd;
        }

    }
}
