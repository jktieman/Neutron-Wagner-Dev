using System;
using System.Linq;
using System.Threading.Tasks;
using AlliedLogger;
using JsonManager;
using NeutronCore.Extensions;
using NeutronCore.Global;
using NeutronData.ModelViews;
using AsyncAwaitBestPractices;
using NeutronEvents;

namespace IPTI.Models
{
    public class IptiDisplayFunctions : IIptiDisplayFunctions
    {
        private readonly IJsonData _jsonData;
        private readonly NeutronVariables _neutronVariables;
        private readonly WorkstationView _workstationView;
        private TcpIptiCommandCenter _tcpIptiCommandCenter;
        private IDisplayController _tcpIptiController;
        private IDynamicLogger _logger;

        public IptiDisplayFunctions(IJsonData jsonData, NeutronVariables neutronVariables, WorkstationView workstationView)
        {
            _jsonData = jsonData;
            _neutronVariables = neutronVariables;
            _workstationView = workstationView;
            Init();
        }
        private void Init()
        {
            _logger = NeutronCore.Global.Logger.SetupLogger("IptiDisplayFunctions");

            if (_tcpIptiCommandCenter == null)
            {
                _tcpIptiCommandCenter = new TcpIptiCommandCenter(_jsonData, _logger, _workstationView);
            }

            var iptiConfig = _jsonData.LoadFile<IptiConfig>();
            Task.Run(() => _logger.LogDetailAsync("IPTI Displays are being used."));

            _tcpIptiController = new TcpIptiController(_jsonData, _workstationView, _neutronVariables, _workstationView.BatchTable, _workstationView.BatchTable.TcpConfiguration, iptiConfig);

            if (_tcpIptiController == null)
            {
                Task.Run(() => _logger.LogDetailAsync("Batch Pick Displays were unable to initialize."));
                Mediator.GetInstance().OnDisplayMessage(this, $"Batch Pick Displays were unable to initialize.");
            }
           

            //TODO:
            //if (_neutronVariables.IptiDisplays)
            //{
            //    _logger.LogDetailAsync("IPTI Displays are being used.").SafeFireAndForget();


            //    //initialize the controller
            //    //var counter = 1;
            //    //TODO
            //    //while (_tcpIptiController.GetInitStatus() != true)
            //    //{
            //    //    var seconds = 250 * counter / 1000;
            //    //    _logger.LogDetailAsync($"Unable to initialize display controller for {seconds} seconds.").SafeFireAndForget();
            //    //    if (counter >= 20)
            //    //    {
            //    //        _logger.LogDetailAsync($"Unable to initialize display controller after {seconds} seconds.").SafeFireAndForget();
            //    //        break;
            //    //    }

            //    //    Thread.Sleep(250);
            //    //    counter += 1;
            //    //}
            //    //_logger.LogDetailAsync($"Display Controller Initialized. Status Code: {_tcpIptiController.GetInitStatus()}").SafeFireAndForget();
            //}
        }
        #region IPTI Display Functions

        public bool IsClientConnected()
        {
            var result = _tcpIptiController.GetInitStatus();
            return result;
        }

        public void DisposeServer()
        {
            _tcpIptiController.DisposeServer();
        }
        
