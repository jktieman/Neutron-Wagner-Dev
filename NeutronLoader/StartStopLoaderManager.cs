using System.Collections.Generic;
using AlliedLogger;
using AlliedPostOffice.Concrete;
using JsonManager;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronData.Models;
using NeutronEvents;

namespace NeutronLoader
{
    public class StartStopLoaderManager
    {
        private readonly IJsonData _jsonData;
        private readonly DynamicLogger _logger;
        private IInterfaceProcessor _interfaceProcessor;
        private readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;
        private readonly Station _rackStation;

        public StartStopLoaderManager(IJsonData jsonData, DynamicLogger logger, NeutronVariables neutronVariables,
            NeutronLicense neutronLicense, Station rackStation)
        {
            _jsonData = jsonData;
            _logger = logger;
            _neutronVariables = neutronVariables;
            _neutronLicense = neutronLicense;
            _rackStation = rackStation;
            InitInterfaceFile();
            Mediator.GetInstance().StartStopLoader += (s, e) => StartStopLoaderAction(e.StartStop);
            Mediator.GetInstance().RunLoaderOnce += (s, e) => RunLoaderOnce();

        }

        private void InitInterfaceFile()
        {
            switch (_neutronLicense.CompanyCode)
            {
                case "SFH":
                    {
                        _interfaceProcessor = new InterfaceProcessorSfh(_neutronVariables, _neutronLicense, _jsonData, _rackStation);
                        break;
                    }
                case "TOP":
                    {
                        _interfaceProcessor = new InterfaceProcessorTop(_neutronVariables, _neutronLicense, _jsonData, _rackStation);
                        break;
                    }
                case "TMG":
                    {
                        _interfaceProcessor = new InterfaceProcessorTmg(_neutronVariables, _neutronLicense, _jsonData, _rackStation);
                        break;
                    }
                case "PR1":
                    {
                        _interfaceProcessor = new InterfaceProcessorPr1(_neutronVariables, _neutronLicense, _jsonData, _rackStation);
                        break;
                    }
                case "MET":
                {
                   _interfaceProcessor = new InterfaceProcessorMET(_neutronVariables, _neutronLicense, _jsonData, _rackStation);
                    break;
                }
                default:
                    {
                        _interfaceProcessor = new InterfaceProcessorPr1(_neutronVariables, _neutronLicense, _jsonData, _rackStation);
                        break;
                    }
            }

        }

        private void RunLoaderOnce()
        {
            _interfaceProcessor.RunLoaderOnce();
        }

        private void StartStopLoaderAction(string startStop)
        {
            if (startStop == "Start")
            {
                _logger.Log("Start Processing Interface Files");
                StartProcessingInterfaceFiles();
            }
            else
            {
                _logger.Log("Stop Processing Interface Files");
                StopProcessingInterfaceFiles();
            }
        }

        private void StartProcessingInterfaceFiles()
        {
            _interfaceProcessor.StartProcessingInterfaceFiles();
        }

        private void StopProcessingInterfaceFiles()
        {
            _interfaceProcessor?.StopProcessingInterfaceFiles();
        }
    }
}
