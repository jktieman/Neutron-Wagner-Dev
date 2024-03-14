using System.Threading.Tasks;
using AlliedLogger;
using AsyncAwaitBestPractices;
using JsonManager;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronData.Interfaces;
using NeutronData.ModelViews;
using NeutronEvents;
using SAPServer;

namespace NeutronLoader
{
    public class StartStopLoaderManager
    {
        private readonly IJsonData _jsonData;
        private IDynamicLogger _logger;
        private IInterfaceProcessor _interfaceProcessor;
        private readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;
        private readonly WorkstationView _workstationView;
        private readonly IOrdersRepository _ordersRepository;
        private ISapService _sapService;

        public StartStopLoaderManager(IJsonData jsonData, NeutronVariables neutronVariables,
            NeutronLicense neutronLicense, WorkstationView workstationView, IOrdersRepository ordersRepository)
        {
            _jsonData = jsonData;
            _neutronVariables = neutronVariables;
            _neutronLicense = neutronLicense;
            _workstationView = workstationView;
            _ordersRepository = ordersRepository;
            Init();
        }

        private void Init()
        {
            _sapService = new SAPService(_jsonData);
            _logger = NeutronCore.Global.Logger.SetupLogger("LoaderManager");
            InitInterfaceFile();
            Mediator.GetInstance().StartStopLoader += async (s, e) => await StartStopLoaderAction(e.StartStop);
            Mediator.GetInstance().RunLoaderOnce += async (s, e) => await RunLoaderOnce();

        }

        private void InitInterfaceFile()
        {
            _logger.LogDetailAsync($"InitInterfaceFile Company Code: {_neutronLicense.CompanyCode}").SafeFireAndForget();
            switch (_neutronLicense.CompanyCode)
            {
                case "SFH":
                    {
                        _interfaceProcessor = new InterfaceProcessorSfh(_neutronVariables, _neutronLicense, _jsonData, _workstationView);
                        break;
                    }
                case "TOP":
                    {
                        _interfaceProcessor = new InterfaceProcessorTop(_neutronVariables, _neutronLicense, _jsonData, _workstationView);
                        break;
                    }
                case "TMG":
                    {
                        _interfaceProcessor = new InterfaceProcessorTmg(_neutronVariables, _neutronLicense, _jsonData, _workstationView);
                        break;
                    }
                case "PR1":
                    {
                        _interfaceProcessor = new InterfaceProcessorPr1(_neutronVariables, _neutronLicense, _jsonData, _workstationView);
                        break;
                    }
                case "MET":
                    {
                        _interfaceProcessor = new InterfaceProcessorMET(_neutronVariables, _neutronLicense, _jsonData, _workstationView);
                        break;
                    }
                case "WAG":
                    {
                        _logger.LogDetailAsync($"WAG - InterfaceProcessorWAG").SafeFireAndForget();
                        _interfaceProcessor = new InterfaceProcessorWAG(_neutronVariables, _neutronLicense, _jsonData, _workstationView, _sapService, _ordersRepository);
                        break;
                    }
                default:
                    {
                        _interfaceProcessor = new InterfaceProcessorPr1(_neutronVariables, _neutronLicense, _jsonData, _workstationView);
                        break;
                    }
            }

        }

        private async Task RunLoaderOnce()
        {
            _logger.LogDetailAsync("Run Loader Once").SafeFireAndForget();
            await _interfaceProcessor.RunLoaderOnce();
        }

        private async Task StartStopLoaderAction(string startStop)
        {
            _logger.LogDetailAsync($"StartStopLoaderAction: {startStop}").SafeFireAndForget();

            if (startStop == "Start")
            {
                _logger.LogDetailAsync("Start Processing Interface Files").SafeFireAndForget();
               await StartProcessingInterfaceFiles();
            }
            else
            {
                _logger.LogDetailAsync("Stop Processing Interface Files").SafeFireAndForget();
                StopProcessingInterfaceFiles();
            }
        }

        public async Task StartProcessingInterfaceFiles()
        {
            _logger.LogDetailAsync("Start ProcessingInterfaceFiles").SafeFireAndForget();

           await _interfaceProcessor.StartProcessingInterfaceFiles();
        }

        public void StopProcessingInterfaceFiles()
        {
            _logger.LogDetailAsync("Stop ProcessingInterfaceFiles").SafeFireAndForget();
            _interfaceProcessor?.StopProcessingInterfaceFiles();
        }
    }
}
