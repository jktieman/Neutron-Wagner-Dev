using System.Collections.Generic;
using AlliedLogger;
using AlliedPostOffice.Concrete;
using JsonManager;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronData.Models;
using NeutronData.ModelViews;
using NeutronEvents;

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

        public StartStopLoaderManager(IJsonData jsonData, NeutronVariables neutronVariables,
            NeutronLicense neutronLicense, WorkstationView workstationView)
        {
            _jsonData = jsonData;
            _neutronVariables = neutronVariables;
            _neutronLicense = neutronLicense;
            _workstationView = workstationView;
            _logger = NeutronCore.Global.Logger.SetupLogger("LoaderManager");
            InitInterfaceFile();
            Mediator.GetInstance().StartStopLoader += (s, e) => StartStopLoaderAction(e.StartStop);
            Mediator.GetInstance().RunLoaderOnce += (s, e) => RunLoaderOnce();

        }

        private void InitInterfaceFile()
        {
            _logger.LogDetailAsync($"InitInterfaceFile Company Code: {_neutronLicense.CompanyCode}");
            switch (_neutronLicense.CompanyCode)
            {
                case "SFH":
                    {
                        _interfaceProcessor = new InterfaceProcessorSfh(_neutronVariables, _neutronLicense, _jsonData, _workstationView, _logger);
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
                        _logger.LogDetailAsync($"WAG - InterfaceProcessorPr1");
                        _interfaceProcessor = new InterfaceProcessorPr1(_neutronVariables, _neutronLicense, _jsonData, _workstationView);
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
            _logger.Log("Run Loader Once");
            _interfaceProcessor.RunLoaderOnce();
        }

        private void StartStopLoaderAction(string startStop)
        {
            _logger.LogDetailAsync($"StartStopLoaderAction: {startStop}");

            if (startStop == "Start")
            {
                _logger.LogDetailAsync("Start Processing Interface Files");
                StartProcessingInterfaceFiles();
            }
            else
            {
                _logger.LogDetailAsync("Stop Processing Interface Files");
                StopProcessingInterfaceFiles();
            }
        }

        private void StartProcessingInterfaceFiles()
        {
            _logger.LogDetailAsync("Start ProcessingInterfaceFiles");

            _interfaceProcessor.StartProcessingInterfaceFiles();
        }

        private void StopProcessingInterfaceFiles()
        {
            _logger.LogDetailAsync("Stop ProcessingInterfaceFiles");
            _interfaceProcessor?.StopProcessingInterfaceFiles();
        }
    }
}
