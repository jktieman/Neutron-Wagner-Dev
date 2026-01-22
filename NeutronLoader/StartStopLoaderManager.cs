using AlliedLogger;
using AsyncAwaitBestPractices;
using JsonManager;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronData.DataContexts;
using NeutronData.Interfaces;
using NeutronData.ModelViews;
using NeutronEvents;
using SAPServer;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ReplenService;

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
        private readonly IReplenRepository _replenRepository;
        private readonly IOrdersRepository _ordersRepository;
        private ISapService _sapService;
        private readonly Func<NeutronDb> _contextFactory;

        public StartStopLoaderManager(IJsonData jsonData, NeutronVariables neutronVariables,
            NeutronLicense neutronLicense, WorkstationView workstationView
            , IReplenRepository replenRepository, IOrdersRepository ordersRepository, Func<NeutronDb> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _jsonData = jsonData;
            _neutronVariables = neutronVariables;
            _neutronLicense = neutronLicense;
            _workstationView = workstationView;
            _replenRepository = replenRepository;
            _ordersRepository = ordersRepository;
            Init();
        }

        private void Init()
        {
            _sapService = new SAPService(_jsonData);
            _logger = NeutronCore.Global.Logger.SetupLogger("LoaderManager");
            InitInterfaceFile();
            Mediator.GetInstance().StartStopLoader += async (s, e) => await StartStopLoaderAction(e.StartStop);
           // Mediator.GetInstance().RunLoaderOnce += async (s, e) => await RunLoaderOnce();

        }

        private void InitInterfaceFile()
        {
            _logger.LogDetailAsync($"InitInterfaceFile Company Code: {_neutronLicense.CompanyCode}").SafeFireAndForget();
            switch (_neutronLicense.CompanyCode)
            {
                case "SFH":
                    {
                        _interfaceProcessor = new InterfaceProcessorSfh(_neutronVariables, _neutronLicense, _jsonData, _workstationView, _contextFactory);
                        break;
                    }
                case "TOP":
                    {
                        _interfaceProcessor = new InterfaceProcessorTop(_neutronVariables, _neutronLicense, _jsonData, _workstationView, _contextFactory);
                        break;
                    }
                case "TMG":
                    {
                        _interfaceProcessor = new InterfaceProcessorTmg(_neutronVariables, _neutronLicense, _jsonData, _workstationView, _contextFactory);
                        break;
                    }
                case "PR1":
                    {
                        _interfaceProcessor = new InterfaceProcessorPr1(_neutronVariables, _neutronLicense, _jsonData, _workstationView, _contextFactory);
                        break;
                    }
                case "MET":
                    {
                        _interfaceProcessor = new InterfaceProcessorMET(_neutronVariables, _neutronLicense, _jsonData, _workstationView, _contextFactory);
                        break;
                    }
                case "WAG":
                    {
                        _logger.LogDetailAsync($"WAG - InterfaceProcessorWAG").SafeFireAndForget();
                        _interfaceProcessor = new InterfaceProcessorWAG(_neutronVariables, _neutronLicense, _jsonData, _workstationView, _sapService, _replenRepository, _ordersRepository,  _contextFactory);
                        Mediator.GetInstance().RunLoaderOnceAsync += async (s, e) => await _interfaceProcessor.RunLoaderOnce();
                        break;
                    }
                default:
                    {
                        _interfaceProcessor = new InterfaceProcessorPr1(_neutronVariables, _neutronLicense, _jsonData, _workstationView, _contextFactory);
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