        public async Task TurnOnBlastzoneDisplay(int bayController, int position, string text)
        {
            _logger.LogDetailAsync($"START").SafeFireAndForget();

            text = text.Replace("-", " ");

            try
            {
                if (!_workstationView.Blastzones.Any()) return;
                var bayId = bayController.ToString().PadLeft(2, '0');
                var controller = _tcpIptiCommandCenter.BlastBayControllers.FirstOrDefault(r => r.BayId == bayId);
                if (controller == null) return;
                if (!controller.Enabled) return;

                var command = controller.TurnOnDisplay(position, text);
                if (command != string.Empty)
                {
                    await _tcpIptiController.SendText(command);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Turn On Blastzone Display Function Failed:{Environment.NewLine}{ex.Message}").SafeFireAndForget();
            }
            _logger.LogDetailAsync($"END").SafeFireAndForget();
        }
        public async Task TurnOffBlastzoneDisplay(int bayController, int position)
        {
            _logger.LogDetailAsync($"START").SafeFireAndForget();
            try
            {
                if (!_workstationView.Blastzones.Any()) return;
                var bayId = bayController.ToString().PadLeft(2, '0');
                var controller = _tcpIptiCommandCenter.BlastBayControllers.FirstOrDefault(r => r.BayId == bayId);
                if (controller == null) return;
                if (!controller.Enabled) return;
                var command = controller.TurnOffDisplay(position);
                if (command != string.Empty)
                {
                    await _tcpIptiController.SendText(command);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Turn Off Blastzone Display Function Failed:{Environment.NewLine}{ex.Message}").SafeFireAndForget();
            }
            _logger.LogDetailAsync($"END").SafeFireAndForget();
        }
        public async Task TurnOnBlastzoneOrderControl(int bayController, string text)
        {
            _logger.LogDetailAsync($"START").SafeFireAndForget();
            try
            {
                if (!_workstationView.Blastzones.Any()) return;
                var bayId = bayController.ToString().PadLeft(2, '0');
                var controller = _tcpIptiCommandCenter.BlastBayControllers.FirstOrDefault(r => r.BayId == bayId);
                if (controller == null) return;
                if (!controller.Enabled) return;
                var command = controller.TurnOnOrderControlModule(text);
                if (command != string.Empty)
                {
                    await _tcpIptiController.SendText(command);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Turn On Blastzone Order Control Function Failed:{Environment.NewLine}{ex.Message}").SafeFireAndForget();
            }
            _logger.LogDetailAsync($"END").SafeFireAndForget();
        }
        public async Task TurnOffBlastzoneOrderControl(int bayController)
        {
            _logger.LogDetailAsync($"START").SafeFireAndForget();
            try
            {
                if (!_workstationView.Blastzones.Any()) return;
                var bayId = bayController.ToString().PadLeft(2, '0');
                var controller = _tcpIptiCommandCenter.BlastBayControllers.FirstOrDefault(r => r.BayId == bayId);
                if (controller == null) return;
                if (!controller.Enabled) return;
                var command = controller.TurnOffOrderControlModule();
                if (command != string.Empty)
                {
                    await _tcpIptiController.SendText(command);
                }
            }
            catch (Exception ex)
            {
                await _logger.LogDetailAsync($"Turn Off Blastzone Order Control Function Failed:{Environment.NewLine}{ex.Message}");
            }
            await _logger.LogDetailAsync($"END");
        }
        public async Task ClearBlastzone()
        {
            await _logger.LogDetailAsync($"START");
            try
            {
                if (!_workstationView.Blastzones.Any()) return;

                foreach (var bayController in _tcpIptiCommandCenter.BlastBayControllers)
                {
                    if (!bayController.Enabled) continue;
                    var text = bayController.ClearDisplays();
                    await _tcpIptiController.SendText(text);
                    //await Task.Delay(100);
                    await TurnOffBlastzoneOrderControl(bayController.BayId.ParseInt());
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Clear Blastzone Function Failed:{Environment.NewLine}{ex.Message}").SafeFireAndForget();
            }
            _logger.LogDetailAsync($"END").SafeFireAndForget();
        }

        public async Task ClearBlastzone(int bayId)
        {
            _logger.LogDetailAsync($"START").SafeFireAndForget();
            try
            {
                if (!_workstationView.Blastzones.Any()) return;
                var bayController = _tcpIptiCommandCenter.BlastBayControllers.FirstOrDefault(r => r.BayId == bayId.ToString().PadLeft(2, '0'));
                if (bayController == null) return;

                if (!bayController.Enabled) return;
                var text = bayController.ClearDisplays();
                await _tcpIptiController.SendText(text);
                //await Task.Delay(500);
                //text = bayController.TurnOffOrderControlModule();
                //await _tcpIptiController.SendText(text);

            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Clear Blastzone Function Failed:{Environment.NewLine}{ex.Message}").SafeFireAndForget();
            }
            _logger.LogDetailAsync($"END").SafeFireAndForget();
        }
        public async Task ClearBatchTable()
        {
            _logger.LogDetailAsync($"START").SafeFireAndForget();
            try
            {
                if (_workstationView.BatchTable == null) return;
                if (!_workstationView.BatchTable.Enabled) return;
                var command = _tcpIptiCommandCenter.BatchBayController.ClearDisplays();
                if (command != string.Empty)
                {
                    await _tcpIptiController.SendText(command);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Clear Batch Table Function Failed:{Environment.NewLine}{ex.Message}").SafeFireAndForget();
            }
            _logger.LogDetailAsync($"END").SafeFireAndForget();
        }
        public async Task TurnOnBatchDisplay(int position, string text)
        {
            text = text.Replace("-", " ");
            _logger.LogDetailAsync($"START: Position: {position}  Text: {text}").SafeFireAndForget();
            try
            {
                if (_workstationView.BatchTable == null) return;
                if (!_workstationView.BatchTable.Enabled) return;
                var command = _tcpIptiCommandCenter.BatchBayController.TurnOnDisplay(position, text);
                if (command != string.Empty)
                {
                    _logger.LogDetailAsync($"Command: {command}").SafeFireAndForget();
                    await _tcpIptiController.SendText(command);
                }

            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Turn On Batch Display Function Failed:{Environment.NewLine}{ex.Message}").SafeFireAndForget();
            }
            _logger.LogDetailAsync($"END").SafeFireAndForget();
        }
        public async Task TurnOffBatchDisplay(int position)
        {
            _logger.LogDetailAsync($"START").SafeFireAndForget();
            try
            {
                if (_workstationView.BatchTable == null) return;
                if (!_workstationView.BatchTable.Enabled) return;
                var command = _tcpIptiCommandCenter.BatchBayController.TurnOffDisplay(position);
                if (command != string.Empty)
                {
                    await _tcpIptiController.SendText(command);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Turn Off Batch Display Function Failed:{Environment.NewLine}{ex.Message}").SafeFireAndForget();
            }
            _logger.LogDetailAsync($"END").SafeFireAndForget();
        }
        public async Task TurnOnBatchOrderControl(string text)
        {
            _logger.LogDetailAsync($"START").SafeFireAndForget();
            try
            {
                if (_workstationView.BatchTable == null) return;
                if (!_workstationView.BatchTable.Enabled) return;
                var command = _tcpIptiCommandCenter.BatchBayController.TurnOnOrderControlModule(text);
                if (command != string.Empty)
                {
                    await _tcpIptiController.SendText(command);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Turn On Batch Order Control Function Failed:{Environment.NewLine}{ex.Message}").SafeFireAndForget();
            }
            _logger.LogDetailAsync($"END").SafeFireAndForget();
        }
        public async Task TurnOffBatchOrderControl()
        {
            _logger.LogDetailAsync($"START").SafeFireAndForget();
            try
            {
                if (_workstationView.BatchTable == null) return;
                if (!_workstationView.BatchTable.Enabled) return;
                var command = _tcpIptiCommandCenter.BatchBayController.TurnOffOrderControlModule();
                if (command != string.Empty)
                {
                    await _tcpIptiController.SendText(command);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Turn Off Batch Order Control Function Failed:{Environment.NewLine}{ex.Message}").SafeFireAndForget();
            }
            _logger.LogDetailAsync($"END").SafeFireAndForget();
        }

        public async Task TurnOnBatchDisplayEnd(int positionNumber)
        {
            _logger.LogDetailAsync($"START").SafeFireAndForget();
            try
            {
                if (_workstationView.BatchTable == null) return;
                if (!_workstationView.BatchTable.Enabled) return;
                var command = _tcpIptiCommandCenter.BatchBayController.TurnOnDisplayEnd(positionNumber);
                if (command != string.Empty)
                {
                    await _tcpIptiController.SendText(command);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Turn On Batch Display Function Failed:{Environment.NewLine}{ex.Message}").SafeFireAndForget();
            }
            _logger.LogDetailAsync($"END").SafeFireAndForget();
        }

        #endregion
    }
}
