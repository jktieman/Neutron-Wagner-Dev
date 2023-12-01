using AlliedLogger;
using AlliedPostOffice;
using JsonManager;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronData.ModelViews;
using NeutronEvents;
using SAPServer;

namespace NeutronLoader
{
    public class StartStopLoaderManager
    {
        private readonly IJsonData _jsonData;
        private readonly IDynamicLogger _logger;
        private IInterfaceProcessor _interfaceProcessor;
        private readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;
        private readonly WorkstationView _workstationView;

        private readonly ISapService _sapService;
        // private readonly ISendEmail _sendEmail;

        public StartStopLoaderManager(IJsonData jsonData, NeutronVariables neutronVariables,
            NeutronLicense neutronLicense, WorkstationView workstationView, ISapService sapService)
        {
            _jsonData = jsonData;
            _neutronVariables = neutronVariables;
            _neutronLicense = neutronLicense;
            _workstationView = workstationView;
            _sapService = sapService;
            //  _sendEmail = sendEmail;
            _logger = NeutronCore.Global.Logger.SetupLogger("LoaderManager");
            InitInterfaceFile();
            Mediator.GetInstance().StartStopLoader += (s, e) => StartStopLoaderAction(e.StartStop);
            Mediator.GetInstance().RunLoaderOnce += (s, e) => RunLoaderOnce();

        }

        private void InitInterfaceFile()
        {
            _ = _logger.LogDetailAsync($"InitInterfaceFile Company Code: {_neutronLicense.CompanyCode}");
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
                     _ = _logger.LogDetailAsync($"WAG - InterfaceProcessorPr1");
                        _interfaceProcessor = new InterfaceProcessorWAG(_neutronVariables, _neutronLicense, _jsonData, _workstationView, _sapService );
                        break;
                    }
                default:
                    {
                        _interfaceProcessor = new InterfaceProcessorPr1(_neutronVariables, _neutronLicense, _jsonData, _workstationView);
                        break;
                    }
            }

        }

        private void RunLoaderOnce()
        {
            _ = _logger.LogDetailAsync("Run Loader Once");
            _interfaceProcessor.RunLoaderOnce();
        }

        private void StartStopLoaderAction(string startStop)
        {
         _ = _logger.LogDetailAsync($"StartStopLoaderAction: {startStop}");

            if (startStop == "Start")
            {
             _ = _logger.LogDetailAsync("Start Processing Interface Files");
                StartProcessingInterfaceFiles();
            }
            else
            {
             _ = _logger.LogDetailAsync("Stop Processing Interface Files");
                StopProcessingInterfaceFiles();
            }
        }

        private void StartProcessingInterfaceFiles()
        {
         _ = _logger.LogDetailAsync("Start ProcessingInterfaceFiles");

            _interfaceProcessor.StartProcessingInterfaceFiles();
        }

        private void StopProcessingInterfaceFiles()
        {
         _ = _logger.LogDetailAsync("Stop ProcessingInterfaceFiles");
            _interfaceProcessor?.StopProcessingInterfaceFiles();
        }
    }
}
