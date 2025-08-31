using AlliedLogger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace HanelCommands
{
    public class HanelCommandService
    {
        private readonly int _numberOfDevices;
        private List<string> _commands;
        private List<string> _subCommands;
        private List<string> _responses;
        private List<string> _subResponses;
       // private List<HanelCommand> _hanelCommands = new List<HanelCommand>();
        private List<HanelDeviceStatus> _currentHanelDeviceStatusList;
        private readonly IDynamicLogger _logger;
        private readonly bool _testing;

        public HanelCommandService(int numberOfDevices, List<HanelDeviceStatus> currentHanelDeviceStatusList)
        {
            _currentHanelDeviceStatusList = currentHanelDeviceStatusList;
            _numberOfDevices = numberOfDevices;
            _testing = false;
            _logger = NeutronCore.Global.Logger.SetupLogger("HanelCommandService");
            Init();
        }

        public HanelCommandService(int numberOfDevices, List<HanelDeviceStatus> currentHanelDeviceStatusList, IDynamicLogger logger)
        {
            _currentHanelDeviceStatusList = currentHanelDeviceStatusList;
            _numberOfDevices = numberOfDevices;
            _logger = logger;
            _testing = true;
            Init();
        }

        private void Init()
        { 
            LoadCommands();
            LoadResponses();
        }

        public HanelDeviceStatus GetDeviceStatus(string deviceNumber)
        {
            var num = int.Parse(deviceNumber);
            return _currentHanelDeviceStatusList.FirstOrDefault(r => r.DeviceNumber == num);
        }

        public IHanelResponse GetHanelResponse(byte[] dataIn)
        {
            IHanelResponse hanelResponse = null;
            var str = System.Text.Encoding.UTF8.GetString(dataIn);
            var s = str.Split('$');

            if (s.Length == 1 && str.Contains("BE"))
            {
                hanelResponse = new HanelResponseBufferEmpty();
                hanelResponse.Response(s);
                return hanelResponse;
            }


            if (IsValidResponse(s))
            {
                var cmd = s[1];
                switch (cmd)
                {
                    case "P XS":
                        {
                            hanelResponse = new HanelResponseAccepted(s);
                            hanelResponse.Response(s);

                            break;

                        }
                    case "P XA":
                    {
                        hanelResponse = new HanelResponseExecuted(s);
                        hanelResponse.Response(s);

                        break;

                    }
                }
            }

            return hanelResponse;
        }
        /// <summary>
        /// Takes a byte array command and returns a HanelCommand
        /// </summary>
        /// <param name="dataIn">byte[]</param>
        /// <returns><see cref="HanelCommand"/></returns>
        public IHanelCommand GetHanelCommand(byte[] dataIn)
        {
            IHanelCommand hanelCommand = null;
            var str = System.Text.Encoding.UTF8.GetString(dataIn);
            var s = str.Split('$');

            if (IsValidCommand(s))
            {
                var cmd = s[1];
                switch (cmd)
                {
                    case "M XC":
                        {
                            hanelCommand = new XCCommand(dataIn);
                            break;
                        }
                    case "M XO":
                        {
                            hanelCommand = new XOCommand(dataIn);
                            break;
                        }
                    case "M XI":
                        {
                            hanelCommand = new XICommand(dataIn);
                            break;
                        }
                    case "M XR":
                        {
                            if (s.Length >= 3 && IsValidSubCommand(s[2]))
                            {
                                var subCmd = s[2];

                                switch (s[2])
                                {
                                    case "E11":
                                        {
                                            hanelCommand = new XR_E11Command(dataIn);
                                            break;
                                        }
                                    case "E12":
                                        {
                                            hanelCommand = new XR_E12Command(dataIn);
                                            break;
                                        }
                                    case "E13":
                                        {
                                            hanelCommand = new XR_E13Command(dataIn);
                                            break;
                                        }
                                    case "E17":
                                        {
                                            hanelCommand = new XR_E20Command(dataIn);
                                            break;
                                        }
                                    case "E20":
                                        {
                                            hanelCommand = new XR_E20Command(dataIn);
                                            break;
                                        }
                                    case "E21":
                                        {
                                            hanelCommand = new XR_E20Command(dataIn);
                                            break;
                                        }
                                    case "E24":
                                        {
                                            hanelCommand = new XR_E20Command(dataIn);
                                            break;
                                        }
                                    case "E42":
                                        {
                                            hanelCommand = new XR_E20Command(dataIn);
                                            break;
                                        }
                                    case "E44":
                                        {
                                            hanelCommand = new XR_E20Command(dataIn);
                                            break;
                                        }
                                    case "E46":
                                        {
                                            hanelCommand = new XR_E20Command(dataIn);
                                            break;
                                        }
                                    case "E47":
                                        {
                                            hanelCommand = new XR_E20Command(dataIn);
                                            break;
                                        }
                                    case "E48":
                                        {
                                            hanelCommand = new XR_E20Command(dataIn);
                                            break;
                                        }
                                    case "E49":
                                        {
                                            hanelCommand = new XR_E20Command(dataIn);
                                            break;
                                        }
                                    case "E50":
                                        {
                                            hanelCommand = new XR_E20Command(dataIn);
                                            break;
                                        }
                                    case "E60":
                                        {
                                            hanelCommand = new XR_E20Command(dataIn);
                                            break;
                                        }
                                    case "E70":
                                        {
                                            hanelCommand = new XR_E20Command(dataIn);
                                            break;
                                        }
                                    case "T000":
                                        {
                                            hanelCommand = new XR_E20Command(dataIn);
                                            break;
                                        }
                                }

                            }

                            break;
                        }
                    case "M XM":
                        {
                            hanelCommand = new XR_E20Command(dataIn);
                            break;
                        }
                    case "M XE":
                        {
                            hanelCommand = new XR_E20Command(dataIn);
                            break;
                        }
                }
            }

            if (hanelCommand != null)
            {
                // hanelCommand.Command = dataIn;
            }

            return hanelCommand;
        }

        private bool IsValidCommand(IReadOnlyList<string> s)
        {
            var result = s.Count >= 2 && _commands.Contains(s[1]);
            return result;
        }

        private bool IsValidSubCommand(string command)
        {
            var result = _subCommands.Contains(command);

            return result;
        }

        private void LoadCommands()
        {
            _commands = new List<string>()
            {
                "M XO", "M XI", "M XR", "M XC", "M XM", "M XE"
            };


            _subCommands = new List<string>()
            {
                "E11", "E12", "E13", "E17", "E20", "E21", "E24", "E42", "E44", "E46", "E47", "E48", "E49", "E50",
                "E60", "E70", "T000"
            };
        }

        private bool IsValidResponse(IReadOnlyList<string> s)
        {
            var result = s.Count >= 1;
            return result;
        }

        private void LoadResponses()
        {
            _responses = new List<string>()
            {
                "P XS"

            };

            _subResponses = new List<string>()
            {
                "E00"
            };
        }

        public IHanelCommand MoveDeviceCommand(int lift, int ap, int tray, int over, int back)
        {
            var hanelCommand = new XR_E20Command(lift.ToString(), ap.ToString(), tray.ToString(), over.ToString(), back.ToString());

            //var l = int.Parse(lift);
            //var device = _currentHanelDeviceStatusList.FirstOrDefault(r => r.DeviceNumber == l);
           // if (device != null)
           // {
           //     device.LastHanelCommand = hanelCommand;
           // }

            return hanelCommand;
        }

        public IHanelCommand Poll()
        {
            var hanelCommand = new PollCommand();
            return hanelCommand;
        }

        public IHanelCommand DisplayText(int lift, int accessPoint, List<DisplayLine> displayLines )
        {
            var hanelCommand = new XOCommand(lift.ToString(), accessPoint.ToString(), displayLines);
            return hanelCommand;
        }

        public IHanelCommand Cancel(int lift, int accessPoint)
        {
            var hanelCommand = new XCCommand(lift.ToString(), accessPoint.ToString());
            return hanelCommand;
        }

        public IHanelCommand GetTray(int lift, int accessPoint = 1)
        {
            var hanelCommand = new XR_E12Command(lift.ToString(), accessPoint.ToString());
            return hanelCommand;
        }

        public IHanelCommand GetTrayInWindow(int lift, int accessPoint = 1)
        {
            var hanelCommand = new XR_E12Command(lift.ToString(), accessPoint.ToString());
            return hanelCommand;
        }

        public string GetResponse(byte[] dataIn)
        {
            IHanelResponse hanelCommand = GetHanelResponse(dataIn);
            var l = int.Parse(hanelCommand.Lift);
            var device = _currentHanelDeviceStatusList.FirstOrDefault(r => r.DeviceNumber == l);
            if (device != null)
            {
                if (device.LastHanelCommand != null)
                {
                    if (!device.LastHanelCommand.Accepted)
                    {
                        device.LastHanelCommand.Accepted = true;
                        return device.LastHanelCommand.CommandAccepted;
                    }
                    //else
                    //{
                    //    device.LastHanelCommand.Accepted = true;
                    //    return device.LastHanelCommand.CommandAccepted;
                    //}
                }




            }

            return string.Empty;
        }

        public HanelDeviceStatus UpdateDeviceStatus(IHanelCommand command)
        {

            try
            {
                var l = int.Parse(command.Lift);
                var device = _currentHanelDeviceStatusList.FirstOrDefault(r => r.DeviceNumber == l);
                if (device != null)
                {
                    if (device.LastHanelCommand != null)
                    {
                        if (!device.LastHanelCommand.Accepted)
                        {
                            device.LastHanelCommand.Accepted = true;
                        }
                    }
                    else
                    {
                        device.LastHanelCommand = command;
                    }
                }

                return device;
            }
            catch (Exception ex)
            {
             _ = _logger.LogDetailAsync($"Exception: {ex.Message}");
                throw;
            }
        }

        public IHanelCommand ChangeDisplaySize(int lift, int accessPoint, string size)
        {
            var hanelCommand = new XR_E13Command(lift.ToString(), accessPoint.ToString(), size);
            return hanelCommand;
        }
    }
}
